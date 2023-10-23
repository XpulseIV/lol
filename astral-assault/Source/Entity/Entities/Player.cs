using System;
using System.Collections.Generic;
using System.Linq;
using AstralAssault.ShipParts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using MouseButtons = AstralAssault.InputEventSource.MouseButtons;

namespace AstralAssault
{
    public class Player : Entity, IInputEventListener, IKeyboardPressedEventListener
    {
        // Ship Parts
        private readonly Cannon _cannon = new MkICannon();
        private readonly Engine _engine = new MkIEngine();

        internal Single Multiplier = 1;

        private const Int32 LightAmmoCost = 1;
        private const Int32 HeavyAmmoCost = 5;

        private readonly Texture2D _ammoBarTexture;
        private const Int32 AmmoBarWidth = 46;
        private const Int32 AmmoBarHeight = 16;
        private const Int32 AmmoBarX = 4;
        private const Int32 AmmoBarY = Game1.TargetHeight - AmmoBarHeight - 4;

        private Vector2 _cursorPosition;
        private Tuple<Vector2, Vector2> _muzzle = new(Vector2.Zero, Vector2.Zero);
        private Boolean _lastCannon;
        private Boolean _isCrosshairActive = true;
        private Boolean _thrusterIsOn;
        private Int64 _lastTimeFired;
        private Single _delta;
        private ParticleEmitter _particleEmitter;

        public Player(GameplayState gameState, Vector2 position) : base(gameState, position) {
            this.MaxHP = 50;
            this.HP = this.MaxHP;

            this.Position = position;
            this.Rotation = MathF.PI / 2;

            this._ammoBarTexture = AssetManager.Load<Texture2D>("AmmoBar");

            this.InitSpriteRenderer();
            this.InitParticleEmitter();
            this.InitCollider();

            this.StartListening();

            this.OutOfBoundsBehavior = OutOfBounds.Bounce;

            this.IsActor = true;
            this.IsFriendly = true;
        }

        private void InitCollider() {
            this.Collider = new Collider(
                this, this.Position,
                24,
                true,
                10);
            this.GameState.CollisionSystem.AddCollider(this.Collider);
        }

        private void InitParticleEmitter() {
            Texture2D particleSpriteSheet = AssetManager.Load<Texture2D>("Particle");

            Rectangle[] textureSources = {
                new(24, 0, 8, 8),
                new(16, 0, 8, 8),
                new(8, 0, 8, 8),
                new(0, 0, 8, 8)
            };

            IParticleProperty[] particleProperties = {
                new CauseOfDeathProperty(CauseOfDeathProperty.CausesOfDeath.LifeSpan, 210),
                new ColorChangeProperty(
                    new[] {
                        Palette.Colors.Blue9,
                        Palette.Colors.Blue8,
                        Palette.Colors.Blue7,
                        Palette.Colors.Blue6,
                        Palette.Colors.Blue5,
                        Palette.Colors.Blue4,
                        Palette.Colors.Blue3
                    },
                    30),
                new SpriteChangeProperty(0, textureSources.Length, 40),
                new VelocityProperty(-1F, 1F, 0.04F, 0.1F)
            };

            this._particleEmitter = new ParticleEmitter(
                particleSpriteSheet,
                textureSources,
                20, this.Position, this.Rotation,
                particleProperties,
                LayerDepth.ThrusterFlame);
        }

