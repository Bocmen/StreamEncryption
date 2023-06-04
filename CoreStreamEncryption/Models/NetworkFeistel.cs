using CoreStreamEncryption.Abstract;
using CoreStreamEncryption.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CoreStreamEncryption.Models
{
    public class NetworkFeistel : IStreamTransformation
    {
        public delegate BigInteger NetworkFeistelFunction(NetworkFeistel currentSetting, BigInteger right, BigInteger key);
        public delegate BigInteger CorrectInputBlockFunction(BigInteger block, PositionOperation positionOperation);

        public readonly int CountBytes;
        public readonly int CountRound;
        private readonly NetworkFeistelFunction _function;
        private readonly CorrectInputBlockFunction _correctInputBlock;

        public readonly int PartCountBit;
        public readonly BigInteger PartMask;

        int IStreamTransformation.CountBytes => CountBytes;
        int IStreamTransformation.CountRound => CountRound;

        public NetworkFeistel(int countByte, int countRound, NetworkFeistelFunction function, CorrectInputBlockFunction correctInputBlock = null)
        {
            CountBytes =countByte <= 0 ? throw new ArgumentException(nameof(countByte)) : countByte;
            CountRound=countRound <= 0 ? throw new ArgumentException(nameof(countRound)) : countRound;
            _function=function ?? throw new ArgumentNullException(nameof(function));
            _correctInputBlock = correctInputBlock;
            PartCountBit = countByte * 4;
            PartMask = new BigInteger(1);
            for (int i = 1; i < PartCountBit; i++)
                PartMask = (PartMask << 1) | PartMask;
        }

        public IEnumerable<byte> Transformation(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            loggerIteration?.StartTranslation(this);
            byte lastVal = 0;
            int countBitsNoRead = 0;
            byte getNextVal()
            {
                if (!dataBytes.MoveNext()) throw new ArgumentOutOfRangeException(nameof(dataBytes));
                return dataBytes.Current;
            }
            while (true)
            {
                if (countBitsNoRead == 0 && dataBytes.MoveNext())
                {
                    lastVal = dataBytes.Current;
                    countBitsNoRead = 8;
                }
                else
                {
                    loggerIteration?.EndTranslation(this);
                    yield break;
                }
                BigInteger left = GenerateValue(ref lastVal, ref countBitsNoRead, getNextVal);
                BigInteger right = GenerateValue(ref lastVal, ref countBitsNoRead, getNextVal);
                BigInteger result;
                if (_correctInputBlock == null)
                    result = Transformation(left, right, keys, loggerIteration);
                else
                {
                    var blockInput = (left << PartCountBit) | right;
                    var correctResult = _correctInputBlock(blockInput, PositionOperation.Start);
                    loggerIteration?.StartBlockCorrect(this, blockInput, correctResult);
                    result = Transformation(_correctInputBlock((left << PartCountBit) | right, PositionOperation.Start), keys, loggerIteration);
                }
                byte[] bytesResult;
                if (_correctInputBlock != null)
                {
                    var correctResult = _correctInputBlock(result, PositionOperation.End);
                    loggerIteration?.EndBlockCorrect(this, result, correctResult);
                    bytesResult = correctResult.ToByteArray();
                }
                else
                    bytesResult = result.ToByteArray();
                if (bytesResult.Length < CountBytes)
                {
                    for (int i = CountBytes - bytesResult.Length; i > 0; i--)
                        yield return 0;
                }
                foreach (var byteData in bytesResult.Take(CountBytes).Reverse())
                    yield return byteData;
            }
        }
        private BigInteger GenerateValue(ref byte lastVal, ref int countBitsNoRead, Func<byte> readNextByte)
        {
            BigInteger result = BigInteger.Zero;
            int countNoRead = PartCountBit;
            while (countNoRead != 0)
            {
                if (countBitsNoRead == 0)
                {
                    lastVal = readNextByte();
                    countBitsNoRead = 8;
                }
                int countBit = Math.Min(countNoRead, countBitsNoRead);
                result <<= countBit;
                int posMaskRead = countBitsNoRead - countBit;
                result |= (lastVal & ((byte.MaxValue >> (8 - countBit)) << posMaskRead)) >> posMaskRead;
                countNoRead -= countBit;
                countBitsNoRead -= countBit;
            }
            return result;
        }
        private BigInteger Transformation(BigInteger value, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => Transformation((value >> PartCountBit) & PartMask, value & PartMask, keys, loggerIteration);
        private BigInteger Transformation(BigInteger left, BigInteger right, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            loggerIteration?.StartTranslationBlock(this, left, right);
            for (int i = 0; i < CountRound; i++)
            {
                if (!keys.MoveNext()) throw new ArgumentOutOfRangeException(nameof(keys));
                BigInteger rightTmp = right;
                BigInteger fResult = _function(this, right, keys.Current);
                right = (left ^ fResult) & PartMask;
                left = rightTmp;
                loggerIteration?.LoggerRoundIteration(this, i, left, right, fResult, keys.Current);
            }
            var result = (right << PartCountBit) | left;
            loggerIteration?.EndTranslationBlock(this, result);
            return result;
        }

        public IEnumerable<byte> Encryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => Transformation(dataBytes, EncryptionGetKeys(keys), loggerIteration);
        public IEnumerable<byte> Decryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => Transformation(dataBytes, DecryptionGetKeys(keys), loggerIteration);

        private IEnumerator<BigInteger> EncryptionGetKeys(IEnumerator<BigInteger> keys)
        {
            while (true)
            {
                if (!keys.MoveNext())
                {
                    keys.Reset();
                    keys.MoveNext();
                }
                yield return keys.Current;
            }
        }
        private IEnumerator<BigInteger> DecryptionGetKeys(IEnumerator<BigInteger> keys)
        {
            List<BigInteger> result = new List<BigInteger>();
            while (keys.MoveNext())
                result.Add(keys.Current);
            int countElements = result.Count;
            int countAdd = countElements % CountRound;
            for (int i = 0; i < countAdd; i++)
                result.Add(result[i % countElements]);
            int countBlock = result.Count / CountRound;
            result = result.Select((x, i) => (x, i)).GroupBy(x => x.i / CountRound).SelectMany(x => x.Select(y => y.x).Reverse()).ToList();
            return EncryptionGetKeys(result.GetEnumerator());
        }

        public enum PositionOperation
        {
            Start,
            End
        }
    }
}
