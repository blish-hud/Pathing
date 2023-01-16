using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Neo.IronLua;
using TmfLib.Pathable;
using Attribute = TmfLib.Prototype.Attribute;

namespace BhModule.Community.Pathing.Scripting.Lib; 

public class World {

    private readonly Dictionary<string, Guid> _guidCache = new(StringComparer.InvariantCultureIgnoreCase);

    // Categories

    public PathingCategory RootCategory => PathingModule.Instance.PackInitiator.PackState.RootCategory;

    public PathingCategory CategoryByType(string id) {
        return PathingModule.Instance.PackInitiator.PackState.RootCategory.TryGetCategoryFromNamespace(id, out var category)
                   ? category 
                   : null;
    }

    // Pathables

    public IPathingEntity PathableByGuid(string guid) {
        if (!_guidCache.TryGetValue(guid, out var g)) {
            _guidCache.Add(guid, g = AttributeParsingUtil.InternalGetValueAsGuid(guid));
        }

        foreach (var pathable in PathingModule.Instance.PackInitiator.PackState.Entities) {
            if (pathable.Guid == g) {
                return pathable;
            }
        }

        return null;
    }

    public LuaTable PathablesByGuid(string guid) {
        var nTable = new LuaTable();

        if (!_guidCache.TryGetValue(guid, out var g)) {
            _guidCache.Add(guid, g = AttributeParsingUtil.InternalGetValueAsGuid(guid));
        }

        foreach (var pathable in PathingModule.Instance.PackInitiator.PackState.Entities) {
            if (pathable.Guid == g) {
                nTable.Add(pathable);
            }
        }

        return nTable;
    }

    // Markers

    public StandardMarker MarkerByGuid(string guid) {
        return PathableByGuid(guid) is StandardMarker marker 
                   ? marker 
                   : null;
    }

    public StandardMarker GetClosestMarker() {
        return PathingModule.Instance.PackInitiator.PackState.Entities.ToArray().OfType<StandardMarker>().OrderBy(marker => marker.DistanceToPlayer).FirstOrDefault();
    }

    public LuaTable GetClosestMarkers(int quantity) {
        var nTable = new LuaTable();

        foreach (var marker in PathingModule.Instance.PackInitiator.PackState.Entities.ToArray().OfType<StandardMarker>().OrderBy(marker => marker.DistanceToPlayer).Take(quantity)) {
            nTable.Add(marker);
        }

        return nTable;
    }

    // Trails

    public StandardTrail TrailByGuid(string guid) {
        return PathableByGuid(guid) is StandardTrail trail
                   ? trail
                   : null;
    }

}