using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ThreadListPage : ContentPage
    {
        private class ThreadMenuItem : MenuItem
        {
            public Api.Thread Data;
            public ThreadMenuItem(Api.Thread Data)
            {
                this.Data = Data;
            }

            public string Title
            {
                get
                {
                    if (Data.OnTop)
                    {
                        return "[OnTop] " + Data.Title;
                    }
                    else
                    {
                        return Data.Title;
                    }
                }
            }
            public string SubID
            {
                get
                {
                    return Data.Id;
                }
            }
            public string PostInfo
            {
                get
                {
                    return Data.PostAuthor + " " + Data.PostTime;
                }
            }
            public string LastPostInfo
            {
                get
                {
                    return Data.LastAuthor + " " + Data.LastTime;
                }
            }
        }

        private ObservableCollection<ThreadMenuItem> threadListData;
        private string currentForumId;
        private int currentPage = 0;
        private HashSet<string> tidSet = new HashSet<string>();
        private bool IsLoading = false;
        private Api.Client Client;

        public ThreadListPage(Api.Client client, string forumId)
        {
            Client = client;
            currentForumId = forumId;
            currentPage = 0;

            InitializeComponent();

            threadListData = new ObservableCollection<ThreadMenuItem>();
            threadList.ItemsSource = threadListData;
            threadList.RefreshCommand = new Command(PullToRefresh);

            Fetch();
        }

        private void Update(List<Api.Thread> list, bool refresh)
        {
            var start = DateTime.UtcNow;

            foreach (var item in list)
            {
                var tid = item.OnTop ? "OnTop" + item.Id : item.Id;
                if (!tidSet.Contains(tid))
                {
                    tidSet.Add(item.Id);
                    threadListData.Add(new ThreadMenuItem(item));
                }
            }

            if (refresh)
            {
                threadList.ItemsSource = threadListData;
            }

            Console.WriteLine("UpdateThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
        }

        public async void Fetch(bool refresh=false)
        {
            if (IsLoading) return;

            IsLoading = true;
            if(!threadList.IsRefreshing && currentPage > 0)
                threadList.BeginRefresh();

            if (refresh)
            {
                threadListData = new ObservableCollection<ThreadMenuItem>();
                currentPage = 0;
                tidSet.Clear();
            }

            var list = await Client.GetForum(currentForumId, currentPage + 1);
            if (list != null)
            {
                currentPage += 1;
                Update(list, refresh);
            }

            if(threadList.IsRefreshing)
                threadList.EndRefresh();
            IsLoading = false;

            if (list == null)
            {
                await DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
        }

        private void PullToRefresh()
        {
            Fetch(true);
        }

        private async void OnThreadSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ThreadMenuItem;
            if (item != null)
            {
                threadList.SelectedItem = null;

                var navPage = (Application.Current as App).RootPage;
                var page = new PostListPage(Client, item.Data);
                await navPage.Navigation.PushAsync(page);
                page.Fetch();
            }
        }


        private void OnItemAppearing(object Sender, ItemVisibilityEventArgs e)
        {
            var t = e.Item as ThreadMenuItem;
            if (threadListData[threadListData.Count - 1].Id == t.Id)
            {
                Fetch();
            }
        }
    }
}

