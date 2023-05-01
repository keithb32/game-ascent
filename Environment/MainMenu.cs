using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Ascent.Environment
{
    internal class MainMenu
    {
        // MonoGame variables
        private Game1 _game;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        // Assets
        private Texture2D banner;
        private SpriteFont font;

        // Menu state
        private List<MenuItem> menuItems;
        private int selectedMenuItemIndex;

        public MainMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            // MonoGame variables
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;

            // Assets
            banner = content.Load<Texture2D>("Menu/menuBanner");
            font = _content.Load<SpriteFont>("Fonts/MenuFont");

            // Menu state
            menuItems = new List<MenuItem>
            {
            new MenuItem("Level 1", new Vector2(1920/2-75, 400), () => StartLevel(1)),
            new MenuItem("Level 2", new Vector2(1920/2-75, 450), () => StartLevel(2)),
            new MenuItem("Level 3", new Vector2(1920/2-75, 500), () => StartLevel(3)),
            new MenuItem("Level 4", new Vector2(1920/2-75, 550), () => StartLevel(4)),
            new MenuItem("Exit", new Vector2(1920/2-75, 600), () => _game.Exit())
            };
            selectedMenuItemIndex = 0;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            // Clear the screen
            _graphicsDevice.Clear(Color.Black);

            // TODO: Position this according to final window resolution
            spriteBatch.Draw(banner, new Rectangle(1920/2 - 250, 100, 500, 150), Color.White);

            for (int i = 0; i < menuItems.Count; i++)
            {
                // Determine the color of the current menu item based on whether it is selected or not
                Color color = (i == selectedMenuItemIndex) ? Color.White : Color.Gray;

                // Draw the text of the current menu item using the appropriate color
                spriteBatch.DrawString(font, menuItems[i].text, menuItems[i].position, color);
            }
        }


        public void Update(GameTime gameTime, MouseState mouseState){
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


        private void StartLevel(int levelNumber)
        {
            _game.nextLevel = levelNumber;
        }

    }
}
