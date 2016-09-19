using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Security.Cryptography;

namespace ForumClient.Api
{
    public interface IForumFetch
    {
        void Begin();
        void NewForum(string Id, string Name, string Desc);
        void End();
    }

    public interface IThreadFetch
    {
        void Begin();
        void NewThread(bool OnTop, string Id, string Title, Author PostMan, string PostTime, Author LastMan, string LastTime);
        void End();
    }

    public interface IPostFetch
    {
        void Begin();
        void BeginPost(string Id, Author PostMan, string PostTime);
        void NewText(string Text);
        void NewLink(string Url, string Text);
        void NewImage(string Url);
        void EndPost(string Id, Author PostMan, string PostTime);
        void End();
    }

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
        public Author Author = new Author();
        public string PostTime;
        public Author Last_Author = new Author();
        public string Last_PostTime;
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
        public string Id;
        public Author Author = new Author();
        public string Content;
        public string PostTime;
        public List<PostNode> Nodes = new List<PostNode>();
    }

    public class Author
    {
        public string Id;
        public string Name;
    }

    public class Client
    {
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
            handler.Proxy = new System.Net.WebProxy("http://192.168.10.148:1080/", true);
            handler.CookieContainer = this.cookies;
            handler.UseCookies = true;
            handler.AllowAutoRedirect = true;
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36");

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
            bool result = true;
            try
            {
                using (var resp = await client.GetAsync(config.logout_url))
                {
                    await resp.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
            }
            return result;
        }

        public async Task<List<Forum>> GetForumList()
        {
            var forums = new List<Forum>();
            try
            {
                using (var resp = await client.GetAsync(config.forumlist_url))
                {
                    var data = await resp.Content.ReadAsByteArrayAsync();
                    var doc = new HtmlDocument();
                    doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding(config.text_encoder));

                    var forum_root = GetElement(doc.DocumentNode, config.forum_root, 0);
                    if (forum_root != null)
                    {
                        foreach (var child1 in forum_root.ChildNodes)
                        {
                            if (child1.NodeType != HtmlNodeType.Element) continue;

                            var forum_category = GetElement(child1, config.forum_category);
                            if (forum_category == null) continue;

                            foreach (var child in forum_category.ChildNodes)
                            {
                                if (child.NodeType != HtmlNodeType.Element) continue;

                                var forum_start = GetElement(child, config.forum_start);
                                if (forum_start == null) continue;

                                var forum = new Forum();
                                HtmlNode node;

                                node = GetElement(forum_start, config.forum_id, 0);
                                if (node == null) continue;
                                forum.Id = GetUrlString(GetAttributeValue(node, "href"), "fid=", "&");

                                node = GetElement(forum_start, config.forum_title, 0);
                                if (node == null) continue;
                                forum.Name = node.InnerText;

                                node = GetElement(forum_start, config.forum_desc, 0);
                                if (node == null) continue;
                                forum.Desc = node.InnerText;

                                forums.Add(forum);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
            return forums;
        }

        public async Task<List<Thread>> GetForum(string fid, int page)
        {
            var threads = new List<Thread>();
            try
            {
                var url = string.Format(config.threadlist_url, fid, page);
                using (var resp = await client.GetAsync(url))
                {
                    var data = await resp.Content.ReadAsByteArrayAsync();

                    var doc = new HtmlDocument();
                    doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding(config.text_encoder));

                    var thread_root = GetElement(doc.DocumentNode, config.thread_root, 0);
                    if (thread_root != null)
                    {
                        foreach (var child in thread_root.ChildNodes)
                        {
                            var thread_start = GetElement(child, config.thread_start);
                            if (thread_start == null) continue;

                            if (GetElement(child, config.thread_default, 0) != null)
                            {
                                for (int i = 0; i < threads.Count; i++)
                                {
                                    threads[i].OnTop = true;
                                }
                                continue;
                            }

                            // Console.WriteLine("=========================================================================================");
                            // PrintNode(0, thread_start);

                            var thread = new Thread();
                            HtmlNode node;

                            node = GetElement(thread_start, config.thread_id, 0);
                            if (node == null) continue;
                            thread.Id = GetUrlString(GetAttributeValue(node, "href"), "tid=", "&");
                            if (thread.Id.LastIndexOf('/') > 0)
                            {
                                thread.Id = thread.Id.Substring(thread.Id.LastIndexOf('/') + 1);
                            }
                            if (thread.Id.IndexOf('.') > 0)
                            {
                                thread.Id = thread.Id.Substring(0, thread.Id.LastIndexOf('.'));
                            }

                            node = GetElement(thread_start, config.thread_title, 0);
                            if (node == null) continue;
                            thread.Title = GetHtmlNodeText(node);

                            node = GetElement(thread_start, config.thread_post_auth_name, 0);
                            if (node == null) continue;
                            thread.Author.Name = node.InnerText;

                            node = GetElement(thread_start, config.thread_post_auth_id, 0);
                            if (node == null) continue;
                            thread.Author.Id = GetUrlString(GetAttributeValue(node, "href"), "uid=", "&");

                            node = GetElement(thread_start, config.thread_post_time, 0);
                            if (node == null) continue;
                            thread.PostTime = node.InnerText;

                            node = GetElement(thread_start, config.thread_last_auth_name, 0);
                            if (node != null)
                                thread.Last_Author.Name = node.InnerText;

                            node = GetElement(thread_start, config.thread_last_auth_id, 0);
                            if (node != null)
                                thread.Last_Author.Id = GetUrlString(GetAttributeValue(node, "href"), "username=", "&");

                            node = GetElement(thread_start, config.thread_last_time, 0);
                            if (node != null)
                                thread.Last_PostTime = node.InnerText;

                            threads.Add(thread);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
            }
            return threads;
        }

        public async Task<List<Post>> GetThread(string tid, int page)
        {
            var retval = new List<Post>();
            try
            {
                var url = string.Format(config.postlist_url, tid, page);
                var doc = await GetHtmlDocument(url, true);
                if (doc == null)
                {
                    return null;
                }
                else
                {
                    var post_root = GetElement(doc.DocumentNode, config.post_root, 0);
                    foreach (var child in post_root.ChildNodes)
                    {
                        var post_start = GetElement(child, config.post_start);
                        if (post_start == null) continue;

                        Console.WriteLine("==================");
                        PrintNode(0, post_start);

                        var post = new Post();
                        HtmlNode node;

                        node = GetElement(post_start, config.post_id, 0);
                        if (node == null)
                            continue;
                        post.Id = GetAttributeValue(node, "href");
                        post.Id = GetUrlString(post.Id, "pid=", "&");

                        node = GetElement(post_start, config.post_auth_name, 0);
                        if (node == null)
                            continue;
                        post.Author.Name = node.InnerText;

                        if(config.post_auth_id.Count>0)
                        {
                            node = GetElement(post_start, config.post_auth_id, 0);
                            if (node == null) continue;
                            post.Author.Name = GetUrlString(GetAttributeValue(node, "href"), "uid=", "&");
                        }
                        else
                        {
                            post.Author.Id = "";
                        }

                        node = GetElement(post_start, config.post_time, 0);
                        if (node == null)
                            continue;
                        post.PostTime = node.InnerText;
                        if (config.post_time_left.Length > 0 && post.PostTime.LastIndexOf(config.post_time_left) >= 0)
                        {
                            post.PostTime = post.PostTime.Substring(post.PostTime.LastIndexOf(config.post_time_left) + config.post_time_left.Length);
                        }
                        if (config.post_time_right.Length > 0 && post.PostTime.LastIndexOf(config.post_time_right) >= 0)
                        {
                            post.PostTime = post.PostTime.Substring(0, post.PostTime.LastIndexOf(config.post_time_right));
                        }

                        node = GetElement(post_start, config.post_content_1, 0);
                        if (node == null)
                        {
                            node = GetElement(post_start, config.post_content_2, 0);
                            if (node == null) continue;
                        }
                        ParseHtmlNode(post.Nodes, node);

                        retval.Add(post);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
                return null;
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


        bool CheckElement(HtmlNode node, ConfigItem item)
        {
            if (node.NodeType != HtmlNodeType.Element) return false;
            foreach (var pair in item)
            {
                if (pair.Key == "name")
                {
                    if (pair.Value != node.Name) return false;
                }
                else if(pair.Key== "inner_html")
                {
                    if (node.InnerHtml != pair.Value) return false;
                }
                else
                {
                    if(pair.Value != GetAttributeValue(node, pair.Key) && (pair.Value != "*" || GetAttributeValue(node, pair.Key) == "")) return false;
                }
            }
            return true;
        }

        HtmlNode GetElement(HtmlNode root, List<ConfigItem> list)
        {
            if (!CheckElement(root, list[0])) return null;
            return GetElement(root, list, 1);
        }

        HtmlNode GetElement(HtmlNode root, List<ConfigItem> list, int index)
        {
            if (index == list.Count) return root;

            foreach (var node in root.ChildNodes)
            {
                HtmlNode retval;
                if (list[index].Count > 0)
                {
                    if (!CheckElement(node, list[index])) continue;
                }
                else
                {
                    retval = GetElement(root, list, index + 1);
                    if (retval != null) return retval;
                    retval = GetElement(node, list, index);
                    if (retval != null) return retval;
                }
                retval = GetElement(node, list, index + 1);
                if (retval != null) return retval;
            }
            return null;
        }

        string GetUrlString(string url, string start, string end)
        {
            var retval = url;
            int pos = retval.IndexOf(start, StringComparison.CurrentCulture);
            if (pos >= 0) retval = retval.Substring(pos + start.Length);
            pos = retval.IndexOf(end, StringComparison.CurrentCulture);
            if (pos >= 0) retval = retval.Substring(0, pos);
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

        void GetHtmlNodeText(System.Text.StringBuilder builder, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Text)
            {
                var text = basenode.OuterHtml;
                text = text.Replace("\n", "");
                text = text.Replace("\r", "");
                text = text.Trim();
                if (text.Length > 0)
                {
                    builder.Append(text);
                }
            }

            foreach (var node in basenode.ChildNodes)
            {
                GetHtmlNodeText(builder, node);
            }
        }

        string GetHtmlNodeText(HtmlNode basenode)
        {
            var builder = new System.Text.StringBuilder();
            GetHtmlNodeText(builder, basenode);
            return builder.ToString();
        }

        void ParseHtmlNode(List<PostNode> nodes, HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Element)
            {
                foreach(var c in config.post_content_ignore)
                {
                    if (CheckElement(basenode, c))
                    {
                        return;
                    }
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

        public async System.Threading.Tasks.Task<byte[]> GetRawData(string url, bool redirect)
        {
            try
            {
                do
                {
                    using (var resp = await client.GetAsync(url))
                    {
                        var data = await resp.Content.ReadAsByteArrayAsync();
                        return data;
                    }
                } while (redirect);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public async System.Threading.Tasks.Task<HtmlDocument> GetHtmlDocument(string url, bool redirect)
        {
            HtmlDocument doc = null;
            try
            {
                do
                {
                    using (var resp = await client.GetAsync(url))
                    {
                        var data = await resp.Content.ReadAsByteArrayAsync();
                        doc = new HtmlDocument();
                        doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding(config.text_encoder));
                        if (redirect)
                        {
                            var config_root = new List<Api.ConfigItem>();
                            var config_item = new Api.ConfigItem();
                            config_item.Add("name", "html");
                            config_root.Add(config_item);
                            config_item = new Api.ConfigItem();
                            config_item.Add("name", "head");
                            config_root.Add(config_item);
                            var head = GetElement(doc.DocumentNode, config_root, 0);
                            redirect = false;
                            if (head != null)
                            {
                                foreach (var meta in head.ChildNodes)
                                {
                                    if (meta.Name == "meta")
                                    {
                                        if (GetAttributeValue(meta, "http-equiv") == "refresh")
                                        {
                                            var rurl = GetAttributeValue(meta, "content");
                                            var index = rurl.IndexOf('=');
                                            if (index >= 0)
                                            {
                                                rurl = rurl.Substring(index + 1).Trim();
                                                url = rurl;
                                                redirect = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } while (redirect);
            }
            catch (Exception)
            {
                doc = null;
            }
            return doc;
        }
    }
}
