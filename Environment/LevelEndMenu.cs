using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System;

namespace Ascent.Environment
{
    public class LevelEndMenu
    {
        private Game1 _game;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        private Texture2D banner;
        private Texture2D gold, silver, bronze;
        private SpriteFont font;
        private List<MenuItem> menuItems;
        private int selectedMenuItemIndex;
        private float goldTime, silverTime, bronzeTime;

        public float startTime;
        public float time;

        public LevelEndMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;
            banner = content.Load<Texture2D>("Menu/menuBanner");
            font = _content.Load<SpriteFont>("Fonts/MenuFont");
            gold = content.Load<Texture2D>("Menu/medal_gold");
            silver = content.Load<Texture2D>("Menu/medal_silver");
            bronze = content.Load<Texture2D>("Menu/medal_bronze");

            menuItems = new List<MenuItem>
            {
                new MenuItem("TIME: " + time, new Vector2(1920/2-75, 500), () => { }),
                new MenuItem("Next Level", new Vector2(1920/2-75, 600), () => NextLevel(game.currentLevel))
            };
            selectedMenuItemIndex = 0;
            startTime = -1.0f;
            time = -1.0f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Clear the screen
            _graphicsDevice.Clear(Color.Black);

            // TODO: Position this according to final window resolution
            spriteBatch.Draw(banner, new Rectangle(1920 / 2 - 250, 100, 500, 150), Color.White);
            if (time < goldTime)
            {
                spriteBatch.Draw(gold, new Rectangle(1920 / 2 - 50, 400, 80, 80), Color.White);
            }
            else if (time < silverTime)
            {
                spriteBatch.Draw(silver, new Rectangle(1920 / 2 - 50, 400, 80, 80), Color.White);
            }
            else if (time < bronzeTime)
            {
                spriteBatch.Draw(bronze, new Rectangle(1920 / 2 - 50, 400, 80, 80), Color.White);
            }

            for (int i = 0; i < menuItems.Count; i++)
            {
                Color color = (i == selectedMenuItemIndex) ? Color.White : Color.Gray;
                spriteBatch.DrawString(font, menuItems[i].text, menuItems[i].position, color);
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
        }

        public void setSplits(float[,] splits, int currentLevel)
        {
            goldTime = splits[currentLevel, 0];
            silverTime = splits[currentLevel, 1];
            bronzeTime = splits[currentLevel, 2];
        }

        public void CallTime(float endTime)
        {
            time = endTime - startTime;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(time);
            menuItems[0].text = timeSpan.ToString(@"mm\:ss\:fff");
        }

        private void NextLevel(int levelNumber)
        {
            _game.nextLevel = levelNumber + 1;
        }
    }
}
