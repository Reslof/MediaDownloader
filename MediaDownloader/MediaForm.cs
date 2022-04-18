using DesktopToast;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaDownloader
{
    public partial class MediaForm : Form
    {
        private int processID;

        public MediaForm()
        {
            EasyLogger.BackupLogs(EasyLogger.LogFile, 7);
            EasyLogger.AddListener(EasyLogger.LogFile);

            InitializeComponent();

            try
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe"))
                {
                    DialogResult dialogResult = MessageBox.Show("Would you like me to download ffmpeg.exe for you?" + Environment.NewLine + Environment.NewLine + "This may take a moment depending on how fast your download speed is.", "FFMPEG", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip", AppDomain.CurrentDomain.BaseDirectory + "ffmpeg-release-essentials.zip");
                        }

                        using (ZipArchive archive = ZipFile.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "ffmpeg-release-essentials.zip"))
                        {
                            archive.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory);
                        }
                        string[] directories = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory);
                        foreach (string directory in directories)
                        {
                            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

                            foreach (string file in files)
                            {
                                if (file.EndsWith("ffmpeg.exe"))
                                {
                                    File.Move(file, AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe");
                                }
                                else
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                        DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        try
                        {
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "ffmpeg-release-essentials.zip");
                        }
                        catch (Exception ex)
                        {
                            EasyLogger.Error(ex);
                            AppendText(ex.Message);
                        }
                    }
                }

                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "youtube-dl.exe"))
                {
                    DialogResult dialogResult = MessageBox.Show("Would you like me to download youtube-dl.exe for you?", "YOUTUBE-DL", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile("https://yt-dl.org/downloads/latest/youtube-dl.exe", AppDomain.CurrentDomain.BaseDirectory + "youtube-dl.exe");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }

            urlTextBox.KeyDown += UrlTextBox_KeyDown;
            FileNameBox.KeyDown += UrlTextBox_KeyDown;

            Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LaunchYoutubedlUpdate();
        }

        private void LaunchYoutubedlUpdate()
        {
            try
            {
                FileNameBox.Enabled = false;
                fileBrowserButton.Enabled = false;
                bulkTextBox.Enabled = false;
                urlTextBox.Enabled = false;
                saveLocationBox.Enabled = false;
                BrowseButton.Enabled = false;
                videoOrigButton.Enabled = false;
                radioMP3Button.Enabled = false;
                DownloadButton.Enabled = false;
                ExitButton.Text = "Cancel";

                Process proc = new Process();
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.EnableRaisingEvents = true;
                proc.StartInfo.CreateNoWindow = true;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = "youtube-dl.exe";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                proc.StartInfo.Arguments = "--update --ignore-errors";

                // Set the data received handlers
                proc.ErrorDataReceived += proc_DataReceived;
                proc.OutputDataReceived += proc_DataReceived;

                // Configure the process exited event
                proc.Exited += new EventHandler(ProcExited);

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private void FileNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DownloadItem();
            }
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DownloadItem();
            }
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            DownloadItem();
        }

        private void DownloadItem()
        {
            if (!string.IsNullOrEmpty(urlTextBox.Text))
            {
                Download(urlTextBox.Text);
            }
        }

        private void Download(string url)
        {
            try
            {
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

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    FileNameBox.Enabled = false;
                    fileBrowserButton.Enabled = false;
                    bulkTextBox.Enabled = false;
                    urlTextBox.Enabled = false;
                    saveLocationBox.Enabled = false;
                    BrowseButton.Enabled = false;
                    videoOrigButton.Enabled = false;
                    radioMP3Button.Enabled = false;
                    DownloadButton.Enabled = false;
                    ExitButton.Text = "Cancel";

                    outputBox.Clear();
                    LaunchYoutubedl(url, option);
                    urlTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Please enter a valid URL", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (ExitButton.Text == "Exit")
            {
                Close();
            }
            else
            {
                foreach (Process p in Process.GetProcessesByName("youtube-dl"))
                {
                    try
                    {
                        if (p.Id == processID)
                        {
                            p.Kill();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadDirectory) && !Directory.Exists(Properties.Settings.Default.DownloadDirectory))
                {
                    saveLocationBox.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                }
                else
                {
                    saveLocationBox.AppendText(Properties.Settings.Default.DownloadDirectory);
                }

                saveLocationBox.TextChanged += SaveLocationBox_TextChanged;
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private void SaveLocationBox_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(saveLocationBox.Text))
            {
                Properties.Settings.Default.DownloadDirectory = saveLocationBox.Text;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }
        }

        private void LaunchYoutubedl(string url, int option, string name = "")
        {
            try
            {
                saveLocationBox.Clear();

                string downloadLocation = saveLocationBox.Text;

                if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadDirectory) && !Directory.Exists(Properties.Settings.Default.DownloadDirectory))
                {
                    saveLocationBox.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                }
                else
                {
                    saveLocationBox.AppendText(Properties.Settings.Default.DownloadDirectory);
                }

                downloadLocation = saveLocationBox.Text;

                string formatOptions = "";
                Process proc = new Process();
                if (option == 0)
                {
                    formatOptions = " --extract-audio --prefer-ffmpeg --audio-format mp3";
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

                if (!string.IsNullOrEmpty(name))
                {
                    proc.StartInfo.Arguments = string.Format(" -o \"{0}.%(ext)s\"" +
                                              formatOptions +
                                              " --restrict-filenames " +
                                              url + " --ignore-errors", name.Trim());
                }
                else if (!string.IsNullOrEmpty(FileNameBox.Text))
                {
                    proc.StartInfo.Arguments = string.Format(" -o \"{0}.%(ext)s\"" +
                                              formatOptions +
                                              " --restrict-filenames " +
                                              url + " --ignore-errors", FileNameBox.Text);
                }
                else
                {
                    proc.StartInfo.Arguments = " -o \"%(title)s.%(ext)s\"" +
                                          formatOptions +
                                          " --restrict-filenames " +
                                          url + " --ignore-errors";
                }

                // Set the data received handlers
                proc.ErrorDataReceived += proc_DataReceived;
                proc.OutputDataReceived += proc_DataReceived;
                // Configure the process exited event
                proc.Exited += new EventHandler(ProcExited);

                proc.Start();

                processID = proc.Id;

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private delegate void AppendTextDelegate(string text);

        // Thread-safe method of appending text to the Trace box
        private void AppendText(string text)
        {
            try
            {
                // Use a delegate if called from a different thread,
                // else just append the text directly
                if (outputBox.InvokeRequired)
                {
                    // Application crashes when this line is executed
                    outputBox.Invoke(new AppendTextDelegate(AppendText), new object[] { text + Environment.NewLine }); //adds a newline at the end
                }
                else
                {
                    outputBox.AppendText(text + Environment.NewLine);

                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        // Handle the date received by the Trace process
        private void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (e.Data != null)
                {
                    AppendText(e.Data);

                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }
        /// <summary>
        /// Actions to take when Trace process completes
        /// </summary>
        private void ProcExited(object sender, EventArgs e)
        {
            try
            {
                Process proc = (Process)sender;

                // Wait a short while to allow all Trace output to be processed and appended
                // before appending the success/fail message.
                Thread.Sleep(40);

                if (proc.ExitCode == 0)
                {
                    AppendText("Operation completed successfully!");

                    ToastNotification.ShowToast(10, ToastAudio.SMS, "Media Downloader", "Operation completed successfully!");
                }
                else
                {
                    AppendText("Operation failed!");

                    ToastNotification.ShowToast(10, ToastAudio.SMS, "Media Downloader", "Operation failed!");

                }

                //thread safe way to update the UI
                this.InvokeEx(f => FileNameBox.Enabled = true);
                this.InvokeEx(f => bulkTextBox.Enabled = true);
                this.InvokeEx(f => fileBrowserButton.Enabled = true);
                this.InvokeEx(f => urlTextBox.Enabled = true);
                this.InvokeEx(f => saveLocationBox.Enabled = true);
                this.InvokeEx(f => BrowseButton.Enabled = true);
                this.InvokeEx(f => videoOrigButton.Enabled = true);
                this.InvokeEx(f => radioMP3Button.Enabled = true);
                this.InvokeEx(f => DownloadButton.Enabled = true);
                this.InvokeEx(f => ExitButton.Enabled = true);
                this.InvokeEx(f => ExitButton.Text = "Exit");

                this.InvokeEx(f => urlTextBox.Focus());
                this.InvokeEx(f => urlTextBox.Select());

                proc.Close();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            saveLocationBox.Clear();
            saveLocationBox.AppendText(fbd.SelectedPath);
        }

        private void fileBrowserButton_Click(object sender, EventArgs e)
        {
            try
            {
                FileNameBox.Enabled = false;
                fileBrowserButton.Enabled = false;
                bulkTextBox.Enabled = false;
                urlTextBox.Enabled = false;
                saveLocationBox.Enabled = false;
                BrowseButton.Enabled = false;
                videoOrigButton.Enabled = false;
                radioMP3Button.Enabled = false;
                DownloadButton.Enabled = false;
                ExitButton.Text = "Cancel";

                outputBox.Clear();
                urlTextBox.Clear();

                int option = 0;

                if (radioMP3Button.Checked)
                {
                    option = 0;
                }
                else if (videoOrigButton.Checked)
                {
                    option = 1;
                }

                if (Properties.Settings.Default.FirstRun)
                {
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.Reload();

                    MessageBox.Show("Make sure there is a url for each line along with the name of each video separated by a semi-colon surrounded by quotation marks. Make sure that the name doesn't have a semi-colon in it. There should only be one semi-colon per line." + Environment.NewLine + Environment.NewLine + "Example:" + Environment.NewLine + "https://domain/video1.mp4; " + "\"" + "The neat-o guy!" + "\"" + Environment.NewLine + "https://domain/video2.mp4; " + "\"" + "The other neat-o guy!" + "\"", "Bulk Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    fileDialog.RestoreDirectory = true;
                    fileDialog.Title = "Bulk Download";
                    fileDialog.DefaultExt = "txt";
                    fileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    fileDialog.CheckFileExists = true;
                    fileDialog.CheckPathExists = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _ = DownloadAsync(fileDialog.FileName, option);
                    }
                    else
                    {
                        FileNameBox.Enabled = true;
                        fileBrowserButton.Enabled = true;
                        bulkTextBox.Enabled = true;
                        urlTextBox.Enabled = true;
                        saveLocationBox.Enabled = true;
                        BrowseButton.Enabled = true;
                        videoOrigButton.Enabled = true;
                        radioMP3Button.Enabled = true;
                        DownloadButton.Enabled = true;
                        ExitButton.Text = "Exit";

                        outputBox.Clear();
                        urlTextBox.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);
            }
        }

        private async Task<int> DownloadAsync(string filename, int option)
        {
            try
            {
                TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

                bulkTextBox.Text = filename;
                string[] lines = File.ReadAllLines(filename, Encoding.UTF8);

                foreach (string line in lines)
                {
                    try
                    {
                        string[] video = line.Split(';');
                        try
                        {
                            Uri uri = new Uri(video[0]);
                            video[0] = uri.AbsoluteUri;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            continue;
                        }
                        outputBox.Text += Environment.NewLine + "Downloading: " + video[0] + "; " + CleanFileName(video[1]) + Environment.NewLine;

                        saveLocationBox.Clear();

                        string downloadLocation = saveLocationBox.Text;

                        if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadDirectory) && !Directory.Exists(Properties.Settings.Default.DownloadDirectory))
                        {
                            saveLocationBox.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                        }
                        else
                        {
                            saveLocationBox.AppendText(Properties.Settings.Default.DownloadDirectory);
                        }

                        downloadLocation = saveLocationBox.Text;

                        string formatOptions = "";
                        Process proc = new Process();
                        if (option == 0)
                        {
                            formatOptions = " --extract-audio --prefer-ffmpeg --audio-format mp3";
                        }
                        else if (option == 1)
                        {
                            formatOptions = "";
                        }

                        proc.StartInfo.WorkingDirectory = downloadLocation;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.EnableRaisingEvents = true;
                        proc.StartInfo.CreateNoWindow = true;

                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.FileName = "youtube-dl.exe";
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        proc.StartInfo.Arguments = string.Format(" -o \"{0}.%(ext)s\"" +
                                                      formatOptions +
                                                      " --restrict-filenames " +
                                                      video[0] + " --ignore-errors", CleanFileName(video[1]).Trim());

                        proc.ErrorDataReceived += proc_DataReceived;
                        proc.OutputDataReceived += proc_DataReceived;

                        proc.Start();

                        processID = proc.Id;

                        proc.BeginErrorReadLine();
                        proc.BeginOutputReadLine();

                        await ISynchronizeInvokeExtensions.WaitForExitAsync(proc);
                    }
                    catch
                    {
                        MessageBox.Show("Make sure that there is a url for each line along with the name of each video after the video separated by a semi-colon surrounded by quotation marks. Make sure that the name doesn't have a semi-colon in it. There should only be one semi-colon per line." + Environment.NewLine + Environment.NewLine + "Example:" + Environment.NewLine + "https://domain/video1.mp4, " + "\"" + "The neat-o guy!" + "\"" + Environment.NewLine + "https://domain/video2.mp4, " + "\"" + "The other neat-o guy!" + "\"", "Bulk Download", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        continue;
                    }
                }

                AppendText("Operation completed...");

                ToastNotification.ShowToast(10, ToastAudio.SMS, "Media Downloader", "Operation completed...");

                //thread safe way to update the UI
                this.InvokeEx(f => FileNameBox.Enabled = true);
                this.InvokeEx(f => bulkTextBox.Enabled = true);
                this.InvokeEx(f => bulkTextBox.Clear());
                this.InvokeEx(f => fileBrowserButton.Enabled = true);
                this.InvokeEx(f => urlTextBox.Enabled = true);
                this.InvokeEx(f => saveLocationBox.Enabled = true);
                this.InvokeEx(f => BrowseButton.Enabled = true);
                this.InvokeEx(f => videoOrigButton.Enabled = true);
                this.InvokeEx(f => radioMP3Button.Enabled = true);
                this.InvokeEx(f => DownloadButton.Enabled = true);
                this.InvokeEx(f => ExitButton.Enabled = true);
                this.InvokeEx(f => ExitButton.Text = "Exit");

                this.InvokeEx(f => urlTextBox.Focus());
                this.InvokeEx(f => urlTextBox.Select());

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                AppendText(ex.Message);

                return 0;
            }
        }

        private static string CleanFileName(string fileName)
        {
            try
            {
                return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
                return null;
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Make sure there is a url for each line along with the name of each video separated by a semi-colon surrounded by quotation marks. Make sure that the name doesn't have a semi-colon in it. There should only be one semi-colon per line." + Environment.NewLine + Environment.NewLine + "Example:" + Environment.NewLine + "https://domain/video1.mp4; " + "\"" + "The neat-o guy!" + "\"" + Environment.NewLine + "https://domain/video2.mp4; " + "\"" + "The other neat-o guy!" + "\"", "Bulk Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

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

    public static Task WaitForExitAsync(this Process process,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (process.HasExited)
        {
            return Task.CompletedTask;
        }

        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) => tcs.TrySetResult(null);
        if (cancellationToken != default(CancellationToken))
        {
            cancellationToken.Register(() => tcs.SetCanceled());
        }

        return process.HasExited ? Task.CompletedTask : tcs.Task;
    }
}