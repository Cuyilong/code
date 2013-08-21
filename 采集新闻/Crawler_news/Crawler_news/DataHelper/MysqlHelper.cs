using Crawler_news.Entity;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Crawler_news.DataHelper
{
    class MysqlHelper
    {
        private  String connStr = String.Format("server={0};user id={1}; password={2}; database=news; pooling=false", "localhost", "root", "kdnet");
        //private string connStr = String.Format("server={0};user id={1}; password={2}; database=kcis; pooling=false", "localhost", "root", "");
        
        //private string sel_lastInsert = @"select LAST_INSERT_ID()";
        public void UpdateInsertTime(string uriStr)
        {
            string error = null;
            string updateInsertTime_sql = string.Format("UPDATE info_news SET news_inserttime='{0}'  WHERE news_url='{1}'",DateTime.Now,uriStr);
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                error = Transaction(conn, updateInsertTime_sql);
                conn.Close();
            }
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally 
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 查找所有的种子
        /// </summary>
        internal List<MyUri> SelectAllSeeds()
        {
            List<MyUri> seeds = new List<MyUri>();
            string selectSeeds_sql = string.Format(@"select * 
                            from info_seed ");
            DataSet testDataSet = null;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {

                conn.Open();
                // 创建一个适配器
                MySqlDataAdapter adapter = new MySqlDataAdapter(selectSeeds_sql, conn);
                // 创建DataSet，用于存储数据.
                testDataSet = new DataSet();
                // 执行查询，并将数据导入DataSet.
                adapter.Fill(testDataSet, "result_data");
            }
            // 关闭数据库连接.
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally 
            {
                conn.Close();
            }
            if (testDataSet != null)
            {
                foreach (DataRow testRow in testDataSet.Tables["result_data"].Rows)
                {

                    string seedUrl = testRow["seed_url"].ToString();
                    int id = int.Parse(testRow["ID"].ToString());
                    if (seedUrl != "")
                    {
                        MyUri seed = new MyUri(seedUrl);
                        seed.Encoding = testRow["seed_encoding"].ToString();
                        seed.Id = id;
                        seed.Name = testRow["seed_name"].ToString();
                        seeds.Add(seed);
                    }
                }
            }
            return seeds;
        }
        /// <summary>
        /// 删除所有已访问记录
        /// </summary>
        internal void Del_All_Visited()
        {
            string error = null;
            string delAllVisited_sql = string.Format("DELETE FROM info_visited ");
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                error = Transaction(conn, delAllVisited_sql);
                conn.Close();
            }
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally 
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 事务处理
        /// </summary>
        /// <param name="conn"></param>
        private string Transaction(MySqlConnection conn, string sql)
        {
            string errorStr = null;
            MySqlTransaction t = conn.BeginTransaction();
            bool isTranSucceed = true;
            try
            {
                HandleSql(conn, t, sql);
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
                isTranSucceed = false;
                errorStr = e.Message;
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            return errorStr;
        }
        /// <summary>
        /// 插入数据.
        /// </summary>
        /// <param name="conn"></param>
        private void HandleSql(MySqlConnection conn, MySqlTransaction t, string sql)
        {
            // 创建一个 Command.
            MySqlCommand insertCommand = conn.CreateCommand();

            // 定义需要执行的SQL语句.
            insertCommand.CommandText = sql;
            // 注意： 只有加了这一句， 才能事务处理！！！
            insertCommand.Transaction = t;
            int insertRowCount = insertCommand.ExecuteNonQuery();
        }

        internal bool IsVisited(MyUri uri)
        {
            /// <summary>
            /// 判断帖子是否已经被访问过
            /// </summary>
            /// <param name="uri"></param>
            /// <returns></returns>
            int count = 0;

            string sel_forum = string.Format(@"select * from info_visited where visited_url = '{0}'", uri.AbsoluteUri);
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                
                // 创建一个适配器
                MySqlDataAdapter adapter = new MySqlDataAdapter(sel_forum, conn);
                // 创建DataSet，用于存储数据.
                DataSet testDataSet = new DataSet();
                // 执行查询，并将数据导入DataSet.
                adapter.Fill(testDataSet, "result_data");
                // 关闭数据库连接.
                conn.Close();
                if (testDataSet.Tables["result_data"].Rows.Count == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally {
                conn.Close();
            }
            return false;
        }
        /// <summary>
        /// 新闻链接是否已存在
        /// </summary>
        /// <param name="uriStr"></param>
        /// <returns></returns>
        public bool IsNewsExist(string uriStr)
        {
            DataSet testDataSet = null;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                string selectNews_sql = string.Format(@"select *  from info_news where news_url = '{0}'", uriStr);
                
                conn.Open();
                // 创建一个适配器
                MySqlDataAdapter adapter = new MySqlDataAdapter(selectNews_sql, conn);
                // 创建DataSet，用于存储数据.
                testDataSet = new DataSet();
                // 执行查询，并将数据导入DataSet.
                adapter.Fill(testDataSet, "result_data");
                // 关闭数据库连接.
                conn.Close();
            }
            catch (Exception e)
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally 
            {
                conn.Close();
            }
            if (testDataSet!=null && testDataSet.Tables["result_data"].Rows.Count != 0)
                return true;
            else 
                return false;
        }
        /// <summary>
        /// 将新闻插入数据库
        /// </summary>
        /// <param name="_news"></param>
        internal void InsertNews(News _news)
        {
            
            string insertPost_sql = string.Format(@"INSERT INTO info_news (FK_seed_news, news_url,news_time,news_insertTime,news_title,news_content,news_source,news_forum) 
            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",_news.Fk_seed_news,_news.Url,_news.Time,DateTime.Now,_news.Title,_news.Content,_news.Source,_news.Forum);


            string error = null;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                error = Transaction(conn, insertPost_sql);
                conn.Close();
            }
            catch (Exception e)
            {
                error = e.Message;
                Console.WriteLine("insert news error------>{0}", error);
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally
            {
                conn.Close();
            }

        }
        /// <summary>
        /// 插入已访问表记录
        /// </summary>
        /// <param name="_news"></param>
        internal void InsertVisited(News _news)
        {
            string errorStr = null;
            string ins_visited = string.Format(@"INSERT INTO info_visited  (visited_url,visited_time) VALUES('{0}','{1}')", _news.Url, DateTime.Now);
            bool isSucceedInsert = true;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                errorStr = Transaction_Visited(conn, ins_visited);
                conn.Close();
            }
            catch (Exception e)
            {
                errorStr = e.Message;
                Console.WriteLine("insert visited error--------->{0}", errorStr);
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally 
            {
                conn.Close();
            }
            //return errorStr;
        }
        /// <summary>
        /// 插入到visited表中的数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="insertVisited_sql"></param>
        /// <returns></returns>
        private string Transaction_Visited(MySqlConnection conn, string insertVisited_sql)
        {
            string errorStr = null;
            MySqlTransaction t = conn.BeginTransaction();
            try
            {
                InsertVisited(conn, t, insertVisited_sql);
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
                errorStr = e.Message;
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            return errorStr;
        }
        /// <summary>
        /// 插入Visited数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="t"></param>
        /// <param name="insert_sql"></param>
        private void InsertVisited(MySqlConnection conn, MySqlTransaction t, string insert_sql)
        {
            // 创建一个 Command.
            MySqlCommand insertCommand = conn.CreateCommand();
            insertCommand.CommandText = insert_sql;
            insertCommand.Transaction = t;
            int insertRowCount = insertCommand.ExecuteNonQuery();
        }

        internal void DelNews7DaysAgo()
        {
            string error = null;
            string del_sql = string.Format("DELETE FROM info_news WHERE  UNIX_TIMESTAMP(NOW()) +28800   -  UNIX_TIMESTAMP(news_insertTime)>24*3600*7");
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                error = Transaction(conn, del_sql);
                conn.Close();
            }
            catch (Exception e)
            {
                error = e.Message;
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(e.Message);
            }
            finally {
                conn.Close();
            }
        }
    }
}
