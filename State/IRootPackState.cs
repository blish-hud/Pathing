using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using TmfLib;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.State {
    public interface IRootPackState : IPackState, IUpdatable {
        new int CurrentMapId { get; set; }

        Task Load();

        Task LoadPackCollection(IPackCollection collection);

        Task Unload();

        IPathingEntity InitPointOfInterest(PointOfInterest pointOfInterest);

        void RemovePathingEntity(IPathingEntity entity);

    }
}
