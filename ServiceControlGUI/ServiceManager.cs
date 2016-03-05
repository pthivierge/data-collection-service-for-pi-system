using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using WSR.Core.Helpers;

namespace WSR.Settings.GUI
{
    public partial class ServiceManager : Form
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (ServiceManager));
        private XmlSettingsManager _currentSetting;

        public ServiceManager()
        {
            InitializeComponent();
        }

        private void InitTreeView()
        {
            treeView1.Select();

            object setting;
            var root = new TreeNode("Settings");
            treeView1.Nodes.Add(root);

            string appSettingsPath = Settings.Default.SettingsPath;
            string settingsCommentsPath = Settings.Default.SettingsCommentsPath;

            if (!Path.IsPathRooted(appSettingsPath))
                appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appSettingsPath);

            if (!Path.IsPathRooted(settingsCommentsPath))
                settingsCommentsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsCommentsPath);


            _logger.Debug("will try to load service settings file: " + appSettingsPath);
            _logger.Debug("will try to load comments file: " + settingsCommentsPath);

            // here is the place you have to add your setting files
            setting = new XmlSettingsManager(typeof (General).FullName, appSettingsPath, settingsCommentsPath);
            root.Nodes.Add(new TreeNode("General Settings") {Tag = setting});

            setting = new XmlSettingsManager(typeof (Advanced).FullName, appSettingsPath, settingsCommentsPath);
            root.Nodes.Add(new TreeNode("Advanced Settings") {Tag = setting});

            //continue to add your settings categories here...

            // end of add settings files

            treeView1.ExpandAll();
            treeView1.SelectedNode = root.FirstNode;

            treeView1_NodeMouseClick(this, new TreeNodeMouseClickEventArgs(root.FirstNode, MouseButtons.Right, 1, 0, 0));
        }

        private void SettingsManager_Load(object sender, EventArgs e)
        {
            try
            {
                _logger.Debug("Application Started.");
                serviceController1.ServiceName = Settings.Default.ServiceName;
                serviceController1.MachineName = System.Environment.MachineName;
                Init();
                timer1.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceController1.ServiceName))
                    serviceController1.ServiceName = Settings.Default.ServiceName;

                serviceController1.Refresh();
                
                lblServiceName.Text = serviceController1.ServiceName;
                lblServiceState.Text = serviceController1.Status.ToString();

                switch (lblServiceState.Text)
                {
                    case "Running":
                        btnServiceAction.Text = "Stop";
                        break;
                    case "Stopped":
                        btnServiceAction.Text = "Start";
                        break;
                }

                btnServiceAction.Enabled = true;
            }
            catch (Exception ex)
            {
                lblServiceState.Text = "Error while looking for service.";
                btnServiceAction.Enabled = false;
                //_logger.Error(ex); // this would fill up logs... 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Save();
            toolStripStatusLabel1.Text = "Settings saved at " + DateTime.Now;
        }

        private void Save()
        {
            ((XmlSettingsManager) propertyGrid1.SelectedObject).SaveSettings();
        }


        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                object settings = e.Node.Tag;

                propertyGrid1.SelectedObject = settings;
                _currentSetting = (XmlSettingsManager) propertyGrid1.SelectedObject;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (btnServiceAction.Text)
            {
                case "Start":
                    if (serviceController1.Status != ServiceControllerStatus.StartPending &&
                        serviceController1.Status != ServiceControllerStatus.Running)
                        serviceController1.Start();
                    break;
                case "Stop":
                    if (serviceController1.CanStop)
                        serviceController1.Stop();
                    break;
            }
        }

        private void Init()
        {
            _logger.Debug("Initializing application.");
            InitTreeView();

            // ajoute un tooltip au bouton save
            // Create the ToolTip and associate with the Form container.
            var toolTip1 = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // Set up the delays for the ToolTip.
            // Force the ToolTip text to be displayed whether or not the form is active.

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(btnSave, "Saves the selected task.");

            // init notification icon properties
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            //Shows the info icon so the user doesn't think there is an error.
            notifyIcon1.BalloonTipText = "FDS Configurator is minimized in system tray...";
            notifyIcon1.BalloonTipTitle = "FDS Service Configurator";
            // this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon"))); //The tray icon to use
            notifyIcon1.Text = "FDS Service Configurator";


            _logger.Debug("Initialization completed.");
        }

        private void openContainingFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FileAppender rootAppender =
                    ((Hierarchy) LogManager.GetRepository()).Root.Appenders.OfType<FileAppender>().FirstOrDefault();
                string GUILogFile = rootAppender != null ? rootAppender.File : string.Empty;
                string serviceLogFile = Path.Combine(Path.GetDirectoryName(GUILogFile),
                    Settings.Default.ServiceLogFileNameForLogViewer);

                if (!string.IsNullOrEmpty(GUILogFile))
                {
                    // Prepare the process to run
                    var processStartInfo = new ProcessStartInfo
                    {
                        Arguments = serviceLogFile.PutIntoQuotes() + " " + GUILogFile.PutIntoQuotes(),
                        FileName = Path.Combine(Directory.GetCurrentDirectory(), Settings.Default.LogViewerExe),
                        WindowStyle = ProcessWindowStyle.Normal,
                        CreateNoWindow = false
                    };

                    var process = new Process {StartInfo = processStartInfo};

                    // Runs the log viewer application
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + "\n See log file for more informations", "error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex);
            }
        }

        private void ServiceManager_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                ShowInTaskbar = false;
            }
            else if (FormWindowState.Normal == WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = new AboutBox1();
            box.ShowDialog();
        }
    }
}
