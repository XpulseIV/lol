using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class Ball : Entity
    {
        public Ball(
            GameplayState gameState,
            Vector2 position,
            Single direction)
            : base(gameState, position) {
            Random rnd = new();
            Int32 speed = rnd.Next(30, 100);

            this.Velocity = new Vector2((Single)Math.Cos(direction), (Single)Math.Sin(direction)) * speed;

            Texture2D spriteSheet;
            Int32 colliderSize;
            Int32 spriteSize;
            Single mass;

            spriteSheet = AssetManager.Load<Texture2D>("Asteroid3");
            spriteSize = 32;
            colliderSize = 24;
            this.MaxHP = 36;
            this.HP = this.MaxHP;
            mass = 18;

            Frame frame = new(new Rectangle(spriteSize * 3, 0, spriteSize, spriteSize));

            this.SpriteRenderer = new SpriteRenderer(spriteSheet, frame, LayerDepth.Foreground);

            this.Collider = new Collider(
                this, this.Position,
                colliderSize,
                true,
                mass);
            this.GameState.CollisionSystem.AddCollider(this.Collider);

            this.OutOfBoundsBehavior = OutOfBounds.Bounce;

            this.IsActor = true;
            this.IsFriendly = true;
        }

        protected override void OnDeath() { }

        public override void OnUpdate(Object sender, UpdateEventArgs e) {
            base.OnUpdate(sender, e);

            if (this.Velocity.Length() > 200) this.Velocity = Vector2.Normalize(this.Velocity) * 200;
        }

        public override void OnCollision(Collider other) {
            if (other.Parent is Bullet) return;
            base.OnCollision(other);
        }
    }
}