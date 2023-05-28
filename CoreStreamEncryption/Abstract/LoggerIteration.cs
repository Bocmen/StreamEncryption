using CoreStreamEncryption.Interface;
using System.Numerics;

namespace CoreStreamEncryption.Abstract
{
    public abstract class LoggerIteration
    {
        public abstract void StartTranslation(IStreamTransformation currentStreamTransformation);
        public abstract void EndTranslation(IStreamTransformation currentStreamTransformation);
        public abstract void StartTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger left, BigInteger right);
        public abstract void LoggerRountIteration(IStreamTransformation currentStreamTransformation, int indexRount, BigInteger left, BigInteger right, BigInteger key);
        public abstract void EndTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger result);
    }
}
