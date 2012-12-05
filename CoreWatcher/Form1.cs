﻿using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CoreWatcher
{
    public partial class Form1 : Form
    {
        private List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();
        private NotifyIcon ramIcon;
        private List<NotifyIcon> cpuNotifyIcons = new List<NotifyIcon>();
        private Dictionary<int, Icon> cpuIcons = new Dictionary<int, Icon>();
        private Dictionary<int, Icon> ramIcons = new Dictionary<int, Icon>();

        private Computer cmp;
        private static readonly float megaByte = 1024.0f * 1024.0f;

        readonly Brush CpuBrush = Brushes.LimeGreen;
        readonly Brush RamBrush = Brushes.MediumOrchid;

        private float maxRam;
        private float actRam;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            this.cmp = new Computer();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                this.cpuCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
                NotifyIcon nic = new NotifyIcon() { Icon = SystemIcons.Question, Visible = true };
                nic.Click += RightClick;
                this.cpuNotifyIcons.Add(nic);
            }

            this.ramIcon = new NotifyIcon() { Icon = SystemIcons.Question, Visible = true };
            this.ramIcon.Click += RightClick;

            this.timer1.Interval = 250;
            this.timer1.Start();
        }

        private void RightClick(object sender, EventArgs ea)
        {
            if (ea is MouseEventArgs)
            {
                if ((ea as MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Right)
                    this.Close();
            }
        }

        private Icon MakePict(int percent, Brush brush)
        {
            Bitmap b = new Bitmap(102, 102);
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(Brushes.White, 0, 0, 102, 102);
            g.FillRectangle(Brushes.Black, 1, 1, 100, 100);
            g.FillRectangle(brush, 1, 101 - percent, 100, percent);
            g.Flush();
            return Icon.FromHandle(b.GetHicon());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < cpuCounters.Count; i++)
            {
                int percent = (int)cpuCounters[i].NextValue();
                if (!this.cpuIcons.ContainsKey(percent))
                    this.cpuIcons.Add(percent, MakePict(percent, CpuBrush));
                this.cpuNotifyIcons[i].Icon = cpuIcons[percent];
                this.cpuNotifyIcons[i].Text = string.Format("Cpu{0} {1}%", cpuCounters[i].InstanceName, percent);
            }
            Console.WriteLine();
            this.maxRam = cmp.Info.TotalPhysicalMemory / megaByte;
            this.actRam = this.maxRam - (cmp.Info.AvailablePhysicalMemory / megaByte);

            int ram = Convert.ToInt32(this.actRam / this.maxRam * 100);
            ramIcon.Text = string.Format("RAM {0:0} MB / {1:0} MB ({2}%)", this.actRam, this.maxRam, ram);
            if (!this.ramIcons.ContainsKey(ram))
                this.ramIcons.Add(ram, MakePict(ram, RamBrush));
            ramIcon.Icon = ramIcons[ram];
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            foreach (var item in cpuNotifyIcons)
            {
                item.Dispose();
            }
            ramIcon.Dispose();
            foreach (var item in cpuCounters)
            {
                item.Dispose();
            }
        }
    }
}
