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
        public string Id;
        public string Title;
        public Author Author;
        public string PostTime;
        public Author Last_Author;
        public string Last_PostTime;
        public int PageNum;
    }

    public class Post
    {
        public Author Author;
        public string Content;
        public string PostTime;
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
            Cookie = new System.Net.CookieContainer();
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            handler.CookieContainer = Cookie;
            handler.UseCookies = true;
            c = new HttpClient(handler);
            c.BaseAddress = new Uri(url);
            ForumUrl = url;
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
                var c = (System.Net.Cookie)formatter.Deserialize(s);
                Cookie.Add(c);
            }
        }

        public async Task<bool> SignIn(string username, string password)
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
            bool retval = false;
            using (var resp = await c.PostAsync(ForumUrl + "forum/logging.php?action=login&loginsubmit=yes&inajax=1", new FormUrlEncodedContent(param)))
            {
                await resp.Content.ReadAsByteArrayAsync();
                foreach (System.Net.Cookie item in Cookie.GetCookies(new Uri(ForumUrl)))
                {
                    if (item.Name == "cdb_auth")
                    {
                        retval = true;
                        break;
                    }
                }
            }
            return retval;
        }

        public async Task<List<Forum>> GetForumList()
        {
            var forums = new List<Forum>();
            using (var resp = await c.GetAsync(ForumUrl + "index.php"))
            {
                var data = await resp.Content.ReadAsByteArrayAsync();
//                var text = System.Text.Encoding.GetEncoding("gbk").GetString(data);
                var doc = new HtmlDocument();
                doc.Load(new System.IO.MemoryStream(data));//, System.Text.Encoding.GetEncoding("gbk"));
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
                        var th = GetElementByType(tr, "th");
                        var div = GetElementByType(th, "div");
                        var h2 = GetElementByType(div, "h2");
                        var a = GetElementByType(h2, "a");
                        var href = GetAttributeValue(a, "href");
                        var p = GetElementByType(div, "p");
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
                foreach (var tbody in FindElementByType(table, "tbody"))
                {
                    var tr = GetElementByType(tbody, "tr");
                    if (tr == null) continue;

                    var th = GetElementByType(tr, "th");
                    if (th == null) continue;

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

                    threads.Add(new Thread() { Id = id, Title = subject_a.InnerText, Author = author, PostTime = author_em.InnerText, Last_Author = l_author, Last_PostTime = l_posttime, PageNum = max_page });
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
                    string content;

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
                        content = postmessage_td.InnerText;
                    }
                    else
                    {
                        var postmessage_lock = GetElementByClass(postmessage, "div", "locked");
                        if (postmessage_lock == null)
                        {
                            postmessage_lock = null;
                        }
                        content = postmessage_lock.InnerText;
                    }

                    retval.Add(new Post() { Author = author, Content = content, PostTime = posttime });
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
            return "none";
        }

        void PrintNode(int level, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
                for (int i = 0; i < level; i++)
                    Console.Write("      ");
                Console.WriteLine("{0}-{1}-{2}", basenode.Name, GetAttributeValue(basenode, "id"), GetAttributeValue(basenode, "class"));
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
            var href = GetAttributeValue(node, "href");
            var id = href.Substring(href.IndexOf('=') + 1);
            if (id.IndexOf('&') > 0) id = id.Substring(0, id.IndexOf('&'));
            retval.Id = id;
            retval.Name = node.InnerText;
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
    }
}
