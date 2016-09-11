using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class FirstPage : ContentPage
    {
        ObservableCollection<MenuItem> forumListData;

        public async void Fech()
        {
            var c = (Application.Current as App).client;
            var list = await c.GetForumList();
            foreach (var item in list)
            {
                forumListData.Add(
                    new MenuItem()
                    {
                        Title = item.Name,
                        SubID = item.Id
                    }
                    );
            }
        }

        public FirstPage()
        {
            InitializeComponent();

            forumListData = new ObservableCollection<MenuItem>();
            forumList.ItemsSource = forumListData;
        }

        async void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;
                var navPage = Parent as NavigationPage;
                var page = new ForumPage();
                await navPage.Navigation.PushAsync(page);
                page.Update(item.SubID);
            }
        }
    }
}
