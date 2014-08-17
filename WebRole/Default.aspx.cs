using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Management;
using System.Threading.Tasks;

namespace WebRole
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        CloudStorageAccount StorageAccount { get; set; } // аккаунт хранилища
        CloudQueueClient Client { get; set; }   // клиент очереди
        CloudQueue Queue { get; set; } // вроде как сама очередь
        CloudQueueMessage Message { get; set; } // сообщение, получаемое из очереди
        CloudQueueMessage Message2 { get; set; } // сообщение, получаемое из очереди
        DateTimeOffset Epoch;

        protected void Page_Load(object sender, EventArgs e)
        {
            // получаем строку подключения к аккаунту хранилища
            StorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            Client = StorageAccount.CreateCloudQueueClient();
            CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
            CloudTable TableData = tableClient.GetTableReference("TableData");
            TableData.CreateIfNotExists();
            CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");
            TableSimulation.CreateIfNotExists();
            CloudTable TableExperiment = tableClient.GetTableReference("TableExperiment");
            TableExperiment.CreateIfNotExists();

            //  bt_droup_data_Click(sender, e);

            Queue = Client.GetQueueReference("queuecrack");
            Queue.CreateIfNotExists();
            //   Queue.Clear();


            Chart1.Visible = false;
            Chart2.Visible = false;
            crt_distribution.Visible = false;
            crt_histogram.Visible = false;
            crt_distrib_func.Visible = false;
        }

        public class EntityOfTableData : TableEntity
        {
            public EntityOfTableData(string id_simulation, string id_line)
            {
                this.PartitionKey = id_simulation;
                this.RowKey = id_line;
            }
            public EntityOfTableData() { }
            public string par1 { get; set; }
            public string par2 { get; set; }
            public string par3 { get; set; }
            public string par4 { get; set; }
            public string par5 { get; set; }
            public string par6 { get; set; }
            public string par7 { get; set; }
            public string par8 { get; set; }
            public string par9 { get; set; }
            public string par10 { get; set; }
            public string par11 { get; set; }
            public string par12 { get; set; }
            public string par13 { get; set; }
            public string par14 { get; set; }
        }

        public class EntityOfTableSimulation : TableEntity
        {
            public static string KeyLength = "000000000000000000000";
            public EntityOfTableSimulation(int id_experiment, int id_simulation)
            {
                this.PartitionKey = id_experiment.ToString(KeyLength);
                this.RowKey = id_simulation.ToString(KeyLength);
            }
            public EntityOfTableSimulation() { }
            public string time_simulation { get; set; }
            public string x { get; set; }
            public string y { get; set; }
        }

        public class EntityOfTableExperiment : TableEntity
        {
            public static string KeyLength = "000000000000000000000";
            public EntityOfTableExperiment(string zero, int id_experiment)
            {
                this.PartitionKey = zero;
                this.RowKey = id_experiment.ToString(KeyLength);
            }
            public EntityOfTableExperiment() { }
            public string time_start_simulation { get; set; }
            public string simulation_count { get; set; }
        }

        private static void InsertToTableExperiment(CloudTable table, string zero, int id_experiment, string time_start_simulation, int simulation_count)
        {
            try
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                EntityOfTableExperiment entity = new EntityOfTableExperiment(zero, id_experiment);
                entity.time_start_simulation = time_start_simulation;
                entity.simulation_count = simulation_count.ToString();
                batchOperation.Insert(entity);
                table.ExecuteBatch(batchOperation);
            }
            catch
            {
                throw;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("My Diploma");
            double l0 = Convert.ToDouble(tb_l0.Text);
            double delta = Convert.ToDouble(tb_delta.Text);
            double a0 = Convert.ToDouble(tb_a0.Text);
            double sigma = Convert.ToDouble(tb_sigma.Text);
            double KI_c_min = Convert.ToDouble(tb_KI_c_min.Text);
            double KI_c_max = Convert.ToDouble(tb_KI_c_max.Text);
            double alpha = Convert.ToDouble(tb_alpha.Text);
            double betta = Convert.ToDouble(tb_betta.Text);
            double mm = Convert.ToDouble(tb_mm.Text);
            double alpha_alpha = Convert.ToDouble(tb_alpha_alpha.Text);
            double betta_betta = Convert.ToDouble(tb_betta_betta.Text);

            //Multi-Dimensional Array
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            double[,] arrMultiD = Resh_env_cycle(l0 * Math.Pow(80, 2) / (Math.Pow(140, 2) * Math.PI), delta, a0, sigma, KI_c_min, KI_c_max, alpha, betta, mm, alpha_alpha, betta_betta);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            long ts = stopWatch.ElapsedMilliseconds;

            // Format and display the TimeSpan value.
            //  float elapsedTime = ts.Milliseconds ;
            //  Label1.Text = "Время рассчета: " + ts.ToString() + " миллисекунд.";

            DataTable dt = new DataTable();
            dt.Columns.Add("№", Type.GetType("System.String"));
            for (int i = 1; i <= 14; i++)
                dt.Columns.Add(i.ToString(), Type.GetType("System.String"));

            /*
            dt.Columns.Add("2", Type.GetType("System.String"));
            dt.Columns.Add("3", Type.GetType("System.String"));
            dt.Columns.Add("4", Type.GetType("System.String"));
            dt.Columns.Add("5", Type.GetType("System.String"));
            dt.Columns.Add("6", Type.GetType("System.String"));
            dt.Columns.Add("7", Type.GetType("System.String"));
            dt.Columns.Add("8", Type.GetType("System.String"));
            dt.Columns.Add("9", Type.GetType("System.String"));
            dt.Columns.Add("10", Type.GetType("System.String"));
            dt.Columns.Add("11", Type.GetType("System.String"));
            dt.Columns.Add("12", Type.GetType("System.String"));
            dt.Columns.Add("13", Type.GetType("System.String"));
            dt.Columns.Add("14", Type.GetType("System.String"));*/
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("1", Type.GetType("System.String"));
            dt2.Columns.Add("7", Type.GetType("System.String"));


            //   Chart1.Series["Testing"].XValueType = System.Web.UI.DataVisualization.Charting.ChartValueType.Int32;

            Chart1.Series.Add("Testing");
            Chart1.AlignDataPointsByAxisLabel();
            Chart1.Series["Testing"].ChartTypeName = "line";

            for (int i = 1; i < arrMultiD.GetLength(0); i++)
            {
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["№"] = i;
                for (int j = 1; j <= 14; j++)
                    dt.Rows[dt.Rows.Count - 1][j.ToString()] = arrMultiD[i, j];
                /*
                                dt2.Rows.Add();
                                dt2.Rows[dt.Rows.Count - 1]["1"] = arrMultiD[i, 1];
                                dt2.Rows[dt.Rows.Count - 1]["7"] = arrMultiD[i, 7];
                                Chart1.Series["Testing"].Points.AddXY(Convert.ToInt64(arrMultiD[i, 7]), arrMultiD[i, 1]);*/
                //////////////

            }
            // Chart1.Series["Testing"].AxisLabel = "papapa";
            //  Chart1.ChartAreas[0].AxisX.Interval = 5000;
            //  Chart1.ChartAreas[0].AxisX.Minimum = 0;
            //Chart1.ChartAreas[0].AxisX.Maximum = 1000;
            //  Chart1.ChartAreas[0].AxisY.Minimum = 0;
            //  Chart1.ChartAreas[0].AxisY.Maximum = 0.08;



            //        Chart1.Series["Testing"].Points.DataBind(testData, "Key", "Value", string.Empty);


            //     Chart1.Series.Add(arrMultiD[i, 1].ToString());
            //     Chart1.Series[arrMultiD[i, 1].ToString()].XValueMember = "ranking_date";
            //     Chart1.Series.Add(arrMultiD[i, 7].ToString());
            //      Chart1.Series[arrMultiD[i, 7].ToString()].YValueMembers = "ranking_position";


            /*
            dt.Rows[dt.Rows.Count - 1]["2"] = arrMultiD[i, 2];
            dt.Rows[dt.Rows.Count - 1]["3"] = arrMultiD[i, 3];
            dt.Rows[dt.Rows.Count - 1]["4"] = arrMultiD[i, 4];
            dt.Rows[dt.Rows.Count - 1]["5"] = arrMultiD[i, 5];
            dt.Rows[dt.Rows.Count - 1]["6"] = arrMultiD[i, 6];
            dt.Rows[dt.Rows.Count - 1]["7"] = arrMultiD[i, 7];
            dt.Rows[dt.Rows.Count - 1]["8"] = arrMultiD[i, 8];
            dt.Rows[dt.Rows.Count - 1]["9"] = arrMultiD[i, 9];
            dt.Rows[dt.Rows.Count - 1]["10"] = arrMultiD[i, 10];
            dt.Rows[dt.Rows.Count - 1]["11"] = arrMultiD[i, 11];
            dt.Rows[dt.Rows.Count - 1]["12"] = arrMultiD[i, 12];
            dt.Rows[dt.Rows.Count - 1]["13"] = arrMultiD[i, 13];
            dt.Rows[dt.Rows.Count - 1]["14"] = arrMultiD[i, 14];
             */

            //GridView1.DataSource = dt2;
            /// GridView1.DataBind();


            //   Chart1.Series.Add("Series1");
            //   Chart1.Series["Series1"].ChartArea = "Default";
            //     Chart1.Series["Series1"].ChartType = SeriesChartType.Line;

            // добавим данные линии
            //     string[] axisXData = new string[] { "a", "b", "c" };
            //     double[] axisYData = new double[] { 0.1, 1.5, 1.9 };
            //      Chart1.Series["Series1"].Points.DataBindXY(axisXData, axisYData);


            //  Label1.Text = (Math.Pow(c1_k(0, 0, 2, 3), 2) * Integral.integral(new Integral.Function(delta_t_func_nominator), 1, 5, 2,3)).ToString();
            //  Label2.Text = integral_e(1, 5, 2*2, 3*2).ToString();

            //   Chart1.DataSource = dt2;

            //      Chart1.Series["7"].YValueMembers = "2";
            /*
            Chart1.DataSource = dt2;
            Chart1.Series[0].XValueMember = "1";
            Chart1.Series[0].YValueMembers = "7";   */
            Chart1.DataBind();
            Chart1.Visible = true;

            //GridView1.DataSource = myTable;//Resh_env_cycle(lll * Math.Pow(80, 2) / (Math.Pow(140, 2) * Math.PI), 0.05, Math.Pow(10, -5), 140, 10, 80, 2, 2, 10, 2, 2);
            //GridView1.DataSource = arrList;

        }
        ///////////////////////////////////////////////////////////////////////////Begin

        class Integral
        {
            public delegate double Function(double x, double SS1, double SS2);
            public static double integral(Function f, double a, double b, double SS1, double SS2)
            {
                double step, sum;
                int number;
                number = 5000; step = (b - a) / number;
                sum = f(a, SS1, SS2) + f(b, SS1, SS2);
                double s1 = 0, s2 = 0, x = a;
                for (int i = 1; i < number; i++)
                {
                    x += step;
                    if (i % 2 == 1)
                        // сумма значений функции в точках с нечетными номерами
                        s1 += f(x, SS1, SS2);
                    else
                        // сумма значений функции в точках с четными номерами
                        s2 += f(x, SS1, SS2);
                }
                sum = step / 3 * (sum + 4 * s1 + 2 * s2);
                return sum;
            }
        }

        const double ff = 1.2;
        const double D = 0.0000000001;
        const double V_H = 1.96 * 0.000001;
        const double R = 8.314;
        const double T = 273 + 20;

        //4340 Steel ->
        const double C = 1.095 * 0.000000000001;
        const double n = 3.24;

        public static double[,] Resh_env_cycle(double l0, double delta, double a0, double sigma,
            double K_Ic_min, double K_Ic_max, double alpha, double betta, double mm,
            double alpha_alpha, double betta_betta)
        {

            double[] l = new double[2500];             //Поставил размер от фонаря
            l[0] = l[1] = l0;
            double k_a1 = 0.0025;
            double[] ka = new double[2500];
            ka[1] = k_a1;
            double k_b1 = 0.000001;
            double[] kb = new double[2500];
            kb[1] = k_b1;
            double[,] aaa = new double[5000, 15];           //Поставил размер от фонаря
            aaa[1, 7] = 0;
            double[] a = new double[2500];                 //Поставил размер от фонаря
            a[0] = a0;
            double[] b = new double[2500];                //Поставил размер от фонаря
            b[0] = 500 * a[0];
            double[] SS_1 = new double[2500];
            double[] SS_2 = new double[2500];
            double[] SSS_1 = new double[2500];
            double[] SSS_2 = new double[2500];
            double[] x = new double[2500];
            double[] y = new double[2500];
            int i = 1;
            int j = 0;
            int jjj = 1;
            int kk = 0;

            while (l[i] < (Math.Pow(K_Ic_max, 2) * (1 - delta)) / (Math.PI * Math.Pow(sigma, 2)))
            {
                a[i] = a_func(l[i], delta, a[0], l[0], mm, K_Ic_max, sigma, alpha_alpha, betta_betta);
                b[i] = 500 * a[i];
                x[0] = 0;
                y[0] = 0;
                for (int i_i = 1; i_i <= 5 * b[i] / a[i] - 1; i_i++)
                {
                    x[i_i] = x[i_i - 1] + a[i] / 5;
                    y[i_i] = Math.Log(f(x[i_i], a[i], b[i], ka[i], kb[i]));
                    //       Console.WriteLine("i = " + (i_i+1).ToString() + "; x[i] = " + x[i_i].ToString() + ";  y[i] = " + y[i_i].ToString()); 
                }

                //  float y = Math.Log(f(x[i], a[i], b[i], ka[i], kb[i]));
                SS_2[i] = slope(x, y);
                SS_1[i] = Math.Exp(intercept(x, y, SS_2[i]));
                //    Console.WriteLine("ai = " + a[i].ToString() + "; b[i] = " + b[i].ToString() + ";  SS_1[i] = " + SS_1[i].ToString() + ";  SS_2[i] = " + SS_2[i].ToString());
                double[] values = new double[2] { SS_1[i], SS_2[i] };


                //    Console.WriteLine(Integral.integral(new Integral.Function(c1), 0, Math.Pow(10,-5) , values));

                double m;
                //     values[2] = c1_k(0, 0, values);
                m = ((1 / 1.5) * Math.Pow(1 - Math.Pow((K_I_(sigma, l[i]) - K_Ic_min) / (K_Ic_max - K_Ic_min), alpha), 1 / betta)) / (integral_e(0, a[i], SS_1[i], SS_2[i]) / a[i]); //(c1_k(0, 0, values) * Integral.integral(new Integral.Function(c1), 0, a[i], values) / a[i]);
                //                Console.WriteLine("Intergral = " + integral(0,10).ToString());
                Console.WriteLine("m = " + m.ToString());
                //      values[2] = c1_k(0, 10, values);
                //    Console.WriteLine("c1_10_10 = " + c1(x[3], values));
                //   Console.WriteLine("Derivative c1_10_10 = " + Integral.derivative(new Integral.Function(c1), x[500-1],1, values));

                double B = Integral.integral(new Integral.Function(B_func), a[i], b[i], SS_1[i], SS_2[i]);
                Console.WriteLine("B_func = " + B);
                double crazy_formula = ((-D * V_H / (3 * R * T)) * Math.Pow(10, 6) * K_I_(sigma, l[i]));
                double delta_t_fraction = integral_e(a[i], b[i], SS_1[i] * SS_1[i], 2 * SS_2[i]) / (crazy_formula * B + D * integral_e(a[i], b[i], Math.Pow(SS_1[i] * SS_2[i], 2), 2 * SS_2[i]));
                //    , //Integral.integral(new Integral.Function(delta_t_func_denominator), a[i], b[i], SS_1[i], SS_2[i]));
                double delta_t = delta_t_fraction * Math.Log(m);
                //       double delta_t = delta_t_func_denominator(x[7], values); //Integral.integral(new Integral.Function(delta_t_func_denominator), a[i], b[i], values);
                Console.WriteLine("delta_t = " + delta_t.ToString());



                double speed_fatigue_fraction = a[i] / speed_fatigue(K_I_(sigma, l[i]));
                Console.WriteLine("speed_fatigue_fraction = " + speed_fatigue_fraction.ToString());
                if (delta_t >= speed_fatigue_fraction)
                {
                    Console.WriteLine("Uslovie 1");
                    aaa[i, 6] = speed_fatigue_fraction;
                    j++;
                    if (j == 1)
                    {
                        aaa[jjj, 4] = i;
                        jjj++;
                    }

                    m = Math.Exp(speed_fatigue_fraction / delta_t_fraction);
                    Console.WriteLine("m = " + m.ToString());
                    kk = 0;
                }
                else
                {
                    Console.WriteLine("Uslovie 2");
                    aaa[i, 6] = delta_t;
                    kk++;
                    if (kk == 1)
                    {
                        aaa[jjj, 4] = i;
                        jjj++;
                    }
                    j = 0;
                }


                aaa[i, 11] = delta_t;
                aaa[i, 1] = l[i];
                aaa[i, 2] = K_I_(sigma, l[i]);
                aaa[i, 3] = m;
                aaa[1, 5] = jjj - 1;
                if (i == 1)
                {
                    aaa[i + 1, 7] = aaa[i, 6];
                }
                if (i > 1)
                {
                    aaa[i + 1, 7] = aaa[i, 7] + aaa[i, 6];
                }
                aaa[i, 8] = integral_e(0, a[i], SS_1[i], SS_2[i]) / a[i];
                l[i + 1] = l[i] + a[i];
                Console.WriteLine("iii = " + i + " ai+1 " + a_func(l[i + 1], delta, a[0], l[0], mm, K_Ic_max, sigma, alpha_alpha, betta_betta));
                a[i + 1] = a_func(l[i + 1], delta, a[0], l[0], mm, K_Ic_max, sigma, alpha_alpha, betta_betta);
                b[i + 1] = 500 * a[i + 1];
                kb[i + 1] = m * c1_k(0, 0, SS_1[i], SS_2[i]) * c1(a[i] + 500 * a[i + 1], SS_1[i], SS_2[i]);
                ka[i + 1] = m * c1_k(0, 0, SS_1[i], SS_2[i]) * c1(a[i + 1] + a[i], SS_1[i], SS_2[i]);

                for (int i_i = 1; i_i <= 5 * b[i + 1] / a[i + 1] - 1; i_i++)
                {
                    x[i_i] = x[i_i - 1] + a[i + 1] / 5;
                    y[i_i] = Math.Log(f(x[i_i], a[i + 1], b[i + 1], ka[i + 1], kb[i + 1]));
                    //        Console.WriteLine("i2 = " + (i_i + 1).ToString() + "; x[i] = " + x[i_i].ToString() + ";  y[i] = " + y[i_i].ToString());
                }
                Console.WriteLine("a[i+1] = " + (a[i + 1]).ToString() + "; b[i+1] = " + b[i + 1].ToString() + ";  ka[i+1] = " + ka[i + 1].ToString() + ";  kb[i+1] = " + kb[i + 1].ToString());


                //  float y = Math.Log(f(x[i], a[i], b[i], ka[i], kb[i]));
                SSS_2[i] = slope(x, y);
                SSS_1[i] = Math.Exp(intercept(x, y, SS_2[i]));
                double[] values2 = new double[2] { SSS_1[i], SSS_2[i] };
                Console.WriteLine("ai = " + a[i].ToString() + "; b[i] = " + b[i].ToString() + ";  SSS_1[i] = " + SSS_1[i].ToString() + ";  SSS_2[i] = " + SSS_2[i].ToString());
                ////////////////////    Console.ReadKey();
                aaa[i, 9] = a[i];
                aaa[i, 10] = aaa[i, 6];
                aaa[i, 12] = speed_fatigue_fraction;
                aaa[i, 13] = SS_1[i];
                aaa[i, 14] = SS_2[i];
                Console.WriteLine("Proverka usl. break:" + (c1_k(0, 0, SSS_1[i], SSS_2[i]) * Integral.integral(new Integral.Function(c1), 0, a[i + 1], SSS_1[i], SSS_2[i]) / a[i + 1]) + " >= " + (1 / 1.5) * Math.Pow(1 - Math.Pow((K_I_(sigma, l[i]) - K_Ic_min) / (K_Ic_max - K_Ic_min), alpha), 1 / betta));
                //  Console.ReadKey();
                if ((delta_t <= 0) || ((integral_e(0, a[i + 1], SS_1[i + 1], SS_2[i + 1]) / a[i + 1]) >= (1 / 1.5) * Math.Pow(1 - Math.Pow((K_I_(sigma, l[i]) - K_Ic_min) / (K_Ic_max - K_Ic_min), alpha), 1 / betta)))
                {
                    break;
                }
                i++;
                //Console.ReadKey();
            }
            if (i == 1)
            {
                aaa[1, 14] = 0;
                aaa[1, 1] = l0;
                Console.WriteLine("Print matrix aaa");
            }
            else
            {
                Console.WriteLine("Print matrix aaa2");
                //   XDocument doc = new XDocument();
                //arrayList arrList; 
                for (int ii = 1; ii < 500/*i*/; ii++)
                {
                    for (int jj = 1; jj < 14; jj++)
                    {
                        Console.Write(aaa[ii, jj] + " ");

                        //      doc.Add(new XElement("root", aaa.Select(x => new XElement(jj.ToString(), x))));
                    }
                    Console.WriteLine();
                }


            }
            return aaa;

        }



        public static double slope(double[] X, double[] Y)
        {
            if (X.Length != Y.Length)
                return 0;
            else
            {
                double sumX = 0;
                foreach (double i in X)
                {
                    sumX += i;
                }
                double sumX2 = 0;
                foreach (double i in X)
                {
                    sumX2 += i * i;
                }
                double sumY = 0;
                foreach (double i in Y)
                {
                    sumY += i;
                }
                double sumXY = 0;
                for (int i = 0; i <= X.Length - 1; i++)
                {
                    sumXY += X[i] * Y[i];
                }
                return (sumXY - sumX * sumY / X.Length) / (sumX2 - sumX * sumX / X.Length);
            }
        }

        public static double intercept(double[] X, double[] Y, double slope)
        {
            if (X.Length != Y.Length)
                return 0;
            else
            {
                double sumXdel = 0;
                foreach (double i in X)
                {
                    sumXdel += i;
                }
                sumXdel = sumXdel / X.Length;
                double sumYdel = 0;
                foreach (double i in Y)
                {
                    sumYdel += i;
                }
                sumYdel = sumYdel / Y.Length;
                return sumYdel - slope * sumXdel;
            }
        }

        public static double speed_fatigue(double K_I)
        {
            return C * ff * Math.Pow(K_I, n);
        }

        public static double a_func(double l, double delta, double a0, double l0, double mm,
            double K_Ic_max, double sigma, double alpha_alpha, double betta_betta)
        {
            double fraction = (Math.Pow(K_Ic_max, 2) * (1 - delta)) / (Math.PI * Math.Pow(sigma, 2));
            return a0 * (1 + (mm - 1) * Math.Pow(1 - Math.Pow((fraction - l) / (fraction - l0), betta_betta), 1 / alpha_alpha));
        }

        public static double K_I_(double sigma, double l)
        {
            return sigma * Math.Sqrt(Math.PI * l);
        }

        public static double c1(double x, double SS1, double SS2)
        {
            return (Math.Exp(SS2 * x));
        }

        public static double c1_k(int degreeGrad, int degree10, double SS1, double SS2)
        {
            return (Math.Pow(SS2, degreeGrad) * Math.Pow(10, degree10) * SS1);
        }


        public static double delta_t_func_nominator(double x, double SS1, double SS2)
        {
            return Math.Pow(c1(x, SS1, SS2), 2);
        }

        public static double delta_t_func_denominator(double x, double SS1, double SS2)
        {
            return c1_k(0, 0, SS1, SS2) * c1_k(2, 0, SS1, SS2) * Math.Pow(c1(x, SS1, SS2), 2);
        }

        public static double B_func(double x, double SS1, double SS2)
        {

            return (c1_k(1, 0, SS1, SS2) * c1(x, SS1, SS2) * c1_k(0, 0, SS1, SS2) * c1(x, SS1, SS2)) / (2 * x * Math.Sqrt(Math.PI * x));
        }

        public static double c10(double x, double a, double ka)
        {
            return 1 - ((1 - ka) / a) * x;
        }

        public static double c20(double x, double a, double b, double ka, double kb)
        {
            return ka * Math.Pow(Math.E, Math.Log(kb / ka) * ((x - a) / (b - a)));
        }

        public static double f(double x, double a, double b, double ka, double kb)
        {
            if (x == 0)
                return 1;
            else if ((x > 0) && (x <= a))
                return c10(x, a, ka);
            else
                return c20(x, a, b, ka, kb);
        }

        public static double integral_e(double a, double b, double SS1, double SS2)
        {
            return (SS1 / SS2) * (Math.Exp(SS2 * b) - Math.Exp(SS2 * a));
        }

        ///////////////////////////////////////////////////////////////////////////End

        protected void Button2_Click(object sender, EventArgs e)
        {
            double l0 = Convert.ToDouble(tb_l0.Text);
            double delta = Convert.ToDouble(tb_delta.Text);
            double a0 = Convert.ToDouble(tb_a0.Text);
            double sigma = Convert.ToDouble(tb_sigma.Text);
            double KI_c_min = Convert.ToDouble(tb_KI_c_min.Text);
            double KI_c_max = Convert.ToDouble(tb_KI_c_max.Text);
            double alpha = Convert.ToDouble(tb_alpha.Text);
            double betta = Convert.ToDouble(tb_betta.Text);
            double mm = Convert.ToDouble(tb_mm.Text);
            double alpha_alpha = Convert.ToDouble(tb_alpha_alpha.Text);
            double betta_betta = Convert.ToDouble(tb_betta_betta.Text);

            //Multi-Dimensional Array
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int simulation_number = 0;
            for (double value = Convert.ToDouble(tb_param1.Text); value < Convert.ToDouble(tb_param2.Text); value = value + Convert.ToDouble(tb_param3.Text))
            {
                simulation_number++;
                switch (ddl_value.SelectedValue)
                {
                    case "l0":
                        l0 = value;
                        break;
                    case "delta":
                        delta = value;
                        break;
                    case "a0":
                        a0 = value;
                        break;
                    case "sigma":
                        sigma = value;
                        break;
                    case "KI_c_min":
                        KI_c_min = value;
                        break;
                    case "KI_c_max":
                        KI_c_max = value;
                        break;
                    case "alpha":
                        alpha = value;
                        break;
                    case "betta":
                        betta = value;
                        break;
                    case "mm":
                        mm = value;
                        break;
                    case "alpha_alpha":
                        alpha_alpha = value;
                        break;
                    case "betta_betta":
                        betta_betta = value;
                        break;
                }
                double[,] arrMultiD = Resh_env_cycle(l0 * Math.Pow(80, 2) / (Math.Pow(140, 2) * Math.PI), delta, a0, sigma, KI_c_min, KI_c_max, alpha, betta, mm, alpha_alpha, betta_betta);


                Chart1.Series.Add(simulation_number.ToString());
                Chart1.Series[simulation_number.ToString()].ChartTypeName = "line";
                ////         Chart1.AlignDataPointsByAxisLabel();

                for (int i = 1; i < arrMultiD.GetLength(0); i++)
                {
                    Chart1.Series[simulation_number.ToString()].Points.AddXY(Convert.ToInt64(arrMultiD[i, 7]), arrMultiD[i, 1]);

                }
            }
            Chart1.DataBind();
            Chart1.Visible = true;

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            long ts = stopWatch.ElapsedMilliseconds;

            // Format and display the TimeSpan value.
            //  float elapsedTime = ts.Milliseconds ;
            //     Label1.Text = "Время рассчета: " + (ts/1000).ToString() + " секунд.";
        }

        protected void ddl_value_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            CultureInfo Culture = new CultureInfo("en-US");
            lb_start.Text = DateTime.Now.ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);
            string l0 = tb_l0.Text;
            string delta = tb_delta.Text;
            string a0 = tb_a0.Text;
            string sigma = tb_sigma.Text;
            string KI_c_min = tb_KI_c_min.Text;
            string KI_c_max = tb_KI_c_max.Text;
            string alpha = tb_alpha.Text;
            string betta = tb_betta.Text;
            string mm = tb_mm.Text;
            string alpha_alpha = tb_alpha_alpha.Text;
            string betta_betta = tb_betta_betta.Text;

            double param1 = Convert.ToDouble(tb_param1.Text);
            double param2 = Convert.ToDouble(tb_param2.Text);
            double param3 = Convert.ToDouble(tb_param3.Text);
            double param4 = Convert.ToDouble(tb_param4.Text);

            int distribution_count;
            if (ddl_distributions.SelectedValue == "Interval")
            {
                distribution_count = (int)Math.Floor((param2 - param1) / param3);
                lb_result.Text = distribution_count.ToString();
            }
            else if (ddl_distributions.SelectedValue == "Weibull")
            {
                distribution_count = (int)param4;
            }
            else
            {
                distribution_count = (int)param3;
            }
            lb_result.ForeColor = System.Drawing.Color.Blue;
            lb_result.Text = (distribution_count).ToString() + " tasks added to Queue.";

            CloudQueueMessage[] Message = new CloudQueueMessage[distribution_count];
            if (ddl_distributions.SelectedValue == "Constant")
            {

             //   l0 = (Convert.ToDouble(l0) / (Math.Pow(Convert.ToDouble(KI_c_max), 2) / (Math.Pow(Convert.ToDouble(sigma), 2) * Math.PI))).ToString();
                Parallel.For(0, distribution_count, t =>
                {
                    // создаем новое сообщение
                    Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                        + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                    // добавляем сообщение в очередь
                    Queue.AddMessage(Message[t]);
                });
            }
            else
            {
                double[] arrDistribution = ditribution_func(param1, param2, param3, param4, distribution_count);
                switch (ddl_value.SelectedValue)
                {
                    case "l0":
                        Parallel.For(0, distribution_count, t =>
                       {
                           // создаем новое сообщение
                           Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + arrDistribution[t] + " " + delta + " " + a0 + " " + sigma + " "
                             + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                           // добавляем сообщение в очередь
                           Queue.AddMessage(Message[t]);
                       });
                        break;
                    case "delta":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + arrDistribution[t] + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "a0":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + arrDistribution[t] + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "sigma":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + arrDistribution[t] + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "KI_c_min":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + arrDistribution[t] + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "KI_c_max":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + arrDistribution[t] + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "alpha":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + arrDistribution[t] + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "betta":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + arrDistribution[t] + " " + mm + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "mm":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + arrDistribution[t] + " " + alpha_alpha + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "alpha_alpha":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + arrDistribution[t] + " " + betta_betta);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                    case "betta_betta":
                        Parallel.For(0, distribution_count, t =>
                        {
                            // создаем новое сообщение
                            Message[t] = new CloudQueueMessage((t + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                              + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + arrDistribution[t]);
                            // добавляем сообщение в очередь
                            Queue.AddMessage(Message[t]);
                        });
                        break;
                }
            }

         //   Chart1.Series[1].Points.AddXY((arrMultiD[table_count - 1, 6]), arrMultiD[table_count - 1, 0]);

      



            lb_finish.Text = DateTime.Now.ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);
            DateTime valval = (DateTime.ParseExact(lb_start.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
            DateTime valval2 = (DateTime.ParseExact(lb_finish.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
            TimeSpan valval3 = valval2.Subtract(valval);
            lb_time.Text = (valval3.TotalMilliseconds / 1000).ToString() + " seconds.";
   //         while ((myThread.IsAlive)||(myThread2.IsAlive)||(myThread3.IsAlive))
   //             Thread.Sleep(10000);
        }
/*
        public class TaskInfo : WebForm1
        {
            public int from;
            public int to;
            public string l0;
            public string delta;
            public string a0;
            public string sigma;
            public string KI_c_min;
            public string KI_c_max;
            public string alpha;
            public string betta;
            public string mm;
            public string alpha_alpha;
            public string betta_betta;
            public TaskInfo(int from, int to, string l0, string delta, string a0, string sigma, string KI_c_min, string KI_c_max,
            string alpha, string betta, string mm, string alpha_alpha, string betta_betta)
            {
                this.from = from;
                this.to = to;
                this.l0 = l0;
                this.delta = delta;
                this.a0 = a0;
                this.sigma = sigma;
                this.KI_c_min = KI_c_min;
                this.KI_c_max = KI_c_max;
                this.alpha = alpha;
                this.betta = betta;
                this.mm = mm;
                this.alpha_alpha = alpha_alpha;
                this.betta_betta = betta_betta;
            }

            public void ThreadAddMessageint (int from, int to, string l0, string delta, string a0, string sigma, string KI_c_min, string KI_c_max,
            string alpha, string betta, string mm, string alpha_alpha, string betta_betta)
        {
            for (int i = from; i < to; i++)
            {
                // создаем новое сообщение
                Message = new CloudQueueMessage((i + 1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                    + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);

                // добавляем сообщение в очередь
                Queue.AddMessage(Message);
            }
            }
        }
 */
        void ThreadAddMessage(string k, string l0, string delta, string a0, string sigma, string KI_c_min, string KI_c_max,
            string alpha, string betta, string mm, string alpha_alpha, string betta_betta)
           {
                // создаем новое сообщение
                Message = new CloudQueueMessage(k + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                    + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);

                // добавляем сообщение в очередь
                Queue.AddMessage(Message);
            }
/*
        void ThreadAddMessage(int from, int to, string l0, string delta, string a0, string sigma, string KI_c_min, string KI_c_max,
            string alpha, string betta, string mm, string alpha_alpha, string betta_betta)
        {

            for (int i = from; i < to; i++)
            {
                // создаем новое сообщение
                Message = new CloudQueueMessage((i+1).ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
                    + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta);

                // добавляем сообщение в очередь

                Queue.AddMessage(Message);
            }
            
        }
*/
        protected void bt_view_Click(object sender, EventArgs e)
        {
            CloudQueue Queue = Client.GetQueueReference("queuecrack");
            Queue.FetchAttributes();

            int? message_count = Queue.ApproximateMessageCount;
            if (message_count.Value > 0)
            {
                lb_result.ForeColor = System.Drawing.Color.Red;
                if (message_count.Value == 1)
                {
                    lb_result.Text = "Calculations  are not finished! It's just one message in the Queue.";
                }
                else
                {
                    
                    lb_result.Text = "Calculations  are not finished! It's " + message_count.Value.ToString() + " messages in the Queue.";
                }
            }
            else
            {
                lb_result.ForeColor = System.Drawing.Color.Green;
                lb_result.Text = "Calculations  completed!";

                CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
                CloudTable TableData = tableClient.GetTableReference("TableData");
                CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");
                int table_simulation_count;
                DataTable dt_data = new DataTable();
                dt_data.Columns.Add("№№", Type.GetType("System.String"));
                dt_data.Columns.Add("№", Type.GetType("System.String"));
                for (int i = 1; i <= 14; i++)
                    dt_data.Columns.Add(i.ToString(), Type.GetType("System.String"));



                TableQuery<EntityOfTableSimulation> QuerySimulation = new TableQuery<EntityOfTableSimulation>();
                table_simulation_count = 0;
                foreach (EntityOfTableSimulation entity in TableSimulation.ExecuteQuery(QuerySimulation))
                {
                    table_simulation_count++;
                }

                double[,] arrTableSimulation = new double[table_simulation_count, 4];
                table_simulation_count = 0;
                foreach (EntityOfTableSimulation entity in TableSimulation.ExecuteQuery(QuerySimulation))
                {
                    arrTableSimulation[table_simulation_count, 0] = Convert.ToDouble(entity.RowKey);
                    arrTableSimulation[table_simulation_count, 1] = Convert.ToDouble(entity.time_simulation);
                    arrTableSimulation[table_simulation_count, 2] = Convert.ToDouble(entity.x);
                    arrTableSimulation[table_simulation_count, 3] = Convert.ToDouble(entity.y);
                    table_simulation_count++;
                    Epoch = entity.Timestamp;
                }
                ///Раскоментить для засекания времени
                /*
                CultureInfo Culture = new CultureInfo("en-US");
                lb_finish.Text = Epoch.ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);
                DateTime valval = (DateTime.ParseExact(lb_start.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
                DateTime valval2 = (DateTime.ParseExact(lb_finish.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
                TimeSpan valval3 = valval2.Subtract(valval);
                lb_time.Text = (valval3.TotalMilliseconds / 1000).ToString() + " seconds.";
            */
                
                //    lb_finish.Text = Epoch.AddHours(4).ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);
                /*
                DateTime valval = (DateTime.ParseExact(lb_start.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
                DateTime valval2 = (DateTime.ParseExact(lb_finish.Text, "HH:mm:ss.fff MM/dd/yyyy", Culture));
                TimeSpan valval3 = valval2.Subtract(valval);
                lb_time.Text = (valval3.TotalMilliseconds / 1000).ToString() + " seconds.";
                */
                //  lb_time.Text = (DateTime.ParseExact(lb_finish.Text, "HH:mm:ss.fff dd-MM-yyyy", Culture) - DateTime.ParseExact(lb_start.Text, "HH:mm:ss.fff dd-MM-yyyy", Culture)).ToString("HH:mm:ss.fff", Culture);
                //     ((DateTime.Parse(lb_finish.Text) - DateTime.Parse(lb_start.Text)).Milliseconds/1000).ToString();
                //Label1.Text = table_count.ToString() + "  " + i.ToString();

                //Vertical DataTableSimulation          
                DataTable dt_simulation = new DataTable();
                dt_simulation.Columns.Add("Point", Type.GetType("System.String"));
                dt_simulation.Columns.Add("Calculation time", Type.GetType("System.String"));
                dt_simulation.Columns.Add("X = l0", Type.GetType("System.String"));
                dt_simulation.Columns.Add("Y = t*", Type.GetType("System.String"));

                double table_sumulation_y_min = arrTableSimulation[0, 3];
                double table_sumulation_y_max = arrTableSimulation[0, 3];
                double total_time_simulation = 0;
                for (int i = 0; i < table_simulation_count; i++)
                {
                    dt_simulation.Rows.Add();
                    dt_simulation.Rows[dt_simulation.Rows.Count - 1]["Point"] = arrTableSimulation[i, 0];
                    dt_simulation.Rows[dt_simulation.Rows.Count - 1]["Calculation time"] = (double)arrTableSimulation[i, 1] / 1000 + " sec";
                    dt_simulation.Rows[dt_simulation.Rows.Count - 1]["X = l0"] = arrTableSimulation[i, 2];
                    dt_simulation.Rows[dt_simulation.Rows.Count - 1]["Y = t*"] = arrTableSimulation[i, 3];
                    if (arrTableSimulation[i, 3] < table_sumulation_y_min)
                    {
                        table_sumulation_y_min = arrTableSimulation[i, 3];
                    }
                    if (arrTableSimulation[i, 3] > table_sumulation_y_max)
                    {
                        table_sumulation_y_max = arrTableSimulation[i, 3];
                    }
                    total_time_simulation += arrTableSimulation[i, 1];
                }
                lb_total_time.Text = (double)total_time_simulation / 1000 + " sec.";

           /*     if (table_sumulation_y_min < 10)
                {
                    table_sumulation_y_min = 10;
                }
                else*/
                {
                    table_sumulation_y_min = Math.Floor((table_sumulation_y_min) / 1000) * 1000;
                }
                //table_sumulation_y_min = 10;

                table_sumulation_y_max = Math.Ceiling((table_sumulation_y_max) / 1000) * 1000;



                /*
                int table_count;
                string KeyLength = "000000000000000000000";
                for (int simulation_number = 1; simulation_number <= table_simulation_count; simulation_number++)
                {
                    TableQuery<EntityOfTableData> queryData = new TableQuery<EntityOfTableData>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, simulation_number.ToString(KeyLength)));
                    //Chart1.Series["Testing"].XAxisType = DataVisualization.Charting.AxisType.Secondary;  
                    Chart1.Series.Add(simulation_number.ToString());
                    Chart1.Series[simulation_number.ToString()].ChartTypeName = "spline";
                    //       Chart1.AlignDataPointsByAxisLabel();

                    table_count = 0;
                    foreach (EntityOfTableData entity in TableData.ExecuteQuery(queryData))
                    {
                        table_count++;
                    }
                      
                    double[,] arrMultiD = new double[table_count, 14];
                    table_count = 0;


                    foreach (EntityOfTableData entity in TableData.ExecuteQuery(queryData))
                    {
                        arrMultiD[table_count, 0] = Convert.ToDouble(entity.par1);
                        arrMultiD[table_count, 1] = Convert.ToDouble(entity.par2);
                        arrMultiD[table_count, 2] = Convert.ToDouble(entity.par3);
                        arrMultiD[table_count, 3] = Convert.ToDouble(entity.par4);
                        arrMultiD[table_count, 4] = Convert.ToDouble(entity.par5);
                        arrMultiD[table_count, 5] = Convert.ToDouble(entity.par6);
                        arrMultiD[table_count, 6] = Convert.ToDouble(entity.par7);
                        arrMultiD[table_count, 7] = Convert.ToDouble(entity.par8);
                        arrMultiD[table_count, 8] = Convert.ToDouble(entity.par9);
                        arrMultiD[table_count, 9] = Convert.ToDouble(entity.par10);
                        arrMultiD[table_count, 10] = Convert.ToDouble(entity.par11);
                        arrMultiD[table_count, 11] = Convert.ToDouble(entity.par12);
                        arrMultiD[table_count, 12] = Convert.ToDouble(entity.par13);
                        arrMultiD[table_count, 13] = Convert.ToDouble(entity.par14);
                        //lb_finish.Text = entity.Timestamp.ToString("HH:mm:ss tt");
                        Epoch = entity.Timestamp;
                        table_count++;
                    }
                    CultureInfo Culture = new CultureInfo("en-US");
                    lb_finish.Text = Epoch.ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);
                //    lb_finish.Text = Epoch.AddHours(4).ToString("HH:mm:ss.fff MM/dd/yyyy", Culture);



                    //  lb_time.Text = (DateTime.ParseExact(lb_finish.Text, "HH:mm:ss.fff dd-MM-yyyy", Culture) - DateTime.ParseExact(lb_start.Text, "HH:mm:ss.fff dd-MM-yyyy", Culture)).ToString("HH:mm:ss.fff", Culture);
                    //     ((DateTime.Parse(lb_finish.Text) - DateTime.Parse(lb_start.Text)).Milliseconds/1000).ToString();
                    //Label1.Text = table_count.ToString() + "  " + i.ToString();
               

                  Chart2.Series["Series1"].Points.AddXY(arrMultiD[0, 0], arrMultiD[table_count - 1, 6]);

////////////////////////////


                    for (int i = 0; i < table_count; i = i + 200)
                    {
                        Chart1.Series[simulation_number.ToString()].Points.AddXY((arrMultiD[i, 6]), arrMultiD[i, 0]);
                    }
                    Chart1.Series[simulation_number.ToString()].Points.AddXY((arrMultiD[table_count - 1, 6]), arrMultiD[table_count - 1, 0]);

      
                    ///////////////////////////////

                    //  Chart1.Series[simulation_number.ToString()].IsXValueIndexed = true;

                    for (int i = 0; i < table_count; i++)
                    {
                        dt_data.Rows.Add();
                        dt_data.Rows[dt_data.Rows.Count - 1]["№№"] = simulation_number;
                        dt_data.Rows[dt_data.Rows.Count - 1]["№"] = i + 1;
                        dt_data.Rows[dt_data.Rows.Count - 1]["1"] = arrMultiD[i, 0];
                        dt_data.Rows[dt_data.Rows.Count - 1]["2"] = arrMultiD[i, 1];
                        dt_data.Rows[dt_data.Rows.Count - 1]["3"] = arrMultiD[i, 2];
                        dt_data.Rows[dt_data.Rows.Count - 1]["4"] = arrMultiD[i, 3];
                        dt_data.Rows[dt_data.Rows.Count - 1]["5"] = arrMultiD[i, 4];
                        dt_data.Rows[dt_data.Rows.Count - 1]["6"] = arrMultiD[i, 5];
                        dt_data.Rows[dt_data.Rows.Count - 1]["7"] = arrMultiD[i, 6];
                        dt_data.Rows[dt_data.Rows.Count - 1]["8"] = arrMultiD[i, 7];
                        dt_data.Rows[dt_data.Rows.Count - 1]["9"] = arrMultiD[i, 8];
                        dt_data.Rows[dt_data.Rows.Count - 1]["10"] = arrMultiD[i, 9];
                        dt_data.Rows[dt_data.Rows.Count - 1]["11"] = arrMultiD[i, 10];
                        dt_data.Rows[dt_data.Rows.Count - 1]["12"] = arrMultiD[i, 11];
                        dt_data.Rows[dt_data.Rows.Count - 1]["13"] = arrMultiD[i, 12];
                        dt_data.Rows[dt_data.Rows.Count - 1]["14"] = arrMultiD[i, 13];
                    }
                }
                Chart1.DataBind();
                Chart1.Visible = true;

                Chart2.DataBind();
                Chart2.Visible = true;

*/               
                
                gv_simulation.DataSource = dt_simulation;
                gv_simulation.DataBind();
                gv_simulation.Visible = true;
                gv_data.DataBind();
                gv_data.DataSource = dt_data;
                gv_data.Visible = true;           /// поставил тут фолс!

                int step_time_simulation = 2500 * (int)Math.Ceiling((table_sumulation_y_max - table_sumulation_y_min) / 1000 / table_simulation_count); //10000; 
                int time_simulation = (int)(table_sumulation_y_max);// - table_sumulation_y_min);



                int step_count = time_simulation / step_time_simulation + 1;

                int[] probability_density = new int[step_count];



                time_simulation = 0;// (int)table_sumulation_y_min;
                for (int i = 0; i < table_simulation_count; i++)
                {
                    probability_density[(int)Math.Floor((arrTableSimulation[i, 3]- table_sumulation_y_min) / step_time_simulation)]++;
                }
                ///// Floor  Ceiling

                crt_histogram.ChartAreas["ChartArea1"].AxisX.Minimum = table_sumulation_y_min;
                crt_histogram.ChartAreas["ChartArea1"].AxisX.Interval = step_time_simulation;

                crt_distrib_func.ChartAreas["ChartArea1"].AxisX.Minimum = table_sumulation_y_min;
                crt_distrib_func.ChartAreas["ChartArea1"].AxisX.Interval = step_time_simulation;

                int time = (int)table_sumulation_y_min + step_time_simulation / 2;
                ////           Chart5.Series["Series1"].Points.AddXY(table_sumulation_y_min, distribution_func);
                ////          Chart5.Series["Series1"].Points.AddXY(time, distribution_func);

                for (int i = 0; i < step_count; i++)
                {
                    crt_histogram.Series["Series1"].Points.AddXY(time, (double)probability_density[i] / table_simulation_count);
                    time += step_time_simulation;
                }
                // Chart6.ChartAreas["ChartArea1"].AxisX.Interval = step_time_simulation;
                crt_distrib_func.ChartAreas["ChartArea1"].AxisY.Maximum = 1;
                double distribution_func = 0;
                crt_distrib_func.Series["Series1"].Points.AddXY(10, distribution_func);
                crt_distrib_func.Series["Series1"].Points.AddXY(table_sumulation_y_min, distribution_func);
                time = (int)table_sumulation_y_min - step_time_simulation / 2;
                for (int i = 0; i < step_count; i++)
                {
                    distribution_func += (double)probability_density[i] / table_simulation_count;
                    time += step_time_simulation;
                    crt_distrib_func.Series["Series1"].Points.AddXY(time, distribution_func);

                }

                crt_histogram.DataBind();
                crt_distrib_func.DataBind();
                crt_histogram.Visible = true;
                crt_distrib_func.Visible = true;

               /*

                dt_data.Columns.Add("№№", Type.GetType("System.String"));
                dt_data.Columns.Add("№", Type.GetType("System.String"));
                for (int i = 1; i <= 14; i++)
                    dt_data.Columns.Add(i.ToString(), Type.GetType("System.String"));


                for (int i = 0; i < table_simulation_count ; i++)
                {
                    dt_data.Rows[dt_data.Rows.Count - 1]["№№"] = 1;
                    dt_data.Rows[dt_data.Rows.Count - 1]["№"] = i + 1;
                    dt_data.Rows[dt_data.Rows.Count - 1]["1"] = arrMultiD[i, 0];
                    dt_data.Rows[dt_data.Rows.Count - 1]["2"] = arrMultiD[i, 1];
                    dt_data.Rows[dt_data.Rows.Count - 1]["3"] = arrMultiD[i, 2];
                    dt_data.Rows[dt_data.Rows.Count - 1]["4"] = arrMultiD[i, 3];
                    dt_data.Rows[dt_data.Rows.Count - 1]["5"] = arrMultiD[i, 4];
                    dt_data.Rows[dt_data.Rows.Count - 1]["6"] = arrMultiD[i, 5];
                    dt_data.Rows[dt_data.Rows.Count - 1]["7"] = arrMultiD[i, 6];
                    dt_data.Rows[dt_data.Rows.Count - 1]["8"] = arrMultiD[i, 7];
                    dt_data.Rows[dt_data.Rows.Count - 1]["9"] = arrMultiD[i, 8];
                    dt_data.Rows[dt_data.Rows.Count - 1]["10"] = arrMultiD[i, 9];
                    dt_data.Rows[dt_data.Rows.Count - 1]["11"] = arrMultiD[i, 10];
                    dt_data.Rows[dt_data.Rows.Count - 1]["12"] = arrMultiD[i, 11];
                    dt_data.Rows[dt_data.Rows.Count - 1]["13"] = arrMultiD[i, 12];
                    dt_data.Rows[dt_data.Rows.Count - 1]["14"] = arrMultiD[i, 13];
                }  */
            }
                 
        }

        protected void bt_droup_data_Click(object sender, EventArgs e)
        {
            //Deleting
            CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
            CloudTable TableData = tableClient.GetTableReference("TableData");
            TableData.DeleteIfExists();
            CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");
            TableSimulation.DeleteIfExists();
            Queue = Client.GetQueueReference("queuecrack");
            Queue.Clear();
            //Creating
            TableData = tableClient.GetTableReference("TableData");
            TableData.CreateIfNotExists();
            TableSimulation = tableClient.GetTableReference("TableSimulation");
            TableSimulation.CreateIfNotExists();
            //Unvisible
            Chart1.Visible = false;
            Chart2.Visible = false;
            gv_simulation.Visible = false;
            gv_data.Visible = false;
            lb_start.Text = "";
            lb_finish.Text = "";
            lb_time.Text = "";
            lb_result.Text = "";
        }

        public double[] ditribution_func(double param1, double param2, double param3, double param4, int distribution_count)
        {
            double[] arrDistribution = new double[distribution_count];

            switch (ddl_distributions.SelectedValue)
            {
                case "Interval":
                   /* int index = 0;
                    for (double value = param1; value <= param2; value += param3)
                    {
                        arrDistribution[index] = value;
                        index++;
                    }*/
                    double value = param1;
                    for (int index = 0; index < distribution_count; index++)
                    {
                        arrDistribution[index] = value;
                        value += param3;
                    }
                    break;
                case "Constant":
                    /* int index = 0;
                     for (double value = param1; value <= param2; value += param3)
                     {
                         arrDistribution[index] = value;
                         index++;
                     }*/
                    for (int index = 0; index < distribution_count; index++)
                    {
                        arrDistribution[index] = param1;
                    }
                    break;
                case "Uniform":
                    var uniform = new MersenneTwister((int)param4);
                    for (int i = 0; i < distribution_count; i++)
                        arrDistribution[i] = param2 + (uniform.NextDouble() * (param1 - param2));
                    break;
                case "Normal":
                    var normal = new Normal(param1, Math.Sqrt(param2));
                    normal.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = normal.Samples().Take(distribution_count).ToArray();
                    break;
                case "Exponential":
                    var exponential = new Exponential(param1);
                    exponential.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = exponential.Samples().Take(distribution_count).ToArray();
                    break;
                case "Gamma":
                    var gamma = new Gamma(param1, param2);
                    gamma.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = gamma.Samples().Take(distribution_count).ToArray();
                    break;
                case "Weibull":
                    var weibull = new Weibull(param2, param3);
                    weibull.RandomSource = new MersenneTwister(7);
                    arrDistribution = weibull.Samples().Take(distribution_count).ToArray();
                    for (int i = 0; i < distribution_count; i++)
                    {
                        arrDistribution[i] = arrDistribution[i] + param1;
                    }
                    break;
            }
            return arrDistribution;
        }

        protected void bt_view_distribution_Click(object sender, EventArgs e)
        {
            double param1 = Convert.ToDouble(tb_param1.Text);
            double param2 = Convert.ToDouble(tb_param2.Text);
            double param3 = Convert.ToDouble(tb_param3.Text);
            double param4 = Convert.ToDouble(tb_param4.Text);

            int distribution_count;
            if (ddl_distributions.SelectedValue == "Interval")
            {
                distribution_count = (int)Math.Floor((param2 - param1) / param3);
                lb_result.Text = distribution_count.ToString();
            }
            else if (ddl_distributions.SelectedValue == "Weibull")
            {
                distribution_count = (int)param4;
            }
            else
            {
                distribution_count = (int)param3;
            }
            
            double[] arrDistribution = ditribution_func(param1, param2, param3, param4, distribution_count);

            double table_sumulation_y_min=arrDistribution[0];
            double table_sumulation_y_max=arrDistribution[0];
            for (int i = 0; i < distribution_count; i++)
            {
                crt_distribution.Series["Series1"].Points.AddXY(i + 1, arrDistribution[i]);
                if (arrDistribution[i] < table_sumulation_y_min)
                {
                    table_sumulation_y_min = arrDistribution[i];
                }
                if (arrDistribution[i] > table_sumulation_y_max)
                {
                    table_sumulation_y_max = arrDistribution[i];
                }
            }
            table_sumulation_y_min = Math.Floor((table_sumulation_y_min) / 100) * 100;

            int step_time_simulation = /* 250 * */(int)Math.Ceiling((table_sumulation_y_max - table_sumulation_y_min) /*/ 100*/ / distribution_count); //10000; 
            int time_simulation = (int)(table_sumulation_y_max);// - table_sumulation_y_min);


            int step_count = time_simulation / step_time_simulation + 1;

            int[] probability_density = new int[step_count];

            time_simulation = 0;// (int)table_sumulation_y_min;
            for (int i = 0; i < distribution_count; i++)
            {
                probability_density[(int)Math.Floor((arrDistribution[i] - table_sumulation_y_min) / step_time_simulation)]++;
            }

            ///// Floor  Ceiling

            crt_histogram.ChartAreas["ChartArea1"].AxisX.Minimum = table_sumulation_y_min;
            crt_histogram.ChartAreas["ChartArea1"].AxisX.Interval = step_time_simulation;

            crt_distrib_func.ChartAreas["ChartArea1"].AxisX.Minimum = table_sumulation_y_min;
            crt_distrib_func.ChartAreas["ChartArea1"].AxisX.Interval = step_time_simulation;

            int time = (int)table_sumulation_y_min +(int)((double)step_time_simulation/2);
            ////           Chart5.Series["Series1"].Points.AddXY(table_sumulation_y_min, distribution_func);
            ////          Chart5.Series["Series1"].Points.AddXY(time, distribution_func);

            for (int i = 0; i < step_count; i++)
            {
                crt_histogram.Series["Series1"].Points.AddXY(time, (double)probability_density[i] / distribution_count);
                time += step_time_simulation;
            }




            crt_distribution.DataBind();
            crt_distribution.Visible = true;

            crt_histogram.DataBind();
            crt_histogram.Visible = true;
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            string l0 = tb_l0.Text;
            string delta = tb_delta.Text;
            string a0 = tb_a0.Text;
            string sigma = tb_sigma.Text;
            string KI_c_min = tb_KI_c_min.Text;
            string KI_c_max = tb_KI_c_max.Text;
            string alpha = tb_alpha.Text;
            string betta = tb_betta.Text;
            string mm = tb_mm.Text;
            string alpha_alpha = tb_alpha_alpha.Text;
            string betta_betta = tb_betta_betta.Text;

            double param1 = Convert.ToDouble(tb_param1.Text);
            double param2 = Convert.ToDouble(tb_param2.Text);
            double param3 = Convert.ToDouble(tb_param3.Text);
            double param4 = Convert.ToDouble(tb_param4.Text); 

            int distribution_count;
            if (ddl_distributions.SelectedValue == "Interval")
            {
                distribution_count = (int)Math.Floor((param2 - param1) / param3);
                lb_result.Text = distribution_count.ToString();
            }
            else if (ddl_distributions.SelectedValue == "Weibull")
            {
                distribution_count = (int)param4;
            }
            else
            {
                distribution_count = (int)param3;
            }
            distribution_count = (int)param4;
            lb_result.ForeColor = System.Drawing.Color.Blue;
            lb_result.Text = (distribution_count).ToString() + " tasks added to Queue.";


            double[] arrDistribution = new double[distribution_count];
            switch (ddl_distributions.SelectedValue)
            {
                case "Interval":
                    lb_start.Text = "DIstribution Interval!";
                    double value = param1;
                    for (int index = 0; index < distribution_count; index++)
                    {
                        arrDistribution[index] = value;
                        value += param3;
                    }
                    break;
                case "Constant":
                    /* int index = 0;
                     for (double value = param1; value <= param2; value += param3)
                     {
                         arrDistribution[index] = value;
                         index++;
                     }*/
                    for (int index = 0; index < distribution_count; index++)
                    {
                        arrDistribution[index] = param1;
                    }
                    break;
                case "Uniform":
                    var uniform = new MersenneTwister((int)param4);
                    for (int i = 0; i < distribution_count; i++)
                        arrDistribution[i] = param2 + (uniform.NextDouble() * (param1 - param2));
                    break;
                case "Normal":
                    var normal = new Normal(param1, Math.Sqrt(param2));
                    normal.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = normal.Samples().Take(distribution_count).ToArray();
                    break;
                case "Exponential":
                    var exponential = new Exponential(param1);
                    exponential.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = exponential.Samples().Take(distribution_count).ToArray();
                    break;
                case "Gamma":
                    var gamma = new Gamma(param1, param2);
                    gamma.RandomSource = new MersenneTwister((int)param4);
                    arrDistribution = gamma.Samples().Take(distribution_count).ToArray();
                    break;
                case "Weibull":
                    var weibull = new Weibull(param2, param3);
                    weibull.RandomSource = new MersenneTwister(7);
                    arrDistribution = weibull.Samples().Take(distribution_count).ToArray();
                    for (int i = 0; i < distribution_count; i++)
                    {
                        arrDistribution[i] = arrDistribution[i] + param1;
                    }
                    break;
            }
            lb_finish.Text = param1.ToString() + " " + param2.ToString() + " " + param3.ToString() + " " + param4.ToString();

            for (int i = 0; i < distribution_count; i++)
                lb_time.Text = lb_time.Text + " " + arrDistribution[i].ToString();

        //    lb_time.Text = distribution_count.ToString() + " " + l0 + " " + delta + " " + a0 + " " + sigma + " "
         //           + KI_c_min + " " + KI_c_max + " " + alpha + " " + betta + " " + mm + " " + alpha_alpha + " " + betta_betta;






        }

        protected void ddl_distributions_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ddl_distributions.SelectedValue)
            {
                case "Interval":
                    lb_param1.Text = "From";
                    lb_param2.Text = "To";
                    lb_param3.Text = "Step";
                    tb_param3.Text = "0,01";
                    lb_param2.Visible = true;
                    tb_param2.Visible = true;
                    lb_param4.Visible = false;
                    tb_param4.Visible = false;
                    break;
                case "Constant":
                    lb_param1.Text = "Value =";
                    lb_param3.Text = "Count";
                    tb_param3.Text = "1";
                    lb_param2.Visible = false;
                    tb_param2.Visible = false;
                    lb_param4.Visible = false;
                    tb_param4.Visible = false;
                    break;
                case "Uniform":
                    lb_param1.Text = "From";
                    lb_param2.Text = "To";
                    lb_param3.Text = "Count";
                    lb_param4.Text = "Seed";
                    tb_param3.Text = "1000";
                    lb_param2.Visible = true;
                    tb_param2.Visible = true;
                    lb_param4.Visible = true;
                    tb_param4.Visible = true;
                    break;
                case "Normal":
                    lb_param1.Text = "Mean";
                    lb_param2.Text = "Variance";
                    lb_param3.Text = "Count";
                    lb_param4.Text = "Seed";
                    tb_param3.Text = "1000";
                    lb_param2.Visible = true;
                    tb_param2.Visible = true;
                    lb_param4.Visible = true;
                    tb_param4.Visible = true;
                    break;
                case "Exponential":
                    lb_param1.Text = "Lamda";
                    lb_param3.Text = "Count";
                    lb_param4.Text = "Seed";
                    tb_param3.Text = "1000";
                    lb_param2.Visible = false;
                    tb_param2.Visible = false;
                    lb_param4.Visible = true;
                    tb_param4.Visible = true;
                    break;
                case "Gamma":
                    lb_param1.Text = "Shape";
                    lb_param2.Text = "Rate";
                    lb_param3.Text = "Number";
                    lb_param4.Text = "Seed";
                    tb_param3.Text = "1000";
                    lb_param2.Visible = true;
                    tb_param2.Visible = true;
                    lb_param4.Visible = true;
                    tb_param4.Visible = true;
                    break;
                case "Weibull":
                    lb_param1.Text = "Min";
                    lb_param2.Text = "Shape (k)";
                    lb_param3.Text = "Scale (lamda)";
                    lb_param4.Text = "Number";
                    tb_param1.Text = "0";
                    tb_param2.Text = "1,5";
                    tb_param3.Text = "15";
                    tb_param4.Text = "1000";
                    lb_param2.Visible = true;
                    tb_param2.Visible = true;
                    lb_param4.Visible = true;
                    tb_param4.Visible = true;                  
                    break;
            }
        }

        public static double fatigue_t_length(double l0, double l, double sigma, double ff)
        {
            return Integral.integral(new Integral.Function(fatigue_integral_func), l0, l, sigma, ff);
        }

        public static double fatigue_integral_func(double x, double sigma, double ff)
        {
            return (1 / (C * Math.Pow(sigma * Math.Sqrt(Math.PI * x), n)));
            //     return (c1_k(1, 0, SS1, SS2) * c1(x, SS1, SS2) * c1_k(0, 0, SS1, SS2) * c1(x, SS1, SS2)) / (2 * x * Math.Sqrt(Math.PI * x));
        }

        public static double[,] fatigue_l_t(double ff, double sigma, double l0, double delta, double K_Ic_max)
        {
            double l = l0;
            double[,] a = new double[100, 2];
            for (int i = 0; i < 100; i++)
            {
                a[i, 0] = l;
                a[i, 1] = fatigue_t_length(l0, l, sigma, ff);
                l = l + (((1 - delta) * Math.Pow(K_Ic_max, 2)) / (Math.PI * Math.Pow(sigma, 2)) - l0) / 100;
            }
            return a;
        }

        protected void Button5_Click(object sender, EventArgs e)
        {
            double[,] aa = fatigue_l_t(1.2, 120, 0.01, 0.05, 80);
            
            Chart1.Series.Add("1");
         //   Chart1.Series["0"].ChartTypeName = "spline";
            for (int i = 0; i < 2; i = i++)
            {
                Chart1.Series["1"].Points.AddXY(4+i,2+i);
            }
            Chart1.DataBind();
        
            lb_finish.Text = aa[5, 0].ToString();
        }




        ////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////End
    }
}
