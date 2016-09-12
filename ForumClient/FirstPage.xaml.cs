using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class FirstPage : ContentPage
    {
        class ForumMenuItem : MenuItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SubID { get; set; }
        }
        ObservableCollection<ForumMenuItem> forumListData;

        public async void Fech()
        {
            var c = (Application.Current as App).client;
            var list = await c.GetForumList();
            foreach (var item in list)
            {
                forumListData.Add( new ForumMenuItem() { Title = item.Name, Description = item.Desc, SubID = item.Id, } );
            }
        }

        public FirstPage()
        {
            InitializeComponent();

            forumListData = new ObservableCollection<ForumMenuItem>();
            forumList.ItemsSource = forumListData;
        }

        async void OnForumSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as ForumMenuItem;
            if (item != null)
            {
                forumList.SelectedItem = null;

                var navPage = Parent as NavigationPage;
                var page = new ForumPage();
                await navPage.Navigation.PushAsync(page);
                page.Update(item.SubID);
            }
        }
    }
}
