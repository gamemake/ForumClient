using System;
using Xamarin.Forms;

namespace ForumClient
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
        }

        async void OnSignUp(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }
        
        void OnSignIn(object sender, EventArgs e)
        {
            var page = new FirstPage();
            Application.Current.MainPage = new NavigationPage(page);
            page.Fech();
        }

        async void OnSettings(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }
    }
}