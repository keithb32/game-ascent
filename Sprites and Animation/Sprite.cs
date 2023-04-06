using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascent.Sprites_and_Animation
{
    // The base class for anything that needs to render a sprite on the screen. Can be inhereted from to easily produce sprites with various properties
    // (see Player.cs, Box.cs, and Pickup.cs for some examples)
    // Derived from this tutorial: https://www.youtube.com/watch?v=OLsiWxgONeM
    public class Sprite
    {
        private Vector2 _spritePosition;

        // note: if you want the position of the sprite to change, you'll have to update the SpritePosition field here. Make sure to change the entire Vector2 at once, not just the X or Y.
        // This is important for the change to propogate to the animation manager.
        // for an example on how to get it to happen automatically, check out the position field of the player or the boxes.
        public Vector2 SpritePosition
        {
            get { return _spritePosition; }
            set
            {
                _spritePosition = value;
                if (_animationManager != null)
                {
                    _animationManager.Position = _spritePosition;
                }
            }
        }

        protected AnimationManager _animationManager;
        protected Dictionary<string, Animation> _animations;


        public Texture2D Texture;

        public Sprite(Dictionary<string, Animation> animations, int Xpos = 0, int Ypos = 0, float Xscale = 1, float Yscale = 1)
        {
            _animations = animations;
            _animationManager = new AnimationManager(_animations.First().Value, Xpos, Ypos, Xscale, Yscale);
            _animationManager.Play(_animations.First().Value);
        }

        public void LoadContent(GraphicsDeviceManager _graphics)
        {
            Texture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            Texture.SetData<Color>(new Color[] { Color.White });
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _animationManager.Draw(_spriteBatch);

        }

        public void Update(GameTime gameTime)
        {
            _animationManager.Update(gameTime);
        }

    }
}
