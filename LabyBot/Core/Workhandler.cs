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


namespace LabyBot
{
    class Workhandler
    {

        private const int SW_SHOWMINIMIZED = 2;
        private const UInt32 WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        bool runable = false;
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public Workhandler()
        {

        }

        public void SetRunable(bool status)
        {
            runable = status;
        }


        public void StartWebWorker(string name, string pw)
        {
            BackgroundWorker webWorker = new BackgroundWorker();
            webWorker.DoWork += webWorker_DoWork;
            webWorker.RunWorkerCompleted += webWorker_RunWorkerCompleted;
            webWorker.RunWorkerAsync(name + ";" + pw);

        }
        public void StartMCWorker(string email, string pw)
        {
            BackgroundWorker mcWorker = new BackgroundWorker();
            mcWorker.DoWork += MCworker_DoWork;
            mcWorker.RunWorkerCompleted += MCworker_RunWorkerCompleted;
            mcWorker.RunWorkerAsync(email + ";" + pw);
        }

        private void webWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        async void webWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string name = e.Argument.ToString().Split(';')[0];
            string pw = e.Argument.ToString().Split(';')[1];
            var service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new FirefoxOptions();
            options.AddArgument("--headless");


            using (IWebDriver driver = new FirefoxDriver(service, options))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                driver.Navigate().GoToUrl("https://www.labymod.net/en");
                await Task.Delay(5000);

                driver.FindElement(By.CssSelector("a.openLogin")).Click(); //open login
                driver.FindElement(By.Id("username")).SendKeys(name); //send name
                driver.FindElement(By.Id("password")).SendKeys(pw); //send password
                if (driver.FindElement(By.Id("memLogin")).Selected)
                { //make sure remember me is unchecked
                    driver.FindElement(By.Id("memLogin")).Click();
                }
                driver.FindElement(By.CssSelector("button.btn-custom.btn-icon.btn-login")).Submit(); //submit login

                await Task.Delay(2000);

                //Test if email 2 auth required
                try
                {
                    if (driver.FindElement(By.Id("login-verification-key")).Displayed)
                    {
                    string auth = Interaction.InputBox("please insert the verification code from the email of " + name, "2 factor auth required", "");
                        if (auth == "" || auth == null)
                        {
                            MessageBox.Show("Auth is empty - please restart and try again!");
                                return;
                        }
                    driver.FindElement(By.Id("login-verification-key")).SendKeys(auth);
                    driver.FindElement(By.XPath("//*[@id=\"navigation\"]/div[2]/div/div/div/div/div/button")).Click();
                    //dc.ConsoleOutput.Add("email auth completed");
                    await Task.Delay(2000);
                    }
                    
                }
                catch
                {
                    
                }

                //Test if 2FA
                try
                {
                    driver.FindElement(By.Id("two-fa-input-swal"));
                    System.Windows.Forms.MessageBox.Show("please disable 2 factor auth on the account: " + name);
                    //dc.ConsoleOutput.Add("please disable 2 factor auth on the account: " + name);
                    SendWebhook(name, "please disable 2 factor auth on the account: " + name, Discord.Color.Red);
                    return;
                }
                catch
                {

                }

                //Test if wrong pw

                try
                {
                    driver.FindElement(By.XPath("//*[contains(text(), 'Wrong username/password')]"));
                    MessageBox.Show("wrong password for " + name);
                    SendWebhook(name, "wrong password for " + name, Discord.Color.Red);
                    return;
                }
                catch
                {

                }

                //switch to dashboard
                driver.Navigate().GoToUrl("https://labymod.net/en/dashboard");
                await Task.Delay(5000);

                //accept cookies

                try
                {
                    driver.FindElement(By.CssSelector("a.btn.btn-primary.js-accept-cookies")).Click();
                }
                catch
                {
                }

                try
                {
                    //driver.FindElement(By.CssSelector("button.btn.btn-sm.btn-custom.pull-right.claimRewardBtn")).Click();

                    var objects = driver.FindElements(By.CssSelector("button.btn.btn-sm.btn-custom.pull-right.claimRewardBtn"));
                    foreach (IWebElement obj in objects)
                    {
                        obj.Click();
                    }
                }
                catch
                {
                    MessageBox.Show("no claimbutton found!");
                    try
                    {
                        driver.FindElement(By.CssSelector("span.status"));
                    }
                    catch
                    {
                        //dc.ConsoleOutput.Add("Allready claimed");
                    }

                }
                try
                {
                    if (driver.FindElement(By.CssSelector("div.dailyCoinValue")).Text == "??")
                    {
                        MessageBox.Show("no reward claimable");
                        SendWebhook(name, "no reward claimable", Discord.Color.Red);


                    }
                    else
                    {
                        try
                        {
                            driver.FindElement(By.CssSelector("span.status"));

                        }
                        catch
                        {

                        }

                    }
                }
                catch
                {
                    return;
                }

                driver.Navigate().GoToUrl("https://labymod.net/en/dashboard");
                await Task.Delay(2000);

                //send stats

                SendWebhook(name, driver.FindElement(By.CssSelector("div.dailyCoinValue")).Text + " labycoins earned - total coins: " + driver.FindElement(By.CssSelector("span.value")).Text + " - streak: " + driver.FindElement(By.CssSelector("span.value.streak")).Text + " :fire: ", Discord.Color.Blue);

                //log out

                driver.Navigate().GoToUrl("https://www.labymod.net/logout");
                await Task.Delay(2000);
                driver.Navigate().GoToUrl("https://www.labymod.net/logout");
                driver.Close();
                driver.Quit();
                KillWindow("Firefox");
            }
        }
        private void MCworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        async void MCworker_DoWork(object sender, DoWorkEventArgs e)
        {
            string email = e.Argument.ToString().Split(';')[0];
            string pw = e.Argument.ToString().Split(';')[1];

            if (!runable)
            {
                return;
            }
            var login = new MLogin();
            var response = login.Authenticate(email, pw);
            //dc.ConsoleOutput.Add("logging into Minecraft Client with email: " + email);
            if (!response.IsSuccess) // failed to automatically log in
            {
                response = login.Authenticate(email, pw);

                if (!response.IsSuccess)
                {
                    //Console.WriteLine("error: " + response.Result.ToString());
                    await Task.Delay(2000);
                    throw new Exception(response.Result.ToString()); // failed to log in
                }
            }

            // This session variable is the result of logging in and is used in MLaunchOption, in the Launch part below.
            var session = response.Session;
            // increase connection limit to fast download
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;

            //var path = new MinecraftPath("game_directory_path");
            var path = new MinecraftPath(); // use default directory

            var launcher = new CMLauncher(path);


            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 2048,
                FullScreen = false,
                ScreenWidth = 800,
                ScreenHeight = 400,
                Session = session,
            };

            var process = await launcher.CreateProcessAsync("LabyMod-3-1.8.9", launchOption);
            process.Start();
            for (int i = 0; i < 26; i++)
            {
                if (!runable)
                {
                    return;
                }
                //minimize
                IntPtr hWnd = FindWindow("Minecraft 1.8.9 ");
                if (!hWnd.Equals(IntPtr.Zero))
                {
                    ShowWindowAsync(hWnd, SW_SHOWMINIMIZED);
                }

                await Task.Delay(500);
            }
            KillWindow("javaw");
            await Task.Delay(1000);
        }
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
    }
}
