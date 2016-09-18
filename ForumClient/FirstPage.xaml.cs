using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class FirstPage : ContentPage
    {
        class ForumMenuItem : MenuItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SubID { get; set; }
        }

        public FirstPage()
        {
            InitializeComponent();
            forumList.RefreshCommand = new Command(PullToRefresh);
        }

        async void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ForumMenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;

                var navPage = Parent as NavigationPage;
                var page = new ForumPage();
                await navPage.Navigation.PushAsync(page);
                page.Fetch(item.SubID, 1);
            }
        }

        public async void Fetch()
        {
            var c = (Application.Current as App).client;
            forumList.BeginRefresh();

            var start = DateTime.UtcNow;
            var list = await c.GetForumList();
            Console.WriteLine("GetForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            var forumListData = new ObservableCollection<ForumMenuItem>();
            foreach (var item in list)
            {
                forumListData.Add(new ForumMenuItem() { Title = item.Name, Description = item.Desc, SubID = item.Id });
            }

            start = DateTime.UtcNow;
            forumList.ItemsSource = forumListData;
            Console.WriteLine("UpdateForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            forumList.EndRefresh();
        }

        public async void PullToRefresh()
        {
            var c = (Application.Current as App).client;

            var start = DateTime.UtcNow;
            var list = await c.GetForumList();
            Console.WriteLine("GetForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            var forumListData = new ObservableCollection<ForumMenuItem>();
            foreach (var item in list)
            {
                forumListData.Add(new ForumMenuItem() { Title = item.Name, Description = item.Desc, SubID = item.Id });
            }

            start = DateTime.UtcNow;
            forumList.ItemsSource = forumListData;
            Console.WriteLine("UpdateForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            forumList.EndRefresh();
        }
    }
}
