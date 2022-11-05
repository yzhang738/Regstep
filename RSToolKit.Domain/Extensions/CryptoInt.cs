using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RSToolKit.Domain
{
    public class CryptoInt : IDisposable
    {
        private const int BufferSize = 1024;  // must be a multiple of 4
        private byte[] RandomBuffer;
        private int BufferOffset;
        private RNGCryptoServiceProvider rng;

        public CryptoInt()
        {
            RandomBuffer = new byte[BufferSize];
            rng = new RNGCryptoServiceProvider();
            BufferOffset = RandomBuffer.Length;
        }

        private void FillBuffer()
        {
            rng.GetBytes(RandomBuffer);
            BufferOffset = 0;
        }

        public int Next(bool allowNegative = false)
        {
            if (BufferOffset >= RandomBuffer.Length)
            {
                FillBuffer();
            }

            int val = BitConverter.ToInt32(RandomBuffer, BufferOffset);
            if (!allowNegative)
                val &= 0x7fffffff;
            BufferOffset += sizeof(int);
            return val;
        }

        public int Next(int maxValue, bool allowNegative = false)
        {
            return Next(allowNegative) % maxValue;
        }

        public int Next(int minValue, int maxValue)
        {
            var allowNegative = false;
            if (minValue < 0)
                allowNegative = true;
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue must be greater than or equal to minValue");
            }
            int range = maxValue - minValue;
            return minValue + Next(range, allowNegative);
        }

        public double NextDouble(bool allowNegative = false)
        {
            int val = Next(allowNegative);
            return (double)val / int.MaxValue;
        }

        public void GetBytes(byte[] buff)
        {
            rng.GetBytes(buff);
        }

        public void Dispose()
        {
            rng.Dispose();
        }
    }
}
