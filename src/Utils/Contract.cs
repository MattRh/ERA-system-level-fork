using System;

namespace src.Utils
{
    public class Contract
    {
        public static void Requires(bool condition)
        {
            if (!condition) {
                throw new Exception("Contract failed");
            }
        }
    }
}