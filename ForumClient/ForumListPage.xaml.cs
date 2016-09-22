using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ForumListPage : ContentPage
    {
        class ForumMenuItem : MenuItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SubID { get; set; }
        }

        private Api.Client Client;
        public ForumListPage(Api.Client client)
        {
            Client = client;

            InitializeComponent();

            forumList.RefreshCommand = new Command(Fetch);

            Fetch();
        }

        private async void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ForumMenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;

                var page = new ThreadListPage(Client, item.SubID);
                var navPage = (Application.Current as App).RootPage;
                await navPage.Navigation.PushAsync(page);
                page.Fetch();
            }
        }

        private bool IsLoading = false;

        private void Update(List<Api.Forum> list)
        {
            var start = DateTime.UtcNow;

            var forumData = new ObservableCollection<ForumMenuItem>();
            foreach (var item in list)
            {
                forumData.Add(new ForumMenuItem() { Title = item.Name, Description = item.Desc, SubID = item.Id });
            }

            start = DateTime.UtcNow;
            forumList.ItemsSource = forumData;
            Console.WriteLine("UpdateForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
        }

        public async void Fetch()
        {
            if (IsLoading) return;

            IsLoading = true;
            if(!forumList.IsRefreshing)
                forumList.BeginRefresh();

            var list = await Client.GetForumList();
            if (list != null)
            {
                Update(list);
            }
            else
            {
                forumList.ItemsSource = new ObservableCollection<ForumMenuItem>();
            }

            forumList.EndRefresh();
            IsLoading = false;

            if (list == null)
            {
                await DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
        }
    }
}
