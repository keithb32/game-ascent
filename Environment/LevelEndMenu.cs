using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Ascent.Environment
{
    public class LevelEndMenu
    {
        private Game1 _game;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        private Texture2D banner;
        private SpriteFont font;
        private List<MenuItem> menuItems;
        private int selectedMenuItemIndex;

        public float startTime;
        public float time;

        public LevelEndMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;
            banner = content.Load<Texture2D>("Menu/menuBanner");
            font = _content.Load<SpriteFont>("Fonts/MenuFont");

            menuItems = new List<MenuItem>
            {
                new MenuItem("TIME: " + time, new Vector2(1920/2-75, 400), () => NextLevel(game.currentLevel)),
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

        public void CallTime(float endTime)
        {
            time = endTime - startTime;
            menuItems[0].text = time.ToString();
        }

        private void NextLevel(int levelNumber)
        {
            _game.nextLevel = levelNumber + 1;
        }
    }
}
