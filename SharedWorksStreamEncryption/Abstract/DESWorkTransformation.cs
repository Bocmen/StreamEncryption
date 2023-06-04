using SharedWorksStreamEncryption.Models;

namespace SharedWorksStreamEncryption.Abstract
{
    public abstract class DESWorkTransformation: WorkTransformation
    {
        protected override DotLogger GetLoggerIteration(DotLogger.Write writer) => new DotLogger(writer, "DES FUNCTION", DotLogger.ConnectType.Right | DotLogger.ConnectType.Key);
    }
}
