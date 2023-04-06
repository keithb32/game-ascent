using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascent.Sprites_and_Animation
{
    // Manages animations for Sprites.
    // Derived from this tutorial: https://www.youtube.com/watch?v=OLsiWxgONeM
    public class AnimationManager
    {
        private Animation _animation;
        private float _timer;
        public Vector2 Position { get; set; }
        Point positionOffset;
        Vector2 scaleOffset;
        public AnimationManager(Animation animation, int shiftX = 0, int shiftY = 0, float scaleX = 1, float scaleY = 1)
        {
            _animation = animation;
            positionOffset = new Point(shiftX, shiftY);
            scaleOffset = new Vector2(scaleX, scaleY);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _animation.Texture,
                new Rectangle((int)Position.X + positionOffset.X, (int)Position.Y + positionOffset.Y, (int)(scaleOffset.X * (float)_animation.FrameWidth), (int)(scaleOffset.Y * (float)_animation.FrameHeight)),
                new Rectangle(_animation.CurrentFrame * _animation.FrameWidth, 0, _animation.FrameWidth, _animation.FrameHeight),
                Color.White,
                0f,
                new Vector2(0, 0),
                _animation.FlipHorizonallty ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
               );
        }
        public void Play(Animation animation)
        {
            if (_animation == animation) return;
            _animation = animation;
            _animation.CurrentFrame = 0;
            _timer = 0;
        }

        public void Stop()
        {
            _timer = 0f;
            _animation.CurrentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer > _animation.FrameSpeed)
            {
                _timer = 0f;
                _animation.CurrentFrame++;
                if (_animation.CurrentFrame >= _animation.FrameCount)
                {
                    _animation.CurrentFrame = 0;
                }
            }
        }
    }
}
