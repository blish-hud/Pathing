using System.Threading.Tasks;
using Blish_HUD;
using TmfLib;

namespace BhModule.Community.Pathing.State {
    public interface IRootPackState : IPackState, IUpdatable {
        new int CurrentMapId { get; set; }

        Task Load();

        Task LoadPackCollection(IPackCollection collection);

        Task Unload();

    }
}
