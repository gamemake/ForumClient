using System.Collections.Generic;
using Xamarin.Forms;

namespace ForumClient
{
    public class MasterPageItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public string Name { get; set; }
        public Api.Config Config { get; set; }
    }

    public partial class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }

        public MasterPage()
        {
            InitializeComponent();

            var masterPageItems = new List<MasterPageItem>();
            foreach (var item in (Application.Current as App).Configs)
            {
                masterPageItems.Add(new MasterPageItem
                {
                    Name = item.Key,
                    IconSource = "hamburger.png",
                    Title = item.Value.name,
                    Config = item.Value
                });
            }

            listView.ItemsSource = masterPageItems;
        }
    }
}
