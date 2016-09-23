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
            threadList.RefreshCommand = new Command(PullToRefresh);

            FetchBegin();
        }

        public void FetchBegin(bool refresh=false)
        {
            if (IsLoading) return;

            IsLoading = true;

            if(!threadList.IsRefreshing && currentPage==0)
                threadList.BeginRefresh();

            if (refresh)
            {
                currentPage = 0;
                tidSet.Clear();
            }

            System.Threading.Tasks.Task.Run(() => Fetch());
        }

        public async void Fetch()
        {
            var list = await Client.GetForum(currentForumId, currentPage + 1);
            Device.BeginInvokeOnMainThread(() => FetchEnd(list));
        }

        public void FetchEnd(List<Api.Thread> list)
        {
            if (threadList.IsRefreshing)
                threadList.EndRefresh();
            IsLoading = false;

            if (list == null)
            {
                DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
            else
            {
                if (currentPage == 0)
                    threadListData = new ObservableCollection<ThreadMenuItem>();

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

                if (currentPage==0)
                {
                    threadList.ItemsSource = threadListData;
                }

                Console.WriteLine("UpdateThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                currentPage += 1;
            }
        }

        private void PullToRefresh()
        {
            FetchBegin(true);
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
            }
        }


        private void OnItemAppearing(object Sender, ItemVisibilityEventArgs e)
        {
            var t = e.Item as ThreadMenuItem;
            if (threadListData[threadListData.Count - 1].Id == t.Id)
            {
                FetchBegin();
            }
        }
    }
}

