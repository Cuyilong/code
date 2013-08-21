using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler_news.Entity
{
    public class MyUri:System.Uri
    {
        public MyUri(string uriString)
            : base(uriString)
        {
        }

        private string encoding;
        public int Depth;
        int id;
        int fk_seed;
        private bool isVisited;
        string name;
        #region
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public string Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Fk_seed
        {
            get { return fk_seed; }
            set { fk_seed = value; }
        }

        public bool IsVisited
        {
            get { return isVisited; }
            set { isVisited = value; }
        }
        #endregion
        //private bool canSave;

        //public bool CanSave
        //{
        //    get { return canSave; }
        //    set { canSave = value; }
        //}
        
        //Int64 fk_forum;

        //public Int64 Fk_forum
        //{
        //    get { return fk_forum; }
        //    set { fk_forum = value; }
        //}
        
    }
}
