using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ParseNews.DataHelper
{
    public struct SHLSegWord
    {
        public string s_szWord; //字符串
        public short s_dwPOS;  //词性标志
        public float s_fWeight;//关键词权重，如果不是关键词，权重为0
        //System.Ushort32
    }
    class SeparateHelper
    {
        private string m_strKey;
        private string m_strWords;
        private string m_strFinger;
        /************************************************************/
        //常量定义部分//
        /************************************************************/
        const int HL_CAL_OPT_KEYWORD = 0x1;//计算关键词附加标识
        const int HL_CAL_OPT_FINGER = 0x2;//计算文章语义指纹标识
        const int HL_CAL_OPT_POS = 0x4;//计算词性标识
        const int HL_CAL_OPT_SEARCH = 0x8;//输出面向检索的分词结果
        /************************************************************/
        //词性定义部分//
        /************************************************************/
        public const int NATURE_D_A = 0x40000000;//形容词 形语素
        public const int NATURE_D_B = 0x20000000;//区别词 区别语素
        public const int NATURE_D_C = 0x10000000;//连词 连语素
        public const int NATURE_D_D = 0x08000000;//副词 副语素
        public const int NATURE_D_E = 0x04000000;//叹词 叹语素
        public const int NATURE_D_F = 0x02000000;//方位词 方位语素
        public const int NATURE_D_I = 0x01000000;//成语
        public const int NATURE_D_L = 0x00800000;//习语
        public const int NATURE_A_M = 0x00400000;//数词 数语素
        public const int NATURE_D_MQ = 0x00200000;//数量词
        public const int NATURE_D_N = 0x00100000;//名词 名语素
        public const int NATURE_D_O = 0x00080000;//拟声词
        public const int NATURE_D_P = 0x00040000;//介词
        public const int NATURE_A_Q = 0x00020000;//量词 量语素
        public const int NATURE_D_R = 0x00010000;//代词 代语素
        public const int NATURE_D_S = 0x00008000;//处所词
        public const int NATURE_D_T = 0x00004000;//时间词
        public const int NATURE_D_U = 0x00002000;//助词 助语素
        public const int NATURE_D_V = 0x00001000;//动词 动语素
        public const int NATURE_D_W = 0x00000800;//标点符号
        public const int NATURE_D_X = 0x00000400;//非语素字
        public const int NATURE_D_Y = 0x00000200;//语气词 语气语素
        public const int NATURE_D_Z = 0x00000100;//状态词
        public const int NATURE_A_NR = 0x00000080;//人名
        public const int NATURE_A_NS = 0x00000040;//地名
        public const int NATURE_A_NT = 0x00000020;//机构团体
        public const int NATURE_A_NX = 0x00000010;//外文字符
        public const int NATURE_A_NZ = 0x00000008;//其他专名
        public const int NATURE_D_H = 0x00000004;
        public const int NATURE_D_K = 0x00000002;//后接成分
        //初始化分词词典
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLSplitInit")]
        private static extern bool HLSplitInit(string path);
        //创建分词句柄
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLOpenSplit")]
        private static extern IntPtr HLOpenSplit();
        //对一段字符串分词
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLSplitWord")]
        private static extern bool HLSplitWord(IntPtr pHandle, string text, int flag);

        //取得分词个数
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLGetWordCnt")]
        private static extern int HLGetWordCnt(IntPtr pHandle);

        //获取指定的分词结果
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLGetWordAt")]
        private static extern IntPtr HLGetWordAt(IntPtr pHandle, int pos);

        //获取关键词个数
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLGetFileKeyCnt")]
        private static extern int HLGetFileKeyCnt(IntPtr pHandle);

        //获取指定下标的关键词
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLGetFileKeyAt")]
        private static extern IntPtr HLGetFileKeyAt(IntPtr pHandle, int pos);

        //装载用户自定义词典
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLOpenUsrDict")]
        private static extern bool HLOpenUsrDict(string lpUserDictName);

        //卸载用户自定义词典
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLFreeUsrDict")]
        private static extern bool HLFreeUsrDict();

        //获得语义指纹
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLGetFingerM")]
        private static extern bool HLGetFingerM(IntPtr hHandle, ref IntPtr rpData, ref Int32 rdwLen);

        //关闭分词句柄
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLCloseSplit")]
        private static extern void HLCloseSplit(IntPtr pHandle);
        //海量分词系统卸载
        [DllImport(@"C:\dll\HLSSplit.dll", SetLastError = true, EntryPoint = "HLFreeSplit")]
        private static extern void HLFreeSplit();
        /// <summary>
        /// 判断是否含有标点
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool IsHasPunctuation(string str)
        {
            if (str == null)
                return false;
            bool hasPunctuation = false;
            bool bInitDict = HLSplitInit(@"C:\dll\");
            if (!bInitDict)
            {
                Console.WriteLine("初始化分词字典失败!", "错误");
            }

            IntPtr hHandle = HLOpenSplit(); //创建分词句柄
            if (hHandle == IntPtr.Zero)
            {
                //创建分词句柄失败
                Console.WriteLine("创建分词句柄失败!", "错误");
                HLFreeSplit();//卸载分词字典
            }

            short iExtraCalcFlag = 0; //附加计算标志，不进行附加计算
            //获得附加计算标识
            //if (this.chkPos.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_POS;//
            //if (this.chkKeyword.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_KEYWORD;
            //if (this.chkSeach.Checked)
            //iExtraCalcFlag |= HL_CAL_OPT_SEARCH;
            //if (this.chkFinger.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_FINGER;
            DateTime bgdt = DateTime.Now;
            bool bSuccess = HLSplitWord(hHandle, str, iExtraCalcFlag);
            System.TimeSpan ts = DateTime.Now - bgdt;
            Console.WriteLine("get names----->" + ts);
            //this.txtMsg.Text = string.Format("用时{0}分{1}秒{2}毫秒", ts.Minutes, ts.Seconds, ts.Milliseconds);
            if (bSuccess)
            {
                //分词成功
                int nResultCnt = HLGetWordCnt(hHandle);//取得分词个数
                for (short i = 0; i < nResultCnt; i++)
                {
                    //取得分词结果
                    IntPtr h = HLGetWordAt(hHandle, i);
                    //取得一个分词结果
                    SHLSegWord pWord = (SHLSegWord)Marshal.PtrToStructure(h, typeof(SHLSegWord));
                    if (GetNatureString(pWord.s_dwPOS).Equals(".w"))
                        hasPunctuation = true;
                }
                HLCloseSplit(hHandle);//关闭分词句柄
            }
            else
            {
                //分词失败
                Console.WriteLine("分词失败!", "错误");
                HLCloseSplit(hHandle);//关闭分词句柄
                HLFreeSplit();//卸载分词字典
            }
            HLFreeSplit(); //卸载分词词典 
            return hasPunctuation;
        }
        /// <summary>
        /// 获取名字
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> GetNames(string text)
        {
            List<string> names = new List<string>();
            bool bInitDict = HLSplitInit(@"C:\dll\");
            if (!bInitDict)
            {
                Console.WriteLine("初始化分词字典失败!", "错误");
                return null;
            }

            IntPtr hHandle = HLOpenSplit(); //创建分词句柄
            if (hHandle == IntPtr.Zero)
            {
                //创建分词句柄失败
                Console.WriteLine("创建分词句柄失败!", "错误");
                HLFreeSplit();//卸载分词字典
                return null;
            }

            short iExtraCalcFlag = 0; //附加计算标志，不进行附加计算
            //获得附加计算标识
            //if (this.chkPos.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_POS;//
            //if (this.chkKeyword.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_KEYWORD;
            //if (this.chkSeach.Checked)
            //iExtraCalcFlag |= HL_CAL_OPT_SEARCH;
            //if (this.chkFinger.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_FINGER;
            DateTime bgdt = DateTime.Now;
            bool bSuccess = HLSplitWord(hHandle, text, iExtraCalcFlag);
            System.TimeSpan ts = DateTime.Now - bgdt;
            Console.WriteLine("get names----->"+ts);
            //this.txtMsg.Text = string.Format("用时{0}分{1}秒{2}毫秒", ts.Minutes, ts.Seconds, ts.Milliseconds);
            if (bSuccess)
            {
                //分词成功
                int nResultCnt = HLGetWordCnt(hHandle);//取得分词个数
                for (short i = 0; i < nResultCnt; i++)
                {
                    //取得分词结果
                    IntPtr h = HLGetWordAt(hHandle, i);
                    //取得一个分词结果
                    SHLSegWord pWord = (SHLSegWord)Marshal.PtrToStructure(h, typeof(SHLSegWord));
                    if (GetNatureString(pWord.s_dwPOS).Equals(".nr"))
                        names.Add(pWord.s_szWord);
                }
                HLCloseSplit(hHandle);//关闭分词句柄
            }
            else
            {
                //分词失败
                Console.WriteLine("分词失败!", "错误");
                HLCloseSplit(hHandle);//关闭分词句柄
                HLFreeSplit();//卸载分词字典
                names = null;
            }
            HLFreeSplit(); //卸载分词词典 
            return names;
        }

        public List<string> GetLabels(string sentence)
        {
            List<string> labels = new List<string>();
            bool bInitDict = HLSplitInit(@"C:\dll\");
            if (!bInitDict)
            {
                Console.WriteLine("初始化分词字典失败!", "错误");
                return null;
            }

            IntPtr hHandle = HLOpenSplit(); //创建分词句柄
            if (hHandle == IntPtr.Zero)
            {
                //创建分词句柄失败
                Console.WriteLine("创建分词句柄失败!", "错误");
                HLFreeSplit();//卸载分词字典
                return null;
            }

            short iExtraCalcFlag = 0; //附加计算标志，不进行附加计算
            //获得附加计算标识
            //if (this.chkPos.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_POS;//
            //if (this.chkKeyword.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_KEYWORD;
            //if (this.chkSeach.Checked)
            //iExtraCalcFlag |= HL_CAL_OPT_SEARCH;
            //if (this.chkFinger.Checked)
            iExtraCalcFlag |= HL_CAL_OPT_FINGER;
            DateTime bgdt = DateTime.Now;
            bool bSuccess = HLSplitWord(hHandle, sentence, iExtraCalcFlag);
            System.TimeSpan ts = DateTime.Now - bgdt;
            Console.WriteLine("get labels---->"+ts);
            //this.txtMsg.Text = string.Format("用时{0}分{1}秒{2}毫秒", ts.Minutes, ts.Seconds, ts.Milliseconds);
            if (bSuccess)
            {
                //分词成功
                int nResultCnt = HLGetWordCnt(hHandle);//取得分词个数
                for (int i = 0; i < nResultCnt; i++)
                {
                    //取得分词结果
                    IntPtr h = HLGetWordAt(hHandle, i);
                    //取得一个分词结果
                    SHLSegWord pWord = (SHLSegWord)Marshal.PtrToStructure(h, typeof(SHLSegWord));
                    if ((GetNatureString(pWord.s_dwPOS).Equals(".n")||  //名词 
                        GetNatureString(pWord.s_dwPOS).Equals(".v") ||   //动词
                        GetNatureString(pWord.s_dwPOS).Equals(".ns")||  //地名
                        GetNatureString(pWord.s_dwPOS).Equals(".m") ||   //数词
                        GetNatureString(pWord.s_dwPOS).Equals(".mq")|| //数量词
                        GetNatureString(pWord.s_dwPOS).Equals(".nr"))&&
                        pWord.s_szWord.Length>1)
                        labels.Add(pWord.s_szWord);
                }
                HLCloseSplit(hHandle);//关闭分词句柄
            }
            else
            {
                //分词失败
                Console.WriteLine("分词失败!", "错误");
                HLCloseSplit(hHandle);//关闭分词句柄
                HLFreeSplit();//卸载分词字典
                labels = null;
            }
            HLFreeSplit(); //卸载分词词典 
            return labels;
        }
        /// <summary>
        /// 获取词性
        /// </summary>
        /// <param name="dwPos"></param>
        /// <returns></returns>
        private string GetNatureString(short dwPos)
        {
            string Nature = ".";
            if ((dwPos & NATURE_D_A) == NATURE_D_A)
            {
                Nature += "a";//形容词
            }
            else if ((dwPos & NATURE_D_B) == NATURE_D_B)
            {
                Nature += "b";//区别词
            }
            else if ((dwPos & NATURE_D_C) == NATURE_D_C)
            {
                Nature += "c";//连词
            }
            else if ((dwPos & NATURE_D_D) == NATURE_D_D)
            {
                Nature += "d";//副词
            }
            else if ((dwPos & NATURE_D_E) == NATURE_D_E)
            {
                Nature += "e";//叹词
            }
            else if ((dwPos & NATURE_D_F) == NATURE_D_F)
            {
                Nature += "f";//方位词
            }
            else if ((dwPos & NATURE_D_I) == NATURE_D_I)
            {
                Nature += "i"; //成语
            }
            else if ((dwPos & NATURE_D_L) == NATURE_D_L)
            {
                Nature += "l";//习语
            }
            else if ((dwPos & NATURE_A_M) == NATURE_A_M)
            {
                Nature += "m";//数词
            }
            else if ((dwPos & NATURE_D_MQ) == NATURE_D_MQ)
            {
                Nature += "mq";//数量词
            }
            else if ((dwPos & NATURE_D_N) == NATURE_D_N)
            {
                Nature += "n";//名词
            }
            else if ((dwPos & NATURE_D_O) == NATURE_D_O)
            {
                Nature += "o";//拟声词
            }
            else if ((dwPos & NATURE_D_P) == NATURE_D_P)
            {
                Nature += "p";//介词
            }
            else if ((dwPos & NATURE_A_Q) == NATURE_A_Q)
            {
                Nature += "q";//量词
            }
            else if ((dwPos & NATURE_D_R) == NATURE_D_R)
            {
                Nature += ".r";//代词
            }
            else if ((dwPos & NATURE_D_S) == NATURE_D_S)
            {
                Nature += "s";//处所词
            }
            else if ((dwPos & NATURE_D_T) == NATURE_D_T)
            {
                Nature += ".t";//时间词
            }
            else if ((dwPos & NATURE_D_U) == NATURE_D_U)
            {
                Nature += "u";//助词
            }
            else if ((dwPos & NATURE_D_V) == NATURE_D_V)
            {
                Nature += "v";//动词
            }
            else if ((dwPos & NATURE_D_W) == NATURE_D_W)
            {
                Nature += "w";//标点符号
            }
            else if ((dwPos & NATURE_D_X) == NATURE_D_X)
            {
                Nature += "x";//非语素字
            }
            else if ((dwPos & NATURE_D_Y) == NATURE_D_Y)
            {
                Nature += "y";//语气词
            }
            else if ((dwPos & NATURE_D_Z) == NATURE_D_Z)
            {
                Nature += "z";//状态词
            }
            else if ((dwPos & NATURE_A_NR) == NATURE_A_NR)
            {
                Nature += "nr";//人名
            }
            else if ((dwPos & NATURE_A_NS) == NATURE_A_NS)
            {
                Nature += "ns";//地名
            }
            else if ((dwPos & NATURE_A_NT) == NATURE_A_NT)
            {
                Nature += "nt";//机构团体
            }
            else if ((dwPos & NATURE_A_NX) == NATURE_A_NX)
            {
                Nature += "nx";//外文字符
            }
            else if ((dwPos & NATURE_A_NZ) == NATURE_A_NZ)
            {
                Nature += "nz";//其他专名
            }
            else if ((dwPos & NATURE_D_H) == NATURE_D_H)
            {
                Nature += "h";//前接成分
            }
            else if ((dwPos & NATURE_D_K) == NATURE_D_K)
            {
                Nature += "k";//后接成分
            }
            else
            {
                Nature += "?";//未知词性
            }
            return Nature;
        }
    }
}
