namespace BhModule.Community.Pathing.Entity {

    public enum EntityRenderTarget {
        World,
        CompassMap,
        FullscreenMap,

        Map = CompassMap | FullscreenMap
    }

}