using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ForumClient
{

    public class PostMenuItem : MenuItem
    {
        private Api.Post postData;
        public PostMenuItem(Api.Post data)
        {
            postData = data;
        }

        public string AuthorName
        {
            get
            {
                return postData.Author.Name;
            }
        }
        public string PostTime
        {
            get
            {
                return postData.PostTime;
            }
        }
        public string PostContent
        {
            get
            {
                return postData.Content;
            }
        }
    }

    public partial class PostCell : StackLayout
    {
        public PostCell(Api.Post data)
        {
            BindingContext = new PostMenuItem(data);
            InitializeComponent();
            foreach (var node in data.Nodes)
            {
                if (node.NodeType == "text")
                {
                    content.Children.Add(new Label() { Text = node.Text });
                }
                if (node.NodeType == "link")
                {
                    var button = new Button() { Text = node.HRef };
                    button.Clicked += (sender, e) =>
                    {
                        var url = node.HRef;
                        Console.WriteLine(url);
                    };
                    content.Children.Add(button);
                }
                if (node.NodeType == "image")
                {
                    content.Children.Add(new Label() { Text = "image : " + node.HRef });
                }
            }
        }
    }
}

