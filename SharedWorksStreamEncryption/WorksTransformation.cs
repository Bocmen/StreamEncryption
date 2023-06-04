using ConsoleLibrary.ConsoleExtensions;
using CoreStreamEncryption.Interface;
using CoreStreamEncryption.Models;
using SharedWorksStreamEncryption.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SharedWorksStreamEncryption
{
    [WorkInvoker.Attributes.LoaderWorkBase("Шифрование/Дешифрование", "", Const.NetworkFeistelNameGroup)]
    public class WorksNetworkFeistel : Abstract.WorkTransformation
    {
        protected override async Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token)
        {
            int countByte = await Console.ReadInt("Введите количество байт", token: token, defaultValue: 2);
            int countRound = await Console.ReadInt("Введите количество раундов", token: token, defaultValue: 8);
            return new NetworkFeistel(countByte, countRound, (currentSetting, right, key) =>
            {
                var resultOp = right ^ (key & currentSetting.PartMask);
                dotLogger?.AddOperation(new DotLogger.OperationFunctionInfo("A", resultOp, DotLogger.ConnectType.Key | DotLogger.ConnectType.Right, currentSetting.PartCountBit));
                resultOp = (resultOp << 1) | ((resultOp & currentSetting.PartMask) >> (currentSetting.PartCountBit - 1));
                dotLogger?.AddOperation(new DotLogger.OperationFunctionInfo("S", resultOp, DotLogger.ConnectType.None, currentSetting.PartCountBit, 0));
                return resultOp & currentSetting.PartMask;
            });
        }
    }

    [WorkInvoker.Attributes.LoaderWorkBase("DES ECB", "Режим простой замены", Const.DESNameGroup)]
    public class WorksDES_ECB : Abstract.DESWorkTransformation
    {
        protected override Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token) => Task.FromResult((IStreamTransformation)new DES(DES.ModeType.ECB));
    }

    [WorkInvoker.Attributes.LoaderWorkBase("DES CBC", "Режим сцепления блоков шифротекста", Const.DESNameGroup)]
    public class WorksDES_CBC : Abstract.DESWorkTransformation
    {
        protected override Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token) => Task.FromResult((IStreamTransformation)new DES(DES.ModeType.CBC));
    }

    [WorkInvoker.Attributes.LoaderWorkBase("DES CFB", "Режим обратной связи по шифротексту", Const.DESNameGroup)]
    public class WorksDES_CFB : Abstract.DESWorkTransformation
    {
        protected override Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token) => Task.FromResult((IStreamTransformation)new DES(DES.ModeType.CFB));
    }

    [WorkInvoker.Attributes.LoaderWorkBase("DES OFB", "Режим обратной связи по выходу", Const.DESNameGroup)]
    public class WorksDES_OFB : Abstract.DESWorkTransformation
    {
        protected override Task<IStreamTransformation> CreateStreamTransformation(DotLogger dotLogger, CancellationToken token) => Task.FromResult((IStreamTransformation)new DES(DES.ModeType.OFB));
    }
}
