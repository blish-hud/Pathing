using System;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using TmfLib;
using TmfLib.Pathable;
using TmfLib.Prototype;
using Attribute = TmfLib.Prototype.Attribute;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class Instance {

        // Vector3

        public Vector3 Vector3(float x, float y, float z) {
            return new Vector3(x, y, z);
        }

        // Color

        public Color Color(int r, int g, int b, int a = 255) {
            return new Color(r, g, b, a);
        }

        // Marker

        private AttributeCollection AttributeCollectionFromLuaTable(LuaTable luaTable) {
            return new AttributeCollection(luaTable.Members.Select(member => new Attribute(member.Key, member.Value.ToString())));
        }

        public StandardMarker Marker(IPackResourceManager resourceManager, LuaTable attributes = null) {
            var poi = new PointOfInterest(resourceManager, 
                                          PointOfInterestType.Marker, 
                                          attributes != null 
                                              ? AttributeCollectionFromLuaTable(attributes) 
                                              : new AttributeCollection(),
                                          PathingModule.Instance.PackInitiator.PackState.RootCategory);

            var marker = PathingModule.Instance.PackInitiator.PackState.InitPointOfInterest(poi) as StandardMarker;

            if (marker.MapId < 0) {
                // Map ID can be assumed since we only show current map icons anyways, but UI filtering doesn't know that.
                marker.MapId = GameService.Gw2Mumble.CurrentMap.Id;
            }

            return marker;
        }

        // Guid

        public Guid Guid(string base64) {
            return AttributeParsingUtil.InternalGetValueAsGuid(base64);
        }

    }
}
