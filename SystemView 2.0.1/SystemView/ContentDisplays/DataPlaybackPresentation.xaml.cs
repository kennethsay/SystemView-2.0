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
                _RadioWin.ProcessDetailDisplayOutput((this._selectedItem.GetMessage() as MTA_RR_Radio_Message).Data);
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
                _TPWin.ProcessDetailDisplayOutput((this._selectedTPItem as TPDataItem).Message.Data);
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
            TriggerList = new DataPresentationTriggers();
            AdvancedTriggers = new DataPresentationAdvancedTriggers();
            DisplayList = new DataPresentationDisplays();
            _RadioWin = new RadioData();
            _TPWin = new TransponderData();
            ExtendedData = new DataExtensions();

            RadioMessageList = new ObservableCollection<RadioDataItem>();

            myRadioMsgs = new RadioMessages();

            InitializeComponent();

            BindSearchData();

            this.DataContext = this;

            _cancelPlayback = false;
            pausePlayback = false;
            PlayData.IsEnabled = false;

            playbackIndex = 0;

            activePresent = this;

            beginPlayback();
        }

        private void beginPlayback()
        {
            try
            {
                _myPlayback = new DataPlayback();

                if (_myPlayback.selectFile())
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
                sb.Append(String.Format("DataPlaybackPresentation::beginPlayback-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        private void doWorkMethod(object sender, DoWorkEventArgs e)
        {
            try
            {
                _myPlayback.FileToReadAndPresent();

                Result = new TagList();
                Previous = new TagList();
                // bool AdvancedUpdate = false;

                Thread.Sleep(60);

                while (!e.Cancel)
                {
                    if (!pausePlayback)
                    {
                        bool gotList = _myPlayback.TagListQueue.TryDequeue(out Result);

                        while(!gotList)
                        {
                            Thread.Sleep(10);
                            gotList = _myPlayback.TagListQueue.TryDequeue(out Result);
                        }

                        List<Byte> UpdatedTags = Previous.CompareDifferences(Result);
                        bool ForceUpdate = false;

                        dynamic tagData = new DataItem();

                        ForceUpdate = updateDisplayList(UpdatedTags);
                        //AdvancedUpdate = updateAdvancedTriggers(UpdatedTags);
                        

                        updateRadio(UpdatedTags, Result);
                        updateTP(UpdatedTags, Result);

                        if (ForceUpdate) // || AdvancedUpdate
                        {
                            Thread.Sleep(5);
                            updateData(tagData);

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
                        }

                        Previous.copyData(Result);

                        if (_cancelPlayback)
                        {
                            e.Cancel = true;
                        }
                    }

                    Thread.Sleep(10);
                }
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
                List<StringBuilder> HeaderRecords = new List<StringBuilder>();
                HeaderRecords = _myPlayback.datHeader;

                Console.WriteLine(HeaderRecords);
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlaybackPresentation::runworkercompleted-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
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

        private void updateData(DataItem Data)
        {
            try
            {
                Data["Index"] = playbackIndex;
                playbackIndex++;

                foreach (var tags in Result.Tags)
                {
                    if (DisplayList.GetDisplays().Contains(tags.TagID))
                    {
                        if (tags.Extended)
                        {
                            if (tags.HasData)
                            {
                                Data[tags.Name] = ExtendedData.fillExtraData((tags as ExtendedTag).BaseTag, (tags as ExtendedTag).Offset, Result);
                            }
                        }
                        else
                        {
                            if (tags.HasData)
                            {
                                Data[tags.Name] = tags.ValueToString();
                            }
                        }
                    }
                }
                DataBind.Insert(0, Data);
            }
            catch (Exception ex)
            {

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
        }

        public void ClearPlayback(object sender, RoutedEventArgs e)
        {
            playbackIndex = 0;
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
