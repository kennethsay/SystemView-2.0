using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Transport;
using AppLogic;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        PTEConnection PTE;

        public Form1()
        {
            InitializeComponent();

            PTE = new PTEConnection();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine(PTE.RealTimeClock.ToString());
            

            Console.WriteLine(PTE.BatteryLevel.ToString());

            Console.WriteLine(PTE.ConfigSettings.ToString());
        }
    }
}
