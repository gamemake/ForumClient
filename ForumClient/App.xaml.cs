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
            var config = new Api.Config();
            string config_name = "1024";

#if __IOS__
            string config_file = System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "json/" + config_name +".json");
            config.LoadFromFile(config_file);
#elif __ANDROID__
            using (var stream = Android.App.Application.Context.ApplicationContext.Assets.Open("json/" + config_name +".json"))
            {
                config.LoadFromText(new System.IO.StreamReader(stream).ReadToEnd());
            }
#endif

            client = new ForumClient.Api.Client(config_name, config);

            InitializeComponent();

            if (client.IsAuthed())
            {
                var page = new FirstPage();
                MainPage = new NavigationPage(page);
                page.Fetch();
            }
            else
            {
                MainPage = new NavigationPage(new AccountPage());
            }
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
