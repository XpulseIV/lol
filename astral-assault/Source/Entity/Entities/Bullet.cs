using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class Bullet : Entity
    {
        public Bullet(GameplayState gameState, Vector2 position, Single rotation, Single speed, BulletType bulletType)
            : base(gameState, position) {
            this.Velocity = new Vector2(
                (Single)Math.Cos(rotation),
                (Single)Math.Sin(rotation)
            ) * speed;

            Texture2D spriteSheet = AssetManager.Load<Texture2D>("Bullet");

            Int32 spriteIndex = bulletType == BulletType.Heavy ? 1 : 0;
            Frame frame = new(new Rectangle(4 * spriteIndex, 0, 4, 4));

            this.SpriteRenderer = new SpriteRenderer(spriteSheet, frame, LayerDepth.Foreground);

            this.Collider = new Collider(
                this, this.Position, 1);
            this.GameState.CollisionSystem.AddCollider(this.Collider);

            this.OutOfBoundsBehavior = OutOfBounds.Destroy;

            this.ContactDamage = bulletType == BulletType.Light ? 4 : 8;

            this.IsFriendly = true;
        }

        public override void OnCollision(Collider other) {
            if (this.IsFriendly == other.Parent.IsFriendly) return;

            this.Destroy();
        }

        public override void OnUpdate(Object sender, UpdateEventArgs e) {
            base.OnUpdate(sender, e);

            if (this.Position.X is > Game1.TargetWidth or < 0 || this.Position.Y is > Game1.TargetHeight or < 0)
                this.Destroy();
        }
    }
}