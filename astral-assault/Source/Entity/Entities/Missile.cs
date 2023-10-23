using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class Missile : Entity
    {
        private const Single RotationSpeed = 1F;
        private const Single Speed = 110F;

        public Missile(GameplayState gameState, Vector2 position) : base(gameState, position) {
            this.MaxHP = 30;
            this.HP = this.MaxHP;

            this.Rotation = this.GetRotationToPlayer();
            this.Velocity = new Vector2(MathF.Cos(this.Rotation), MathF.Sin(this.Rotation)) * Speed;

            Texture2D spriteSheet = AssetManager.Load<Texture2D>("Missile");

            Frame frame = new(
                new Rectangle(0, 0, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                new Rectangle(48, 0, 16, 16));

            this.SpriteRenderer = new SpriteRenderer(spriteSheet, frame, LayerDepth.Foreground);

            this.Collider = new Collider(
                this, this.Position,
                6,
                true,
                6);
            this.GameState.CollisionSystem.AddCollider(this.Collider);

            this.OutOfBoundsBehavior = OutOfBounds.Wrap;

            this.IsActor = true;

            this.HealthBarYOffset = 16;

            this.ContactDamage = 40;
        }

        public override void OnUpdate(Object sender, UpdateEventArgs e) {
            base.OnUpdate(sender, e);

            Single rotationToPlayer = this.GetRotationToPlayer();

            if (Single.IsNaN(rotationToPlayer)) return;

            Single rotationDiff = rotationToPlayer - this.Rotation;

            Int32 diffSign = Math.Sign(rotationDiff);

            Single toRotate = Math.Min(Math.Abs(rotationDiff), RotationSpeed * e.DeltaTime);

            this.Rotation += toRotate * diffSign;

            this.Velocity = new Vector2(MathF.Cos(this.Rotation), MathF.Sin(this.Rotation)) * Speed;
        }

        public override void OnCollision(Collider other) {
            base.OnCollision(other);

            switch (other.Parent) {
                case Player: {
                    this.OnDeath();
                    break;
                }
                case Bullet: {
                    Random rnd = new();
                    String soundName = "Hurt" + rnd.Next(1, 4);

                    Jukebox.PlaySound(soundName, 0.5F);
                    break;
                }
            }
        }

        protected override void OnDeath() {
            this.GameState.ExplosionController.SpawnExplosion(this.Position);

            Random rnd = new();
            String soundName = "Explosion" + rnd.Next(1, 4);

            Jukebox.PlaySound(soundName, 0.5F);

            this.GameState.Player.Multiplier += 0.5F;

            const Int32 score = 1200;

            this.GameState.DebrisController.SpawnScoreText(this.Position, score);

            this.GameState.Root.Score += (Int32)(score * this.GameState.Player.Multiplier);

            this.GameState.EnemySpawner.EnemiesKilled++;

            base.OnDeath();
        }

        private Single GetRotationToPlayer() {
            Player player = this.GameState.Player;

            if (player is null) return this.Rotation;

            Single xDiff = player.Position.X - this.Position.X;
            Single yDiff = player.Position.Y - this.Position.Y;

            return MathF.Atan2(yDiff, xDiff);
        }
    }
}