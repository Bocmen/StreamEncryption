using ConsoleLibrary.ConsoleExtensions;
using CoreStreamEncryption.Interface;
using SharedWorksStreamEncryption.Models;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreStreamEncryption.Extensions;

namespace SharedWorksStreamEncryption.Abstract
{
    public abstract class WorkTransformation: WorkInvoker.Abstract.WorkBase
    {
        private const string DefaultText = "12345678";
        private readonly static BigInteger[] Keys = Enumerable.Range(0, 32).Select(x => (BigInteger)x).ToArray();
        private readonly bool _isViewLogger;

        protected WorkTransformation(bool isViewLogger = false) => _isViewLogger=isViewLogger;

        protected abstract Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token);

        public override async Task Start(CancellationToken token)
        {
            string textInput = await Console.ReadLine("Введите текст для шифрования", token: token, defaultValue: DefaultText);
            BigInteger[] keys = await Console.ReadArrayBigInteger("Введите ключи", token: token, defaultsValue: Keys);

            DotLogger loggerIteration = null;
            StringBuilder stringBuilder = null;
            if (_isViewLogger && await Console.ReadBool("Хотите получить все логи?", token: token))
            {
                stringBuilder = new StringBuilder();
                loggerIteration = new DotLogger((v) => stringBuilder.Append(v));
            }
            IStreamTransformation streamTransformation = await CreateStreamTransformation(loggerIteration, token);
            var result = streamTransformation.Encryption(Const.Encoding.GetBytes(textInput), keys, loggerIteration).ToArray();
            if (stringBuilder != null)
                await Console.DrawWebViewDot(stringBuilder.ToString());
            await Console.ReadLine("Результат шифрования", token: token, defaultValue: Const.Encoding.GetString(result).Replace('\0', '?'));
            await Console.ReadLine("Результат дешифрования", token: token, defaultValue: Const.Encoding.GetString(streamTransformation.Decryption(result, keys).ToArray()));
        }
    }
}
