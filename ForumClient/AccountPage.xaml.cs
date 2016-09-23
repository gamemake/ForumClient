using System;
using Xamarin.Forms;

namespace ForumClient
{
    public partial class AccountPage : ContentPage
    {
        private Api.Client Client;
        public AccountPage(Api.Client client)
        {
            Client = client;
            InitializeComponent();
            UsernameEntry.Keyboard = Keyboard.Create(0);
        }

        async void OnSignIn(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text))
            {
                MessageLabel.Text = "请输入用户名和密码";
                return;
            }
            var result = await Client.SignIn(UsernameEntry.Text, PasswordEntry.Text);
            if (result != "")
            {
                MessageLabel.Text = result;
                return;
            }

            Client.SaveCookies();
            OnAnonymous(sender, e);
        }

        void OnAnonymous(object sender, EventArgs e)
        {
            if (!Client.IsAuthed() && !Client.Config.AllowAnonymous)
            {
                MessageLabel.Text = "此站点不支持匿名登录";
                return;
            }

            var page = new ForumListPage(Client);
            (Application.Current as App).DetailPage = page;
            page.Fetch();
        }
    }
}