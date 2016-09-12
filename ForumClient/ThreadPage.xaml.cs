using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ThreadPage : ContentPage
    {
        public ThreadPage()
        {
            InitializeComponent();
        }

        public async void Update(string tid)
        {
            var c = (Application.Current as App).client;
            var list = await c.GetThread(tid, 1);
            foreach (var item in list)
            {
                var cell = new PostCell(item);
                content.Children.Add(cell);
            }
        }

        void OnPostSelected(object sender, SelectedItemChangedEventArgs e)
        {
        }
    }
}