        private void InitSpriteRenderer() {
            Texture2D spriteSheet = AssetManager.Load<Texture2D>("Player");

            Animation idleAnimation = new(
                new[] {
                    new Frame(
                        new Rectangle(0, 0, 32, 32),
                        new Rectangle(0, 32, 32, 32),
                        new Rectangle(0, 64, 32, 32),
                        new Rectangle(0, 96, 32, 32),
                        120)
                },
                true);

            Animation tiltRightAnimation = new(
                new[] {
                    new Frame(
                        new Rectangle(352, 0, 32, 32),
                        new Rectangle(352, 32, 32, 32),
                        new Rectangle(352, 64, 32, 32),
                        new Rectangle(352, 96, 32, 32))
                },
                true);

            Animation tiltLeftAnimation = new(
                new[] {
                    new Frame(
                        new Rectangle(32, 0, 32, 32),
                        new Rectangle(32, 32, 32, 32),
                        new Rectangle(32, 64, 32, 32),
                        new Rectangle(32, 96, 32, 32))
                },
                true);

            Transition[] transitions = {
                new(1, 0, new[] { 0 }, "Tilt", 0),
                new(0, 1, new[] { 1 }, "Tilt", 1),
                new(0, 2, new[] { 2 }, "Tilt", -1),
                new(2, 0, new[] { 0 }, "Tilt", 0),
                new(2, 1, new[] { 0, 1 }, "Tilt", 1),
                new(1, 2, new[] { 0, 2 }, "Tilt", -1)
            };

            this.SpriteRenderer = new SpriteRenderer(
                spriteSheet,
                new[] { idleAnimation, tiltRightAnimation, tiltLeftAnimation },
                LayerDepth.Foreground,
                transitions,
                new[] { "Tilt" });
        }

        public override List<DrawTask> GetDrawTasks() {
            List<DrawTask> drawTasks = new();

            if (this._thrusterIsOn)
                this._particleEmitter.StartSpawning();
            else
                this._particleEmitter.StopSpawning();

            drawTasks.AddRange(this._particleEmitter.CreateDrawTasks());
            drawTasks.AddRange(base.GetDrawTasks());

            this._thrusterIsOn = false;

            drawTasks.AddRange(this.GetAmmoBarDrawTasks());

            return drawTasks;
        }

        private void StartListening() {
            InputEventSource.KeyboardEvent += this.OnKeyboardEvent;
            InputEventSource.MouseMoveEvent += this.OnMouseMoveEvent;
            InputEventSource.MouseButtonEvent += this.OnMouseButtonEvent;
            InputEventSource.KeyboardPressedEvent += this.OnKeyboardPressedEvent;
        }

        private void StopListening() {
            InputEventSource.KeyboardEvent -= this.OnKeyboardEvent;
            InputEventSource.MouseMoveEvent -= this.OnMouseMoveEvent;
            InputEventSource.MouseButtonEvent -= this.OnMouseButtonEvent;
            InputEventSource.KeyboardPressedEvent -= this.OnKeyboardPressedEvent;
            this._particleEmitter.StopListening();
        }

        private void HandleMovement(Int32 xAxis, Int32 yAxis) {
            // acceleration and deceleration
            Vector2 forward = new Vector2(
                (Single)Math.Cos(this.Rotation),
                (Single)Math.Sin(this.Rotation)
            ) * this._engine.MoveSpeed * this._delta;

            Vector2 right = new Vector2(
                (Single)Math.Cos(this.Rotation + MathF.PI / 2),
                (Single)Math.Sin(this.Rotation + MathF.PI / 2)
            ) * this._engine.TiltSpeed * this._delta;

            this.Velocity = new Vector2(
                Math.Clamp(this.Velocity.X + (forward.X * yAxis) + (right.X * xAxis), -this._engine.MaxSpeed, this._engine.MaxSpeed),
                Math.Clamp(this.Velocity.Y + (forward.Y * yAxis) + (right.Y * xAxis), -this._engine.MaxSpeed, this._engine.MaxSpeed)
            );

            if (this.Velocity.Length() > this._engine.MaxSpeed) {
                this.Velocity.Normalize();
                this.Velocity *= this._engine.MaxSpeed;
            }
            else if (this.Velocity.Length() < -this._engine.MaxSpeed) {
                this.Velocity.Normalize();
                this.Velocity *= -this._engine.MaxSpeed;
            }
        }

