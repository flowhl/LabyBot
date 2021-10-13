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
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using IWshRuntimeLibrary;

namespace LabyBot.MVVM.View
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {

        readonly string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }
        

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string[] newlines = { StartMinimizedInput.IsChecked + "", webhookInput.Text, Cred1.Text, Cred2.Text, Cred3.Text, Cred4.Text, Cred5.Text };
            
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "labybotsettings.txt")))
            {
                foreach (string line in newlines)
                    outputFile.WriteLine(line);
            }
            
        }
        private void LoadSettings()
        {
            string[] lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
            StartMinimizedInput.IsChecked = Convert.ToBoolean(lines[0]);
            webhookInput.Text = lines[1];
            try
            {
                Cred1.Text = lines[2];
                Cred2.Text = lines[3];
                Cred3.Text = lines[4];
                Cred4.Text = lines[5];
                Cred5.Text = lines[6];
            }
            catch
            {

            }
            
        }



        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                string destination = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                System.IO.File.Delete( destination + @"\Labybot.lnk"); 
            }
            catch
            {
                MessageBox.Show("Failed to remove from startup");
            }
            
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                string applocation = curAssembly.Location;
                string destination = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                WshShell wshShell = new WshShell();
                string fileName = destination + "\\" + "Labybot" + ".lnk";
                IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(fileName);
                shortcut.TargetPath = applocation;
                shortcut.Save();


            }
            catch
            {
                MessageBox.Show("Failed to add to startup");
            }
            
            
        }
    }
}
