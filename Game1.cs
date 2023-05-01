using Ascent.Environment;
using Ascent.Player_and_Objects;
using Ascent.Scores;
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

        // Background
        private Point GameBounds = new Point(1920, 1080); // window resolution
        List<Sprite> backgroundSprites = new List<Sprite>();
        private Color color = new Color(46, 90, 137);
        private float scale = 2.0f;

        // Content
        private TileManager tiles;
        private SoundManager sounds;
        private SpriteFont font;

        // Menu
        private MainMenu mainMenu;
        private LevelEndMenu endMenu;
        private PauseMenu pauseMenu;
        public int currentLevel = 0;
        public bool isPaused { get; set; } = false;
        public bool isTransitioning { get; set; } = false;
        public int nextLevel { get; set; } = 0;

        // Game state
        private Player player1;
        private float[,] medalSplits; // in ms

        // Inputs
        private KeyboardState keyboardState;
        private GamePadState gamePadState;
        private MouseState mouseState;

        public Game1()
        {
            // Default monogame variables
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Member variables
            sounds = SoundManager.CreateInstance(Content);
            tiles = new TileManager(Content, this, scale);
            font = Content.Load<SpriteFont>("Fonts\\File");
            mainMenu = new MainMenu(this, GraphicsDevice, Content);
            endMenu = new LevelEndMenu(this, GraphicsDevice, Content);
            pauseMenu = new PauseMenu(this, GraphicsDevice, Content);
            
            sounds.PlayMusic("Title");
        }

        protected override void Initialize()
        {
            medalSplits = new float[,] {
                { 6000, 15000, 30000 }, // Level 1
                { 6000, 12000, 20000 }, // Level 2
                { 3000, 9000, 25000 }, // Level 3
                { 12000, 35000, 45000 }, // Level 4
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Player sprite
            player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
            player1.LoadContent(_graphics);

            // Level backgrounds
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

            // Respawn player
            if (player1.isDead)
            {
                player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
            }

            // Pause game if player presses escape while not on main menu
            if (currentLevel > 0 && Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                isPaused = true;
            }

            // Switch to main menu
            if (nextLevel != currentLevel && nextLevel == 0)
            {
                currentLevel = nextLevel;
                // update music
                sounds.PlayMusic("Title");
            }
            else if (nextLevel != currentLevel && nextLevel != 0) // Draw new tile layout if level has changed and we're not going to the main menu
            {
                currentLevel = nextLevel;
                // update music
                sounds.PlayMusic("Level" + currentLevel);
                // Reset level timer
                endMenu.elapsedTime = -1.0f;
                endMenu.startTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

                // Load tiles + medal splits for next level
                endMenu.SetSplits(medalSplits, currentLevel - 1);
                tiles.LoadLevel(currentLevel);

                // Respawn player for next level
                player1 = new Player(Content, (int)tiles.playerSpawn.X, (int)tiles.playerSpawn.Y, scale);
            }

            HandleInput(gameTime);

            if (isPaused)
            {
                pauseMenu.Update(gameTime, mouseState);
            }
            else if (currentLevel == 0)
            {
                mainMenu.Update(gameTime, mouseState);
            }
            else if (tiles.goalReached)
            {
                if (endMenu.elapsedTime < 0.0f)
                {
                    endMenu.RecordElapsedTime((float)gameTime.TotalGameTime.TotalMilliseconds, currentLevel);
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

            else if (currentLevel == 0 || nextLevel == 0) // need to check both current and next level to prevent tileset from displaying during menu transition
            {
                mainMenu.Draw(_spriteBatch);
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

                float levelTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                
                tiles.Draw(_spriteBatch);
                player1.Draw(_spriteBatch);
                _spriteBatch.DrawString(font, endMenu.GetElapsedTime(levelTime), new Vector2(1750, 25), Color.White);
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