        private void HandleFiring() {
            if (!this._isCrosshairActive) return;

            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if ((this._lastTimeFired + this._cannon.ShootDelay) > timeNow) return;

            if (this._cannon.CurrentAmmo < this._cannon.BulletType switch {
                    BulletType.Light => LightAmmoCost,
                    BulletType.Heavy => HeavyAmmoCost,
                    _ => throw new ArgumentOutOfRangeException()
                }) return;

            this._cannon.CurrentAmmo -= this._cannon.BulletType switch {
                BulletType.Light => LightAmmoCost,
                BulletType.Heavy => HeavyAmmoCost,
                _ => throw new ArgumentOutOfRangeException()
            };

            Random rnd = new();
            String soundName = (this._cannon.BulletType == BulletType.Heavy ? "Heavy" : "") + "Shoot" + rnd.Next(1, 4);
            Jukebox.PlaySound(soundName, 0.5F);

            this._lastTimeFired = timeNow;

            Single xDiff = this._cursorPosition.X - (this._lastCannon ? this._muzzle.Item1.X : this._muzzle.Item2.X);
            Single yDiff = this._cursorPosition.Y - (this._lastCannon ? this._muzzle.Item1.Y : this._muzzle.Item2.Y);

            Single rot = (Single)Math.Atan2(yDiff, xDiff);

            this.GameState.Entities.Add(
                new Bullet(this.GameState, this._lastCannon ? this._muzzle.Item1 : this._muzzle.Item2,
                    rot,
                    this._cannon.BulletSpeed,
                    this._cannon.BulletType));

            this._lastCannon = !this._lastCannon;
        }

        private List<DrawTask> GetAmmoBarDrawTasks() {
            DrawTask emptyDrawTask = new(this._ammoBarTexture,
                new Rectangle(0, 0, AmmoBarWidth, AmmoBarHeight),
                new Vector2(AmmoBarX, AmmoBarY),
                0,
                LayerDepth.HUD,
                new List<IDrawTaskEffect>(),
                Color.White,
                Vector2.Zero);

            const Int32 margin = 3;
            Int32 width = (Int32)((this._cannon.CurrentAmmo / (Single)this._cannon.MaxAmmo) *
                                  (AmmoBarWidth - (margin * 2)));

            DrawTask fullDrawTask = new(this._ammoBarTexture,
                new Rectangle(0, AmmoBarHeight, width + margin, AmmoBarHeight),
                new Vector2(AmmoBarX, AmmoBarY),
                0,
                LayerDepth.HUD,
                new List<IDrawTaskEffect>(),
                Color.White,
                Vector2.Zero);

            return new List<DrawTask>() { emptyDrawTask, fullDrawTask };
        }

        public override void OnCollision(Collider other) {
            base.OnCollision(other);

            if (other.Parent is not Asteroid) return;

            if (this.Multiplier > 1) Jukebox.PlaySound("MultiplierBroken");

            this.Multiplier = 1;

            Random rnd = new();

            String soundName = rnd.Next(3) switch {
                0 => "Hurt1",
                1 => "Hurt2",
                2 => "Hurt3",
                _ => throw new ArgumentOutOfRangeException()
            };

            Jukebox.PlaySound(soundName, 0.5F);
        }

        public override void Destroy() {
            this.StopListening();

            base.Destroy();
        }

        protected override void OnDeath() {
            Game1 root = this.GameState.Root;

            Jukebox.PlaySound("GameOver");

            root.GameStateMachine.ChangeState(new GameOverState(root));

            base.OnDeath();
        }

        public void OnKeyboardEvent(Object sender, KeyboardEventArgs e) {
            Int32 xAxis = 0;
            Int32 yAxis = 0;

            if (e.Keys.Contains(Keys.D)) {
                xAxis = 1;
                this.SpriteRenderer.SetAnimationCondition("Tilt", 1);
            }
            else if (e.Keys.Contains(Keys.A)) {
                xAxis = -1;
                this.SpriteRenderer.SetAnimationCondition("Tilt", -1);
            }
            else
                this.SpriteRenderer.SetAnimationCondition("Tilt", 0);

            if (e.Keys.Contains(Keys.W)) {
                yAxis = 1;
                this._thrusterIsOn = true;
            }
            else if (e.Keys.Contains(Keys.S)) yAxis = -1;

            this.HandleMovement(xAxis, yAxis);
        }

