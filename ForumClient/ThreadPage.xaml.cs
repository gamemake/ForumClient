using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ThreadPage : ContentPage
    {
        ObservableCollection<MenuItem> postListData;

        public ThreadPage()
        {
            InitializeComponent();
            postListData = new ObservableCollection<MenuItem>();
            postList.ItemsSource = postListData;
        }

        public async void Update(string tid)
        {
            var c = (Application.Current as App).client;
            var list = await c.GetThread(tid, 1);

            postListData.Clear();
            foreach (var item in list)
            {
                postListData.Add(new MenuItem() { Title = item.Author.Name + ":" + item.PostTime });
            }
        }
    }
}

