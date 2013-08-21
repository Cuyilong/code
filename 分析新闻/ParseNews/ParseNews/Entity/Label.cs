using System;
using System.Collections.Generic;
using System.Text;

namespace ParseNews.Entity
{
    class Label
    {
        string labelName;
        int labelCount;
        int labelweight;

        DateTime time;
        public string LabelName
        {
            get { return labelName; }
            set { labelName = value; }

        }
        public int LabelNameCount
        {
            get { return labelCount; }
            set { labelCount = value; }
        }
        public int LabelWeight
        {
            get { return labelweight; }
            set { labelweight = value; }
        }

        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
        public int CompareTo(object obj)
        {
            if (obj is Label)
            {
                Label templabel = obj as Label;
                return this.LabelWeight.CompareTo(templabel.LabelWeight);
            }
            throw new NotImplementedException("搞错了呗");
        }
    }
}
