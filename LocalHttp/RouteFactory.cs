using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.LocalHttp {
    public class RouteFactory {

        private const string STATICROUTE_404 = "/404";
        private const string STATICROUTE_500 = "/500";

        private readonly Dictionary<string, Route> _routes = new(StringComparer.OrdinalIgnoreCase);

        public RouteFactory() {
            foreach (var route in GetRoutes()) {
                _routes.Add(route.RouteAttribute.Route, Activator.CreateInstance(route.RouteType) as Route);
            }
        }

        public async Task HandleRequest(HttpListenerContext context, string forcedRoute = null) {
            if (_routes.TryGetValue(forcedRoute ?? context.Request.Url.AbsolutePath, out var route)) {
                try {
                    await route.HandleResponse(context);
                } catch (Exception) {
                    await HandleRequest(context, STATICROUTE_500);
                }
            } else {
                await HandleRequest(context, STATICROUTE_404);
            }
        }

        private static IEnumerable<(RouteAttribute RouteAttribute, Type RouteType)> GetRoutes() {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes()) {
                var routeAttribute = type.GetCustomAttribute<RouteAttribute>();

                if (routeAttribute != null) {
                    yield return (routeAttribute, type);
                }
            }
        }

    }
}
