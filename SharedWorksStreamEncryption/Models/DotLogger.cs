using CoreStreamEncryption.Interface;
using SharedWorksStreamEncryption.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SharedWorksStreamEncryption.Models
{
    public class DotLogger : CoreStreamEncryption.Abstract.LoggerIteration
    {
        private const string LabelRight = "Right";
        private const string LabelLeft = "Left";
        private const string LabelFunction = "F";
        private const string LabelRound = "Round [{0}]";
        private const string LabelNameSubgraphBlock = "Block [{0}]";
        private const string LabelKey = "Key";

        private const string LabelInputBlock = "Input";
        private const string LabelEditInputBlock = "EditTo";

        private const string LabelOutputBlock = "Combine";
        private const string LabelEditOutputBlock = LabelEditInputBlock;

        public delegate void Write(string data);

        private readonly Write _writer;
        private readonly string _nameDefaultFunction;
        private readonly ConnectType _connectTypeDefaultFunction;
        private long _indexator = 0;
        private long _lastIndexLeft;
        private long _lastIndexRight;
        private long _indexKey;
        private long _indexBlock = 0;
        private readonly List<OperationFunctionInfo> _functionOperations = new List<OperationFunctionInfo>();
        private BigInteger? originalInputBlock;
        private BigInteger? originalOutputBlock;

        public DotLogger(Write writer)
        {
            _writer = writer;
        }

        public DotLogger(Write writer, string nameFunction, ConnectType connectType)
        {
            _writer = writer;
            _nameDefaultFunction = nameFunction;
            _connectTypeDefaultFunction = connectType;
        }

        private long GetUnique() => ++_indexator;
        private string GetNameLastLeft() => $"left_{_indexBlock}_{_lastIndexLeft}";
        private string GetNameLastRight() => $"right_{_indexBlock}_{_lastIndexRight}";
        private string GetNameLastKey() => $"key_{_indexKey}";

        private static string GetContentElementOperation(string operationName, BigInteger value, long countBit)
        {
            char[] result = new char[operationName.Length + countBit * 2];
            for (int i = 0; i < operationName.Length; i++)
                result[i] = operationName[i];
            for (int i = result.Length - 1; i > operationName.Length; i-=2)
            {
                char setChar = (value & 1) == 0 ? '0' : '1';
                value >>= 1;
                result[i] = setChar;
                result[i - 1] = '|';
            }
            return new string(result);
        }
        public override void StartTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger left, BigInteger right)
        {
            _indexBlock++;
            _lastIndexLeft = GetUnique();
            _lastIndexRight = GetUnique();
            _writer($"subgraph cluster_Block_{_indexBlock} {{");
            _writer($"subgraph clusterStartBlock_{_indexBlock} {{");
            _writer($"label=\"{string.Format(LabelNameSubgraphBlock, _indexBlock)}\";");
            BigInteger inputBlock = (left << currentStreamTransformation.PartCountBit()) | right;
            BigInteger? editInputBlock = null;
            if (originalInputBlock != null)
            {
                editInputBlock = inputBlock;
                inputBlock = (BigInteger)originalInputBlock;
            }
            string nameLastBlock = $"{nameof(LabelInputBlock)}_{GetUnique()}";
            _writer($"{nameLastBlock}[shape=record, label=\"{GetContentElementOperation(LabelInputBlock, inputBlock, currentStreamTransformation.CountBytes * 8)}\"];");
            if (originalInputBlock != null)
            {
                string nameBlock = $"{nameof(LabelEditInputBlock)}_{GetUnique()}";
                _writer($"{nameBlock}[shape=record, label=\"{GetContentElementOperation(LabelEditInputBlock, (BigInteger)editInputBlock, currentStreamTransformation.CountBytes * 8)}\"];");
                _writer($"{nameLastBlock} -> {nameBlock};");
                nameLastBlock = nameBlock;
                originalInputBlock = null;
            }
            _writer($"{GetNameLastLeft()}[shape=record, label=\"{GetContentElementOperation(LabelLeft, left, currentStreamTransformation.PartCountBit())}\"];");
            _writer($"{GetNameLastRight()}[shape=record, label=\"{GetContentElementOperation(LabelRight, right, currentStreamTransformation.PartCountBit())}\"];");
            _writer($"{nameLastBlock} -> {GetNameLastLeft()};");
            _writer($"{nameLastBlock} -> {GetNameLastRight()};");
            _writer("}");
        }

        public override void EndTranslationBlock(IStreamTransformation currentStreamTransformation, BigInteger result)
        {
            _writer($"subgraph cluster_ResultBlock_{_indexBlock} {{");
            string nameElement = $"resultBlock_{_indexBlock}";
            _writer($"{nameElement}[shape=record, label=\"{GetContentElementOperation(LabelOutputBlock, result, currentStreamTransformation.CountBytes * 8)}\"];");
            _writer($"{GetNameLastRight()} -> {nameElement};");
            _writer($"{GetNameLastLeft()} -> {nameElement};");
            if (originalOutputBlock != null)
            {
                string nameBlock = $"{nameof(LabelOutputBlock)}_{GetUnique()}";
                _writer($"{nameBlock}[shape=record, label=\"{GetContentElementOperation(LabelEditOutputBlock, (BigInteger)originalOutputBlock, currentStreamTransformation.CountBytes * 8)}\"];");
                originalOutputBlock = null;
                _writer($"{nameElement} -> {nameBlock};");
            }
            _writer("}}");
        }
        private string GetNameOperation(int indexOparation, int round) => $"F_Op_{_indexBlock}_{round}_{indexOparation}";
        public override void LoggerRoundIteration(IStreamTransformation currentStreamTransformation, int indexRound, BigInteger left, BigInteger right, BigInteger key)
        {
            string oldNameLeft = GetNameLastLeft();
            _lastIndexLeft = GetUnique();
            _indexKey = GetUnique();

            _writer($"subgraph clusterRound_{_indexBlock}_{indexRound} {{");
            _writer($"label=\"{string.Format(LabelRound, indexRound + 1)}\";");
            _writer($"{GetNameLastKey()}[shape=record, label=\"{GetContentElementOperation(LabelKey, key, currentStreamTransformation.PartCountBit())}\"];");
            string rightName = GetNameLastRight();
            if (_functionOperations.Any())
            {
                _writer($"subgraph clusterF_{GetUnique()} {{");
                _writer($"label=\"{LabelFunction}\";");
                int indexOperation = 0;
                string currentNameOperation = string.Empty;
                foreach (var operation in _functionOperations)
                    _writer($"{GetNameOperation(indexOperation++, indexRound)}[shape=record, label=\"{GetContentElementOperation(operation.Name, operation.Result, operation.CountBit)}\"];");
                _writer("}");
                indexOperation = 0;
                foreach (var operation in _functionOperations)
                {
                    currentNameOperation = GetNameOperation(indexOperation++, indexRound);
                    if (operation.ConnectToOperations != null)
                    {
                        foreach (var connectTo in operation.ConnectToOperations)
                            _writer($"{GetNameOperation(connectTo, indexRound)} -> {currentNameOperation};");
                    }
                    LoggFunctionConnect(operation.ConnectType, currentNameOperation);
                }
                rightName = currentNameOperation;
            }
            else if(_nameDefaultFunction != null)
            {
                rightName = GetNameOperation(0, indexRound);
                _writer($"{rightName}[shape=record, label=\"{_nameDefaultFunction}\"]");
                LoggFunctionConnect(_connectTypeDefaultFunction, rightName);
            }
            void LoggFunctionConnect(ConnectType connectType, string connectTo)
            {
                if (connectType.HasFlag(ConnectType.Right)) _writer($"{GetNameLastRight()} -> {connectTo};");
                if (connectType.HasFlag(ConnectType.Left)) _writer($"{GetNameLastLeft()} -> {connectTo};");
                if (connectType.HasFlag(ConnectType.Key)) _writer($"{GetNameLastKey()} -> {connectTo};");
            }
            _functionOperations.Clear();
            _writer($"{GetNameLastLeft()}[shape=record, label=\"{GetContentElementOperation(LabelLeft, left, currentStreamTransformation.PartCountBit())}\"];");
            _writer($"{GetNameLastRight()} -> {GetNameLastLeft()};");
            _lastIndexRight = GetUnique();
            _writer($"{GetNameLastRight()}[shape=record, label=\"{GetContentElementOperation(LabelRight, right, currentStreamTransformation.PartCountBit())}\"];");
            _writer($"{rightName} -> {GetNameLastRight()};");
            _writer($"{oldNameLeft} -> {GetNameLastRight()};");
            _writer("}");
        }
        public override void StartTranslation(IStreamTransformation currentStreamTransformation) => _writer("digraph G {");
        public override void EndTranslation(IStreamTransformation currentStreamTransformation) => _writer("}");
        public void AddOperation(OperationFunctionInfo currentStreamTransformation) => _functionOperations.Add(currentStreamTransformation);
        public override void StartBlockCorrect(IStreamTransformation currentStreamTransformation, BigInteger blockInput, BigInteger blockOutput) => originalInputBlock = blockInput;
        public override void EndBlockCorrect(IStreamTransformation currentStreamTransformation, BigInteger blockInput, BigInteger blockOutput) => originalOutputBlock = blockInput;

        public struct OperationFunctionInfo
        {
            public string Name { get; private set; }
            public BigInteger Result { get; private set; }
            public int[] ConnectToOperations { get; private set; }
            public ConnectType ConnectType { get; private set; }
            public long CountBit { get; private set; }

            public OperationFunctionInfo(string name, BigInteger result, ConnectType connectType, long countBit, params int[] connectTiOperations)
            {
                Name=name;
                Result=result;
                ConnectToOperations=connectTiOperations;
                ConnectType=connectType;
                CountBit=countBit;
            }
        }
        [Flags]
        public enum ConnectType : byte
        {
            None = 1,
            Left = 2,
            Right = 4,
            Key = 8
        }
    }
}
