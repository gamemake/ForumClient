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
            public string Title { get; set; }
            public string SubID { get; set; }
            public string PageInfo { get; set; }
            public string PostInfo { get; set; }
            public string LastPostInfo { get; set; }
        }
        ObservableCollection<ThreadMenuItem> threadListData;

        public ForumPage()
        {
            InitializeComponent();
            threadListData = new ObservableCollection<ThreadMenuItem>();
            threadList.ItemsSource = threadListData;
        }

        public async void Update(string forumId)
        {
            var c = (Application.Current as App).client;
            var list = await c.GetForum(forumId, 1);

            threadListData.Clear();
            foreach (var item in list)
            {
                string postInfo = item.Author.Name + " " + item.PostTime;
                string lastInfo = "";
                if (item.Last_Author != null)
                {
                    lastInfo = item.Last_Author.Name + " " + item.Last_PostTime;
                }
                threadListData.Add(new ThreadMenuItem() { Title = item.Title, SubID = item.Id, PageInfo="10/10", PostInfo = postInfo, LastPostInfo = lastInfo });
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
    }
}

