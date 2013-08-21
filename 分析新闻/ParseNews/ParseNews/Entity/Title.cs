using System;
using System.Collections.Generic;
using System.Text;

namespace ParseNews.Entity
{
    class Title
    {
        string titleContent;
        string titleForum;
        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Content
        {
            get { return titleContent; }
            set { titleContent = value; }
        }
        public string TitleForum
        {
            get { return titleForum; }
            set { titleForum = value; }
        }
    }
}
