using System;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ForumClient.Api
{
    public class NodeParser
    {
        private List<PostNode> Nodes;
        private StringBuilder Strings;
        private Stack<string> Links;

        public NodeParser()
        {
        }

        public List<PostNode> Parse(HtmlNode rootnode)
        {
            Nodes = new List<PostNode>();
            Strings = new StringBuilder();
            Links = new Stack<string>();
            ParseNodeChildren(rootnode);
            CommitText();
            return Nodes;
        }

        private void ParseNode(HtmlNode basenode)
        {
            if (basenode.NodeType == HtmlNodeType.Text)
            {
                Strings.Append(HtmlEntity.DeEntitize(basenode.InnerText).Trim());
                return;
            }

            if (basenode.NodeType == HtmlNodeType.Element)
            {
                switch (basenode.Name)
                {
                    case "br":
                        CommitText();
                        break;
                    case "img":
                        CommitImage(basenode.GetAttributeValue("src", null));
                        break;
                    case "p":
                        ParseNodeChildren(basenode);
                        CommitText();
                        break;
                    case "a":
                        CommitLink(basenode, basenode.GetAttributeValue("href", null));
                        break;
                    default:
                        ParseNodeChildren(basenode);
                        break;
                }
                return;
            }
        }

        private void ParseNodeChildren(HtmlNode basenode)
        {
            foreach (var node in basenode.ChildNodes)
            {
                ParseNode(node);
            }
        }

        private void CommitText()
        {
            if (Strings.Length > 0)
            {
                if (Links.Count > 0)
                {
                    Nodes.Add(new PostNode()
                    {
                        NodeType = "link",
                        HRef = Links.Peek(),
                        Text = Strings.ToString()
                    });
                }
                else
                {
                    Nodes.Add(new PostNode()
                    {
                        NodeType = "text",
                        HRef = "",
                        Text = Strings.ToString()
                    });
                }
                Strings.Clear();
            }
        }

        private void CommitImage(string src)
        {
            CommitText();
            
            Nodes.Add(new PostNode()
            {
                NodeType = "image",
                HRef = Links.Count > 0 ? Links.Peek() : "",
                Text = src
            });
        }

        private void CommitLink(HtmlNode basenode, string href)
        {
            CommitText();

            Links.Push(href == null ? "" : href);
            ParseNodeChildren(basenode);
            CommitText();
            Links.Pop();
        }

        public void PrintNode(HtmlNode rootnode)
        {
            PrintNode(rootnode, 0);
        }

        private void PrintNode(HtmlNode basenode, int level)
        {
            if (basenode.NodeType == HtmlNodeType.Text)
            {
                var text = HtmlEntity.DeEntitize(basenode.InnerText).Replace("\n", "").Replace("\r", "");
                if (text.Length > 20)
                {
                    text = text.Substring(0, 20) + "...";
                }

                for (var i = 0; i < level; i++)
                    Console.Write("  ");
                Console.WriteLine(text);
                return;
            }

            if (basenode.NodeType == HtmlNodeType.Element)
            {
                for (var i = 0; i < level; i++)
                    Console.Write("  ");
                Console.Write("<{0}", basenode.Name);
                foreach (var a in basenode.Attributes)
                {
                    Console.Write(" {0}=\"{1}\"", a.Name, a.Value);
                }
                if (basenode.ChildNodes.Count > 0)
                {
                    Console.WriteLine(">");
                    foreach (var n in basenode.ChildNodes)
                    {
                        PrintNode(n, level + 1);
                    }
                    for (var i = 0; i < level; i++)
                        Console.Write("  ");
                    Console.WriteLine("</{0}>", basenode.Name);
                }
                else
                {
                    Console.WriteLine("/>");
                }
                return;
            }
        }
    }
}
