using System.Net;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp.Routes {
    [Route("/500")]
    public class HttpStatus500 : Route {

        public override async Task HandleResponse(HttpListenerContext context) {
            await RespondStatus(context, 500);
        }

    }
}
