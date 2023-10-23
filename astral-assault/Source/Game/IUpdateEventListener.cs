using System;

namespace AstralAssault
{
    public interface IUpdateEventListener
    {
        void OnUpdate(Object sender, UpdateEventArgs e);
    }
}