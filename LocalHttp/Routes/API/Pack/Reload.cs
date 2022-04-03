using System.Net;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp.Routes.API.Pack {
    [Route("/api/pack/reload")]
    public class Reload : Route {

        public override async Task HandleResponse(HttpListenerContext context) {
            PathingModule.Instance.PackInitiator.ReloadPacks();

            await RespondOk(context);
        }

    }
}
