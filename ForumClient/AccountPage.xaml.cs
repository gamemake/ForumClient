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
        
        async void OnSignIn(object sender, EventArgs e)
        {
            var c = (Application.Current as App).client;
            var result = await c.SignIn(UsernameEntry.Text, PasswordEntry.Text);
            if (result)
            {
                var cookie = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Cookies.bin");
                c.SaveCookies(cookie);


                var page = new FirstPage();
                var navPage = Parent as NavigationPage;
                navPage.Navigation.InsertPageBefore(page, this);
                await navPage.Navigation.PopAsync();
                page.Fech();
            }
            else
            {
                MessageLabel.Text = "error";
            }
            
        }

        async void OnSettings(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }
    }
}