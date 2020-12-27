using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace znrsserver
{
    public class HttpHelpercs
    {
        public static string HttpGet(string url)
        {
            string result = "";
            try
            {
                HttpWebRequest wbRequest = (HttpWebRequest)WebRequest.Create(url);
                wbRequest.Method = "GET";
                wbRequest.ContentType = "application/json";//链接类型
                HttpWebResponse wbResponse = (HttpWebResponse)wbRequest.GetResponse();
                using (Stream responseStream = wbResponse.GetResponseStream())
                {
                    using (StreamReader sReader = new StreamReader(responseStream, System.Text.Encoding.Default))
                    {
                        result = sReader.ReadToEnd();
                    }
                }






            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }


        public static string HttpGet(string url, System.Text.Encoding dd)
        {
            string result = "";
            try
            {
                HttpWebRequest wbRequest = (HttpWebRequest)WebRequest.Create(url);
                wbRequest.Method = "GET";
                wbRequest.ContentType = "application/x-www-form-urlencoded";//链接类型
                HttpWebResponse wbResponse = (HttpWebResponse)wbRequest.GetResponse();
                using (Stream responseStream = wbResponse.GetResponseStream())
                {
                    using (StreamReader sReader = new StreamReader(responseStream, dd))
                    {
                        result = sReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static string token = "";
        public static string account = "guolu";//登录账号，需要赋值
        public static string password = "e10adc3949ba59abbe56e057f20f883e";//登录密码，需要赋值
        public static string url = "http://172.21.32.160:8040";//实时数据和回写接口地址，需要赋值

        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        public static string gettoken()
        {
            if (string.IsNullOrEmpty(token))
            {
                string url_token = "http://172.21.32.160:8551/account/login";


                //JObject param1 = (JObject)JsonConvert.DeserializeObject("{\"account\": \""+account+"\",\"password\": \""+password+"\"}");
                string param1 = "{\"account\":\"" + account + "\",\"password\":\"" + password + "\"}";
                string data = HttpPost(url_token, param1, 0);
                return data;
                //   JObject ret = JObject.Parse(data);
                //  token = ret["authorization"].ToString();
            }
            return token;
        }

        public static string HttpPost(string postUrl, string param, int k)
        {
            string ret = "";
            try
            {

                //byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                if (k == 1)
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        token = gettoken();
                    }
                    webReq.Headers.Add("Authorization", token);
                }


                webReq.ContentType = "application/json";
                // byte[] btBodys = Encoding.UTF8.GetBytes(param.ToString());
                //string a = "{\"account\": \"ics_data\",\"password\": \"e10adc3949ba59abbe56e057f20f883e\"}";
                //  string a = param;
                byte[] btBodys = Encoding.UTF8.GetBytes(param);
                webReq.ContentLength = btBodys.Length;
                //webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(btBodys, 0, btBodys.Length);//写入参数
                newStream.Close();
                using (HttpWebResponse response = (HttpWebResponse)webReq.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader sReader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
                        {
                            ret = sReader.ReadToEnd();
                        }
                    }
                    if (k == 0)
                    {
                        ret = response.Headers["authorization"];
                    }
                }

            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }

            return ret;
        }


    }
}
