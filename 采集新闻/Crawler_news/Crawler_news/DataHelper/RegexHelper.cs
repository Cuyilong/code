using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler_news.DataHelper
{
    public static class RegexHelper
    {
        public static string regexNewsTitle = @".*?(?=\[s\])";
        public static string regexNewsTextBetweenLabel = @"(?<=>).*?(?=<)";
        public static string regexNewsTime_1 = @"(?:(?!0000)[0-9]{4}-(?:(?:0?[1-9]|1[0-2])-(?:0?[1-9]|1[0-9]|2[0-8])|(?:0[13-9]|1[0-2])-(?:29|30)|(?:0[13578]|1[02])-31)|(?:[0-9]{2}(?:0[48]|[2468][048]|[13579][26])|(?:0[48]|[2468][048]|[13579][26])00)-02-29)\s+([01][0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?";
        public static string regexNewsTime_2 = @"(?<=>)[0-9]{4}年[0-9]{2}月[0-9]{2}日\s*(&nbsp;)*[0-9]{2}:[0-9]{2}(?=<)";
        public static string regexNewsSinaZhengwen = @"(<[^>]+>[^<>]*){1,4}(?=(\s*(&nbsp;)*(&gt;)*(&nbsp;)*正文\s*<))";
        // (<[^>]+>[^<>]*){4}(?=((&nbsp;)*(&gt;)*(&nbsp;)*正文\s*<))
        public static string regexNews163Zhengwen = @"(<[^>]+>.*?){1}(?=(<[^<>]+>(\s)(&gt;)(\s){2}正文<))";
        public static string regexNewsTextHasHref = @"(?<=(<\w*\s*href[^>]*>))[^<>]*(?=<)";
        public static string regexNewsTitleLabel = @"(?<=(<title>))[^<>]*(?=(</title>))";
        public static string regexNewsForum_xiaHuaXian = @"(?<=-)[^-<>]*";
        public static string regexNewsForum_sinaShouYe = @"(<[^>]+>[^<>]*){1,4}(?=(新浪首页<))";
        public static string regexNewsTime_ifengDuanping = @"[0-9]{4}_[0-9]{2}/[0-9]{2}";
    }
}
