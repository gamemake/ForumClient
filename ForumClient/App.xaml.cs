using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ForumClient;

namespace ForumClient
{
    public partial class App : Application
    {
        public NavigationPage RootPage
        {
            get { return MainPage as NavigationPage; }
        }

        public Page MasterPage
        {
            get { return BasePage.Master; }
            set { BasePage.Master = value; }
        }

        public Page DetailPage
        {
            get { return BasePage.Detail; }
            set { BasePage.Detail = value; }
        }

        private MasterDetailPage _BasePage;
        public MasterDetailPage BasePage
        {
            get { return _BasePage; }
        }

        public Dictionary<string, Api.Config> Configs;

        public App()
        {
            LoadConfigs();

            InitializeComponent();

            _BasePage = new MainPage();
            MainPage = new NavigationPage(BasePage);
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
                        var config = LoadConfig(name);
                        if (config != null)
                        {
                            Configs.Add(name, config);
                        }
                    }
                }
            }
        }

        private Api.Config LoadConfig(string config_name)
        {
            try
            {
                var config = new Api.Config();

#if __IOS__
            config.LoadFromText(System.IO.File.ReadAllText(System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "config/" + config_name +".txt")));
#elif __ANDROID__
                using (var stream = Android.App.Application.Context.ApplicationContext.Assets.Open("config/" + config_name + ".txt"))
                {
                    config.LoadFromText(new System.IO.StreamReader(stream).ReadToEnd());
                }
#else

#endif

                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine("load Api.Config{0}) exception", config_name);
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
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