        public void OnKeyboardPressedEvent(Object sender, KeyboardEventArgs e) { }

        public void OnMouseMoveEvent(Object sender, MouseMoveEventArgs e) {
            Point scale = new((Int32)this.GameState.Root.ScaleX, (Int32)this.GameState.Root.ScaleY);
            this._cursorPosition.X = e.Position.ToVector2().X / scale.X;
            this._cursorPosition.Y = e.Position.ToVector2().Y / scale.Y;
        }

        public void OnMouseButtonEvent(Object sender, MouseButtonEventArgs e) {
            if (e.Button is MouseButtons.Left) this.HandleFiring();
        }

        public override void OnUpdate(Object sender, UpdateEventArgs e) {
            base.OnUpdate(sender, e);

            this._delta = e.DeltaTime;

            this.HandleCrosshair();

            this.ApplyFriction();

            this.UpdateMuzzlePositions();
            this.UpdateParticleEmitterPosition();
        }

        private void UpdateMuzzlePositions() {
            Vector2 muzzle1;
            Vector2 muzzle2;

            const Single x = 8;
            const Single y = 10;

            {
                Single rot = (MathF.PI / 8) * (Single)Math.Round(this.Rotation / (MathF.PI / 8));

                Single x2 = (Single)((x * Math.Cos(rot)) - (y * Math.Sin(rot)));
                Single y2 = (Single)((y * Math.Cos(rot)) + (x * Math.Sin(rot)));

                muzzle1 = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            {
                Single rot = (MathF.PI / 8) * (Single)Math.Round(this.Rotation / (MathF.PI / 8));

                Single x2 = (Single)((x * Math.Cos(rot)) + (y * Math.Sin(rot)));
                Single y2 = (Single)((-y * Math.Cos(rot)) + (x * Math.Sin(rot)));

                muzzle2 = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            this._muzzle = new Tuple<Vector2, Vector2>(muzzle1, muzzle2);
        }

        private void UpdateParticleEmitterPosition() {
            Single emitterRotation = (this.Rotation + MathF.PI) % (2 * MathF.PI);
            Vector2 emitterPosition = new(11, 0);

            {
                Single x2 =
                    (Single)((emitterPosition.X * Math.Cos(emitterRotation)) +
                             (emitterPosition.Y * Math.Sin(emitterRotation)));
                Single y2 =
                    (Single)((emitterPosition.Y * Math.Cos(emitterRotation)) +
                             (emitterPosition.X * Math.Sin(emitterRotation)));

                emitterPosition = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            this._particleEmitter.SetTransform(emitterPosition, emitterRotation);
        }

        private void ApplyFriction() {
            Single sign = Math.Sign(this.Velocity.Length());

            if (sign != 0) {
                Single direction = (Single)Math.Atan2(this.Velocity.Y, this.Velocity.X);

                this.Velocity -=
                    new Vector2((Single)Math.Cos(direction), (Single)Math.Sin(direction)) * this._engine.Friction * this._delta *
                    sign;
            }
        }

        private void HandleCrosshair() {
            // check range to cursor
            Single distance = Vector2.Distance(this.Position, this._cursorPosition);
            this._isCrosshairActive = distance >= 12;

            // rotate player
            if (this._isCrosshairActive) {
                Single xDiff = this._cursorPosition.X - this.Position.X;
                Single yDiff = this._cursorPosition.Y - this.Position.Y;

                this.Rotation = (Single)Math.Atan2(yDiff, xDiff);
            }
        }
    }
}