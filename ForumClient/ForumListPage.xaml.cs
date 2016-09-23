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

            FetchBegin();
        }

        private void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ForumMenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;

                (Application.Current as App).ShowThreadListPage(Client, item.SubID);
            }
        }

        private bool IsLoading = false;

        private void FetchBegin()
        {
            if (IsLoading) return;

            IsLoading = true;
            if(!forumList.IsRefreshing)
                forumList.BeginRefresh();

            System.Threading.Tasks.Task.Run(() => Fetch());
        }

        private async void Fetch()
        {
            var list = await Client.GetForumList();
            if (list == null)
            {
                Device.BeginInvokeOnMainThread(() => FetchEnd(null));
                return;
            }

            var items = new ObservableCollection<ForumMenuItem>();
            foreach (var item in list)
            {
                items.Add(new ForumMenuItem() { Title = item.Name, Description = item.Desc, SubID = item.Id });
            }
            Device.BeginInvokeOnMainThread(() => FetchEnd(items));
        }

        private void FetchEnd(ObservableCollection<ForumMenuItem> items)
        {
            forumList.EndRefresh();
            IsLoading = false;

            if (items == null)
            {
                DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
            else
            {
                var start = DateTime.UtcNow;
                forumList.ItemsSource = items;
                Console.WriteLine("UpdateForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
            }
        }
    }
}
