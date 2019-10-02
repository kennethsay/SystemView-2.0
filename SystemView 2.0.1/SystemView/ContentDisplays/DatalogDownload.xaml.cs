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
using System.Windows.Forms;
using Telerik.Windows.Controls;
using System.IO;
using System.Threading;

using Telerik.Windows.Input;

using AppLogic;
using UserControl = System.Windows.Controls.UserControl;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for DatalogDownload.xaml
    /// </summary>
    public partial class DatalogDownload : UserControl, INotifyPropertyChanged
    {
        Datalog myDatalogDownload;
        Datalog.DATALOG_TIME_SELECTION _selectedDownloadType;
        object DownloadObject;
        RadSaveFileDialog saveDialog;
        StringBuilder DefaultFileName;
        bool DLCancel;


        private string _savePath;
        public string SavePath
        {
            get
            {
                return _savePath;
            }
            set
            {
                _savePath = value;
                OnPropertyChanged("SavePath");
            }
        }


        public DatalogDownload()
        {
            InitializeComponent();

            this.DataContext = this;

            //Initialize the download manager
            this.myDatalogDownload = new Datalog();

            this.myDatalogDownload.CanUploadCSV = EmployeeLogin.UserAuthenticated;

            //Display the default download option screen
            this.DownloadObject = new DownloadDatalogAllAvailable();
            DownloadTypeContent.Content = this.DownloadObject;

            this.CancelBtn.IsEnabled = false;

            SavePath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

            saveDialog = new RadSaveFileDialog();
            saveDialog.Owner = SystemView.MainWindow._appWindow;

            DLCancel = false;

            setIntialSavePath();

        }

        private void setIntialSavePath()
        {
            DateTime DTNOW = DateTime.Now;

            DefaultFileName = new StringBuilder();
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DefaultFileName.Append(string.Format("{0}\\ACSES-{1}", Path, DTNOW.ToString("MM-dd-yyyy_hh-mm")));
            SavePath = DefaultFileName.ToString();
        }

        public void ChangedDownloadParameter(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (DatalogDownloadType.SelectedIndex)
                {
                    case 0:
                        this.DownloadObject = new DownloadDatalogAllAvailable();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.ALL_AVAILABLE;
                        break;

                    case 1:
                        this.DownloadObject = new DataLogDownloadByTime();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.RECENT_BY_TIME;
                        break;

                    case 2:
                        this.DownloadObject = new DatalogDownloadBySize();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.RECENT_BY_SIZE;
                        break;

                    case 3:
                        this.DownloadObject = new DatalogDownloadTimeRange();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.SELECT_RANGE;
                        break;

                    default:
                        break;
                }

                DownloadTypeContent.Content = this.DownloadObject;
            }
            catch (Exception ex)
            {

            }
        }

        private void updateDownloadSelection()
        {
            try
            {
                switch (DatalogDownloadType.SelectedIndex)
                {
                    case 0:
                        this.DownloadObject = new DownloadDatalogAllAvailable();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.ALL_AVAILABLE;
                        break;

                    case 1:
                        this.DownloadObject = new DataLogDownloadByTime();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.RECENT_BY_TIME;
                        break;

                    case 2:
                        this.DownloadObject = new DatalogDownloadBySize();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.RECENT_BY_SIZE;
                        break;

                    case 3:
                        this.DownloadObject = new DatalogDownloadTimeRange();
                        this._selectedDownloadType = Datalog.DATALOG_TIME_SELECTION.SELECT_RANGE;
                        break;

                    default:
                        break;
                }

                DownloadTypeContent.Content = this.DownloadObject;
            }
            catch (Exception ex)
            {

            }
        }

        public void BeginSelectedDownload(object sender, RoutedEventArgs e)
        {
            try
            {
                myDatalogDownload._userSelection = this._selectedDownloadType;

                switch (this._selectedDownloadType)
                {
                    case Datalog.DATALOG_TIME_SELECTION.SELECT_RANGE:
                        myDatalogDownload.StartDT = (this.DownloadObject as DatalogDownloadTimeRange).StartDateTime;
                        myDatalogDownload.EndDT = (this.DownloadObject as DatalogDownloadTimeRange).EndDateTime;
                        break;

                    case Datalog.DATALOG_TIME_SELECTION.ALL_AVAILABLE:
                        break;

                    case Datalog.DATALOG_TIME_SELECTION.RECENT_BY_SIZE:
                        myDatalogDownload._percentBlockUpload = (this.DownloadObject as DatalogDownloadBySize).DownloadPercentage;
                        break;

                    case Datalog.DATALOG_TIME_SELECTION.RECENT_BY_TIME:
                        myDatalogDownload._userTimeSelected = (this.DownloadObject as DataLogDownloadByTime).SelectedTimeInterval;
                        break;

                    default:
                        break;
                }

                if (this.DownloadObject.GetType() == typeof(DatalogDownloadTimeRange))
                {
                    if ((this.DownloadObject as DatalogDownloadTimeRange).Error != null)
                    {
                        RadWindow.Alert((this.DownloadObject as DatalogDownloadTimeRange).Error);
                    }
                    else
                    {
                        StartDownload();
                    }
                }
                else
                {
                    StartDownload();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void StartDownload()
        {
            DLCancel = false;

            myDatalogDownload.SetFileName = SavePath;

            this.DownloadButton.IsEnabled = false;
            this.CancelBtn.IsEnabled = true;

            this.DownloadObject = new DatalogDownloadInProgress();
            DownloadTypeContent.Content = this.DownloadObject;
            myDatalogDownload.CreateExcelFile(SavePath);

            myDatalogDownload.UserDownloadParameters();

            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += datalogWorker_doWork;
            _worker.RunWorkerCompleted += datalogWorkerComplete;
            _worker.ProgressChanged += datalogWorkerProgressChanged;
            _worker.RunWorkerAsync();            
        }

        private void datalogWorker_doWork(object sender, DoWorkEventArgs e)
        {
            while(!e.Cancel)
            {
                e.Cancel = myDatalogDownload.writeToDatalogFile(sender as BackgroundWorker);      
                
                if (DLCancel)
                {
                    e.Cancel = true;
                }
            }            
        }

        public void datalogWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            (DownloadObject as DatalogDownloadInProgress).DownloadProgress.Value = (int)(e.UserState);
        }


        private void datalogWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DownloadObject = new DatalogDownloadCompleted();
            DownloadTypeContent.Content = this.DownloadObject;

            this.DownloadButton.IsEnabled = true;
            this.CancelBtn.IsEnabled = false;

            Thread.Sleep(3000);

            DatalogDownloadType.SelectedIndex = 4;


        }



        public void selectSavePath(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog FileDialog = new SaveFileDialog();
                FileDialog.RestoreDirectory = true;
                FileDialog.FileName = DefaultFileName.ToString();
                FileDialog.Filter = "SDF Files (*.sdf)|*.sdf";
                FileDialog.ShowDialog();

                if (FileDialog.FileName != "")
                {
                    SavePath = FileDialog.FileName.ToString();
                }
            }
            catch
            {

            }

        }

        /// <summary>
        /// INotifyPropertyChanged handler and Methods
        /// </summary>

        #region Property Changed Manager

        // The property changed event handler is necessary for the implementation of the INotifyPropertyChanged class. This class handles automatically 
        // variable value updates from the model to the view. The OnPropertyChanged function alerts the display manager of a change in a bound variable 
        // value and causes a MainWindow update to reflect the changes. 
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DLCancel = true;
        }
    }
}
