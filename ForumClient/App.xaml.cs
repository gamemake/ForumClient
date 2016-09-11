using Xamarin.Forms;

namespace ForumClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var cookie = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Cookies.bin");
            if (System.IO.File.Exists(cookie))
            {
                var c = (Application.Current as App).client;
                c.LoadCookies(cookie);

                var page = new FirstPage();
                MainPage = new NavigationPage(page);
                page.Fech();
            }
            else
            {
                MainPage = new NavigationPage(new AccountPage());
            }
        }

        public Api.Client client = new Api.Client("http://www.hi-pda.com/");

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
