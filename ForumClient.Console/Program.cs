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
            var client = new Api.Client("http://www.hi-pda.com/");

            await client.SignIn("gamemake", "123456");

            var forumList = await client.GetForumList();
            foreach (var forum in forumList)
            {
                Console.WriteLine("Forum {0}", forum.Name);
            }

            var threadList = await client.GetForum("9", 1);
            foreach (var thread in threadList)
            {
                Console.WriteLine("Thread {0} {1} {2}", thread.Id, thread.Author.Name, thread.Title);
            }

            var postList = await client.GetThread("193801", 1);
            foreach (var post in postList)
            {
                Console.WriteLine("Post {0} {1}", post.Author, post.PostTime);
            }

            /*
            var postList = await client.GetThread("1343651", 1);
            for (int i = 0; i < postList.Count; i++)
            {
                Console.WriteLine("{0} {1}", i + 1, postList[i].Author.Name);
            }
            postList = await client.GetThread("1343651", 2);
            for (int i = 0; i < postList.Count; i++)
            {
                Console.WriteLine("{0} {1}", i + 1, postList[i].Author.Name);
            }
            */
        }
    }
}
