using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class ThreadPage : ContentPage
    {
        private System.Threading.Semaphore sem = new System.Threading.Semaphore(0, 2);
        private int sleep_time = 0;
        private bool quit_flag = false;
        
        public ThreadPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            quit_flag = true;
        }

        public void Update(Api.Thread tinfo)
        {
            content.Children.Add(new Label()
            {
                Text = tinfo.Title,
                FontAttributes = FontAttributes.Bold
            });
            
            System.Threading.Tasks.Task.Run(() =>
            {
                UpdateInternal(tinfo.Id);
            });
        }

        private void WaitSignal()
        {
            sem.WaitOne();
            if (sleep_time > 0)
            {
                System.Threading.Thread.Sleep(sleep_time);
            }
            // Console.WriteLine("execute time {0}", sleep_time);
        }

        private void OnSplitter()
        {
            var start = System.DateTime.UtcNow;
            content.Children.Add(new BoxView()
            {
                HeightRequest = 5,
                Color = Color.Silver
            });
            sleep_time = (int)((System.DateTime.UtcNow - start).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        private void OnText(string text)
        {
            var start = System.DateTime.UtcNow;
            content.Children.Add(new Label()
            {
                Text = text
            });
            sleep_time = (int)((System.DateTime.UtcNow - start).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        private void OnLink(string text, string url)
        {
            var start = System.DateTime.UtcNow;
            var link = new Label() { Text = text, LineBreakMode = LineBreakMode.TailTruncation, TextColor = Color.Blue, HorizontalOptions = LayoutOptions.Start };
            link.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { OpenLink(url); }) });
            content.Children.Add(link);
            sleep_time = (int)((start - System.DateTime.UtcNow).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        private void OnImage(string url)
        {
            var start = System.DateTime.UtcNow;
            var image = new Image();
            content.Children.Add(image);
            System.Threading.Tasks.Task.Run(async () =>
            {
                var client = (Application.Current as App).client;
                var data = await client.GetRawData(url, true);
                if(data!=null)
                {
                    var source = ImageSource.FromStream(() => new System.IO.MemoryStream(data));
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        image.Source = source;
                    });
                }
                /*
                                System.Uri uri;
                                System.Uri.TryCreate(url, UriKind.Absolute, out uri);

                                var result = System.Threading.Tasks.Task<ImageSource>.Factory.StartNew(() => ImageSource.FromUri(uri));
                                var source = await result;
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    image.Source = source;
                                });
                */
            });
            sleep_time = (int)((System.DateTime.UtcNow - start).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        private void OnPostInfo(Api.Post post)
        {
            var start = System.DateTime.UtcNow;
            var refen = new Label() { Text = " 引用 ", TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.End };
            refen.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { PostReference(post.Id, "aa"); }) });
            var reply = new Label() { Text = " 回复 ", TextColor = Color.Blue, FontSize = 10, HorizontalOptions = LayoutOptions.End };
            reply.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { PostReply(post.Id, "aa"); }) });
            content.Children.Add(new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 0,
                Children =
                    {
                        new Label() { Text = post.Author.Name, TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.StartAndExpand },
                        new Label() { Text = post.PostTime,   TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.End },
                        refen, reply
                    }
            });
            sleep_time = (int)((System.DateTime.UtcNow - start).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        public async void UpdateInternal(string tid)
        {
            try
            {
                var c = (Application.Current as App).client;
                var list = await c.GetThread(tid, 1);

                if (list == null)
                {
                    Device.BeginInvokeOnMainThread( async () =>
                    {
                        await DisplayAlert("提示错误", "数据刷新失败", "确定");
                    });
                }

                foreach (var item in list)
                {
                    if (quit_flag) return;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.OnSplitter();
                    });
                    sem.WaitOne();

                    foreach (var node in item.Nodes)
                    {
                        await System.Threading.Tasks.Task.Delay(10);

                        if (node.NodeType == "text")
                        {
                            var text = node.Text;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                this.OnText(text);
                            });
                            WaitSignal();
                        }
                        if (node.NodeType == "link")
                        {
                            var url = node.HRef;
                            var text = node.Text;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                this.OnLink(text, url);
                            });
                            WaitSignal();
                        }
                        if (node.NodeType == "image")
                        {
                            var url = node.HRef;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                this.OnImage(url);
                            });
                            WaitSignal();
                        }
                    }

                    var post = item;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.OnPostInfo(post);
                    });
                    WaitSignal();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
            }
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

