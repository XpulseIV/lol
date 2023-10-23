using System;

namespace AstralAssault
{
    public class UpdateEventArgs : EventArgs
    {
        public Single DeltaTime { get; }

        public UpdateEventArgs(Single deltaTime) {
            this.DeltaTime = deltaTime;
        }
    }
}