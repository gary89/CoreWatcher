﻿using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CoreWatcher
{
    class CoreWatcherContext : ApplicationContext
    {
        private List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();
        private NotifyIcon ramIcon;
        private List<NotifyIcon> cpuNotifyIcons = new List<NotifyIcon>();
        private Dictionary<int, Icon> cpuIcons = new Dictionary<int, Icon>();
        private Dictionary<int, Icon> ramIcons = new Dictionary<int, Icon>();

        private Computer cmp;
        private static readonly float megaByte = 1024.0f * 1024.0f;

        private Timer timer;

        readonly Brush CpuBrush = Brushes.LimeGreen;
        readonly Brush RamBrush = Brushes.MediumOrchid;

        private float maxRam;
        private float actRam;
       
        public CoreWatcherContext()
        {
            cmp = new Computer();
            timer = new Timer();
            timer.Tick += timer1_Tick;

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                cpuCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
                NotifyIcon nic = new NotifyIcon() { Icon = SystemIcons.Question, Visible = true };
                nic.Click += rightClick;
                cpuNotifyIcons.Add(nic);
            }

            ramIcon = new NotifyIcon() { Icon = SystemIcons.Question, Visible = true };
            ramIcon.Click += rightClick;

            timer.Interval = 250;
            timer.Start();
        }

        private void rightClick(object sender, EventArgs ea)
        {
            MouseEventArgs mea = ea as MouseEventArgs;

            if (mea == null)
                return;

            if (mea.Button == MouseButtons.Right)
                close();
        }

        private Icon makePict(int percent, Brush brush)
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
                if (!cpuIcons.ContainsKey(percent))
                    cpuIcons.Add(percent, makePict(percent, CpuBrush));
                cpuNotifyIcons[i].Icon = cpuIcons[percent];
                cpuNotifyIcons[i].Text = string.Format("Cpu{0} {1}%", cpuCounters[i].InstanceName, percent);
            }
            Console.WriteLine();
            maxRam = cmp.Info.TotalPhysicalMemory / megaByte;
            actRam = maxRam - (cmp.Info.AvailablePhysicalMemory / megaByte);

            int ram = Convert.ToInt32(actRam / maxRam * 100);
            ramIcon.Text = string.Format("RAM {0:0} MB / {1:0} MB ({2}%)", actRam, maxRam, ram);
            if (!ramIcons.ContainsKey(ram))
                ramIcons.Add(ram, makePict(ram, RamBrush));
            ramIcon.Icon = ramIcons[ram];
        }

        private void close()
        {
            timer.Stop();
            foreach (var item in cpuNotifyIcons)
                item.Dispose();
            ramIcon.Dispose();
            foreach (var item in cpuCounters)
                item.Dispose();

            Application.Exit();
        }
    }
}
