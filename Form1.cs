
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BreakTimer
{
    public partial class Form1 : Form
    {
        NotifyIcon notifyIcon;
        readonly Button buttonDelayOneHour;
        readonly Button buttonDelayTenMinutes;
        readonly Button buttonSnooze3Minutes;
        readonly int breakTimeLength = 60; //seconds
        readonly int workTimeLength = 600; //seconds
        readonly int delayOneHour = 3600; //seconds
        readonly int toBottomDistance = 120;
        readonly int buttonWidth = 200;
        readonly int buttonHeight = 40;
        readonly int buttonGap = 40;
        Timer timerBreak;
        Timer timerWork;
        Label labelCountdown;
        int currentCountdown = 60;
        readonly DateTime startupTime = DateTime.Now;

        public Form1()
        {
            InitializeComponent();
            buttonDelayOneHour = CreateButton("Delay 1 hour", buttonclick_DeleyOneHour);
            buttonDelayTenMinutes = CreateButton("Delay 10 minutes", buttonclick_DeleyTenMinutes);
            buttonSnooze3Minutes = CreateButton("Snooze 3 minutes", buttonclick_Snooze);

            CreateLabel();

            Load += new EventHandler(Form_Load);
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        private void CreateLabel()
        {
            labelCountdown = new Label
            {
                Location = new Point(12, 41),
                Size = new Size(buttonWidth, buttonWidth),
                Text = "00",
                Font = new Font("Arial", 80),
            };
            Controls.Add(labelCountdown);
        }

        private Button CreateButton(string text, EventHandler clickEvent)
        {
            var button = new Button
            {
                Location = new Point(12, GetNextYPosition()),
                Size = new Size(buttonWidth, buttonHeight),
                Text = text
            };
            button.Click += new EventHandler(clickEvent);
            Controls.Add(button);

            return button;
        }

        private int GetNextYPosition()
        {
            return 41;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            CreateNotifyIcon();
            CreateContextMenu();
            timerWork = new Timer();
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
                Hide();
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
            Hide();
            timerBreak.Stop();
            timerWork.Stop();
            timerWork.Interval = newInterval;
            timerWork.Start();
        }

        private void EnterFullScreen()
        {
            SetFullScreenProperties();

            SetButtonLocation(buttonDelayOneHour, -1);
            SetButtonLocation(buttonDelayTenMinutes, 0);
            SetButtonLocation(buttonSnooze3Minutes, 1);

            SetLabelLocation(labelCountdown);

            void SetFullScreenProperties()
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
                Show();
            }

            void SetButtonLocation(Button button, int horizontalPosition)
            {
                int x = Width / 2 + horizontalPosition * (buttonWidth + buttonGap);
                int y = Height - toBottomDistance;
                button.Location = new Point(x - buttonWidth / 2, y);
            }

            void SetLabelLocation(Label label)
            {
                int x = Width / 2 - label.Width / 2;
                int y = Height / 2 - label.Height / 2;
                label.Location = new Point(x, y);
            }
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

                string message = $"BreakTimer has been running for {timeSpan.TotalMinutes} minutes.";
                RegistryLogger.Log(message);
                m.Result = (IntPtr)1;
            }

            base.WndProc(ref m);
        }
    }
}
