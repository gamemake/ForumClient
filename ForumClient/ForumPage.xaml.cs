using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ForumPage : ContentPage
    {
        ObservableCollection<MenuItem> threadListData;

        public ForumPage()
        {
            InitializeComponent();
            threadListData = new ObservableCollection<MenuItem>();
            threadList.ItemsSource = threadListData;
        }

        public async void Update(string forumId)
        {
            var c = (Application.Current as App).client;
            var list = await c.GetForum(forumId, 1);

            threadListData.Clear();
            foreach (var item in list)
            {
                threadListData.Add( new MenuItem() { Title = item.Title, SubID = item.Id } );
            }
        }

        async void OnThreadSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            if (item != null)
            {
                var navPage = Parent as NavigationPage;
                var page = new ThreadPage();
                await navPage.Navigation.PushAsync(page);
                page.Update(item.SubID);
            }
        }
    }
}

