using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace AstralAssault
{
    public class Entity : IUpdateEventListener
    {
        public Vector2 Position;
        public Vector2 Velocity;
        protected Single ContactDamage;
        protected Single Rotation;
        public Collider Collider;
        protected SpriteRenderer SpriteRenderer;
        protected readonly GameplayState GameState;
        protected OutOfBounds OutOfBoundsBehavior = OutOfBounds.Wrap;
        protected Boolean IsActor = false;
        protected Single MaxHP;
        protected Single HP;
        protected Int32 HealthBarYOffset = 20;

        private Boolean _isHighlighted;
        private Int64 _timeStartedHighlightingMS;
        private Single _highlightAlpha;

        public Boolean IsFriendly;

        private readonly Int64 _timeSpawned;
        public Int64 TimeSinceSpawned => (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - this._timeSpawned;

        private Texture2D _healthBarTexture;

        protected enum OutOfBounds
        {
            DoNothing,
            Wrap,
            Bounce,
            Destroy
        }

        protected Entity(GameplayState gameState, Vector2 position) {
            this.GameState = gameState;
            this.Position = position;
            this._timeSpawned = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            UpdateEventSource.UpdateEvent += this.OnUpdate;

            this.CreateHealthBarTexture();
        }

        public virtual void OnUpdate(Object sender, UpdateEventArgs e) {
            if (this.IsActor && (this.HP <= 0)) {
                this.OnDeath();
                return;
            }

            this.Position += this.Velocity * e.DeltaTime;
            this.Collider?.SetPosition(this.Position.ToPoint());

            switch (this.OutOfBoundsBehavior) {
                case OutOfBounds.DoNothing: {
                    break;
                }

                case OutOfBounds.Wrap: {
                    this.Position.X = this.Position.X switch {
                        < 0 => Game1.TargetWidth,
                        > Game1.TargetWidth => 0,
                        _ => this.Position.X
                    };

                    this.Position.Y = this.Position.Y switch {
                        < 0 => Game1.TargetHeight,
                        > Game1.TargetHeight => 0,
                        _ => this.Position.Y
                    };

                    break;
                }

                case OutOfBounds.Bounce: {
                    this.Velocity = this.Position.X switch {
                        < 0 => Vector2.Reflect(this.Velocity, Vector2.UnitX),
                        > Game1.TargetWidth => Vector2.Reflect(this.Velocity, -Vector2.UnitX),
                        var _ => this.Velocity
                    };

                    this.Velocity = this.Position.Y switch {
                        < 0 => Vector2.Reflect(this.Velocity, -Vector2.UnitY),
                        > Game1.TargetHeight => Vector2.Reflect(this.Velocity, Vector2.UnitY),
                        var _ => this.Velocity
                    };

                    break;
                }

                case OutOfBounds.Destroy: {
                    if (this.Position.X is < 0 or > Game1.TargetWidth || this.Position.Y is < 0 or > Game1.TargetHeight)
                        this.Destroy();

                    break;
                }

                default: {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual void OnCollision(Collider other) {
            if (!this.IsActor || (other.Parent.IsFriendly == this.IsFriendly)) return;

            this.HP = Math.Max(0, this.HP - other.Parent.ContactDamage);

            this._isHighlighted = true;
            this._timeStartedHighlightingMS = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            this._highlightAlpha = 0.7F;

            this.SpriteRenderer.EffectContainer.SetEffect<HighlightEffect, Single>(this._highlightAlpha);
        }

        public virtual List<DrawTask> GetDrawTasks() {
            List<DrawTask> drawTasks = new();

            if (this._isHighlighted) {
                const Single decayRate = 0.005F;

                Int64 timeNowMS = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                Single timeSinceStartedS = (timeNowMS - this._timeStartedHighlightingMS) / 1000F;

                this._highlightAlpha = 0.7F * MathF.Pow(decayRate, timeSinceStartedS);

                if (this._highlightAlpha <= 0.01) {
                    this._isHighlighted = false;
                    this._highlightAlpha = 0;
                    this.SpriteRenderer.EffectContainer.RemoveEffect<HighlightEffect>();
                }
                else
                    this.SpriteRenderer.EffectContainer.SetEffect<HighlightEffect, Single>(this._highlightAlpha);
            }

            Vector4 fullColor = Palette.GetColorVector(Palette.Colors.Green7);
            if (this.IsActor)
                drawTasks.AddRange(this.CreateBarDrawTasks(this.HP, this.MaxHP, fullColor, this.HealthBarYOffset));
            drawTasks.Add(this.SpriteRenderer.CreateDrawTask(this.Position, this.Rotation));

            return drawTasks;
        }

        public virtual void Destroy() {
            this.GameState.Entities.Remove(this);
            this.GameState.CollisionSystem.RemoveCollider(this.Collider);

            UpdateEventSource.UpdateEvent -= this.OnUpdate;
        }

        protected virtual void OnDeath() {
            this.Destroy();
        }

        private void CreateHealthBarTexture() {
            this._healthBarTexture = new Texture2D(this.GameState.Root.GraphicsDevice, 1, 1);
            Color[] data = { Color.White };
            this._healthBarTexture.SetData(data);
        }

        protected List<DrawTask> CreateBarDrawTasks(Single value, Single maxValue, Vector4 fillColor, Int32 yOffset) {
            const Int32 width = 20;
            const Int32 height = 3;

            Int32 filled = (Int32)Math.Ceiling((value / maxValue) * width);

            Int32 x = (Int32)this.Position.X - (width / 2);
            Int32 y = (Int32)this.Position.Y - yOffset;

            Rectangle outline = new(x - 1, y - 1, width + 2, height + 2);
            Rectangle emptyHealthBar = new(x, y, width, height);
            Rectangle fullHealthBar = new(x, y, filled, height);

            Vector4 outlineColor = Palette.GetColorVector(Palette.Colors.Black);
            Vector4 emptyColor = Palette.GetColorVector(Palette.Colors.Red6);

            Rectangle source = new(0, 0, 1, 1);

            DrawTask background = new(this._healthBarTexture,
                source,
                outline,
                0,
                LayerDepth.HealthBar,
                new List<IDrawTaskEffect> { new ColorEffect(outlineColor) },
                Color.Black);

            DrawTask empty = new(this._healthBarTexture,
                source,
                emptyHealthBar,
                0,
                LayerDepth.HealthBar,
                new List<IDrawTaskEffect> { new ColorEffect(emptyColor) },
                Color.Red);

            DrawTask full = new(this._healthBarTexture,
                source,
                fullHealthBar,
                0,
                LayerDepth.HealthBar,
                new List<IDrawTaskEffect> { new ColorEffect(fillColor) },
                Color.LimeGreen);

            return new List<DrawTask> { background, empty, full };
        }
    }
}