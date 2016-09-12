using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Security.Cryptography;

namespace ForumClient.Api
{
    public class Forum
    {
        public string Id;
        public string Name;
        public string Desc;
    }

    public class Thread
    {
        public bool OnTop = false;
        public string Id;
        public string Title;
        public Author Author;
        public string PostTime;
        public Author Last_Author;
        public string Last_PostTime;
        public int PageNum;
        public int ReplyCount;
        public int ViewCount;
    }

    public class PostNode
    {
        public string NodeType;
        public string Text;
        public string HRef;
    }

    public class Post
    {
        public Author Author;
        public string Content;
        public string PostTime;
        public List<PostNode> Nodes;
    }

    public class Author
    {
        public string Id;
        public string Name;
    }

    public class Client
    {
        private string ForumUrl;
        private HttpClient c;
        private System.Net.CookieContainer Cookie;

        public Client(string url)
        {
            ForumUrl = url;
            Cookie = new System.Net.CookieContainer();
            var CookieFile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Cookies.bin");
            if (System.IO.File.Exists(CookieFile))
            {
                LoadCookies(CookieFile);
            }

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            handler.CookieContainer = Cookie;
            handler.UseCookies = true;
            c = new HttpClient(handler);
            c.BaseAddress = new Uri(url);
        }

        public bool IsAuthed()
        {
            foreach (System.Net.Cookie item in Cookie.GetCookies(new Uri(ForumUrl)))
            {
                if (item.Name == "cdb_auth")
                {
                    return true;
                }
            }
            return false;
        }

