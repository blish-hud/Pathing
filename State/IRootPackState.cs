using System.Collections.Generic;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using TmfLib.Model;

namespace BhModule.Community.Pathing.State {
    public interface IRootPackState : IPackState, IUpdatable {

        new int CurrentMapId { get; set; }

        Task Load();

        /// <summary>
        /// Replaces the loaded entities with those from <paramref name="slices"/>.
        /// </summary>
        Task LoadMapSlices(PathingCategory rootCategory, IReadOnlyList<MapSlice> slices);

        /// <summary>
        /// Unloads the entities while keeping the categories and textures.
        /// </summary>
        Task UnloadEntities();

        Task Unload();

        IPathingEntity InitPathable(PathableSource source);

        void RemovePathingEntity(IPathingEntity entity);

    }
}
