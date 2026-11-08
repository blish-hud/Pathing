using System;
using System.Security.Cryptography;
using Encoding = System.Text.Encoding;

namespace BhModule.Community.Pathing.Utility {
    public static class GuidExtensions {

        public static Guid Xor(this Guid a, Guid b) {
            unsafe {
                long* ap = (long*)&a;
                long* bp = (long*)&b;
                ap[0] ^= bp[0];
                ap[1] ^= bp[1];

                return *(Guid*)ap;
            }
        }

        public static Guid ToGuid(this string value) {
            using var md5 = MD5.Create();
            return new(md5.ComputeHash(Encoding.Default.GetBytes(value)));
        }

        private sealed class CachedGuid {
            public readonly string Value;
            public readonly Guid   Guid;

            public CachedGuid(string value, Guid guid) {
                this.Value = value;
                this.Guid  = guid;
            }
        }

        private static volatile CachedGuid _lastCharacterGuid;

        public static Guid ToCachedCharacterGuid(this string characterName) {
            var last = _lastCharacterGuid;

            // string.Equals checks for the same reference first, which is the common case (the name string only changes with the character).
            if (last != null && string.Equals(last.Value, characterName, StringComparison.Ordinal)) {
                return last.Guid;
            }

            var guid = characterName.ToGuid();
            _lastCharacterGuid = new CachedGuid(characterName, guid);
            return guid;
        }

        public static string ToBase64String(this Guid guid) {
            return Convert.ToBase64String(guid.ToByteArray()).Substring(0, 22) + "==";
        }

    }
}
