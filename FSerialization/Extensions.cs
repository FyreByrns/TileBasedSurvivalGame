using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSerialization {
    public static class Extensions {
        public static byte[] FSData(this string me) {
            List<byte> bytes = new List<byte>();
            bytes.Append(me);
            return bytes.ToArray();
        }
    }
}
