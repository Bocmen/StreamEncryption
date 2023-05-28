using CoreStreamEncryption.Abstract;
using CoreStreamEncryption.Interface;
using CoreStreamEncryption.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CoreStreamEncryption.Models.DES;

namespace CoreStreamEncryption.Models
{
    public class DES_CFB_OFB : IStreamTransformation
    {
        public ModeType Mode { get; private set; }
        private readonly NetworkFeistel _networkFeistel;
        private ulong _vector;
        private byte _jBlockBits;
        private bool _isDecryption;

        public int CountBytes => _networkFeistel.CountBytes;
        public int CountRound => _networkFeistel.CountRound;


        public DES_CFB_OFB(ModeType modeType)
        {
            Mode = modeType;
            _networkFeistel = new NetworkFeistel(8, 16, DESUtils.Function, GetBlockEndGeneration);
        }

        public IEnumerable<byte> Transformation(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte> Encryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            StartTranslation(false);
            return CostumTransformation(_networkFeistel.Encryption(GetDataTransformation(), keys, loggerIteration).GetEnumerator(), dataBytes);
        }
        public IEnumerable<byte> Decryption(IEnumerator<byte> dataBytes, IEnumerator<BigInteger> keys, LoggerIteration loggerIteration = null)
        {
            StartTranslation(true);
            return CostumTransformation(_networkFeistel.Encryption(GetDataTransformation(), keys, loggerIteration).GetEnumerator(), dataBytes);
        }
        private void StartTranslation(bool isDecryption)
        {
            _vector = ulong.MaxValue;
            _isDecryption = isDecryption;
        }
        private IEnumerator<byte> GetDataTransformation()
        {
            while (true)
            {
                foreach (var value in BitConverter.GetBytes(_vector))
                    yield return value;
            }
        }
        private IEnumerable<byte> CostumTransformation(IEnumerator<byte> outputsData, IEnumerator<byte> inputData)
        {
            while (inputData.MoveNext())
            {
                NextBlock(outputsData);
                var result = (byte)(inputData.Current ^ _jBlockBits);
                yield return result;
                _vector <<= 8;
                switch (Mode)
                {
                    case ModeType.OFB:
                        _vector |= _jBlockBits;
                        break;
                    case ModeType.CFB:
                        if (_isDecryption)
                            _vector |= inputData.Current;
                        else
                            _vector |= result;
                        break;
                }
            }
        }
        private BigInteger GetBlockEndGeneration(BigInteger bigInteger, NetworkFeistel.PositionOperation positionOperation)
        {
            switch (positionOperation)
            {
                case NetworkFeistel.PositionOperation.Start:
                    return DESUtils.Permutations(bigInteger, positionOperation);
                default:
                    var result = DESUtils.Permutations(bigInteger, positionOperation);
                    _jBlockBits = (byte)(result >> 56);
                    return result;
            }
        }
        private void NextBlock(IEnumerator<byte> outputsData)
        {
            for (int i = 0; i < 8; i++)
                outputsData.MoveNext();
        }
    }
}
