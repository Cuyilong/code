using Crawler_news.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Threading;

namespace Crawler_news.DataHelper
{
    class WebHelper
    {
        #region 私有变量
        /// <summary>
        /// 网页URL地址
        /// </summary>
        private static string url = null;
        /// <summary>
        /// 是否使用代码服务器：0 不使用  1 使用代理服务器
        /// </summary>
        private static int proxyState = 0;
        /// <summary>
        /// 代理服务器地址
        /// </summary>
        private static string proxyAddress = "proxy2.nfdaily.com";
        /// <summary>
        /// 代理服务器端口
        /// </summary>
        private static string proxyPort = "8080";
        /// <summary>
        /// 代理服务器用户名
        /// </summary>
        private static string proxyAccount = "zhangchi";
        /// <summary>
        /// 代理服务器密码
        /// </summary>
        private static string proxyPassword = "123789";
        /// <summary>
        /// 代理服务器域
        /// </summary>
        private static string proxyDomain = "nfdaily";

        #endregion


        /// <summary>
        /// 读取指定URL地址，获取内容
        /// </summary>
        public string GetContent(MyUri uri)
        {
            if (uri.AbsoluteUri == "http://comment.news.sohu.com/djpm/")
                return getHtml2(uri);
            else
                return getHtml1(uri);

        }
        private static string getHtml2(MyUri uri)
        {
            string read = null;
            HttpWebResponse response = null;
            HttpWebRequest wr = null;
            WebProxy proxy = new WebProxy("proxy2.nfdaily.com:8080", false);
            proxy.Credentials = new NetworkCredential(@"zhangchi", "123789", "nfdaily");
            StringBuilder s = new StringBuilder(102400);
            bool isGet = true;
            int time = 0;
            do
            {
                try
                {
                    wr = (HttpWebRequest)WebRequest.Create(uri.AbsoluteUri);
                    //wr.Timeout = 5000;
                    Thread.Sleep(30);
                    wr.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    //wr.Proxy = proxy;
                    DateTime now = DateTime.Now;
                    response = (HttpWebResponse)wr.GetResponse();
                    TimeSpan t = DateTime.Now - now;
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug(string.Format("{0}请求响应时间--->{1}",uri.AbsoluteUri, t.ToString()));
                    //Console.WriteLine(t);
                    //wr.Abort();
                    time++;
                }
                catch (Exception ex)
                {
                    isGet = false;
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug(string.Format("错误信息{0}--->{1}", uri.AbsoluteUri, ex.Message));
                    read = ex.Message;
                }
            } while (isGet == false && time < 7);
            string[] keys = response.Headers.AllKeys;
            GZipStream g = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
            StreamReader test = new StreamReader(g,Encoding.GetEncoding(uri.Encoding));
            if (read != null)
            {
                read = test.ReadToEnd();
                read = Regex.Match(read, "<style>.*<style>", RegexOptions.Singleline).Value;
            }
            //if (response != null)
            //    response.Close();
            //if (wr != null)
            //    wr.Abort();
            return read;

        }


        private static string getHtml1(MyUri uri)
        {
            string tempCode = null;
            string error = null;
            int time = 0;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            do                                                               //访问失败时重新访问，最多重新访问4次
            {
                WebProxy proxy = new WebProxy("proxy2.nfdaily.com:8080", false);
                proxy.Credentials = new NetworkCredential(@"zhangchi", "123789", "nfdaily");
                request = HttpWebRequest.Create(uri.AbsoluteUri) as HttpWebRequest;
                Thread.Sleep(30);
                //request.Timeout = 5000;
                //request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
                //request.AllowAutoRedirect = false;
                request.AllowAutoRedirect = true;
                //request.Proxy = proxy;
                int a = 0;
                time += 1;
                error = null;
                try
                {
                    DateTime now = DateTime.Now;
                    response = request.GetResponse() as HttpWebResponse;
                    TimeSpan t = DateTime.Now - now;
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug(string.Format("{0}请求响应时间--->{1}",uri.AbsoluteUri, t.ToString()));
                    //Console.WriteLine(t);
                    //request.Abort();
                }
                catch (Exception ex)
                {
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug(string.Format("错误信息{0}--->{1}", uri.AbsoluteUri, ex.Message));
                    tempCode = ex.Message;
                    //Console.WriteLine(ex.Message);
                    //error = ex.Message;
                    ////response = ex.Response as HttpWebResponse;
                    //Thread.Sleep(5 * 1000);
                }
            } while (error != null && time < 7);
            System.IO.Stream resStream = null;
            StreamReader sr = null;
            
            try
            {
                resStream = response.GetResponseStream();
                if (uri.AbsoluteUri.Contains("http://59.39.71.239:886/"))
                    sr = new StreamReader(resStream, Encoding.UTF8);
                else
                    sr = new StreamReader(resStream, Encoding.GetEncoding(uri.Encoding));
                tempCode = sr.ReadToEnd();
                response.Close();
                resStream.Close();
                sr.Close();
                if (uri.AbsoluteUri == "http://news.163.com/rank/")
                {
                    string pattern = @"<h2>全站</h2>.*<h2>科技</h2>";
                    tempCode = Regex.Match(tempCode, pattern, RegexOptions.Singleline).Value;
                }
                if (uri.AbsoluteUri == "http://news.qq.com/paihang.htm")
                {
                    tempCode = Regex.Match(tempCode, "<tbody>.*<tbody>", RegexOptions.Singleline).Value;
                }
                if (uri.AbsoluteUri == "http://news.ifeng.com/hotnews/")
                {
                    tempCode = Regex.Match(tempCode, "shtml.*<h4>资讯</h4>", RegexOptions.Singleline).Value;
                }
                if (uri.AbsoluteUri == "http://news.sina.com.cn/hotnews/")
                {
                    tempCode = Regex.Match(tempCode, "<!--seo内容输出开始-->.*<!--seo内容输出结束-->", RegexOptions.Singleline).Value;
                }
            }
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("错误信息{0}--->{1}", uri.AbsoluteUri, e.Message));
            }
            //if (response != null)
            //    response.Close();
            //if (request != null)
            //    request.Abort();
            return tempCode;
        }
    }
}

 