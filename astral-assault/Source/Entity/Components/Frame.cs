using System;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public struct Frame
    {
        public Rectangle[] Rotations { get; } = new Rectangle[4];
        public Rectangle Source { get; }
        public Boolean HasRotations => this.Rotations[0] != Rectangle.Empty;
        public Int32 Time { get; }

        public Frame(Rectangle e, Rectangle see, Rectangle se, Rectangle sse, Int32 time = 0) {
            this.Rotations[0] = e;
            this.Rotations[1] = see;
            this.Rotations[2] = se;
            this.Rotations[3] = sse;

            this.Source = Rectangle.Empty;

            this.Time = time;
        }

        public Frame(Rectangle source, Int32 time = 0) {
            Array.Fill(this.Rotations, Rectangle.Empty);

            this.Source = source;

            this.Time = time;
        }
    }
}