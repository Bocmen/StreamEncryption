using CoreStreamEncryption.Abstract;
using CoreStreamEncryption.Interface;
using CoreStreamEncryption.Utils;
using System.Collections.Generic;
using System.Numerics;
using static CoreStreamEncryption.Models.DES;

namespace CoreStreamEncryption.Models
{
    public class DES_ECB_CBC : IStreamTransformation
    {
        private readonly NetworkFeistel _networkFeistel;
        public int CountBytes => _networkFeistel.CountBytes;
        public int CountRound => _networkFeistel.CountRound;
        public ModeType Mode { get; private set; }

        private BigInteger _initVector;
        private BigInteger _backBlock;
        private ModeTransformation _currentInfoTransformation;

        public DES_ECB_CBC(ModeType mode)
        {
            Mode = mode;
            _networkFeistel = new NetworkFeistel(8, 16, DESUtils.Function, CorrectBlock);
        }

        public IEnumerable<byte> Decryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            InitTransformation(ModeTransformation.Decryption);
            return _networkFeistel.Decryption(dataBytes, GenerateKeys(keys), loggerIteration);
        }
        public IEnumerable<byte> Encryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            InitTransformation(ModeTransformation.Encryption);
            return _networkFeistel.Encryption(dataBytes, GenerateKeys(keys), loggerIteration);
        }
        public IEnumerable<byte> Transformation(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            InitTransformation(ModeTransformation.Default);
            return _networkFeistel.Transformation(dataBytes, GenerateKeys(keys), loggerIteration);
        }
        private void InitTransformation(ModeTransformation modeTransformation)
        {
            _currentInfoTransformation = modeTransformation == ModeTransformation.Default ? ModeTransformation.Encryption : modeTransformation;
            _initVector = ulong.MaxValue;
        }

        private static IEnumerator<BigInteger> GenerateKeys(IEnumerator<BigInteger> inputKeysBlock)
        {
            while (inputKeysBlock.MoveNext())
            {
                foreach (var key in DESUtils.GenerateKeys(inputKeysBlock.Current))
                    yield return key;
            }
            yield break;
        }

        private BigInteger CorrectBlock(BigInteger bigInteger, NetworkFeistel.PositionOperation positionOperation)
        {
            if (positionOperation == NetworkFeistel.PositionOperation.End)
            {
                switch (Mode)
                {
                    default: return DESUtils.Permutations(bigInteger, positionOperation);
                    case ModeType.CBC:
                        switch (_currentInfoTransformation)
                        {
                            case ModeTransformation.Decryption:
                                var result = DESUtils.Permutations(bigInteger, positionOperation);
                                result ^= _initVector;
                                _initVector = _backBlock;
                                return result;
                            default:
                                _initVector = DESUtils.Permutations(bigInteger, positionOperation);
                                return _initVector;
                        }
                }
            }
            else
            {
                _backBlock = bigInteger;
                switch (Mode)
                {
                    default:
                        return DESUtils.Permutations(bigInteger, positionOperation);
                    case ModeType.CBC:
                        switch (_currentInfoTransformation)
                        {
                            case ModeTransformation.Decryption:
                                return DESUtils.Permutations(bigInteger, positionOperation);
                            default:
                                bigInteger ^= _initVector;
                                return DESUtils.Permutations(bigInteger, positionOperation);
                        }
                }
            }
            return -1;
        }

        private enum ModeTransformation : byte
        {
            Default,
            Encryption,
            Decryption
        }
    }
}
