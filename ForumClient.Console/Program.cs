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
            var forumList = await client.GetForumList();
        }
    }
}
