using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

namespace StreamEncryptionApp.Droid
{
    [Activity(Label = "StreamEncryptionApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            ConsoleLibrary.Platform.Droid.InitLabrary.Init(this, savedInstanceState);

            LoadApplication(new App());
        }
        public override void OnBackPressed() => ConsoleLibrary.Platform.Droid.InitLabrary.SendBackPressed(base.OnBackPressed);
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}