using TmfLib.Attributes;

namespace BhModule.Community.Pathing.Utility {
    public static class AttributeCollectionExtensions {

        public static bool TryPopAttribute(this AttributeReader attributes, string attributeName, out PackAttribute attribute) {
            return attributes.TryConsume(attributeName, out attribute);
        }

    }
}
