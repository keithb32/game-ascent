using Ascent.Environment;
using Ascent.Player_and_Objects;
using Ascent.Sprites_and_Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Ascent
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Point GameBounds = new Point(960, 1280);    // window resolution

        private Player player1;
        private TileManager tiles;

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
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            tiles = new TileManager(Content, this);

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

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            
            HandleInput(gameTime);

            player1.Update(gameTime, keyboardState, mouseState, gamePadState, GameBounds, tiles);
            tiles.Update(gameTime, GameBounds, player1);


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

            // TODO: Add your drawing code here
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            background0.Draw(_spriteBatch);
            background1.Draw(_spriteBatch);
            background2.Draw(_spriteBatch);

            tiles.Draw(_spriteBatch);
            player1.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Quit()
        {
            this.Exit();
        }
    }
}