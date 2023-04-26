using Ascent.Player_and_Objects;
using Ascent.Sprites_and_Animation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;
using System.Diagnostics;

namespace Ascent.Environment
{
    // This class handles parsing, loading, and rendering tiled files, as well as tile collisions and interactions.
    // A couple notes on constructing tiled files:
    // This class expects a tile layer titled "Ground" that contains all of the tiles that the player should collide with.

    // It can also handle a second tile layer called "Semisolids" that will contain all of the tiles that the player should treat as a semisolid (meaning, they can land on it but can also jump up thorugh it)

    // It can also handle an object layer titled "Goal", that contains one object. The tile manager will place a goal object (right now, a gem that moves the player to the next level) at the position of that object.
    // It can also handle an object layer titled "Boxes", that contains any number of objects. It will spawn a box at the position of every object in that object layer.
    // It can also handle an object layer titled "Pickups" that contains any number of objects. It will spawn a cherry pickup at the position of every object in that object layer.

    // Finally, it also works fine with any Tile Layers named anything else- for these layers, it'll render/draw the layer, but not give that layer any collision (useful for background objects, grass, etc.)

    // only the "Ground" tile layer is required for it to run- all the other layers are optional as you need them.
    internal class TileManager
    {
        private TiledMap map;
        private Dictionary<int, TiledTileset> tilesets;
        private Texture2D tilesetTexture;
        private Texture2D obstacleTexture;

        private List<Pickup> cherries;
        public List<Box> boxes;
        private Pickup goal;
        public bool goalReached;

        public Vector2 playerSpawn = new Vector2(20, 20);

        public int level = 1;
        private int maxLevel = 3;

        public int numCherries = 0;

        private ContentManager con;
        private SoundManager soundManager;
        private Game1 game;

        public float scale;

        private SpriteFont font;

        public bool newLevel = false;

        [Flags]
        enum Trans
        {
            None = 0,
            Flip_H = 1 << 0,
            Flip_V = 1 << 1,
            Flip_D = 1 << 2,
            Rotate_90 = Flip_D | Flip_H,
            Rotate_180 = Flip_H | Flip_V,
            Rotate_270 = Flip_V | Flip_D,

            Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
        }

        public TileManager(ContentManager Content, Game1 theGame, float scale=2.0f)
        {
            game = theGame;
            con = Content;
            soundManager = SoundManager.GetInstance(Content);
            this.scale = scale;
            LoadLevel(1);
            tilesets = map.GetTiledTilesets(Content.RootDirectory + "\\Environment\\");
            tilesetTexture = Content.Load<Texture2D>("Environment\\tileset");
            obstacleTexture = Content.Load<Texture2D>("Environment\\obstacle_tiles");
            font = Content.Load<SpriteFont>("Fonts\\File");
        }

        // Load a level of a given number (assumes level tiled files will be in the Environment folder, and named LevelX , where X is the level number)
        public void LoadLevel(int level)
        {
            if(level == 0)
            {
                return;
            }
            if (level > maxLevel)
            {
                game.Quit();
                return;
            }

            if(this.level != level)
            {
                newLevel = true;
            }
            

            Debug.WriteLine("Level loading: " + level);

            //set up the tiles
            map = new TiledMap(con.RootDirectory + "\\Environment\\Level" + level + ".tmx");
            map.TileHeight = (int)(scale * map.TileHeight);
            map.TileWidth = (int)(scale * map.TileWidth);

            goalReached = false;


            // set up the goal (gem)
            var goalLayers = map.Layers.FirstOrDefault(x => x.name == "Goal");
            if (goalLayers != null)
            {
                var goalData = map.Layers.First(x => x.name == "Goal").objects.First();
                var goalAnimation = new Dictionary<string, Animation>()
                {
                    {"Idle", new Animation(con.Load<Texture2D>("Pickups/gem"), 5) }
                };
                goal = new Pickup(goalAnimation, (int) (scale * goalData.x), (int)(scale * goalData.y), 3.45f, 3.45f);
            }
            else
            {
                goal = null;
            }

            // set up player spawn
            var playerSpawnLayer = map.Layers.FirstOrDefault(x => x.name == "PlayerSpawn");
            if (playerSpawnLayer != null)
            {
                var playerSpawnData = map.Layers.First(x => x.name == "PlayerSpawn").objects.First();
                playerSpawn = new Vector2((int) (scale* playerSpawnData.x), (int)(scale * playerSpawnData.y));
            }
            else
            {
                playerSpawn = new Vector2(20,20);
            }

            // set up the pickups (cherries)
            cherries = new List<Pickup>();

            var cherryAnimation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(con.Load<Texture2D>("Pickups/cherry"), 5) }
            };
            cherryAnimation["Idle"].FrameSpeed = 0.2f;

