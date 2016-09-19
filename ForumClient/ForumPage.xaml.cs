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

        ObservableCollection<ThreadMenuItem> threadListData;
        string currentForumId;
        HashSet<string> tidSet = new HashSet<string>();
        bool IsLoading = false;
        int MaxPageIndex = 0;

        public ForumPage()
        {
            InitializeComponent();
            threadListData = new ObservableCollection<ThreadMenuItem>();
            threadList.ItemsSource = threadListData;
            threadList.RefreshCommand = new Command(PullToRefresh);
        }

        public async void Fetch(string forumId, int page)
        {
            if (IsLoading) return;
            IsLoading = true;
            threadList.BeginRefresh();

            var c = (Application.Current as App).client;
            if (page < 1) page = 1;

            var start = DateTime.UtcNow;
            var list = await c.GetForum(forumId, page);
            Console.WriteLine("GetThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            if (list == null)
            {
                await DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
            else
            {
                MaxPageIndex = page;
                currentForumId = forumId;

                if (page == 1)
                {
                    threadListData = new ObservableCollection<ThreadMenuItem>();
                    tidSet.Clear();
                }

                foreach (var item in list)
                {
                    var tid = item.OnTop ? "OnTop" + item.Id : item.Id;
                    if (!tidSet.Contains(tid))
                    {
                        tidSet.Add(item.Id);
                        threadListData.Add(new ThreadMenuItem(item));
                    }
                }

                if (page == 1)
                {
                    start = DateTime.UtcNow;
                    threadList.ItemsSource = threadListData;
                    Console.WriteLine("UpdateThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                }
            }

            threadList.EndRefresh();
            IsLoading = false;
        }

        async void OnThreadSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ThreadMenuItem;
            if (item != null)
            {
                threadList.SelectedItem = null;

                var navPage = Parent as NavigationPage;
                var page = new ThreadPage();
                await navPage.Navigation.PushAsync(page);
                page.Update(item.Data);
            }
        }

        async void PullToRefresh()
        {
            if (IsLoading)
            {
                threadList.EndRefresh();
                return;
            }

            IsLoading = true;
            var c = (Application.Current as App).client;

            var start = DateTime.UtcNow;
            var list = await c.GetForum(currentForumId, 1);
            Console.WriteLine("GetThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            threadListData = new ObservableCollection<ThreadMenuItem>();
            tidSet.Clear();

            if (list == null)
            {
                await DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
            else
            {
                foreach (var item in list)
                {
                    var tid = item.OnTop ? "OnTop" + item.Id : item.Id;
                    if (!tidSet.Contains(tid))
                    {
                        tidSet.Add(item.Id);
                        threadListData.Add(new ThreadMenuItem(item));
                    }
                }
            }

            start = DateTime.UtcNow;
            threadList.ItemsSource = threadListData;
            Console.WriteLine("UpdateThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
            IsLoading = false;
        }

        void OnItemAppearing(object Sender, ItemVisibilityEventArgs e)
        {
            var t = e.Item as ThreadMenuItem;
            if (threadListData[threadListData.Count - 1].Id == t.Id)
            {
                Fetch(currentForumId, MaxPageIndex + 1);
            }
        }
    }
}

