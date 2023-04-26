using Ascent.Environment;
using Ascent.Player_and_Objects;
using Ascent.Sprites_and_Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ascent
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Point GameBounds = new Point(1920, 1080);    // window resolution

        private Player player1;
        private TileManager tiles;
        private SoundManager sounds;

        private LevelMenu menu;
        private LevelEndMenu endMenu;
        public int currentLevel = 0;
        private float[,] medalSplits; // in ms
        public int nextLevel { get; set; } = 0;

        private PauseMenu pauseMenu;
        public bool isPaused { get; set; } = false;
        public bool isTransitioning { get; set; } = false;

        List<Sprite> backgroundSprites = new List<Sprite>();

        private Color color = new Color(46, 90, 137);

        private float scale = 2.0f;


        // inputs
        private KeyboardState keyboardState;
        private GamePadState gamePadState;
        private MouseState mouseState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            sounds = SoundManager.CreateInstance(Content);
            tiles = new TileManager(Content, this);
            
            menu = new LevelMenu(this, GraphicsDevice, Content);
            endMenu = new LevelEndMenu(this, GraphicsDevice, Content);
            pauseMenu = new PauseMenu(this, GraphicsDevice, Content);
            
            sounds.PlaySound("gameStart");
        }

        protected override void Initialize()
        {
            base.Initialize();

            medalSplits = new float[,] { 
                { 3000, 5000, 10000 }, // Level 1
                { 3111, 5111, 11111 }, // Level 2
                { 3222, 5222, 12222 }, // Level 3
                { 3111, 5111, 11111 }, // Level 4
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            tiles = new TileManager(Content, this, scale);

            player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
            player1.LoadContent(_graphics);

            var background0Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_0"), 1) }
            };
            backgroundSprites.Add(new Sprite(background0Animation, 0, 300, 5f, 5f));
            backgroundSprites.Add(new Sprite(background0Animation, 750, 300, 5f, 5f));
            var background1Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_1"), 1) }
            };
            backgroundSprites.Add(new Sprite(background1Animation,0, 350, 5f, 5f));
            backgroundSprites.Add(new Sprite(background1Animation, 780, 350, 5f, 5f));
            var background2Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_2"), 1) }
            };
            backgroundSprites.Add(new Sprite(background2Animation, 0, 430, 5f, 5f));
            backgroundSprites.Add(new Sprite(background2Animation, 800, 430, 5f, 5f));

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            if (endMenu.startTime < 0.0f)
            {
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (player1.isDead)
            {
                player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }
            // Pause game if player presses escape while not on main menu
            if (currentLevel > 0 && Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                isPaused = true;
            }

            // Draw new tile layout if level has changed and we're not going to the main menu
            if (nextLevel != currentLevel)
            {
                currentLevel = nextLevel;
                endMenu.time = -1.0f;
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                if(currentLevel != 0)
                {
                    endMenu.setSplits(medalSplits, currentLevel - 1);
                }
                tiles.LoadLevel(currentLevel);
                //jimmy addon
                player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
            }

            HandleInput(gameTime);

            if (isPaused)
            {
                pauseMenu.Update(gameTime, mouseState);
            }
            else if (currentLevel == 0)
            {
                menu.Update(gameTime, mouseState);
            }
            else if (tiles.goalReached)
            {
                if (endMenu.time < 0.0f)
                {
                    endMenu.CallTime((float)gameTime.TotalGameTime.TotalMilliseconds);
                }
                endMenu.Update(mouseState);
            }
            else
            {
                player1.Update(gameTime, keyboardState, mouseState, gamePadState, GameBounds, tiles);
                tiles.Update(gameTime, GameBounds, player1);
            }

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            if (capabilities.IsConnected)
            {
                gamePadState = GamePad.GetState(PlayerIndex.One);
            }
            mouseState = Mouse.GetState();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(color);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (isPaused)
            {
                pauseMenu.Draw(_spriteBatch);
            }
            else if (currentLevel == 0)
            {
                menu.Draw(_spriteBatch);
            }
            else if (tiles.goalReached)
            {
                endMenu.Draw(_spriteBatch);
            }
            else
            {
                foreach (Sprite backgroundSprite in backgroundSprites)
                {
                    backgroundSprite.Draw(_spriteBatch);
                }

                tiles.Draw(_spriteBatch);
                player1.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Quit()
        {
            this.Exit();
        }
    }
}