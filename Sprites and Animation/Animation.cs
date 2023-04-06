using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascent.Sprites_and_Animation
{
    // helper class for animations/sprites.
    // Derived from this tutorial: https://www.youtube.com/watch?v=OLsiWxgONeM
    public class Animation
    {
        public int CurrentFrame { get; set; }
        public int FrameCount { get; private set; }
        public float FrameSpeed { get; set; }
        public int FrameHeight { get { return Texture.Height; } }
        public int FrameWidth { get { return Texture.Width / FrameCount; } }

        public bool isLooping { get; set; }
        public bool FlipHorizonallty { get; set; }
        public Texture2D Texture { get; private set; }
        public Animation(Texture2D texture, int frameCount, bool flipHorizontally = false)
        {
            Texture = texture;
            FrameCount = frameCount;
            isLooping = true;
            FrameSpeed = 0.1f;
            FlipHorizonallty = flipHorizontally;
        }
    }
}
