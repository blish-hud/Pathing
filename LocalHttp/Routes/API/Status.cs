using System.Net;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp.Routes.API {
    [Route("/api/status")]
    public class Status : Route {

        public override async Task HandleResponse(HttpListenerContext context) {
            if (context.Request.HttpMethod == "GET") {
                await Respond(new {
                                  OverlayVersion = Blish_HUD.Program.OverlayVersion.ToString(),
                                  PathingVersion = PathingModule.Instance.Version.ToString()
                              }, context);
            } else {
                await base.HandleResponse(context);
            }
        }

    }
}
