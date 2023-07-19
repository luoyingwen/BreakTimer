using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace BreakTimer
{
    class RegistryLogger
    {
        private const string SubKey = "BreakTimer";
        private const string ValueName = "LogData";
        private const int MaxLogLines = 10;

        public static void Log(string message)
        {
            try
            {
                using (RegistryKey key = GetOrCreateSubKey())
                {
                    string[] existingLog = key.GetValue(ValueName) as string[];

                    string newLogLine = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message;

                    if (existingLog != null && existingLog.Length >= MaxLogLines)
                    {
                        existingLog = existingLog.Skip(1).ToArray(); // Remove the oldest log line
                    }

                    if (existingLog == null)
                    {
                        existingLog = new string[] { newLogLine };
                    }
                    else
                    {
                        existingLog = existingLog.Concat(new string[] { newLogLine }).ToArray();
                    }

                    // Save the updated log to the registry
                    key.SetValue(ValueName, existingLog, RegistryValueKind.MultiString);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions or log them to a different location if needed
                Console.WriteLine("Error logging to the registry: " + ex.Message);
            }
        }

        private static RegistryKey GetOrCreateSubKey()
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey subKey = currentUserKey.OpenSubKey(SubKey, true);

            if (subKey == null)
            {
                subKey = currentUserKey.CreateSubKey(SubKey);
            }

            return subKey;
        }
    }

    public partial class Form1 : Form
    {
        NotifyIcon notifyIcon;
        Button buttonDelayOneHour;
        Button buttonDelayTenMinutes;
        Button buttonSnooze3Minutes;
        int breakTimeLength = 60; //seconds
        int workTimeLength = 600; //seconds
        int delayOneHour = 3600; //seconds
        int toBottomDistance = 120;
        int buttonWidth = 200;
        int buttonHeight = 40;
        int buttonGap = 40;
        Timer timerBreak;
        Timer timerWork;
        Label labelCountdown;
        int currentCountdown = 60;
        DateTime startupTime = DateTime.Now;

        public Form1()
        {
            InitializeComponent();

            buttonDelayOneHour = new Button();
            buttonDelayOneHour.Location = new Point(12, 41);
            buttonDelayOneHour.Size = new Size(buttonWidth, buttonHeight);
            buttonDelayOneHour.Text = "Delay 1 hour";
            buttonDelayOneHour.Click += new EventHandler(buttonclick_DeleyOneHour);
            this.Controls.Add(buttonDelayOneHour);

            buttonDelayTenMinutes = new Button();
            buttonDelayTenMinutes.Location = new Point(12, 41);
            buttonDelayTenMinutes.Size = new Size(buttonWidth, buttonHeight);
            buttonDelayTenMinutes.Text = "Delay 10 minutes";
            buttonDelayTenMinutes.Click += new EventHandler(buttonclick_DeleyTenMinutes);
            this.Controls.Add(buttonDelayTenMinutes);

            buttonSnooze3Minutes = new Button();
            buttonSnooze3Minutes.Location = new Point(12, 41);
            buttonSnooze3Minutes.Size = new Size(buttonWidth, buttonHeight);
            buttonSnooze3Minutes.Text = "Snooze 3 minutes";
            buttonSnooze3Minutes.Click += new EventHandler(buttonclick_Snooze);
            this.Controls.Add(buttonSnooze3Minutes);

            labelCountdown = new Label();
            labelCountdown.Location = new Point(12, 41);
            labelCountdown.Size = new Size(buttonWidth, buttonWidth);
            labelCountdown.Text = "0";
            labelCountdown.Font = new Font(labelCountdown.Font.FontFamily, 80);
            this.Controls.Add(labelCountdown);

            this.Load += new EventHandler(Form_Load);
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            CreateNotifyIcon();
            //CreateContextMenu();
            timerWork = new Timer();
            ;
            timerWork.Interval = workTimeLength * 1000;
            timerWork.Tick += TimerWork_Tick;
            timerWork.Start();

            timerBreak = new Timer();
            timerBreak.Interval = 1000;
            timerBreak.Tick += TimerBreak_Tick;
        }

        private void TimerBreak_Tick(object sender, EventArgs e)
        {
            currentCountdown--;
            labelCountdown.Text = currentCountdown.ToString();
            if (currentCountdown <= 0)
            {
                timerBreak.Stop();
                this.Hide();
                timerWork.Interval = workTimeLength * 1000;
                timerWork.Start();
            }
        }

        private void TimerWork_Tick(object sender, EventArgs e)
        {
            timerWork.Stop();
            EnterFullScreen();
            currentCountdown = breakTimeLength;
            labelCountdown.Text = currentCountdown.ToString();
            timerBreak.Start();
        }

        private void buttonclick_DeleyOneHour(object sender, EventArgs e)
        {
            CreateNewTimer(delayOneHour * 1000);
        }

        private void buttonclick_DeleyTenMinutes(object sender, EventArgs e)
        {
            CreateNewTimer(delayOneHour / 6 * 1000);
        }

        private void buttonclick_Snooze(object sender, EventArgs e)
        {
            CreateNewTimer(delayOneHour / 60 * 1000);
        }

        private void CreateNewTimer(int newInterval)
        {
            this.Hide();
            timerBreak.Stop();
            timerWork.Stop();
            timerWork.Interval = newInterval;
            timerWork.Start();
        }

        private void EnterFullScreen()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Show();

            int formWidth = this.Width;
            int formHeight = this.Height;

            buttonDelayOneHour.Location = new Point(
                formWidth / 2 - buttonWidth - buttonWidth / 2 - buttonGap,
                formHeight - toBottomDistance
            );
            buttonDelayTenMinutes.Location = new Point(
                formWidth / 2 - buttonWidth / 2,
                formHeight - toBottomDistance
            );
            buttonSnooze3Minutes.Location = new Point(
                formWidth / 2 + buttonWidth / 2 + buttonGap,
                formHeight - toBottomDistance
            );

            labelCountdown.Location = new Point(
                formWidth / 2 - labelCountdown.Width / 2,
                formHeight / 2 - labelCountdown.Height / 2
            );
        }

        private void CreateNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            Icon myIcon = BreakTimer.Properties.Resources.Icon1;
            notifyIcon.Icon = Properties.Resources.Icon1;
            notifyIcon.Text = "BreakTimer";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
        }

        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItem = new MenuItem();
            menuItem.Text = "退出";
            menuItem.Click += new EventHandler(menuItem_ExitApp);
            contextMenu.MenuItems.Add(menuItem);

            notifyIcon.ContextMenu = contextMenu;
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            EnterFullScreen();
        }

        private void menuItem_ExitApp(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_QUERYENDSESSION = 0x0011;
            if (m.Msg == WM_QUERYENDSESSION)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan timeSpan = currentTime - startupTime;

                string message = string.Format(
                    "BreakTimer has been running for {0} minutes.",
                    timeSpan.TotalMinutes.ToString()
                );
                RegistryLogger.Log(message);
                m.Result = (IntPtr)1;
            }

            base.WndProc(ref m);
        }
    }
}
