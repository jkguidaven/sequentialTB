using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CoreDLL.system
{
    public class SysConfig
    {
        private struct SystemOptions{
            public String m_strOption;
            public String m_strValue;

            public SystemOptions(String strOption, String strValue)
            {
                m_strOption = strOption;
                m_strValue = strValue;
            }
        }

        private static readonly String CONFIG_FILE = "setting.ini";

        protected static String GET_CONFIG(String strConfig, String Option)
        {
            List<SystemOptions> lstSOptions = loadOptions(strConfig);

            foreach (SystemOptions SOption in lstSOptions)
            {
                if (SOption.m_strOption.ToLower().Trim().Equals(Option.ToLower().Trim()))
                    return SOption.m_strValue;
            }

            throw new Exception("Enable to find Configuration[" + strConfig +":" + Option + "] in " + CONFIG_FILE + " file..");
        }

        private static List<SystemOptions> loadOptions(String strConfig)
        {
            List<SystemOptions> lstSOptions = new List<SystemOptions>();


            using (StreamReader clsSReader = new StreamReader(CONFIG_FILE))
            {
                String strLine = null;
                Boolean bAddOption = false;
                while ((strLine = clsSReader.ReadLine()) != null)
                {
                    if (!strLine.Trim().Equals(""))
                    {
                        if (isCategoryLine(strLine))
                            bAddOption = isCategoryMatch(strLine, strConfig);
                        else
                        {
                            if (bAddOption)
                                lstSOptions.Add(parseToSystemOption(strLine));
                        }
                    }
                }
            }

            return lstSOptions;
        }

        private static Boolean isCategoryLine(String strLine)
        {
            strLine = strLine.Trim();
            return strLine[0] == '[' && strLine[strLine.Length - 1] == ']';
        }

        private static SystemOptions parseToSystemOption(String strLine)
        {
            strLine = strLine.Trim();
            return new SystemOptions((strLine.Split('='))[0], (strLine.Split('='))[1]);
        }

        private static Boolean isCategoryMatch(String strLine, String strConfig)
        {
            strLine   = strLine.Trim().ToLower();
            strConfig = strConfig.Trim().ToLower();

            return strLine.Equals("[" + strConfig + "]");
        }



    }
}
