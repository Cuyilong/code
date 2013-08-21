using Crawler_news.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Crawler_news.DataHelper
{
    class SeedHelper
    {
        int _allKdUrls = 0;
        int _timeOutKdUrl = 0;
        int _timeOutNormalUrl = 0;
        private Queue<MyUri> _UriSeedQueue;
        private Queue<MyUri> _UriNewsQueue;
        private Thread[] _threadsSeed;
        private Thread[] _threadsNews;
        DateTime _lastDequeueNewsTime;
        public SeedHelper()
        {
            _UriSeedQueue = new Queue<MyUri>();
            _UriNewsQueue = new Queue<MyUri>();
            InitSeeds();
            _lastDequeueNewsTime = DateTime.Now;
            CreateThreads_seed();
            CreateThreads_News();
        }
        /// <summary>
        /// 创建处理新闻链接的线程
        /// </summary>
        private void CreateThreads_News()
        {
            _threadsNews = new Thread[40];
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    if (_threadsNews[i] == null || _threadsNews[i].ThreadState != ThreadState.Suspended)
                    {
                        _threadsNews[i] = new Thread(ThreadFunction_news);
                        _threadsNews[i].Start();
                    }
                }
            }
            catch (ThreadStartException)
            { }
        }
        /// <summary>
        /// 处理新闻链接的线程运行时执行的函数
        /// </summary>
        /// <param name="obj"></param>
        private void ThreadFunction_news(object obj)
        {
            while (true)
            {
                MyUri newsUri = DequeueNewsUri();
                if (newsUri != null)
                {
                    NewsHelper newsHelper = new NewsHelper();
                    int result = NewsHelper.ParseNewsUri(newsUri);
                    _allKdUrls += 1;
                    if (result == 1)
                        _timeOutNormalUrl += 1;
                    else if (result == 2)
                        _timeOutKdUrl += 1;
                }
                else
                    if (isFinished())
                    {
                        AbordNewsThreads();
                        break;
                    }
                    else
                        Thread.Sleep(1 * 1000);
            }
            int threadLiving = 0;
            foreach (var thread in _threadsNews)
                if (thread.ThreadState == ThreadState.Aborted || thread.ThreadState == ThreadState.Stopped)
                    threadLiving++;
            if (threadLiving <= 1)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("结束采集{0}", DateTime.Now));
                log.Debug(string.Format("全部链接数{0}", _allKdUrls));
                log.Debug(string.Format("全部链接数{0}", _allKdUrls));
                log.Debug(string.Format("kd超时链接数{0}", _timeOutKdUrl));
                log.Debug(string.Format("门户超时链接数{0}", _timeOutNormalUrl));
            }
        }
        /// <summary>
        /// 判断是否已处理完
        /// </summary>
        /// <returns></returns>
        private bool isFinished()
        {
            TimeSpan ds = DateTime.Now.Subtract(_lastDequeueNewsTime);  
            if (ds.Minutes>0&&ds.Seconds > 30 && _UriSeedQueue.Count==0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 获取一个新的新闻链接
        /// </summary>
        /// <returns></returns>
        private MyUri DequeueNewsUri()
        {
            Monitor.Enter(_UriNewsQueue);
            MyUri newsUri = null;
            if (_UriNewsQueue.Count != 0)
            {
                newsUri = _UriNewsQueue.Dequeue();
                _lastDequeueNewsTime = DateTime.Now;
            }   
            Monitor.Exit(_UriNewsQueue);
            return newsUri;
        }
        /// <summary>
        /// 生成处理种子链接的多个线程
        /// </summary>
        private void CreateThreads_seed()
        {
            _threadsSeed = new Thread[5];
            for (int i = 0; i < 1; i++)
            {
                if (_threadsSeed[i] == null || _threadsSeed[i].ThreadState != ThreadState.Suspended)
                {
                    _threadsSeed[i] = new Thread(ThreadFunction_seed);
                    _threadsSeed[i].Start();
                }
            }
        }
        /// <summary>
        /// 处理种子链接的线程运行时执行的函数
        /// </summary>
        /// <param name="obj"></param>
        private void ThreadFunction_seed(object obj)
        {
            while (true)
            {
                MyUri uri = DequeueSeedUri();
                if (uri != null)
                {
                    Thread.Sleep(1 * 1000);
                    ParseSeedUri(uri);
                }
                else
                    if (IsSeedHandled())
                    {
                        AbordSeedThreads();
                        break;
                    }
                    else
                        Thread.Sleep(1 * 1000);
            }
        }
        /// <summary>
        /// 判断种子的处理是不是已经结束
        /// </summary>
        /// <returns></returns>
        private bool IsSeedHandled()
        {
            if (_UriSeedQueue.Count == 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 停止处理种子的线程
        /// </summary>
        private void AbordSeedThreads()
        {
            foreach (var thread in _threadsSeed)
            {
                if (thread != null&&thread.ThreadState!=ThreadState.Running)
                    thread.Abort();
            }
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="uri"></param>
        private void ParseSeedUri(MyUri seedUri)
        {
            WebHelper webHelper = new WebHelper();
            string contentStr = webHelper.GetContent(seedUri);
            if (contentStr == null)
            {
                Console.WriteLine("获取不到种子列表信息");
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("{0}--->{1}", "获取不到种子列表信息",seedUri.AbsoluteUri));
            }
            else
            {
                List<MyUri> newsUris = GetNewsUri(contentStr, seedUri);
                Console.WriteLine(seedUri.AbsoluteUri + "--->" + newsUris.Count + "--->" + DateTime.Now);
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(seedUri.AbsoluteUri + "--->" + newsUris.Count + "--->" + DateTime.Now);
                EnqueueNewsUris(newsUris);
            }
        }
        /// <summary>
        /// 将新闻链接入队列
        /// </summary>
        /// <param name="newsUris"></param>
        private void EnqueueNewsUris(List<MyUri> newsUris)
        {
            Monitor.Enter(_UriNewsQueue);
            foreach(var newsUri in newsUris)
                _UriNewsQueue.Enqueue(newsUri);
            Monitor.Exit(_UriNewsQueue);
        }
        /// <summary>
        /// 获取新闻链接
        /// </summary>
        /// <param name="contentStr"></param>
        /// <returns></returns>
        private List<MyUri> GetNewsUri(string contentStr, MyUri uri)
        {

            string content = Regex.Replace(contentStr, @"<script(\s[^>]*?)?>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            string strRef = @"(href|HREF|src|SRC)[ ]*=[ ]*[""'][^""'#>]+[""']";
            List<MyUri> matchcollections = new List<MyUri>();

            MatchCollection matches = new Regex(strRef).Matches(content);

            foreach (Match match in matches)
            {
                strRef = GetRef(match);
                MyUri newUri = GetNewUri(strRef, uri);
                newUri.IsVisited = IsVisited(newUri);
                if (!newUri.IsVisited && Isvalueable(newUri))
                {
                    newUri.Name = uri.Name;
                    newUri.Encoding = uri.Encoding;
                    newUri.Fk_seed = uri.Id;
                    matchcollections.Add(newUri);
                }
            }
            return matchcollections;
        }
               
        /// <summary>
        /// 判断链接是否有价值，包括是否已被访问过
        /// </summary>
        /// <param name="newUri"></param>
        /// <returns></returns>
        private bool Isvalueable(MyUri newUri)
        {
            
            bool isNewsExist = false;              
            bool isValueable = false;
            MysqlHelper mysqlHelper = new MysqlHelper();
            isNewsExist = mysqlHelper.IsNewsExist(newUri.AbsoluteUri);
            if (!isNewsExist)
            {
                string[] ContainArray = { ".shtml", "html", "htm" };
                foreach (string contain in ContainArray)
                    if (newUri.AbsoluteUri.EndsWith(contain) &&
                        (!newUri.AbsoluteUri.Contains(@"163.com/special")) &&
                        (!newUri.AbsoluteUri.Contains(@"http://comment.ifeng.com/")) &&
                        (!newUri.AbsoluteUri.Contains(@"http://comment2.news.sohu.com")) &&
                        (!newUri.AbsoluteUri.Contains(@"http://news.ifeng.com/photo")) &&
                        (!newUri.AbsoluteUri.Contains(@"http://slide.mil.news.sina.com.cn")) &&
                        (!newUri.AbsoluteUri.Contains(@"slide.news.sina")) &&

                        (!newUri.AbsoluteUri.Contains("video.sina")))
                    {
                        isValueable = true;
                        break;
                    }
            }
            else
            { UpdateUri(newUri); }
            return isValueable;
        }
        /// <summary>
        /// 更新新闻纪录的插入时间
        /// </summary>
        /// <param name="newUri"></param>
        private void UpdateUri(MyUri newUri)
        {
            MysqlHelper mysqlHelper = new MysqlHelper();
            mysqlHelper.UpdateInsertTime(newUri.AbsoluteUri);
        }

        private bool IsVisited(MyUri newUri)
        {
            return VisitedHelper.IsVisited(newUri);
        }

        private MyUri GetNewUri(string strRef, MyUri uri)
        {
            if (strRef.IndexOf("..") != -1 || strRef.StartsWith("/") == true || strRef.StartsWith("http://") == false)
                strRef = new Uri(uri, strRef).AbsoluteUri;

            MyUri newUri = new MyUri(strRef);

            newUri.Depth = uri.Depth + 1;
            return newUri;
        }
        private string GetRef(Match match)
        {
            string strRef;
            strRef = match.Value.Substring(match.Value.IndexOf('=') + 1).Trim('"', '\'', '#', ' ', '>');
            return strRef;
        }
        


        

        /// <summary>
        /// 停止处理新闻的线程
        /// </summary>
        /// 
        private void AbordNewsThreads()
        {
            foreach (var thread in _threadsNews)
            {
                if (thread != null&&thread.ThreadState!=ThreadState.Running)
                    thread.Abort();
            }
        }
        /// <summary>
        /// 从种子队列中出链接
        /// </summary>
        /// <returns></returns>
        private MyUri DequeueSeedUri()
        {
            MyUri uri = null;
            Monitor.Enter(_UriSeedQueue);
            try
            {
                uri = (MyUri)_UriSeedQueue.Dequeue();
            }
            catch (Exception)
            {
            }
            Monitor.Exit(_UriSeedQueue);
            return uri;
        }
        /// <summary>
        /// 获取所有的种子链接
        /// </summary>
        private void InitSeeds()
        {
            MysqlHelper mysqlHelper = new MysqlHelper();
            List<MyUri> seeds = mysqlHelper.SelectAllSeeds();
            if (seeds.Count != 0)
            {
                Monitor.Enter(_UriSeedQueue);
                foreach (var uri in seeds)
                {
                    _UriSeedQueue.Enqueue(uri);
                }
                Monitor.Exit(_UriSeedQueue);
            }
        }
    }
}
