using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Forms = System.Windows.Forms;
namespace LabyBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Forms.NotifyIcon _notifyIcon = new Forms.NotifyIcon();
        private string _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";

        public MainWindow()
        {
            Logger.Log("starting components");
            InitializeComponent();
            Logger.Log("Done starting components");
        }

        /// <summary>
        /// Minimizes the window
        /// </summary>
        public void MinimizeAll()
        {
            Logger.Log("Minimizing");
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
            Application.Current.MainWindow.WindowState = WindowState.Minimized;

            _notifyIcon.Icon = new System.Drawing.Icon(_appPath + @"Images\128.ico");
            _notifyIcon.Text = "Labybot Client";
            _notifyIcon.Click += NotifyIcon_Click;

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show", System.Drawing.Image.FromFile(_appPath + @"Images\dock-window.png"), OnShowClicked);
            _notifyIcon.ContextMenuStrip.Items.Add("Quit", System.Drawing.Image.FromFile(_appPath + @"Images\close.png"), OnQuitClicked);

            _notifyIcon.Visible = true;

        }

        /// <summary>
        /// Dargs the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            MinimizeAll();
            
        }

        private void OnQuitClicked(object sender, EventArgs e)
        {
            Logger.Log("Shutting Down");
            Application.Current.Shutdown();
        }        

        private void OnShowClicked(object sender, EventArgs e)
        {

            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            this.Activate();

        }

        public void OnExit()
        {
            _notifyIcon.Dispose();
        }
        public void SendNotification(string Message)
        {
            _notifyIcon.ShowBalloonTip(3000, "Labybot", Message, Forms.ToolTipIcon.Info);
        }
    }
}
