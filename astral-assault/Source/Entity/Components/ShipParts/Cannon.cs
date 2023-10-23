using System;
using System.Collections.Generic;
using System.Linq;

namespace AstralAssault.ShipParts
{
    public abstract class Cannon
    {
        public Int32 MaxAmmo;
        public Int32 CurrentAmmo;
        public BulletType BulletType;
        public Single BulletSpeed;
        public Int32 ShootDelay;
    }

    public class MkICannon : Cannon
    {
        public MkICannon() {
            this.MaxAmmo = 200;
            this.CurrentAmmo = 50;
            this.BulletType = BulletType.Light;
            this.BulletSpeed = 250;
            this.ShootDelay = 200;
        }
    }
}