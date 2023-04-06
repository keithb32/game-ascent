using Ascent.Sprites_and_Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Ascent.Player_and_Objects
{
    public class Pickup : Sprite
    {
        public Rectangle hitbox;
        public Pickup(Dictionary<string, Animation> animations, int Xpos = 0, int Ypos = 0, float Xscale = 1, float Yscale = 1) : base(animations, Xpos, Ypos, Xscale, Yscale)
        {
            hitbox = new Rectangle(Xpos, Ypos, (int)Xscale * _animations.First().Value.FrameWidth, (int)Yscale * _animations.First().Value.Texture.Height);
        }
    }
}
