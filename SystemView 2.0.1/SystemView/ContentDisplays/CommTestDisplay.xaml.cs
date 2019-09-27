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
using System.ComponentModel;
using AppLogic;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for CommTestDisplay.xaml
    /// </summary>
    public partial class CommTestDisplay : UserControl
    {
        private bool _active;
        public BitmapImage CommTestResult { get; set; }
        public string CommTestElapsedTime { get; set; }


        public CommTestDisplay()
        {
            InitializeComponent();

            TimeDisplay.Text = "0.0 ms";
            IDMatch.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-checked-16-grey.png"));
            StableIP.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-checked-16-grey.png"));

            _active = true;
        }
                 

        private void commTestFail()
        {            
            IDMatch.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-cancel-16.png"));
            StableIP.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-cancel-16.png"));
        }

        public void commTestPass(string Time)
        {
            TimeDisplay.Text = Time + "ms";
            IDMatch.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-checked-16.png"));
            StableIP.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\source\repos\SystemView_Telerik_Version\Icons\icons8-checked-16.png"));
        }

        public bool isActive()
        {
            return this._active;
        }
        public void CommTest_Start(object sender, RoutedEventArgs e)
        {
            try
            {
                CommTest CommTester = new CommTest();


                if (CommTester.ConnectionEstablished)
                {
                    commTestPass(CommTester.ElapsedTime);
                }
                else
                {
                    commTestFail();
                }

                Console.WriteLine(CommTester.ToString());
            }
            catch (Exception ex)
            {
                commTestFail();
                Console.WriteLine("CommTestDisplay::CommTest_Start threw exception: {0}", ex);
            }
            
            
        }

    }
}
