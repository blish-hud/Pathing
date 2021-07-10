using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public abstract class ManagedState : IDisposable {
        
        protected readonly IRootPackState _rootPackState;

        public bool Running { get; private set; } = false;

        protected ManagedState(IRootPackState rootPackState) {
            _rootPackState = rootPackState;
        }

        public async Task<ManagedState> Start() {
            if (this.Running) return this;

            await Initialize();

            this.Running = true;

            return this;
        }

        public void Stop() {
            if (!this.Running) return;

            this.Running = false;
        }

        public abstract Task Reload();

        public abstract void Update(GameTime gameTime);

        protected abstract Task<bool> Initialize();

        protected abstract void Unload();

        public void Dispose() {
            Stop();
        }

    }
}
