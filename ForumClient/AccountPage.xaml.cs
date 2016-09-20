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

        async void OnSignIn(object sender, EventArgs e)
        {
            var result = await App.GetClient().SignIn(UsernameEntry.Text, PasswordEntry.Text);
            if (result == "")
            {
                App.GetClient().SaveCookies();
                OnAnonymousLogin(sender, e);
            }
            else
            {
                MessageLabel.Text = result;
            }
        }

        async void OnAnonymousLogin(object sender, EventArgs e)
        {
            var page = new FirstPage();
#if __ANDROID__
            Application.Current.MainPage = new NavigationPage(page);
#else
            var navPage = Parent as NavigationPage;
            navPage.Navigation.InsertPageBefore(page, this);
            await navPage.Navigation.PopAsync();
#endif
            page.Fetch();
        }

        async void OnSettings(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new SettingsPage());
        }
    }
}