        public void SaveCookies(string filename)
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var s = System.IO.File.Create(filename))
            {
                formatter.Serialize(s, Cookie);
            }
        }

        public void LoadCookies(string filename)
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var s = System.IO.File.OpenRead(filename))
            {
                var v = formatter.Deserialize(s);
                var c = v as System.Net.CookieContainer;
                foreach (var item in c.GetCookies(new Uri(ForumUrl)))
                {
                    var d = item as System.Net.Cookie;
                    Cookie.Add(d);
                }
            }
        }

        public async Task<string> SignIn(string username, string password)
        {
            var param = new Dictionary<string, string>();
            param.Add("formhash", "37b0d4e3");
            param.Add("referer", ForumUrl + "index.php");
            param.Add("loginfield", "username");
            param.Add("username", username);
            param.Add("password", MD5Password(password));
            param.Add("questionid", "0");
            param.Add("answer", "");
            param.Add("loginsubmit", "true");
            param.Add("cookietime", "2592000");
            string error;
            using (var resp = await c.PostAsync(ForumUrl + "forum/logging.php?action=login&loginsubmit=yes&inajax=1", new FormUrlEncodedContent(param)))
            {
                var data = await resp.Content.ReadAsByteArrayAsync();
                var text = System.Text.Encoding.GetEncoding("gbk").GetString(data);
                if (text.IndexOf(ForumUrl, StringComparison.CurrentCulture) > 0)
                {
                    error = "";
                }
                else
                {
                    if (text.IndexOf("CDATA[", StringComparison.CurrentCulture) > 0) text = text.Substring(text.IndexOf("CDATA[", StringComparison.CurrentCulture) + 6);
                    if (text.IndexOf("]]", StringComparison.CurrentCulture) > 0) text = text.Substring(0, text.IndexOf("]]", StringComparison.CurrentCulture));
                    error = text;
                }
            }

            if (!IsAuthed() && error == "")
            {
                return "unkown error";
            }
            else
            {
                return error;
            }
        }

        public async Task<bool> SignOut()
        {
            bool result = true;
            using (var resp = await c.GetAsync(ForumUrl + "forum/logging.php?action=logout&formhash=420b6ba6"))
            {
                await resp.Content.ReadAsByteArrayAsync();
            }
            return result;
        }

        public async Task<List<Forum>> GetForumList()
        {
            var forums = new List<Forum>();
            using (var resp = await c.GetAsync(ForumUrl + "forum/index.php"))
            {
                var data = await resp.Content.ReadAsByteArrayAsync();
                var doc = new HtmlDocument();
                doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding("gbk"));
                var html = GetElementByType(doc.DocumentNode, "html");
                var body = GetElementByType(html, "body");
                var wrap = GetElementById(body, "div", "wrap");
                var main = GetElementByClass(wrap, "div", "main");
                var content = GetElementByClass(main, "div", "content");
                foreach (var mainbox in FindElementByClass(content, "div", "mainbox list"))
                {
                    var table = GetElementByType(mainbox, "table");
                    if (table == null) continue;
                    foreach (var tbody in FindElementByType(table, "tbody"))
                    {
                        var tr = GetElementByType(tbody, "tr");
                        if (tr == null) continue;
                        var th = GetElementByType(tr, "th");
                        if (th == null) continue;
                        var div = GetElementByType(th, "div");
                        if (div == null) continue;
                        var h2 = GetElementByType(div, "h2");
                        if (h2 == null) continue;
                        var a = GetElementByType(h2, "a");
                        if (a == null) continue;
                        var href = GetAttributeValue(a, "href");
                        if (href == null) continue;
                        var p = GetElementByType(div, "p");
                        if (p == null) continue;
                        var name = a.InnerText;
                        var id = href.Substring(href.IndexOf('=') + 1);
                        if (id.IndexOf('&') > 0) id = id.Substring(0, id.IndexOf('&'));
                        forums.Add(new Forum() { Id = id, Name = name, Desc = p.InnerText });
                    }
                }
            }
            return forums;
        }

        public async Task<List<Thread>> GetForum(string fid, int page)
        {
            var threads = new List<Thread>();

            using (var resp = await c.GetAsync(ForumUrl + "forum/forumdisplay.php?fid=" + fid + "&page=" + page.ToString()))
            {
                var data = await resp.Content.ReadAsByteArrayAsync();
                var doc = new HtmlDocument();
                doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding("gbk"));
                var html = GetElementByType(doc.DocumentNode, "html");
                var body = GetElementByType(html, "body");
                var wrap = GetElementById(body, "div", "wrap");
                var main = GetElementByClass(wrap, "div", "main");
                var content = GetElementByClass(main, "div", "content");
                var threadlist = GetElementById(content, "div", "threadlist");
                var table = GetElementByType(threadlist, "table");
                var OnTop = true;
                foreach (var tbody in FindElementByType(table, "tbody"))
                {
                    var tr = GetElementByType(tbody, "tr");
                    if (tr == null) continue;

                    var th = GetElementByType(tr, "th");
                    if (th == null) continue;

                    if (GetAttributeValue(th, "class") == "subject")
                    {
                        OnTop = false;
                        continue;
                    }

                    var max_page = 1;
                    var pages = GetElementByClass(th, "span", "threadpages");
                    if (pages != null)
                    {
                        foreach (var page_item in FindElementByType(pages, "a"))
                        {
                            var href = GetAttributeValue(page_item, "href");
                            var ndx = href.Substring(href.LastIndexOf("page=", StringComparison.CurrentCulture) + 5);
                            if (ndx.IndexOf('&') > 0) ndx = ndx.Substring(0, ndx.IndexOf('&'));
                            int num = int.Parse(ndx);
                            if (num > max_page) max_page = num;
                        }
                    }

                    var subject_span = GetElementByType(th, "span");
                    if (subject_span == null) continue;
                    var subject_a = GetElementByType(subject_span, "a");
                    var subject_href = GetAttributeValue(subject_a, "href");
                    var id = subject_href.Substring(subject_href.IndexOf('=') + 1);
                    if (id.IndexOf('&') > 0) id = id.Substring(0, id.IndexOf('&'));

                    var author_td = GetElementByClass(tr, "td", "author");
                    var author_cite = GetElementByType(author_td, "cite");
                    var author_a = GetElementByType(author_cite, "a");
                    var author = PaseAuthor(author_a);
                    var author_em = GetElementByType(author_td, "em");

                    Author l_author = null;
                    String l_posttime = "";
                    var last_td = GetElementByClass(tr, "td", "lastpost");
                    if (last_td != null)
                    {
                        var last_cite = GetElementByType(last_td, "cite");
                        var last_a = GetElementByType(last_cite, "a");
                        l_author = PaseAuthor(last_a);
                        var last_em = GetElementByType(last_td, "em");
                        var last_em_a = GetElementByType(last_em, "a");
                        l_posttime = last_em_a.InnerText;
                    }

                    threads.Add(new Thread() { OnTop = OnTop, Id = id, Title = subject_a.InnerText, Author = author, PostTime = author_em.InnerText, Last_Author = l_author, Last_PostTime = l_posttime, PageNum = max_page });
                }
            }
            return threads;
        }

        public async Task<List<Post>> GetThread(string tid, int page)
        {
            var retval = new List<Post>();
            var real_url = ForumUrl + "forum/viewthread.php?tid=" + tid + "&extra=page%3D1&page=" + page.ToString();
            using (var resp = await c.GetAsync(real_url))
            {
                var data = await resp.Content.ReadAsByteArrayAsync();
                var doc = new HtmlDocument();
                doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding("gbk"));
                var html = GetElementByType(doc.DocumentNode, "html");
                var body = GetElementByType(html, "body");
                var wrap = GetElementById(body, "div", "wrap");
                var postlist = GetElementById(wrap, "div", "postlist");
                foreach (var div in FindElementByType(postlist, "div"))
                {
                    var table = GetElementByType(div, "table");
                    if (table == null) continue;
                    var tr = GetElementByType(table, "tr");
                    if (tr == null) continue;

                    var postauthor = GetElementByClass(tr, "td", "postauthor");
                    var postinfo = GetElementByClass(postauthor, "div", "postinfo");
                    var postinfo_a = GetElementByType(postinfo, "a");
                    var author = PaseAuthor(postinfo_a);

                    var postcontent = GetElementByClass(tr, "td", "postcontent");

                    var postinfo_1 = GetElementByClass(postcontent, "div", "postinfo");
                    var postinfo_x = GetElementByClass(postinfo_1, "div", "posterinfo");
                    var postinfo_i = GetElementByClass(postinfo_x, "div", "authorinfo");
                    var postinfo_em = GetElementByType(postinfo_i, "em");
                    var posttime = postinfo_em.InnerText;
                    posttime = posttime.Substring(posttime.IndexOf(' ') + 1);

                    var defaultpost = GetElementByClass(postcontent, "div", "defaultpost");
                    var postmessage = GetElementByClass(defaultpost, "div", "postmessage firstpost");
                    if (postmessage == null)
                    {
                        postmessage = GetElementByClass(defaultpost, "div", "postmessage ");
                    }

                    string content = "";
                    var nodes = new List<PostNode>();

                    var postmessage_div = GetElementByClass(postmessage, "div", "t_msgfontfix");
                    if (postmessage_div != null)
                    {
                        var postmessage_table = GetElementByType(postmessage_div, "table");
                        if (postmessage_table == null)
                        {
                            postmessage_table = null;
                        }
                        var postmessage_tr = GetElementByType(postmessage_table, "tr");
                        var postmessage_td = GetElementByType(postmessage_tr, "td");

                        ParseHtmlNode(nodes, postmessage_td);
                        content = postmessage_td.InnerText;
                        /*
                        var builder = new System.Text.StringBuilder();
                        ParseHtmlNode(builder, postmessage_td);
                        content = builder.ToString();
                        Console.WriteLine("============================");
                        Console.WriteLine(content);
                        Console.WriteLine("============================");
                        */
                    }
                    else
                    {
                        var postmessage_lock = GetElementByClass(postmessage, "div", "locked");
                        if (postmessage_lock == null)
                        {
                            postmessage_lock = null;
                        }
                        content = postmessage_lock.InnerText;
                        nodes.Add(new PostNode()
                        {
                            NodeType = "text",
                            Text = content
                        });
                    }

                    retval.Add(new Post() { Author = author, Content = content, PostTime = posttime, Nodes = nodes });
                }
            }
            return retval;
        }

        HtmlNode GetElementByType(HtmlNode basenode, string type)
        {
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == type)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        HtmlNode GetElementById(HtmlNode basenode, string name, string id)
        {
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == name)
                    {
                        foreach (var attr in node.ChildAttributes("id"))
                        {
                            if (attr.Name == "id" && attr.Value == id)
                            {
                                return node;
                            }
                        }
                    }
                }
            }
            return null;
        }

        HtmlNode GetElementByClass(HtmlNode basenode, string name, string cname)
        {
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == name)
                    {
                        foreach (var attr in node.ChildAttributes("class"))
                        {
                            if (attr.Value == cname)
                            {
                                return node;
                            }
                        }
                    }
                }
            }
            return null;
        }

        List<HtmlNode> FindElementByType(HtmlNode basenode, string name)
        {
            var retval = new List<HtmlNode>();
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == name)
                    {
                        retval.Add(node);
                    }
                }
            }
            return retval;
        }

        List<HtmlNode> FindElementByClass(HtmlNode basenode, string name, string cname)
        {
            var retval = new List<HtmlNode>();
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == name)
                    {
                        foreach (var attr in node.ChildAttributes("class"))
                        {
                            if (attr.Name == "class" && attr.Value == cname)
                            {
                                retval.Add(node);
                                break;
                            }
                        }
                    }
                }
            }
            return retval;
        }

        string GetAttributeValue(HtmlNode basenode, string name)
        {
            foreach (var attr in basenode.ChildAttributes(name))
            {
                if (attr.Name == name)
                    return attr.Value;
            }
            return "";
        }

        void PrintNode(int level, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
                for (int i = 0; i < level; i++)
                    Console.Write("      ");
                var text = basenode.InnerHtml.Replace("\n", "");
                if (text.Length > 20) text = text.Substring(0, 20);
                Console.WriteLine("{0}-{1}-{2}-{3}", basenode.Name, GetAttributeValue(basenode, "id"), GetAttributeValue(basenode, "class"), text);
                foreach (var node in basenode.ChildNodes)
                {
                    PrintNode(level + 1, node);
                }
            }
        }

        void PrintHtml(HtmlDocument doc)
        {
            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                PrintNode(0, node);
            }
        }

        Author PaseAuthor(HtmlNode node)
        {
            var retval = new Author();
            retval.Name = "UNKNOWN";
            retval.Id = "0";
            if (node != null)
            {
                var href = GetAttributeValue(node, "href");
                if (href != null)
                {
                    var id = href.Substring(href.IndexOf('=') + 1);
                    if (id.IndexOf('&') > 0) id = id.Substring(0, id.IndexOf('&'));
                    retval.Id = id;
                    retval.Name = node.InnerText;
                }
            }
            return retval;
        }

        string MD5Password(string password)
        {
            var md5 = MD5.Create();
            var sb = new System.Text.StringBuilder();
            foreach (var b in md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(password)))
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        void ParseHtmlNode(System.Text.StringBuilder builder, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
                if (basenode.Name == "div" && GetAttributeValue(basenode, "class") == "t_attach")
                {
                    return;
                }
                if (basenode.Name == "a")
                {
                    var href = GetAttributeValue(basenode, "href");
                    HtmlNode firstnode = null;
                    if (basenode.ChildNodes.Count > 0)
                    {
                        firstnode = basenode.ChildNodes[0];
                    }
                    if (firstnode != null)
                    {
                        builder.Append("link: " + href + " text: " + firstnode.InnerText);
                    }
                    else
                    {
                        builder.Append("link: " + href + " text: none");
                    }
                    builder.Append("\n");
                    return;
                }
                if (basenode.Name == "img")
                {
                    var src = GetAttributeValue(basenode, "file").Trim();
                    if (src == "")
                    {
                        src = GetAttributeValue(basenode, "src").Trim();
                    }
                    builder.Append("image: " + src);
                    builder.Append("\n");
                    return;
                }
                if (basenode.Name == "span" && GetAttributeValue(basenode, "style") == "position: absolute; display: none")
                {
                    return;
                }
            }

            if (basenode.NodeType == HtmlNodeType.Text)
            {
                var text = basenode.InnerText.Replace("&nbsp;", " ").Trim();
                if (text.Length > 0)
                {
                    var lines = basenode.InnerText.Split(new char['\n']);
                    bool first = false;
                    foreach (var line in lines)
                    {
                        if (!first) builder.Append(" ");
                        first = true;
                        builder.Append(line.Trim());
                    }
                    builder.Append("\n");
                }
            }

            foreach (var node in basenode.ChildNodes)
            {
                ParseHtmlNode(builder, node);
            }
        }

        void ParseHtmlNode(List<PostNode> nodes, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
                if (basenode.Name == "div" && GetAttributeValue(basenode, "class") == "t_attach")
                {
                    return;
                }
                if (basenode.Name == "a")
                {
                    var href = GetAttributeValue(basenode, "href");
                    HtmlNode firstnode = null;
                    if (basenode.ChildNodes.Count > 0)
                    {
                        firstnode = basenode.ChildNodes[0];
                    }
                    if (firstnode != null)
                    {
                        nodes.Add(new PostNode()
                        {
                            NodeType = "link",
                            HRef = href,
                            Text = firstnode.InnerText
                        });
                    }
                    else
                    {
                        nodes.Add(new PostNode()
                        {
                            NodeType = "link",
                            HRef = href,
                            Text = ""
                        });
                    }
                    return;
                }
                if (basenode.Name == "img")
                {
                    var src = GetAttributeValue(basenode, "file").Trim();
                    if (src == "")
                    {
                        src = GetAttributeValue(basenode, "src").Trim();
                    }
                    nodes.Add(new PostNode()
                    {
                        NodeType = "image",
                        HRef = src
                    });
                    return;
                }
                if (basenode.Name == "span" && GetAttributeValue(basenode, "style") == "position: absolute; display: none")
                {
                    return;
                }
            }

            if (basenode.NodeType == HtmlNodeType.Text)
            {
                var text = basenode.InnerText;
                text = text.Replace("\r", "");
                text = text.Replace("\n", "");
                if (text.IndexOf("nbsp", StringComparison.CurrentCulture) >= 0)
                {
                    text = text.Replace("&nbsp;", " ");
                }
                text = text.Replace("&nbsp;", " ");
                text = text.Trim();
                if (text.Length > 0)
                {
                    var lines = basenode.InnerText.Split(new char['\n']);
                    bool first = true;
                    var builder = new System.Text.StringBuilder();
                    foreach (var line in lines)
                    {
                        if (!first) builder.Append(" ");
                        first = false;
                        builder.Append(line.Trim());
                    }
                    nodes.Add(new PostNode()
                    {
                        NodeType = "text",
                        Text = builder.ToString()
                    });
                }
            }

            foreach (var node in basenode.ChildNodes)
            {
                ParseHtmlNode(nodes, node);
            }
        }
    }
}
