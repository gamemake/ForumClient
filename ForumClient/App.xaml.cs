using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        private Api.Client client;

        public static Api.Client GetClient()
        {
            return (Application.Current as App).client;
        }

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AccountPage());
        }

        public void SetClient(string name)
        {
            var config = new Api.Config();
            string config_name = "1024";

#if __IOS__
            string config_file = System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "config/" + config_name +".txt");
            config.LoadFromText(System.IO.File.ReadAllText(config_file));
#elif __ANDROID__
            using (var stream = Android.App.Application.Context.ApplicationContext.Assets.Open("config/" + config_name + ".txt"))
            {
                config.LoadFromText(new System.IO.StreamReader(stream).ReadToEnd());
            }
#endif

            client = new ForumClient.Api.Client(config_name, config);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
