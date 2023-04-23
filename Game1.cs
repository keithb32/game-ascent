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

        private LevelMenu menu;
        private LevelEndMenu endMenu;
        public int currentLevel = 0;
        private float[,] medalSplits; // in ms
        public int nextLevel { get; set; } = 0;

        private Sprite background0;
        private Sprite background1;
        private Sprite background2;

        private Color color = new Color(46, 90, 137);


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
            tiles = new TileManager(Content, this);
            menu = new LevelMenu(this, GraphicsDevice, Content);
            endMenu = new LevelEndMenu(this, GraphicsDevice, Content);
        }

        protected override void Initialize()
        {
            base.Initialize();

            medalSplits = new float[,] { 
                { 3000, 5000, 10000 }, // Level 1
                { 3111, 5111, 11111 }, // Level 2
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            player1 = new Player(Content);
            player1.LoadContent(_graphics);

            var background0Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_0"), 1) }
            };
            background0 = new Sprite(background0Animation, 0, 450, 3.45f, 3.45f);
            var background1Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_1"), 1) }
            };
            background1 = new Sprite(background1Animation,0, 550, 3.45f, 3.45f);
            var background2Animation = new Dictionary<string, Animation>()
            {
                {"Idle", new Animation(Content.Load<Texture2D>("Backgrounds/background_2"), 1) }
            };
            background2 = new Sprite(background2Animation, 0, 610, 3.45f, 3.45f);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (endMenu.startTime < 0.0f)
            {
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (player1.isDead)
            {
                player1 = new Player(Content);
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            // if level number was changed, update it
            // ideally would not write like this but /shrug
            if (nextLevel != currentLevel)
            {
                currentLevel = nextLevel;
                endMenu.time = -1.0f;
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                endMenu.setSplits(medalSplits, currentLevel - 1);
                tiles.LoadLevel(currentLevel);
            }

            HandleInput(gameTime);

            if (currentLevel == 0)
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

            if (currentLevel == 0)
            {
                menu.Draw(_spriteBatch);
            }
            else if (tiles.goalReached)
            {
                endMenu.Draw(_spriteBatch);
            }
            else
            {
                background0.Draw(_spriteBatch);
                background1.Draw(_spriteBatch);
                background2.Draw(_spriteBatch);

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