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
        public string user_agent;
        public string base_url;
        public string login_url;
        public string logout_url;
        public string forumlist_url;
        public string threadlist_url;
        public string postlist_url;
        public string cookie_check;
        public string login_messge;

        public string forum_node;
        public string forum_id;
        public string forum_name;
        public string forum_desc;

        public string thread_node;
        public string thread_ontop;
        public string thread_id;
        public string thread_title;
        public string thread_post_author;
        public string thread_post_time;
        public string thread_last_author;
        public string thread_last_time;

        public string post_node;
        public string post_id;
        public string post_author;
        public string post_time;
        public string post_content;
        public string post_ignore;
        public string post_page_end;
        public string post_page_next;

        public void LoadFromText(string text)
        {
            var lines = text.Replace('\r', ' ').Split(new char[] { '\n' });
            foreach (var line in lines)
            {
                string t = line;
                var pos = line.IndexOf('#');
                if (pos >= 0) t = t.Substring(0, pos);
                t = t.Trim();
                if (t.Length > 0)
                {
                    pos = t.IndexOf('=');
                    if (pos > 0)
                    {
                        var name = t.Substring(0, pos).Trim();
                        var value = t.Substring(pos + 1).Trim();
                        var field = this.GetType().GetField(name);
                        if (field != null)
                        {
                            field.SetValue(this, value);
                        }
                    }
                }
            }
        }
    }
}
