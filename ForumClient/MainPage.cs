using Xamarin.Forms;

namespace ForumClient
{
    public class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            Master = new MainMenuPage();
            Detail = new HomePage();
        }
    }
}
