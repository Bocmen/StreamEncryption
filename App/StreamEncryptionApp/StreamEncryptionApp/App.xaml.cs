using System.Reflection;
using System.Text;
using WorkInvoker.Pages;
using Xamarin.Forms;

namespace StreamEncryptionApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            WorkInvoker.WorksLoader.AppendWorks(Assembly.GetAssembly(typeof(SharedWorksStreamEncryption.Const)));
            SettingPage.ApplayThemes();
            MainPage = WorkInvoker.Pages.MainPage.CreateRootPage(new DefaultViewWorks()
            {
                Title = DefaultViewWorks.DefaultTitlePage
            }, new SettingPage()
            {
                Title = SettingPage.DefaultTitlePage
            });
        }
    }
}
