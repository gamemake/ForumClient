using Xamarin.Forms;

namespace ForumClient
{
    public partial class App : Application
    {
        public App()
        {
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
