using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using CmlLib.Core;
using CmlLib.Core.Auth;
using DiscordWebhook;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Microsoft.VisualBasic;
using Discord.Webhook;
using Discord;

namespace LabyBot.MVVM.View
{

    public partial class HomeView : UserControl
    {
        //console
        ConsoleContent dc = new ConsoleContent();
        MainWindow wnd = (MainWindow)Application.Current.MainWindow;

        private const int SW_SHOWMINIMIZED = 2;
        private const UInt32 WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        Workhandler workhandler = new Workhandler();
        bool runable = false;
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        bool StartMinimized = false;
        public HomeView()
        {
            InitializeComponent();
            DataContext = dc;
            ChangeMcStatus("stopped", Brushes.Red);
            ChangeWebStatus("stopped", Brushes.Red);
            if (File.Exists(System.IO.Path.Combine(docPath, "labybotsettings.txt")))
            {
                try
                {
                    string[] templines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
                    StartMinimized = Convert.ToBoolean(templines[0]);
                }
                catch
                {
                    MessageBox.Show("Settings file damaged");
                }
            }
            else
            {
                MessageBox.Show("no settings file found - created new one in documents");
                string[] newlines = { false + "", "" };

                using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "labybotsettings.txt")))
                {
                    foreach (string line in newlines)
                        outputFile.WriteLine(line);
                }
            }

            if (StartMinimized)
            {
                try
                {
                    wnd.MinimizeAll();
                    runable = true;
                    McInstances(true);
                    dc.ConsoleOutput.Add("starting all");

                }
                catch (Exception ex)
                {
                    Logger.Log("Error when Minimizing: " + ex.Message);
                }}
        }
            

        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            runable = true;
            McInstances(true);
            dc.ConsoleOutput.Add("starting all");
        }

        private void StopAllButton_Click(object sender, RoutedEventArgs e)
        {
            runable = false;
            dc.ConsoleOutput.Add("stopping all");
            workhandler.AbortMcWorker();
            workhandler.AbortWebWorker();
            KillWindow("Firefox");
        }

        private void LaunchMC_Click(object sender, RoutedEventArgs e)
        {
            runable = true;
            McInstances(false);
        }

        private void WebClaimer_Click(object sender, RoutedEventArgs e)
        {
            WebInstaces();
            ChangeWebStatus("running", Brushes.Green);
        }
        private void ChangeMcStatus(String status, Brush color)
        {
            MCStatus.Text = status;
            MCStatus.Foreground = color;
        }
        private void ChangeWebStatus(String status, Brush color)
        {
            WebStatus.Text = status;
            WebStatus.Foreground = color;
        }
                
        /// <summary>
        /// starts the web service
        /// </summary>
        public async void WebInstaces()
        {
            dc.ConsoleOutput.Add("starting Web Driver");
            string[] lines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));

            for (int i = 2; i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(';');
                if (temp[0] == "" || temp[0] == null)
                {
                    break;
                }
                dc.ConsoleOutput.Add("running Web Worker for " + temp[0]);
               workhandler.StartWebWorker(temp[0], temp[1]);

                await Task.Delay(1000);
                dc.ConsoleOutput.Add("finished Web Worker for " + temp[0]);
            }
            await Task.Delay(40000);
            dc.ConsoleOutput.Add("finished Web Driver");
            ChangeWebStatus("stopped", Brushes.Red);
        }

        /// <summary>
        /// starts the mc instances
        /// </summary>
        /// <param name="runWeb"></param>
        public async void McInstances(bool runWeb)
        {
            dc.ConsoleOutput.Add("started Mc Driver");
            ChangeMcStatus("running", Brushes.Green);
            string[] lines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));

            for (int i = 2; i < lines.Length; i++)
            {
                if (!runable)
                {
                    break;
                }
                string[] credentials = lines[i].Split(';');
                if (credentials[0] == "" || credentials[0] == null)
                {
                    break;
                }
                dc.ConsoleOutput.Add("running mc with " + credentials[2]);
                workhandler.StartMCWorker(credentials[2], credentials[3]);
                await Task.Delay(40000);
                dc.ConsoleOutput.Add("finished client");
            }
            dc.ConsoleOutput.Add("mc finished");
            ChangeMcStatus("stopped", Brushes.Red);
            if (runWeb)
            {
                WebInstaces();
                ChangeWebStatus("running", Brushes.Green);
            }

        }

        /// <summary>
        /// Kills all windows with the given name
        /// </summary>
        /// <param name="name"></param>
        private static void KillWindow(string name)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                process.Kill();
            }
        }
        static void Minimize(string title)
        {
            IntPtr hWnd = FindWindow(title);
            if (!hWnd.Equals(IntPtr.Zero))
            {
                ShowWindowAsync(hWnd, SW_SHOWMINIMIZED);
            }
        }
        public static IntPtr FindWindow(string titleName)
        {
            Process[] pros = Process.GetProcesses(".");
            foreach (Process p in pros)
            {
                if (p.MainWindowTitle.ToUpper().StartsWith(titleName.ToUpper()))
                    return p.MainWindowHandle;
            }
            return new IntPtr();
        }

        /// <summary>
        /// Sends a discord webhook
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public async void SendWebhook(string title, string message, Discord.Color color)
        {
            string[] lines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
            using (var client = new DiscordWebhookClient(lines[1]))
            {
                var embed = new EmbedBuilder
                {
                    Title = title,
                    Description = message,
                    Color = color
                };

                await client.SendMessageAsync(text: "", embeds: new[] { embed.Build() });
            }

        }

        /// <summary>
        /// Class for handling a Console
        /// </summary>
        public class ConsoleContent : INotifyPropertyChanged
        {
            string consoleInput = string.Empty;
            ObservableCollection<string> consoleOutput = new ObservableCollection<string>() { "Console loaded..." };

            public string ConsoleInput
            {
                get
                {
                    return consoleInput;
                }
                set
                {
                    consoleInput = value;
                    OnPropertyChanged("ConsoleInput");
                }
            }

            public ObservableCollection<string> ConsoleOutput
            {
                get
                {
                    return consoleOutput;
                }
                set
                {
                    consoleOutput = value;
                    OnPropertyChanged("ConsoleOutput");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged(string propertyName)
            {
                if (null != PropertyChanged)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
