using Crawler_news.DataHelper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Crawler_news
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread deleteNews7DaysAgo = new Thread(DeleteFunction);
            deleteNews7DaysAgo.Start();
            Thread startCrawler = new Thread(StartFunction);
            startCrawler.Start();

        }

        private static void DeleteFunction(object obj)
        {
            while (true)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("开始采集{0}",DateTime.Now));
                MysqlHelper mysqlHelper = new MysqlHelper();
                mysqlHelper.DelNews7DaysAgo();
                Thread.Sleep(1000*60*60*24*7);
            }
        }

        private static void StartFunction(object obj)
        {
            while (true)
            {
                VisitedHelper.DelAllVisited();
                SeedHelper seedHelper = new SeedHelper();
                Thread.Sleep(3600000);
            }
        }
    }
}
