using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascent.Sprites_and_Animation
{
    public class Sprite
    {
        private Vector2 _spritePosition;
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
