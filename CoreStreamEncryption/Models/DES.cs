using CoreStreamEncryption.Abstract;
using CoreStreamEncryption.Interface;
using System.Collections.Generic;
using System.Numerics;

namespace CoreStreamEncryption.Models
{
    public class DES : IStreamTransformation
    {
        private IStreamTransformation _currentModelDes;

        public int CountBytes => _currentModelDes.CountBytes;
        public int CountRound => _currentModelDes.CountRound;

        public DES(ModeType modeType)
        {
            switch (modeType)
            {
                case ModeType.ECB:
                case ModeType.CBC:
                    _currentModelDes = new DES_ECB_CBC(modeType);
                    break;
                default:
                    _currentModelDes = new DES_CFB_OFB(modeType);
                    break;
            }
        }

        public IEnumerable<byte> Decryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => _currentModelDes.Decryption(dataBytes, keys, loggerIteration);
        public IEnumerable<byte> Encryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => _currentModelDes.Encryption(dataBytes, keys, loggerIteration);
        public IEnumerable<byte> Transformation(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null) => _currentModelDes.Transformation(dataBytes, keys, loggerIteration);

        public enum ModeType : byte
        {
            ECB,
            CBC,
            CFB,
            OFB
        }
    }
}
