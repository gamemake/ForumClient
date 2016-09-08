using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public string SubID { get; set; }
    }

    public partial class MainPage : MasterDetailPage
    {
        static public ObservableCollection<MenuItem> ItemsXXX { get; set; }
        
        public MainPage()
        {
            ItemsXXX = new ObservableCollection<MenuItem>();

            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Contacts",
                IconSource = "hamburger.png",
                SubID = "11"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "TodoList",
                IconSource = "hamburger.png",
                SubID = "12"
            });
            ItemsXXX.Add(new MenuItem
            {
                Title = "Reminders",
                IconSource = "hamburger.png",
                SubID = "13"
            });

            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
            menuList.ItemsSource = ItemsXXX;
            menuList.SelectedItem = ItemsXXX[0];
        }

        void OnSignOut(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new AccountPage());
        }

        async void OnSettings(object sender, EventArgs e)
        {
            IsPresented = false;
            await detailPage.Navigation.PushAsync(new SettingsPage());
        }

        void OnMenuSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            if (item != null)
            {
                IsPresented = false;
            }
        }
    }
}

