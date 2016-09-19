using System;
using System.Threading.Tasks;

namespace ForumClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DoTest();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static async void DoTest()
        {
            try
            {

                var config = new Api.Config();
                config.LoadFromFile("/Users/gamemake/Projects/ForumClient/Data/hipda.json");
                var client = new Api.Client("hipda", config);

                //await client.SignIn("gamemake", "123456");

                var forumList = await client.GetForumList();
                foreach (var forum in forumList)
                {
                    Console.WriteLine("Forum {0} {1}", forum.Id, forum.Name);
                }

                var threadList = await client.GetForum("9", 1);
                foreach (var thread in threadList)
                {
                    Console.WriteLine("Thread {0} {1} {2}", thread.Id, thread.Author.Name, thread.Title);
                }

                var postList = await client.GetThread("448060", 1);
                foreach (var post in postList)
                {
                    Console.WriteLine("Post {0} {1}", post.Author, post.PostTime);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
