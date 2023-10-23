using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public class CollisionSystem : IUpdateEventListener
    {
        public List<Collider> Colliders { get; } = new();
        private List<Tuple<Collider, Collider>> _lastCollisions = new();

        public CollisionSystem() {
            UpdateEventSource.UpdateEvent += this.OnUpdate;
        }

        public static void CalculateVelocities(Collider a, Collider b) {
            Vector2 v1 = a.Parent.Velocity;
            Vector2 v2 = b.Parent.Velocity;

            Single m1 = a._mass;
            Single m2 = b._mass;

            Vector2 x1 = a.Centre;
            Vector2 x2 = b.Centre;

            
            return;
            
            Vector2 v1New = v1 - (((2 * m2) / (m1 + m2)) * (Vector2.Dot(v1 - v2, x1 - x2) / (x1 - x2).LengthSquared()) *
                                  (x1 - x2));
            Vector2 v2New = v2 - (((2 * m1) / (m1 + m2)) * (Vector2.Dot(v2 - v1, x2 - x1) / (x2 - x1).LengthSquared()) *
                                  (x2 - x1));

            a.Parent.Velocity = v1New;
            b.Parent.Velocity = v2New;
        }

        public void OnUpdate(Object sender, UpdateEventArgs e) {
            List<Tuple<Collider, Collider>> currentCollisions = new();

            for (Int32 i = 0; i < (this.Colliders.Count - 1); i++) {
                Collider collider = this.Colliders[i];
                for (Int32 j = i + 1; j < this.Colliders.Count; j++) {
                    Collider other = this.Colliders[j];
                    if (collider == other) continue;

                    if (collider.CollidesWith(other, e.DeltaTime)) {
                        if (collider.IsSolid && other.IsSolid && (collider.Parent.TimeSinceSpawned > 1000))
                            CalculateVelocities(collider, other);

                        Tuple<Collider, Collider> colliderPair = new(collider, other);
                        currentCollisions.Add(colliderPair);
                        if (this._lastCollisions.Contains(colliderPair)) continue;

                        collider.Parent.OnCollision(other);
                        other.Parent.OnCollision(collider);
                    }
                }
            }

            this._lastCollisions = currentCollisions;
        }

        public void AddCollider(Collider collider) {
            this.Colliders.Add(collider);
        }

        public void RemoveCollider(Collider collider) {
            this.Colliders.Remove(collider);
        }
    }
}