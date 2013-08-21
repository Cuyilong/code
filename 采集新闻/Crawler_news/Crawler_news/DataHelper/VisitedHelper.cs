using Crawler_news.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler_news.DataHelper
{
    public static class VisitedHelper
    {

        internal static void DelAllVisited()
        {
            MysqlHelper helper = new MysqlHelper();
            helper.Del_All_Visited();
        }
        public static bool IsVisited(MyUri uri)
        {
            MysqlHelper helper = new MysqlHelper();
            return helper.IsVisited(uri);
        }
    }
}
