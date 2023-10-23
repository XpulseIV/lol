using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public class Collider
    {
        public readonly Entity Parent;
        public Vector2 Centre;
        public Int32 Radius;
        public Boolean IsSolid;
        public readonly Single _mass;

        public Collider(Entity parent, Vector2 centre, Int32 radius, Boolean isSolid, Single mass) {
            this.Parent = parent;
            this.Centre = centre;
            this.Radius = radius / 2;
            this.IsSolid = isSolid;
            this._mass = mass;
        }

        public Collider(Entity parent, Vector2 centre, Int32 radius) {
            this.Parent = parent;
            this.Centre = centre;
            this.Radius = radius / 2;
            this.IsSolid = false;
            this._mass = 0;
        }

        public Boolean CollidesWith(
            Collider other,
            Single deltaTime) {
            Collider circle1 = this;
            Collider circle2 = other;

            Single distanceBetweenCirclesSquared =
                ((circle2.Centre.X - circle1.Centre.X) * (circle2.Centre.X - circle1.Centre.X)) +
                ((circle2.Centre.Y - circle1.Centre.Y) * (circle2.Centre.Y - circle1.Centre.Y));

            if (distanceBetweenCirclesSquared <
                ((circle1.Radius + circle2.Radius) * (circle1.Radius + circle2.Radius))) return true;

            return false;
        }

        public void SetPosition(Point position) {
            this.Centre.X = position.X;
            this.Centre.Y = position.Y;
        }
    }
}