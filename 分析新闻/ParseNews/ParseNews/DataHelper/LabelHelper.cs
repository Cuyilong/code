using ParseNews.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ParseNews.DataHelper
{
    class LabelHelper
    {
        private List<Entity.Person> person;
        private Thread[] _thread_person;
        private Queue<Person> _personQueue;
        public string content;
        private static SeparateHelper _separateHelper = new SeparateHelper();
        List<Person> _persons = new List<Person>();

        public LabelHelper(List<Entity.Person> persons, string contents)
        {
            _personQueue = new Queue<Person>();
            MysqlHelper mysqlHelper = new MysqlHelper();
            content = contents;              //数据库获取全部内容
            EnqueuePerson(persons);
            _thread_person = new Thread[20];
            try
            {
                for (int i = 0; i < 1; i++)
                {
                    if (_thread_person[i] == null || _thread_person[i].ThreadState != ThreadState.Suspended)
                    {
                        _thread_person[i] = new Thread(ThreadFunction);
                        _thread_person[i].Start();
                    }
                    //List<Label> personlabel = ParseSentence(GetSentences(oneperson, content));
                    //string labels = Getlabel(personlabel);
                    //oneperson.Labels = labels;
                    //oneperson.Time = DateTime.Now;
                    //mysqlHelper.InsertPerson(oneperson);
                }
            }

            catch (ThreadStartException)
            { }
        }

        private void ThreadFunction()
        {
            MysqlHelper mysqlHelper = new MysqlHelper();
            while (true)
            {
                Person oneperson = DequeuePerson();
                if (oneperson != null)
                {
                    List<Label> personlabels = new List<Label>();
                    Console.WriteLine("------->"+oneperson.PersonName);
                    List<Sentence> sentences = GetSentences(ref oneperson, content);
                    personlabels = ParseSentence(sentences);
                    string labels = null;
                    string updateLabels = null;
                    if (personlabels != null && personlabels.Count != 0)
                        labels = GetLabelStr(personlabels);
                    if(personlabels!=null&&personlabels.Count!=0)
                        updateLabels = GetUpdateLabels(personlabels,oneperson.NewsIds);
                    oneperson.Labels = labels;
                    oneperson.UpdateLabels = updateLabels;
                    oneperson.Time = DateTime.Now;
                    
                    if (labels != null)
                        _persons.Add(oneperson);
                    else
                        Console.WriteLine(oneperson.PersonName);
                }
                else
                    {
                        break;
                    }
            }
            int i = 0;
            foreach (var thread in _thread_person)
            {
                if (thread != null&&thread.ThreadState == ThreadState.Running)
                    i++;
            }
            if (i == 1)
            {
                DateTime now = DateTime.Now;
                foreach (var person in _persons)
                {
                    int year = now.Year;
                    int month = now.Month;
                    int date = now.Day;
                    int hour = now.Hour;
                    DateTime datetime = new DateTime(year, month, date, hour, 0, 0);
                    person.ClassifyTime = datetime; 
                    mysqlHelper.InsertPerson(person);
                }
                Console.WriteLine("分析结束-------------------〉"+DateTime.Now);
            }
                
        }
        /// <summary>
        /// 获取更新的标签
        /// </summary>
        /// <param name="personlabels"></param>
        /// <param name="sentences"></param>
        /// <returns></returns>
        private string GetUpdateLabels(List<Label> personlabels, List<int> newsIds)
        {
            string updateLabels = null;
            if(personlabels!=null)
            {
                MysqlHelper mysqlHelper = new MysqlHelper ();
                string content = mysqlHelper.GetLabelContent(newsIds);

                for (int i = 0; i < personlabels.Count;i++ )
                {
                    List<Sentence> sentences = GetLabelSentences(personlabels[i].LabelName, content);
                    string updateLabel = GetUpdateLabel(personlabels[i].LabelName, sentences);
                    if (IsUpdateLabelValuabel(updateLabel))
                        updateLabels += updateLabel + ",";
                }
            }
            return updateLabels;
        }
        /// <summary>
        /// 判断新获得的标签有没有价值
        /// </summary>
        /// <param name="updateLabel"></param>
        /// <returns></returns>
        private bool IsUpdateLabelValuabel(string updateLabel)
        {
            bool isValuable = true;
            SeparateHelper sep = new SeparateHelper();
            bool hasPunctuation = sep.IsHasPunctuation(updateLabel);
            if (updateLabel == null || updateLabel.Trim() == "" || hasPunctuation)
                isValuable = false;
            return isValuable;
        }
        /// <summary>
        /// 获取更新的标签
        /// </summary>
        /// <param name="labelStr"></param>
        /// <param name="sentences"></param>
        /// <returns></returns>
        private string GetUpdateLabel(string oldLabel, List<Sentence> sentences)
        {
            string newLabelStr = null;
            List<Label> labels = new List<Label>();
            List<Label> updateLabels = new List<Label> ();
            for (int sentenceIndex = 0; sentenceIndex < sentences.Count; sentenceIndex++)
            {
                for (int nextIndex = sentenceIndex + 1; nextIndex < sentences.Count; nextIndex++)
                {
                    Label label = null;
                    newLabelStr = GetNewLabel(oldLabel, sentences[sentenceIndex].Content, sentences[nextIndex].Content);
                    if (newLabelStr != null && newLabelStr.Length > 0)
                    {
                        label = new Label ();
                        label.LabelNameCount = 1;
                        label.LabelName = newLabelStr;
                        labels.Add(label);
                    }
                }
            }
            labels.Sort(Comparison1);
            if (labels.Count > 0)
                return labels[0].LabelName;
            else
                return null;
            //AddToLabels(labels, ref updateLabels);

        }
        private static string GetNewLabel(string oldLabel, string sentence1, string sentence2)
        {
            char[] oldLabels = oldLabel.ToCharArray();
            char[] sentence_1 = sentence1.ToCharArray();
            char[] sentence_2 = sentence2.ToCharArray();
            int startIndex = 0;
            int LabelLength = 0;
            //char[] newLable = new char[20];
            int labelStartIndex_1 = GetLabelStartIndex(oldLabels, sentence_1);
            int labelStartIndex_2 = GetLabelStartIndex(oldLabels, sentence_2);
            int labelEndIndex_1 = labelStartIndex_1 + oldLabel.Length - 1;
            int labelEndIndex_2 = labelStartIndex_2 + oldLabel.Length - 1;
            int leftIndex_1 = -1;
            int rightIndex_1 = -1;
            for (int i = 0; (labelStartIndex_1 - i) >= 0 && (labelStartIndex_2 - i) >= 0; i++)
            {
                if (sentence_1[labelStartIndex_1 - i] == sentence_2[labelStartIndex_2 - i])
                    leftIndex_1 = labelStartIndex_1 - i;
                else
                    break;
            }
            for (int i = 0; (labelEndIndex_1 + i) < sentence1.Length && (labelEndIndex_2 + i) < sentence2.Length; i++)
            {
                if (sentence_1[labelEndIndex_1 + i] == sentence_2[labelEndIndex_2 + i])
                    rightIndex_1 = labelEndIndex_1 + i;
                else
                    break;
            }
            char[] newLabels = null;
            if (leftIndex_1 >= 0 && rightIndex_1 >= 0)
                newLabels = sentence1.ToCharArray(leftIndex_1, rightIndex_1 - leftIndex_1 + 1);
            StringBuilder sb = new StringBuilder();
            if(newLabels!=null)
            foreach (var achar in newLabels)
                sb.Append(achar);
            return sb.ToString();
        }
        /// <summary>
        /// 获取标签开始的索引
        /// </summary>
        /// <param name="label"></param>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private static int GetLabelStartIndex(char[] label, char[] sentence)
        {
            int LabelStartIndex = -1;
            int labelLength = label.Length;
            for (int sentenceIndex = 0; sentenceIndex < sentence.Length; sentenceIndex++)
                for (int labelIndex = 0, sentenceMoveIndex = sentenceIndex; labelIndex < label.Length && sentenceMoveIndex < sentence.Length; labelIndex++, sentenceMoveIndex++)
                    if (label[labelIndex] == sentence[sentenceMoveIndex])
                    {
                        if (labelIndex == labelLength - 1)
                            LabelStartIndex = sentenceIndex;
                    }
                    else
                        break;
            return LabelStartIndex;
        }
        /// <summary>
        /// 去掉互相包含关系中长度小的标签
        /// </summary>
        /// <param name="personLabels"></param>
        /// <returns></returns>
        private string GetLabelStr(List<Label> personLabels)
        {
            string labels = null;
            bool isContains = false;
            for (int index = 0; index < personLabels.Count; index++) 
            {
                for (int nextIndex = 1; nextIndex < personLabels.Count; nextIndex++)
                {
                    if (personLabels[nextIndex].LabelName.Length > personLabels[index].LabelName.Length)
                        if (personLabels[nextIndex].LabelName.Contains(personLabels[index].LabelName))
                            isContains = true;
                }
                if (!isContains)
                    labels += personLabels[index].LabelName + ",";
                else
                    isContains = false;
            }
                
            return labels;
        }
        private int Comparison(Label l1, Label l2)
        {
            return l2.LabelWeight - l1.LabelWeight;
        }
        private int Comparison1(Label l1, Label l2)
        {
            return l2.LabelNameCount - l1.LabelNameCount;
        }
        private List<Label> ParseSentence(List<Sentence> list)
        {
            List<Label> temp = new List<Label>();
            List<string> labelstrs = new List<string>();
            Monitor.Enter(_separateHelper);
            SeparateHelper separateHelper = new SeparateHelper ();
            foreach (Sentence sentence in list)
            {
                //labelstrs = _separateHelper.GetLabels(sentence.Content);
                labelstrs = separateHelper.GetLabels(sentence.Content);
                AddToLabels(TrunToLabel(labelstrs, sentence.PersonName), ref temp);
            }
            Monitor.Exit(_separateHelper);
            temp.Sort(Comparison);
            List<Label> labels = new List<Label>();
            for (int i = 0; i < temp.Count && i < 10; i++)
                labels.Add(temp[i]);
            return labels;
        }

        private List<Label> TrunToLabel(List<string> labelstrs, string personName)
        {
            List<Label> labels = new List<Label>();
            int personIndex = -1;
            if (labelstrs != null && labelstrs.Count != 0)
                for (int i = 0; i < labelstrs.Count; i++)
                {
                    if (labelstrs[i] == personName)
                        personIndex = i;
                }
            if (personIndex != -1)
            {
                for (int i = personIndex, j = 0; i >= 0; i--, j++)
                {
                    Label label = new Label();
                    label.LabelName = labelstrs[i];
                    label.LabelWeight = 10 - j;
                    if (label.LabelName != personName)
                        labels.Add(label);
                }
                for (int i = personIndex, j = 0; i < labelstrs.Count; i++, j++)
                {
                    Label label = new Label();
                    label.LabelName = labelstrs[i];
                    label.LabelWeight = 10 - j;
                    if (label.LabelName != personName)
                        labels.Add(label);
                }
            }
            else labels = null;
            return labels;

        }

        private void AddToLabels(List<Label> newLabels, ref List<Label> oldLabels)
        {
            int i = 0;
            if (newLabels != null && newLabels.Count != 0)
            {
                foreach (var newLabel in newLabels)
                {
                    foreach (var oldLabel in oldLabels)
                    {
                        if (oldLabel.LabelName == newLabel.LabelName)
                        {
                            oldLabel.LabelWeight += newLabel.LabelWeight;
                            oldLabel.LabelNameCount++;
                            i = 1;
                        }
                    }
                    if (i == 0)
                    {
                        oldLabels.Add(newLabel);
                    }
                    else
                        i = 0;
                }
            }
        }

        private List<Sentence> GetLabelSentences(string label, string content)
        {
            List<Sentence> sentences = new List<Sentence>();
            try
            {
                MatchCollection matches = Regex.Matches(content, string.Format(@"(?<=[。！”])[^。！”]+{0}[^。！”]+(?=[。！”])", label), RegexOptions.Singleline);
                if (matches != null && matches.Count != 0)
                {
                    foreach (var match in matches)
                    {
                        Sentence sentence = new Sentence();
                        sentence.Content = (match as Match).Value;
                        sentences.Add(sentence);
                    }
                }
            }
            catch (Exception)
            { }
            return sentences;
        }
        private List<Sentence> GetSentences(ref Person person, string content)
        {
            List<Sentence> sentences = new List<Sentence>();
            try
            {
                MatchCollection matches = Regex.Matches(content, string.Format(@"(?<=[。！”])[^。！”]+{0}[^。！”]+(?=[。！”])", person.PersonName), RegexOptions.Singleline);
                if (matches != null && matches.Count != 0)
                {
                    foreach (var match in matches)
                    {
                        Sentence sentence = new Sentence();
                        sentence.Content = (match as Match).Value;
                        sentence.PersonName = person.PersonName;
                        sentences.Add(sentence);
                    }
                }
                person.NameCount += matches.Count;   //addadd
                person.Weight += matches.Count;      //addadd
            }
            catch (Exception)
            { }
            return sentences;
        }

        private void EnqueuePerson(List<Person> persons)
        {
            Monitor.Enter(_personQueue);
            foreach (Person person in persons)
                _personQueue.Enqueue(person);
            Monitor.Exit(_personQueue);
        }
        private Person DequeuePerson()
        {
            Monitor.Enter(_personQueue);
            Person person = null;
            if (_personQueue.Count != 0)
            {
                person = _personQueue.Dequeue();
            }
            Monitor.Exit(_personQueue);
            return person;
        }
        private void AbordThreads()
        {
            foreach (var thread in _thread_person)
            {
                if (thread != null && thread.ThreadState != ThreadState.Running)
                    thread.Abort();
            }
        }

        private bool isFinished()
        {
            if (_personQueue.Count == 0)
                return true;
            else
                return false;
        }

        public string newLabelStr { get; set; }
    }
}
