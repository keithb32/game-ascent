using Ascent.Player_and_Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ascent.Sprites_and_Animation
{
    // Note: this code is largely inspired/taken from this tutorial:https://www.youtube.com/watch?v=FcnvwtyxLds
    // The tutorial is for Unity but it translates pretty easily into Monogame (yay C#)
    // Anyways what this class does is it simulates/draws rope physics, which is used to animate the tether for the grapple
    // the advantage of this class over a simple line is that this lets the rope have a bit of slack when the player isn't at the max distance
    // If you're wondering what a RopeSegment is, it's a helper struct at the bottom
    internal class Rope
    {
        List<RopeSegment> ropeSegments;
        private float ropeSegLen;

        private Player player;
        private Vector2 endPoint;
        private float length;
        private int numSegments;
        private Texture2D tether;

        public Rope(Player playerRef, Vector2 endRopePosition, float lengthOfRope, int numSegmentsInRope, Texture2D tether)
        {
            player = playerRef;
            endPoint= endRopePosition;
            length = lengthOfRope;
            numSegments = numSegmentsInRope;
            this.tether = tether;
            ropeSegments = new List<RopeSegment>();
            ropeSegLen = lengthOfRope / numSegmentsInRope - 1.0f ;

            Vector2 temp = player.GetCenter();
            Vector2 directionOfRope = endRopePosition - player.GetCenter();
            directionOfRope.Normalize();
            directionOfRope *= ropeSegLen;
            for(int i=0; i<numSegments; i++)
            {
                ropeSegments.Add(new RopeSegment(temp));
                temp += directionOfRope;
            }
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

        public void Draw(SpriteBatch _spriteBatch)
        {
            Simulate();
            for(int i=0;i<numSegments-1;i++)
            {
                DrawLine(_spriteBatch, ropeSegments[i].posNow, ropeSegments[i + 1].posNow, Color.White, 3);
            }
        }

        public void Simulate()
        {
            // SIMULATION
            Vector2 forceGravity = new Vector2(0f, 0.2f); //changed from -1.5

            for (int i = 1; i < this.numSegments; i++)
            {
                RopeSegment firstSegment = this.ropeSegments[i];
                Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += velocity;
                firstSegment.posNow += forceGravity;
                this.ropeSegments[i] = firstSegment;
            }

            //CONSTRAINTS
            for (int i = 0; i < 50; i++)
            {
                this.ApplyConstraint();
            }
        }

        private void ApplyConstraint()
        {
            //Constrant to Player
            RopeSegment firstSegment = this.ropeSegments[0];
            firstSegment.posNow = player.GetCenter();
            this.ropeSegments[0] = firstSegment;

            //Constraint to other point
            RopeSegment endSegment = this.ropeSegments[(int)this.numSegments - 1];
            endSegment.posNow = this.endPoint;
            this.ropeSegments[this.numSegments - 1] = endSegment;

            for (int i = 0; i < this.numSegments - 1; i++)
            {
                RopeSegment firstSeg = this.ropeSegments[i];
                RopeSegment secondSeg = this.ropeSegments[i + 1];

                float dist = Vector2.Distance(firstSeg.posNow,secondSeg.posNow);
                float error = Math.Abs(dist - this.ropeSegLen);
                Vector2 changeDir = new Vector2(0,0);

                if (dist > ropeSegLen)
                {
                    changeDir = (firstSeg.posNow - secondSeg.posNow);
                    changeDir.Normalize();
                }
                else if (dist < ropeSegLen)
                {
                    changeDir = (secondSeg.posNow - firstSeg.posNow);
                    changeDir.Normalize();
                }

                Vector2 changeAmount = changeDir * error;
                if (i != 0)
                {
                    firstSeg.posNow -= changeAmount * 0.5f;
                    this.ropeSegments[i] = firstSeg;
                    secondSeg.posNow += changeAmount * 0.5f;
                    this.ropeSegments[i + 1] = secondSeg;
                }
                else
                {
                    secondSeg.posNow += changeAmount;
                    this.ropeSegments[i + 1] = secondSeg;
                }
            }
        }
    }
    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }

}
