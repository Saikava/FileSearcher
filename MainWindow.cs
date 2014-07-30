using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace FileSearcher
{
    public partial class MainWindow : Form
    {
        private Boolean m_closing;
        private DateTime StartTime;

        private delegate void FoundInfoSyncHandler(FoundInfoEventArgs e);
        private FoundInfoSyncHandler FoundInfo;

        private delegate void ThreadEndedSyncHandler(ThreadEndedEventArgs e);
        private ThreadEndedSyncHandler ThreadEnded;

        private delegate void StringSearchEndedHandler(EventArgs e);
        private StringSearchEndedHandler StringSearchEnded;

        private int count;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Загрузка конфигурации
            UserConfig.Load();

            Location = new Point(UserConfig.Data.LocationX, UserConfig.Data.LocationY);
            Size = new Size(UserConfig.Data.Width, UserConfig.Data.Height);
            WindowState = (FormWindowState)UserConfig.Data.WindowState;

            searchDirTextBox.Text = UserConfig.Data.SearchDir;
            fileNameTextBox.Text = UserConfig.Data.FileName;
            containingTextBox.Text = UserConfig.Data.ContainingText;
            asciiRadioButton.Checked = UserConfig.Data.AsciiChecked;
            unicodeRadioButton.Checked = UserConfig.Data.UnicodeChecked;

            // Подписка на делегаты
            FoundInfo += this_FoundInfo;
            ThreadEnded += this_ThreadEnded;
            StringSearchEnded += this_StringSearchEnded;

            // Подписка на события
            Searcher.FoundInfo += Searcher_FoundInfo;
            Searcher.ThreadEnded += Searcher_ThreadEnded;
            Searcher.StringSearchEnded += Searcher_StringSearchEnded;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_closing = true;
            Searcher.Stop();

            // Сохранение конфигурации
            if (WindowState == FormWindowState.Normal)
            {
                UserConfig.Data.LocationX = Location.X;
                UserConfig.Data.LocationY = Location.Y;
                UserConfig.Data.Width = Size.Width;
                UserConfig.Data.Height = Size.Height;
            }
            if (WindowState != FormWindowState.Minimized)
            {
                UserConfig.Data.WindowState = (Int32)WindowState;
            }

            UserConfig.Data.SearchDir = searchDirTextBox.Text;
            UserConfig.Data.FileName = fileNameTextBox.Text;
            UserConfig.Data.ContainingText = containingTextBox.Text;
            UserConfig.Data.AsciiChecked = asciiRadioButton.Checked;
            UserConfig.Data.UnicodeChecked = unicodeRadioButton.Checked;

            UserConfig.Save();
        }

        private void selectSearchDirButton_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog {SelectedPath = searchDirTextBox.Text};
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                searchDirTextBox.Text = dlg.SelectedPath;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            Searcher.Stop();
            timer1.Stop();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartTime = DateTime.Now;
            timer1.Start();

            treeView1.Nodes.Clear();

            // Получение параметров для потока поиска
            var fileNamesString = fileNameTextBox.Text;
            var fileNames = fileNamesString.Split(new Char[] { ';' });
            var validFileNames = new List<String>();
            foreach (var fileName in fileNames)
            {
                var trimmedFileName = fileName.Trim();
                if (trimmedFileName != "")
                {
                    validFileNames.Add(trimmedFileName);
                }
            }

            var encoding = asciiRadioButton.Checked ? Encoding.ASCII : Encoding.Unicode;

            var pars = new SearcherParams(searchDirTextBox.Text.Trim(),
                                                        validFileNames,
                                                        containingTextBox.Text.Trim(),
                                                        encoding);

            if (Searcher.Start(pars))
            {
                DisableButtons();
            }
            else
            {
                MessageBox.Show("The searcher is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Searcher_FoundInfo(FoundInfoEventArgs e)
        {
            if (!m_closing)
            {
                Invoke(FoundInfo, new object[] { e });
            }
        }

        private void this_FoundInfo(FoundInfoEventArgs e)
        {
            CreateResultsListItem(e.Info);
        }

        private void Searcher_ThreadEnded(ThreadEndedEventArgs e)
        {
            if (!m_closing)
            {
                Invoke(ThreadEnded, new object[] { e });
            }
        }

        private void this_ThreadEnded(ThreadEndedEventArgs e)
        {
            timer1.Stop();
            EnableButtons();

            if (!e.Success)
            {
                MessageBox.Show(e.ErrorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Searcher_StringSearchEnded(EventArgs e)
        {
            if (!m_closing)
            {
                Invoke(StringSearchEnded, new object[] { e });
            }
        }

        private void this_StringSearchEnded(EventArgs e)
        {
            labelCurrentFile.Text = "Поиск файла...";
        }

        private void EnableButtons()
        {
            searchDirTextBox.Enabled = true;
            selectSearchDirButton.Enabled = true;
            fileNameTextBox.Enabled = true;
            stopButton.Enabled = false;
            startButton.Enabled = true;
        }

        private void DisableButtons()
        {
            searchDirTextBox.Enabled = false;
            selectSearchDirButton.Enabled = false;
            fileNameTextBox.Enabled = false;
            containingTextBox.Enabled = false;
            asciiRadioButton.Enabled = false;
            unicodeRadioButton.Enabled = false;
            stopButton.Enabled = true;
            startButton.Enabled = false;
        }

        public static String GetBytesStringKB(Int64 bytesCount)
        {
            var bytesShow = (bytesCount + 1023) >> 10;
            var bytesString = GetPointString(bytesShow) + " KB";
            return bytesString;
        }

        public static String GetPointString(Int64 value)
        {
            var pointString = value.ToString(CultureInfo.InvariantCulture);

            var i = 3;
            while (pointString.Length > i)
            {
                pointString = pointString.Substring(0, pointString.Length - i) + "." + pointString.Substring(pointString.Length - i, i);
                i += 4;
            }

            return pointString;
        }

        private void CreateResultsListItem(FileSystemInfo info)
        {
            // Создание нового айтема
            var lvi = new ListViewItem {Text = info.FullName};

            var lvsi = new ListViewItem.ListViewSubItem();
            if (info is FileInfo)
            {
                lvsi.Text = GetBytesStringKB(((FileInfo)info).Length);
            }
            else
            {
                lvsi.Text = "";
            }
            lvi.SubItems.Add(lvsi);

            lvsi = new ListViewItem.ListViewSubItem
                   {
                       Text =
                           info.LastWriteTime.ToShortDateString() + " " +
                           info.LastWriteTime.ToShortTimeString()
                   };
            lvi.SubItems.Add(lvsi);

            lvi.ToolTipText = info.FullName;

            // Добавление нового айтема в список

            count++;
            labelNumber.Text = count.ToString(CultureInfo.InvariantCulture);

            labelCurrentFile.Text = info.Name;
            if (info is FileInfo)
            {
                var fileInfo = (FileInfo)info;
                var filePath = fileInfo.FullName;
                AddNode(filePath);
            }
        }

        private void AddNode(string path)
        {
            TreeNode lastNode = null;
            string subPathAgg = string.Empty;
                foreach (var subPath in path.Split('\\'))
                {
                    subPathAgg += subPath + '\\';
                    var nodes = treeView1.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = treeView1.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelTime.Text = (DateTime.Now - StartTime).ToString();
        }
    }
}
