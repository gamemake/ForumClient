using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ForumClient.Api
{
    public class ConfigItem : Dictionary<string, string>
    {
    }

    public class Config
    {
        public string text_encoder;
        public string base_url;
        public string login_url;
        public string logout_url;
        public string forumlist_url;
        public string threadlist_url;
        public string postlist_url;
        public string cookie_check;
        public string login_messge;

        public List<ConfigItem> forum_root = new List<ConfigItem>();
        public List<ConfigItem> forum_category = new List<ConfigItem>();
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
        public string post_time_left = "";
        public string post_time_right = "";
        public List<ConfigItem> post_content_1 = new List<ConfigItem>();
        public List<ConfigItem> post_content_2 = new List<ConfigItem>();
        public List<ConfigItem> post_content_ignore = new List<ConfigItem>();

        string GetItemString(JObject obj, string name)
        {
            var value = obj.GetValue(name);
            if (value != null)
            {
                if (value.Type == JTokenType.String)
                {
                    return value.ToString();
                }
            }
            return "";
        }

        List<ConfigItem> GetList(JArray data)
        {
            var retval = new List<ConfigItem>();
            for (int i = 0; i < data.Count; i++)
            {
                var obj = data[i] as JObject;
                var item = new ConfigItem();
                foreach (var pair in obj)
                {
                    if (pair.Value.Type == JTokenType.String)
                    {
                        item.Add(pair.Key, pair.Value.ToString());
                    }
                }
                retval.Add(item);
            }
            return retval;
        }

        public bool LoadFromStream(System.IO.StreamReader stream)
        {
            using (var json_stream = new JsonTextReader(stream))
            {
                var JsonRoot = (JObject)JToken.ReadFrom(json_stream);
                var retval = this;

                retval.text_encoder = JsonRoot["text_encoder"].ToString();
                retval.base_url = JsonRoot["base_url"].ToString();
                retval.login_url = JsonRoot["login_url"].ToString();
                retval.logout_url = JsonRoot["logout_url"].ToString();
                retval.forumlist_url = JsonRoot["forumlist_url"].ToString();
                retval.threadlist_url = JsonRoot["threadlist_url"].ToString();
                retval.postlist_url = JsonRoot["postlist_url"].ToString();
                retval.cookie_check = JsonRoot["cookie_check"].ToString();
                retval.login_messge = JsonRoot["login_messge"].ToString();

                var forum_list = JsonRoot["forum_list"] as JObject;
                retval.forum_root = GetList(forum_list["forum_root"] as JArray);
                retval.forum_category = GetList(forum_list["forum_category"] as JArray);
                retval.forum_start = GetList(forum_list["forum_start"] as JArray);
                retval.forum_id = GetList(forum_list["forum_id"] as JArray);
                retval.forum_title = GetList(forum_list["forum_title"] as JArray);
                retval.forum_desc = GetList(forum_list["forum_desc"] as JArray);

                var thread_list = JsonRoot["thread_list"] as JObject;
                retval.thread_root = GetList(thread_list["thread_root"] as JArray);
                retval.thread_start = GetList(thread_list["thread_start"] as JArray);
                retval.thread_default = GetList(thread_list["thread_default"] as JArray);
                retval.thread_id = GetList(thread_list["thread_id"] as JArray);
                retval.thread_title = GetList(thread_list["thread_title"] as JArray);
                retval.thread_post_auth_name = GetList(thread_list["post_auth_name"] as JArray);
                retval.thread_post_auth_id = GetList(thread_list["post_auth_id"] as JArray);
                retval.thread_post_time = GetList(thread_list["post_time"] as JArray);
                retval.thread_last_auth_name = GetList(thread_list["last_auth_name"] as JArray);
                retval.thread_last_auth_id = GetList(thread_list["last_auth_id"] as JArray);
                retval.thread_last_time = GetList(thread_list["last_time"] as JArray);

                var post_list = JsonRoot["post_list"] as JObject;
                retval.post_root = GetList(post_list["post_root"] as JArray);
                retval.post_start = GetList(post_list["post_start"] as JArray);
                retval.post_id = GetList(post_list["post_id"] as JArray);
                retval.post_auth_name = GetList(post_list["post_auth_name"] as JArray);
                retval.post_auth_id = GetList(post_list["post_auth_id"] as JArray);
                retval.post_time = GetList(post_list["post_time"] as JArray);
                retval.post_time_left = GetItemString(post_list, "post_time_left");
                retval.post_time_right = GetItemString(post_list, "post_time_right");
                retval.post_content_1 = GetList(post_list["content_1"] as JArray);
                retval.post_content_2 = GetList(post_list["content_2"] as JArray);
                retval.post_content_ignore = GetList(post_list["content_ignore"] as JArray);
                
                return true;
            }
        }

        public bool LoadFromFile(string file)
        {
            try
            {
                using (var file_stream = new System.IO.StreamReader(file, new System.Text.UTF8Encoding(false)))
                {
                    return LoadFromStream(file_stream);
                }
            }
            catch(Exception)
            {
            }
            return false;
        }
    }
}
