using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault
{
    public class EnemySpawner : IUpdateEventListener
    {
        public Int32 EnemiesKilled { get; set; }

        private readonly GameplayState _gameState;

        private const Single BaseAsteroidSpawnInterval = 24000;
        private const Single BaseMissileSpawnInterval = 36000;
        private Single _asteroidSpawnInterval = BaseAsteroidSpawnInterval;
        private Single _missileSpawnInterval = BaseMissileSpawnInterval;
        private Int64 _lastAsteroidSpawnTime;
        private Int64 _lastMissileSpawnTime;

        private readonly List<QueuedMissile> _missileQueue = new();
        private const Int32 MissileWarningDuration = 1600;
        private const Int32 MissileWarningMargin = 16;

        private readonly Int64 _timeStarted = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        private const Int32 TimeBeforeFirstMissileSpawn = 10000;

        private readonly Texture2D _missileWarningTexture;

        public EnemySpawner(GameplayState gameState) {
            this._gameState = gameState;

            UpdateEventSource.UpdateEvent += this.OnUpdate;

            this._missileWarningTexture = AssetManager.Load<Texture2D>("MissileWarning");
        }

        private void SpawnAsteroid() {
            Random rnd = new();

            Vector2 position = GenerateEnemyPosition();
            Asteroid.Sizes size = (Asteroid.Sizes)rnd.Next(0, 3);

            Vector2 gameCenter = new(Game1.TargetWidth / 2F, Game1.TargetHeight / 2F);
            Single angleToCenter = MathF.Atan2(gameCenter.Y - position.Y, gameCenter.X - position.X);
            angleToCenter += MathHelper.ToRadians(rnd.Next(-45, 45));

            this._gameState.Entities.Add(new Asteroid(this._gameState, position, angleToCenter, size));
        }

        private void SpawnMissile(Vector2 position) {
            this._gameState.Entities.Add(new Missile(this._gameState, position));
        }

        public List<DrawTask> GetDrawTasks() => this.GetMissileWarningDrawTasks();

        public void OnUpdate(Object sender, UpdateEventArgs e) {
            /*Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if ((timeNow - this._lastAsteroidSpawnTime) > this._asteroidSpawnInterval) {
                this._lastAsteroidSpawnTime = timeNow;
                this.SpawnAsteroid();
            }

            if (((timeNow - this._lastMissileSpawnTime) > this._missileSpawnInterval) &&
                ((timeNow - this._timeStarted) > TimeBeforeFirstMissileSpawn)) {
                this._lastMissileSpawnTime = timeNow;

                Int32 amountToSpawn = (Int32)MathF.Pow(1.02F, this.EnemiesKilled);

                for (Int32 i = 0; i < amountToSpawn; i++) {
                    Vector2 position = GenerateEnemyPosition();
                    QueuedMissile queuedMissile = new(timeNow + MissileWarningDuration, position);
                    this._missileQueue.Add(queuedMissile);
                }
            }

            this.HandleQueuedMissiles();

            this._asteroidSpawnInterval = BaseAsteroidSpawnInterval * MathF.Pow(0.96F, this.EnemiesKilled);
            this._missileSpawnInterval = BaseMissileSpawnInterval * MathF.Pow(0.98F, this.EnemiesKilled);

            if (this._gameState.EnemiesAlive == 0) this._asteroidSpawnInterval = 0;*/
        }

        private List<DrawTask> GetMissileWarningDrawTasks() {
            List<DrawTask> drawTasks = new();

            const Int32 frameCount = 32;
            const Int32 timePerFrame = MissileWarningDuration / frameCount;

            foreach (QueuedMissile queuedMissile in this._missileQueue) {
                Int64 timeSpawned = queuedMissile.TimeToLaunchMS - MissileWarningDuration;
                Int32 timeSinceSpawned = (Int32)((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeSpawned);
                Int32 spriteIndex = timeSinceSpawned / timePerFrame;

                Rectangle sourceRectangle = new(24 * spriteIndex, 0, 24, 24);

                drawTasks.Add(new DrawTask(this._missileWarningTexture,
                    sourceRectangle,
                    GetMissileWarningPosition(queuedMissile.Position),
                    0,
                    LayerDepth.HUD,
                    new List<IDrawTaskEffect>(),
                    Color.White,
                    new Vector2(12, 12)));
            }

            return drawTasks;
        }

        private void HandleQueuedMissiles() {
            if (this._missileQueue.Count == 0) return;

            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            List<QueuedMissile> missilesToRemove = new();

            foreach (QueuedMissile queuedMissile in this._missileQueue) {
                if (timeNow < queuedMissile.TimeToLaunchMS) continue;

                this.SpawnMissile(queuedMissile.Position);
                missilesToRemove.Add(queuedMissile);
            }

            foreach (QueuedMissile queuedMissile in missilesToRemove) this._missileQueue.Remove(queuedMissile);
        }

        public void StopListening() {
            UpdateEventSource.UpdateEvent -= this.OnUpdate;
        }

        private static Vector2 GetMissileWarningPosition(Vector2 missileSpawnPoint) {
            Int32 x = Math.Clamp((Int32)missileSpawnPoint.X, MissileWarningMargin,
                Game1.TargetWidth - MissileWarningMargin);
            Int32 y = Math.Clamp((Int32)missileSpawnPoint.Y, MissileWarningMargin,
                Game1.TargetHeight - MissileWarningMargin);

            return new Vector2(x, y);
        }

        private static Vector2 GenerateEnemyPosition() {
            Random rnd = new();
            Int32 side = rnd.Next(0, 4);

            Int32 x = side switch {
                0 => 0,
                1 => Game1.TargetWidth,
                2 => rnd.Next(0, Game1.TargetWidth),
                3 => rnd.Next(0, Game1.TargetWidth),
                _ => throw new ArgumentOutOfRangeException()
            };

            Int32 y = side switch {
                0 => rnd.Next(0, Game1.TargetHeight),
                1 => rnd.Next(0, Game1.TargetHeight),
                2 => 0,
                3 => Game1.TargetHeight,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new Vector2(x, y);
        }
    }
}