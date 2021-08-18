using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace mmc_production
{
    class ProdData
    {
        public ProdData()
        { }

        //操作员工号
        private string account;

        public string Account
        {
            get { return account; }
            set { account = value;  }
        }

        //设备号
        private string device;

        public string Device
        {
            get { return device;  }
            set { device = value;  }
        }

    }

    //battery list
    enum battery_enum
    {
        Maxwell=0,
        Panasonic=1,
        Taiteng=2
    }

    class ProdDataHandler
    {
        public static string PRODUCTION_INFO_UPDATE_PWD = "75022801";

        static readonly String MES_ACCOUNT_ID_TEST = "admin";
        static readonly String MES_DEVICE_TEST = "HW-Device-Group";
        static readonly String MES_MAKE_ORDER_TEST = "QH-MO201605190001-1401";

        static string CONFIG_FILE_NAME = "config.xml";
        static string NODE_ROOT = "configuration";
        static string NODE_XWD = "xwd";
        static string NODE_ACCOUNT = "M_USERNO";
        static string NODE_DEVICE = "M_MACHINENO";
        static string NODE_MAKE_ORDER = "M_MO";
        static string NODE_PRINT_LOC = "print_location";
        static string NODE_PRODUCT_INFO = "productdata";
        static string NODE_PRODUCT_NTC = "ntc";
        static string NODE_PRODUCT_BATTERY = "battery";
        static string NODE_PRODUCT_HW = "hw_version";
        static string NODE_PRODUCT_MODLENUM = "model_num";
        static string NODE_PRODUCT_WORK_LOC = "work_Location";
        static string NODE_PRODUCT_FACTORY = "factory";

        public static string mes_account = "";
        public static string mes_device = "";
        public static string mes_make_order = "";

        public static string print_loc = "";

        public static string ntc = "";
        public static string battery = "";
        public static string hw_ver = "";
        public static string model_num = "";
        public static string work_loc = "";
        public static string factory = "";

        //public static string sn = "";
        public static string mesinputtest = "";


        public ProdDataHandler()
        {

        }

        public void readData()
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ApplicationBase+CONFIG_FILE_NAME))
            {
                //doc.Load(CONFIG_FILE_NAME);
                doc.Load(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + CONFIG_FILE_NAME);
                //doc.Load(System.Windows.Forms.Application.ExecutablePath + CONFIG_FILE_NAME);

                XmlNode xn = doc.SelectSingleNode(NODE_ROOT);
                XmlNode xe_xwd = xn[NODE_XWD];

                mes_account = xe_xwd[NODE_ACCOUNT].InnerText;
                mes_device = xe_xwd[NODE_DEVICE].InnerText;
                mes_make_order = xe_xwd[NODE_MAKE_ORDER].InnerText;

                print_loc = xn[NODE_PRINT_LOC].InnerText;

                XmlNode xe_pdata = xn[NODE_PRODUCT_INFO];
                ntc = xe_pdata[NODE_PRODUCT_NTC].InnerText;
                battery = xe_pdata[NODE_PRODUCT_BATTERY].InnerText;
                hw_ver = xe_pdata[NODE_PRODUCT_HW].InnerText;
                model_num = xe_pdata[NODE_PRODUCT_MODLENUM].InnerText;
                work_loc = xe_pdata[NODE_PRODUCT_WORK_LOC].InnerText;
                factory = xe_pdata[NODE_PRODUCT_FACTORY].InnerText;
            }
            if (mes_account=="")
            {
                mes_account = MES_ACCOUNT_ID_TEST;
                mes_device = MES_DEVICE_TEST;
                mes_make_order = MES_MAKE_ORDER_TEST;
            }

        }

        public void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        private void CreateXmlDoc()
        {
            XmlDocument doc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = doc.CreateXmlDeclaration("1.0", "utf-8", "");
            doc.AppendChild(node);
            try
            {
                XmlNode root = doc.CreateElement(NODE_ROOT);
                doc.AppendChild(root);

                XmlNode xe_xwd = doc.CreateElement(NODE_XWD);
                CreateNode(doc, xe_xwd, NODE_ACCOUNT, "");
                CreateNode(doc, xe_xwd, NODE_DEVICE, "");
                CreateNode(doc, xe_xwd, NODE_MAKE_ORDER, "");
                root.AppendChild(xe_xwd);
                
                CreateNode(doc, root, NODE_PRINT_LOC, "");

                XmlNode xe_prod = doc.CreateElement(NODE_PRODUCT_INFO);
                CreateNode(doc, xe_prod, NODE_PRODUCT_NTC, "");
                CreateNode(doc, xe_prod, NODE_PRODUCT_BATTERY, "");
                CreateNode(doc, xe_prod, NODE_PRODUCT_HW, "");
                CreateNode(doc, xe_prod, NODE_PRODUCT_MODLENUM, "");
                CreateNode(doc, xe_prod, NODE_PRODUCT_WORK_LOC, "");
                CreateNode(doc, xe_prod, NODE_PRODUCT_FACTORY, "");
                root.AppendChild(xe_prod);

                doc.Save(CONFIG_FILE_NAME);
            }
            catch (Exception e)
            {
            }
        }

        private XmlDocument getXmlDoc()
        {
            XmlDocument doc = new XmlDocument();
            if (!File.Exists(CONFIG_FILE_NAME))
            {
                CreateXmlDoc();
            }
            doc.Load(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + CONFIG_FILE_NAME);
            
            return doc;
        }

        public bool writeMesData(string user, string hw, string order)
        {
            bool res = false;
            XmlDocument doc = getXmlDoc();
            XmlNode xn = doc.SelectSingleNode(NODE_ROOT);
            XmlElement xe = (XmlElement)xn[NODE_XWD];

            xe[NODE_ACCOUNT].InnerText = user; 
            xe[NODE_DEVICE].InnerText = hw;
            xe[NODE_MAKE_ORDER].InnerText = order;

            mes_account = user;
            mes_device = hw;
            mes_make_order = order;
            try
            {
                doc.Save(CONFIG_FILE_NAME);
                res = true;
            }
            catch(Exception e)
            {
                
            }
           
            return res;
        }

        public bool writePrintLoc(string path)
        {
            bool res = false;
            XmlDocument doc = getXmlDoc();
            XmlNode xn = doc.SelectSingleNode(NODE_ROOT);
            XmlElement xe = (XmlElement)xn;

            xe[NODE_PRINT_LOC].InnerText = path;

            print_loc = path;
            try
            {
                doc.Save(CONFIG_FILE_NAME);
                res = true;
            }
            catch (Exception e)
            {

            }

            return res;
        }

        public bool writeProductInfo(string ntcstr, string bat, string hw, string num, string loc, string fac)
        {
            bool res = false;
            XmlDocument doc = getXmlDoc();
            XmlNode xn = doc.SelectSingleNode(NODE_ROOT);
            XmlElement xe = (XmlElement)xn[NODE_PRODUCT_INFO];

            ntc = ntcstr;
            battery = bat;
            hw_ver = hw;
            model_num = num;
            work_loc = loc;
            factory = fac;

            try
            {
                xe[NODE_PRODUCT_NTC].InnerText = ntcstr;
                xe[NODE_PRODUCT_BATTERY].InnerText = bat; 
                xe[NODE_PRODUCT_HW].InnerText = hw.ToUpper();
                xe[NODE_PRODUCT_MODLENUM].InnerText = num.ToUpper();
                xe[NODE_PRODUCT_WORK_LOC].InnerText = loc.ToUpper();
                xe[NODE_PRODUCT_FACTORY].InnerText = fac.ToUpper();
                doc.Save(CONFIG_FILE_NAME);
                res = true;
                
            }
            catch (Exception e)
            {

            }

            return res;
        }

    }//ProdDataHandler

}
