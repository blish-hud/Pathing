using TmfLib.Attributes;
using TmfLib.Model;

namespace BhModule.Community.Pathing.Entity {

    /// <summary>
    /// What an entity is built from: a pathable from a pack or one created by a script.
    /// </summary>
    public sealed class PathableSource {

        public PathableKind Kind { get; }

        public PathingCategory Category { get; }

        public int MapId { get; }

        public IPackResourceManager ResourceManager { get; }

        public AttributeReader Attributes { get; }

        /// <summary>
        /// The trail's points, or <c>null</c> for markers and script created trails.
        /// </summary>
        public TrailData TrailData { get; }

        public PathableSource(PathableKind kind, PathingCategory category, int mapId, IPackResourceManager resourceManager, AttributeReader attributes, TrailData trailData = null) {
            this.Kind            = kind;
            this.Category        = category;
            this.MapId           = mapId;
            this.ResourceManager = resourceManager;
            this.Attributes      = attributes;
            this.TrailData       = trailData;
        }

    }
}
