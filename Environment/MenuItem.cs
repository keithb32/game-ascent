using Microsoft.Xna.Framework;
using System;

namespace Ascent.Environment
{
    public class MenuItem
    {
        public string text;
        public Vector2 position;
        public Rectangle bounds;
        public Action action;

        public MenuItem(string text, Vector2 position, Action action)
        {
            this.text = text;
            this.position = position;
            bounds = new Rectangle((int)position.X, (int)position.Y, 200, 40);
            this.action = action;
        }
    }
}