﻿using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace LeapMIDI
{
    public partial class MainForm : Form
    {
        LeapStuff leap = new LeapStuff();

        System.Drawing.Graphics graphicsObj = null;
        Pen redPen = new Pen(System.Drawing.Color.Red, 5);
        Pen yellowPen = new Pen(System.Drawing.Color.Yellow, 5);

        SimpleKalman kal_x = new SimpleKalman();
        SimpleKalman kal_y = new SimpleKalman();

        public MainForm()
        {
            InitializeComponent();

            graphicsObj = pictureBox1.CreateGraphics();

            System.Windows.Forms.Timer aTimer = new System.Windows.Forms.Timer();            
            aTimer.Tick += OnTimedEvent;
            aTimer.Interval = 10; // milliseconds
            aTimer.Enabled=true;
        }   

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, EventArgs e)
        {
          leap.Update();
          LeapLabel.Text = leap.info;
          Animate();
          RobotControl();
        }

        int robot_count = 0;

        private void RobotControl()
        {
            robot_count++;
            if (robot_count < 10 )  return;
            robot_count = 0;

            if (leap.hands != 1)
            {
                 SendLeftRight(0, 0);
            }

            float speed = (leap.posY - 80f) / 400;

            if (speed < 0) speed = 0;
            if (speed > 100) speed = 100;

          //  int left = 
            SendLeftRight((int)speed, (int)speed);
        }

        private void SendUdp(int srcPort, string dstIp, int dstPort, byte[] data)
        {
            using (UdpClient c = new UdpClient(srcPort))
                c.Send(data, data.Length, dstIp, dstPort);
        }

        private void SendLeftRight(int left, int right)
        {
            String command = left.ToString() + "," + right.ToString();
            SendUdp(1000, "192.168.7.20", 2000, Encoding.ASCII.GetBytes(command));
        }

        private void Animate()
        {
            if (graphicsObj == null) return;

            graphicsObj.Clear(Color.Black);

            // Position

            float px = (pictureBox1.Width / 2f) + leap.posX;
            float py = (pictureBox1.Height) - leap.posY;
            float psize = 50 + (leap.posZ);
            try
            {
                graphicsObj.DrawEllipse(redPen, px, py, psize, psize);
            }
            catch { }

            // Motion

            float mx = (pictureBox1.Width / 2f) + leap.velX;
            float my = (pictureBox1.Height / 2f) - leap.velY;
            float msize = 5 + (leap.pinch * 10f);

            float kx = (float)kal_x.update(mx);
            float ky = (float)kal_y.update(my);

            graphicsObj.DrawEllipse(yellowPen, kx, ky, msize, msize);          
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
          Animate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
          Environment.Exit(0);
        }
    }
}
