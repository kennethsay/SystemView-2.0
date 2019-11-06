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
using AppLogic;

namespace SystemView.ContentDisplays
{

    /// <summary>
    /// Interaction logic for DataPlaybackPresentation.xaml
    /// </summary>
    public partial class DataPlaybackPresentation : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<DataItem> _dataBind;
        private ObservableCollection<TPDataItem> _tpDataBind;
        private DataExtensions ExtendedData;
        private bool _cancelPlayback;
        private UInt32 playbackIndex;
        private DataPlayback _myPlayback;
        private DataPresentationTriggers TriggerList;
        private DataPresentationAdvancedTriggers AdvancedTriggers;
        private DataPresentationDisplays DisplayList;
        private RadioData _RadioWin;
        private TransponderData _TPWin;
        private bool pausePlayback;
        private TagList Result;
        private TagList Previous;
        private RadioMessages myRadioMsgs;
        public DataPlaybackPresentation activePresent;

        // needed for TRACKLIMIT
        private int _lastDash;
        private int _tempDash;
        private int _tempTrackSpeed;
        private int _altDash;
        private int _initDDFlag;

        private int currentEventNum;

        // used to signal display of first record on startup
        private bool _init;

        private bool newFile;

        private static int _playbackBCPnum;



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
                    this._dataBind = value;
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

