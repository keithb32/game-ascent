using Microsoft.Xna.Framework.Input;using Ascent.Sprites_and_Animation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascent.Environment;
using System.Reflection.Metadata;

namespace Ascent.Player_and_Objects
{
    // The class for the player!
    internal class Player : Sprite
    {
        private const bool DebugMode = false;
        // collider
        public Rectangle Rect;

        // just the very bottom of the collider (used for colliding with semisolids)
        private Rectangle FeetRect;

        private Vector2 _position;
        // Note: whenever updating the player's position, make sure to change the entire Position Vector2 and not just the X or Y of the Vector2!
        // This is important to cause the below code to run, which propogates the change to the player's hitboxes and sprite.
        // For example, don't use "Position.X = 5"; , use "Position = new Vector2( 5, Position.Y)";
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
        private Vector2 velocity;

        // values used to control swinging
        private Vector2 _grapplePoint;
        public Vector2 GrapplePoint
        {
            get { return _grapplePoint; }
            set
            {
                _grapplePoint = value;
            }
        }
        private float grappleHookLength = 0f;

        // values used to control charging the dash
        private float chargeAmount = 10f;
        private float chargeMax = 90f;

        // movement parameters
        private float acceleration = 1.2f;
        private float MaxMoveSpeed = 15f;

        // gravity strength
        private float gravity = 1.0f;

        // current facing direction
        public string facingDirection = "Right";

        // current state of the player
        private enum playerState
        {
            Move,
            Charge,
            Launch,
            LaunchLag,
            Grapple
        }

        private playerState state = playerState.Move;

        private Texture2D tether;

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
            GrapplePoint = new Vector2(-1, -1);
            tether = Content.Load<Texture2D>("Player/tether");
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState, GamePadState gamePadState, Point GameBounds, TileManager tiles)
        {
            bool isGrounded = checkIfGrounded(GameBounds, tiles);

            // Check if the player is grappling
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                state = playerState.Grapple;
            }
            else
            {
                // player must hold down mouse to swing, and if they let go, reset the grapple point
                GrapplePoint = new Vector2(-1, -1);
                if (state == playerState.Grapple)
                {
                    state = playerState.Move;
                }
            }

            string animationToPlay = "Idle";

