using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using znrsserver;

namespace jjpointadd32
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            timer2.Start();
        }
        #region 请求封装

        public static int bid = 6;//6号炉
        public static string nspace = "unit06";//6号炉

        private JToken JinJieHttp(DataTable dt_pk, string ptype, ref Dictionary<string, string> dicindexname)
        {
            string para = "{\"tags\":[";
            foreach (DataRow item in dt_pk.Rows)
            {
                para += "{\"items\":[\"" + ptype + "\"],\"namespace\": \"" + nspace + "\",\"tag\":\"" + item[0].ToString() + "\"},";
                dicindexname.Add(item[0].ToString(), item[2].ToString());
            }
            para = para.Substring(0, para.Length - 1) + "]}";

            string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
            //string url_write = url + "/macs/v1/realtime/write/writePoints";//锦界
            String r = HttpHelpercs.HttpPost(url_realdata, para);
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);
            JToken ja = rt["data"];
            return ja;
        }

        private JToken JinJieHttp(string para)
        {
            string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
            //string url_write = url + "/macs/v1/realtime/write/writePoints";//锦界
            String r = HttpHelpercs.HttpPost(url_realdata, para);
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);

            JToken ja = rt["data"];
            return ja;
        }

        private List<JToken> JinJieHttpMore(DataTable dt_pk, string ptype)
        {
            //string sql_pointkkscode = "SELECT kkscode,Name_kw,bhcindex,badkks,backkks,frontkks,stopkks from dncchqpoint WHERE DncBoilerId=" + bid + " and position=1";
            List<JToken> arr = new List<JToken>();
            for (int i = 3; i < 7; i++)
            {
                string para = "{\"tags\":[";
                foreach (DataRow item in dt_pk.Rows)
                {
                    para += "{\"items\":[\"" + ptype + "\"],\"namespace\": \"" + nspace + "\",\"tag\":\"" + item[i].ToString() + "\"},";
                }
                para = para.Substring(0, para.Length - 1) + "]}";

                string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
                                                                          //string url_write = url + "/macs/v1/realtime/write/writePoints";//锦界
                String r = HttpHelpercs.HttpPost(url_realdata, para);
                JObject rt = (JObject)JsonConvert.DeserializeObject(r);
                JToken ja = rt["data"];
                arr.Add(ja);
            }
            return arr;
        }

        private JToken JinJieHttp(DataTable dt_pk, string ptype)
        {
            string para = "{\"tags\":[";
            foreach (DataRow item in dt_pk.Rows)
            {
                para += "{\"items\":[\"" + ptype + "\"],\"namespace\": \"" + nspace + "\",\"tag\":\"" + item[0].ToString() + "\"},";
            }
            para = para.Substring(0, para.Length - 1) + "]}";

            string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
            //string url_write = url + "/macs/v1/realtime/write/writePoints";//锦界
            String r = HttpHelpercs.HttpPost(url_realdata, para);
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);

            JToken ja = rt["data"];
            return ja;
        }
        private JToken JinJieHttp(List<string> dt_pk, string ptype)
        {
            string para2 = "{\"tags\":[";
            foreach (string item in dt_pk)
            {
                para2 += "{\"items\":[\"" + ptype + "\"],\"namespace\": \"" + nspace + "\",\"tag\":\"" + item + "\"},";
            }
            para2 = para2.Substring(0, para2.Length - 1) + "]}";
            //  MessageBox.Show(para2);
            string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
            String r2 = HttpHelpercs.HttpPost(url_realdata, para2);
            //  MessageBox.Show(r2);
            JObject rt2 = (JObject)JsonConvert.DeserializeObject(r2);

            JToken ja2 = rt2["data"];
            return ja2;
        }

        private JToken JinJieHttp(string dt_pk, string ptype)
        {
            string para2 = "{\"tags\":[{\"items\":[\"" + ptype + "\"],\"namespace\": \"" + nspace + "\",\"tag\":\"" + dt_pk + "\"}]}";
            string url_realdata = "/macs/v1/realtime/read/findPoints";//锦界
            String r2 = HttpHelpercs.HttpPost(url_realdata, para2);
            JObject rt2 = (JObject)JsonConvert.DeserializeObject(r2);
            JToken ja2 = rt2["data"];
            return ja2;
        }

        private bool JinJieHttpDo(string dt_pk, string pmode)
        {
            string para2 = "{\"tags\":[{\"items\": [{\"item\": \"" + pmode + "\",\"value\": 1}],\"namespace\": \"" + nspace + "\",\"tag\":\"" + dt_pk + "\"}]";
            string url_realdata = "/macs/v1/realtime/write/writePoints";//锦界
            String r2 = HttpHelpercs.HttpPost(url_realdata, para2);
            JObject rt2 = (JObject)JsonConvert.DeserializeObject(r2);
            JToken ja2 = rt2["status"];
            return ja2.ToString().Equals("0");
        }


        private bool JinJieHttpDo(string dt_pk, string pmode, string value)
        {
            string para2 = "{\"tags\":[{\"items\": [{\"item\": \"" + pmode + "\",\"value\": " + value + "}],\"namespace\": \"" + nspace + "\",\"tag\":\"" + dt_pk + "\"}]";
            string url_realdata = "/macs/v1/realtime/write/writePoints";//锦界
            String r2 = HttpHelpercs.HttpPost(url_realdata, para2);
            JObject rt2 = (JObject)JsonConvert.DeserializeObject(r2);
            JToken ja2 = rt2["status"];
            return ja2.ToString().Equals("0");
        }

        #endregion

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}
