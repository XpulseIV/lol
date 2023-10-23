using System;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public struct QueuedMissile
    {
        public Int64 TimeToLaunchMS { get; }
        public Vector2 Position { get; }

        public QueuedMissile(Int64 timeToLaunchMS, Vector2 position) {
            this.TimeToLaunchMS = timeToLaunchMS;
            this.Position = position;
        }
    }
}