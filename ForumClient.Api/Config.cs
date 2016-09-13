using System;
using System.Collections.Generic;

namespace ForumClient.Api
{
    public class Config
    {
        public class ConfigItem
        {
            public string _type;
            public string _id;
            public string _class;
        }

        public string base_url;
        public string login_url;
        public string login_cookie_check;
        public string login_error_messge;
        public string logout_url;
        public string forum_url;
        public string thread_url;
        public string post_url;

        public List<ConfigItem> forum_root = new List<ConfigItem>();
        public List<ConfigItem> forum_start = new List<ConfigItem>();
        public List<ConfigItem> forum_id = new List<ConfigItem>();
        public List<ConfigItem> forum_title = new List<ConfigItem>();
        public List<ConfigItem> forum_desc = new List<ConfigItem>();

        public List<ConfigItem> thread_root = new List<ConfigItem>();
        public List<ConfigItem> thread_start = new List<ConfigItem>();
        public List<ConfigItem> thread_default = new List<ConfigItem>();
        public List<ConfigItem> thread_id = new List<ConfigItem>();
        public List<ConfigItem> thread_title = new List<ConfigItem>();
        public List<ConfigItem> thread_post_auth_name = new List<ConfigItem>();
        public List<ConfigItem> thread_post_auth_id = new List<ConfigItem>();
        public List<ConfigItem> thread_post_time = new List<ConfigItem>();
        public List<ConfigItem> thread_last_auth_name = new List<ConfigItem>();
        public List<ConfigItem> thread_last_auth_id = new List<ConfigItem>();
        public List<ConfigItem> thread_last_time = new List<ConfigItem>();

        public List<ConfigItem> post_root = new List<ConfigItem>();
        public List<ConfigItem> post_start = new List<ConfigItem>();
        public List<ConfigItem> post_id = new List<ConfigItem>();
        public List<ConfigItem> post_auth_name = new List<ConfigItem>();
        public List<ConfigItem> post_auth_id = new List<ConfigItem>();
        public List<ConfigItem> post_time = new List<ConfigItem>();
        public List<ConfigItem> post_content_1 = new List<ConfigItem>();
        public List<ConfigItem> post_content_2 = new List<ConfigItem>();

        public static Config Load(string file)
        {
            return null;
        }
    }
}
