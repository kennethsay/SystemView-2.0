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
using System.Collections.ObjectModel;
using System.ComponentModel;
using AppLogic;
using System.Dynamic;
using System.Threading;
using SystemView.ContentDisplays;

using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace SystemView.ContentDisplays
{
    
    /// <summary>
    /// Interaction logic for DataPresentation.xaml
    /// </summary>
    public partial class DataPresentation : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<DataItem> _dataBind;
        private ObservableCollection<TPDataItem> _tpDataBind;
        private DataExtensions ExtendedData;
        private RTM _myRTM;
        private bool _cancelRTM;
        private UInt32 RTMIndex;
        private DataPresentationTriggers TriggerList;
        private DataPresentationAdvancedTriggers AdvancedTriggers;
        private DataPresentationDisplays DisplayList;
        public RadioData _RadioWin;
        private TransponderData _TPWin;
        private bool userRTMPause;
        private TagList Result;
        private TagList Previous;
        private RadioMessages myRadioMsgs;
        public DataPresentation activePresent;

        // needed for TRACKLIMIT
        private int lastDash = 0;
        private int tempDash = 0;
        private int tempTrackSpeed = 0;
        private int altDash = 0;
        private int initDDFlag = 0;

        private RadioDataItem _selectedItem;
        public RadioDataItem SelectedRadioItem
        {
            get
            {
                return this._selectedItem;
            }
            set
            {
                this._selectedItem = value;
                _RadioWin.ProcessDetailDisplayOutput((this._selectedItem.GetMessage()));
                OnPropertyChanged("SelectedItem");
            }
        }

        private TPDataItem _selectedTPItem;
        public TPDataItem SelectedTPItem
        {
            get
            {
                return this._selectedTPItem;
            }
            set
            {
                this._selectedTPItem = value;
                _TPWin.ProcessDetailDisplayOutput((this._selectedTPItem).GetMessage());
                OnPropertyChanged("SelectedTPItem");
            }
        }

        private ObservableCollection<RadioDataItem> _radioMessageList;
        public ObservableCollection<RadioDataItem> RadioMessageList
        {
            get
            {
                return this._radioMessageList;
            }
            set
            {
                this._radioMessageList = value;
                OnPropertyChanged("RadioMessageList");
            }
        }

        public ObservableCollection<DataItem> DataBind
        {
            get
            {
                return this._dataBind;
            }
            set
            {
                if (this._dataBind != value)
                {
                   this. _dataBind = value;                   
                }
            }
        }
                

        public ObservableCollection<TPDataItem> TPDataBind
        {
            get
            {
                return this._tpDataBind;
            }
            set
            {
                if (this._tpDataBind != value)
                {
                    this._tpDataBind = value;
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }


        public DataPresentation()
        {

            DataBind = new ObservableCollection<DataItem>();
            TPDataBind = new ObservableCollection<TPDataItem>();
            TriggerList = new DataPresentationTriggers();
            AdvancedTriggers = new DataPresentationAdvancedTriggers();
            DisplayList = new DataPresentationDisplays();
            _RadioWin = new RadioData();
            _TPWin = new TransponderData();
            myRadioMsgs = new RadioMessages();
            ExtendedData = new DataExtensions();

            RadioMessageList = new ObservableCollection<RadioDataItem>();

            InitializeComponent();

            BindSearchData();

            this.DataContext = this;

            _cancelRTM = false;
            userRTMPause = false;
            RTM_Play.IsEnabled = false;

            RTMIndex = 0;

            activePresent = this;

            SystemView.MainWindow._appWindow.IOToggle.IsEnabled = true;
            this.Unloaded += UnloadedEvent;

            // needed for TRACKLIMIT
            lastDash = 0;
            tempDash = 0;
            tempTrackSpeed = 0;
            altDash = 0;
            initDDFlag = 0;

            startRTM();    
        }        

        private void UnloadedEvent(object sender, RoutedEventArgs e)
        {
            _cancelRTM = true;
            byte[] ClearBuffer = new byte[4096];

            PTEConnection.Comm.GetAsyncClient.Receive(ClearBuffer);
            SystemView.MainWindow._appWindow.IOToggle.IsEnabled = false;
        }

        private void startRTM()
        {
            try
            {
                _myRTM = new RTM();

                if (_myRTM.RTMStarted)
                {                    
                    BackgroundWorker _worker = new BackgroundWorker();
                    _worker.WorkerSupportsCancellation = true;
                    _worker.DoWork += doWorkMethod;
                    _worker.RunWorkerCompleted += runWorkerCompletedMethod;
                    _worker.RunWorkerAsync();                    
                }
            }            
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPresentation::startRTM-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        private void doWorkMethod(object sender, DoWorkEventArgs e)
        {
            try
            {
                Result = new TagList();
                Previous = new TagList();
                bool AdvancedUpdate = false;

                while (!e.Cancel)
                {
                    if (!userRTMPause)
                    {
                        Result = _myRTM.CollectRecord();
                        List<Byte> UpdatedTags = Result.CompareDifferences(Previous);   
                        if (UpdatedTags.Count > 0)
                        {
                            RTMIndex++;
                        }

                        dynamic RTMData = new DataItem();

                        AdvancedUpdate = updateAdvancedTriggers(UpdatedTags);

                        updateRadio(UpdatedTags);
                        updateTP(UpdatedTags);

                        updateData(UpdatedTags, RTMData);                            
                        

                        Previous.copyData(Result);                        

                        if (_cancelRTM)
                        {
                            e.Cancel = true;
                        }
                    }                    

                    Thread.Sleep(250);
                }                
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred

                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPresentation::doworkmethod-threw exception {0}", ex.ToString()));

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
                sb.Append(String.Format("DataPresentation::runworkercompleted-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        private void updateIO()
        {
            foreach(ExtendedTag Tag in Result.Tags)
            {
                ExtendedData.fillExtraData(Tag.BaseTag, Tag.Offset, Result);
            }
        }

        private void updateRadio(List<Byte> Tags)
        {
            try
            {
                if (Tags.Contains(Globals.RadioMsgTagIndex))                            
                {
                    MTA_RR_Radio_Message RR_Radio_Message = new MTA_RR_Radio_Message();

                    RR_Radio_Message = myRadioMsgs.ProcessMessage(Result, RTMIndex);

                    dynamic DataItem = new RadioDataItem(RR_Radio_Message);

                    RadioMessageList.Insert(0, DataItem);
                }
            }
            catch (Exception ex)
            {

            }
        }        

        private void updateTP(List<Byte> Tags)
        {
            try
            {
                if (Tags.Contains(Globals.TPMsgTagIndex))
                {
                    TPMessage myTPMsg = new TPMessage(RTMIndex);
                    myTPMsg.ProcessMessage(Result);

                    dynamic TPData = new TPDataItem(myTPMsg);

                    TPDataBind.Insert(0, TPData);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void updateData(List<byte> updated, DataItem Data)
        {
            try
            {
                Data["Index"] = RTMIndex;

                foreach (var tags in Result.Tags)
                {
                    if (DisplayList.GetDisplays().Contains(tags.TagID))
                    {
                        if (tags.Extended)
                        {
                            Data[tags.Name] = ExtendedData.fillExtraData((tags as ExtendedTag).BaseTag, (tags as ExtendedTag).Offset, Result);
                        }
                        else
                        {
                            // these parameters must be processed/formatted
                            if (tags.TagID == 1 | tags.TagID == 2 | tags.TagID == 21 | tags.TagID == 23 | tags.TagID == 39)
                            {
                                Data[tags.Name] = dataNotRaw(tags.TagID, tags);
                            }

                            else
                            {
                                Data[tags.Name] = tags.ValueToString();
                            }
                        }                        
                    }                   
                }

                if (!updateDisplayList(updated))
                {
                    if (DataBind.Count == 0)
                    {
                        DataBind.Insert(0, Data);
                    }
                    else
                    {
                        DataBind[0] = Data;
                    }
                }
                else
                {
                    DataBind.Insert(0, Data);
                }                
            }
            catch (Exception ex)
            {

            }
        }

        private string dataNotRaw(int tagID, Tag tag)
        {
            switch (tagID)
            {
                case 1:
                    return getMilePost(tag);

                case 2:
                    return getChainage(tag);

                case 21:
                    return getTrackLimit(tag);

                case 23:
                    return getTrainType(tag);

                case 39:
                    return getSignalStatus(tag);

                // should never happen
                default:
                    return "error in DataPlaybackPresentation.xaml.cs : method dataNotRaw";
            }
        }

        private string getMilePost(Tag tag)
        {
            float loc = (float)Datalog.bytesToInt(tag.Data(), 2) / 176;
            return (Math.Truncate(loc * 10) / 10).ToString();
        }

        private string getChainage(Tag tag)
        {
            int chainage = Datalog.bytesToInt(tag.Data(), 2) * 10;
            return chainage.ToString();
        }

        private string getTrackLimit(Tag tag)
        {
            byte[] dashBytes = Result.Tags.Find(X => X.TagID == 30).Data();
            byte dashByte = dashBytes[0];
            bool dash;
            bool tsrListOK;


            int trackSpeed = Datalog.bytesToInt(tag.Data(), 1);
            bool mbs;
            byte[] mbsBytes = Result.Tags.Find(X => X.TagID == 78).Data();

            if ((byte)(mbsBytes[1] & 0x04) == 0x04)
            {
                mbs = true;
            }
            else
            {
                mbs = false;
            }

            dashByte = (byte)(dashByte & 0x80);

            if (dashByte == 0x80)
            {
                dash = true;
            }
            else
            {
                dash = false;
            }

            byte[] tsrListBytes = Result.Tags.Find(X => X.TagID == 38).Data();
            byte tsrListByte = tsrListBytes[0];

            tsrListByte = (byte)((tsrListByte >> 7) & 0x01);

            if (tsrListByte == 0x01)
            {
                tsrListOK = false;
            }
            else
            {
                tsrListOK = true;
            }

            if ((byte)(tsrListBytes[0] & 0x3f) > 0)
            {
                altDash = 0;
            }
            else
            {
                altDash = 1;
            }

            if (dash)
            {
                if ((!tsrListOK) & (altDash > 0))
                {
                    if (mbs & trackSpeed != 0)
                    {
                        if (lastDash == 0)
                        {
                            lastDash = 1;
                        }

                        return "--";
                    }

                    else if (mbs & trackSpeed == 0)
                    {
                        lastDash = 0;
                        return trackSpeed.ToString();
                    }

                    else
                    {
                        if (tempDash == 0)
                        {
                            tempDash = 1;
                            lastDash = 0;
                            return "--";
                        }

                        else
                        {
                            tempDash = 0;
                            lastDash = 0;
                            initDDFlag = 1;
                            return trackSpeed.ToString();
                        }
                    }
                }

                else if (trackSpeed != 0)
                {
                    tempDash = 1;
                    lastDash = 1;
                    return "--";
                }

                else
                {
                    tempDash = 0;
                    lastDash = 0;
                    return trackSpeed.ToString();
                }
            }

            else if (tempTrackSpeed != trackSpeed | (tsrListOK & lastDash == 1))
            {
                tempTrackSpeed = trackSpeed;
                lastDash = 0;
                initDDFlag = 1;
                return trackSpeed.ToString();
            }

            else if (tsrListOK & lastDash == 0 & initDDFlag == 0)
            {
                tempTrackSpeed = trackSpeed;
                lastDash = 0;
                initDDFlag = 1;
                return trackSpeed.ToString();
            }

            else
            {
                tempTrackSpeed = trackSpeed;
                lastDash = 0;
                return trackSpeed.ToString();
            }
        }

        private string getTrainType(Tag tag)
        {
            int trainType = Datalog.bytesToInt(tag.Data(), 1);

            if (trainType == 0)
            {
                return "0";
            }
            else if (trainType == 1)
            {
                return "A";
            }
            else if (trainType == 2)
            {
                return "B";
            }
            else if (trainType == 3)
            {
                return "C";
            }
            else if (trainType == 4)
            {
                return "D";
            }
            else if (trainType == 5)
            {
                return "E";
            }
            else
            {
                return "x";
            }

        }

        private string getSignalStatus(Tag tag)
        {
            int sigStat = Datalog.bytesToInt(tag.Data(), 1);

            switch (sigStat)
            {
                case 0:
                    return "STOP";

                case 1:
                    return "10";

                case 2:
                    return "15";

                case 3:
                    return "20";

                case 4:
                    return "25";

                case 5:
                    return "30";

                case 6:
                    return "35";

                case 7:
                    return "40";

                case 8:
                    return "45";

                case 9:
                    return "50";

                case 10:
                    return "60";

                case 11:
                    return "70";

                case 12:
                    return "80";

                case 13:
                    return "NA";

                case 14:
                    return "NA";

                case 15:
                    return "STOP";

                default:
                    return "error in DataPlaybackPresentation.xaml.cs : method getSignalStatus";
            }
        }

        private bool updateDisplayList(List<Byte> Tags)
        {
            bool rc = false;
            try
            {
                foreach (byte tagid in DisplayList.GetDisplays())
                {
                    if (TriggerList.GetTriggers().Contains(tagid))
                    {
                        if (Tags.Contains(tagid))
                        {
                            rc = true;
                        }
                    }
                }
            }
            catch
            {
                rc = false;
            }
            return rc;
        }

        private bool updateAdvancedTriggers(List<Byte> Tags)
        {
            bool rc = false;
            try
            {
                foreach (byte tagid in DisplayList.GetDisplays())
                {
                    if (AdvancedTriggers.GetTriggers().Contains(tagid))
                    {
                        if (Tags.Contains(tagid))
                        {
                            rc = true;
                        }
                    }
                }
            }
            catch
            {
                rc = false;
            }
            return rc;
        }

        private void BindSearchData()
        {
            try
            {
                this.PresentationGrid.ShowSearchPanel = false;
                var searchPanel = this.PresentationGrid.ChildrenOfType<GridViewSearchPanel>().FirstOrDefault();
                //this.SearchBox.SetBinding(TextBox.TextProperty, new Binding("SearchText") { Source = searchPanel.DataContext, Mode = BindingMode.TwoWay });                   
                
            }
            catch
            {

            }
            
        }

        private void closeRTMSession()
        {
            _cancelRTM = true;
        }

        /// <summary>
        /// Displays Data Presentation Window Trigger Dialog that allows user to add or remove active displays to/from the window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void modifyDisplay(object sender, RoutedEventArgs e)
        {
            try
            {
                Window displayWindow = new Window
                {
                    Title = "Active Displays",
                    Content = DisplayList,
                    Width = 300,
                    Height = 400
                };

                displayWindow.Owner = SystemView.MainWindow._appWindow;
                SystemView.MainWindow._appWindow.IsEnabled = true;
                displayWindow.Show();
            }
            catch
            {

            }            
        }

        /// <summary>
        /// Displays Data Presentation Window Trigger Dialog that allows user to add or remove active triggers to/from the window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modifyTriggers(object sender, RoutedEventArgs e)
        {
            try
            {
                Window triggerWindow = new Window
                {
                    Title = "Active Triggers",
                    Content = TriggerList,
                    Width = 300,
                    Height = 400
                };

                triggerWindow.Owner = SystemView.MainWindow._appWindow;
                triggerWindow.Show();
            }
            catch
            {

            }
        }

        private void modifyAdanvancedTriggers(object sender, RoutedEventArgs e)
        {
            try
            {
                Window advancedTriggers = new Window
                {
                    Title = "Advanced Triggers",
                    Content = AdvancedTriggers,
                    Width = 300,
                    Height = 400
                };
                advancedTriggers.Owner = SystemView.MainWindow._appWindow;
                advancedTriggers.Show();
            }
            catch
            {

            }
        }

        private void displayRadio_Window(object sender, RoutedEventArgs e)
        {
            try
            {
                Window radioWindow = new Window
                {
                    Title = "Radio Data Viewer",
                    Content = _RadioWin,
                    Width = 1200,
                    Height = 400
                };

                radioWindow.DataContext = this;
                radioWindow.Owner = SystemView.MainWindow._appWindow;
                radioWindow.Show();
            }
            catch
            {

            }
        }

        private void displayTP_Window(object sender, RoutedEventArgs e)
        {
            try
            {
                Window tpWindow = new Window
                {
                    Title = "Transponder Data Viewer",
                    Content = _TPWin,
                    Width = 1200,
                    Height = 400
                };

                tpWindow.DataContext = this;
                tpWindow.Owner = SystemView.MainWindow._appWindow;
                tpWindow.Show();
            }
            catch (Exception ex)
            {

            }
        }

        public void PauseRTM(object sender, RoutedEventArgs e)
        {
            userRTMPause = true;
            RTM_Play.IsEnabled = true;
            RTM_Pause.IsEnabled = false;
        }
        public void ResumeRTM(object sender, RoutedEventArgs e)
        {
            userRTMPause = false;
            RTM_Play.IsEnabled = false;
            RTM_Pause.IsEnabled = true;
        }

        public void ClearRTM(object sender, RoutedEventArgs e)
        {
            RTMIndex = 0;
            this._dataBind.Clear();
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
    }
}
