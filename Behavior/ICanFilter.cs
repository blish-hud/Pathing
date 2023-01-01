namespace BhModule.Community.Pathing.Behavior {
    public interface ICanFilter {

        bool IsFiltered();

        string FilterReason();

    }
}
