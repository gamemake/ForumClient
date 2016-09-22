using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace ForumClient
{
    public partial class PostListPage : ContentPage
    {
        private System.Threading.Semaphore sem = new System.Threading.Semaphore(0, 2);
        private int sleep_time = 0;
        private bool quit_flag = false;

        private Api.Thread threadInfo;
        private int currentPage = 0;
        private bool IsLoading = false;
        private int totalNode = 0;
        private int currentNode = 0;
        private bool lastPage = false;
        private Api.Client Client;

        public PostListPage(Api.Client client, Api.Thread info)
        {
            Client = client;
            threadInfo = info;

            InitializeComponent();

            scrollView.Scrolled += OnScrolled;
            content.Children.Add(new Label()
            {
                Text = threadInfo.Title,
                FontAttributes = FontAttributes.Bold
            });

            Fetch();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            quit_flag = true;
        }

        public async void Fetch()
        {
            if (IsLoading) return;
            IsLoading = true;

            var list = await Client.GetThread(threadInfo.Id, currentPage + 1, () => { lastPage = true; });
            if (list != null)
            {
                currentPage += 1;

                totalNode = 0;
                currentNode = 0;
                foreach (var post in list)
                {
                    totalNode += post.Content.Count;
                }
                progressBar.Progress = 0;

                System.Threading.Tasks.Task.Run(() =>
                {
                    UpdateInternal(list);
                    IsLoading = false;
                });
            }
            else
            {
                IsLoading = false;
                await DisplayAlert("提示错误", "数据刷新失败", "确定");
            }
        }

        private void WaitSignal()
        {
            sem.WaitOne();
            // Console.WriteLine("execute time {0}", sleep_time);

            sleep_time = sleep_time / 3;

            if (sleep_time > 0)
            {
                System.Threading.Thread.Sleep(sleep_time);
            }
        }

        private async System.Threading.Tasks.Task<ImageSource> GetImageSource(string url)
        {
#if false
            var data = await App.GetClient().GetRawData(url, true);
            return ImageSource.FromStream(() => new System.IO.MemoryStream(data));
#else
            System.Uri uri;
            System.Uri.TryCreate(url, UriKind.Absolute, out uri);
            return await System.Threading.Tasks.Task<ImageSource>.Factory.StartNew(() => ImageSource.FromUri(uri));
#endif
        }

        private void BeginPost(Api.Post post)
        {
            content.Children.Add(new BoxView()
            {
                HeightRequest = 5,
                Color = Color.Silver
            });
        }

        private void UpdateNode(Api.PostNode node)
        {
            var start = System.DateTime.UtcNow;

            if (node.NodeType == "text")
            {
                content.Children.Add(new Label()
                {
                    Text = node.Text
                });
            }
            else if (node.NodeType == "link")
            {
                var link = new Label() { Text = node.Text, LineBreakMode = LineBreakMode.TailTruncation, TextColor = Color.Blue, HorizontalOptions = LayoutOptions.Start };
                link.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => { OpenLink(node.HRef); }) });
                content.Children.Add(link);
            }
            else if (node.NodeType == "image")
            {
                var image = new Image();
                content.Children.Add(image);
                System.Threading.Tasks.Task.Run(async () =>
                {
                    var source = await GetImageSource(node.HRef);
                    if (source != null)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            image.Source = source;
                        });
                    }
                });
            }

            currentNode += 1;
            if (currentPage < totalNode)
            {
                progressBar.Progress = (double)currentNode / (double)totalNode;
            }
            else
            {
                progressBar.Progress = 0;
            }

            sleep_time = (int)((System.DateTime.UtcNow - start).Ticks / System.TimeSpan.TicksPerMillisecond);
            sem.Release(1);
        }

        private void EndPost(Api.Post post)
        {
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
                        new Label() { Text = post.PostAuthor, TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.StartAndExpand },
                        new Label() { Text = post.PostTime,   TextColor=Color.Gray, FontSize = 10, HorizontalOptions=LayoutOptions.End },
                        refen, reply
                    }
            });
        }

        private void UpdateInternal(List<Api.Post> list)
        {
            foreach (var item in list)
            {
                if (quit_flag) return;

                var post = item;
                Device.BeginInvokeOnMainThread(() =>
                {
                    BeginPost(post);
                });

                foreach (var node in item.Content)
                {
                    System.Threading.Thread.Sleep(10);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        UpdateNode(node);
                    });
                    WaitSignal();
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    EndPost(post);
                });
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

        void OnScrolled(object Sender, ScrolledEventArgs e)
        {
            if (scrollView.ScrollY + scrollView.Height * 1.5 > scrollView.ContentSize.Height)
            {
                if (!lastPage)
                {
                    Fetch();
                }
            }
        }
    }
}
