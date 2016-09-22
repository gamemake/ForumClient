using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        void OnChangeForum(object sender, EventArgs e)
        {
            var button = sender as Button;

            var client = Api.Client.CreateClient(button.Text.ToLower());
            (Application.Current as App).DetailPage = new ForumListPage(client);
            (Application.Current as App).BasePage.IsPresented = false;
        }
    }
}
