using CoreStreamEncryption.Abstract;
using System.Collections.Generic;
using System.Numerics;

namespace CoreStreamEncryption.Interface
{
    public interface IStreamTransformation
    {
        int CountBytes { get; }
        int CountRound { get; }

        IEnumerable<byte> Transformation(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null);
        IEnumerable<byte> Encryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null);
        IEnumerable<byte> Decryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null);
    }
}
