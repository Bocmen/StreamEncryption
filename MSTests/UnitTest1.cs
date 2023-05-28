using CoreStreamEncryption.Models;
using System.Numerics;
using CoreStreamEncryption.Extensions;
using static CoreStreamEncryption.Models.DES;

namespace MSTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNetworkFeistel()
        {
            Random random = new();
            int countByte = 2;
            int countRound = 8;
            int countBytesData = 1;

            NetworkFeistel networkFeistel = new(countByte, countRound, (currentSetting, right, key) =>
            {
                var resultOp = right ^ (key & currentSetting.PartMask);
                resultOp = (resultOp << 1) | ((resultOp & currentSetting.PartMask) >> (currentSetting.PartCountBit - 1));
                return resultOp & currentSetting.PartMask;
            });

            for (int i = 0; i < 100; i++)
            {
                byte[] bytesStart = new byte[countByte * countBytesData];
                BigInteger[] keys = Enumerable.Range(0, (bytesStart.Length / countByte) * countRound).Select(x => random.NextInt64()).Select(x => (BigInteger)x).ToArray();
                random.NextBytes(bytesStart);
                var encryption = networkFeistel.Encryption(bytesStart, keys).ToArray();
                var decryption = networkFeistel.Decryption(encryption, keys).ToArray();
                CollectionAssert.AreEqual(decryption, bytesStart, $"{nameof(bytesStart)}[{string.Join(", ", bytesStart)}]\n{nameof(decryption)}[{string.Join(", ", decryption)}]{nameof(keys)}[{string.Join(", ", keys)}]");
            }
        }
        public void TestDES(ModeType modeType, int countBytesData = 100)
        {
            Random random = new();
            DES des = new(modeType);

            for (int i = 0; i < 100; i++)
            {
                byte[] bytesStart = new byte[des.CountBytes * countBytesData];
                BigInteger[] keys = Enumerable.Range(0, bytesStart.Length).Select(x => random.NextInt64()).Select(x => (BigInteger)x).ToArray();
                random.NextBytes(bytesStart);
                var encryption = des.Encryption(bytesStart, keys).ToArray();
                var decryption = des.Decryption(encryption, keys).ToArray();
                CollectionAssert.AreEqual(decryption, bytesStart, $"{nameof(bytesStart)}[{string.Join(", ", bytesStart)}]\n{nameof(decryption)}[{string.Join(", ", decryption)}]{nameof(keys)}[{string.Join(", ", keys)}]");
            }
        }
        [TestMethod]
        public void TestDES_ECB() => TestDES(ModeType.ECB);

        [TestMethod]
        public void TestDES_CBC() => TestDES(ModeType.CBC);

        [TestMethod]
        public void TestDES_CFB() => TestDES(ModeType.CFB);

        [TestMethod]
        public void TestDES_OFB() => TestDES(ModeType.OFB);
    }
}