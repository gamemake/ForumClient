using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ForumPage : ContentPage
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
            public string PageInfo
            {
                get
                {
                    return "10/10";
                }
            }
            public string PostInfo
            {
                get
                {
                    return Data.Author.Name + " " + Data.PostTime;
                }
            }
            public string LastPostInfo
            {
                get
                {
                    if (Data.Last_Author != null)
                    {
                        return Data.Last_Author.Name + " " + Data.Last_PostTime;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        private ObservableCollection<ThreadMenuItem> threadListData;
        private string currentForumId;
        private int currentPage = 0;
        HashSet<string> tidSet = new HashSet<string>();
        bool IsLoading = false;

        public ForumPage(string forumId)
        {
            currentForumId = forumId;
            currentPage = 0;

            InitializeComponent();
            threadListData = new ObservableCollection<ThreadMenuItem>();
            threadList.ItemsSource = threadListData;
            threadList.RefreshCommand = new Command(PullToRefresh);
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

            var start = DateTime.UtcNow;
            var list = await App.GetClient().GetForum(currentForumId, currentPage + 1);
            if (list != null)
            {
                Console.WriteLine("GetThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
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

                var navPage = Parent as NavigationPage;
                var page = new ThreadPage(item.Data);
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

