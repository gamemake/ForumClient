using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        public App()
        {
#if __IOS__
            string config_file = System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "json/hipda.json");
            var config = new Api.Config();
            config.LoadFromFile(config_file);
            client = new ForumClient.Api.Client("hipda", config);
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

#if __IOS__
        public Api.Client client;
#endif
#if __ANDROID__
#endif

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
