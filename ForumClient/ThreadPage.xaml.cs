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
            bool first = true;
            foreach (var item in list)
            {
                if (!first)
                {
                    content.Children.Add(new BoxView()
                    {
                        HeightRequest = 5,
                        Color = Color.Silver
                    });
                }
                first = false;

                var refen = new Label() { Text = " 引用 ", TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.End };
                refen.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { }) });
                var reply = new Label() { Text = " 回复 ", TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.End };
                reply.GestureRecognizers.Add(new TapGestureRecognizer(){ Command = new Command(() => { }) });

                content.Children.Add(new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 0,
                    Children =
                    {
                        new Label() { Text = item.Author.Name, TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.StartAndExpand },
                        new Label() { Text = item.PostTime,   TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.End },
                        refen, reply
                    }
                });
                foreach(var node in item.Nodes)
                {
                    if (node.NodeType == "text")
                    {
                        content.Children.Add(new Label()
                        {
                            Text = node.Text
                        });
                    }
                    if (node.NodeType == "link")
                    {
                        var link = new Label() { Text = node.Text, LineBreakMode=LineBreakMode.TailTruncation, TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.Start };
                        link.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { }) });
                        content.Children.Add(link);
                    }
                    if (node.NodeType == "image")
                    {
                        content.Children.Add(new Image()
                        {
                            Source = new UriImageSource()
                            {
                                Uri = new Uri(node.HRef),
                                CachingEnabled = true,
                                CacheValidity = new System.TimeSpan(10, 0, 0)
                            }
                        });
                    }
                }
            }
        }
    }
}

