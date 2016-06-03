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
        public const int sleep = 5000;
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
            //wypisanie do texboxa hostName'ów by pokazać że kod zadziała
            string text = "";
            foreach (var t in hostNames)
            {
               text += t;
               text += " => \n";
            }
            textBox9.Text = text;
        }
        //do testowania, czy nie zablokuje okna jakiś wątek
        private void random_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            textBox8.Text = rnd.Next().ToString();
        }
        // zadanie1.a użycie task
        private void tasks_Click(object sender, EventArgs e)
        {
            int n = Int32.Parse(textBox1.Text);
            int k = Int32.Parse(textBox2.Text);
            // osobny task, by nie zakłócać głównego wątku
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

                    Thread.Sleep(sleep);
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
            //kod nie czeka na zakończenie delegata, idzie dalej
            MethodAsyncIntDelegat a = new MethodAsyncIntDelegat(methodA);
            IAsyncResult metA = a.BeginInvoke(n, k, null, null);
            MethodAsyncIntDelegat b = new MethodAsyncIntDelegat(methodB);
            IAsyncResult metB = b.BeginInvoke(n, k, null, null);

            Thread.Sleep(sleep);

            //kod czeka na wyniki delegatów
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
            
        }
        // zadanie1.c metody async - await, kod nie czeka na koniec metody tylko idzie dalej
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

            await Task.Delay(sleep);
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
            //pobranie liczby z textboxa
            int totalSteps = (int)e.Argument;
            long a = 0, b = 1;
            //fibonacci 
            for (int j = 1; j <= totalSteps; j++)
            {              
                b += a; //pod zmienną b przypisujemy wyraz następny czyli a+b
                a = b - a; //pod zmienną a przypisujemy wartość zmiennej b
                Thread.Sleep(100);
                //wysłanie info do metody, a ona zaktualizuje progressbar
                (sender as BackgroundWorker).ReportProgress((int)(100 / totalSteps) * j, null);                
            }
            //wynik obliczeń
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
        public static void Compress(DirectoryInfo directorySelected)
        {
            //foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            directorySelected.GetFiles().AsParallel().ForAll(            
             fileToCompress =>
            {          
                using (FileStream originalFileStream = fileToCompress.OpenRead())
                {
                    if ((File.GetAttributes(fileToCompress.FullName) &
                       FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                    {
                        using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                        {
                            using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                               CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);

                            }
                        }
                        FileInfo info = new FileInfo(directorySelected.Parent.ToString() + "\\" + fileToCompress.Name + ".gz");                       
                        //Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                        //fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                    }

                }
            });
            //usunięcie zkompresowanych plików
            directorySelected.GetFiles().AsParallel()
                .Where(file => file.Extension != ".gz")
                .ForAll(                
            fileToCompress =>
            {
                fileToCompress.Delete();
            });
        }

        public static void Decompress(DirectoryInfo directorySelected)
        {
            directorySelected.GetFiles().AsParallel()
                .Where(file => file.Extension.Equals(".gz"))
                .ForAll(
             fileToDecompress =>
             {
                 using (FileStream originalFileStream = fileToDecompress.OpenRead())
                 {
                     string currentFileName = fileToDecompress.FullName;
                     string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                     using (FileStream decompressedFileStream = File.Create(newFileName))
                     {
                         using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                         {
                             decompressionStream.CopyTo(decompressedFileStream);
                             Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                         }
                     }
                 }                 
             });
            //usunięcie zdekompresowanych archiwów
            directorySelected.GetFiles().AsParallel()
                .Where(file => file.Extension.Equals(".gz"))
                .ForAll(
             fileToDecompress =>
             {
                 fileToDecompress.Delete();
             });
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
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
                    DirectoryInfo directorySelected = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                    Compress(directorySelected);
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
                DirectoryInfo directorySelected = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                Decompress(directorySelected);

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
                .Select(host => new { Name = host, Ip = Dns.GetHostAddresses(host) }) //funkcja pobiera tablicę ip
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
