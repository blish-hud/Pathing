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
            return new(MD5.Create().ComputeHash(Encoding.Default.GetBytes(value)));
        }

        public static string ToBase64String(this Guid guid) {
            return Convert.ToBase64String(guid.ToByteArray()).Substring(0, 22) + "==";
        }

    }
}
