using System.Runtime.InteropServices;

namespace BhModule.Community.Pathing.Utility {

    internal static class StableSortKey {

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatBits {
            [FieldOffset(0)] public float F;
            [FieldOffset(0)] public uint  U;
        }

        private static uint OrderedBits(float key) {
            if (float.IsNaN(key)) return 0;
            if (key == 0f) key = 0f; // Normalize -0 to +0

            uint bits = new FloatBits { F = key }.U;
            return (bits & 0x80000000u) != 0 ? ~bits : bits | 0x80000000u;
        }

        public static ulong Ascending(float key, int index) => ((ulong)OrderedBits(key) << 32) | (uint)index;

        public static ulong Descending(float key, int index) => ((ulong)~OrderedBits(key) << 32) | (uint)index;

        public static int GetIndex(ulong sortKey) => (int)(uint)sortKey;

    }
}
