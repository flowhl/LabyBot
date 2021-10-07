using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace LabyBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private readonly Forms.NotifyIcon _notifyIcon = new Forms.NotifyIcon();
        private Window _MainWindow = Application.Current.MainWindow;
        bool StartMinimized;
        public MainWindow()
        {
            string[] lines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
            StartMinimized = Convert.ToBoolean(lines[0]);

            if (StartMinimized)
            {
                this.Visibility = Visibility.Hidden;
                this.ShowInTaskbar = false;
                Application.Current.MainWindow.WindowState = WindowState.Minimized;

                _notifyIcon.Icon = new System.Drawing.Icon(@"Images\128.ico");
                _notifyIcon.Text = "Labybot Client";
                _notifyIcon.Click += NotifyIcon_Click;

                _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
                _notifyIcon.ContextMenuStrip.Items.Add("Show", System.Drawing.Image.FromFile(@"Images\dock-window.png"), OnShowClicked);
                _notifyIcon.ContextMenuStrip.Items.Add("Quit", System.Drawing.Image.FromFile(@"Images\close.png"), OnQuitClicked);

                _notifyIcon.Visible = true;
            }
            
            InitializeComponent();
        }

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
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
            Application.Current.MainWindow.WindowState = WindowState.Minimized;

            _notifyIcon.Icon = new System.Drawing.Icon(@"Images\128.ico");
            _notifyIcon.Text = "Labybot Client";
            _notifyIcon.Click += NotifyIcon_Click;

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show", System.Drawing.Image.FromFile(@"Images\dock-window.png"), OnShowClicked);
            _notifyIcon.ContextMenuStrip.Items.Add("Quit",System.Drawing.Image.FromFile(@"Images\close.png") ,OnQuitClicked);

            _notifyIcon.Visible = true;
            
        }

        private void OnQuitClicked(object sender, EventArgs e)
        {
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
