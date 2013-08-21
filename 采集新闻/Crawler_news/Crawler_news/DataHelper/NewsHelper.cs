using Crawler_news.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler_news.DataHelper
{
   class NewsHelper
    {
        private static News _news;

        public NewsHelper()
        {
            _news = new News();
        }
        /// <summary>
        /// 处理新闻链接 
        /// 返回0：不操作超时
        /// 返回1：非kduri操作超时
        /// 返回2：kdurl操作超时
        /// </summary>
        /// <param name="oldUri"></param>
        internal static int ParseNewsUri(MyUri oldUri)
        {
            int result = 0;
            if (oldUri.AbsoluteUri.Equals(@"http://sports.sina.com.cn/g/laliga/2013-08-08/12216712337.shtml"))
            { 
            
            }
            string constructUri = string.Format(@"http://59.39.71.239:886/getnewscontent.aspx?password=Kcis123_AutoGetNewsContent&url={0}",oldUri.AbsoluteUri);
            MyUri newUri = new MyUri(constructUri);
            WebHelper webHelper = new WebHelper();

            string oldResponseStr = webHelper.GetContent(oldUri);
            string newResponseStr = webHelper.GetContent(newUri);
            if (oldResponseStr.Equals("操作超时"))
            {
                result = 1;
            }
            if (newResponseStr.Equals("操作超时"))
            {
                result = 2;
            }
            GetNewsInfo(oldResponseStr, newResponseStr,oldUri);
            InsertNews();
            InsertVisited();
            return result;
        }
        /// <summary>
        /// 插入已访问记录
        /// </summary>
        private static void InsertVisited()
        {
            if (_news != null)
            {
                MysqlHelper mysqlhelper = new MysqlHelper();
                mysqlhelper.InsertVisited(_news);
            }
            
        }
        /// <summary>
        /// 插入新闻纪录
        /// </summary>
        private static void InsertNews()
        {
            if (_news != null&&_news.Content!=null&&_news.Content.Trim()!="")
            {
                if (_news.Title == null || _news.Title == "")
                    Console.WriteLine(string.Format("{0}-->获取不到标题",_news.Url));
                if (_news.Content == null || _news.Content.Trim() == "")
                    Console.WriteLine(string.Format("{0}-->获取不到内容", _news.Url));
                if (_news.Forum == null || _news.Forum.Trim() == "")
                    Console.WriteLine(string.Format("{0}-->获取不到板块", _news.Url));
                if (_news.Time == null || _news.Time.Trim() == "")
                    Console.WriteLine(string.Format("{0}-->获取不到时间", _news.Url));
                MysqlHelper mysqlHelper = new MysqlHelper();
                mysqlHelper.InsertNews(_news);
                
            }
        }

        #region
        /// <summary>
        /// 获取新闻的信息
        /// </summary>
        /// <param name="newsContentStr"></param>
        private static void GetNewsInfo(string oldResponseStr,string newResponseStr,MyUri uri)
        {
            _news.Source = uri.Name;
            _news.Fk_seed_news = uri.Fk_seed;
            if (_news.Fk_seed_news == null || _news.Fk_seed_news == 0)
                Console.WriteLine("{0}的外键---〉{1}", uri.AbsoluteUri, uri.Fk_seed);
            _news.Url = uri.AbsoluteUri;
            if (_news.Url == null || _news.Url == "")
                Console.WriteLine("{0}的链接",uri.AbsoluteUri);
            if (newResponseStr != null&&newResponseStr!=""&&oldResponseStr!=null&&oldResponseStr!="")
            { 
                _news.Content = GetNewsContent(newResponseStr);
                _news.Title = GetNewsTitle(newResponseStr);
                //_news.Summary = GetNewsSummary(oldResponseStr);
                //_news.Source = GetNewsSource(oldResponseStr);                 //还没实现
                _news.Forum = GetNewsForumTrans(oldResponseStr,uri);
                _news.Time = GetNewsTimeTrans(oldResponseStr,uri);
            }
        }
        /// <summary>
        /// 新闻的板块获取的跳转
        /// </summary>
        /// <param name="oldResponseStr"></param>
        /// <returns></returns>
        private static string GetNewsForumTrans(string responseStr,MyUri uri)
        {
            string forum = null;
            if (uri.AbsoluteUri.Contains("sina.com"))
                forum = GetNewsForum(responseStr, 1);
            else if (uri.AbsoluteUri.Contains("qq.com"))
                forum = GetNewsForum(responseStr, 2);
            else if (uri.AbsoluteUri.Contains("sohu.com") )
                forum = GetNewsForum(responseStr, 3);
            else if(uri.AbsoluteUri.Contains("163.com") )
                forum = GetNewsForum(responseStr, 4);
            else if (uri.AbsoluteUri.Contains("ifeng.com"))
                forum = GetNewsForum(responseStr, 5);
                return forum;
        }

        private static string GetNewsForum(string responseStr,int style)
        {
            string forum = null;
            switch (style)
            {
                case 1:  //新浪  从正文往上找
                    string contantZhengwen = null;
                    MatchCollection matches2 = null;
                    MatchCollection matches1 = new Regex(RegexHelper.regexNewsSinaZhengwen).Matches(responseStr);
                    if (matches1 != null && matches1.Count != 0)
                        contantZhengwen = matches1[0].Value;
                    if(contantZhengwen!=null)
                        matches2 = new Regex(RegexHelper.regexNewsTextHasHref).Matches(contantZhengwen);
                    if (matches2 != null && matches2.Count != 0)
                        forum = matches2[matches2.Count - 1].Value;
                    else
                    {
                        matches1 = new Regex(RegexHelper.regexNewsForum_sinaShouYe).Matches(responseStr);
                        if (matches1 != null && matches1.Count != 0)
                        {
                            matches2 = new Regex(RegexHelper.regexNewsTextHasHref).Matches(matches1[0].Value);
                            if (matches2 != null && matches2.Count != 0)
                                for (int i = matches2.Count - 1; i >= 0; i--)
                                {
                                    if(matches2[i].Value.Trim()!="")
                                        forum = matches2[i].Value;
                                }
                        }
                    }
                    break;
                case 2:    //腾讯
                    string contentAfterTitle = null;
                    MatchCollection matches4 = null;
                    MatchCollection matches3 = new Regex(string.Format(@"更多(.*?)相关内容列表")).Matches(responseStr);
                    if (matches3 != null && matches3.Count != 0)
                    {   contentAfterTitle = matches3[0].Groups[1].Value;
                   
                                forum = contentAfterTitle;
                                break;
                            }
                    break;
                case 3:       //搜狐   考虑title标签与标签中的下划线
                    MatchCollection matches5 = null;
                    MatchCollection matches6 = null;
                    string contentBetwTitleLabel = null;
                    matches5 = new Regex(RegexHelper.regexNewsTitleLabel).Matches(responseStr);
                    if (matches5 != null && matches5.Count != 0)
                        contentBetwTitleLabel = matches5[0].Value;
                    matches6 = new Regex(RegexHelper.regexNewsForum_xiaHuaXian).Matches(contentBetwTitleLabel);
                    if (matches6 != null && matches6.Count != 0)
                        if (matches6[matches6.Count - 1].Value.Trim().Equals("搜狐"))
                            forum = matches6[matches6.Count - 2].Value;
                        else forum = matches6[matches6.Count - 1].Value;
                    break;
                case 4:  // 网易 从正文往上找
                    string contantZhengwen_163 = null;
                    MatchCollection matches8 = null;
                    MatchCollection matches7 = null; 
                    matches7 = new Regex(RegexHelper.regexNewsSinaZhengwen).Matches(responseStr);
                    if(matches7==null||matches7.Count==0)
                        matches7 = new Regex(RegexHelper.regexNews163Zhengwen).Matches(responseStr);
                    if (matches7 != null && matches7.Count != 0)
                        contantZhengwen_163 = matches7[0].Value;
                    if (contantZhengwen_163 != null)
                        matches8 = new Regex(RegexHelper.regexNewsTextHasHref).Matches(contantZhengwen_163);
                    if (matches8 != null && matches8.Count != 0)
                        forum = matches8[matches8.Count - 1].Value;
                    break;
                case 5:   //凤凰
                    string contantZhengwen_f = null;
                    MatchCollection matches9 = null;
                    MatchCollection matches10 = new Regex(RegexHelper.regexNewsSinaZhengwen).Matches(responseStr);
                    if (matches10 != null && matches10.Count != 0)
                        contantZhengwen_f = matches10[0].Value;
                    if(contantZhengwen_f!=null)
                        matches9 = new Regex(RegexHelper.regexNewsTextHasHref).Matches(contantZhengwen_f);
                    if(matches9!=null&&matches9.Count!=0)
                        forum = matches9[matches9.Count-1].Value;
                    break;
            }
            return forum;
        }
        private static string GetNewsTimeTrans(string responseStr,MyUri uri)
        {
            string time = null;
            if (uri.AbsoluteUri.Contains("163.com") || uri.AbsoluteUri.Contains("qq.com"))
                time = GetNewsTime(responseStr, 1);
            else if (uri.AbsoluteUri.Contains("sina.com"))
                time = GetNewsTime(responseStr, 2);
            else if (uri.AbsoluteUri.Contains("sohu.com") || uri.AbsoluteUri.Contains("ifeng.com"))
                time = GetNewsTime(responseStr,3);
            return time;
        }
        /// <summary>
        /// 分网站获取时间
        /// </summary>
        /// <param name="responseStr"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string GetNewsTime(string responseStr, int style)
        {
            string time = null;
            
            switch(style)
            {
                case 1:          //网易,腾讯
                    MatchCollection matches1 = new Regex(RegexHelper.regexNewsTime_2).Matches(responseStr);
                    if (matches1.Count == 0)
                        matches1 = new Regex(RegexHelper.regexNewsTime_1).Matches(responseStr);
                    if (matches1.Count != 0)
                        time = matches1[0].Value;
                    break;
                case 2:          //新浪
                   
                    MatchCollection matches2 = null;
                    MatchCollection matches3 = null;
                    if (_news.Title != null&&_news.Title != "")
                        matches2 = new Regex(string.Format(@"(?<={0}\s*)(<[^>]+>[^<>]*){1}", _news.Title, "{10}")).Matches(responseStr);
                    if (matches2!=null&&matches2.Count != 0) 
                        matches3 = new Regex(RegexHelper.regexNewsTextBetweenLabel).Matches(matches2[0].Value);
                    if (matches3 != null && matches3.Count != 0)
                        foreach (var match in matches3)
                        {
                            if (match.ToString().Trim() != ""&&match.ToString().Contains("年"))
                            {
                                time = match.ToString();
                                if (time.Contains(@"&nbsp;"))
                                    time = Regex.Replace(time, "&nbsp;","");
                                break;
                            }
                        }
                    else
                    {
                        matches3 = new Regex(RegexHelper.regexNewsTime_2).Matches(responseStr);
                        if (matches3 != null && matches3.Count != 0)
                            time = matches3[0].Value;
                    }
                    break;
                case 3:   //搜狐 凤凰网
                    MatchCollection matches4 = null;
                    matches4 = new Regex(RegexHelper.regexNewsTime_2).Matches(responseStr);
                    if (matches4 == null || matches4.Count == 0)
                        matches4 = new Regex(RegexHelper.regexNewsTime_ifengDuanping).Matches(responseStr);
                    if (matches4 != null && matches4.Count != 0)
                        time = matches4[0].Value;
                    break;
            }
            return time;
        }

        private static string GetNewsSource(string newsContentStr)
        {
            throw new NotImplementedException();
        }

        private static string GetNewsSummary(string newsContentStr)
        {
            throw new NotImplementedException();
        }

        private static string GetNewsTitle(string responseStr)
        {
            string title = null;
            MatchCollection matches = new Regex(RegexHelper.regexNewsTitle).Matches(responseStr);
            if (matches != null && matches.Count != 0)
                title = matches[0].Value;
            return title;
        }

        private static string GetNewsContent(string responseStr)
        {   
            MatchCollection matches = new Regex(RegexHelper.regexNewsTextBetweenLabel).Matches(responseStr);
            StringBuilder str=new StringBuilder();
            if (matches != null && matches.Count != 0)
                for (int i = 0; i < matches.Count;i++ )
                {
                    if (matches[i].Value != null && matches[i].Value != "")
                        str.Append(matches[i]);
                }
            string content = str.ToString();
            content = Regex.Replace(content, "'", "\\\'");
            return content;
        }
        #endregion
    }
    }
