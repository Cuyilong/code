using System;
using System.Collections.Generic;
using System.Text;

namespace ParseNews.Entity
{
    class Sentence
    {
        string content;
        string personName;
        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        public string PersonName
        {
            get { return personName; }
            set { personName = value; }
        }
    }
}
