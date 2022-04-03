using System.Net;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp.Routes {
    [Route("/404")]
    public class HttpStatus404 : Route {

        public override async Task HandleResponse(HttpListenerContext context) {
            await RespondStatus(context, 404);
        }

    }
}
