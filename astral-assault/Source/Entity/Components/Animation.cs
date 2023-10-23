using System;

namespace AstralAssault
{
    public struct Animation
    {
        public Frame[] Frames { get; }
        public Boolean HasRotation { get; }
        public Boolean IsLooping { get; }

        public Animation(Frame[] frames, Boolean hasRotation, Boolean isLooping = false) {
            this.Frames = frames;
            this.HasRotation = hasRotation;
            this.IsLooping = isLooping;
        }
    }
}