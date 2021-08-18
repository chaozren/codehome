using System;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace mmc_production
{
    class Mesnet
    {
        static readonly string BASE_URL_BUSINESS = "http://116.6.21.92:9097/MESInterface.asmx";
        //static readonly String BASE_URL_TEST = "https://mmc-factory-test.mi-ae.cn/mmc/api/";
        static readonly String CMD_CheckUserDo = "CheckUserDo";
        static readonly String USERNO = "M_USERNO";
        static readonly String PWD = "M_PASSWORD";
        static readonly String DEVICE_NUM = "M_MACHINENO";
        //static readonly String CHECK_USER_DO_RESULT = "soap:Body";

        static readonly String CMD_InputTest = "WeldInputTest";
        static readonly String SN = "M_SN";
        static readonly String RESULT = "M_RESULT";
        static readonly String ERROR = "M_ERROR";
        static readonly String VALUE = "M_ITEMVALUE";
        static readonly String MO = "M_MO";
        static readonly String WeldInputTestResponse = "WeldInputTestResponse";
        static readonly String WeldInputTestResult = "WeldInputTestResult";

        public static readonly String RESP_RESULT = "result";
        public static readonly String RESP_MSG = "msg";

       

        public Mesnet()
        {

        }

        public static Hashtable CheckUserDo(string userinfo, string pwd, string deviceinfo)
        {
            Hashtable ht = new Hashtable();
            ht.Add(USERNO, userinfo);
            ht.Add(PWD, pwd);
            ht.Add(DEVICE_NUM, deviceinfo);

            XmlDocument resDoc = WebSvcCaller.QuerySoapWebService(BASE_URL_BUSINESS, CMD_CheckUserDo, ht);
            //XmlElement resEle = WebSvcCaller.QuerySoapWebService(BASE_URL_BUSINESS, CMD_CheckUserDo, ht);
            Hashtable result = new Hashtable(); 
            if (resDoc != null)
            {
                //XmlNamespaceManager mgr = new XmlNamespaceManager(resDoc.NameTable);
                //mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                //XmlNode node = resDoc.SelectSingleNode(CHECK_USER_DO_RESULT);
                XmlElement node = resDoc.DocumentElement;
                XmlNode child = (XmlNode)node;
                while (child.FirstChild != null)
                {
                    child = child.FirstChild;
                }
                //XmlElement nodechild = 
                //resEle.
                //XmlElement xe = (XmlElement)child;
                if (String.Equals(child.Value, "TRUE"))
                {
                    result[RESP_RESULT] = true;
                }else
                {
                    result[RESP_RESULT] = false;
                }
                result[RESP_MSG] = child.Value;
            }
            else
            {
                result[RESP_RESULT] = false;
                result[RESP_MSG] = "500/403 etc error";
            }

            return result;
        }

        public static Hashtable WeldInputTest(string res, string error, string value)
        {
            Hashtable ht = new Hashtable();
            ht.Add(SN, Common.sn);
            ht.Add(USERNO, ProdDataHandler.mes_account);
            ht.Add(DEVICE_NUM, ProdDataHandler.mes_device);
            ht.Add(MO, ProdDataHandler.mes_make_order);
            ht.Add(RESULT, res);
            ht.Add(ERROR, error);
            ht.Add(VALUE, value);

            XmlDocument resDoc = WebSvcCaller.QuerySoapWebService(BASE_URL_BUSINESS, CMD_InputTest, ht);
            //XmlElement resEle = WebSvcCaller.QuerySoapWebService(BASE_URL_BUSINESS, CMD_CheckUserDo, ht);
            Hashtable result = new Hashtable();
            if (resDoc != null)
            {
                //XmlNamespaceManager mgr = new XmlNamespaceManager(resDoc.NameTable);
                //mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                //XmlNode node = resDoc.SelectSingleNode(CHECK_USER_DO_RESULT);
                XmlElement node = resDoc.DocumentElement;
                XmlNode child = (XmlNode)node;
                while (child.FirstChild != null)
                {
                    child = child.FirstChild;
                }
                //XmlElement nodechild = 
                //resEle.
                //XmlElement xe = (XmlElement)child;
                if (child.Value.IndexOf("TRUE", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result[RESP_RESULT] = true;
                }
                else
                {
                    result[RESP_RESULT] = false;
                }
                result[RESP_MSG] = child.Value;
            }
            else
            {
                result[RESP_RESULT] = false;
                result[RESP_MSG] = "500/403 etc error";
            }

            return result;
        }

    }
}
