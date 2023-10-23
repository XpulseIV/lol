using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class Crosshair : Entity, IMouseEventListener
    {
        public Crosshair(GameplayState gameState) : base(gameState, new Vector2(0, 0)) {
            InputEventSource.MouseButtonEvent += this.OnMouseButtonEvent;
            InputEventSource.MouseMoveEvent += this.OnMouseMoveEvent;

            Texture2D spriteSheet = AssetManager.Load<Texture2D>("Crosshair");

            Frame activeFrame = new(new Rectangle(0, 0, 16, 16));
            Frame inactiveFrame = new(new Rectangle(64, 0, 16, 16));

            Animation deactivateAnimation = new(
                new[] {
                    new Frame(new Rectangle(16, 0, 16, 16), 30),
                    new Frame(new Rectangle(32, 0, 16, 16), 20),
                    new Frame(new Rectangle(48, 0, 16, 16), 10)
                },
                false);

            Animation activateAnimation = new(
                new[] {
                    new Frame(new Rectangle(48, 0, 16, 16), 30),
                    new Frame(new Rectangle(32, 0, 16, 16), 20),
                    new Frame(new Rectangle(16, 0, 16, 16), 10)
                },
                false);

            Animation activeAnimation = new(new[] { activeFrame }, false);
            Animation inactiveAnimation = new(new[] { inactiveFrame }, false);

            Transition[] transitions = {
                new(0, 1, new[] { 2, 1 }, "IsActive", 0),
                new(1, 0, new[] { 3, 0 }, "IsActive", 1)
            };

            this.SpriteRenderer = new SpriteRenderer(
                spriteSheet,
                new[] { activeAnimation, inactiveAnimation, deactivateAnimation, activateAnimation },
                LayerDepth.Crosshair,
                transitions,
                new[] { "IsActive" });

            this.OutOfBoundsBehavior = OutOfBounds.DoNothing;
        }

        public override void Destroy() {
            InputEventSource.MouseButtonEvent -= this.OnMouseButtonEvent;
            InputEventSource.MouseMoveEvent -= this.OnMouseMoveEvent;

            base.Destroy();
        }

        public override void OnUpdate(Object sender, UpdateEventArgs e) {
            if (this.GameState.Player == null) return;

            Vector2 playerPosition = this.GameState.Player.Position;
            Single distance = Vector2.Distance(playerPosition, this.Position);

            this.SpriteRenderer.SetAnimationCondition("IsActive", distance < 12 ? 0 : 1);
        }

        public void OnMouseButtonEvent(Object sender, MouseButtonEventArgs e) { }

        public void OnMouseMoveEvent(Object sender, MouseMoveEventArgs e) {
            Point scale = new((Int32)this.GameState.Root.ScaleX, (Int32)this.GameState.Root.ScaleY);
            this.Position = (e.Position / scale).ToVector2();
        }
    }
}