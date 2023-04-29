using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Ascent.Environment
{
    internal class PauseMenu
    {
        // MonoGame variables
        private Game1 _game;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        // Assets
        private SpriteFont font;
        private Texture2D blackTexture;

        // Menu state
        private List<MenuItem> menuItems;
        private int selectedMenuItemIndex;
        private float transitionAlpha;

        public PauseMenu(Game1 game, GraphicsDevice graphicsDevice, ContentManager content) {

            // MonoGame variables
            _game = game;
            _graphicsDevice = graphicsDevice;
            _content = content;

            // Assets
            font = _content.Load<SpriteFont>("Fonts/MenuFont");
            blackTexture = _content.Load<Texture2D>("Backgrounds/blackBackground");

            // Menu state
            menuItems = new List<MenuItem>
            {
            new MenuItem("Resume", new Vector2(1920/2-100, 450), () => {_game.isPaused = false;}),
            new MenuItem("Main Menu", new Vector2(1920/2-100, 500), () => {_game.isTransitioning = true;})
            };
            selectedMenuItemIndex = 0;
            transitionAlpha = 0;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            // Clear the screen
            _graphicsDevice.Clear(Color.Gray * 0.50f);

            for (int i = 0; i < menuItems.Count; i++)
            {
                // Determine the color of the current menu item based on whether it is selected or not
                Color color = (i == selectedMenuItemIndex) ? Color.White : Color.Gray;

                // Draw the text of the current menu item using the appropriate color
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


        public void Update(GameTime gameTime, MouseState mouseState)
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
    }
}
