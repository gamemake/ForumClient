﻿using System;
using Xamarin.Forms;

namespace ForumClient
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
            UsernameEntry.Keyboard = Keyboard.Create(0);
        }

        async void OnSignUp(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }

        async void SignIn(string name)
        {
            var client = Api.Client.CreateClient(name);

            if (!client.IsAuthed())
            {
                if (!string.IsNullOrEmpty(UsernameEntry.Text) && !string.IsNullOrEmpty(PasswordEntry.Text))
                {
                    var result = await client.SignIn(UsernameEntry.Text, PasswordEntry.Text);
                    if (result != "")
                    {
                        MessageLabel.Text = result;
                        return;
                    }

                    client.SaveCookies();
                }
            }

            var page = new ForumListPage(client);
#if __ANDROID__
            Application.Current.MainPage = new NavigationPage(page);
#else
            var navPage = Parent as NavigationPage;
            navPage.Navigation.InsertPageBefore(page, this);
            await navPage.Navigation.PopAsync();
#endif
            page.Fetch();
        }

        void OnSignIn_1024(object sender, EventArgs e)
        {
            SignIn("1024");
        }

        void OnSignIn_HiPDA(object sender, EventArgs e)
        {
            SignIn("hipda");
        }

        async void OnSettings(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }
    }
}