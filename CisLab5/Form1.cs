using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CisLab5
{
    public partial class Form1 : Form
    {
        string[] hostNames = { "www.microsoft.com",
            "www.apple.com", "www.google.com",
            "www.ibm.com", "cisco.netacad.net",
            "www.oracle.com", "www.nokia.com",
            "www.hp.com", "www.dell.com",
            "www.samsung.com", "www.toshiba.com",
            "www.siemens.com", "www.amazon.com",
            "www.sony.com", "www.canon.com", 
            "www.acer.com", "www.motorola.com", "www.alcatel-lucent.com"
            };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string text = "";
            foreach (var t in hostNames)
            {
               text += t;
               text += " => \n";
            }
            textBox9.Text = text;
        }

        private void random_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            textBox8.Text = rnd.Next().ToString();
        }
        // zadanie1.a task
        private void tasks_Click(object sender, EventArgs e)
        {
            int n = Int32.Parse(textBox1.Text);
            int k = Int32.Parse(textBox2.Text);

            Task mainTask = Task.Run(
                () =>
                {
                    Task<int> taskA = Task.Run(
                        () =>
                        {
                            int licznik = 1;
                            int warunek = n - k + 1;

                            for (int i = n; i >= warunek; i--)
                            {
                                licznik = licznik * i;
                            }


                            return licznik;
                        }
                    );

                    Task<int> taskB = Task.Run(
                        () =>
                        {
                            int mianownik = 1;

                            for (int i = 1; i <= k; i++)
                            {
                                mianownik = mianownik * i;
                            }

                            return mianownik;
                        }
                    );

                    Task<int>[] work = new Task<int>[] { taskA, taskB };

                    Task<double> finalTask = Task.Factory.ContinueWhenAll(work,
                        (tasks) =>
                        {
                            Task<int>[] task = (Task<int>[])tasks;
                            double wynik = task[0].Result / task[1].Result;
                            
                            return wynik;
                        }
                    );                    
                    finalTask.Wait();

                    Thread.Sleep(10000);
                    if (textBox3.InvokeRequired)
                    {
                        textBox3.BeginInvoke((Action)(() => textBox3.Text = finalTask.Result.ToString()));
                    }
                                               
                }
                
            );
           

        }
        // zadanie1.b delegaci
        private delegate double MethodAsyncDelegat(int n, int k);
        private delegate int MethodAsyncIntDelegat(int n, int k);

        private int methodA(int n, int k)
        {
            int licznik = 1;
            int warunek = n - k + 1;

            for (int i = n; i >= warunek; i--)
            {
                licznik = licznik * i;
            }

            return licznik;
        }

        private int methodB(int n, int k)
        {
            int mianownik = 1;

            for (int i = 1; i <= k; i++)
            {
                mianownik = mianownik * i;
            }

            return mianownik;
        }

        private double MethodAsync(int n, int k)
        {
            MethodAsyncIntDelegat a = new MethodAsyncIntDelegat(methodA);
            IAsyncResult metA = a.BeginInvoke(n, k, null, null);
            MethodAsyncIntDelegat b = new MethodAsyncIntDelegat(methodB);
            IAsyncResult metB = b.BeginInvoke(n, k, null, null);

            Thread.Sleep(10000);

            int resultA = a.EndInvoke(metA);
            int resultB = b.EndInvoke(metB);

            double result = resultA / resultB;

            if (textBox4.InvokeRequired)
            {
                textBox4.BeginInvoke((Action)(() => textBox4.Text = result.ToString()));
            }

            return result;
            
        }

        private void Delegates_Click(object sender, EventArgs e)
        {
            int n = Int32.Parse(textBox1.Text);
            int k = Int32.Parse(textBox2.Text);

            MethodAsyncDelegat d = new MethodAsyncDelegat(MethodAsync);
            IAsyncResult rezult = d.BeginInvoke(n, k, null, null);
            //double result = d.EndInvoke(rezult);
            //textBox4.Text = result.ToString();

        }
        // zadanie1.c async
        async Task<int> taskA(int n, int k)
        {
            int licznik = 1;
            int warunek = n - k + 1;

            for (int i = n; i >= warunek; i--)
            {
                licznik = licznik * i;
            }
            
            return licznik;
        }

        async Task<int> taskB(int n, int k)
        {
            int mianownik = 1;

            for (int i = 1; i <= k; i++)
            {
                mianownik = mianownik * i;
            }

            return mianownik;
        }

        async Task finalTask(int n, int k)
        {
            double wynik = 10;
            int licznik = await taskA(n, k);
            int mianownik= await taskB(n, k);

            await Task.Delay(10000);
            await Task.Run(() => Thread.Sleep(3000));

            wynik = licznik / mianownik;
            textBox5.Text = wynik.ToString();
          
        }
                         
        private async void async_Click(object sender, EventArgs e)
        {
            int n = Int32.Parse(textBox1.Text);
            int k = Int32.Parse(textBox2.Text);

           await finalTask(n,k);
          
        }
        //zadanie2        
        private void fibonacci_Click(object sender, EventArgs e)
        {
            int n = Int32.Parse(textBox6.Text);

            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Value = 0;            
            backgroundWorker1.RunWorkerAsync(n);
        }
        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {            
            int totalSteps = (int)e.Argument;
            long a = 0, b = 1;

            for (int j = 1; j <= totalSteps; j++)
            {              
                b += a; //pod zmienną b przypisujemy wyraz następny czyli a+b
                a = b - a; //pod zmienną a przypisujemy wartość zmiennej b
                Thread.Sleep(100);
                (sender as BackgroundWorker).ReportProgress((int)(100 / totalSteps) * j, null);                
            }

            e.Result = a;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox7.Text = e.Result.ToString();
        }
        //zadanie3
        public static void CompressStringToFile(string fileName, string value)
        {
            // A.
            // Write string to temporary file.
            string temp = Path.GetTempFileName();
            File.WriteAllText(temp, value);

            // B.
            // Read file into byte array buffer.
            byte[] b;
            using (FileStream f = new FileStream(temp, FileMode.Open))
            {
                b = new byte[f.Length];
                f.Read(b, 0, (int)f.Length);
            }

            // C.
            // Use GZipStream to write compressed bytes to target file.
            using (FileStream f2 = new FileStream(fileName, FileMode.Create))
            using (GZipStream gz = new GZipStream(f2, CompressionMode.Compress, false))
            {
                gz.Write(b, 0, b.Length);
            }
        }

        private void compress_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            // OK button was pressed.
            if (result == DialogResult.OK)
            {
                try
                {
                    
                    string anyString = File.ReadAllText(result.ToString());
                   
                    CompressStringToFile("new.gz", anyString);
                }
                catch
                {
                    // Could not compress.
                }

            }
            // Cancel button was pressed.
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        private void decompress_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            // OK button was pressed.
            if (result == DialogResult.OK)
            {


            }
            // Cancel button was pressed.
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }
        //zadanie4
        private void resolve_Click(object sender, EventArgs e)
        {
            textBox9.Text = "";
            var d = hostNames.AsParallel()
                .Select(host => new { Name = host, Ip = Dns.GetHostAddresses(host) })
                .Select(host => new { name = host.Name + " => " + host.Ip.First().ToString() });
                //.ForAll(host => textBox9.Text += (host.Name + " => " + host.Ip.First().ToString() + "\n"));
                //.ForAll(host => Console.WriteLine(host.Name + " => " + host.Ip.First().ToString()));

            foreach(var f in d)
            {
                textBox9.Text += (f.name + "\t");                
            }

        }
       

        
    }
}
