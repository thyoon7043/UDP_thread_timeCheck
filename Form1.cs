using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDPTest
{
    public partial class Form1 : Form
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint timeEndPeriod(uint uMilliseconds);

        string m_strIPAddress;
        int m_nPortNumber;
        int m_nPeroid;

        UdpClient m_Udp = null;
        bool m_bThreading = false;

        public Form1()
        {
            InitializeComponent();

            timeBeginPeriod(1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bThreading = false;

            timeEndPeriod(1);
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            m_strIPAddress = textBox_IPAddress.Text;
            m_nPortNumber = int.Parse(textBox_PortNumber.Text);
            m_nPeroid = int.Parse(textBox_Period.Text);

            m_bThreading = true;

            Thread thread = new Thread(new ThreadStart(SendDataThreadFunc));
            thread.Priority = System.Threading.ThreadPriority.Highest;
            thread.IsBackground = true;
            thread.Start();

            button_Start.Enabled = false;
            button_Stop.Enabled = true;
        }

        void SendDataThreadFunc()
        {
            try
            {
                m_Udp = new UdpClient();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
            

            DateTime dtLast = DateTime.Now;
            int nCount = 0;

            while (m_bThreading)
            {
                if ((DateTime.Now - dtLast).TotalMilliseconds >= m_nPeroid)
                {
                    try
                    {
                        byte[] btX = BitConverter.GetBytes(nCount);
                        byte[] btY = BitConverter.GetBytes(nCount);
                        byte[] btData = new byte[btX.Length + btY.Length];
                        btX.CopyTo(btData, 0);
                        btY.CopyTo(btData, 4);
                        m_Udp.Send(btData, btData.Length, m_strIPAddress, m_nPortNumber);
                    }
                    catch (Exception) { }

                    dtLast = DateTime.Now;

                    nCount++;
                    if (nCount > 100) nCount = 0;
                }
            }

            m_Udp.Close();
            m_Udp.Dispose();
            m_Udp = null;
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            m_bThreading = false;

            button_Start.Enabled = true;
            button_Stop.Enabled = false;
        }
    }
}
