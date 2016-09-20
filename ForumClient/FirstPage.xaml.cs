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
            forumList.RefreshCommand = new Command(Fetch);
        }

        private async void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ForumMenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;

                var navPage = Parent as NavigationPage;
                var page = new ForumPage(item.SubID);
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

            var start = DateTime.UtcNow;
            var list = await App.GetClient().GetForumList();
            if (list != null)
            {
                Console.WriteLine("GetForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
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
