using System;
using System.Threading.Tasks;

namespace ForumClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var sem = new System.Threading.Semaphore(0, 1);
            Run(sem, args);
            sem.WaitOne();
        }

        public static async void Run(System.Threading.Semaphore sem, string[] args)
        {
            try
            {
                if (args.Length >= 2)
                {
                    var config = Api.Config.LoadConfig(args[0]);
                    var client = new Api.Client(args[0], config);
                    if (args[1] == "forum")
                    {
                        var list = await client.GetForumList();
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                Console.WriteLine("Forum {0} {1}", item.Id, item.Name);
                            }
                        }
                    }
                    else if (args[1] == "thread")
                    {
                        if (args.Length >= 4)
                        {
                            var list = await client.GetForum(args[2], int.Parse(args[3]));
                            if (list != null)
                            {
                                foreach (var item in list)
                                {
                                    Console.WriteLine("Thread {0} {1} {2}", item.Id, item.PostAuthor, item.Title);
                                }
                            }
                        }
                    }
                    else if (args[1] == "post")
                    {
                        if (args.Length >= 4)
                        {
                            var list = await client.GetThread(args[2], int.Parse(args[3]));
                            if (list != null)
                            {
                                foreach (var item in list)
                                {
                                    Console.WriteLine("Post {0} {1}", item.PostAuthor, item.PostTime);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (args.Length >= 3)
                        {
                            var doc = await client.GetHtmlDocument(args[1], false);
                            if (doc != null)
                            {
                                var nodes = doc.DocumentNode.SelectNodes(args[2]);
                                if (nodes == null)
                                {
                                    Console.WriteLine("no match node");
                                }
                                else
                                {
                                    foreach (var node in nodes)
                                    {
                                        Console.WriteLine("== START [{0}]", node.XPath);
                                        new Api.NodeParser().PrintNode(node);
                                        Console.WriteLine("== END   [{0}]", node.XPath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception  : {0}", e.Message);
                Console.WriteLine("StackTrace : {0}", e.StackTrace);
            }
            sem.Release(1);
        }
    }
}
