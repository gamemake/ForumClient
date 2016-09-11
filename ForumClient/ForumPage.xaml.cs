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

        void OnThreadSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }
}

