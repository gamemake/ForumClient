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

            var app = (Application.Current as App);
            foreach (var item in app.Configs)
            {
                var name = item.Key;
                var button = new Button();
                button.Text = item.Value.name;
                button.Clicked += (sender, e) =>
                {
                    ChangeForum(name);
                };
                menuList.Children.Add(button);
            }
        }

        void OnHomePage(object sender, EventArgs e)
        {
            (Application.Current as App).BasePage.IsPresented = false;
            (Application.Current as App).DetailPage = new HomePage();
        }

        void ChangeForum(string name)
        {
            (Application.Current as App).BasePage.IsPresented = false;

            var config = (Application.Current as App).Configs[name];
            var client = new Api.Client(name, config);
            if (client.IsAuthed())
            {
                (Application.Current as App).DetailPage = new ForumListPage(client);
            }
            else
            {
                (Application.Current as App).DetailPage = new AccountPage(client);
            }
        }
    }
}
