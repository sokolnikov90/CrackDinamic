using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {

        CloudStorageAccount StorageAccount { get; set; }    // аккаунт хранилища
        CloudQueueClient Client { get; set; }               // клиент очереди
        CloudQueue Queue { get; set; }                      // вроде как сама очередь
        CloudQueueMessage Message { get; set; }             // сообщение, получаемое(отправляемое) из(в) очереди(ь)
        
        public override void Run()
        {
            CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
            CloudTable TableData = tableClient.GetTableReference("TableData");
            CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");
            string[] argument = new string[12];
            TimeSpan interval = new TimeSpan(0, 10, 0);
            while (true)
            {
         
               Message = Queue.GetMessage(interval, null, null);
                if (Message != null)
                {
                    argument = Message.AsString.Split(' ');
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    double[,] arrMultiD = Resh_env_cycle(Convert.ToDouble(argument[1]) * Math.Pow(Convert.ToDouble(argument[6]), 2) / (Math.Pow(Convert.ToDouble(argument[4]), 2) * Math.PI), Convert.ToDouble(argument[2]),
                        Convert.ToDouble(argument[3]), Convert.ToDouble(argument[4]), Convert.ToDouble(argument[5]), Convert.ToDouble(argument[6]),
                        Convert.ToDouble(argument[7]), Convert.ToDouble(argument[8]), Convert.ToDouble(argument[9]), Convert.ToDouble(argument[10]), Convert.ToDouble(argument[11]));
/*
                    double[,] arrMultiD = Resh_env_cycle(Convert.ToDouble(argument[1]), Convert.ToDouble(argument[2]),
                        Convert.ToDouble(argument[3]), Convert.ToDouble(argument[4]), Convert.ToDouble(argument[5]), Convert.ToDouble(argument[6]),
                        Convert.ToDouble(argument[7]), Convert.ToDouble(argument[8]), Convert.ToDouble(argument[9]), Convert.ToDouble(argument[10]), Convert.ToDouble(argument[11]));
      */                 
                    stopWatch.Stop();
                    double [,] a = fatigue_l_t(0.5,Convert.ToDouble(argument[4]),Convert.ToDouble(argument[1]) * Math.Pow(Convert.ToDouble(argument[6]), 2) / (Math.Pow(Convert.ToDouble(argument[4]), 2) * Math.PI),
                        Convert.ToDouble(argument[2]), Convert.ToDouble(argument[6]));
                    // Get the elapsed time as a TimeSpan value.
                    long ts = stopWatch.ElapsedMilliseconds;
                    int last_table_line = 0;

                                       
                    for (int i=1; i < arrMultiD.GetLength(0); i++)
                    {
                        
                        if (arrMultiD[i, 1] != 0)
                        {
                    /*        InsertToTableData(TableData, Convert.ToInt32(argument[0]), i, arrMultiD[i, 1].ToString(), arrMultiD[i, 2].ToString(), arrMultiD[i, 3].ToString(), arrMultiD[i, 4].ToString(),
                                              arrMultiD[i, 5].ToString(), arrMultiD[i, 6].ToString(), arrMultiD[i, 7].ToString(), arrMultiD[i, 8].ToString(), arrMultiD[i, 9].ToString(),
                                              arrMultiD[i, 10].ToString(), arrMultiD[i, 11].ToString(), arrMultiD[i, 12].ToString(), arrMultiD[i, 13].ToString(), arrMultiD[i, 14].ToString());
                      */      last_table_line = i;
                        }
                        else
                        {
                            break;
                        }
                         
                        if (arrMultiD[i, 1] == 0)
                        {
                            last_table_line = i - 1;
                            break;
                        }
                          
                    }






                    InsertToTableSimulation(TableSimulation, 0, Convert.ToInt32(argument[0]), ts.ToString(), arrMultiD[1, 1].ToString(), arrMultiD[last_table_line, 7].ToString());
                    // удаляем сообщение из очереди
                    Queue.DeleteMessage(Message);
                }
                else
                {
                    Thread.Sleep(1000);
                }

            }
        }

///////////////
               //     Run_func(argument);
              //      InsertToTableData(TableData, argument[0], argument[1], argument[2], argument[3], argument[4], argument[5],
             //           argument[6], argument[7], argument[8], argument[9], argument[10], "0", "a",
             //           "b", "c", "d");
/*
                  for (int i = 1; i < 6; i++)
                    {

                        InsertToTableData(TableData, argument[0], i.ToString(), argument[1], argument[2], argument[3], argument[4], argument[5],
                        argument[6], argument[7], argument[8], argument[9], argument[10], argument[11], "a",
                        "b", "c");
                    } */
 


        public class EntityOfTableData : TableEntity
        {
            public static string KeyLength = "000000000000000000000";
            public EntityOfTableData(int id_simulation, int id_line)
            {
                this.PartitionKey = id_simulation.ToString(KeyLength);
                this.RowKey = id_line.ToString(KeyLength);
            }
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
            public string time_simulation { get; set; }
            public string x { get; set; }
            public string y { get; set; }
        }

        private static void InsertToTableData(CloudTable table, int id_simulation, int id_line, string par1, string par2, string par3, string par4, 
            string par5, string par6, string par7, string par8, string par9, string par10, string par11, string par12, string par13, string par14)
        {
            try
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                EntityOfTableData entity = new EntityOfTableData(id_simulation, id_line);
                entity.par1 = par1;
                entity.par2 = par2;
                entity.par3 = par3;
                entity.par4 = par4;
                entity.par5 = par5;
                entity.par6 = par6;
                entity.par7 = par7;
                entity.par8 = par8;
                entity.par9 = par9;
                entity.par10 = par10;
                entity.par11 = par11;
                entity.par12 = par12;
                entity.par13 = par13;
                entity.par14 = par14;
                batchOperation.Insert(entity);
                table.ExecuteBatch(batchOperation);
            }
            catch
            {
                throw;
            }
        }

        private static void InsertToTableSimulation(CloudTable table, int id_simulation, int id_result, string time_simulation, string x, string y)
        {
            try
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                EntityOfTableSimulation entity = new EntityOfTableSimulation(id_simulation, id_result);
                entity.time_simulation = time_simulation;
                entity.x = x;
                entity.y = y;
                batchOperation.Insert(entity);
                table.ExecuteBatch(batchOperation);
            }
            catch
            {
                throw;
            }
        }
        /*
        public void Run_func(string[] argument)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            double[,] arrMultiD = Resh_env_cycle(Convert.ToDouble(argument[1]) * Math.Pow(80, 2) / (Math.Pow(140, 2) * Math.PI), Convert.ToDouble(argument[2]),
                Convert.ToDouble(argument[3]), Convert.ToDouble(argument[4]), Convert.ToDouble(argument[5]), Convert.ToDouble(argument[6]),
                Convert.ToDouble(argument[7]), Convert.ToDouble(argument[8]), Convert.ToDouble(argument[9]), Convert.ToDouble(argument[10]), Convert.ToDouble(argument[11]));
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            long ts = stopWatch.ElapsedMilliseconds;

            CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
            CloudTable TableData = tableClient.GetTableReference("TableData");
            CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");

            for (int i = 1; i < arrMultiD.GetLength(0); i++)
            {
                if (arrMultiD[i, 1] != 0)
                {
                    InsertToTableData(TableData, argument[0], i.ToString(), arrMultiD[i, 1].ToString(), arrMultiD[i, 2].ToString(), arrMultiD[i, 3].ToString(), arrMultiD[i, 4].ToString(),
                                      arrMultiD[i, 5].ToString(), arrMultiD[i, 6].ToString(), arrMultiD[i, 7].ToString(), arrMultiD[i, 8].ToString(), arrMultiD[i, 9].ToString(),
                                      arrMultiD[i, 10].ToString(), arrMultiD[i, 11].ToString(), arrMultiD[i, 12].ToString(), arrMultiD[i, 13].ToString(), arrMultiD[i, 14].ToString());
                }
            }
            InsertToTableSimulation(TableSimulation, argument[0], "0", ts.ToString());
        }
         */

        public override bool OnStart()
        {
            // Задайте максимальное число одновременных подключений 
            ServicePointManager.DefaultConnectionLimit = 12;

            // получаем строку подключения к аккаунту хранилища
            StorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // инициализация клиента очереди
            Client = StorageAccount.CreateCloudQueueClient();

            // создаем очередь
            Queue = Client.GetQueueReference("queuecrack");

            // создаем контейнер очереди
            Queue.CreateIfNotExists();

            CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();
            CloudTable TableData = tableClient.GetTableReference("TableData");
            CloudTable TableSimulation = tableClient.GetTableReference("TableSimulation");
            TableData.CreateIfNotExists();
            TableSimulation.CreateIfNotExists();
            return base.OnStart();
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

        ////////////////////////////////////////////////////////////

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


        ////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////End


    }
}
