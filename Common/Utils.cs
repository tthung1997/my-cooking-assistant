using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class Utils
    {
        public static byte[] GenerateByteArray(int length)
        {
            byte[] randomByteArray = new byte[length];
            Random random = new Random(Guid.NewGuid().GetHashCode());
            random.NextBytes(randomByteArray);
            return randomByteArray;
        }
    }
}
