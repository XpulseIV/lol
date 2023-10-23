using System;

namespace AstralAssault.ShipParts
{
    public abstract class Engine
    {
        public Single MoveSpeed;
        public Single MaxSpeed;
        public Single TiltSpeed;
        public Single Friction;
    }

    public class MkIEngine : Engine
    {
        public MkIEngine() {
            this.MoveSpeed = 200;
            this.MaxSpeed = 100;
            this.TiltSpeed = 200;
            this.Friction = 30;
        }
    }
}