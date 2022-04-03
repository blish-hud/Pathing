using System.Net;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp.Routes.API.Edit {
    [Route("/api/edit/history")]
    public class History : Route {

        public override Task HandleResponse(HttpListenerContext context) {
            return base.HandleResponse(context);
        }

    }
}
