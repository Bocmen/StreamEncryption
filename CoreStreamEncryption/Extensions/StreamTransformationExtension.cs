using CoreStreamEncryption.Abstract;
using CoreStreamEncryption.Interface;
using System.Collections.Generic;
using System.Numerics;

namespace CoreStreamEncryption.Extensions
{
    public static class StreamTransformationExtension
    {
        public static IEnumerable<byte> Transformation<T>(this T streamTransformation, IEnumerable<byte> dataBytes, IEnumerable<BigInteger> keys, LoggerIteration loggerIteration = null) where T : IStreamTransformation
            => streamTransformation.Transformation(dataBytes.GetEnumerator(), keys.GetEnumerator(), loggerIteration);
        public static IEnumerable<byte> Encryption<T>(this T streamTransformation, IEnumerable<byte> dataBytes, IEnumerable<BigInteger> keys, LoggerIteration loggerIteration = null) where T : IStreamTransformation
            => streamTransformation.Encryption(dataBytes.GetEnumerator(), keys.GetEnumerator(), loggerIteration);
        public static IEnumerable<byte> Decryption<T>(this T streamTransformation, IEnumerable<byte> dataBytes, IEnumerable<BigInteger> keys, LoggerIteration loggerIteration = null) where T : IStreamTransformation
            => streamTransformation.Decryption(dataBytes.GetEnumerator(), keys.GetEnumerator(), loggerIteration);
    }
}
