using CoreStreamEncryption.Interface;

namespace SharedWorksStreamEncryption.Extension
{
    public static class StreamTransformationExtension
    {
        public static int PartCountBit(this IStreamTransformation streamTransformation) => streamTransformation.CountBytes * 4;
    }
}
