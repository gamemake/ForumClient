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

            var start = DateTime.UtcNow;
            var list = await c.GetThread(tid, 1);
            Console.WriteLine("GetPostList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);

            start = DateTime.UtcNow;
            bool first = true;
            content.BatchBegin();
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
                refen.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { PostReference(tid, "aa"); }) });
                var reply = new Label() { Text = " 回复 ", TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.End };
                reply.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { PostReply(tid, "aa"); }) });

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

                foreach (var node in item.Nodes)
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
                        var url = node.HRef;
                        var link = new Label() { Text = node.Text, LineBreakMode = LineBreakMode.TailTruncation, TextColor = Color.Blue, HorizontalOptions = LayoutOptions.Start };
                        link.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { OpenLink(url); }) });
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
            content.BatchCommit();
            Console.WriteLine("UpdatePostList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
        }

        public async void OpenLink(string url)
        {
            var answer = await DisplayAlert("是否打开此外部链接？", url, "确定", "取消");
            if (answer)
            {
                Xamarin.Forms.Device.OpenUri(new Uri(url));
            }
        }

        public async void PostReply(string tid, string pid)
        {
            await DisplayAlert("提示", "功能未实现", "确定");
        }

        public async void PostReference(string tid, string pid)
        {
            await DisplayAlert("提示", "功能未实现", "确定");
        }
    }
}

