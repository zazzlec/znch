using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace znrsserver
{
    public partial class Service1 : ServiceBase
    {
        #region 全局变量
        public static string url = "http://172.21.32.160:8040";//接口地址，需要赋值
        public static string ns = "guolu";
        public static string url_realdata = url + "/v1/macs/rtdata/" + ns;
        public static string url_write = url + "/v1/macs/rtdata/" + ns + "/write";

        public static int bid = 5;
        public static string boiler_name = "";
        public static int edfh = 0;
        public static int Ch_Run = 0;//0停止 1吹灰
        public static DateTime Ch_StartTime = DateTime.MinValue;//吹灰开始时间
        public static DateTime Ch_EndTime = DateTime.MinValue;//吹灰结束时间
        //public static int bid = 5;
        //public static int bid = 5;
        public static int c = 0;
        #endregion

        #region 主线程
        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DBHelper db = new DBHelper();
            //5秒一次
            #region 获取吹灰器状态并更新dncchqpoint表
            bool bc = GetpData_chq(db);
            #endregion



            //1分钟一次
            if (c % (1*12) == 0)
            {
                AddLgoToTXT("--------------------------------------------------------");
                AddLgoToTXT("开始请求数据 ： " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                AddLgoToTXT("--------------------------------------------------------");

                #region 获取锅炉信息，将实时负荷插入负荷表

                List<Chpoint> pp = null;
              
                string sql_boiler = "select Edfh,K_Name_kw,Ch_Run,Ch_StartTime,Ch_EndTime from dncboiler where Status=1 and IsDeleted=0 and Id=" + bid;
                DataTable dt_boiler = db.GetCommand(sql_boiler);
                if (dt_boiler != null && dt_boiler.Rows.Count > 0)
                {
                    edfh = int.Parse(dt_boiler.Rows[0][0].ToString());
                    boiler_name = dt_boiler.Rows[0][1].ToString();
                    Ch_Run = int.Parse(dt_boiler.Rows[0][2].ToString());
                    Ch_StartTime = DateTime.Parse(dt_boiler.Rows[0][3].ToString());
                    Ch_EndTime = DateTime.Parse(dt_boiler.Rows[0][4].ToString());
                }
                int fhval = 0;
                bool b = GetpData(db, true, "1", out fhval);
                db.CommandExecuteNonQuery("insert into dncfhdata(RealTime,Fh_Val,Remarks,Status,IsDeleted,DncBoilerId,DncBoiler_Name) values(now()," + fhval + ",'',1,0," + bid + ",'" + bid.ToString() + "号机组');");

                #endregion

                #region 执行吹灰后30分钟后，更新水冷壁吹灰器鳍片背火侧温度差
                string qwe = "select Ch_Run  from dncchqpoint where last_temp_dif_Val >0 and DncBoilerId=" + bid;
                DataTable ee = db.GetCommand(qwe);

                if (ee.Rows.Count == 0)
                {
                    TimeSpan d3 = DateTime.Now.Subtract(Ch_EndTime);
                    if (d3.TotalMinutes > 30)
                    {
                        int g = 0;
                        b = GetpData(db, true, "77,78,79,80,81,82,83,84", out g);

                        pp = GetNowChSlb(db);

                        List<string> arr = new List<string>();
                        foreach (var item in pp)
                        {
                            string sql = "update dncchqpoint set last_temp_dif_Val=" + item.Now_temp_dif_Val + " where DncBoilerId=" + bid + " and Name_kw='IR'" + item.Id;
                            arr.Add(sql);
                        }
                        db.ExecuteTransaction(arr);
                    }
                }
                #endregion

                //5分钟一次
                if (c % (5 * 12) == 0)
                {
                    #region 5分钟
                    try
                    {

                        b = GetpData(db, false, "", out fhval);

                        #region 测点数据添加
                        List<string> arr = new List<string>();
                        string sql_point_all = "select  Name_kw,DncTypeId,DncType_Name,Pvalue from dncchpointkks where Status=1 and IsDeleted=0 and DncBoilerId=" + bid;
                        DataTable dt_pvalue = db.GetCommand(sql_point_all);
                        List<point> vl = new List<point>();
                        foreach (DataRow item in dt_pvalue.Rows)
                        {
                            point p = new point();
                            p.name = item[0].ToString();
                            p.typeid = int.Parse(item[1].ToString());
                            p.typename = item[2].ToString();
                            p.pvalue = double.Parse(item[3].ToString());
                            vl.Add(p);
                        }
                        // List<int> typelist = new List<int>();
                        int typeid = 0;
                        string typename = "";
                        string sql_pgroup = "select Id,K_Name_kw from dncchhztype where Status=1 and IsDeleted=0 and Id<=76";
                        DataTable dt_pgroup = db.GetCommand(sql_pgroup);
                        DateTime dtnow = DateTime.Now;

                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        foreach (DataRow item in dt_pgroup.Rows)
                        {
                            //typelist.Add(int.Parse(item[0].ToString()));
                            typeid = int.Parse(item[0].ToString());
                            typename = item[1].ToString();
                            JArray jar = JArray.Parse("[]");
                            var f = vl.FindAll(x => x.typeid == typeid);
                            foreach (var i in f)
                            {
                                jar.Add(i.pvalue);
                            }
                            //jar.ToString()
                            arr.Add("insert into dncchhzpoint(DncTypeId,DncType_Name,RealTime,Pvalue,Status,IsDeleted,DncBoilerId,DncBoiler_Name) values(" + typeid + ",'" + typename + "','" + dtnow + "','" + jar.ToString() + "',1,0," + bid + ",'" + bid.ToString() + "号机组')");
                            dic.Add(typeid + "", jar.ToString());
                        }

                        if (pp == null)
                        {
                            pp = GetNowChSlb(db);
                        }


                        foreach (var item in pp)
                        {
                            string sql = "update dncchqpoint set  now_temp_qp_Val=" + item.now_temp_qp_Val + "now_temp_bh_Val=" + item.now_temp_bh_Val + "now_temp_dif_Val=" + item.Now_temp_dif_Val + " where DncBoilerId=" + bid + " and Name_kw='IR'" + item.Id;
                            arr.Add(sql);
                        }

                        db.ExecuteTransaction(arr);

                        // 计算污染率，加吹灰列表
                        Znchtask(db, DateTime.Now, dic);


                        #endregion



                    }
                    catch (Exception rrr)
                    {

                        AddLgoToTXT(rrr.Message + "\n " + rrr.StackTrace);
                    }
                    #endregion
                }

                #region 加入吹灰执行列表

                Znchrun(db, DateTime.Now);

                #endregion

                #region 执行吹灰
                string sql_chexe = "";
                //先判断本体再空预器
                sql_chexe = "select * from dncboiler where Ch_Run=1 and Ch_StartTime is null and Ch_EndTime is null and NowStatus=1 and Id="+bid;
                if (db.Readcommand(sql_chexe)>0)
                {
                    //将不需要吹灰的吹灰器设置为挂牌状态（回写接口）
                    sql_chexe = "select kkscode from dncchqpoint where Name_kw not in (select Name_kw from dncchrunlist where Status=1 and IsDeleted=0 and DncBoilerId=" + bid + ") AND DncBoilerId=" + bid + " and position<>0";
                    DataTable tb = db.GetCommand(sql_chexe);

                    string para = "{\"data\":[";
                    foreach (DataRow item in tb.Rows)
                    {
                        para += "{\"name\": \""+item[0].ToString()+".JC\",\"value\":1.0 },";
                    }
                    para = para.Substring(0, para.Length) + "]}";

                    string rts= Chq_hold(para,3);

                    //吹灰器挂牌成功，调取程控吹灰组态接口
                    if (rts=="ok")
                    {
                        ///调取程控吹灰组态接口
                        ///todo



                    }


                }
                


                #endregion
            }


            c++;
        }

        #endregion

        #region 业务方法
        /// <summary>
        /// dncchpointkks调接口
        /// </summary>
        /// <param name="db"></param>
        /// <param name="fw">是否有范围</param>
        /// <param name="str">范围值</param>
        /// <param name="fh">输出负荷</param>
        /// <returns></returns>
        public bool GetpData(DBHelper db, bool fw, string str, out int fh)
        {
            fh = 0;
            string sql = "";
            if (fw)
            {
                sql = "select DISTINCT Name_kw,Pmode from dncchpointkks where DncBoilerId=" + bid + " and DncTypeId in (" + str + ") and Status=1 and IsDeleted = 0";

            }
            else
            {
                sql = "select DISTINCT Name_kw,Pmode from dncchpointkks where DncBoilerId=" + bid + " and Status=1 and IsDeleted = 0";
            }

            DataTable s2 = db.GetCommand(sql);

            string para = "{\"points\":[";
            foreach (DataRow item in s2.Rows)
            {
                para += "\"" + item[0].ToString() + (item[1].ToString().Equals("1") ? ".DV\"," : ".AV\",");
            }
            para = para.Substring(0, para.Length) + "]}";

            String r = HttpHelpercs.HttpPost(url_realdata, para, 1);
            //   AddLgoToTXT("部门数据已获取  开始同步 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);
            List<string> arr = new List<string>();


            string sql_up_pvalue = "";
            JToken ja = rt["results"]["data"];
            double value = 0d;
            for (int i = 0; i < ja.Count(); i++)
            {
                var item = ja[i];
                var name = item["name"].ToString().Replace(".AV", "").Replace(".DV", "");
                if (item["value"] != null)
                {
                    value = double.Parse(item["value"].ToString());
                }
                else
                {
                    value = 999;
                }
                if (name.Equals("AMGENMW"))
                {
                    fh = (int)value;
                }
                var timestamp = long.Parse(item["timestamp"].ToString());
                DateTime up_date = ConvertLongToDateTime(timestamp);
                sql_up_pvalue = "update dncchpointkks set Pvalue=" + value + ",RealTime='" + up_date + "' where Name_kw='" + name + "' and DncBoilerId=" + bid;
                arr.Add(sql_up_pvalue);
            }
            return db.ExecuteTransaction(arr);
        }

        /// <summary>
        /// 吹灰器状态调接口
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool GetpData_chq(DBHelper db)
        {
         
            string sql = "select DISTINCT kkscode from dncchqkks where DncBoilerId=" + bid + " and Status=1 and IsDeleted = 0";
            DataTable s2 = db.GetCommand(sql);

            string para = "{\"points\":[";
            foreach (DataRow item in s2.Rows)
            {
                para += "\"" + item[0].ToString() + ".DV\",";
            }
            para = para.Substring(0, para.Length) + "]}";

            String r = HttpHelpercs.HttpPost(url_realdata, para, 1);
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);
            List<string> arr = new List<string>();

            string sql_up_pvalue = "";
            JToken ja = rt["results"]["data"];
            double value = 0d;
            for (int i = 0; i < ja.Count(); i++)
            {
                var item = ja[i];
                var name = item["name"].ToString().Replace(".DV", "");
                if (item["value"] != null)
                {
                    value = double.Parse(item["value"].ToString());
                }
                else
                {
                    value = -1;
                }
               
                var timestamp = long.Parse(item["timestamp"].ToString());
                DateTime up_date = ConvertLongToDateTime(timestamp);
                sql_up_pvalue = "update dncchqkks set Pvalue=" + value + ",RealTime='" + up_date + "' where kkscode='" + name + "' and DncBoilerId=" + bid;
                arr.Add(sql_up_pvalue);
            }
            if (db.ExecuteTransaction(arr))
            {
                arr.Clear();
                sql = "select chq_name,chstatus_des,chstatus,updown from dncchqkks where Pvalue=1 and  DncBoilerId=" + bid;
                DataTable ts = db.GetCommand(sql);
                foreach (DataRow item in ts.Rows)
                {

                    arr.Add("update dncchqpoint set DncChstatusId="+item[2].ToString()+ ",DncChstatus_Name'"+ item[1].ToString() + "',updown="+ item[3].ToString()+ " where Name_kw='"+ item[0].ToString() + "' and DncBoilerId=" + bid);
                }
                if (db.ExecuteTransaction(arr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 数据回写调用接口，
        /// </summary>
        /// <param name="para">传入接口的参数</param>
        /// <param name="k">1：模拟量回写 2：开关量回写 3：吹灰器挂牌</param>
        /// <returns></returns>
        public string Chq_hold(string para,int k)
        {
            String r = HttpHelpercs.HttpPost(url_write, para, 1);
            JObject rt = (JObject)JsonConvert.DeserializeObject(r);
            List<string> arr = new List<string>();
            JToken ja = rt["results"]["data"];
            string rts = "";
            string rps = "";
            switch (k)
            {
                case 1:
                rps = ".AV";
                    break;
                case 2:
                    rps = ".DV";
                    break;
                case 3:
                    rps = ".JC";
                    break;
            }


            for (int i = 0; i < ja.Count(); i++)
            {
                var item = ja[i];
                var name = item["name"].ToString().Replace(rps, "");
                if (item["status"].ToString() !="success")
                {
                    rts += name + ",";
                }
               
            }

            if (rts == "")
            {
                return "ok";
            }
            else
            {
                rts = rts.Substring(0, rts.Length);
                AddLgoToTXT("--------------------------------------------------------");
                AddLgoToTXT(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" 以下吹灰器挂牌失败 ： ");
                AddLgoToTXT(rts);
                AddLgoToTXT("--------------------------------------------------------");
                return rts;
            }

        }

        /// <summary>
        /// 非周期性，吹灰受热面焓增计算、存储（5分钟/次） 计算污染率，加吹灰列表
        /// </summary>
        /// <param name="boilerid"></param>
        /// <param name="csyntime"></param>
        private void Znchtask(DBHelper db ,DateTime csyntime, Dictionary<string, string> dic)
        {

            try
            {
                List<string> arr = new List<string>();


                #region 参数取值
                DateTime now_time = DateTime.Now;

                double dg_zwxs_design = 0;
                double pg_zwxs_design = 0;
                double mg_zwxs_design = 0;
                double dz_zwxs_design = 0;
                double gz_zwxs_design = 0;
                double fs_zwxs_design = 0;
                double zs_zwxs_design = 0;

                //获取智能吹灰参数配置表信息
                string sql_ch_para = "select dg_zwxs_design_Val,pg_zwxs_design_Val,mg_zwxs_design_Val,dz_zwxs_design_Val,gz_zwxs_design_Val,fs_zwxs_design_Val,zs_zwxs_design_Val,ctylxs_Val,xs_py_x_modify_Val,wind_temp_in_design_Val,py_temp_in_wind_temp_modify_xs_Val,kyq_yq_ratio_low_Val,mg_wrl_low_Val,gz_wrl_low_Val from dncch_parameter where Status=1 and IsDeleted=0 and DncBoilerId=" + bid;
                DataTable dt_ch_para = db.GetCommand(sql_ch_para);
                dg_zwxs_design = double.Parse(dt_ch_para.Rows[0][0].ToString());//低过设计沾污系数
                pg_zwxs_design = double.Parse(dt_ch_para.Rows[0][1].ToString());//屏过设计沾污系数
                mg_zwxs_design = double.Parse(dt_ch_para.Rows[0][2].ToString());//末过设计沾污系数
                dz_zwxs_design = double.Parse(dt_ch_para.Rows[0][3].ToString());//低再设计沾污系数
                gz_zwxs_design = double.Parse(dt_ch_para.Rows[0][4].ToString());//高再设计沾污系数
                fs_zwxs_design = double.Parse(dt_ch_para.Rows[0][5].ToString());//分级省煤器设计沾污系数
                zs_zwxs_design = double.Parse(dt_ch_para.Rows[0][6].ToString());//主省煤器设计沾污系数
                double ctylxs = double.Parse(dt_ch_para.Rows[0][7].ToString());//给水泵中间抽头压力系数
                double xs_py_x_modify = double.Parse(dt_ch_para.Rows[0][8].ToString());//经X比修正后排烟温度变化值系数
                double wind_temp_in_design = double.Parse(dt_ch_para.Rows[0][9].ToString());//设计进口风温
                double py_temp_in_wind_temp_modify_xs = double.Parse(dt_ch_para.Rows[0][10].ToString());//经进口风温修正后排烟温度变化值系数
                double kyq_yq_ratio_low_Val = double.Parse(dt_ch_para.Rows[0][11].ToString());//空预器烟气侧效率下限
                double mg_wrl_low = double.Parse(dt_ch_para.Rows[0][12].ToString());//高过污染率下限
                double gz_wrl_low = double.Parse(dt_ch_para.Rows[0][13].ToString());//高再污染率上限
                double yl_fh_out = Compute.Avgdata(dic["1"]);//实际负荷

                string sql_area = "select id,K_Name_kw,Wrlhigh_Val from dnccharea where DncChtypeId>1 and DncBoilerId=" + bid;
                DataTable dt = db.GetCommand(sql_area);

                charea ar = new charea();
                Dictionary<string, charea> dnccharea_wrl = new Dictionary<string, charea>();
                foreach (DataRow item in dt.Rows)
                {
                    ar.id = int.Parse(item[0].ToString());
                    ar.wrl_limit = double.Parse(item[2].ToString());
                    dnccharea_wrl.Add(item[1].ToString(), ar);
                }

                #endregion

                #region 空预器计算
                string sql_kyq_run = "select Id,Name_kw,DncBoiler_Name,Lastchtime from dncchqpoint where DncChtypeId=4 and DncBoilerId=" + bid;
                string sql_exe_ky = "select * from dncchrunlist_kyq where OffTime is null or OffTime=''";
                int runnum_ky = db.GetCommand(sql_exe_ky).Rows.Count;
                DataTable dt_kyq = db.GetCommand(sql_kyq_run);
                DateTime lastruntime = DateTime.MinValue;
                int kyqid = 0;
                string kyqname = "";
                string bname = "";
                if (runnum_ky == 0 && int.Parse(csyntime.Subtract(lastruntime).Minutes.ToString()) > 30)
                {
                    double yrq_in_yw = Compute.Avgdata(dic["47"]);//预热器进口烟温
                    double yrq_yq_pd = Compute.Avgdata(dic["51"]);//实际烟气侧压降
                    double kyq_in_fw_1 = Compute.Avgdata(dic["58"]);//空预器进口一次风温
                    double kyq_out_fw_1 = Compute.Avgdata(dic["59"]);//空预器出口一次风温
                    double kyq_in_fw_2 = Compute.Avgdata(dic["60"]);//空预器进口二次风温
                    double kyq_out_fw_2 = Compute.Avgdata(dic["61"]);//空预器出口二次风温
                                                                     // double kyq_wind_2 = Compute.Sumdata(dic["62"]) / 3; //空预器二次风量，6个测点求和除以3
                    double kyq_wind_2 = Compute.Avgdata(dic["62"]);//测试环境只有一个测点，直接用
                    double kyq_in_yw = Compute.Avgdata(dic["63"]);//空预器进口烟温
                    double kyq_py_temp = Compute.Avgdata(dic["64"]);//实测空预器排烟温度
                    double kyq_in_o2 = Compute.Avgdata(dic["65"]);//空预器进口氧量
                    double kyq_out_o2 = Compute.Avgdata(dic["66"]);//空预器出口氧量
                    double kyq_wind_1 = Compute.Sumdata(dic["67"]) + Compute.Sumdata(dic["68"]) + Compute.Sumdata(dic["69"]) + Compute.Sumdata(dic["70"]) + Compute.Sumdata(dic["71"]) + Compute.Sumdata(dic["72"]);//一次风量=A磨进口一次风量+B磨进口一次风量+……+F磨进口一次风量

                    //以下为空气预热器实际烟气侧效率计算逻辑

                    double kyq_in_xs_kqgl = 21 / (21 - kyq_in_o2);//空预器进口过量空气系数
                    double kyq_out_xs_kqgl = 21 / (21 - kyq_out_o2);//空预器出口过量空气系数

                    double kyq_in_wind_temp_jq = (kyq_in_fw_1 * kyq_wind_1 + kyq_in_fw_2 * kyq_wind_2) / (kyq_wind_1 + kyq_wind_2);//加权平均后空预器进口风温
                    double kyq_out_wind_temp_jq = (kyq_out_fw_1 * kyq_wind_1 + kyq_out_fw_2 * kyq_wind_2) / (kyq_wind_1 + kyq_wind_2);//加权平均后空预器出口风温

                    double kyq_wind_temp_inout_avg = (kyq_in_wind_temp_jq + kyq_out_wind_temp_jq) / 2;//空预器进出口风温平均值
                    double kyq_gas_temp_inout_avg = 240d;//空预器进出口烟温平均值
                    double kyq_airc = Airc(kyq_wind_temp_inout_avg);  //空预器进出口平均风温对应比热
                    double kyq_gasc = 0d;//空预器进出口平均烟温（入口与零漏风）对应比热
                    double px_temp_0_modify = 0d;//零漏风修正后的排烟温度
                    double kyq_lfl = (kyq_out_xs_kqgl - kyq_in_xs_kqgl) / kyq_in_xs_kqgl * 0.9 * 100;//空预器漏风率
                    double kyq_gas_temp_inout_avg_modify = 0d;//修正后的空预器进出口烟温平均值



                    for (int i = 0; i < 20; i++)
                    {
                        kyq_gasc = Gasc(kyq_gas_temp_inout_avg);
                        px_temp_0_modify = kyq_lfl * kyq_airc * (kyq_py_temp - kyq_in_wind_temp_jq) / (100 * kyq_gasc) + kyq_py_temp;
                        kyq_gas_temp_inout_avg_modify = (kyq_in_yw + px_temp_0_modify) / 2;
                        if (Math.Abs(kyq_gas_temp_inout_avg_modify - kyq_gas_temp_inout_avg) < 0.1)
                        {

                            break;
                        }
                        else
                        {
                            kyq_gas_temp_inout_avg = kyq_gas_temp_inout_avg_modify;
                            continue;
                        }

                    }
                    kyq_gasc = Gasc(kyq_gas_temp_inout_avg_modify);
                    px_temp_0_modify = kyq_lfl * kyq_airc * (kyq_py_temp - kyq_in_wind_temp_jq) / (100 * kyq_gasc) + kyq_py_temp;
                    double x_ratio_design = 0.0039 * Math.Pow(yl_fh_out / 100, 3) - 0.0565 * Math.Pow(yl_fh_out / 100, 2) + 0.2725 * yl_fh_out / 100 + 0.2979;//设计X比
                    double x_ratio_real = (kyq_in_yw - px_temp_0_modify) / (kyq_out_wind_temp_jq - kyq_in_wind_temp_jq);//实测X比
                    double py_temp_x_modify = (x_ratio_design - x_ratio_real) * xs_py_x_modify;//经X比修正后排烟温度变化值
                    double wind_temp_in_real = kyq_wind_1 / (kyq_wind_1 + kyq_wind_2) * kyq_in_fw_1 + kyq_wind_1 / (kyq_wind_1 + kyq_wind_2) * kyq_in_fw_2;//实测进口风温

                    double py_temp_in_wind_temp_modify = (wind_temp_in_design - wind_temp_in_real) * py_temp_in_wind_temp_modify_xs;//经进口风温修正后排烟温度变化值
                    double py_temp_modify = kyq_py_temp + py_temp_x_modify + py_temp_in_wind_temp_modify;//修正后的排烟温度
                    double yq_ratio_real = (kyq_in_yw - py_temp_modify) / (kyq_in_yw - wind_temp_in_real);//实际烟气侧效率
                    arr.Clear();
                    arr.Add("update dnccharea set Wrl_Val=" + yq_ratio_real + ",RealTime='" + csyntime + "' where DncBoilerId=" + bid + " and K_Name_kw='空气预热器';");
                    arr.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["空气预热器"].id + ",'空气预热器'," + yq_ratio_real + "," + dnccharea_wrl["空气预热器"].wrl_limit + ",'"+ csyntime + "';");
                    arr.Add("update dncchpoint_wrl set Wrl_Val=" + yq_ratio_real + ",RealTime='" + csyntime + "' where DncBoilerId=" + bid + " and DncCharea_Name='空气预热器';");

                    if (yq_ratio_real < kyq_yq_ratio_low_Val)
                    {
                        

                        
                        foreach (DataRow item in dt_kyq.Rows)
                        {
                            kyqid = int.Parse(item[0].ToString());
                            kyqname = item[1].ToString();
                            bname = item[2].ToString();
                            arr.Add("insert into dncchrunlist_kyq(Name_kw, AddTime, Remarks, Status, IsDeleted, DncChqpointId, DncChqpoint_Name, DncBoilerId, DncBoiler_Name) values('" + kyqname + "', '" + csyntime + "', '" + kyqname + "', 1, 0, " + kyqid + ", '" + kyqname + "', " + bid + ", '" + bname + "'); ");
                            lastruntime = DateTime.Parse(item[3].ToString());
                        }

                        db.ExecuteTransaction(arr);
                        arr.Clear();
                    }
                }
                #endregion

                #region 锅炉本体吹灰逻辑计算

                StringBuilder sql_chlist_add = new StringBuilder();
                string sql_exe = "select * from dncchrunlist where OffTime is null or OffTime=''";
                int runnum = db.GetCommand(sql_exe).Rows.Count;

                string sql_fh = "select Fh from dncfhdata ORDER BY RealTime DESC LIMIT 11";
                DataTable dt_fh = db.GetCommand(sql_fh);
                //int[] fh = new int[10];
                bool a = true;
                if (dt_fh != null && dt_fh.Rows.Count > 0)
                {
                    for (int i = 1; i < 11; i++)
                    {
                        if (Math.Abs(int.Parse(dt_fh.Rows[i + 1][0].ToString()) - int.Parse(dt_fh.Rows[i][0].ToString())) > 5)
                        {
                            a = false;
                            break;
                        }

                    }
                }
                #region 声明变量
                double flq_press = 0;//分离器压力
                double mg_out_press_left = 0;//末过左侧出口压力
                double mg_out_press_right = 0;//末过右侧出口压力
                double dg_out_qw_left = 0;//低过左侧出口汽温测点
                double dg_out_qw_right = 0;//低过右侧出口汽温测点
                double dg_bw_left =0;//低过左侧壁温测点
                double dg_bw_right = 0;//低过右侧壁温测点
                double dg_in_temp_left = 0;//低过左侧进口温度
                double dg_in_temp_right = 0;//低过右侧进口温度
                double pg_out_qw_left =0;//屏过左侧出口汽温测点
                double pg_out_qw_right = 0;//屏过右侧出口汽温测点
                double pg_bw_left = 0;//屏过左侧壁温测点
                double pg_bw_right = 0;//屏过右侧壁温测点
                double pg_in_temp_left =0;//屏过左侧进口温度
                double pg_in_temp_right = 0;//屏过右侧进口温度
                double mg_out_qw_left = 0;//末过左侧出口汽温测点
                double mg_out_qw_right = 0;//末过右侧出口汽温测点
                double mg_bw_left = 0;//末过左侧壁温测点
                double mg_bw_right = 0;//末过右侧壁温测点
                double mg_in_temp_left = 0;//末过左侧进口温度
                double mg_in_temp_right =0;//末过右侧进口温度
                double dz_in_press_left = 0;//低再左侧入口压力值
                double dz_in_press_right = 0;//低再右侧入口压力值
                double gz_out_press_left = 0;//高再左侧出口压力值
                double gz_out_press_right = 0;//高再右侧出口压力值
                double dz_out_qw_left = 0;//低再左侧出口汽温测点
                double dz_out_qw_right = 0;//低再右侧出口汽温测点
                double dz_bw_left = 0;//低再左侧壁温测点
                double dz_bw_right =0;//低再右侧壁温测点
                double dz_in_temp_left = 0;//低再左侧进口温度 
                double dz_in_temp_right = 0;//低再右侧进口温度 
                double gz_out_qw_left = 0;//高再左侧出口汽温测点
                double gz_out_qw_right = 0;//高再右侧出口汽温测点
                double gz_bw_left = 0;//高再左侧壁温测点
                double gz_bw_right = 0;//高再右侧壁温测点
                double gz_in_temp_left = 0;//高再左侧进口温度
                double gz_in_temp_right = 0;//高再右侧进口温度
                                                                     // double yq_in_press = Compute.Avgdata(dic["39"]);//烟气侧进口压力
                double fs_out_temp = 0;//分级省煤器出口温度
                double fs_in_press = 0;//分级省煤器进口压力
                double fs_in_temp = 0;//分级省煤器进口温度
                                                               // double yq_out_press = Compute.Avgdata(dic["43"]);//烟气侧出口压力
                double zs_out_temp = 0;//主省煤器出口温度


                double fs_out_press = 0; //分级省煤器出口压力
                double zs_out_press = 0;//主省煤器出口压力
                double zs_in_press = 0;//主省煤器进口压力
                double zs_in_temp = 0;//主省煤器进口温度

                double gsll = 0;//给水流量
                double gspress = 0;//给水压力
                double gstemp = 0;//给水温度
                double sgjwq_front_temp_left =0;//事故减温器前温度（左侧）
                double sgjwq_front_temp_right = 0;//事故减温器前温度（右侧）
                double cql =0;//总抽气量

                double dg_dbkd_left =0;//低过挡板开度（左侧）
                double dg_dbkd_right = 0;//低过挡板开度（右侧）
                double dz_dbkd_left =0;//低再挡板开度（左侧）
                double dz_dbkd_right = 0;//低再挡板开度（右侧）


                double dg_xrl_design = 0;//低过设计吸热量
                double pg_xrl_design = 0;//屏过设计吸热量
                double mg_xrl_design = 0;//末过设计吸热量
                double dz_xrl_design = 0;//低再设计吸热量
                double gz_xrl_design = 0;//高再设计吸热量
                double fs_xrl_design = 0;//分省设计吸热量
                double zs_xrl_design = 0; //主省设计吸热量


                double dg_out_press_left = 0;//低过左侧出口压力
                double dg_out_temp_left = 0;//低过左侧出口温度
                double dg_out_hz_left = 0;//低过左侧出口焓
                double dg_in_press_left = 0;//低过左侧进口压力
                double dg_in_hz_left = 0;//低过左侧进口焓
                double dg_hz_left = 0;//低过左侧焓增



                double dg_out_press_right = 0;//低过右侧出口压力
                double dg_out_temp_right = 0;//低过右侧出口温度
                double dg_out_hz_right = 0;//低过右侧出口焓
                double dg_in_press_right = 0;//低过右侧进口压力
                double dg_in_hz_right = 0;//低过右侧进口焓
                double dg_hz_right = 0;//低过右侧焓增


                double pg_out_press_left = 0;//屏过左侧出口压力
                double pg_out_temp_left = 0;//屏过左侧出口温度
                double pg_out_hz_left = 0;//屏过左侧出口焓
                double pg_in_press_left = 0;//屏过左侧进口压力
                double pg_in_hz_left = 0;//屏过左侧进口焓
                double pg_hz_left = 0;//屏过左侧焓增


                double pg_out_press_right = 0;//屏过右侧出口压力
                double pg_out_temp_right =0;//屏过右侧出口温度
                double pg_out_hz_right = 0;//屏过右侧出口焓
                double pg_in_press_right = 0;//屏过右侧进口压力
                double pg_in_hz_right = 0;//屏过右侧进口焓
                double pg_hz_right = 0;//屏过右侧焓增


                double mg_out_temp_left = 0;//末过左侧出口温度
                double mg_out_hz_left = 0;//末过左侧出口焓
                double mg_in_press_left = 0;//末过左侧进口压力
                double mg_in_hz_left = 0;//末过左侧进口焓
                double mg_hz_left = 0;//末过左侧焓增


                double mg_out_temp_right = 0;//末过右侧出口温度
                double mg_out_hz_right = 0;//末过右侧出口焓
                double mg_in_press_right = 0;//末过右侧进口压力
                double mg_in_hz_right = 0;//末过右侧进口焓
                double mg_hz_right = 0;//末过右侧焓增


                double dz_out_press_left = 0;//低再左侧出口压力
                double dz_out_temp_left = 0;//低再左侧出口温度
                double dz_out_hz_left = 0;//低再左侧出口焓
                double dz_in_hz_left = 0; //低再左侧进口焓
                double dz_hz_left = 0;//低再左侧焓增


                double dz_out_press_right = 0;//低再右侧出口压力
                double dz_out_temp_right = 0;//低再右侧出口温度
                double dz_out_hz_right = 0;//低再右侧出口焓
                double dz_in_hz_right = 0; //低再右侧进口焓
                double dz_hz_right = 0;//低再右侧焓增


                double gz_out_temp_left = 0;//高再左侧出口温度
                double gz_out_hz_left = 0;//高再左侧出口焓
                double gz_in_press_left = 0;//高再左侧进口压力
                double gz_in_hz_left = 0;//高再左侧进口焓
                double gz_hz_left = 0;//高再左侧焓增


                double gz_out_temp_right = 0;//高再右侧出口温度
                double gz_out_hz_right = 0;//高再右侧出口焓
                double gz_in_press_right = 0;//高再右侧进口压力
                double gz_in_hz_right = 0;//高再右侧进口焓
                double gz_hz_right = 0;//高再右侧焓增


                double fs_out_hz =0;//分省出口焓
                double fs_in_hz = 0;//分省进口焓
                double fs_hz = 0;//分省焓增


                double zs_out_hz = 0;//主省出口焓
                double zs_in_hz = 0;//主省进口焓
                double zs_hz =0;//主省焓增


                double whz_jws = 0;//一级减温水焓值、二级减温水焓值
                double hz_jwq_front_1_left = 0; //一级减温器前过热蒸汽焓值（左侧）
                double hz_jwq_back_1_left = 0; //一级减温器后过热蒸汽焓值（左侧）
                double hz_jwq_front_1_right = 0; //一级减温器前过热蒸汽焓值（右侧）
                double hz_jwq_back_1_right = 0; //一级减温器后过热蒸汽焓值（右侧）

                double jws_1_left = 0;//一级减温水量（左侧）
                double jws_1_right = 0;//一级减温水量（右侧）

                double jws_1 = 0;//一级减温水总量

                double hz_jwq_front_2_left = 0; //二级减温器前过热蒸汽焓值（左侧）
                double hz_jwq_back_2_left = 0; //二级减温器后过热蒸汽焓值（左侧）
                double hz_jwq_front_2_right = 0; //二级减温器前过热蒸汽焓值（右侧）
                double hz_jwq_back_2_right = 0; //二级减温器后过热蒸汽焓值（右侧）

                double jws_2_left = 0;//二级减温水量（左侧）
                double jws_2_right = 0;//二级减温水量（右侧）
                double jws_2 = 0;//二级减温水总量

                double sjzr_grzq = 0;//设计再热/过热蒸汽流量比
                double grq_jws = 0;//过热器减温水总量
                double gp_out_zqll = 0;//高排出口蒸汽流量

                double sgjws_press = 0;//事故减温水压力
                double whz_sgjws = 0;//事故减温水焓值
                double hz_sgjwq_front_left = 0;//事故减温器前再热蒸汽焓值（左侧）
                double hz_sgjwq_back_left = 0;//事故减温器后再热蒸汽焓值（左侧）
                double hz_sgjwq_front_right = 0;//事故减温器前再热蒸汽焓值（右侧）
                double hz_sgjwq_back_right = 0;//事故减温器后再热蒸汽焓值（右侧）

                double zrq_zqll_single = 0;//当前单侧再热蒸汽流量
                double zrq_sgjws_left = 0;//再热器事故减温水量（左侧）

                double zrq_sgjws_right = 0;//再热器事故减温水量（右侧）

                double dzzqll_single_left = 0;//单侧低再蒸汽流量（左侧）
                double dzzqll_single_right = 0;//单侧低再蒸汽流量（右侧）

                double hz_wljwq_front_left =0;//微量减温器前再热蒸汽焓值（左侧）
                double hz_wljwq_back_left = 0;//微量减温器后再热蒸汽焓值（左侧）
                double hz_wljwq_front_right =0;//微量减温器前再热蒸汽焓值（右侧）
                double hz_wljwq_back_right = 0;//微量减温器后再热蒸汽焓值（右侧）

                double zrq_wljws_left = 0;//再热器微量减温水量（左侧）

                double zrq_wljws_right = 0;//再热器微量减温水量（右侧）


                double gzzqll_single_left = 0;//单侧高再蒸汽流量（左侧）
                double gzzqll_single_right = 0;//单侧高再蒸汽流量（右侧）



                double dg_xrl_real_left = 0;//低过左侧实际吸热量
                double dg_wrl_left = 0;//低过左侧污染率
                double dg_xrl_real_right = 0;//低过右侧实际吸热量
                double dg_wrl_right = 0;//低过右侧污染率

                double pg_xrl_real_left = 0;//屏过左侧实际吸热量
                double pg_wrl_left = 0;//屏过左侧污染率
                double pg_xrl_real_right = 0;//屏过右侧实际吸热量
                double pg_wrl_right = 0;//屏过右侧污染率

                double mg_xrl_real_left = 0;//末过左侧实际吸热量
                double mg_wrl_left =0;//末过左侧污染率
                double mg_xrl_real_right = 0;//末过右侧实际吸热量
                double mg_wrl_right = 0;//末过右侧污染率

                double dz_xrl_real_left = 0;//低再左侧实际吸热量
                double dz_wrl_left = 0;//低再左侧污染率
                double dz_xrl_real_right =0;//低再右侧实际吸热量
                double dz_wrl_right = 0;//低再右侧污染率

                double gz_xrl_real_left = 0;//高再左侧实际吸热量
                double gz_wrl_left = 0;//高再左侧污染率
                double gz_xrl_real_right = 0;//高再右侧实际吸热量
                double gz_wrl_right = 0;//高再右侧污染率

                double zs_xrl_real = 0;//主省煤器实际吸热量
                double zs_wrl = 0;//主省污染率
                double fs_xrl_real = 0;//分级省煤器实际吸热量
                double fs_wrl = 0;//分级省煤器污染率

                #endregion


                


                if (yl_fh_out > 400 && Ch_Run == 0 && DateTime.Now.Subtract(Ch_EndTime).TotalMinutes >= 35 && a)
                {
                    #region 数据计算
                    flq_press = Compute.Avgdata(dic["2"]);//分离器压力
                    mg_out_press_left = Compute.Avgdata(dic["3"]);//末过左侧出口压力
                    mg_out_press_right = Compute.Avgdata(dic["4"]);//末过右侧出口压力
                    dg_out_qw_left = Compute.Avgdata(dic["5"]);//低过左侧出口汽温测点
                    dg_out_qw_right = Compute.Avgdata(dic["6"]);//低过右侧出口汽温测点
                    dg_bw_left = Compute.Avgdata(dic["7"]);//低过左侧壁温测点
                    dg_bw_right = Compute.Avgdata(dic["8"]);//低过右侧壁温测点
                    dg_in_temp_left = Compute.Avgdata(dic["9"]);//低过左侧进口温度
                    dg_in_temp_right = Compute.Avgdata(dic["10"]);//低过右侧进口温度
                    pg_out_qw_left = Compute.Avgdata(dic["11"]);//屏过左侧出口汽温测点
                    pg_out_qw_right = Compute.Avgdata(dic["12"]);//屏过右侧出口汽温测点
                    pg_bw_left = Compute.Avgdata(dic["13"]);//屏过左侧壁温测点
                    pg_bw_right = Compute.Avgdata(dic["14"]);//屏过右侧壁温测点
                    pg_in_temp_left = Compute.Avgdata(dic["15"]);//屏过左侧进口温度
                    pg_in_temp_right = Compute.Avgdata(dic["16"]);//屏过右侧进口温度
                    mg_out_qw_left = Compute.Avgdata(dic["17"]);//末过左侧出口汽温测点
                    mg_out_qw_right = Compute.Avgdata(dic["18"]);//末过右侧出口汽温测点
                    mg_bw_left = Compute.Avgdata(dic["19"]);//末过左侧壁温测点
                    mg_bw_right = Compute.Avgdata(dic["20"]);//末过右侧壁温测点
                    mg_in_temp_left = Compute.Avgdata(dic["21"]);//末过左侧进口温度
                    mg_in_temp_right = Compute.Avgdata(dic["22"]);//末过右侧进口温度
                    dz_in_press_left = Compute.Avgdata(dic["23"]);//低再左侧入口压力值
                    dz_in_press_right = Compute.Avgdata(dic["24"]);//低再右侧入口压力值
                    gz_out_press_left = Compute.Avgdata(dic["25"]);//高再左侧出口压力值
                    gz_out_press_right = Compute.Avgdata(dic["26"]);//高再右侧出口压力值
                    dz_out_qw_left = Compute.Avgdata(dic["27"]);//低再左侧出口汽温测点
                    dz_out_qw_right = Compute.Avgdata(dic["28"]);//低再右侧出口汽温测点
                    dz_bw_left = Compute.Avgdata(dic["29"]);//低再左侧壁温测点
                    dz_bw_right = Compute.Avgdata(dic["30"]);//低再右侧壁温测点
                    dz_in_temp_left = Compute.Avgdata(dic["31"]);//低再左侧进口温度 
                    dz_in_temp_right = Compute.Avgdata(dic["32"]);//低再右侧进口温度 
                    gz_out_qw_left = Compute.Avgdata(dic["33"]);//高再左侧出口汽温测点
                    gz_out_qw_right = Compute.Avgdata(dic["34"]);//高再右侧出口汽温测点
                    gz_bw_left = Compute.Avgdata(dic["35"]);//高再左侧壁温测点
                    gz_bw_right = Compute.Avgdata(dic["36"]);//高再右侧壁温测点
                    gz_in_temp_left = Compute.Avgdata(dic["37"]);//高再左侧进口温度
                    gz_in_temp_right = Compute.Avgdata(dic["38"]);//高再右侧进口温度
                                                                  //  yq_in_press = Compute.Avgdata(dic["39"]);//烟气侧进口压力
                    fs_out_temp = Compute.Avgdata(dic["40"]);//分级省煤器出口温度
                    fs_in_press = Compute.Avgdata(dic["41"]);//分级省煤器进口压力
                    fs_in_temp = Compute.Avgdata(dic["42"]);//分级省煤器进口温度
                                                            //  yq_out_press = Compute.Avgdata(dic["43"]);//烟气侧出口压力
                    zs_out_temp = Compute.Avgdata(dic["44"]);//主省煤器出口温度


                    fs_out_press = fs_in_press - 0.04; //分级省煤器出口压力
                    zs_out_press = fs_in_press - 0.12;//主省煤器出口压力
                    zs_in_press = fs_out_press;//主省煤器进口压力
                    zs_in_temp = Compute.Avgdata(dic["46"]);//主省煤器进口温度

                    gsll = Compute.Avgdata(dic["52"]);//给水流量
                    gspress = Compute.Avgdata(dic["53"]);//给水压力
                    gstemp = Compute.Avgdata(dic["54"]);//给水温度
                    sgjwq_front_temp_left = Compute.Avgdata(dic["55"]);//事故减温器前温度（左侧）
                    sgjwq_front_temp_right = Compute.Avgdata(dic["56"]);//事故减温器前温度（右侧）
                    cql = Compute.Avgdata(dic["57"]);//总抽气量

                    dg_dbkd_left = Compute.Avgdata(dic["73"]);//低过挡板开度（左侧）
                    dg_dbkd_right = Compute.Avgdata(dic["74"]);//低过挡板开度（右侧）
                    dz_dbkd_left = Compute.Avgdata(dic["75"]);//低再挡板开度（左侧）
                    dz_dbkd_right = Compute.Avgdata(dic["76"]);//低再挡板开度（右侧）


                    dg_xrl_design = (0.0336 * Math.Pow(yl_fh_out / 6.6, 2) - 2.2052 * (yl_fh_out / 6.6) + 107.85) / 2;//低过设计吸热量
                    pg_xrl_design = (-0.0106 * Math.Pow(yl_fh_out / 6.6, 2) + 6.4571 * (yl_fh_out / 6.6) + 39.429) / 2;//屏过设计吸热量
                    mg_xrl_design = (0.004 * Math.Pow(yl_fh_out / 6.6, 2) + 2.4461 * (yl_fh_out / 6.6) + 4.1171) / 2;//末过设计吸热量
                    dz_xrl_design = (0.0045 * Math.Pow(yl_fh_out / 6.6, 2) + 4.721 * (yl_fh_out / 6.6) + 18.6) / 2;//低再设计吸热量
                    gz_xrl_design = (0.0215 * Math.Pow(yl_fh_out / 6.6, 2) + 2.1836 * (yl_fh_out / 6.6) + 1.9571) / 2;//高再设计吸热量
                    fs_xrl_design = 0.009 * Math.Pow(yl_fh_out / 6.6, 2) - 1.01 * (yl_fh_out / 6.6) + 69.4;//分省设计吸热量
                    zs_xrl_design = 0.0411 * Math.Pow(yl_fh_out / 6.6, 2) - 2.7678 * (yl_fh_out / 6.6) + 171.51; //主省设计吸热量


                    dg_out_press_left = flq_press - (flq_press - mg_out_press_left) / 4 * 2;//低过左侧出口压力
                    dg_out_temp_left = dg_out_qw_left * 0.95 + dg_bw_left * 0.05;//低过左侧出口温度
                    dg_out_hz_left = Steamhz(dg_out_temp_left, dg_out_press_left);//低过左侧出口焓
                    dg_in_press_left = flq_press - (flq_press - mg_out_press_left) / 4 * 1;//低过左侧进口压力
                    dg_in_hz_left = Steamhz(dg_in_temp_left, dg_in_press_left);//低过左侧进口焓
                    dg_hz_left = dg_out_hz_left - dg_in_hz_left;//低过左侧焓增



                    dg_out_press_right = flq_press - (flq_press - mg_out_press_right) / 4 * 2;//低过右侧出口压力
                    dg_out_temp_right = dg_out_qw_right * 0.95 + dg_bw_right * 0.05;//低过右侧出口温度
                    dg_out_hz_right = Steamhz(dg_out_temp_right, dg_out_press_right);//低过右侧出口焓
                    dg_in_press_right = flq_press - (flq_press - mg_out_press_right) / 4 * 1;//低过右侧进口压力
                    dg_in_hz_right = Steamhz(dg_in_temp_right, dg_in_press_right);//低过右侧进口焓
                    dg_hz_right = dg_out_hz_right - dg_in_hz_right;//低过右侧焓增


                    pg_out_press_left = flq_press - (flq_press - mg_out_press_left) / 4 * 3;//屏过左侧出口压力
                    pg_out_temp_left = pg_out_qw_left * 0.95 + pg_bw_left * 0.05;//屏过左侧出口温度
                    pg_out_hz_left = Steamhz(pg_out_temp_left, pg_out_press_left);//屏过左侧出口焓
                    pg_in_press_left = flq_press - (flq_press - mg_out_press_left) / 4 * 2;//屏过左侧进口压力
                    pg_in_hz_left = Steamhz(pg_in_temp_left, pg_in_press_left);//屏过左侧进口焓
                    pg_hz_left = pg_out_hz_left - pg_in_hz_left;//屏过左侧焓增


                    pg_out_press_right = flq_press - (flq_press - mg_out_press_right) / 4 * 3;//屏过右侧出口压力
                    pg_out_temp_right = pg_out_qw_right * 0.95 + pg_bw_right * 0.05;//屏过右侧出口温度
                    pg_out_hz_right = Steamhz(pg_out_temp_right, pg_out_press_right);//屏过右侧出口焓
                    pg_in_press_right = flq_press - (flq_press - mg_out_press_right) / 4 * 2;//屏过右侧进口压力
                    pg_in_hz_right = Steamhz(pg_in_temp_right, pg_in_press_right);//屏过右侧进口焓
                    pg_hz_right = pg_out_hz_right - pg_in_hz_right;//屏过右侧焓增


                    mg_out_temp_left = mg_out_qw_left * 0.9 + mg_bw_left * 0.1;//末过左侧出口温度
                    mg_out_hz_left = Steamhz(mg_out_temp_left, mg_out_press_left);//末过左侧出口焓
                    mg_in_press_left = flq_press - (flq_press - mg_out_press_left) / 4 * 3;//末过左侧进口压力
                    mg_in_hz_left = Steamhz(mg_in_temp_left, mg_in_press_left);//末过左侧进口焓
                    mg_hz_left = mg_out_hz_left - mg_in_hz_left;//末过左侧焓增


                    mg_out_temp_right = mg_out_qw_right * 0.9 + mg_bw_right * 0.1;//末过右侧出口温度
                    mg_out_hz_right = Steamhz(mg_out_temp_right, mg_out_press_right);//末过右侧出口焓
                    mg_in_press_right = flq_press - (flq_press - mg_out_press_right) / 4 * 3;//末过右侧进口压力
                    mg_in_hz_right = Steamhz(mg_in_temp_right, mg_in_press_right);//末过右侧进口焓
                    mg_hz_right = mg_out_hz_right - mg_in_hz_right;//末过右侧焓增


                    dz_out_press_left = dz_in_press_left - (dz_in_press_left - gz_out_press_left) / 2;//低再左侧出口压力
                    dz_out_temp_left = dz_out_qw_left * 0.95 + dz_bw_left * 0.05;//低再左侧出口温度
                    dz_out_hz_left = Steamhz(dz_out_temp_left, dz_out_press_left);//低再左侧出口焓
                    dz_in_hz_left = Steamhz(dz_in_temp_left, dz_in_press_left); //低再左侧进口焓
                    dz_hz_left = dz_out_hz_left - dz_in_hz_left;//低再左侧焓增


                    dz_out_press_right = dz_in_press_right - (dz_in_press_right - gz_out_press_right) / 2;//低再右侧出口压力
                    dz_out_temp_right = dz_out_qw_right * 0.95 + dz_bw_right * 0.05;//低再右侧出口温度
                    dz_out_hz_right = Steamhz(dz_out_temp_right, dz_out_press_right);//低再右侧出口焓
                    dz_in_hz_right = Steamhz(dz_in_temp_right, dz_in_press_right); //低再右侧进口焓
                    dz_hz_right = dz_out_hz_right - dz_in_hz_right;//低再右侧焓增


                    gz_out_temp_left = gz_out_qw_left * 0.9 + gz_bw_left * 0.1;//高再左侧出口温度
                    gz_out_hz_left = Steamhz(gz_out_temp_left, gz_out_press_left);//高再左侧出口焓
                    gz_in_press_left = dz_out_press_left;//高再左侧进口压力
                    gz_in_hz_left = Steamhz(gz_in_temp_left, gz_in_press_left);//高再左侧进口焓
                    gz_hz_left = gz_out_hz_left - gz_in_hz_left;//高再左侧焓增


                    gz_out_temp_right = gz_out_qw_right * 0.9 + gz_bw_right * 0.1;//高再右侧出口温度
                    gz_out_hz_right = Steamhz(gz_out_temp_right, gz_out_press_right);//高再右侧出口焓
                    gz_in_press_right = dz_out_press_right;//高再右侧进口压力
                    gz_in_hz_right = Steamhz(gz_in_temp_right, gz_in_press_right);//高再右侧进口焓
                    gz_hz_right = gz_out_hz_right - gz_in_hz_right;//高再右侧焓增


                    fs_out_hz = Steamhz(fs_out_temp, fs_out_press);//分省出口焓
                    fs_in_hz = Steamhz(fs_in_temp, fs_in_press);//分省进口焓
                    fs_hz = fs_out_hz - fs_in_hz;//分省焓增


                    zs_out_hz = Steamhz(zs_out_temp, zs_out_press);//主省出口焓
                    zs_in_hz = Steamhz(zs_in_temp, zs_in_press);//主省进口焓
                    zs_hz = zs_out_hz - zs_in_hz;//主省焓增


                    whz_jws = Whz(gstemp, gspress);//一级减温水焓值、二级减温水焓值
                    hz_jwq_front_1_left = Steamhz(dg_out_temp_left, pg_in_press_left); //一级减温器前过热蒸汽焓值（左侧）
                    hz_jwq_back_1_left = Steamhz(pg_in_temp_left, pg_in_press_left); //一级减温器后过热蒸汽焓值（左侧）
                    hz_jwq_front_1_right = Steamhz(dg_out_temp_right, pg_in_press_right); //一级减温器前过热蒸汽焓值（右侧）
                    hz_jwq_back_1_right = Steamhz(pg_in_temp_right, pg_in_press_right); //一级减温器后过热蒸汽焓值（右侧）

                    jws_1_left = (hz_jwq_front_1_left - hz_jwq_back_1_left) * gsll / 2 / (hz_jwq_back_1_left - whz_jws);//一级减温水量（左侧）
                    if (jws_1_left < 0)
                    {
                        jws_1_left = 0;
                    }
                    jws_1_right = (hz_jwq_front_1_right - hz_jwq_back_1_right) * gsll / 2 / (hz_jwq_back_1_right - whz_jws);//一级减温水量（右侧）
                    if (jws_1_right < 0)
                    {
                        jws_1_right = 0;
                    }
                    jws_1 = jws_1_left + jws_1_right;//一级减温水总量

                    hz_jwq_front_2_left = Steamhz(pg_out_temp_left, mg_in_press_left); //二级减温器前过热蒸汽焓值（左侧）
                    hz_jwq_back_2_left = Steamhz(mg_in_temp_left, mg_in_press_left); //二级减温器后过热蒸汽焓值（左侧）
                    hz_jwq_front_2_right = Steamhz(pg_out_temp_right, mg_in_press_right); //二级减温器前过热蒸汽焓值（右侧）
                    hz_jwq_back_2_right = Steamhz(mg_in_temp_right, mg_in_press_right); //二级减温器后过热蒸汽焓值（右侧）

                    jws_2_left = (hz_jwq_front_2_left - hz_jwq_back_2_left) * (gsll + jws_1) / 2 / (hz_jwq_back_2_left - whz_jws);//二级减温水量（左侧）
                    if (jws_2_left < 0)
                    {
                        jws_2_left = 0;
                    }
                    jws_2_right = (hz_jwq_front_2_right - hz_jwq_back_2_right) * (gsll + jws_1) / 2 / (hz_jwq_back_2_right - whz_jws);//二级减温水量（右侧）
                    if (jws_2_right < 0)
                    {
                        jws_2_right = 0;
                    }
                    jws_2 = jws_2_left + jws_2_right;//二级减温水总量

                    sjzr_grzq = 0.0017 * Math.Pow(yl_fh_out / 100, 3) - 0.0175 * Math.Pow(yl_fh_out / 100, 2) + 0.0381 * (yl_fh_out / 100) + 0.8318;//设计再热/过热蒸汽流量比
                    grq_jws = jws_1 + jws_2;//过热器减温水总量
                    gp_out_zqll = sjzr_grzq * (gsll + grq_jws);//高排出口蒸汽流量

                    sgjws_press = ctylxs * gspress;//事故减温水压力
                    whz_sgjws = Whz(gstemp, sgjws_press);//事故减温水焓值
                    hz_sgjwq_front_left = Steamhz(sgjwq_front_temp_left, dz_in_press_left);//事故减温器前再热蒸汽焓值（左侧）
                    hz_sgjwq_back_left = Steamhz(dz_in_temp_left, dz_in_press_left);//事故减温器后再热蒸汽焓值（左侧）
                    hz_sgjwq_front_right = Steamhz(sgjwq_front_temp_right, dz_in_press_right);//事故减温器前再热蒸汽焓值（右侧）
                    hz_sgjwq_back_right = Steamhz(dz_in_temp_right, dz_in_press_right);//事故减温器后再热蒸汽焓值（右侧）

                    zrq_zqll_single = gp_out_zqll / 2;//当前单侧再热蒸汽流量
                    zrq_sgjws_left = (hz_sgjwq_front_left - hz_sgjwq_back_left) * (zrq_zqll_single - cql / 2) / (hz_sgjwq_back_left - whz_sgjws);//再热器事故减温水量（左侧）
                    if (zrq_sgjws_left < 0)
                    {
                        zrq_sgjws_left = 0;
                    }
                    zrq_sgjws_right = (hz_sgjwq_front_right - hz_sgjwq_back_right) * (zrq_zqll_single - cql / 2) / (hz_sgjwq_back_right - whz_sgjws);//再热器事故减温水量（右侧）
                    if (zrq_sgjws_right < 0)
                    {
                        zrq_sgjws_right = 0;
                    }
                    dzzqll_single_left = (gp_out_zqll - cql) / 2 + zrq_sgjws_left;//单侧低再蒸汽流量（左侧）
                    dzzqll_single_right = (gp_out_zqll - cql) / 2 + zrq_sgjws_right;//单侧低再蒸汽流量（右侧）

                    hz_wljwq_front_left = Steamhz(dz_out_temp_left, gz_in_press_left);//微量减温器前再热蒸汽焓值（左侧）
                    hz_wljwq_back_left = Steamhz(gz_in_temp_left, gz_in_press_left);//微量减温器后再热蒸汽焓值（左侧）
                    hz_wljwq_front_right = Steamhz(dz_out_temp_right, gz_in_press_right);//微量减温器前再热蒸汽焓值（右侧）
                    hz_wljwq_back_right = Steamhz(gz_in_temp_right, gz_in_press_right);//微量减温器后再热蒸汽焓值（右侧）

                    zrq_wljws_left = (hz_wljwq_front_left - hz_wljwq_back_left) * dzzqll_single_left / (hz_wljwq_back_left - whz_sgjws);//再热器微量减温水量（左侧）

                    zrq_wljws_right = (hz_wljwq_front_right - hz_wljwq_back_right) * dzzqll_single_right / (hz_wljwq_back_right - whz_sgjws);//再热器微量减温水量（右侧）


                    gzzqll_single_left = dzzqll_single_left + zrq_wljws_left;//单侧高再蒸汽流量（左侧）
                    gzzqll_single_right = dzzqll_single_right + zrq_wljws_right;//单侧高再蒸汽流量（右侧）



                    dg_xrl_real_left = dg_hz_left * (gsll / 2) / 1000;//低过左侧实际吸热量
                    dg_wrl_left = dg_xrl_design / dg_xrl_real_left;//低过左侧污染率
                    dg_xrl_real_right = dg_hz_right * (gsll / 2) / 1000;//低过右侧实际吸热量
                    dg_wrl_right = dg_xrl_design / dg_xrl_real_right;//低过右侧污染率

                    pg_xrl_real_left = pg_hz_left * (gsll / 2 + jws_1_left) / 1000;//屏过左侧实际吸热量
                    pg_wrl_left = pg_xrl_design / pg_xrl_real_left;//屏过左侧污染率
                    pg_xrl_real_right = pg_hz_right * (gsll / 2 + jws_1_right) / 1000;//屏过右侧实际吸热量
                    pg_wrl_right = pg_xrl_design / pg_xrl_real_right;//屏过右侧污染率

                    mg_xrl_real_left = mg_hz_left * (gsll / 2 + grq_jws) / 1000;//末过左侧实际吸热量
                    mg_wrl_left = mg_xrl_design / mg_xrl_real_left;//末过左侧污染率
                    mg_xrl_real_right = mg_hz_right * (gsll / 2 + grq_jws) / 1000;//末过右侧实际吸热量
                    mg_wrl_right = mg_xrl_design / mg_xrl_real_right;//末过右侧污染率

                    dz_xrl_real_left = dz_hz_left * dzzqll_single_left / 1000;//低再左侧实际吸热量
                    dz_wrl_left = dz_xrl_design / dz_xrl_real_left;//低再左侧污染率
                    dz_xrl_real_right = dz_hz_right * dzzqll_single_right / 1000;//低再右侧实际吸热量
                    dz_wrl_right = dz_xrl_design / dz_xrl_real_right;//低再右侧污染率

                    gz_xrl_real_left = gz_hz_left * gzzqll_single_left / 1000;//高再左侧实际吸热量
                    gz_wrl_left = gz_xrl_design / gz_xrl_real_left;//高再左侧污染率
                    gz_xrl_real_right = gz_hz_right * gzzqll_single_right / 1000;//高再右侧实际吸热量
                    gz_wrl_right = gz_xrl_design / gz_xrl_real_right;//高再右侧污染率

                    zs_xrl_real = zs_hz * gsll / 1000;//主省煤器实际吸热量
                    zs_wrl = zs_xrl_design / zs_xrl_real;//主省污染率
                    fs_xrl_real = fs_hz * gsll / 1000;//分级省煤器实际吸热量
                    fs_wrl = fs_xrl_design / fs_xrl_real;//分级省煤器污染率

                    #endregion
                    
                    #region 水冷壁非周期性

                    double mg_wrl = (mg_wrl_left + mg_wrl_right) / 2;
                    double gz_wrl = (gz_wrl_left + gz_wrl_right) / 2;

                    if (mg_wrl < mg_wrl_low && gz_wrl < gz_wrl_low)
                    {
                        // string sql_chlist = "select Name_kw,Wrl_Val,Wrlhigh_Val,last_temp_dif_Val,now_temp_dif_Val,dx_temp_dif_Val,slb_circle_num,slb_floor_Val from V_chwrl where Status=1  and DncBoilerId=" + boilerid;
                        string sql_chlist = "select Id,Name_kw,slb_floor_Val,last_temp_dif_Val,now_temp_dif_Val,DncBoiler_Name from dncchqpoint where DncChtypeId=1 and DncBoilerId=" + bid;
                        DataTable dt_chlist = db.GetCommand(sql_chlist);
                        var list1 = new List<Chpoint>();
                        var list2 = new List<Chpoint>();
                        var list3 = new List<Chpoint>();
                        var list4 = new List<Chpoint>();
                        foreach (DataRow item in dt_chlist.Rows)
                        {
                            var obj = new Chpoint();
                            obj.Id = int.Parse(item[0].ToString());
                            obj.Name_kw = item[1].ToString();
                            obj.Slb_floor_Val = int.Parse(item[2].ToString());
                            obj.Last_temp_dif_Val = double.Parse(item[3].ToString());
                            obj.Now_temp_dif_Val = double.Parse(item[4].ToString());
                            obj.Last_now_dif = double.Parse(item[3].ToString()) - double.Parse(item[4].ToString());
                            obj.DncBoilerId = bid;
                            obj.DncBoiler_Name = item[5].ToString();
                            if (obj.Id == 1)
                            {
                                list1.Add(obj);
                            }
                            if (obj.Id == 2)
                            {
                                list2.Add(obj);
                            }
                            if (obj.Id == 3)
                            {
                                list3.Add(obj);
                            }
                            if (obj.Id == 4)
                            {
                                list4.Add(obj);
                            }

                        }
                        
                        List<Chpoint> chlist = new List<Chpoint>();
                        chlist = Chlist(list1).Concat(Chlist(list2)).Concat(Chlist(list3)).Concat(Chlist(list4)).ToList();
                        int cl_num = chlist.Count;

                        for (int i = 0; i < cl_num; i++)
                        {
                            int id = chlist[i].Id;

                            string chq_name = chlist[i].Name_kw;
                            arr.Add("insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted) values('" + chq_name + "','" + now_time + "'," + id + ",'" + chq_name + "',1," + bid + ",'" + bname + "',1,0);");

                        }

                    }
                    #endregion

                    
                }
                #region 污染率存储


                List<string> arr2 = new List<string>();
                now_time = csyntime;

                arr2.Add("update dnccharea set Wrl_Val=" + pg_wrl_left + ",RealTime='" + now_time + "'where DncBoilerId=" + bid + " and K_Name_kw='屏式过热器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["屏式过热器（左侧）"].id + ",'屏式过热器（左侧）'," + pg_wrl_left + "," + dnccharea_wrl["屏式过热器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + pg_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='屏式过热器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["屏式过热器（右侧）"].id + ",'屏式过热器（右侧）'," + pg_wrl_right + "," + dnccharea_wrl["屏式过热器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + mg_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='高温过热器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["高温过热器（左侧）"].id + ",'高温过热器（左侧）'," + mg_wrl_left + "," + dnccharea_wrl["高温过热器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + mg_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='高温过热器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["高温过热器（右侧）"].id + ",'高温过热器（右侧）'," + mg_wrl_right + "," + dnccharea_wrl["高温过热器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + gz_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='高温再热器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["高温再热器（左侧）"].id + ",'高温再热器（左侧）'," + gz_wrl_left + "," + dnccharea_wrl["高温再热器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + gz_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='高温再热器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["高温再热器（右侧）"].id + ",'高温再热器（右侧）'," + gz_wrl_right + "," + dnccharea_wrl["高温再热器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + dz_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='低温再热器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["低温再热器（左侧）"].id + ",'低温再热器（左侧）'," + dz_wrl_left + "," + dnccharea_wrl["低温再热器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + dz_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='低温再热器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["低温再热器（右侧）"].id + ",'低温再热器（右侧）'," + dz_wrl_right + "," + dnccharea_wrl["低温再热器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + dg_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='低温过热器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["低温过热器（左侧）"].id + ",'低温过热器（左侧）'," + dg_wrl_left + "," + dnccharea_wrl["低温过热器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + dg_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='低温过热器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["低温过热器（右侧）"].id + ",'低温过热器（右侧）'," + dg_wrl_right + "," + dnccharea_wrl["低温过热器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + zs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='主省煤器（左侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["主省煤器（左侧）"].id + ",'主省煤器（左侧）'," + zs_wrl + "," + dnccharea_wrl["主省煤器（左侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + zs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='主省煤器（右侧）';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["主省煤器（右侧）"].id + ",'主省煤器（右侧）'," + zs_wrl + "," + dnccharea_wrl["主省煤器（右侧）"].wrl_limit + ",'" + now_time + "';");
                arr2.Add("update dnccharea set Wrl_Val=" + fs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and K_Name_kw='分级省煤器';");
                arr2.Add("insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0," + bid + ",'" + bname + "'," + dnccharea_wrl["分级省煤器"].id + ",'分级省煤器'," + fs_wrl + "," + dnccharea_wrl["分级省煤器"].wrl_limit + ",'" + now_time + "';");

                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + pg_wrl_left + ",RealTime='" + now_time + "'where DncBoilerId=" + bid + " and DncCharea_Name='屏式过热器（左侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + pg_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='屏式过热器（右侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + mg_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='高温过热器（左侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + mg_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='高温过热器（右侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + gz_wrl_left + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='高温再热器（左侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + gz_wrl_right + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='高温再热器（右侧）';");
                if (dz_dbkd_left > 60)
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dz_wrl_left + ",RealTime='" + now_time + "',Status=1 where DncBoilerId=" + bid + " and DncCharea_Name='低温再热器（左侧）';");
                }
                else
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dz_wrl_left + ",RealTime='" + now_time + "',Status=0 where DncBoilerId=" + bid + " and DncCharea_Name='低温再热器（左侧）';");
                }
                if (dz_dbkd_right > 60)
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dz_wrl_right + ",RealTime='" + now_time + "',Status=1 where DncBoilerId=" + bid + " and DncCharea_Name='低温再热器（右侧）';");
                }
                else
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dz_wrl_right + ",RealTime='" + now_time + "',Status=0 where DncBoilerId=" + bid + " and DncCharea_Name='低温再热器（右侧）';");
                }
                if (dg_dbkd_left > 50)
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dg_wrl_left + ",RealTime='" + now_time + "',Status=1 where DncBoilerId=" + bid + " and DncCharea_Name='低温过热器（左侧）';");
                }
                else
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dg_wrl_left + ",RealTime='" + now_time + "',Status=0 where DncBoilerId=" + bid + " and DncCharea_Name='低温过热器（左侧）';");
                }
                if (dg_dbkd_right > 50)
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dg_wrl_right + ",RealTime='" + now_time + "',Status=1 where DncBoilerId=" + bid + " and DncCharea_Name='低温过热器（右侧）';");
                }
                else
                {
                    arr2.Add("update dncchpoint_wrl set Wrl_Val=" + dg_wrl_right + ",RealTime='" + now_time + "',Status=0 where DncBoilerId=" + bid + " and DncCharea_Name='低温过热器（右侧）';");
                }
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + zs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='主省煤器（左侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + zs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='主省煤器（右侧）';");
                arr2.Add("update dncchpoint_wrl set Wrl_Val=" + fs_wrl + ",RealTime='" + now_time + "' where DncBoilerId=" + bid + " and DncCharea_Name='分级省煤器';");


                now_time = DateTime.Now;



                if (arr2.Count > 0)
                {
                    db.ExecuteTransaction(arr2);
                }
                #endregion

                #region 尾部烟道污染率超标加入待吹灰列表
                string sql_chlist_else1 = "select DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name from dncchpoint_wrl where Status=1 and IsDeleted=0 and (DncChtypeId=2 or DncChtypeId=3) and Wrl_Val>Wrlhigh_Val and DncBoilerId=" + bid;
                DataTable dt_chlist_else1 = db.GetCommand(sql_chlist_else1);
 
                if (dt_chlist_else1 != null && dt_chlist_else1.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_chlist_else1.Rows)
                    {
                        arr.Add("insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted) values('" + item[1].ToString() + "','" + now_time + "'," + int.Parse(item[0].ToString()) + ",'" + item[1].ToString() + "',1," + int.Parse(item[2].ToString()) + ",'" + item[3].ToString() + "',1,0);");
                    }

                }
                #endregion

                #region 水平烟道超过10天或者尾部烟道超过20天未吹

                string sql_yd = "select Id,Name_kw,DncBoilerId,DncBoiler_Name from dncchqpoint where DncBoilerId=" + bid + " and  ((position=2 and TIMESTAMPDIFF(DAY,Lastchtime,DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%S'))>=10) or (position=3 and TIMESTAMPDIFF(DAY,Lastchtime,DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%S'))>=20))";
                DataTable dt_yd = db.GetCommand(sql_yd);

                if (dt_yd != null && dt_yd.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_yd.Rows)
                    {

                        arr.Add("insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted) values('" + item[1].ToString() + "','" + now_time + "'," + int.Parse(item[0].ToString()) + ",'" + item[1].ToString() + "',1," + int.Parse(item[2].ToString()) + ",'" + item[3].ToString() + "',1,0);");
                    }
                }
                #endregion

                #region 负荷低于330MV超过6小时，将IK21和IK22加入列表
                string sql_fh_6 = "select TIMESTAMPDIFF(HOUR, RealTime, now()) RealTime from dncfhdata where Fh_Val>=330 order by RealTime desc LIMIT 1";
                DataTable dt_fh_6 = db.GetCommand(sql_fh_6);
                int hour_fh6 = int.Parse(dt_fh_6.Rows[0][0].ToString());


                if (runnum == 0 && hour_fh6 >= 6)
                {
                    string sql_fh6_point = "select Id,Name_kw,DncBoilerId,DncBoiler_Name from dncchqpoint where DncBoilerId=" + bid + " and (Name_kw='IK21' or Name_kw='IK22')";
                    DataTable dt_fh6_p = db.GetCommand(sql_fh6_point);
                    foreach (DataRow item in dt_fh6_p.Rows)
                    {
                        arr.Add("insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted) values('" + item[1].ToString() + "','" + now_time + "'," + int.Parse(item[0].ToString()) + ",'" + item[1].ToString() + "',1," + int.Parse(item[2].ToString()) + ",'" + item[3].ToString() + "',1,0);");
                    }
                }
                #endregion

                if (arr.Count>0)
                {
                    db.ExecuteTransaction(arr);
                }

                #endregion
            }

            catch (Exception rrr)
            {
                AddLgoToTXT(rrr.Message + "\n " + rrr.StackTrace);
            }

        }

        /// <summary>
        /// 获取当前水冷壁吹灰器数据
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private List<Chpoint> GetNowChSlb(DBHelper db)
        {
            List<Chpoint> cp = new List<Chpoint>();
            Dictionary<int, double> dic1 = new Dictionary<int, double>();
            Dictionary<int, double> dic2 = new Dictionary<int, double>();
            string sql_ch_para = "select Pvalue,Slb_Val,DncTypeId from dncchpointkks where DncTypeId in (77,78,79,80) and DncBoilerId=" + bid;
            DataTable dt_ch_para = db.GetCommand(sql_ch_para);
            foreach (DataRow item in dt_ch_para.Rows)
            {
                dic1.Add(int.Parse(item[1].ToString()),double.Parse(item[0].ToString()));
            }

            sql_ch_para = "select Pvalue,Slb_Val,DncTypeId from dncchpointkks where DncTypeId in (81,82,83,84) and DncBoilerId=" + bid;
            dt_ch_para = db.GetCommand(sql_ch_para);
            foreach (DataRow item in dt_ch_para.Rows)
            {
                dic2.Add(int.Parse(item[1].ToString()), double.Parse(item[0].ToString()));
            }
            for (int i = 1; i <= 96; i++)
            {
                Chpoint c = new Chpoint();
                c.Id = i;
                c.now_temp_qp_Val = dic1[i];
                c.now_temp_bh_Val = dic2[i];
                c.Now_temp_dif_Val = dic1[i] - dic2[i];
                cp.Add(c);
            }

            return cp;
        }

        /// <summary>
        /// 判断是否加入执行列表
        /// </summary>
        /// <param name="boilerid"></param>
        /// <param name="csyntime"></param>

        private void Znchrun(DBHelper db, DateTime csyntime)
        {
            List<string> arr = new List<string>();
            int chtime_sum = 0;
            int reason = 0;
            string sql_chlist_sum = "select count(*),Ch_time_Val from v_chlist where DncBoilerId=" + bid + " GROUP BY DncChtypeId,Ch_time_Val";
            DataTable dt_chtime = db.GetCommand(sql_chlist_sum);
            if (dt_chtime != null && dt_chtime.Rows.Count > 0)
            {
                foreach (DataRow item in dt_chtime.Rows)
                {
                    int num = int.Parse(item[0].ToString());
                    int chtime = int.Parse(item[1].ToString());
                    if (num % 2 == 0)
                    {
                        chtime_sum += num / 2 * chtime;
                    }
                    else
                    {
                        chtime_sum += (num + 1) / 2 * chtime;
                    }

                }
                string sql_wrl_cx = "select * from dnccharea where DncBoilerId=" + bid + " and (DncChtypeId=2 or DncChtypeId=3) and K_Name_kw<>'分级省煤器' and ((Wrl_Val-Wrlhigh_Val)>=0.25 or ((K_Name_kw='高温过热器（左侧）' or K_Name_kw='高温过热器（右侧）') and Wrl_Val>0 and Wrl_Val<0.7 ) or ((K_Name_kw='高温再热器（左侧）' or K_Name_kw='高温再热器（右侧）') and Wrl_Val>0 and Wrl_Val<0.75 ))";
                DataTable dt_wrl_cx = db.GetCommand(sql_wrl_cx);
                string rname = "";
                if (chtime_sum > 7200)
                {
                    reason = 1;
                }
                else if (dt_wrl_cx != null && dt_wrl_cx.Rows.Count > 0)
                {
                    reason = 2;
                }
                else if ( !Ch_StartTime.Date.Equals(csyntime.Date) && csyntime > DateTime.Parse(csyntime.ToShortDateString() + " 20:00:00"))
                {
                    reason = 3;
                }

                switch (reason)
                {

                    case 1:
                        rname = "待吹灰列表达7200s";
                        break;
                    case 2:
                        rname = "污染率超限";
                        break;
                    case 3:
                        rname = "晚20点一天未吹";
                        break;

                }
                if (reason > 0)
                {
                    string sql_chlist = "select  DISTINCT DncChqpointId,DncChqpoint_Name,DncBoiler_Name from dncchlist  where Status=1 and IsDeleted=0 and DncBoilerId=" + bid;
                    DataTable dt_chlist = db.GetCommand(sql_chlist);
                    if (dt_chlist != null && dt_chlist.Rows.Count > 0)
                    {

                        
                        int chqid = 0;
                        string chqname = "";
                        string bname = "";

                        foreach (DataRow item in dt_chlist.Rows)
                        {
                            chqid = int.Parse(item[0].ToString());
                            chqname = item[1].ToString();
                            bname = item[2].ToString();
                            arr.Add("insert into dncchrunlist (Name_kw,AddTime,Remarks,Status,IsDeleted,DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name) values ('" + chqname + "','" + csyntime + "','" + rname + "',1,0," + chqid + ",'" + chqname + "'," + bid + ",'" + bname + "');");

                        }
                        arr.Add("update dncchlist set Status=0 where DncBoilerId=" + bid + ";");
                        arr.Add("update dncboiler set Ch_Run=1,Ch_StartTime=null,Ch_EndTime=null where DncBoilerId=" + bid + ";");
                        db.ExecuteTransaction(arr);
                    }

                }
            }


        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="logstring"></param>
        public static void AddLgoToTXT(string logstring)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "logs/log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            if (!System.IO.File.Exists(path))
            {
                FileStream stream = System.IO.File.Create(path);
                stream.Close();
                stream.Dispose();
            }
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(logstring);
            }
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public string GetTimeStamp()
        {
            //TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return Convert.ToInt64(ts.TotalSeconds).ToString();

            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //Ticks表示自 0001 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以 100 毫微秒为间隔的间隔数
            //除10000000调整为10位  1 毫秒 = 10^-3 秒，1 微秒 = 10 ^ -6 秒，1 毫微秒 = 10 ^ -9 秒，100 毫微秒 = 10 ^ -7 秒
            long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000000;
            return t + "";
        }

        /// <summary>        
        /// 时间戳转为C#格式时间        
        /// </summary>        
        /// <param name=”timeStamp”></param>        
        /// <returns></returns>        
        public static DateTime ConvertLongToDateTime(long timeStamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);
            long lTime = timeStamp * 10000;
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 受热面焓增计算公式
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="press"></param>
        /// <returns></returns>
        private double Steamhz(double temp, double press)
        {
            temp = temp + 273.15;//℃转成K

            return 2022.7 + 1.6675 * temp + 0.00029593 * Math.Pow(temp, 2) - 1269000000 * press / Math.Pow(temp, 2.7984) - 1.0185 * Math.Pow(10, 23) * Math.Pow(press, 2) / Math.Pow(temp, 8.3207);
        }

        /// <summary>
        /// 空气比热公式
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private double Airc(double temp)
        {
            double airc = -2 * Math.Pow(10, -15) * Math.Pow(temp, 5) + 2 * Math.Pow(10, -12) * Math.Pow(temp, 4) - 9 * Math.Pow(10, -10) * Math.Pow(temp, 3) + 3 * Math.Pow(10, -7) * Math.Pow(temp, 2) + 8 * Math.Pow(10, -6) * temp + 1.005;
            return airc;
        }

        /// <summary>
        /// 烟气比热公式
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        private double Gasc(double temp)
        {
            double gasc = 3 * Math.Pow(10, -17) * Math.Pow(temp, 5) - 9 * Math.Pow(10, -14) * Math.Pow(temp, 4) + 3 * Math.Pow(10, -11) * Math.Pow(temp, 3) + 4 * Math.Pow(10, -8) * Math.Pow(temp, 2) + Math.Pow(10, -4) * temp + 0.9837;
            return gasc;
        }

        /// <summary>
        /// 减温水焓增计算公式
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="press"></param>
        /// <returns></returns>

        private double Whz(double temp, double press)
        {

            // return 130.06 + 0.947711 * t ^ 1.2521 + p * (0.7234 - 9.2384 * (10 ^ -10) * t ^ 3.6606);
            // var whz = 130.06 + 0.947711 * Math.Pow(temp, 1.2521) + press * (0.7234 - 9.2384 * (10e-10) * Math.Pow(temp, 3.6606));
            var whz = 130.06 + 0.947711 * Math.Pow(temp, 1.2521) + press * (0.7234 - 9.2384 * Math.Pow(10, -10) * Math.Pow(temp, 3.6606));
            return whz;
        }


        /// <summary>
        /// 计算水冷壁一层非周期性吹灰逻辑
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<Chpoint> Chlist(List<Chpoint> list)
        {
            var q = from u in list.ToArray() select u;
            int num = list.Count;
            double dif_sum = 0d;
            double dx_sum = 0d;
            double dx = 0d;
            List<Chpoint> rtlist = new List<Chpoint>();
            for (int i = 0; i < num; i++)
            {
                dif_sum += list[i].Now_temp_dif_Val;
            }
            double avg = Math.Round(dif_sum * 1.0 / num, 4);

            for (int j = 0; j < num; j++)
            {
                double minus = list[j].Now_temp_dif_Val - avg;
                dx_sum += Math.Pow(minus, 2);
            }
            dx = dx_sum / num;

            if (dx > 60)
            {
                rtlist = q.OrderByDescending(x => x.Last_now_dif).Take(4).ToList();

            }
            else if (dx > 40 && dx <= 60)
            {
                rtlist = q.OrderByDescending(x => x.Last_now_dif).Take(6).ToList();
            }
            else if (dx >= 20 && dx <= 40)
            {
                rtlist = q.OrderByDescending(x => x.Last_now_dif).Take(8).ToList();
            }
            else
            {
                rtlist = list;
            }

            return rtlist;

        }
        #endregion

        #region 内部实体类
        class point
        {
            public string name { get; set; }
            public double pvalue { get; set; }
            public double typeid { get; set; }
            public string typename { get; set; }
        }
        class charea
        {
            public int id { get; set; }
            public string name { get; set; }
            public int bid { get; set; }
            public double wrl { get; set; }
            public double wrl_limit { get; set; }
        }
        class Chpoint
        {
            /// <summary>
            /// 吹灰器序号
            /// </summary>
            public System.Int32 Id { get; set; }
            /// <summary>
            /// 吹灰器描述
            /// </summary>
            public System.String Name_kw { get; set; }
            /// <summary>
            /// 锅炉ID
            /// </summary>
            public System.Int32 DncBoilerId { get; set; }
            /// <summary>
            /// 锅炉名称
            /// </summary>
            public System.String DncBoiler_Name { get; set; }
            /// <summary>
            /// 水冷壁的层级
            /// </summary>
            public System.Int32 Slb_floor_Val { get; set; }
            /// <summary>
            /// 上次吹灰列表清空时差值
            /// </summary>
            public System.Double Last_temp_dif_Val { get; set; }
            /// <summary>
            /// 当前温度差值
            /// </summary>
            public System.Double Now_temp_dif_Val { get; set; }
            /// <summary>
            /// 差值降低量
            /// </summary>
            public System.Double Last_now_dif { get; set; }

            /// <summary>
            /// 当前鳍片温度
            /// </summary>
            public System.Double now_temp_qp_Val { get; set; }
            /// <summary>
            /// 当前背火侧温度
            /// </summary>
            public System.Double now_temp_bh_Val { get; set; }
        }
        #endregion

        #region 其他
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            c = 0;
        }

        protected override void OnStop()
        {
            c = 0;
        }
        #endregion

    }
}