            // check player's current state
            if (state == playerState.Move)
            {
                // default state; take input

                // move left/right
                if (keyboardState.IsKeyDown(Keys.A) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < -0.2f)
                {
                    facingDirection = "Left";
                    velocity.X = MathHelper.Clamp(velocity.X - acceleration, -MaxMoveSpeed, MaxMoveSpeed);
                    if (isGrounded)
                    {
                        animationToPlay = "Walk";
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0.2f)
                {
                    facingDirection = "Right";
                    velocity.X = MathHelper.Clamp(velocity.X + acceleration, -MaxMoveSpeed, MaxMoveSpeed);
                    if (isGrounded)
                    {
                        animationToPlay = "Walk";
                    }
                }

                // jumping
                if (keyboardState.IsKeyDown(Keys.W) || gamePadState.IsButtonDown(Buttons.X) || gamePadState.IsButtonDown(Buttons.A))
                {
                    if (isGrounded && velocity.Y >= 0)
                    {
                        velocity.Y = -20f;
                        animationToPlay = "Jump";
                    }
                }
                if (!isGrounded)
                {
                    if (velocity.Y < 0f)
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
            else if (state == playerState.Grapple)
            {
                // Check if GrapplePoint is set already, and if it isn't set, update it to the mouse position
                if (GrapplePoint.X == -1 && GrapplePoint.Y == -1)
                {
                    GrapplePoint = new Vector2(mouseState.X, mouseState.Y);
                }

                // add gravity to the player
                velocity.Y += gravity;

                // Calculate the line between the player and the grapple point
                Vector2 grappleLine = GrapplePoint - Position;

                // Calculate the distance between the player and the grapple point
                float grappleDistance = grappleLine.Length();

                // Calculate the tangential vector of the grapple line
                Vector2 grappleTangent = new Vector2(-grappleLine.Y, grappleLine.X);
                grappleTangent.Normalize();
                // project current velocity onto grappleTangent
                Vector2 projectedVelocity = Vector2.Dot(velocity, grappleTangent) * grappleTangent;


                velocity = projectedVelocity;
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
                    velocity += new Vector2(-1f, -0.2f) * chargeAmount;
                }
                else
                {
                    velocity += new Vector2(1f, -0.2f) * chargeAmount;
                }
                state = playerState.LaunchLag;
            }
            else if (state == playerState.LaunchLag)
            {
                // player is launching during a dash (no input until they slow down a bit)

                animationToPlay = "Jump";
                if (Math.Abs(velocity.X) <= 15f)
                {
                    state = playerState.Move;
                }
            }

            // try to move the player according to their velocity
            HandlePhysics(GameBounds, tiles);

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
        private bool checkIfGrounded(Point GameBounds, TileManager tiles)
        {
            bool ret = false;
            Position += new Vector2(0, 1);
            ret = checkBounds(Rect, GameBounds, tiles) || checkBoundsWithSemisolids(FeetRect, tiles) || checkBoundsWithBox(Rect, tiles.boxes).Count > 0;
            Position -= new Vector2(0, 1);
            return ret;
        }

        // attempts to move the player according to their velocity for this update step.
        // if that movement would cause them to collide with something, don't move and reset velocity instead.
        private void HandlePhysics(Point GameBounds, TileManager tiles)
        {
            // apply some damping forces (horizontal drag and gravity)

            if (GrapplePoint.X == -1 && GrapplePoint.Y == -1)
            {
                velocity.Y += gravity;
                // if grounded, apply x damping factor of 0.9. otherwise, apply x damping factor of 0.95.
                if (checkIfGrounded(GameBounds, tiles))
                {
                    velocity.X *= 0.9f;
                }
                else
                {
                    velocity.X *= 0.99f;
                }
            }

            // first try to move it in the x direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveXRect = new Rectangle(Math.Min(Rect.X, Rect.X + (int)velocity.X), Rect.Y, Rect.Width + Math.Abs((int)velocity.X), Rect.Height);

            List<Box> boxCollisions = checkBoundsWithBox(MoveXRect, tiles.boxes);


            // see if that collider intersects with anything; if it does, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveXRect, GameBounds, tiles))
            {
                if (state == playerState.LaunchLag)
                {
                    velocity.X = -0.2f * velocity.X;
                }
                else
                {
                    velocity.X = 0;
                }
            }
            else if (boxCollisions.Count > 0)
            {
                foreach (Box box in boxCollisions)
                {
                    if (state == playerState.LaunchLag)
                    {
                        // if currently launching with space, transfer your force to the box and bounce back a bit
                        box.TransferForce(new Vector2(velocity.X, 0));
                        velocity.X = -0.1f * velocity.X;
                    }
                    else
                    {
                        // if not currently launching, then just push it in front of you
                        if (box.PushX(velocity.X * 0.9f, GameBounds, tiles, this))
                        {
                            velocity.X *= 0.9f;
                            Position += new Vector2((int)velocity.X, 0);
                        }
                        else
                        {
                            velocity.X = 0;
                        }
                    }
                }
            }
            else
            {
                Position += new Vector2((int)velocity.X, 0);
            }

            // now try to move in the y direction

            // define a rectangle that covers the new x position for collider and the old x position for the collider, plus everything that's in between them
            Rectangle MoveYRect = new Rectangle(Rect.X, Math.Min(Rect.Y, Rect.Y + (int)velocity.Y), Rect.Width, Rect.Height + (int)velocity.Y);

            // do the same for the feet rectangle (for semisolids)
            Rectangle MoveFeetRect = new Rectangle(FeetRect.X, Math.Min(FeetRect.Y, FeetRect.Y + (int)velocity.Y), FeetRect.Width, FeetRect.Height + (int)velocity.Y);

            boxCollisions = checkBoundsWithBox(MoveYRect, tiles.boxes);

            // see if those colliders collide with anything; if they do, stop velocity in that direction. Otherwise, move it there.
            if (checkBounds(MoveYRect, GameBounds, tiles) || (velocity.Y > 0 && checkBoundsWithSemisolids(MoveFeetRect, tiles)))
            {
                velocity.Y = 0;
            }
            else if (boxCollisions.Count > 0)
            {
                foreach (Box box in boxCollisions)
                {
                    if (state == playerState.LaunchLag)
                    {
                        // if currently launching with space, transfer your force to the box and bounce back a bit
                        box.TransferForce(new Vector2(0, velocity.Y));
                        velocity.Y = -0.1f * velocity.Y;
                    }
                    else
                    {
                        // if not currently launching, then just push it a bit
                        if (box.PushY(velocity.Y * 0.9f, GameBounds, tiles, this))
                        {
                            velocity.Y *= 0.9f;
                            Position += new Vector2(0, (int)velocity.Y);
                        }
                        else
                        {
                            velocity.Y = 0;
                        }
                        velocity.Y = 0;
                    }
                }
            }
            else
            {
                Position += new Vector2(0, (int)velocity.Y);
            }

        }

        // helper method for debugging
        private void DrawRectangle(SpriteBatch sb, Rectangle Rec, Color color)
        {
            Vector2 pos = new Vector2(Rec.X, Rec.Y);
            sb.Draw(Texture, pos, Rec, color * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);
        }

        // https://stackoverflow.com/questions/16403809/drawing-lines-in-c-sharp-with-xna
        public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(tether, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }


        // Note: you can uncomment the following function to debug the player's hitbox (shows both player hitbox and feet hitbox over the sprite)

        public void Draw(SpriteBatch _spriteBatch)
        {
            _animationManager.Draw(_spriteBatch);
            if (state == playerState.Grapple)
            {
                DrawLine(_spriteBatch, Position, GrapplePoint, Color.White, 3);
            }

            if (DebugMode)
            {
                // Draw line between player and grapple point
                DrawLine(_spriteBatch, Position, GrapplePoint, Color.Red, 1);

                // Draw grapple point
                _spriteBatch.Draw(Texture, GrapplePoint, new Rectangle(0, 0, 5, 5), Color.Red * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);

                // Draw tangent line
                _spriteBatch.Draw(Texture, new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2), new Rectangle(0, 0, 1, 1), Color.Blue * 1.0f, (float)Math.Atan2(GrapplePoint.Y - (Rect.Y + Rect.Height / 2), GrapplePoint.X - (Rect.X + Rect.Width / 2)) + (float)Math.PI / 2, Vector2.Zero, new Vector2(Vector2.Distance(GrapplePoint, new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2)), 1), SpriteEffects.None, 0.000001f);

                // main collision box
                _spriteBatch.Draw(Texture, new Vector2(Rect.X, Rect.Y), Rect, Color.White * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);

                // feet rectangle
                _spriteBatch.Draw(Texture, new Vector2(FeetRect.X, FeetRect.Y), FeetRect, Color.Green * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.000001f);
            }
        }
    }
}
