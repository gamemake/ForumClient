using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        public NavigationPage RootPage
        {
            get { return MainPage as NavigationPage; }
        }

        public Page MasterPage
        {
            get { return BasePage.Master; }
            set { BasePage.Master = value; }
        }

        public Page DetailPage
        {
            get { return BasePage.Detail; }
            set { BasePage.Detail = value; }
        }

        private MasterDetailPage _BasePage;
        public MasterDetailPage BasePage
        {
            get { return _BasePage; }
        }

        public App()
        {
            InitializeComponent();
            _BasePage = new MasterDetailPage()
            {
                Master = new MainMenuPage(),
                Detail = new HomePage()
            };
            MainPage = new NavigationPage(BasePage);
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
