using System;
using System.Collections.Generic;
using System.Text;

namespace ParseNews.Entity
{
    class News
    {
        string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        string url;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        string content;


        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        string time;

        public string Time
        {
            get { return time; }
            set { time = value; }
        }
    }

}
