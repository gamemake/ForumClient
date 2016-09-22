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
        public string PostAuthor;
        public string PostTime;
        public string LastAuthor;
        public string LastTime;
    }

    public class PostNode
    {
        public string NodeType;
        public string Text;
        public string HRef;
    }

    public class Post
    {
        public string Id;
        public string PostAuthor;
        public string PostTime;
        public List<PostNode> Content = new List<PostNode>();
    }

    public class Client
    {
        public delegate void PageEndNotify();

        private Config config;
        private string cookie_file;
        private System.Net.CookieContainer cookies;
        private HttpClient client;

        public HttpClient GetHttpClient()
        {
            return client;
        }

        public Client(string name, Config config)
        {
            this.config = config;
            this.cookie_file = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Cookies-" + name + ".bin");
            this.cookies = new System.Net.CookieContainer();
            if (System.IO.File.Exists(this.cookie_file))
            {
                LoadCookies();
            }

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            handler.CookieContainer = this.cookies;
            handler.UseCookies = true;
            handler.AllowAutoRedirect = true;
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", config.user_agent);

            client.BaseAddress = new Uri(this.config.base_url);
        }

        public bool IsAuthed()
        {
            foreach (System.Net.Cookie item in cookies.GetCookies(new Uri(config.base_url)))
            {
                if (item.Name == "cdb_auth")
                {
                    return true;
                }
            }
            return false;
        }

        public void SaveCookies()
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var s = System.IO.File.Create(cookie_file))
            {
                formatter.Serialize(s, cookies);
            }
        }

        public void LoadCookies()
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var s = System.IO.File.OpenRead(cookie_file))
            {
                var v = formatter.Deserialize(s);
                var c = v as System.Net.CookieContainer;
                foreach (var item in c.GetCookies(new Uri(config.base_url)))
                {
                    var d = item as System.Net.Cookie;
                    cookies.Add(d);
                }
            }
        }

        public async Task<string> SignIn(string username, string password)
        {
            string error;

            try
            {
                var param = new Dictionary<string, string>();
                param.Add("formhash", "37b0d4e3");
                param.Add("referer", config.forumlist_url);
                param.Add("loginfield", "username");
                param.Add("username", username);
                param.Add("password", MD5Password(password));
                param.Add("questionid", "0");
                param.Add("answer", "");
                param.Add("loginsubmit", "true");
                param.Add("cookietime", "2592000");
                using (var resp = await client.PostAsync(config.login_url, new FormUrlEncodedContent(param)))
                {
                    var data = await resp.Content.ReadAsByteArrayAsync();
                    var text = System.Text.Encoding.GetEncoding(config.text_encoder).GetString(data);
                    if (text.IndexOf(config.forumlist_url, StringComparison.CurrentCulture) > 0)
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
            }
            catch (Exception e)
            {
                error = e.Message;
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
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
            var ret = await GetHtmlDocument(config.logout_url, false);
            return ret != null;
        }

        public async Task<List<Forum>> GetForumList()
        {
            try
            {
                var doc = await GetHtmlDocument(config.forumlist_url, true);
                if (doc == null) return null;

                var forums = new List<Forum>();
                var start = DateTime.UtcNow;

                var nodes = doc.DocumentNode.SelectNodes(config.forum_node);
                if (nodes == null)
                {
                    Console.WriteLine("ParseForumList error : nodes not found");
                    return null;
                }

                foreach (var node in nodes)
                {
                    var id = GetStringByXPath(node, config.forum_id);
                    var name = GetStringByXPath(node, config.forum_name);
                    var desc = GetStringByXPath(node, config.forum_desc);

                    if (id == "" || name == "" || desc == "")
                    {
                        Console.WriteLine("error in parse forum node : id == \"\" || name == \"\" || desc == \"\"");
                    }
                    else
                    {
                        forums.Add(new Api.Forum()
                        {
                            Id = GetUrlString(id, "fid=", "&"),
                            Name = name,
                            Desc = desc
                        });
                    }
                }

                Console.WriteLine("ParseForumList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                return forums;
            }
            catch (Exception e)
            {
                Console.WriteLine("ParseForumList exception");
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
        }

        public async Task<List<Thread>> GetForum(string fid, int page)
        {
            try
            {
                var url = string.Format(config.threadlist_url, fid, page);
                var doc = await GetHtmlDocument(url, true);
                if (doc == null) return null;

                var start = DateTime.UtcNow;
                var threads = new List<Thread>();

                var nodes = doc.DocumentNode.SelectNodes(config.thread_node);
                if (nodes == null)
                {
                    Console.WriteLine("ParseThreadList error : nodes not found");
                    return null;
                }

                foreach (var node in nodes)
                {
                    var ontop = node.SelectSingleNode(config.thread_ontop);
                    var id = FixThreadId(GetStringByXPath(node, config.thread_id));
                    var title = GetStringByXPath(node, config.thread_title);
                    var post_author = GetStringByXPath(node, config.thread_post_author);
                    var post_time = FixTimeString(GetStringByXPath(node, config.thread_post_time));
                    var last_author = GetStringByXPath(node, config.thread_last_author);
                    var last_time = FixTimeString(GetStringByXPath(node, config.thread_last_time));

                    if (id == "" || title == "")
                    {
                        Console.WriteLine("error in parse thrad node : id == null || title == null");
                    }

                    threads.Add(new Api.Thread()
                    {
                        OnTop = ontop != null,
                        Id = GetUrlString(GetUrlString(id, "tid=", "&"), "/", "."),
                        Title = HtmlEntity.DeEntitize(title),
                        PostAuthor = post_author,
                        PostTime = post_time,
                        LastAuthor = last_author,
                        LastTime = last_time
                    });
                }

                Console.WriteLine("ParseThreadList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                return threads;
            }
            catch (Exception e)
            {
                Console.WriteLine("ParseThreadList exception");
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
        }

        public async Task<List<Post>> GetThread(string tid, int page, PageEndNotify notify = null)
        {
            try
            {
                var url = string.Format(config.postlist_url, tid, page);
                var doc = await GetHtmlDocument(url, true);
                if (doc == null) return null;

                var start = DateTime.UtcNow;
                var posts = new List<Post>();

                var nodes = doc.DocumentNode.SelectNodes(config.post_node);
                if (nodes == null)
                {
                    Console.WriteLine("ParsePostList error : nodes not found");
                    return null;
                }

                foreach (var node in nodes)
                {
                    var id = GetStringByXPath(node, config.post_id);
                    id = GetUrlString(id, "post=", "&");
                    id = GetUrlString(id, "pid=", "&");
                    var author = GetStringByXPath(node, config.post_author);
                    var time = FixTimeString(GetStringByXPath(node, config.post_time));
                    var content = node.SelectSingleNode(config.post_content);

                    if (author == "" || content == null)
                    {
                        Console.WriteLine("error in parse thrad node : author == \"\" || content == null");
                    }

                    var list = new List<Api.PostNode>();
                    RemoveNodes(content);
                    ParseHtmlNode(list, content);

                    posts.Add(new Api.Post()
                    {
                        Id = id,
                        PostAuthor = author,
                        PostTime = time,
                        Content = list
                    });
                }

                var last_page = true;
                if (!string.IsNullOrWhiteSpace(config.post_page_end))
                {
                    last_page = doc.DocumentNode.SelectSingleNode(config.post_page_end) != null;
                }
                if (!string.IsNullOrWhiteSpace(config.post_page_next))
                {
                    last_page = doc.DocumentNode.SelectSingleNode(config.post_page_next) != null;
                }

                Console.WriteLine("ParsePostList {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                if (last_page) notify();
                return posts;
            }
            catch (Exception e)
            {
                Console.WriteLine("ParsePostList exception");
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
        }

        public async System.Threading.Tasks.Task<HtmlDocument> GetHtmlDocument(string url, bool redirect)
        {
            try
            {
                HtmlDocument doc = null;
                var start = System.DateTime.UtcNow;
                double html_time = 0;

                for (;;)
                {
                    using (var resp = await client.GetAsync(url))
                    {
                        var data = await resp.Content.ReadAsByteArrayAsync();

                        var html_start = DateTime.UtcNow;
                        doc = new HtmlDocument();
                        doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding(config.text_encoder));
                        html_time += (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond;

                        if (!redirect) break;
                        var rurl = GetStringByXPath(doc.DocumentNode, "/html/head/meta[@http-equiv='refresh']/@content");
                        if (rurl == "") break;
                        url = GetUrlString(rurl, "url=", null);
                    }
                }

                Console.WriteLine("ParseHtmlDocument {0}", html_time);
                Console.WriteLine("GetHtmlDocument {0}", (double)(DateTime.UtcNow - start).Ticks / (double)TimeSpan.TicksPerSecond);
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetHtmlDocument failed");
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
        }

        string GetStringByXPath(HtmlNode basenode, string path)
        {
            var node = basenode.SelectSingleNode(path);
            if (node == null) return "";
            var pos = path.LastIndexOf('/');
            if (pos > 0)
            {
                if (path.Substring(pos + 1).StartsWith("@"))
                {
                    return GetAttributeValue(node, path.Substring(pos + 2));
                }
            }
            return HtmlEntity.DeEntitize(node.InnerText);
        }

        string FixTimeString(string time)
        {
            if (time.Length == 0) return "";
            int start, end;
            for (start = 0; !Char.IsDigit(time[start]); start++)
            {
                if (start + 1 == time.Length) return "";
            }
            for (end = time.Length - 1; end > start && !Char.IsDigit(time[end]); end--) { }
            return time.Substring(start, end - start + 1);
        }

        string FixThreadId(string value)
        {
            value = GetUrlString(value, "tid=", "&");
            if (value.Length == 0) return "";
            int end, start;
            for (end = value.Length - 1; !Char.IsDigit(value[end]); end--)
            {
                if (end == 0) return "";
            }
            for (start = end; start > 0 && Char.IsDigit(value[start - 1]); start--) { }
            return value.Substring(start, end - start + 1);
        }

        string GetUrlString(string url, string start, string end)
        {
            var retval = url;
            int pos = retval.IndexOf(start, StringComparison.CurrentCulture);
            if (pos >= 0)
            {
                retval = retval.Substring(pos + start.Length);
                if (!string.IsNullOrEmpty(end))
                {
                    pos = retval.IndexOf(end, StringComparison.CurrentCulture);
                    if (pos >= 0) retval = retval.Substring(0, pos);
                }
            }
            return retval;
        }

        void PrintNode(int level, HtmlNode basenode)
        {
            if (level == 4) return;

            if (basenode.NodeType == HtmlNodeType.Element)
            {
                for (int i = 0; i < level; i++)
                    Console.Write("      ");
                var text = basenode.InnerHtml.Replace('\n', ' ').Replace('\r', ' ');
                if (text.Length > 40) text = text.Substring(0, 40);
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

        string GetAttributeValue(HtmlNode basenode, string name)
        {
            foreach (var attr in basenode.ChildAttributes(name))
            {
                if (attr.Name == name)
                    return attr.Value;
            }
            return "";
        }

        void RemoveNodes(HtmlNode basenode)
        {
            var nodes = basenode.SelectNodes(config.post_ignore);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }
        }

        void ParseHtmlNode(List<PostNode> nodes, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
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
            }

            if (basenode.NodeType == HtmlNodeType.Text)
            {

                var text = HtmlEntity.DeEntitize(basenode.InnerText);
                text = text.Trim();
                if (text.Length > 0)
                {
                    nodes.Add(new PostNode()
                    {
                        NodeType = "text",
                        Text = text
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
