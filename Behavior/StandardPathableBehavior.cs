namespace BhModule.Community.Pathing.Behavior {

    public enum StandardPathableBehavior : int {
        // TacO
        AlwaysVisible               = 0,
        ReappearOnMapChange         = 1,
        ReappearOnDailyReset        = 2,
        OnlyVisibleBeforeActivation = 3,
        ReappearAfterTimer          = 4,
        ReappearOnMapReset          = 5, // Not currently implemented (neither by us nor TacO)
        OncePerInstance             = 6,
        OnceDailyPerCharacter       = 7,

        // Blish HUD
        ReappearOnWeeklyReset = 101
    }

}