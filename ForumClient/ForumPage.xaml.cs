using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ForumPage : ContentPage
    {
        public ForumPage()
        {
            InitializeComponent();
        }

        async void OnClick(object sender, EventArgs e)
        {
            var navPage = Parent as NavigationPage;
            await navPage.Navigation.PushAsync(new ThreadPage());
        }
    }
}

