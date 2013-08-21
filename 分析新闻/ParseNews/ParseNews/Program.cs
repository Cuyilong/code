using ParseNews.DataHelper;
using ParseNews.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace ParseNews
{
    class Program
    {
        
        public static List<Person> persons = new List<Person>();
       
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("开始分析" + DateTime.Now);
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("{0}---〉{1}", "开始分析", DateTime.Now));
                
                MysqlHelper mysqlHelper =new MysqlHelper();
                string newscontent = mysqlHelper.SelectAllContent();
                if(newscontent == null)
                    Thread.Sleep(1000 * 60 * 50);
                LabelHelper labelHelper;
                List<Title> titles = GetTitle();
                SearchPersons(titles,newscontent);
                if (persons != null && persons.Count != 0)
                    labelHelper = new LabelHelper(persons,newscontent);
                log4net.ILog log2 = log4net.LogManager.GetLogger("MyLogger");
                log.Debug(string.Format("{0}---〉{1}","要分析的人数：", persons.Count.ToString()));
                Thread.Sleep(1000 * 60 * 60);
            }
        }



        private static List<Title> GetTitle()
        {   //这里读数据库
            MysqlHelper mysqlHelper = new MysqlHelper();

            List<Title> titleList = mysqlHelper.SelectAllTitle();
            return titleList;
        }
        private static void SearchPersons(List<Title> titles, string content)
        {
            //这里分析title，找person并且weight和int+1，并且将forum赋值给person
            SeparateHelper separateHelper = new SeparateHelper();
            List<string> names = new List<string>();
            foreach (Title title in titles)
            {
                names = separateHelper.GetNames(title.Content);
                if (names != null && names.Count != 0)
                    foreach (string name in names)
                    {


                        if (name.Length > 1 && !PersonHasExist(name, title))
                        {
                            Person person = new Person();
                            if (name.Length == 2 )
                            {
                                if (!PersonHasExist(FindRealName(name, content), title))
                                {
                                    person.PersonName = FindRealName(name, content);

                                    //}
                                    //else
                                    // person.PersonName = name; 

                                    person.Forum = title.TitleForum + ","; //addadd
                                    person.NameCount += 1;               //addadd
                                    person.Weight = 5;
                                    person.NewsIds.Add(title.Id);
                                    News news = new News();
                                    MysqlHelper mysqlHelper = new MysqlHelper();
                                    news = mysqlHelper.SelectNews(title.Id);
                                    person.News = news;
                                    persons.Add(person);
                                    if (person.PersonName.Equals("李某"))
                                    {
                                        log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                                        log.Debug(string.Format("{0}名字长度等于2", person.PersonName));
                                    }
                                }
                            }
                            else
                            {
                                person.PersonName = name;
                                person.Forum = title.TitleForum + ","; //addadd
                                person.NameCount += 1;               //addadd
                                person.Weight = 5;
                                person.NewsIds.Add(title.Id);
                                News news = new News();
                                MysqlHelper mysqlHelper = new MysqlHelper();
                                news = mysqlHelper.SelectNews(title.Id);
                                person.News = news;
                                persons.Add(person);
                                if (person.PersonName.Equals("李某"))
                                {
                                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                                    log.Debug(string.Format("{0}名字长度大于2",person.PersonName));
                                    log.Debug(string.Format("{0}名字长度为{1}", name,name.Length));
                                }

                            }
                        }
                    }
            }
        }

        private static string FindRealName(string name, string content)
        {


            List<NameLabel> realnamelist = new List<NameLabel>();

            MatchCollection mc = Regex.Matches(content, string.Format("{0}.", name));
            foreach (Match re in mc)
            {
                int count = 0;
                NameLabel namelist = new NameLabel();
                namelist.Name = re.Value;
                namelist.Count = 0;
                if (realnamelist.Count == 0)
                    realnamelist.Add(namelist);
                for (int i = 0; i < realnamelist.Count; i++)
                {
                    if (realnamelist[i].Name == namelist.Name)
                    {
                        realnamelist[i].Count++;
                        count++;
                    }
                }
                if (count == 0)
                {
                    namelist.Count = 1;
                    realnamelist.Add(namelist);
                }




            }
            realnamelist.Sort(Comparison);
            if (realnamelist.Count == 0)
            {
                if (name == "李某")
                {
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug("realnamelist是空的");
                }
                return name;
            }
                
            if (name == "李某")
            {
                log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                foreach(var nameLabel in realnamelist)
                    log.Debug(string.Format("{0}---〉{1},{2},{3}", nameLabel.Name,nameLabel.Count,mc.Count, (double)nameLabel.Count / (double)mc.Count));
            }
            if ((double)realnamelist[0].Count / (double)mc.Count > 0.6)
            {
                if (name == "李某")
                {
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug(string.Format("大于0.6的名字：{0}", realnamelist[0].Name));
                }
                return realnamelist[0].Name.Trim();
            }
            else
            {
                if (name == "李某")
                {
                    log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
                    log.Debug("第一个李某某比例没大于0.6");
                }
                return name;
            }

        }


        


        private static int Comparison(NameLabel l1, NameLabel l2)
        {
            return l2.Count- l1.Count;
        }
        /// <summary>
        /// 判断人名是不是已经存在，在的话更新
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool PersonHasExist(string name, Title title)
        {
            bool isExist = false;
            foreach (var person in persons)
                if (person.PersonName == name)
                {
                    person.NameCount++;
                    person.Weight += 5;
                    person.NewsIds.Add(title.Id);
                    if (!person.Forum.Contains(title.TitleForum))  //addadd
                        person.Forum += title.TitleForum + ",";   //addadd
                    isExist = true;
                }
            return isExist;
        }
    }
}