            var pickupLayer = map.Layers.FirstOrDefault(x => x.name == "Pickups");
            if(pickupLayer != null)
            {
                foreach (var obj in pickupLayer.objects)
                {
                    cherries.Add(new Pickup(cherryAnimation, (int)(scale * obj.x), (int)(scale * obj.y), 3.45f, 3.45f));
                }
            }

            // set up the boxes
            boxes = new List<Box>();

            var boxAnimation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(con.Load<Texture2D>("Objects/crate"), 1) }
            };
            var boxLayer = map.Layers.FirstOrDefault(x => x.name == "Boxes");
            if(boxLayer!= null){
                foreach (var obj in boxLayer.objects)
                {
                    boxes.Add(new Box(boxAnimation, (int)(scale * obj.x), (int)(scale * obj.y), 3.45f, 3.45f));
                }
            }
            this.level = level;
        }

        public void Update(GameTime gameTime, Point GameBounds, Player player)
        {
            if(goal != null){
                goal.Update(gameTime);
            }
            
            foreach (Box b in boxes)
            {
                b.Update(GameBounds, this, player);
            }
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            // draw tiles (will likely be moved to its own object later)
            var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);
            foreach (var layer in tileLayers)
            {
                for (var y = 0; y < layer.height; y++)
                {
                    for (var x = 0; x < layer.width; x++)
                    {
                        // Assuming the default render order is used which is from right to bottom
                        var index = (y * layer.width) + x;
                        var gid = layer.data[index]; // The tileset tile index
                        var tileX = x * map.TileWidth;
                        var tileY = y * map.TileHeight;

                        // Gid 0 is used to tell there is no tile set
                        if (gid == 0)
                        {
                            continue;
                        }

                        // Helper method to fetch the right TiledMapTileset instance
                        // This is a connection object Tiled uses for linking the correct tileset to the 
                        // gid value using the firstgid property
                        var mapTileset = map.GetTiledMapTileset(gid);

                        // Retrieve the actual tileset based on the firstgid property of the connection object 
                        // we retrieved just now
                        var tileset = tilesets[mapTileset.firstgid];

                        // Use the connection object as well as the tileset to figure out the source rectangle
                        var rect = map.GetSourceRect(mapTileset, tileset, gid);

                        // Create destination and source rectangles
                        var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                        var destination = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);

                        // You can use the helper methods to get information to handle flips and rotations
                        Trans tileTrans = Trans.None;
                        if (map.IsTileFlippedHorizontal(layer, x, y)) tileTrans |= Trans.Flip_H;
                        if (map.IsTileFlippedVertical(layer, x, y)) tileTrans |= Trans.Flip_V;
                        if (map.IsTileFlippedDiagonal(layer, x, y)) tileTrans |= Trans.Flip_D;

                        SpriteEffects effects = SpriteEffects.None;
                        double rotation = 0f;
                        switch (tileTrans)
                        {
                            case Trans.Flip_H: effects = SpriteEffects.FlipHorizontally; break;
                            case Trans.Flip_V: effects = SpriteEffects.FlipVertically; break;

                            case Trans.Rotate_90:
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            case Trans.Rotate_180:
                                rotation = Math.PI;
                                destination.X += map.TileWidth;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_270:
                                rotation = Math.PI * 3 / 2;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_90AndFlip_H:
                                effects = SpriteEffects.FlipHorizontally;
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            default:
                                break;
                        }

                        // Render sprite at position tileX, tileY using the rect
                        if (layer.name == "Spikes")
                        {
                            _spriteBatch.Draw(obstacleTexture, destination, source, Color.White,
                                (float)rotation, Vector2.Zero, effects, 0);
                        }
                        else
                        {
                            _spriteBatch.Draw(tilesetTexture, destination, source, Color.White,
                                (float)rotation, Vector2.Zero, effects, 0);
                        }
                    }
                }
            }

            // done drawing the tiles
            // now draw the pickups/boxes/goal (if any)
            if(goal != null)
            {
                goal.Draw(_spriteBatch);
            }
            

            foreach (Pickup p in cherries)
            {
                p.Draw(_spriteBatch);
            }

            foreach (Box b in boxes)
            {
                b.Draw(_spriteBatch);
            }

            // draw text
            var textLayer = map.Layers.FirstOrDefault(x => x.name == "Text");
            if (textLayer != null)
            {
                foreach (var obj in textLayer.objects)
                {
                    _spriteBatch.DrawString(font, obj.name, new Vector2(scale*obj.x, scale*obj.y), Color.White);
                }
            }
            }
            

        // checks if the ground tile layer intersects with a rectangle; used for player collisions
        // Note: assumes that the tiles the player needs to collide with as solid walls are all in a tiled layer called "Ground"
        public bool Intersects(Rectangle rect)
        {
            var groundLayer = map.Layers.First(x => x.name == "Ground");
            for (int y = rect.Top / map.TileHeight; y < groundLayer.height && y < rect.Bottom / map.TileHeight + 1; y++)
            {
                for (int x = rect.Left / map.TileWidth; x < groundLayer.width && x < rect.Right / map.TileWidth + 1; x++)
                {
                    var index = (y * groundLayer.width) + x;
                    if(index < 0 || index > groundLayer.data.Length)
                    {
                        return true;
                    }
                    if (groundLayer.data[index] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // checks if the top of any tile in the semisolid tile layer interects with a rectangle; used for player collisions.
        // Note: assumes that the semisolid platforms that the player needs to interact with are all in a tiled layer called "Semisolids" 
        public bool IntersectsWithSemisolids(Rectangle rect)
        {
            var semisolidLayer = map.Layers.FirstOrDefault(x => x.name == "Semisolids");
            if(semisolidLayer == null || semisolidLayer.data.Length==0)
            {
                return false;
            }
            for (int y = rect.Top / map.TileHeight; y < semisolidLayer.height && y < rect.Bottom / map.TileHeight + 1; y++)
            {
                for (int x = rect.Left / map.TileWidth; x < semisolidLayer.width && x < rect.Right / map.TileWidth + 1; x++)
                {
                    var index = (y * semisolidLayer.width) + x;
                    if (semisolidLayer.data[index] != 0)
                    {
                        if (rect.Intersects(new Rectangle(x * map.TileWidth, y * map.TileHeight - 1, map.TileWidth, 2)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IntersectsWithSpikes(Rectangle rect)
        {
            int spikeHitboxSize = map.TileHeight /2;
            var spikeLayer = map.Layers.FirstOrDefault(x => x.name == "Spikes");
            if (spikeLayer == null)
                return false;
            for (int y = rect.Top / map.TileHeight; y < spikeLayer.height && y < rect.Bottom / map.TileHeight + 1; y++)
            {
                for (int x = rect.Left / map.TileWidth; x < spikeLayer.width && x < rect.Right / map.TileWidth + 1; x++)
                {
                    var index = (y * spikeLayer.width) + x;
                    if (spikeLayer.data[index] != 0)
                    {
                        // if spike collision is wonky, adj here probably
                        // we got a spike, we need to see which hitbox to apply

                        //upward facing spike
                        if (spikeLayer.data[index] == 199)
                        {
                            if (rect.Intersects(new Rectangle(x * map.TileWidth, (y+1) * map.TileHeight - spikeHitboxSize, map.TileWidth, spikeHitboxSize)))
                            {
                                return true;
                            }
                        }
                        // downward facing spike
                        else if (spikeLayer.data[index] == 200)
                        {
                            if (rect.Intersects(new Rectangle(x * map.TileWidth, y * map.TileHeight, map.TileWidth, spikeHitboxSize)))
                            {
                                return true;
                            }
                        }
                        //left facing spike
                        else if (spikeLayer.data[index] == 202)
                        {
                            if (rect.Intersects(new Rectangle((x + 1) * map.TileWidth - spikeHitboxSize, y * map.TileHeight, spikeHitboxSize, map.TileHeight)))
                            {
                                return true;
                            }
                        }
                        //right facing spike
                        else if (spikeLayer.data[index] == 201)
                        {
                            if (rect.Intersects(new Rectangle(x * map.TileWidth, y * map.TileHeight, spikeHitboxSize, map.TileHeight)))
                            {
                                return true;
                            }
                        }


                    }
                }
            }
            return false;

        }
        
        // checks if a rectangle intersects with a pickup or the goal; if it intersects with a pickup, collect it; if it intersects with the goal, go to the next level.
        public void DoObjectInteraction(Rectangle rect)
        {
            for (int i = 0; i < cherries.Count; i++)
            {
                if (cherries[i].hitbox.Intersects(rect))
                {
                    cherries.RemoveAt(i);
                    i--;
                    numCherries++;

                    // Play pickup sound
                }
            }

            if (goal != null && goal.hitbox.Intersects(rect))
            {
                //LoadLevel(level + 1);
                goalReached = true;
                //level++;
                //LoadLevel(level);

                // Play sound
                soundManager.PlaySound("endLevel");
            }
        }
    }
}
