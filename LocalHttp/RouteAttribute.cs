using System;

namespace BhModule.Community.Pathing.LocalHttp {
    [AttributeUsage(AttributeTargets.Class)]
    public class RouteAttribute : Attribute {

        public string Route { get; }

        public RouteAttribute(string route) {
            this.Route = route;
        }

    }
}
