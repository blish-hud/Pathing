using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Content;

namespace BhModule.Community.Pathing.LocalHttp.Routes.API.Pack {
    [Route("/api/pack/register")]
    public class Register : Route {

        private const string QS_TYPE = "type";
        private const string QS_URI  = "uri";

        public override async Task HandleResponse(HttpListenerContext context) {
            bool handled = false;

            if (context.Request.QueryString.AllKeys.Contains(QS_TYPE) && context.Request.QueryString.AllKeys.Contains(QS_URI)) {
                switch (context.Request.QueryString[QS_TYPE].ToLowerInvariant()) {
                    case "web":
                        var webReader = new WebReader(context.Request.QueryString[QS_URI]);
                        await webReader.InitWebReader();
                        var webPack   = TmfLib.Pack.FromIDataReader(webReader);
                        
                        await PathingModule.Instance.PackInitiator.LoadPack(webPack);
                        handled = true;
                        break;
                }
            }

            if (handled) {
                await RespondOk(context);
            } else {
                await RespondStatus(context, 500);
            }
        }

    }
}
