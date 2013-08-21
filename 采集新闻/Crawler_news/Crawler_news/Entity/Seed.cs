using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler_news.Entity
{
    class Seed
    {
        int id;
        string url;
        string name;
        DateTime insertTime;

        #region
        public DateTime InsertTime
        {
            get { return insertTime; }
            set { insertTime = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        #endregion
    }
}
