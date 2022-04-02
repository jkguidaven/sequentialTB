using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreDLL.utilities
{
    public static class StringHelper
    {

        private static readonly String DATEREGX = @"\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2}";

        public static bool In<T>(this T me, params T[] set)
        {
            return set.Contains(me);
        }


        public static String[] SplitExpression(String strExpression)
        {
            char prevCh = '\0';

            List<String> lstSplitString = new List<String>();
            StringBuilder BufferedStr = new StringBuilder();

            Boolean bIsStackStringLiteral = false;
            Boolean bPrevIsSymbol = false;
            Boolean bPrevIsBracket = false;
            Boolean bPrevIsNumber = false;
            foreach (char ch in strExpression)
            {
                prevCh = ch;
                if (!bIsStackStringLiteral)
                {
                    if (ch.In<char>('=', '>', '<', '!', '&', '|',','))
                    {
                        if (!bPrevIsSymbol || bPrevIsBracket || bPrevIsNumber)
                        {
                            if (BufferedStr.Length > 0)
                            {
                                lstSplitString.Add(BufferedStr.ToString());
                                BufferedStr.Clear();
                            }
                            bPrevIsBracket = false;
                        }
                        BufferedStr.Append(ch);
                        if (bPrevIsSymbol)
                        {
                            if (BufferedStr.Length > 0)
                            {
                                lstSplitString.Add(BufferedStr.ToString());
                                BufferedStr.Clear();
                            }
                            bPrevIsSymbol = false;
                        }
                        else
                            bPrevIsSymbol = true;

                        bPrevIsNumber = false;
                    }
                    else if (ch.In<char>('0', '1', '2', '3', '4', '5', '6', '7', '8', '9'))
                    {
                        if (!bPrevIsNumber || bPrevIsBracket || bPrevIsSymbol)
                        {
                            if (BufferedStr.Length > 0)
                            {
                                lstSplitString.Add(BufferedStr.ToString());
                                BufferedStr.Clear();
                            }
                            bPrevIsBracket = false;
                        }
                        BufferedStr.Append(ch);
                        bPrevIsNumber = true;
                    }
                    else if (ch.In<char>('(', ' ', ')'))
                    {
                        if (BufferedStr.Length > 0)
                        {
                            lstSplitString.Add(BufferedStr.ToString());
                            BufferedStr.Clear();
                        }

                        bPrevIsSymbol = false;

                        if (ch != ' ')
                        {
                            bPrevIsBracket = true;
                            BufferedStr.Append(ch);
                        }
                        bPrevIsNumber = false;
                    }
                    else if (ch == '\'')
                    {
                        if (BufferedStr.Length > 0)
                        {
                            lstSplitString.Add(BufferedStr.ToString());
                            BufferedStr.Clear();
                        }

                        BufferedStr.Append(ch);
                        bPrevIsSymbol = false;
                        bPrevIsBracket = false;
                        bPrevIsNumber = false;
                        bIsStackStringLiteral = true;
                    }
                    else
                    {
                        if (bPrevIsSymbol || bPrevIsBracket)
                        {
                            if (BufferedStr.Length > 0)
                            {
                                lstSplitString.Add(BufferedStr.ToString());
                                BufferedStr.Clear();
                            }
                        }

                        bPrevIsSymbol = false;
                        bPrevIsBracket = false;
                        bPrevIsNumber = false;
                        BufferedStr.Append(ch);
                    }
                }
                else
                {
                    BufferedStr.Append(ch);
                    if (ch == '\'')
                    {
                        if (BufferedStr.Length > 0)
                        {
                            lstSplitString.Add(BufferedStr.ToString());
                            BufferedStr.Clear();
                        }
                        bIsStackStringLiteral = false;
                    }
                }


            }

            if (bIsStackStringLiteral)
                throw new Exception("Invalid expression");

            if (BufferedStr.Length > 0)
            {
                lstSplitString.Add(BufferedStr.ToString());
                BufferedStr.Clear();
            }
            return lstSplitString.ToArray();

        }



        public static Boolean isStringLiteral(String strVal)
        {
            return (strVal[0] == '\'' && strVal[strVal.Length - 1] == '\'');
        }

        public static Boolean isStringBoolean(String strVal)
        {
            return strVal.Equals("true") || strVal.Equals("false");
        }

        public static Boolean isStringInteger(String strVal)
        {
            int n = 0;
            return Int32.TryParse(strVal, out n);
        }


        public static String[] splitParameter(String strVal)
        {
            List<String> parameters = new List<String>();
            String buff = "";
            for (int i = 0; i < strVal.Length; i++)
            {
                if (strVal[i] == ',')
                {
                    parameters.Add(buff);
                    buff = "";
                }
                else if (strVal[i] == '(')
                {
                    int PairCount = 0;
                    while (i < strVal.Length)
                    {
                        if (strVal[i] == '(')
                            PairCount++;

                        if (strVal[i] == ')')
                        {
                            PairCount--;

                            if (PairCount == 0)
                            {

                                buff += strVal[i];
                                break;
                            }
                        }

                        buff += strVal[i];
                        i++;
                    }

                    if (!buff.Trim().Equals(""))
                    {
                        parameters.Add(buff);
                        buff = "";
                    }
                }
                else
                {
                    buff += strVal[i];
                }
            }

            if (buff.Trim() != "")
                parameters.Add(buff);


            return parameters.ToArray();
        }


        public static String repeat(String strVal, int count)
        {
            String buff = "";

            for (int i = 0; i < count; i++)
                buff += strVal;

            return buff;
        }

        public static String getTimeStamp(String strVal)
        {
            return Regex.Match(strVal, DATEREGX).Value;
        }


        public static String LToDS(long timestamp)
        {
            long month = timestamp / 2678400;
            timestamp = timestamp - (2678400 * month);

            long day = timestamp / 86400;
            timestamp = timestamp - (86400 * day);

            long hour = timestamp / 3600;
            timestamp = timestamp - (3600 * hour);

            long minute = timestamp / 60;
            timestamp = timestamp - (60 * minute);

            long second = timestamp;


            return month + "/" + day + "  " +
                  (hour   < 10 ? "0" + hour   : hour.ToString()) + ":" +
                  (minute < 10 ? "0" + minute : minute.ToString()) + ":" +
                  (second < 10 ? "0" + second : second.ToString());
        }

        public static long DSToL(String timestamp)
        {
            // check if timestamp has correct format
            if (!Regex.IsMatch(timestamp, DATEREGX))
                throw new Exception("Invalid timestamp format");
            else
            {
                String monthday = Regex.Match(timestamp, @"\d{2}/\d{2}").Value.Trim();
                int month = int.Parse(monthday.Substring(0, 2));
                int day = int.Parse(monthday.Substring(3, 2));

                String hh_mm_ss = Regex.Match(timestamp, @"\d{2}:\d{2}:\d{2}").Value.Trim();
                int hour = int.Parse(hh_mm_ss.Substring(0, 2));
                int minute = int.Parse(hh_mm_ss.Substring(3, 2));
                int second = int.Parse(hh_mm_ss.Substring(6, 2));

                return (month * 2678400) + //multiple month to 2678400 seconds
                       (day * 86400) + //multiple day to 86400 seconds
                       (hour * 3600) + //multiple hour to 3600 seconds
                       (minute * 60) + //multiple minute to 60 seconds
                       second;

            }

        }
    }
}
