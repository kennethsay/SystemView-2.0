using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
    /// Interaction logic for RTCDisplay.xaml
    /// </summary>
    public partial class RTCDisplay : UserControl, INotifyPropertyChanged
    {
        private RTC _myRTC;
        private bool _Active;
        private string _pcTime;

        public string PCTime
        {
            get { return _pcTime; }
            set
            {
                if (_pcTime != value)
                {
                    _pcTime = value;
                    OnPropertyChanged("PCTime");
                }
            }
        }

        public string OBCTime
        {
            get { return _obcTime; }
            set
            {
                if (_obcTime != value)
                {
                    _obcTime = value;
                    OnPropertyChanged("OBCTime");
                }
            }
        }

        private string _obcTime;

        public RTCDisplay()
        {
            try
            {
                InitializeComponent();

                _myRTC = new RTC();

                _pcTime = "";
                _obcTime = "";

                this.DataContext = this;
                
                BackgroundWorker _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.DoWork += doWorkMethod;
                _worker.RunWorkerCompleted += runWorkerCompletedMethod;
                _worker.RunWorkerAsync();                

            }
            catch
            {

            }           
        }

        private void updateRTCUITime()
        {
            try
            {
                OBCTime = _myRTC.OBCLocalTime.ToString();
                PCTime = _myRTC.PCTime.ToString();                
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC Display::updateRTCUITime-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }

        }

        private void updateRTCValues()
        {
            try
            {
                _myRTC.UpdateRTCReading();
                updateRTCUITime();
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC Display::updateRTCValues-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }

        }      
        

        /// Sync RTC and Clear Flash
        /// 

        public void ClearFlash(object sender, RoutedEventArgs e)
        {             
            try
            {
                FlashErase myFlashErase;
                MessageBoxResult FlashEraseVerify = MessageBox.Show("This action will delete vital flash data. Do you wish to proceed?", "Proceed?", MessageBoxButton.YesNo);
                if (FlashEraseVerify == MessageBoxResult.Yes)
                {
                    myFlashErase = new FlashErase();

                    if (myFlashErase.FlashEraseStarted)
                    {
                        FlashErase.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\Desktop\SystemView\SystemView\Icons\icons8-checked-16.png"));
                    }
                    else
                    {
                        SyncComplete.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\Desktop\SystemView\SystemView\Icons\icons8-cancel.png"));
                    }
                }
                else
                {
                    SyncComplete.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\Desktop\SystemView\SystemView\Icons\icons8-checked-16-grey.png"));
                }
            }
            catch
            {

            }
        }


        public void SyncPCRTCTime(object sender, RoutedEventArgs e)
        {
            try
            {
                _myRTC.Sync();
                updateRTCValues();
                updateRTCUITime();

                SyncComplete.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\Desktop\SystemView\SystemView\Icons\icons8-checked-16.png"));

            }
            catch
            {
                SyncComplete.Source = new BitmapImage(new Uri(@"C:\Users\WIN10TESTPC\Desktop\SystemView\SystemView\Icons\icons8-cancel.png"));
            }
        }

        private void doWorkMethod(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (!e.Cancel)
                {

                    updateRTCValues();

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTCDisplay::doworkmethod-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        private void runWorkerCompletedMethod(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC Display::runworkercompleted-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }    
}
