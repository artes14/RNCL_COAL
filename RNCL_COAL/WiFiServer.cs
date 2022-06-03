using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace RNCL_COAL
{
    class WiFiServer
    {
        Thread DownloadThread;
        Thread UploadThread;
        string base_uri = "http://192.168.0.150/";
        Form1 mainForm;
        Label lbl_progress;
        ProgressBar progressBar;

        public WiFiServer()
        {
            lbl_progress = mainForm.lbl_progress;
            progressBar = mainForm.progressBar1;
        }
        public void startDownload(string filename, string foldername)
        {
            DownloadThread = new Thread(() =>
            {
                WebClient client = new WebClient();
                string uri = base_uri + "download";
                try
                {
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(uri + "?download=" + foldername + "/" + filename), Application.StartupPath + @"\" + foldername + @"\" + filename);

                }
                catch
                {

                }
            });
            DownloadThread.Start();
        }
        public void startUpload(string filename, string foldername)
        {
            UploadThread = new Thread(() => {
                WebClient client = new WebClient();
                string uri = base_uri+"fupload";
                try
                {
                    client.DownloadData(base_uri + "u" + foldername);
                    client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged);
                    client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
                    client.Headers.Add("filename", System.IO.Path.GetFileName(filename));
                    client.UploadFileAsync(new Uri(uri), filename);
                }
                catch
                {

                }
                
            });
            UploadThread.Start();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            mainForm.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                if(lbl_progress!=null || progressBar != null)
                {
                    lbl_progress.Text = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                    progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                }
            });
        }
        void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            mainForm.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesSent.ToString());
                double totalBytes = double.Parse(e.TotalBytesToSend.ToString());
                double percentage = bytesIn / totalBytes * 100;
                if (lbl_progress != null || progressBar != null)
                {
                    lbl_progress.Text = "Uploaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                    progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                }
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (lbl_progress != null)
                mainForm.BeginInvoke((MethodInvoker)delegate {
                lbl_progress.Text = "Completed";
            });
        }
        void client_UploadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (lbl_progress != null)
                mainForm.BeginInvoke((MethodInvoker)delegate {
                lbl_progress.Text = "Completed";
            });
        }


    }
}
