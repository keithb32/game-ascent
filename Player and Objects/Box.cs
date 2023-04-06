using Ascent.Environment;
using Ascent.Sprites_and_Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Ascent.Player_and_Objects
{
    internal class Box : Sprite
    {
        public Rectangle hitbox;
        public Rectangle footHitbox;

        Queue<Vector2> ForcesToAdd = new Queue<Vector2>();

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                hitbox.X = (int)_position.X;
                hitbox.Y = (int)_position.Y;
                footHitbox.X = (int)_position.X;
                footHitbox.Y = (int)_position.Y + hitbox.Height - 2;
                SpritePosition = new Vector2(_position.X, _position.Y);
            }
        }

        private Vector2 velocity;

        private float gravity = 1.0f;

        public Box(Dictionary<string, Animation> animations, int Xpos = 0, int Ypos = 0, float Xscale = 1, float Yscale = 1) : base(animations, 0, 0, Xscale, Yscale)
        {
            hitbox = new Rectangle(Xpos, Ypos, (int)(Xscale * _animations.First().Value.Texture.Width), (int)(Yscale * _animations.First().Value.Texture.Height));
            footHitbox = new Rectangle(Xpos, Ypos + hitbox.Height - 2, hitbox.Width, 2);
            Position = new Vector2(Xpos, Ypos);
        }

        public void Update(Point GameBounds, TileManager tiles, Player player)
        {
            //foreach(Vector2 force in ForcesToAdd)
            //{
            //    Debug.Log(force)
            //    velocity += force;
            //}
            //ForcesToAdd = new Queue<Vector2>();
            HandlePhysics(GameBounds, tiles, player);
            //Debug.WriteLine(velocity);
        }

        private bool checkBounds(Rectangle CollisionRect, Point GameBounds, TileManager tiles, Player player)
        {
            if (CollisionRect.Bottom > GameBounds.Y || CollisionRect.Top < 0 || CollisionRect.Left < 0 || CollisionRect.Right > GameBounds.X)
            {
                return true;
            }
            if (tiles.Intersects(CollisionRect))
            {
                return true;
            }
            if (CollisionRect.Intersects(player.Rect))
            {
                return true;
            }
            return false;
        }

        private bool checkBoundsWithSemisolids(Rectangle CollisionRect, TileManager tiles)
        {
            if (tiles.IntersectsWithSemisolids(CollisionRect))
            {
                return true;
            }
            return false;
        }

        private bool TryToMoveX(float xChange, Point GameBounds, TileManager tiles, Player player)
        {
            // first try to move it in the x direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveXRect = new Rectangle(Math.Min(hitbox.X, hitbox.X + (int)xChange), hitbox.Y, hitbox.Width + (int)xChange, hitbox.Height);


            // see if that collider intersects with anything; if it does, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveXRect, GameBounds, tiles, player))
            {
                return false;
            }
            else
            {
                Position += new Vector2((int)xChange, 0);
                return true;
            }
        }

        // attempts to move the player according to their velocity for this update step.
        // if that movement would cause them to collide with something, don't move and reset velocity instead.
        private void HandlePhysics(Point GameBounds, TileManager tiles, Player player)
        {
            // apply some forces (horizontal drag and gravity)
            velocity.X *= 0.9f;

            velocity.Y += gravity;

            if (!TryToMoveX(velocity.X, GameBounds, tiles, player))
            {
                if (!TryToMoveX(velocity.X * 3f / 4, GameBounds, tiles, player))
                {
                    if (!TryToMoveX(velocity.X / 2, GameBounds, tiles, player))
                    {
                        TryToMoveX(velocity.X / 4, GameBounds, tiles, player);
                    }

                }
                velocity.X = 0;
            }

            // now try to move in the y direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveYRect = new Rectangle(hitbox.X, Math.Min(hitbox.Y, hitbox.Y + (int)velocity.Y), hitbox.Width, hitbox.Height + (int)velocity.Y);

            // do the same for the feet rectangle (for semisolids)
            Rectangle MoveFeetRect = new Rectangle(footHitbox.X, Math.Min(footHitbox.Y, footHitbox.Y + (int)velocity.Y), footHitbox.Width, footHitbox.Height + (int)velocity.Y);

            // see if those colliders collide with anything; if they do, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveYRect, GameBounds, tiles, player) || (velocity.Y > 0 && checkBoundsWithSemisolids(MoveFeetRect, tiles)))
            {
                velocity.Y = 0;
            }
            else
            {
                Position += new Vector2(0, (int)velocity.Y);
            }

        }

        public bool PushX(float xPush, Point GameBounds, TileManager tiles, Player player)
        {
            // try to move it in the x direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveXRect = new Rectangle(Math.Min(hitbox.X, hitbox.X + (int)xPush), hitbox.Y, hitbox.Width + (int)xPush, hitbox.Height);


            // see if that collider intersects with anything; if it does, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveXRect, GameBounds, tiles, player))
            {
                return false;
            }
            else
            {
                Position += new Vector2((int)xPush, 0);
                return true;
            }
        }

        public bool PushY(float yPush, Point GameBounds, TileManager tiles, Player player)
        {
            // try to move in the y direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveYRect = new Rectangle(hitbox.X, Math.Min(hitbox.Y, hitbox.Y + (int)yPush), hitbox.Width, hitbox.Height + (int)yPush);

            // do the same for the feet rectangle (for semisolids)
            Rectangle MoveFeetRect = new Rectangle(footHitbox.X, Math.Min(footHitbox.Y, footHitbox.Y + (int)yPush), footHitbox.Width, footHitbox.Height + (int)yPush);

            // see if those colliders collide with anything; if they do, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveYRect, GameBounds, tiles, player) || (yPush > 0 && checkBoundsWithSemisolids(MoveFeetRect, tiles)))
            {
                return false;
            }
            else
            {
                Position += new Vector2(0, (int)yPush);
                return true;
            }
        }

        public void TransferForce(Vector2 Force)
        {
            velocity += Force;
        }
    }
}
