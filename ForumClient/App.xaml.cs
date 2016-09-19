using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        public App()
        {
            var config = new Api.Config();
#if __IOS__
            string config_file = System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "json/hipda.json");
            config.LoadFromFile(config_file);
            client = new ForumClient.Api.Client("hipda", config);
#elif __ANDROID__
            using (var stream = Android.App.Application.Context.ApplicationContext.Assets.Open("json/hipda.json"))
            {
                config.LoadFromStream(new System.IO.StreamReader(stream));
            }
#endif

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

        public Api.Client client;

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
