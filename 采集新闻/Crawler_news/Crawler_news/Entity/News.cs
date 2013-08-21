using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler_news.Entity
{
    class News
    {
        int id;
        int fk_seed_news;
        string url;
        string time;
        DateTime insertTime;
        string title;
        string content;
        string source;
        string forum;
        string summary;
        #region
        public string Forum
        {
            get { return forum; }
            set { forum = value; }
        }
        public string Summary
        {
            get { return summary; }
            set { summary = value; }
        }public string Source
        {
            get { return source; }
            set { source = value; }
        }public string Content
        {
            get { return content; }
            set { content = value; }
        }public string Title
        {
            get { return title; }
            set { title = value; }
        }public DateTime InsertTime
        {
            get { return insertTime; }
            set { insertTime = value; }
        }public string Time
        {
            get { return time; }
            set { time = value; }
        }public string Url
        {
            get { return url; }
            set { url = value; }
        }
        public int Fk_seed_news
        {
            get { return fk_seed_news; }
            set { fk_seed_news = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        #endregion
    }
}
