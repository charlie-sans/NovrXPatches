using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaseX;
using FrooxEngine;
using CloudX;
using NovrX;

namespace FrooxEngine
{
    public class Component : ComponentBase<Component>, IComponentBase, IDestroyable, IWorker, IWorldElement, IUpdatable, IChangeable, IAudioUpdatable, IInitializable, ILinkable
    {
        public Slot Slot { get; private set; }

        public bool IsUnderLocalUser => Slot?.IsUnderLocalUser ?? false;

        protected override bool CanRunUpdates => Slot.IsActive;

        

        internal void KeyAssigned(string key)
        {
            if (base.IsStarted)
            {
                MarkChangeDirty();
            }
        }

        internal void KeyRemoved(string key)
        {
            if (base.IsStarted)
            {
                MarkChangeDirty();
            }
        }

        protected override void InitializeSyncMembers()
        {
            base.InitializeSyncMembers();
        }
    }
}