        public static int PlaybackBCPNum
        {
            get { return _playbackBCPnum; }
            set
            {
                _playbackBCPnum = value;
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


        public DataPlaybackPresentation()
        {

            DataBind = new ObservableCollection<DataItem>();
            TPDataBind = new ObservableCollection<TPDataItem>();
            RadioMessageList = new ObservableCollection<RadioDataItem>();
            TriggerList = new DataPresentationTriggers();
            AdvancedTriggers = new DataPresentationAdvancedTriggers();
            DisplayList = new DataPresentationDisplays();
            _RadioWin = new RadioData();
            _TPWin = new TransponderData();
            ExtendedData = new DataExtensions();

            myRadioMsgs = new RadioMessages();

            InitializeComponent();

            BindSearchData();

            this.DataContext = this;

            _cancelPlayback = false;
            pausePlayback = false;
            PlayData.IsEnabled = true;
            PauseData.IsEnabled = false;


            playbackIndex = 0;

            activePresent = this;

            this.Unloaded += UnloadedEvent;

            // needed for TRACKLIMIT
             _lastDash = 0;
             _tempDash = 0;
             _tempTrackSpeed = 0;
             _altDash = 0;
             _initDDFlag = 0;

             PlaybackBCPNum = 0;
            currentEventNum = 0;


            _init = true;
            newFile = true;

            beginPlayback();
        }

        private void UnloadedEvent(object sender, RoutedEventArgs e)
        {
            closeSession();
        }

        private void beginPlayback()
        {
            try
            {
                _myPlayback = new DataPlayback();

                if (_myPlayback.selectFile())
                {
                    openFile();
                }
            }

            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlaybackPresentation::beginPlayback-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

       /* private void newPlaybackWorker()
        {
            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += doWorkMethod;
            _worker.RunWorkerCompleted += runWorkerCompletedMethod;
            _worker.RunWorkerAsync();
        }*/

        private void openFile()
        {
            try
            {
                _myPlayback.FileToReadAndPresent();

                Result = new TagList();
                Previous = new TagList();
                // bool AdvancedUpdate = false;

                Thread.Sleep(60);

                _myPlayback.queueOneRecord(currentEventNum);
                currentEventNum++;

                Thread.Sleep(60);

                showRecord();

                /* while (!e.Cancel)
                 {
                     if (!pausePlayback)
                     {
                         showRecord();
                     }

                     Thread.Sleep(10);
                 }*/
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlaybackPresentation::doworkmethod-threw exception {0}", ex.ToString()));

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
                sb.Append(String.Format("DataPlaybackPresentation::runworkercompleted-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        private void showRecord()
        {
            bool gotList = _myPlayback.TagListQueue.TryDequeue(out Result);

            while (!gotList)
            {
                Thread.Sleep(1);
                gotList = _myPlayback.TagListQueue.TryDequeue(out Result);
            }

            List<Byte> UpdatedTags = Previous.CompareDifferences(Result);

            dynamic tagData = new DataItem();


            //AdvancedUpdate = updateAdvancedTriggers(UpdatedTags);


            updateRadio(UpdatedTags, Result);
            updateTP(UpdatedTags, Result);
            updateData(UpdatedTags, tagData);


            /*
            if (AdvancedUpdate)
            {
                int advancedData = 11;
                while (advancedData > 0)
                {
                    Previous.copyData(Result);

                    Thread.Sleep(20);

                    _myPlayback.TagListQueue.TryDequeue(out Result);

                    tagData = new DataItem();

                    updateData(tagData);
                    advancedData--;
                }
                AdvancedUpdate = false;
            }*/


            Previous.copyData(Result);

            if (_cancelPlayback)
            {
                //e.Cancel = true;
            }

            // on startup, display only the first record, then pause
            if (_init)
            {
                _init = false;
                pausePlayback = true;
            }
        }

        private void updateRadio(List<Byte> Tags, TagList thisList)
        {
            try
            {
                if (Tags.Contains(Globals.RadioMsgTagIndex))
                {
                    MTA_RR_Radio_Message RR_Radio_Message;

                    RR_Radio_Message = myRadioMsgs.ProcessMessage(thisList, playbackIndex);

                    dynamic DataItem = new RadioDataItem(RR_Radio_Message);

                    RadioMessageList.Insert(0, DataItem);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CopyDifferences(List<byte> TagDiff)
        {
            foreach(byte b in TagDiff)
            {
                if (Result.GetTag(b).HasData)
                {
                    Previous.GetTag(b).Data(Result.GetTag(b).Data());
                }                
            }
        }

        private void updateTP(List<Byte> Tags, TagList thisList)
        {
            try
            {
                if (Tags.Contains(Globals.TPMsgTagIndex))
                {
                    TPMessage myTPMsg = new TPMessage(playbackIndex);
                    myTPMsg.ProcessMessage(thisList);

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
                Data["Index"] = currentEventNum;
               // playbackIndex++;

                foreach (var tags in Result.Tags)
                {
                    if (DisplayList.GetDisplays().Contains(tags.TagID))
                    {
                        if (tags.Extended)
                        {
                           
                            //if (tags.HasData)
                           // {
                                
                                Data[tags.Name] = ExtendedData.fillExtraData((tags as ExtendedTag).BaseTag, (tags as ExtendedTag).Offset, Result);
                            //}
                        }

                        else
                        {
                            //if (tags.HasData)
                          //  {
                                // these parameters must be processed/formatted
                                if (tags.TagID == 1 | tags.TagID == 2 | tags.TagID == 21 | tags.TagID == 23 | tags.TagID == 39)
                                {
                                    Data[tags.Name] = dataNotRaw(tags.TagID, tags); 
                                }
                                
                                else
                                {
                                    Data[tags.Name] = tags.ValueToString();
                                }   
                          //  }
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
                _altDash = 0;
            }
            else
            {
                _altDash = 1;
            }

            if (dash)
            {
                if ((!tsrListOK) & (_altDash > 0))
                {
                    if (mbs & trackSpeed != 0)
                    {
                        if (_lastDash == 0)
                        {
                            _lastDash = 1;
                        }

                        return "--";
                    }

                    else if (mbs & trackSpeed == 0)
                    {
                        _lastDash = 0;
                        return trackSpeed.ToString();
                    }

                    else
                    {
                        if (_tempDash == 0)
                        {
                            _tempDash = 1;
                            _lastDash = 0;
                            return "--";
                        }

                        else
                        {
                            _tempDash = 0;
                            _lastDash = 0;
                            _initDDFlag = 1;
                            return trackSpeed.ToString();
                        }
                    }
                }

                else if (trackSpeed != 0)
                {
                    _tempDash = 1;
                    _lastDash = 1;
                    return "--";
                }

                else
                {
                    _tempDash = 0;
                    _lastDash = 0;
                    return trackSpeed.ToString();
                }
            }

            else if (_tempTrackSpeed != trackSpeed | (tsrListOK & _lastDash == 1))
            {
                _tempTrackSpeed = trackSpeed;
                _lastDash = 0;
                _initDDFlag = 1;
                return trackSpeed.ToString();
            }

            else if (tsrListOK & _lastDash == 0 & _initDDFlag == 0)
            {
                _tempTrackSpeed = trackSpeed;
                _lastDash = 0;
                _initDDFlag = 1;
                return trackSpeed.ToString();
            }

            else
            {
                _tempTrackSpeed = trackSpeed;
                _lastDash = 0;
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

            switch(sigStat)
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
                this.SearchBox.SetBinding(TextBox.TextProperty, new Binding("SearchText") { Source = searchPanel.DataContext, Mode = BindingMode.TwoWay });
            }
            catch
            {

            }
        }

        private void closeSession()
        {
            _cancelPlayback = true;
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

        /// <summary>
        /// Displays Data Presentation Window Trigger Dialog that allows user to add or remove active triggers to/from the window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        public void Pause(object sender, RoutedEventArgs e)
        {
            pausePlayback = true;
            PlayData.IsEnabled = true;
            PauseData.IsEnabled = false;
        }
        public void Resume(object sender, RoutedEventArgs e)
        {
            pausePlayback = false;
            PlayData.IsEnabled = false;
            PauseData.IsEnabled = true;

            BackgroundWorker playworker = new BackgroundWorker();
            playworker.WorkerSupportsCancellation = true;
            playworker.DoWork += resumeDoWork;
            playworker.RunWorkerCompleted += runWorkerCompletedMethod;
            playworker.RunWorkerAsync();
        }

        private void resumeDoWork(object sender, DoWorkEventArgs e)
        {
           // currentEventNum = 100000;
            while (currentEventNum < _myPlayback.NumEvents & !pausePlayback)
            {  
                _myPlayback.queueOneRecord(currentEventNum);

                Thread.Sleep(60);

                showRecord();

                currentEventNum++;
            }
        }

        public void ClearPlayback(object sender, RoutedEventArgs e)
        {
            this._dataBind.Clear();
            playbackIndex = 0;
            _init = true;
            pausePlayback = false;
            //newPlaybackWorker();
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
