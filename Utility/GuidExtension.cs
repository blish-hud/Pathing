using System;
using System.Security.Cryptography;
using Encoding = System.Text.Encoding;

namespace BhModule.Community.Pathing.Utility {
    public static class GuidExtension {

        public static Guid Xor(this Guid a, Guid b) {
            unsafe {
                Int64* ap = (Int64*)&a;
                Int64* bp = (Int64*)&b;
                ap[0] ^= bp[0];
                ap[1] ^= bp[1];

                return *(Guid*)ap;
            }
        }

        public static Guid ToGuid(this string value) {
            return new(MD5.Create().ComputeHash(Encoding.Default.GetBytes(value)));
        }

    }
}
