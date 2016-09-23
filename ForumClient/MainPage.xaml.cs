using System.Collections.Generic;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            InitializeComponent();

            masterPage.ListView.ItemSelected += OnItemSelected;

            if (Device.OS == TargetPlatform.Windows)
            {
                Master.Icon = "swap.png";
            }

            var masterPageItems = new List<MasterPageItem>();
            foreach (var item in (Application.Current as App).Configs)
            {
                masterPageItems.Add(new MasterPageItem
                {
                    IconSource = "hamburger.png",
                    Title = item.Value.name,
                    Config = item.Value
                });
            }

            masterPage.ListView.ItemsSource = masterPageItems;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                var client = new Api.Client(item.Name, item.Config);
                if (client.IsAuthed())
                {
                    (Application.Current as App).ShowForumListPage(client);
                }
                else
                {
                    (Application.Current as App).ShowAccountPage(client);
                }

                masterPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}
