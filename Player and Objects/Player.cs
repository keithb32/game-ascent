using Ascent.Sprites_and_Animation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascent.Environment;

namespace Ascent.Player_and_Objects
{
    internal class Player : Sprite
    {
        // collider
        public Rectangle Rect;

        // just the very bottom of the collider (used for colliding with semisolids)
        private Rectangle FeetRect;

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Rect.X = (int)_position.X;
                Rect.Y = (int)_position.Y;
                FeetRect.X = (int)_position.X;
                FeetRect.Y = (int)_position.Y + Rect.Height - 2;
                SpritePosition = new Vector2(_position.X, _position.Y);
            }
        }
        private Vector2 velocity1;


        private float chargeAmount = 10f;
        private float chargeMax = 90f;

        private float acceleration = 1.2f;
        private float MaxMoveSpeed = 15f;

        private float gravity = 1.0f;

        public string facingDirection = "Right";

        private enum playerState
        {
            Move,
            Charge,
            Launch,
            LaunchLag
        }

        private playerState state = playerState.Move;

        public Player(ContentManager Content) : base(
                new Dictionary<string, Animation>()
                {
                    {"IdleRight", new Animation(Content.Load<Texture2D>("Player/player-idle"), 4) },
                    {"IdleLeft", new Animation(Content.Load<Texture2D>("Player/player-idle"), 4, true) },
                    {"WalkLeft", new Animation(Content.Load<Texture2D>("Player/player-run"), 6, true) },
                    {"WalkRight", new Animation(Content.Load<Texture2D>("Player/player-run"), 6) },
                    {"JumpRight", new Animation(Content.Load<Texture2D>("Player/player-jump"), 1) },
                    {"JumpLeft", new Animation(Content.Load<Texture2D>("Player/player-jump"), 1, true) },
                    {"FallRight", new Animation(Content.Load<Texture2D>("Player/player-fall"), 1) },
                    {"FallLeft", new Animation(Content.Load<Texture2D>("Player/player-fall"), 1, true) },
                    {"CrouchRight", new Animation(Content.Load<Texture2D>("Player/player-crouch"), 2) },
                    {"CrouchLeft", new Animation(Content.Load<Texture2D>("Player/player-crouch"), 2, true) }
                },
                 -38, 
                 -50, 
                 3.45f, 
                 3.45f
            )
        {
            Rect = new Rectangle(10, 10, 40, 60);
            FeetRect = new Rectangle(Rect.X, Rect.Y + Rect.Height - 2, Rect.Width, 2);
            Position = new Vector2(20, 20);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, Point GameBounds, TileManager tiles, List<Box> boxes)
        {

            bool isGrounded = checkIfGrounded(GameBounds, tiles, boxes);
            string animationToPlay = "Idle";

            // check player's current state
            if (state == playerState.Move)
            {
                // default state; take input

                // move left/right
                if (keyboardState.IsKeyDown(Keys.A) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < -0.2f)
                {
                    facingDirection = "Left";
                    velocity1.X = MathHelper.Clamp(velocity1.X - acceleration, -MaxMoveSpeed, MaxMoveSpeed);
                    if (isGrounded)
                    {
                        animationToPlay = "Walk";
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0.2f)
                {
                    facingDirection = "Right";
                    velocity1.X = MathHelper.Clamp(velocity1.X + acceleration, -MaxMoveSpeed, MaxMoveSpeed);
                    if (isGrounded)
                    {
                        animationToPlay = "Walk";
                    }
                }

                // jumping
                if (keyboardState.IsKeyDown(Keys.W) || gamePadState.IsButtonDown(Buttons.X) || gamePadState.IsButtonDown(Buttons.A))
                {
                    if (isGrounded && velocity1.Y >= 0)
                    {
                        velocity1.Y = -20f;
                        animationToPlay = "Jump";
                    }
                }
                if (!isGrounded)
                {
                    if (velocity1.Y < 0f)
                    {
                        animationToPlay = "Jump";
                    }
                    else
                    {
                        animationToPlay = "Fall";
                    }
                }

                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    chargeAmount = 20f;
                    state = playerState.Charge;
                    animationToPlay = "Crouch";
                }
            }
            else if (state == playerState.Charge)
            {
                //player is charging the dash (holding down space)

                animationToPlay = "Crouch";


                if (keyboardState.IsKeyDown(Keys.A) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < -0.2f)
                {
                    facingDirection = "Left";
                }
                if (keyboardState.IsKeyDown(Keys.D) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0.2f)
                {
                    facingDirection = "Right";
                }


                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    if (chargeAmount < chargeMax)
                    {
                        chargeAmount += 1f;
                    }
                }
                else
                {
                    state = playerState.Launch;
                }
            }
            else if (state == playerState.Launch)
            {
                // player is about to launch (just let go of space)

                animationToPlay = "Crouch";
                if (facingDirection == "Left")
                {
                    velocity1 += new Vector2(-1f, -0.2f) * chargeAmount;
                }
                else
                {
                    velocity1 += new Vector2(1f, -0.2f) * chargeAmount;
                }
                state = playerState.LaunchLag;
            }
            else if (state == playerState.LaunchLag)
            {
                // player is launching during a dash (no input until they slow down a bit)

                animationToPlay = "Jump";
                if (Math.Abs(velocity1.X) <= 15f)
                {
                    state = playerState.Move;
                }
            }

            // try to move the player according to their velocity
            HandlePhysics(GameBounds, tiles, boxes);

            // interact with non-collidables in the tileset, including picking up pickups and reaching the goal flag
            tiles.DoObjectInteraction(Rect);

            _animationManager.Update(gameTime);
            _animationManager.Play(_animations[animationToPlay + facingDirection]);
        }

        // checks if the player is colliding with any solid ground tiles or with the borders of the game.
        private bool checkBounds(Rectangle CollisionRect, Point GameBounds, TileManager tiles)
        {
            if (CollisionRect.Bottom > GameBounds.Y || CollisionRect.Top < 0 || CollisionRect.Left < 0 || CollisionRect.Right > GameBounds.X)
            {
                return true;
            }
            if (tiles.Intersects(CollisionRect))
            {
                return true;
            }
            return false;
        }

        // checks if a collisions rectangle is colliding with any semisolids.
        private bool checkBoundsWithSemisolids(Rectangle CollisionRect, TileManager tiles)
        {
            if (tiles.IntersectsWithSemisolids(CollisionRect))
            {
                return true;
            }
            return false;
        }

        // check if a collision rectangle is colliding with any boxes. Returns a list of the boxes the player is colliding with.
        private List<Box> checkBoundsWithBox(Rectangle CollisionRect, List<Box> boxes)
        {
            List<Box> result = new List<Box>();
            foreach (Box box in boxes)
            {
                if (CollisionRect.Intersects(box.hitbox))
                {
                    result.Add(box);
                }
            }

            return result;
        }

        // check if the player is currently grounded.
        private bool checkIfGrounded(Point GameBounds, TileManager tiles, List<Box> boxes)
        {
            bool ret = false;
            Position += new Vector2(0, 1);
            ret = checkBounds(Rect, GameBounds, tiles) || checkBoundsWithSemisolids(FeetRect, tiles) || checkBoundsWithBox(Rect, boxes).Count > 0;
            Position -= new Vector2(0, 1);
            return ret;
        }

        // attempts to move the player according to their velocity for this update step.
        // if that movement would cause them to collide with something, don't move and reset velocity instead.
        private void HandlePhysics(Point GameBounds, TileManager tiles, List<Box> boxes)
        {
            // apply some forces (horizontal drag and gravity)
            velocity1.X *= 0.9f;

            velocity1.Y += gravity;

            // first try to move it in the x direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveXRect = new Rectangle(Math.Min(Rect.X, Rect.X + (int)velocity1.X), Rect.Y, Rect.Width + Math.Abs((int)velocity1.X), Rect.Height);

            List<Box> boxCollisions = checkBoundsWithBox(MoveXRect, boxes);


            // see if that collider intersects with anything; if it does, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveXRect, GameBounds, tiles))
            {
                if (state == playerState.LaunchLag)
                {
                    velocity1.X = -0.2f * velocity1.X;
                }
                else
                {
                    velocity1.X = 0;
                }
            }
            else if (boxCollisions.Count > 0)
            {
                foreach (Box box in boxCollisions)
                {
                    if (state == playerState.LaunchLag)
                    {
                        // if currently launching with space, transfer your force to the box and bounce back a bit
                        box.TransferForce(new Vector2(velocity1.X, 0));
                        velocity1.X = -0.1f * velocity1.X;
                    }
                    else
                    {
                        // if not currently launching, then just push it in front of you
                        if (box.PushX(velocity1.X * 0.9f, GameBounds, tiles, this))
                        {
                            velocity1.X *= 0.9f;
                            Position += new Vector2((int)velocity1.X, 0);
                        }
                        else
                        {
                            velocity1.X = 0;
                        }
                    }
                }
            }
            else
            {
                Position += new Vector2((int)velocity1.X, 0);
            }

            // now try to move in the y direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveYRect = new Rectangle(Rect.X, Math.Min(Rect.Y, Rect.Y + (int)velocity1.Y), Rect.Width, Rect.Height + (int)velocity1.Y);

            // do the same for the feet rectangle (for semisolids)
            Rectangle MoveFeetRect = new Rectangle(FeetRect.X, Math.Min(FeetRect.Y, FeetRect.Y + (int)velocity1.Y), FeetRect.Width, FeetRect.Height + (int)velocity1.Y);

            boxCollisions = checkBoundsWithBox(MoveYRect, boxes);

            // see if those colliders collide with anything; if they do, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveYRect, GameBounds, tiles) || (velocity1.Y > 0 && checkBoundsWithSemisolids(MoveFeetRect, tiles)))
            {
                velocity1.Y = 0;
            }
            else if (boxCollisions.Count > 0)
            {
                foreach (Box box in boxCollisions)
                {
                    if (state == playerState.LaunchLag)
                    {
                        // if currently launching with space, transfer your force to the box and bounce back a bit
                        box.TransferForce(new Vector2(0, velocity1.Y));
                        velocity1.Y = -0.1f * velocity1.Y;
                    }
                    else
                    {
                        // if not currently launching, then just push it a bit
                        if (box.PushY(velocity1.Y * 0.9f, GameBounds, tiles, this))
                        {
                            velocity1.Y *= 0.9f;
                            Position += new Vector2(0, (int)velocity1.Y);
                        }
                        else
                        {
                            velocity1.Y = 0;
                        }
                        velocity1.Y = 0;
                    }
                }
            }
            else
            {
                Position += new Vector2(0, (int)velocity1.Y);
            }

        }

        // helper method for debugging
        private void DrawRectangle(SpriteBatch sb, Rectangle Rec, Color color)
        {
            Vector2 pos = new Vector2(Rec.X, Rec.Y);
            sb.Draw(Texture, pos, Rec, color * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);
        }


        // Note: you can uncomment the following function to debug the player's hitbox (shows both player hitbox and feet hitbox over the sprite)

        //public void Draw(SpriteBatch _spriteBatch)
        //{
        //    _animationManager.Draw(_spriteBatch);
        //    _spriteBatch.Draw(Texture, new Vector2(Rect.X, Rect.Y), Rect, Microsoft.Xna.Framework.Color.White * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);  // uncomment to see player collision box
        //    _spriteBatch.Draw(Texture, new Vector2(FeetRect.X, FeetRect.Y), FeetRect, Microsoft.Xna.Framework.Color.Green * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f); // uncomment to see feet rectangle
        //}
    }
}
