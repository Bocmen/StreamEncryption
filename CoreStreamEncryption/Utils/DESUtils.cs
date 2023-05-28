using CoreStreamEncryption.Models;
using System.Collections.Generic;
using System.Numerics;

namespace CoreStreamEncryption.Utils
{
    internal static partial class DESUtils
    {
        public static BigInteger Permutations(BigInteger bigInteger, NetworkFeistel.PositionOperation positionOperation)
        {
            int[] dataPermutations = positionOperation == NetworkFeistel.PositionOperation.Start ? IP : FP;
            BigInteger newBlock = new BigInteger((long)0);
            for (int i = 0; i < 64; i++)
                newBlock |= ((bigInteger >> (64 - dataPermutations[i]) & 0x01)) << (63 - i);
            return newBlock;
        }
        public static BigInteger Function(NetworkFeistel currentSetting, BigInteger block32b, BigInteger key48b)
        {
            ulong block48b = ExpansionPermutations((uint)block32b);
            block48b ^= (ulong)key48b;
            byte[] sin = new byte[8];
            for (int i = 0; i < sin.Length; i++)
                sin[i] = (byte)((block48b >> (42 - (i * 6))) & 63);
            uint merged = 0;
            List<object> sd = new List<object>();
            for (int i = 0; i < 8; i++)
            {
                byte sinCurrent = sin[i];
                int row = ((sinCurrent >> 4) & 2) | (sinCurrent & 1);
                int column = (sinCurrent & 30) >> 1;
                merged = (merged << 4) | Sbox[i][row][column];
                sd.Add(Sbox[i][row][column]);
            }
            return Permutation(merged);
        }
        public static IEnumerable<BigInteger> GenerateKeys(BigInteger keyInput56b)
        {
            uint kOne32b = 0, kTwo32b = 0;
            ulong inputCast = (ulong)keyInput56b;
            KeyPermutationPart(ref kOne32b, ref inputCast, K1P);
            KeyPermutationPart(ref kTwo32b, ref inputCast, K2P);
            for (int i = 0; i < 16; i++)
            {
                byte shiftValue;
                switch (i)
                {
                    case 0:
                    case 1:
                    case 8:
                    case 15:
                        shiftValue = 1;
                        break;
                    default:
                        shiftValue = 2;
                        break;
                }
                kOne32b = LShift28Bit(kOne32b, shiftValue);
                kTwo32b = LShift28Bit(kTwo32b, shiftValue);
                ulong block56b = kOne32b >> 4;
                block56b = ((block56b << 32) | kTwo32b) << 4;
                ulong block48b = 0;
                for (int j = 0; j < 48; j++)
                    block48b |= ((block56b >> (64 - CP[j])) & 0x01) << (63 - j);
                yield return block48b;
            }
            yield break;
        }

        private static ulong ExpansionPermutations(uint block32b)
        {
            ulong block48b = 0;
            for (int i = 0; i < 48; i++)
                block48b |= (ulong)((block32b >> (32 - EP[i])) & 0x01) << (47 - i);
            return block48b;
        }
        private static uint Permutation(uint block32b)
        {
            uint new_block32b = 0;
            for (byte i = 0; i < 32; ++i)
                new_block32b |= ((block32b >> (32 - P[i])) & 0x01) << (31 - i);
            return new_block32b;
        }
        private static void KeyPermutationPart(ref uint blockSet, ref ulong block56b, byte[] dataPermutation)
        {
            blockSet = 0;
            for (int i = 0; i < dataPermutation.Length; i++)
                blockSet |= (uint)(((block56b >> (64 - dataPermutation[i])) & 0x01) << (31 - i));
        }
        private static uint LShift28Bit(uint x, int L) => (uint)((((x) << (L)) | ((x) >> (-(L) & 27))) & (((ulong)1 << 32) - 1));
    }
}
