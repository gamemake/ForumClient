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

        public ForumPage()
        {
            InitializeComponent();
            threadListData = new ObservableCollection<ThreadMenuItem>();
            threadList.ItemsSource = threadListData;
            threadList.RefreshCommand = new Command(PullToRefresh);
        }

        public async void Fetch(string forumId, int page)
        {
            var c = (Application.Current as App).client;
            if (page < 1) page = 1;
            currentForumId = forumId;
            var list = await c.GetForum(forumId, page);

            if (page == 1)
            {
                threadListData = new ObservableCollection<ThreadMenuItem>();
            }

            foreach (var item in list)
            {
                threadListData.Add(new ThreadMenuItem(item));
            }

            if (page == 1)
            {
                threadList.ItemsSource = threadListData;
            }
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
                page.Update(item.SubID);
            }
        }

        async void PullToRefresh()
        {
            var c = (Application.Current as App).client;
            var list = await c.GetForum(currentForumId, 1);
            threadListData = new ObservableCollection<ThreadMenuItem>();
            foreach (var item in list)
            {
                threadListData.Add(new ThreadMenuItem(item));
            }
            threadList.ItemsSource = threadListData;
            threadList.EndRefresh();
        }
    }
}

