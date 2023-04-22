using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

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

        private float time;

        public LevelEndMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;
            banner = content.Load<Texture2D>("Menu/menuBanner");
            font = _content.Load<SpriteFont>("Fonts/MenuFont");

            menuItems = new List<MenuItem>
            {
                new MenuItem("Next Level", new Vector2(1920/2-75, 400), () => NextLevel(game.currentLevel))
            };
            selectedMenuItemIndex = 0;
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

        private void NextLevel(int levelNumber)
        {
            _game.nextLevel = levelNumber;
        }
    }
}
