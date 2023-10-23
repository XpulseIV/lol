﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public class GameplayState : GameState, IUpdateEventListener
    {
        public readonly List<Entity> Entities;
        public readonly CollisionSystem CollisionSystem = new();
        public EnemySpawner EnemySpawner;

        public Player Player => (Player)this.Entities.Find(entity => entity is Player);

        public Int32 EnemiesAlive =>
            this.Entities.Count(entity => entity is not AstralAssault.Player and not Crosshair);

        private static readonly Vector4 MultiplierBrokenColor = new(1, 0, 0, 1);
        private static readonly Vector4 MultiplierIncreaseColor = new(1, 1, 0, 1);
        private static readonly Vector4 MultiplierDefaultColor = new(1, 1, 1, 1);
        private Vector4 _multiplierColor = MultiplierDefaultColor;
        private Single _prevMultiplier = 1;

        public readonly DebrisController DebrisController;
        public readonly ExplosionController ExplosionController = new();

        public GameplayState(Game1 root) : base(root) {
            this.Entities = new List<Entity>();
            this.EnemySpawner = new EnemySpawner(this);
            this.DebrisController = new DebrisController(this);
        }

        public override List<DrawTask> GetDrawTasks() {
            List<DrawTask> drawTasks = new();

            foreach (Entity entity in this.Entities) drawTasks.AddRange(entity.GetDrawTasks());

            drawTasks.AddRange(this.EnemySpawner.GetDrawTasks());
            drawTasks.AddRange(this.DebrisController.GetDrawTasks());
            drawTasks.AddRange(this.ExplosionController.GetDrawTasks());
            drawTasks.AddRange(this.GetScoreDrawTasks());

            return drawTasks;
        }

        public override void Enter() {
            this.Entities.Add(new Player(this, new Vector2(Game1.TargetWidth / 2F, Game1.TargetHeight / 2F)));
            this.Entities.Add(new Crosshair(this));
            this.Root.Score = 0;

            UpdateEventSource.UpdateEvent += this.OnUpdate;

            for (int i = 0; i < 100; i++) {
                this.Entities.Add(new Ball(this, new Vector2().Random(), new Random().NextSingle()));
            }
        }

        public override void Exit() {
            this.EnemySpawner.StopListening();
            while (this.Entities.Count > 0) this.Entities[0].Destroy();

            UpdateEventSource.UpdateEvent -= this.OnUpdate;
        }

        public void OnUpdate(Object sender, UpdateEventArgs e) {
            if (this.Player == null) return;

            Single multiplier = this.Player.Multiplier;

            if (multiplier != this._prevMultiplier) {
                this._multiplierColor =
                    multiplier > this._prevMultiplier ? MultiplierIncreaseColor : MultiplierBrokenColor;
                this._prevMultiplier = multiplier;
            }
            else
                this._multiplierColor = Vector4.Lerp(this._multiplierColor, MultiplierDefaultColor, e.DeltaTime * 2);
        }

        private List<DrawTask> GetScoreDrawTasks() {
            List<DrawTask> drawTasks = new();

            String scoreText = $"Score: {this.Root.Score}";
            Color textColor = Palette.GetColor(Palette.Colors.Grey9);
            List<DrawTask> scoreTasks = scoreText.CreateDrawTasks(new Vector2(4, 4), textColor, LayerDepth.HUD);
            drawTasks.AddRange(scoreTasks);

            String multiplierText =
                $"Score multi.: X{this.Player.Multiplier.ToString("0.0", CultureInfo.GetCultureInfo("en-US"))}";

            List<DrawTask> multiplierTasks = multiplierText.CreateDrawTasks(
                new Vector2(480 - (multiplierText.Length * 8) - 4, 4),
                textColor,
                LayerDepth.HUD,
                new List<IDrawTaskEffect> { new ColorEffect(this._multiplierColor) });
            drawTasks.AddRange(multiplierTasks);

            return drawTasks;
        }
    }
}