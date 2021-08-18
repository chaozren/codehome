using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmc_production
{
    class Common
    {
        public static readonly string XINWANDA_FACTORY_CODE="X100"; //XINWADA not need mes login
        public static string mac;
        public static string sn;

        public const int LOG_TYPE_INFO = 1;
        public const int LOG_TYPE_ERROR = 2;
        public const int LOG_TYPE_CLEAR = 3;
        public const int LOG_TYPE_WRITE_COMMAND_DONE = 4; //写设备成功
        public const int LOG_TYPE_WRITE_COMMAND_FAIL = 5; //写设备失败

        public static String[] array = { "^XA", "^LH0,0", "^FO5,5", "^BQ,2,2", "^FDQA,", "^FS", "^FO68,5", "^BQ,2,2", "^FDQA,", "^FS", "^XZ" };
        private static readonly string FILE_NAME = @".\sn.txt";

        //public static ExcelHandler mExcelHandler;

        public static bool print_sn()
        {
            FileStream fs;
            if (ProdDataHandler.print_loc == "" || ProdDataHandler.print_loc == null)
            {
                return false;
            }
            else
            {
                string[] lines = new string[array.Length];
                Array.Copy(array, lines, array.Length);
                string macstr = mac;
                lines[4] = lines[4] + macstr;
                lines[8] = lines[8] + macstr;
                File.WriteAllLines(FILE_NAME, lines);
                try
                {
                    if (ProdDataHandler.print_loc.EndsWith(@"\"))
                    {
                        Console.Write(@"print loc with \");
                        File.Copy(FILE_NAME, ProdDataHandler.print_loc + FILE_NAME, true);
                    }
                    else
                    {
                        Console.Write(@"print loc without \");
                        File.Copy(FILE_NAME, ProdDataHandler.print_loc + @"\" + FILE_NAME, true);
                    }
                    return true;
                }
                catch(Exception e)
                {
                    return false;
                }
            }

            
        }

    }
}
