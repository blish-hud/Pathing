using BhModule.Community.Pathing.Behavior.Filter;
using Blish_HUD;
using BhModule.Community.Pathing.Behavior.Modifier;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Utility
{
    internal static class CategoryAttributeUtil
    {

        internal static bool TryGetAchievementId(this PathingCategory pathingCategory, out int achievementId) {
            achievementId = -1;

            if (pathingCategory.ExplicitAttributes.TryGetAttribute(AchievementFilter.ATTR_ID, out var achievementAttr) &&
                InvariantUtil.TryParseInt(achievementAttr?.Value, out achievementId)) {
                return true;
            }

            return false;
        }

        internal static bool TryGetAchievementBit(this PathingCategory pathingCategory, out int achievementBit) {
            achievementBit = -1;
            
            if (pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_BIT, out var achievementBitAttr) &&
                    InvariantUtil.TryParseInt(achievementBitAttr, out achievementBit)) {
                return true;
            }

            return false;
        }

        internal static bool TryGetCopy(this PathingCategory pathingCategory, out string copyValue)
        {
            copyValue   = string.Empty;

            if (pathingCategory.ExplicitAttributes.TryGetAttribute(CopyModifier.PRIMARY_ATTR_NAME, out var copyValueAttr))
            {
                copyValue = copyValueAttr.GetValueAsString();
                return true;

            }

            return false;
        }

        internal static bool TryGetCopyMessage(this PathingCategory pathingCategory, out string copyMessage)
        {
            copyMessage = CopyModifier.DEFAULT_COPYMESSAGE;

            if (pathingCategory.ExplicitAttributes.TryGetAttribute(CopyModifier.ATTR_MESSAGE, out var copyMessageAttr))
            {
                copyMessage = copyMessageAttr.GetValueAsString();

                return true;
            }

            return false;
        }

    }
}
