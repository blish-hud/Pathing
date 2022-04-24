using System.Net;
using System.Threading.Tasks;
using Blish_HUD;

namespace BhModule.Community.Pathing.LocalHttp.Routes.API {
    [Route("/api/status")]
    public class Status : Route {

        public override async Task HandleResponse(HttpListenerContext context) {
            if (context.Request.HttpMethod == "GET") {
                await Respond(new {
                                  OverlayVersion = Program.OverlayVersion.ToString(),
                                  PathingVersion = PathingModule.Instance.Version.ToString(),
                                  PlayerMapX = GameService.Gw2Mumble.UI.MapCenter.X,
                                  PlayerMapY = GameService.Gw2Mumble.UI.MapCenter.Y,
                              }, context);
            } else {
                await base.HandleResponse(context);
            }
        }

    }
}
