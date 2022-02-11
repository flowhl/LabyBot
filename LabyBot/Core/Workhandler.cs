using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using CmlLib.Core;
using CmlLib.Core.Auth;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Microsoft.VisualBasic;
using Discord.Webhook;
using Discord;
using System.Collections.Generic;

namespace LabyBot
{
    class Workhandler
    {
        List<AbortableBackgroundWorker> webWorkers = new List<AbortableBackgroundWorker>();
        AbortableBackgroundWorker mcWorker = new AbortableBackgroundWorker();

        private const int SW_SHOWMINIMIZED = 2;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        readonly string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public Workhandler()
        {

        }

        public void AbortMcWorker()
        {
            Logger.Log("Aborting MC Worker");
            mcWorker.Abort();
            mcWorker.Dispose();
        }

        public void AbortWebWorker()
        {
            Logger.Log("Aborting Web Worker");
            foreach (var webWorker in webWorkers)
            {
                webWorker.Abort();
                webWorker.Dispose();
            }
            
        }

        /// <summary>
        /// starts the webworker
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pw"></param>
        public void StartWebWorker(string name, string pw)
        {
            var newWorker = new AbortableBackgroundWorker();
            webWorkers.Add(newWorker);
            newWorker.DoWork += WebWorker_DoWork;
            newWorker.RunWorkerCompleted += WebWorker_RunWorkerCompleted;
            newWorker.RunWorkerAsync(name + ";" + pw);
        }

        /// <summary>
        /// starts the mc worker
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pw"></param>
        public void StartMCWorker(string email, string pw)
        {
            mcWorker.DoWork += MCworker_DoWork;
            mcWorker.RunWorkerCompleted += MCworker_RunWorkerCompleted;
            mcWorker.RunWorkerAsync(email + ";" + pw);
        }

        private void WebWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        /// <summary>
        /// Work function of the webworker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void WebWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger.Log("Web Worker doing work");
            string name = e.Argument.ToString().Split(';')[0];
            string pw = e.Argument.ToString().Split(';')[1];
            try
            {
                var service = FirefoxDriverService.CreateDefaultService();                
                service.HideCommandPromptWindow = true;
                var options = new FirefoxOptions();
                options.AddArgument("--headless");

                using (IWebDriver driver = new FirefoxDriver(service, options))
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.labymod.net/en");
                        await Task.Delay(3000);

                        driver.FindElement(By.CssSelector("a.openLogin")).Click(); //open login
                        driver.FindElement(By.Id("username")).SendKeys(name); //send name
                        driver.FindElement(By.Id("password")).SendKeys(pw); //send password
                        if (driver.FindElement(By.Id("memLogin")).Selected)
                        { //make sure remember me is unchecked
                            driver.FindElement(By.Id("memLogin")).Click();
                        }
                        driver.FindElement(By.CssSelector("button.btn-custom.btn-icon.btn-login")).Submit(); //submit login

                        await Task.Delay(2000);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Webdriver error on initialization: " + exception.Message);
                        Logger.Log("Webdriver error on initialization: " + exception.Message);
                    }


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
                    catch (Exception exception)
                    {
                        MessageBox.Show("Error on 2step auth: " + exception.Message);
                        Logger.Log("Error on 2step auth: " + exception.Message);
                    }

                    //Test if 2FA
                    try
                    {
                        driver.FindElement(By.Id("two-fa-input-swal"));
                        System.Windows.Forms.MessageBox.Show("please disable 2 factor auth on the account: " + name);
                        //dc.ConsoleOutput.Add("please disable 2 factor auth on the account: " + name);
                        SendWebhook(name, "please disable 2 factor auth on the account: " + name, Discord.Color.Red);
                        Logger.Log("2FA needs to be disabled!");
                        return;
                    }
                    catch
                    {
                        ///everything is fine because there should not be a 2fa element
                    }

                    //Test if wrong pw

                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(), 'Wrong username/password')]"));
                        MessageBox.Show("wrong password for " + name);
                        SendWebhook(name, "wrong password for " + name, Discord.Color.Red);
                        Logger.Log("wrong password for " + name);
                        return;
                    }
                    catch 
                    {
                        ///everything is fine because there should not be a wrong login message
                    }

                    //switch to dashboard
                    try
                    {
                         driver.Navigate().GoToUrl("https://labymod.net/en/dashboard");
                         await Task.Delay(5000);
                    }
                    catch(Exception exception)
                    {
                        MessageBox.Show("Error when trying to reach the Labymod dashboard: " + exception.Message);
                        Logger.Log("Error when trying to reach the Labymod dashboard: " + exception.Message);
                        return;
                    }
                    

                    //accept cookies

                    try
                    {
                        driver.FindElement(By.CssSelector("a.btn.btn-primary.js-accept-cookies")).Click();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Error when trying to accept the cookies: " + exception.Message);
                        Logger.Log("Error when trying to accept the cookies: " + exception.Message);
                        return;
                    }

                    try
                    {
                        var objects = driver.FindElements(By.CssSelector("button.btn.btn-sm.btn-custom.pull-right.claimRewardBtn"));
                        foreach (IWebElement obj in objects)
                        {
                            obj.Click();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("no claimbutton found!");
                        Logger.Log("no claimbutton found!");
                        try
                        {
                            driver.FindElement(By.CssSelector("span.status"));
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show("Error when trying to claim: " + exception.Message);
                            Logger.Log("Error when trying to claim: " + exception.Message);
                            return;
                        }

                    }
                    try
                    {
                        if (driver.FindElement(By.CssSelector("div.dailyCoinValue")).Text == "??")
                        {
                            MessageBox.Show("no reward claimable");
                            Logger.Log("no reward claimable");
                            SendWebhook(name, "no reward claimable", Discord.Color.Red);
                            return;
                        }
                        else
                        {
                                driver.FindElement(By.CssSelector("span.status"));                            
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Log("Error when checking if reward was claimed: " + exception.Message);
                    }
                    try
                    {
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
                    }
                    catch (Exception exception)
                    {
                        Logger.Log("Error logging out: " + exception.Message);
                        return;
                    }

                    string[] arrLine = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
                    arrLine[0] = DateTime.Today.ToString("d");
                    File.WriteAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"), arrLine);

                    KillWindow("Firefox");
            }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error creating firefox driver: " + exception.Message);
                Logger.Log("Error creating firefox driver: " + exception.Message);
            }
        }
        private void MCworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        /// <summary>
        /// Runs the McWorker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void MCworker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger.Log("MC Worker doing work");
            string email = e.Argument.ToString().Split(';')[0];
            string pw = e.Argument.ToString().Split(';')[1];

            try
            {
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
            catch(Exception exception)
            {
                MessageBox.Show("Error in MC Worker: " + exception.Message);
                Logger.Log("Error in MC Worker: " + exception.Message);
            }
        }

        /// <summary>
        /// Sends a Webhook with title, message and color
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public async void SendWebhook(string title, string message, Discord.Color color)
        {
            try
            {
                string[] lines = File.ReadAllLines(System.IO.Path.Combine(docPath, "labybotsettings.txt"));
                using (var client = new DiscordWebhookClient(lines[2]))
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
            catch (Exception exception)
            {
                Logger.Log("Webhook Error: " + exception.Message);
            }


        }
        /// <summary>
        /// finds kills the window with a given name
        /// </summary>
        /// <param name="name"></param>
        private static void KillWindow(string name)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                process.Kill();
            }
        }

        /// <summary>
        /// finds a window with a given name
        /// </summary>
        /// <param name="titleName"></param>
        /// <returns></returns>
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
