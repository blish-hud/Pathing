using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public abstract class ManagedState : IDisposable {

        private Thread _stateLoop;

        private readonly   int            _intervalSleep;
        protected readonly IRootPackState _rootPackState;

        public bool Running { get; private set; } = false;

        protected ManagedState(IRootPackState rootPackState, int intervalSleep) {
            _rootPackState = rootPackState;
            _intervalSleep = intervalSleep;
        }

        public async Task Start() {
            if (this.Running) return;

            await Initialize();

            _stateLoop = new Thread(this.Run);
            _stateLoop.Start();
        }

        private void Stop() {
            if (!this.Running) return;

            this.Running = false;

            _stateLoop.Join();
            _stateLoop = null;
        }

        private void Run() {
            var gameTimer = Stopwatch.StartNew();

            var lastGameTime = TimeSpan.Zero;

            this.Running = true;

            while (this.Running) {
                var curGameTime = gameTimer.Elapsed;
                var gameTime    = new GameTime(curGameTime, curGameTime - lastGameTime);

                Update(gameTime);

                lastGameTime = curGameTime;

                Thread.Sleep(_intervalSleep);
            }

            Unload();

            gameTimer.Stop();
        }

        public abstract Task Reload();

        protected abstract Task<bool> Initialize();

        protected abstract void Unload();

        protected abstract void Update(GameTime gameTime);

        public void Dispose() {
            Stop();
        }

    }
}
