using System;
using System.Collections.Generic;
using System.Text;

namespace ParseNews.Entity
{
    class Person
    {
        string personName;
        int nameCount;
        int weight;
        string labels;
        List<int> newsIds ;
        DateTime classifyTime;
        News news;

        internal News News
        {
            get { return news; }
            set { news = value; }
        }

        public DateTime ClassifyTime
        {
            get { return classifyTime; }
            set { classifyTime = value; }
        }
        public Person()
        {
            newsIds = new List<int>();
        }
        public List<int> NewsIds
        {
            get { return newsIds; }
            set { newsIds = value; }
        }
        DateTime time;
        string forum;
        string updateLabels;
        int titleID;

        public string Labels
        {
            get { return labels; }
            set { labels = value; }
        }
        public int TitleID
        {
            get { return titleID; }
            set { titleID = value; }
        }

        public string UpdateLabels
        {
            get { return updateLabels; }
            set { updateLabels = value; }
        }
        public string Forum
        {
            get { return forum; }
            set { forum = value; }
        }
        public string PersonName
        {
            get { return personName; }
            set { personName = value; }

        }
        public int NameCount
        {
            get { return nameCount; }
            set { nameCount = value; }
        }
        public int Weight
        {
            get { return weight; }
            set { weight = value; }
        }
        
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
    }
}
