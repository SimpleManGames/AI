using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI
{
    public class AgentAI
    {
        Game1 game;
        Random RNG = new Random();

        public enum AIState
        {
            Chasing,
            Caught,
            Wander,
            Evade,
            Attack
        }
        public AIState state = AIState.Wander;
        const float ChaseSpeed = 3.0f;
        const float TurnSpeed = 0.15f;
        const float ChaseDistance = 250.0f;
        const float CaughtDistance = 80f;
        const float ChaseHysteresis = 15.0f;

        float orientation;
        Vector2 targetPos, position, wanderDir;

        Texture2D texture;

        public AgentAI(Game1 game, Vector2 target, Texture2D texture) {
            this.game = game;
            this.targetPos = target;
            this.texture = texture;
            this.position = Vector2.Zero;
        }

        public void Update(GameTime gameTime) {
            targetPos = game.target.position;
            UpdateEnemy();
        }

        private void UpdateEnemy() {
            float chaseThreshold = ChaseDistance;
            float caughtThreshold = CaughtDistance;

            if (state == AIState.Wander)
                chaseThreshold -= ChaseHysteresis / 2;
            else if (state == AIState.Chasing) {
                chaseThreshold += ChaseHysteresis / 2;
                caughtThreshold -= ChaseHysteresis / 2;
            } else if (state == AIState.Caught)
                caughtThreshold += ChaseHysteresis / 2;

            float disFromTarget = Vector2.Distance(position, targetPos);
            if (disFromTarget > chaseThreshold)
                state = AIState.Wander;
            else if (disFromTarget > caughtThreshold)
                state = AIState.Chasing;
            else
                state = AIState.Caught;

            float currentSpeed;
            if (state == AIState.Chasing) {
                orientation = TurnToFace(position, targetPos, orientation, TurnSpeed);
                currentSpeed = ChaseSpeed;
            } else if (state == AIState.Wander) {
                Wander(position, ref wanderDir, ref orientation, TurnSpeed);
                currentSpeed = .25f * ChaseSpeed;
            } else
                currentSpeed = 0.0f;

            Vector2 heading = new Vector2((float)Math.Cos(orientation), (float)Math.Sin(orientation));
            position += heading * currentSpeed;
        }
        private void Wander(Vector2 position, ref Vector2 wanderDir, ref float orientation, float turnspeed) {
            wanderDir.X += MathHelper.Lerp(-.25f, .25f, (float)RNG.NextDouble());
            wanderDir.Y += MathHelper.Lerp(-.25f, .25f, (float)RNG.NextDouble());

            if (wanderDir != Vector2.Zero)
                wanderDir.Normalize();

            orientation = TurnToFace(position, position + wanderDir, orientation, .15f * turnspeed);
            Vector2 screenCenter = Vector2.Zero;
            screenCenter.X = game.graphics.PreferredBackBufferWidth / 2;
            screenCenter.Y = game.graphics.PreferredBackBufferHeight / 2;
            float disFromScreenCenter = Vector2.Distance(screenCenter, position);
            float maxDisFromCenter = Math.Min(screenCenter.Y, screenCenter.X);
            float normalizedDist = disFromScreenCenter / maxDisFromCenter;
            float turnToCenterSpeed = .3f * normalizedDist * normalizedDist * turnspeed;
            orientation = TurnToFace(position, screenCenter, orientation, turnToCenterSpeed);
        }
        private static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed) {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            float desiredAngle = (float)Math.Atan2(y, x);
            float diffence = WrapAngle(desiredAngle - currentAngle);
            diffence = MathHelper.Clamp(diffence, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + diffence);
        }
        private static float WrapAngle(float radians) {
            while (radians < -MathHelper.Pi)
                radians += MathHelper.TwoPi;
            while (radians > MathHelper.Pi)
                radians -= MathHelper.TwoPi;
            return radians;
        }
        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture: texture, 
                position: position, 
                sourceRectangle: null, 
                color: Color.White, 
                rotation: orientation, 
                origin: new Vector2(texture.Width / 2, texture.Height / 2),
                scale: 0.25f,
                effects: SpriteEffects.None, 
                layerDepth: 0.0f);

            spriteBatch.DrawString(game.Content.Load<SpriteFont>(@"DefaultFont"), position.ToString(), Vector2.One, Color.Red);
        }
    }
}
