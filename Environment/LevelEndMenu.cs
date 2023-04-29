using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using Ascent.Scores;

namespace Ascent.Environment
{
    public class LevelEndMenu
    {
        // MonoGame variables
        private Game1 _game;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        private ScoreManager _scores;

        
        // Assets 
        private Texture2D banner;
        private Texture2D gold, silver, bronze;
        private SpriteFont font;
        private Texture2D blackTexture;

        // Menu state
        private List<MenuItem> menuItems;
        private int selectedMenuItemIndex;
        private float goldTime, silverTime, bronzeTime;
        public float startTime, elapsedTime, bestTime;
        private float transitionAlpha;

        public LevelEndMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            // MonoGame variables
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;

            // Assets
            _scores = ScoreManager.Load();
            banner = content.Load<Texture2D>("Menu/menuBanner");
            font = _content.Load<SpriteFont>("Fonts/MenuFont");
            gold = content.Load<Texture2D>("Menu/medal_gold");
            silver = content.Load<Texture2D>("Menu/medal_silver");
            bronze = content.Load<Texture2D>("Menu/medal_bronze");
            blackTexture = _content.Load<Texture2D>("Backgrounds/blackBackground");

            // Menu state     
            menuItems = new List<MenuItem>
            {
                new MenuItem("SUMMARY ", new Vector2(1920/2-83, 325), () => { }),
                new MenuItem("TIME: ", new Vector2(1920/2-200, 500), () => { }),
                new MenuItem("BEST TIME: ", new Vector2(1920/2 + 50, 500), () => { }),
                new MenuItem("Replay Level", new Vector2(1920/2-95, 625), () => { game.currentLevel -= 1; NextLevel(game.currentLevel); }),
                new MenuItem("Next Level", new Vector2(1920/2-80, 675), () => NextLevel(game.currentLevel)),
                new MenuItem("Back to Main Menu", new Vector2(1920/2-138, 725), () => {game.isTransitioning = true; })
            };
            selectedMenuItemIndex = 0;
            startTime = elapsedTime = -1.0f;
            transitionAlpha = 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Clear the screen
            _graphicsDevice.Clear(Color.Black);

            // TODO: Position this according to final window resolution
            spriteBatch.Draw(banner, new Rectangle(1920 / 2 - 245, 100, 500, 150), Color.White);

            if (elapsedTime < goldTime)
            {
                spriteBatch.Draw(gold, new Rectangle(1920 / 2 - 45, 400, 80, 80), Color.White);
            }
            else if (elapsedTime < silverTime)
            {
                spriteBatch.Draw(silver, new Rectangle(1920 / 2 - 45, 400, 80, 80), Color.White);
            }
            else if (elapsedTime < bronzeTime)
            {
                spriteBatch.Draw(bronze, new Rectangle(1920 / 2 - 45, 400, 80, 80), Color.White);
            }

            for (int i = 0; i < menuItems.Count; i++)
            {
                Color color = (i == selectedMenuItemIndex || i <= 2) ? Color.White : Color.Gray;
                spriteBatch.DrawString(font, menuItems[i].text, menuItems[i].position, color);
            }

            // If the game is transitioning, draw a black rectangle over the entire screen
            if (_game.isTransitioning)
            {
                // Calculate size of screen and color of the black rectangle based on the current transition alpha
                Rectangle screenRectangle = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
                Color color = Color.Black * transitionAlpha;

                // Draw the black rectangle using the calculated size and color
                spriteBatch.Draw(blackTexture, screenRectangle, color);
            }

        }

        public void Update(MouseState mouseState)
        {
            Point mousePosition = mouseState.Position;

            // Loop over menu items to see which item is selected
            // and invoke the menu item if the user clicks it
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].bounds.Contains(mousePosition))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        menuItems[selectedMenuItemIndex].action.Invoke();
                    }
                    else
                    {
                        selectedMenuItemIndex = i;
                    }
                }
            }

            if (_game.isTransitioning)
            {
                // Gradually increase the transition alpha
                transitionAlpha += 0.02f;

                // If the transition is complete, switch to the main menu
                if (transitionAlpha >= 1f)
                {
                    // Switch to the main menu
                    _game.isTransitioning = false;
                    _game.nextLevel = 0;
                    _game.isPaused = false;
                    transitionAlpha = 0f;
                }
            }

        }

        public void SetSplits(float[,] splits, int currentLevel)
        {
            goldTime = splits[currentLevel, 0];
            silverTime = splits[currentLevel, 1];
            bronzeTime = splits[currentLevel, 2];
        }

        // Records player's final time after completing a level
        public void RecordElapsedTime(float endTime, int currentLevel)
        {
            elapsedTime = endTime - startTime;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(elapsedTime);

            // Update score manager
            _scores.Add(currentLevel, elapsedTime);
            menuItems[1].text = "Total Time\n" + TimeSpan.FromMilliseconds(elapsedTime).ToString(@"mm\:ss\:fff");
            menuItems[2].text = "Best Time\n" + TimeSpan.FromMilliseconds(_scores.bestTimes[currentLevel]).ToString(@"mm\:ss\:fff");            
        }

        // Computes the time the player has spent on the level so far, and returns it as a string
        public String GetElapsedTime(float endTime)
        {
            float t = endTime - startTime;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(t);
            return timeSpan.ToString(@"mm\:ss\:fff");
        }

        private void NextLevel(int levelNumber)
        {
            _game.nextLevel = levelNumber + 1;
        }
    }
}
