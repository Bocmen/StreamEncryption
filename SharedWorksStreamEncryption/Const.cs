using System.Text;

namespace SharedWorksStreamEncryption
{
    public static class Const
    {
        public const string NetworkFeistelNameGroup = "Сеть Фейстеля";
        public const string DESNameGroup = "DES";
        public readonly static Encoding Encoding;

        static Const()
        {
            try
            {
                Encoding = Encoding.GetEncoding(1251);
            }
            catch
            {
                Encoding = Encoding.Default;
            }
        }
    }
}
