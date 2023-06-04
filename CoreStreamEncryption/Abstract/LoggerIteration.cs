using CoreStreamEncryption.Interface;
using System.Numerics;

namespace CoreStreamEncryption.Abstract
{
    public abstract class LoggerIteration
    {
        public abstract void StartBlockCorrect(IStreamTransformation currentStreamTransformation, BigInteger blockInput, BigInteger blockOutput);
        public abstract void EndBlockCorrect(IStreamTransformation currentStreamTransformation, BigInteger blockInput, BigInteger blockOutput);

        public abstract void StartTranslation(IStreamTransformation currentStreamTransformation);
        public abstract void EndTranslation(IStreamTransformation currentStreamTransformation);
        public abstract void StartTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger left, BigInteger right);
        public abstract void LoggerRoundIteration(IStreamTransformation currentStreamTransformation, int indexRount, BigInteger left, BigInteger right, BigInteger fResult, BigInteger key);
        public abstract void EndTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger result);
    }
}
