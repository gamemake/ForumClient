using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ForumClient.Api
{
    public class Forum
    {
        public string Id;
        public string Name;
    }

    public class Client
    {
        private string ForumUrl;
        private HttpClient c;


        public Client(string url)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }
            c = new HttpClient(handler);
            ForumUrl = url;
        }

        public async Task<List<Forum>> GetForumList()
        {
            try
            {
                var resp = await c.GetAsync(ForumUrl + "index.php");
                var data = await resp.Content.ReadAsByteArrayAsync();
                var doc = new HtmlDocument();
                doc.Load(new System.IO.MemoryStream(data), System.Text.Encoding.GetEncoding("gbk"));
                var html = GetElementByType(doc.DocumentNode, "html");
                var body = GetElementByType(html, "body");
                var wrap = GetDivById(body, "wrap");
                var main = GetDivByClass(wrap, "main");
                var content = GetDivByClass(main, "content");
                foreach (var mainbox in FindDivByClass(content, "mainbox list"))
                {
                    var table = GetElementByType(mainbox, "table");
                    var tbody = GetElementByType(table, "tbody");
                    var tr = GetElementByType(tbody, "tr");
                    var th = GetElementByType(tr, "th");
                    var div = GetElementByType(th, "div");
                    var h2 = GetElementByType(div, "h2");
                    var a = GetElementByType(h2, "a");
                    var href = GetAttributeValue(a, "href");
                    var name = a.InnerText;
                    var id = href.Substring(href.LastIndexOf('=') + 1);
                    Console.WriteLine("tid={0} name={1}", id, name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public HtmlNode GetElementByType(HtmlNode basenode, string type)
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

        public HtmlNode GetDivById(HtmlNode basenode, string id)
        {
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == "div")
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
    
        public HtmlNode GetDivByClass(HtmlNode basenode, string name)
        {
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == "div")
                    {
                        foreach (var attr in node.ChildAttributes("class"))
                        {
                            if (attr.Value == name)
                            {
                                return node;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<HtmlNode> FindElementByType(HtmlNode basenode, string name)
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

        public List<HtmlNode> FindDivByClass(HtmlNode basenode, string name)
        {
            var retval = new List<HtmlNode>();
            foreach (var node in basenode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == "div")
                    {
                        foreach (var attr in node.ChildAttributes("class"))
                        {
                            if (attr.Value == name)
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

        public string GetAttributeValue(HtmlNode basenode, string name)
        {
            foreach (var attr in basenode.ChildAttributes(name))
            {
                if (attr.Name == name)
                    return attr.Value;
            }
            return "";
        }
    }
}
