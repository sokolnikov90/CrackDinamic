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

        #region Взаимодействие с хранилищем данных Azure Storage

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

        #endregion;

        #region Обработка событий

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

        }

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
                ///Раскомментить для вычисления времени выполнения
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


                table_sumulation_y_min = Math.Floor((table_sumulation_y_min) / 1000) * 1000;
       

                table_sumulation_y_max = Math.Ceiling((table_sumulation_y_max) / 1000) * 1000;
 
                
                gv_simulation.DataSource = dt_simulation;
                gv_simulation.DataBind();
                gv_simulation.Visible = true;
                gv_data.DataBind();
                gv_data.DataSource = dt_data;
                gv_data.Visible = true;          

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

                //Разкомментить для вывода полной таблицы
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
                    double value = param1;
                    for (int index = 0; index < distribution_count; index++)
                    {
                        arrDistribution[index] = value;
                        value += param3;
                    }
                    break;
                case "Constant":
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

        #endregion;
    }
}
