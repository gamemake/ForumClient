using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        async void OnReturn(object sender, EventArgs e)
        {
            var Main = Parent as NavigationPage;
            await Main.Navigation.PopAsync();

        }
    }
}

