using System;
using System.Collections.Generic;
using System.Text;

namespace MySecretChatServer
{
    static class MyTime
    {
        public static string getTime()
        {
            DateTime time = DateTime.Now;
            string t = String.Format("{0}:{1}:{2}", time.Hour, 
                time.Minute, time.Second);

            return t;
        }

        public static string getTimeForFileName()
        {
            DateTime time = DateTime.Now;
            string t = String.Format("{0}H{1}M{2}S", time.Hour,
                time.Minute, time.Second);

            return t;
        }
    }
}
