using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        public Dictionary<string, Api.Config> Configs;
        private MainPage mainPage;

        public App()
        {
            LoadConfigs();

            InitializeComponent();

            mainPage = new MainPage();
            MainPage = mainPage;
        }

        public void ShowAccountPage(Api.Client client)
        {
            mainPage.Detail = new NavigationPage(new AccountPage(client));
        }

        public void ShowForumListPage(Api.Client client)
        {
            mainPage.Detail = new NavigationPage(new ForumListPage(client));
        }

        public void ShowThreadListPage(Api.Client client, string fid)
        {
            (mainPage.Detail as NavigationPage).PushAsync(new ThreadListPage(client, fid));
        }

        public void ShowPostListPage(Api.Client client, Api.Thread tinfo)
        {
            (mainPage.Detail as NavigationPage).PushAsync(new PostListPage(client, tinfo));
        }

        private void LoadConfigs()
        {
            Configs = new Dictionary<string, Api.Config>();
#if __ANDROID__
            var files = Android.App.Application.Context.ApplicationContext.Assets.List("config");
#elif __IOS__
            Foundation.NSError err;
            var files = Foundation.NSFileManager.DefaultManager.GetDirectoryContent(System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "config"), out err);
#endif

            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.EndsWith(".txt"))
                    {
                        var name = file.Substring(0, file.Length - 4);
                        var config = Api.Config.LoadConfig(name);
                        if (config != null)
                        {
                            Configs.Add(name, config);
                        }
                    }
                }
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
