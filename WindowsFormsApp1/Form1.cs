using ExcelDataReader;
using SynZnrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using wind;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        private double[] risklife(int bid, int h2s)
        {
            string sql = "select clxs,wdxs,circle,cir_time,density,pbr from dncgwfs_parameter where DncBoilerId=" + bid + " limit 1;";
            double[] rd = new double[2];
            SynZnrs.DBHelper db = new SynZnrs.DBHelper();
            DataTable dt = db.GetCommand(sql);
            double clxs = double.Parse(dt.Rows[0][0].ToString());
            double wdxs = double.Parse(dt.Rows[0][1].ToString());
            double circle = double.Parse(dt.Rows[0][2].ToString());
            double cir_time = double.Parse(dt.Rows[0][3].ToString());
            double density = double.Parse(dt.Rows[0][4].ToString());
            double pbr = double.Parse(dt.Rows[0][5].ToString());
            double h2snd = h2s * 1.0000 / 10000;
            double fssd = Math.Pow(clxs,0.5) * Math.Pow(h2snd,(0.5 * wdxs)) * Math.Pow(circle ,-0.5) * cir_time / density / (pbr - 1) * 10;//腐蚀深度
            double fshd = fssd * pbr;//腐蚀厚度
            rd[0] = fssd / (6.35 * 0.6) * 100;
            rd[1]= fshd / (6.35 * 0.7) * 100;
            return rd;


        }

        private double ltmj(double door_w, double door_h, int a0, int real_a)
        {
            double r_ltmj = door_w / Math.Cos(a0 * Math.PI / 180) * (door_h - door_h * Math.Cos((a0 + real_a * ((90 - a0)*1.00 / 100)) * Math.PI / 180));
            return r_ltmj;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            timer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string jz = comboBox1.SelectedItem.ToString();
            string sql = "select DncScrpointId from dncscrpointdata where DncBoilerId=" + jz + " order by realtime desc limit 1;";
            SynZnrs.DBHelper db = new SynZnrs.DBHelper();
            DataTable dt = db.GetCommand(sql);
            string lastindex = 18+"";
            if (dt.Rows.Count > 0)
            {
                lastindex = dt.Rows[0][0].ToString();
            }
            int newid = 0;
            if (lastindex.Equals("18"))
            {
                newid = 1;
            }
            else
            {
                newid = int.Parse(lastindex) + 1;
            }
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            double t1 = 2.5d;//2.5 - 4
            int t2 = 180;//180 - 250
            double t3 = 2.5d;//2.5 - 4
            int t4 = 20;//20 - 100
            double t5 = 0d;//2.5 - 4
            double t6 = 0d;//2.5 - 4
            Random rd = new Random();
            t1 = rd.Next(25, 40) / 10.0d;
            t2 = rd.Next(180, 250);
            t3 = rd.Next(25, 40) / 10.0d;
            t4 = rd.Next(20, 150);
            t5 = rd.Next(0, 30) / 10.0d;

            if (rd.Next(10)>8)
            {
                t5= rd.Next(30, 100) / 10.0d;
            }

            t6 = (t2 - t4) * 1.0d / t2 * 100 * 100 / 100.0d;


            string advive = "";
            if (newid % 4 == 0)
            {
                advive = "";
            }
            else if (newid % 4 == 1)
            {
                advive = "建议减小该区域喷嘴喷氨量";
            }
            else if (newid % 4 == 2)
            {
                advive = "";
            }
            else
            {
                advive = "建议增加该区域喷嘴喷氨量";
            }


            List<string> sqlarr = new List<string>();
            sql = "INSERT INTO dncscrpointdata(DncScrpointId,DncScrpoint_Name,RealTime,O2_in,Nox_in,O2_out,Nox_out,Nh3out,Scr_ratio,Catalysts_life,Advice,Remarks,Status,IsDeleted,DncBoilerId,DncBoiler_Name) VALUES ( '" + newid + "', '" + newid + "号格', '" + now + "', '" + t1 + "', '" + t2 + "', '" + t3 + "', '" + t4 + "', '" + t5 + "', '" + t6 + "', '"+newid+"', '"+advive+"', null, '1', '0', '" + jz + "', '" + jz + "号机组');";
            sqlarr.Add(sql);
            sql = "update dncscrpoint set NowStatus=0 where  DncBoilerId= " + jz+";";
            sqlarr.Add(sql);
            sql = "update dncscrpoint set RealTime='"+ now + "',O2_in=" + t1 + ",Nox_in=" + t2 + ",O2_out=" + t3 + ",Nox_out=" + t4 + ",Nh3out=" + t5 + ",Scr_ratio=" + t6 + ",NowStatus=1,Catalysts_life="+newid+ ",Advice='"+advive+ "',CheckTime=null,CheckPerson=null  where NameId_Val=" + newid + " and DncBoilerId= "+ jz+";";
            sqlarr.Add(sql);

            db.ExecuteTransaction(sqlarr);
        }

        

        private void Timer2_Tick(object sender, EventArgs e)
        {
            string jz = comboBox1.SelectedItem.ToString();           
            SynZnrs.DBHelper db = new SynZnrs.DBHelper();
            string sql_wind = "select Id,Wind_Name_kw,DncWindgroupId,DncGroup_Name,Doorwitdh,Doorheight,Angle0,Base_percent  from dncwind where DncBoilerId='" + jz+"' order by Id asc";
            string sql_group = "select Id,Group_Name_kw where DncBoilerId='" + jz + "'";
            DataTable dt = db.GetCommand(sql_wind);
            int sum_wind = dt.Rows.Count;
            StringBuilder sql_in_w_d = new StringBuilder();
            DateTime nowtime = DateTime.Now;
            Random rd = new Random();
            int ang = 0;
            double ltmj_val = 0d;
            double ltmj_sum = 0d;
            double bper = 0d;

           
            Dictionary<int, wind.Dncwind> dic_ltmj = new Dictionary<int, wind.Dncwind>();   //数据存入词典，方便后续再次计算百分比后更新 

            List<wind.Dncwind> wg = new List<Dncwind>();
            for (int i = 0; i < sum_wind; i++)
            {
                wind.Dncwind cwind = new wind.Dncwind();
                if (i<=4)
                {
                    ang = rd.Next(50, 100);
                    ltmj_val = ltmj(int.Parse(dt.Rows[i][4].ToString()), int.Parse(dt.Rows[i][5].ToString()), int.Parse(dt.Rows[i][6].ToString()), ang);
                    ltmj_sum += ltmj_val;
                    bper = double.Parse(dt.Rows[i][7].ToString());
                    cwind.Real_ltmj = ltmj_val;
                    cwind.DncWindgroupId = int.Parse(dt.Rows[i][2].ToString());
                    dic_ltmj.Add(int.Parse(dt.Rows[i][0].ToString()), cwind);
                   
                    sql_in_w_d.Append("insert into dncwinddata(DncWindId,DncWind_Name,RealTime,Real_angle,Real_ltmj,Status,IsDeleted,DncBoilerId,DncBoiler_Name,Base_percent) values('" + dt.Rows[i][0].ToString() + "','" + dt.Rows[i][1].ToString() + "','" + nowtime + "'," + ang + "," + ltmj_val + ",1,0,'" + jz + "','" + jz + "号机组',"+bper+");");
                }
                else
                {
                    ang = rd.Next(0, 100);
                    ltmj_val = ltmj(int.Parse(dt.Rows[i][4].ToString()), int.Parse(dt.Rows[i][5].ToString()), int.Parse(dt.Rows[i][6].ToString()), ang);
                    ltmj_sum += ltmj_val;
                    cwind.Real_ltmj = ltmj_val;
                    cwind.DncWindgroupId = int.Parse(dt.Rows[i][2].ToString());
                    dic_ltmj.Add(int.Parse(dt.Rows[i][0].ToString()), cwind);
                    bper = double.Parse(dt.Rows[i][7].ToString());
                    sql_in_w_d.Append("insert into dncwinddata(DncWindId,DncWind_Name,RealTime,Real_angle,Real_ltmj,Status,IsDeleted,DncBoilerId,DncBoiler_Name,Base_percent) values('" + dt.Rows[i][0].ToString() + "','" + dt.Rows[i][1].ToString() + "','" + nowtime + "'," + ang + "," + ltmj_val + ",1,0,'" + jz+"','" + jz + "号机组'," + bper + ");");
                }
            }                   

            if (!string.IsNullOrEmpty(sql_in_w_d.ToString()))
            {
                db.CommandExecuteNonQuery(sql_in_w_d.ToString());
            }

            int  sum_wind2= dic_ltmj.Count;
            int kid = 0;
           
            StringBuilder sql_up_w_d = new StringBuilder();
            StringBuilder sql_up_w = new StringBuilder();
            StringBuilder sql_up_wg = new StringBuilder();
            for (int i = 0; i < sum_wind2; i++)
            {
                wind.Dncwind kvalue = new wind.Dncwind();
                kid = dic_ltmj.ElementAt(i).Key;
               kvalue = dic_ltmj.ElementAt(i).Value;
               
               double per_wind = kvalue.Real_ltmj / ltmj_sum * 100;
                kvalue.Real_percent = per_wind;
                wg.Add(kvalue);
                sql_up_w_d.Append("update dncwind set Real_percent="+ per_wind + ",RealTime='"+nowtime+"' where Id=" + kid+";");
               sql_up_w.Append("update dncwinddata set Real_percent=" + per_wind + " where DncWindId=" + kid+ " and RealTime='"+nowtime+"';");
               
            }
            var q = from u in wg.ToArray() select u;

            var gb= q.GroupBy(x => x.DncWindgroupId);

            Dictionary<int, double> resultgb = new Dictionary<int, double>();
            foreach (var item in gb)
            {
                double aa = 0d;
                foreach (var a in item)
                {
                    aa += a.Real_percent;
                }
                resultgb.Add(item.Key, aa);
            }

            for (int i = 0; i < resultgb.Count; i++)
            {
                sql_up_wg.Append("update dncwindgroup set RealTime='" + nowtime + "',Real_percent="+ resultgb.ElementAt(i).Value+ " where Id=" + resultgb.ElementAt(i).Key+";");
            }

            if (!string.IsNullOrEmpty(sql_up_w_d.ToString()))
            {
                db.CommandExecuteNonQuery(sql_up_w_d.ToString());
            }
            if (!string.IsNullOrEmpty(sql_up_w.ToString()))
            {
                db.CommandExecuteNonQuery(sql_up_w.ToString());
            }

            if (!string.IsNullOrEmpty(sql_up_wg.ToString()))
            {
                db.CommandExecuteNonQuery(sql_up_wg.ToString());
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //string ltmj1 = ltmj(1118, 534, 15, 40).ToString();
            //MessageBox.Show(ltmj1);
            timer2.Start();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            timer2.Stop();
        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
            string jz = comboBox1.SelectedItem.ToString();
            string sql = "select NameId_Val from dncgwfspointdata where DncBoilerId=" + jz + " order by realtime desc limit 1;";
            SynZnrs.DBHelper db = new SynZnrs.DBHelper();
            DataTable dt = db.GetCommand(sql);
            string lastindex = 12 + "";
            if (dt.Rows.Count > 0)
            {
                lastindex = dt.Rows[0][0].ToString();
            }
            int newid = 0;
            if (lastindex.Equals("12"))
            {
                newid = 1;
            }
            else
            {
                newid = int.Parse(lastindex) + 1;
            }
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Random rd = new Random();
            int h2s = rd.Next(0, 300);
            if (rd.Next(10) > 8)
            {
                h2s = rd.Next(300, 1000);
            }
            int adnum = rd.Next(1, 11);
            string advice = "";
            switch (adnum)
            {
                case 1:
                    advice = "OFAI风门开至40%。";
                    break;
                case 2:
                    advice = "以10%的幅度开大CD层风门。";
                    break;
                case 3:
                    advice = "超开大4号角偏置，以5%幅度操作。";
                    break;
                case 4:
                    advice = "超开大1号角偏置，以5%幅度操作。";
                    break;
                case 5:
                    advice = "超开大2号角偏置，以5%幅度操作。";
                    break;
                case 6:
                    advice = "以10%的幅度开大AB层风门。";
                    break;
                case 7:
                    advice = "以5%的幅度开大EF和FF层风门。";
                    break;
                case 8:
                    advice = "①整体分配比例差于试验基准工况，则调整到基准工况；②燃尽降低相对值的20 %，偏置 + 直吹提升相对值的20 %，以10 % 幅度调整；③总氧量提升绝对值0.25 %（单次），总提升不超过0.5 %。";
                    break;
                case 9:
                    advice = "以5%的幅度开大CD/EF/FF层风门。";
                    break;
                case 10:
                    advice = "左墙（1.2.3中任两点）超开大EF和FF风门，同时以10%幅度操作。";
                    break;
                case 11:
                    advice = "开大BC和DE以10 % 的幅度其余投运磨偏置风。";
                    break;
                    
            }


            double[] rld = risklife(int.Parse(jz), h2s);

            List<string> sqlarr = new List<string>();
            sql = "INSERT INTO dncgwfspointdata(NameId_Val,RealTime,H2s,Advice,Status,IsDeleted,DncBoilerId,DncBoiler_Name,Fsrisk,Slblife) VALUES ( '" + newid + "', '" + now + "', " + h2s + ", '" + advice + "', '1', '0', '" + jz + "', '" + jz + "号机组',"+ rld[0] + ","+ rld[1] + ");";
            sqlarr.Add(sql);
            sql = "update dncgwfspoint set NowStatus=0 where  DncBoilerId= " + jz + ";";
            sqlarr.Add(sql);
            if (h2s > 300)
            {
                sql = "update dncgwfspoint set RealTime='" + now + "',H2s=" + h2s + ",NowStatus=1,Advice='" + advice + "',Fstime= Fstime+15,Fsrisk=Fsrisk + " + rld[0]+ ",Slblife=Slblife-"+ rld[1]+ " where NameId_Val=" + newid + " and DncBoilerId= " + jz + ";";
            }
            else
            {
                sql = "update dncgwfspoint set RealTime='" + now + "',H2s=" + h2s + ",NowStatus=1,Advice='" + advice + "',Fsrisk=Fsrisk + " + rld[0] + ",Slblife=Slblife-" + rld[1] + "  where NameId_Val=" + newid + " and DncBoilerId= " + jz + ";";
            }
            
            sqlarr.Add(sql);

            db.ExecuteTransaction(sqlarr);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            timer3.Start();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            timer3.Stop();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            SynZnrs.DBHelper db = new SynZnrs.DBHelper();
            List<string> sqlarr = new List<string>();
            string sql_exe = "select * from dncchrunlist where OffTime is null or OffTime=''";
            int runnum = db.GetCommand(sql_exe).Rows.Count;


            string sql_last_ch = "select OffTime from dncchrunlist ORDER BY OffTime desc LIMIT 1";
            DataTable dt_last_ch = db.GetCommand(sql_last_ch);
            DateTime d1 = DateTime.Parse("2020-1-1 00:00:00");
            if (dt_last_ch.Rows.Count > 0 && dt_last_ch.Rows[0][0].ToString() != "" && dt_last_ch.Rows[0][0].ToString() != null)
            {

                d1 = DateTime.Parse(dt_last_ch.Rows[0][0].ToString());
            }

            DateTime d2 = DateTime.Now;
            TimeSpan d3 = d2.Subtract(d1);

            if (runnum == 0 && int.Parse(d3.Minutes.ToString()) > 5)
            {
              
            string bid = comboBox1.SelectedItem.ToString();
            string sql = "select id,Wrlhigh_Val,K_Name_kw,DncBoiler_Name from dnccharea where id>1 and DncBoilerId=" + bid;
            DataTable dt = db.GetCommand(sql);
            Random rd = new Random();


            Dictionary<string, double> dnccharea_wrl = new Dictionary<string, double>();
          
            //string sql = "update dnccharea set Wrl_Val= ROUND((Wrlhigh_Val + ((RAND()*600)/1000 - 0.3)),2),RealTime=now()  where id>1 and DncBoilerId=" + bid + ";";
            foreach (DataRow item in dt.Rows)
            {
                double op = rd.Next(0, 600)/1000d-0.4;
                double d= Math.Round(double.Parse(item[1].ToString()) + op, 2); 
                sql = "update dnccharea set Wrl_Val= "+d+",RealTime=now()  where id="+item[0].ToString()+" and DncBoilerId=" + bid + ";";
                sqlarr.Add(sql);

                sql = "insert into dnccharea_his (Status,IsDeleted,DncBoilerId,DncBoiler_Name,DncChareaId,DncCharea_Name,Wrl_Val,Wrlhigh_Val,RealTime) value (1,0,"+bid+",'"+ item[3].ToString() + "',"+ item[0].ToString() + ",'"+ item[2].ToString() + "',"+d+","+ double.Parse(item[1].ToString()) + ",now())";
                sqlarr.Add(sql);

                sql = "update dncchpoint_wrl set Wrl_Val= " + d + ",RealTime=now() where DncChareaId=" + item[0].ToString() + " and DncBoilerId=" + bid + ";";
                sqlarr.Add(sql);

                dnccharea_wrl.Add(item[2].ToString(), d);
            }
                
                string pvalue = "[";
                for (int i = 0; i < 16; i++)
                {
                    double rsqqp = (rd.Next(0, 700) + 3500) * 1.0 / 10;
                    pvalue = pvalue+rsqqp.ToString()+",";
                }
                pvalue = pvalue.Substring(0, pvalue.Length - 1) + "]";

                sqlarr.Add("insert into dncchhzpoint (DncTypeId,DncType_Name,RealTime,Pvalue,Status,IsDeleted,DncBoilerId,DncBoiler_Name) values (85,'燃烧区域鳍片温度',now(),'"+pvalue+"',1,0,"+bid+",'"+bid+"号机组')");

            List<wendu> temp = new List<wendu>();
            List<double> six = new List<double>();
            for (int i = 1; i <= 96; i++)
            {
                double qp = (rd.Next(0, 700) + 3500)*1.0/10;
                if (i % 6 == 1)
                {
                    six.Clear();
                }
                six.Add(qp);
                if (i % 6 == 0)
                {
                    double bh=six.Min()- (rd.Next(0,15)+15);
                    for (int j = 0; j < 6; j++)
                    {
                        temp.Add(new wendu() {
                            id = i - (5 - j),
                            qp = six[j],
                            bhc= bh,
                            dif= six[j]-bh
                        });
                    }
                }
            }

            foreach (var item in temp)
            {
                sql = "update dncchqpoint set  now_temp_qp_Val=" + item.qp + ",now_temp_bh_Val=" + item.bhc + ",now_temp_dif_Val=" + item.dif+"   where DncBoilerId=" + bid+ " and Name_kw='IR"+item.id+"'";
                sqlarr.Add(sql);
            }


                db.ExecuteTransaction(sqlarr);
                sqlarr.Clear();

                //1 待吹灰
                #region 1

                double mg_wrl = (dnccharea_wrl["高温过热器（左侧）"] + dnccharea_wrl["高温过热器（右侧）"]) / 2;
                double gz_wrl = (dnccharea_wrl["高温再热器（左侧）"] + dnccharea_wrl["高温再热器（右侧）"]) / 2;

                if (mg_wrl < 0.825 && gz_wrl < 0.9)
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
                        obj.DncBoilerId = int.Parse(bid);
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
                        string bname = chlist[i].DncBoiler_Name;
                        sql = "insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted,Wrl_Val,Wrlhigh_Val) values('" + chq_name + "',now()," + id + ",'" + chq_name + "',1," + bid + ",'" + bname + "',1,0,0,0);";
                        sqlarr.Add(sql);

                    }

                }
                #endregion
                //2,3 待吹灰
                string sql_chlist_else = "select DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name from dncchpoint_wrl where Status=1 and IsDeleted=0 and (DncChtypeId=2 or DncChtypeId=3) and Wrl_Val>Wrlhigh_Val and DncBoilerId=" + bid;
                DataTable dt_chlist_else = db.GetCommand(sql_chlist_else);
                if (dt_chlist_else != null && dt_chlist_else.Rows.Count > 0)
                {
                    foreach (DataRow item in dt_chlist_else.Rows)
                    {
                        sql = "insert into dncchlist (K_Name_kw,AddTime,DncChqpointId,DncChqpoint_Name,AddReason,DncBoilerId,DncBoiler_Name,Status,IsDeleted,Wrl_Val,Wrlhigh_Val) values('" + item[1].ToString() + "',now()," + int.Parse(item[0].ToString()) + ",'" + item[1].ToString() + "',1," + int.Parse(item[2].ToString()) + ",'" + item[3].ToString() + "',1,0,0,0);";
                        sqlarr.Add(sql);
                    }
                }

                //4 吹灰执行
                string sql_ky_exe = "select * from dncchrunlist_kyq where OffTime is null or OffTime=''";
                int runnum_kyq = db.GetCommand(sql_ky_exe).Rows.Count; 
                if(runnum_kyq==0)
                {
                    string sql_chlist_kyq = "select DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name from dncchpoint_wrl where Status=1 and IsDeleted=0 and DncChtypeId=4 and Wrl_Val<Wrlhigh_Val and DncBoilerId=" + bid;
                    DataTable dt_chlist_kyq = db.GetCommand(sql_chlist_kyq);
                    if (dt_chlist_kyq != null && dt_chlist_kyq.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt_chlist_kyq.Rows)
                        {
                            sql = "insert into dncchrunlist_kyq (Name_kw,AddTime,Remarks,Status,IsDeleted,DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name) values ('" + item[1].ToString() + "',now(),'烟气侧效率低',1,0," + int.Parse(item[0].ToString()) + ",'" + item[1].ToString() + "'," + bid + ",'" + item[3].ToString() + "');";
                            sqlarr.Add(sql);
                        }

                    }

                    db.ExecuteTransaction(sqlarr);
                }
               
            }
            


            
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer4.Start();
            timerCHRun.Start();
            timerState.Start();
            timer5.Start();
            timer6.Start();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            timer4.Stop();
            timerCHRun.Stop();
            timerState.Stop();
            timer5.Stop();
            timer6.Stop();
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




        /// <summary>
        /// 判断是否加入执行列表
        /// </summary>
        /// <param name="boilerid"></param>
        /// <param name="csyntime"></param>

        private void Znchrun(int boilerid, DateTime csyntime)
        {
            DBHelper db = new DBHelper();          
            int chtime_sum = 0;
            int reason = 0;
            string sql_chlist_sum = "select count(*),Ch_time_Val from v_chlist where DncBoilerId=" + boilerid + " GROUP BY DncChtypeId,Ch_time_Val";
            DataTable dt_chtime = db.GetCommand(sql_chlist_sum);
            if(dt_chtime!=null&&dt_chtime.Rows.Count>0)
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
                
                string sql_wrl_cx = "select * from dnccharea where DncBoilerId=" + boilerid + " and (DncChtypeId=2 or DncChtypeId=3) and K_Name_kw<>'分级省煤器' and ((Wrl_Val-Wrlhigh_Val)>=0.25 or ((K_Name_kw='高温过热器（左侧）' or K_Name_kw='高温过热器（右侧）') and Wrl_Val>0 and Wrl_Val<0.7 ) or ((K_Name_kw='高温再热器（左侧）' or K_Name_kw='高温再热器（右侧）') and Wrl_Val>0 and Wrl_Val<0.75 ))";
                DataTable dt_wrl_cx = db.GetCommand(sql_wrl_cx);
                string sql_norun = "select * from dncchrunlist  where DATE_FORMAT(AddTime,'%Y-%m-%d')= DATE_FORMAT('2020-7-7 20:20:00','%Y-%m-%d') ";
                DataTable dt_norun = db.GetCommand(sql_norun);
                string rname = "";
                if (chtime_sum > 7200)
                {
                    reason = 1;
                }
                else if (dt_wrl_cx != null && dt_wrl_cx.Rows.Count > 0)
                {
                    reason = 2;
                }
                else if (dt_norun != null && dt_norun.Rows.Count > 0 && csyntime > DateTime.Parse(DateTime.Now.ToShortDateString() + " 20:00:00"))
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
                    string sql_chlist = "select  DISTINCT DncChqpointId,DncChqpoint_Name,DncBoiler_Name from dncchlist  where Status=1 and IsDeleted=0 and DncBoilerId=" + boilerid;
                    DataTable dt_chlist = db.GetCommand(sql_chlist);
                    if (dt_chlist != null && dt_chlist.Rows.Count > 0)
                    {

                        StringBuilder sql_runlist = new StringBuilder();
                        int chqid = 0;
                        string chqname = "";
                        string bname = "";
                       

                        foreach (DataRow item in dt_chlist.Rows)
                        {
                            chqid = int.Parse(item[0].ToString());
                            chqname = item[1].ToString();
                            bname = item[2].ToString();
                            sql_runlist.Append("insert into dncchrunlist (Name_kw,AddTime,Remarks,Status,IsDeleted,DncChqpointId,DncChqpoint_Name,DncBoilerId,DncBoiler_Name) values ('" + chqname + "','" + csyntime + "','" + rname + "',1,0," + chqid + ",'" + chqname + "'," + boilerid + ",'" + bname + "');");

                        }
                        sql_runlist.Append("update dncchlist set Status=0 where DncBoilerId=" + boilerid + ";");
                        if (!string.IsNullOrEmpty(sql_runlist.ToString()))
                        {
                            db.CommandExecuteNonQuery(sql_runlist.ToString());
                        }
                    }

                }
            }
         

        }

        private void timerCHRun_Tick(object sender, EventArgs e)
        {
            Znchrun(int.Parse(comboBox1.SelectedItem.ToString()), DateTime.Now);
        }

        private void timerState_Tick(object sender, EventArgs e)
        {
            string bid = comboBox1.SelectedItem.ToString();
            DBHelper db = new DBHelper();

            

            List<string> arr = new List<string>();
            string sql = "update dncchrunlist set OffTime=now(),Status=0 where DncBoilerId=" + bid+ " and Status=1 and IsDeleted=0 and RunTime is not null";
            arr.Add(sql);

            sql = "update dncchrunlist set RunTime=now() where DncBoilerId=" + bid + " and Status=1 and IsDeleted=0 ORDER BY DncChqpointId ASC,Id ASC LIMIT 2";
            arr.Add(sql);

            sql = "update dncchqpoint set DncChstatusId=1,DncChstatus_Name='状态正常 未工作' where DncBoilerId=" + bid + " and DncChtypeId<>4 ";
            arr.Add(sql);

            sql = "update dncchqpoint set DncChstatusId=2,DncChstatus_Name='正在吹灰' where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist where DncBoilerId=" + bid + " and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is null)";
            arr.Add(sql);

            db.ExecuteTransaction(arr);
            arr.Clear();


            string s = "select *  from dncchrunlist where DncBoilerId=" + bid + " and Status = 1 ";
            int i = db.GetCommand(s).Rows.Count;
            string del = "";
           
            if (i == 0)
            {
                del = "update dncchrunlist set IsDeleted=1 where DncBoilerId=" + bid + "";
              
            }
           
            if (!string.IsNullOrEmpty(del))
            {
                db.CommandExecuteNonQuery(del);
            }
        }

        private void Timer5_Tick(object sender, EventArgs e)
        {
            string bid = comboBox1.SelectedItem.ToString();
            DBHelper db = new DBHelper();



            List<string> arr = new List<string>();
            string sql = "update dncchrunlist_kyq set OffTime=now(),Status=0 where DncBoilerId=" + bid + " and Status=1 and IsDeleted=0 and RunTime is not null";
            arr.Add(sql);

            sql = "update dncchrunlist_kyq set RunTime=now() where DncBoilerId=" + bid + " and Status=1 and IsDeleted=0 ORDER BY AddTime ASC LIMIT 2";
            arr.Add(sql);

            sql = "update dncchqpoint set DncChstatusId=1,DncChstatus_Name='状态正常 未工作' where DncBoilerId=" + bid + " and DncChtypeId=4 ";
            arr.Add(sql);

            sql = "update dncchqpoint set DncChstatusId=2,DncChstatus_Name='正在吹灰' where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist_kyq where DncBoilerId=" + bid + " and DncChtypeId=4   and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is null)";
            arr.Add(sql);

            db.ExecuteTransaction(arr);
            arr.Clear();


            string s = "select *  from dncchrunlist_kyq where DncBoilerId=" + bid + " and Status = 1 ";
            int i = db.GetCommand(s).Rows.Count;
            string del = "";

            if (i == 0)
            {
                del = "update dncchrunlist_kyq set IsDeleted=1 where DncBoilerId=" + bid + "";
               
            }
          
            if (!string.IsNullOrEmpty(del))
            {
                db.CommandExecuteNonQuery(del);
            }
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            string bid = comboBox1.SelectedItem.ToString();
            DBHelper db = new DBHelper();



            List<string> arr = new List<string>();
            string sql = "update dncchqpoint set updown=0 where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist_kyq where DncBoilerId=" + bid + "    and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is null and date_sub(now(),interval 30 second)>RunTime )";
            arr.Add(sql);
            sql = "update dncchqpoint set updown=-1 where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist_kyq where DncBoilerId=" + bid + "    and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is not null )";
            arr.Add(sql);

            sql = "update dncchqpoint set updown=0 where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist where DncBoilerId=" + bid + "   and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is null and date_sub(now(),interval 30 second)>RunTime )";
            arr.Add(sql);
            sql = "update dncchqpoint set updown=-1 where DncBoilerId=" + bid + " and id in (select DncChqpointId from dncchrunlist where DncBoilerId=" + bid + "   and Status=1 and IsDeleted=0 and RunTime is not null and OffTime is not null )";
            arr.Add(sql);
            db.ExecuteTransaction(arr);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            JObject jobj = JObject.Parse("{a:[1,2,3],b:{qwe:123,ert:456}}");
            JToken result = jobj as JToken;
            string s = result["b"].ToString();
            MessageBox.Show(s);
            //string csvpath = System.AppDomain.CurrentDomain.BaseDirectory;
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //using (var stream = System.IO.File.Open(csvpath + "DCS.xlsx", FileMode.Open, FileAccess.Read))
            //{
            //    using (var reader = ExcelReaderFactory.CreateReader(stream))
            //    {
            //        StreamWriter sw = new StreamWriter(csvpath + "sql.txt");

            //        while (reader.Read())
            //        {
            //            if (reader.GetString(0)=="")
            //            {
            //                break;
            //            }
            //            string sql = "alter table dncchpointadddata add COLUMN "+ reader.GetString(0) + " double DEFAULT 0 COMMENT '"+ reader.GetString(1) + "';";
            //            sw.WriteLine(sql);
            //        }
            //        sw.Flush();
            //        sw.Close();
            //    }
            //}
            MessageBox.Show("ok");
        }

        public string UnicodeToString(string value)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                  value, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }
    }

    class wendu
    {
        public int id { get; set; }
        public double qp { get; set; }
        public double bhc { get; set; }
        public double dif { get; set; }
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
    }

}
