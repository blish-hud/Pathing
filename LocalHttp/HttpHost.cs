using System;
using System.Net;
using System.Threading;

namespace BhModule.Community.Pathing.LocalHttp {
    public class HttpHost {

        private readonly HttpListener _listener;
        private readonly RouteFactory _router;

        private readonly int _threads;

        private int _threadGeneration = 0;

        public HttpHost(int port, int threads = 4) {
            _threads = threads;

            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.Expect100Continue      = false;
            ServicePointManager.MaxServicePoints       = 100;

            _listener = new HttpListener();
            _router   = new RouteFactory();

            _listener.Prefixes.Add($"http://localhost:{port}/");
        }

        public void Start() {
            _listener.Start();

            for (int i = 0; i < _threads; i++) {
                var listenThread = new Thread(ListenerThread) {
                    Priority     = ThreadPriority.Normal,
                    IsBackground = true
                };

                listenThread.Start();
            }
        }

        public void Close() {
            _threadGeneration++;
            _listener.Close();
        }

        private void ListenerThread() {
            IAsyncResult syncResult = null;

            int threadGeneration = _threadGeneration;

            while (threadGeneration == _threadGeneration) {
                syncResult = _listener.BeginGetContext(ListenerCallback, _listener);
                syncResult.AsyncWaitHandle.WaitOne();
            }
        }

        private async void ListenerCallback(IAsyncResult result) {
            try {
                if (result.AsyncState is HttpListener listener) {
                    // Call EndGetContext to complete the asynchronous operation.
                    var context = listener.EndGetContext(result);

                    await _router.HandleRequest(context);
                }
            } catch (Exception) {
                /* NOOP */
            }
        }

    }
}
