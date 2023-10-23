using System;
using Microsoft.Xna.Framework;

namespace AstralAssault
{
    public static class UpdateEventSource
    {
        public static event EventHandler<UpdateEventArgs> UpdateEvent;

        public static void Update(GameTime gameTime) {
            Single deltaTime = (Single)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateEvent?.Invoke(null, new UpdateEventArgs(deltaTime));
        }
    }
}