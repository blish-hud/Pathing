using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Utility {
    public static class AttributeCollectionExtensions {

        public static bool TryPopAttribute(this AttributeCollection attributeCollection, string attributeName, out IAttribute attribute) {
            if (attributeCollection.TryGetAttribute(attributeName, out var popAttr)) {
                attributeCollection.Remove(popAttr);
            }

            attribute = popAttr;

            return attribute != null;
        }

    }
}
