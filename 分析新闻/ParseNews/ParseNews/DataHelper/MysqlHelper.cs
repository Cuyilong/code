using MySql.Data.MySqlClient;
using ParseNews.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace ParseNews.DataHelper
{
    class MysqlHelper
    {
        private string connStr = String.Format("server={0};user id={1}; password={2}; database=news; pooling=false", "localhost", "root", "kdnet");
        internal List<Title> SelectAllTitle()
        {
            List<Title> titles = new List<Title>();
            string selectSeeds_sql = string.Format(@" SELECT ID, news_title,news_forum, news_insertTime FROM info_news WHERE  UNIX_TIMESTAMP(NOW()) +28800   -  UNIX_TIMESTAMP(news_insertTime)<24*3600");
            MySqlConnection conn = new MySqlConnection(connStr);
            // 创建一个适配器
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectSeeds_sql, conn);
            // 创建DataSet，用于存储数据.
            DataSet testDataSet = new DataSet();
            // 执行查询，并将数据导入DataSet.
            adapter.Fill(testDataSet, "result_data");
            // 关闭数据库连接.
            conn.Close();
            foreach (DataRow testRow in testDataSet.Tables["result_data"].Rows)
            {

                string titlename = testRow["news_title"].ToString();
                string titleForum = testRow["news_forum"].ToString();
                int id = int.Parse(testRow["ID"].ToString());
                DateTime insertTime = Convert.ToDateTime(testRow["news_insertTime"].ToString());
                TimeSpan jet = DateTime.Now - insertTime;
                if (titlename != "")
                //&&jet.TotalHours<=24)
                {
                    Title title = new Title();
                    title.Content = titlename;
                    title.TitleForum = titleForum;
                    title.Id = id;
                    titles.Add(title);
                }
            }
            return titles;
        }
        internal string SelectAllContent()
        {
            string allContent = null;
            string selectAllContent_sql = string.Format(@"select news_content from info_news where  UNIX_TIMESTAMP(NOW()) +28800 -UNIX_TIMESTAMP(news_insertTime)<24*3600");
            //
            MySqlConnection conn = new MySqlConnection(connStr);
            // 创建一个适配器
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectAllContent_sql, conn);
            // 创建DataSet，用于存储数据.
            DataSet testDataSet = new DataSet();
            // 执行查询，并将数据导入DataSet.
            adapter.Fill(testDataSet, "result_data");
            // 关闭数据库连接.
            conn.Close();
            foreach (DataRow testRow in testDataSet.Tables["result_data"].Rows)
            {
                string content = testRow["news_content"].ToString();
                allContent += content;
            }
            return allContent;
        }
        public News SelectNews(int newsId)
        {
            string selectAllContent_sql = string.Format(@"select * from info_news where ID='{0}'",newsId.ToString());
            //
            MySqlConnection conn = new MySqlConnection(connStr);
            // 创建一个适配器
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectAllContent_sql, conn);
            // 创建DataSet，用于存储数据.
            DataSet testDataSet = new DataSet();
            // 执行查询，并将数据导入DataSet.
            adapter.Fill(testDataSet, "result_data");
            // 关闭数据库连接.
            conn.Close();
            News news = new News();
            foreach (DataRow testRow in testDataSet.Tables["result_data"].Rows)
            {
                
                string content = testRow["news_content"].ToString();
                string url = testRow["news_url"].ToString();
                string title = testRow["news_title"].ToString();
                string time = testRow["news_time"].ToString();
                news.Content = content;
                news.Time = time;
                news.Title = title;
                news.Url = url;
            }
            return news;
        }

        internal void InsertPerson(Person oneperson)//将person插入数据库
        {
            oneperson.News.Content = Regex.Replace(oneperson.News.Content, "'", "\\'");
            string insertPost_sql = string.Format(@"INSERT INTO info_person (personName, nameCount,weight,label,time,news_forum,label2,groupTime,
            news_title,news_url,news_time,news_content) 
            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}','{8}','{9}','{10}','{11}')",
            oneperson.PersonName, oneperson.NameCount, oneperson.Weight, oneperson.Labels, DateTime.Now, oneperson.Forum,oneperson.UpdateLabels,oneperson.ClassifyTime,
            oneperson.News.Title,oneperson.News.Url,oneperson.News.Time,oneperson.News.Content);
            log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
            log.Debug(insertPost_sql);
            string error = null;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(insertPost_sql, conn);
                // 创建DataSet，用于存储数据.
                DataSet testDataSet = new DataSet();
                // 执行查询，并将数据导入DataSet.
                adapter.Fill(testDataSet, "result_data");
                conn.Close();
            }
            catch (Exception e)
            {
                error = e.Message;
                Console.WriteLine("insert news error------>{0}", error);
                log4net.ILog log2 = log4net.LogManager.GetLogger("MyLogger");
                log2.Debug(e.Message);
            }
        }

        internal string GetLabelContent(List<int> newsIds)
        {
            string contents = null;
            string content = null;
            foreach (var id in newsIds)
            {
                content = GetContent(id);
                if (content != null)
                    contents += content;
                content = null;
            }
            return contents;
        }

        private string GetContent(int id)
        {
            string contents = null;
            string selectContent_sql = string.Format(@"select news_content from info_news where  ID='{0}'",id);
            //
            MySqlConnection conn = new MySqlConnection(connStr);
            // 创建一个适配器
            MySqlDataAdapter adapter = new MySqlDataAdapter(selectContent_sql, conn);
            // 创建DataSet，用于存储数据.
            DataSet testDataSet = new DataSet();
            // 执行查询，并将数据导入DataSet.
            adapter.Fill(testDataSet, "result_data");
            // 关闭数据库连接.
            conn.Close();
            foreach (DataRow testRow in testDataSet.Tables["result_data"].Rows)
            {
                string content = testRow["news_content"].ToString();
                contents += content;
            }
            return contents;
        }
    }
}
