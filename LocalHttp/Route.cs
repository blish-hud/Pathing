using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BhModule.Community.Pathing.LocalHttp {
    public abstract class Route {

        public virtual async Task HandleResponse(HttpListenerContext context) {
            await RespondStatus(context, 405);
        }

        protected async Task Respond(string source, HttpListenerContext context, int httpStatusCode = 200, string contentType = "text/html; charset=UTF-8") {
            var response = context.Response;

            if (response.ContentLength64 > 0) {
                return;
            }

            byte[] outBytes = Encoding.UTF8.GetBytes(source);

            response.StatusCode      = httpStatusCode;
            response.ContentType     = contentType;
            response.ContentLength64 = outBytes.Length;
            await response.OutputStream.WriteAsync(outBytes, 0, outBytes.Length);
            response.OutputStream.Close();
        }

        protected async Task Respond(object obj, HttpListenerContext context, int httpStatusCode = 200, string contentType = "application/json") {
            await Respond(JsonConvert.SerializeObject(obj, Formatting.Indented), context, httpStatusCode, contentType);
        }

        protected async Task RespondOk(HttpListenerContext context) {
            await Respond("OK", context);
        }

        protected async Task RespondStatus(HttpListenerContext context, int statusCode) {
            await Respond(statusCode.ToString(), context, statusCode);
        }

    }
}
