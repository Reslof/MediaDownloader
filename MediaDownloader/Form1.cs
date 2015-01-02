using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void urlBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void saveLocationBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioMP3Button_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void videoOrigButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void outputBox_TextChanged(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //sets download path read only
            saveLocationBox.ReadOnly = true;
            urlTextBox.ReadOnly = true;

            //gets radio option
            int option = 0;

            if (radioMP3Button.Checked)
            {
                option = 0;
            }
            else if (videoOrigButton.Checked)
            {
                option = 1;
            }
            //room for more options

            string url = urlTextBox.Text;
            if (System.Uri.IsWellFormedUriString(url, System.UriKind.Absolute))
            {
                outputBox.Clear();
                LaunchYoutubedl(url, option);
                urlTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Invalid URL. Please enter a valid URL. For more help, click the \"Help\" button.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {   
            //sets download location to Desktop as a default
            saveLocationBox.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }
        void LaunchYoutubedl(string url, int option)
        {
            string downloadLocation = saveLocationBox.Text;
            string formatOptions = "";
            var proc = new Process();
            if(option == 0){

                formatOptions = " -x --audio-quality 0 --prefer-ffmpeg --audio-format mp3";
            }
            else if (option == 1)
            {
                formatOptions = "";
            }
           
            // set up output redirection and process parameters
            proc.StartInfo.WorkingDirectory = downloadLocation;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "youtube-dl.exe";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = " -o \"%(title)s.%(ext)s\"" +
                                  formatOptions +
                                  " --restrict-filenames " +
                                  url;

            // Set the data received handlers
            proc.ErrorDataReceived += proc_DataReceived;
            proc.OutputDataReceived += proc_DataReceived;

            // Configure the process exited event
            proc.Exited += new EventHandler(ProcExited);

            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();

        }

        delegate void AppendTextDelegate(string text);

        // Thread-safe method of appending text to the console box
        private void AppendText(string text)
        {
            // Use a delegate if called from a different thread,
            // else just append the text directly
            if (this.outputBox.InvokeRequired)
            {
                // Application crashes when this line is executed
                outputBox.Invoke(new AppendTextDelegate(this.AppendText), new object[] { text + Environment.NewLine }); //adds a newline at the end
            }
            else
            {
                this.outputBox.AppendText(text);
                
            }
        }
        
        // Handle the date received by the console process
        void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.AppendText(e.Data);

            }
        }
        /// <summary>
        /// Actions to take when console process completes
        /// </summary>
        private void ProcExited(object sender, System.EventArgs e)
        {
            Process proc = (Process)sender;

            // Wait a short while to allow all console output to be processed and appended
            // before appending the success/fail message.
            Thread.Sleep(40);

            if (proc.ExitCode == 0)
            {
                this.AppendText(Environment.NewLine + "Success.");
                MessageBox.Show("Operation completed successfully.");
                
            }
            else
            {
                this.AppendText(Environment.NewLine + "Failed.");
                MessageBox.Show("Operation failed.");

            }    
            
            //thread safe way to update the UI
            this.InvokeEx(f => Application.UseWaitCursor = false);
            this.InvokeEx(f => urlTextBox.ReadOnly = false);
            this.InvokeEx(f => saveLocationBox.ReadOnly = false);

            proc.Close();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "If you are having trouble downloading audio from a specific site, be sure that the website is supported by youtube-dl." +
            "\nSupported sites: https://rg3.github.io/youtube-dl/supportedsites.html" +
            "\n\nIcons made by Freepik. Flaticon Creative Commons BY 3.0",
            "Help",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1
            );
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            saveLocationBox.Clear();
            saveLocationBox.AppendText(fbd.SelectedPath);
            saveLocationBox.ReadOnly = true;
        }
    }
}
//thread safe way of accessing UI elements. Thanks StackOverflow
public static class ISynchronizeInvokeExtensions
{
    public static void InvokeEx<T>(this T @this, Action<T> action) where T : ISynchronizeInvoke
    {
        if (@this.InvokeRequired)
        {
            @this.Invoke(action, new object[] { @this });
        }
        else
        {
            action(@this);
        }
    }
}