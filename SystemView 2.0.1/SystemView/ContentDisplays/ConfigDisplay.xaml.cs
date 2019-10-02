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
using AppLogic;
using System.Threading;
using Telerik.Windows.Controls;
using System.ComponentModel;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for ConfigDisplay.xaml
    /// </summary>
    public partial class ConfigDisplay : UserControl, INotifyPropertyChanged
    {        

        private Config _myConfig;
        private Config.CONFIG_CHANGE_STATE _eState;
        MainWindow AppWindow;
        bool InitComplete = false;
        int TempFeatureSet1;
        int TempFeatureSet2;
        int TempFeatureSet3;

        #region Config Properties

        private int _railroad;
        public int Railroad
        {
            get
            {
                return _railroad;
            }             
            set
            {
                _railroad = value;
                UpdateRailroadDependentItems();
                ParameterChanged();
                OnPropertyChanged("Railroad");
            }
        }

        private int _trainType;
        public int TrainType
        {
            get
            {
                return _trainType;
            }
            set
            {
                _trainType = value;
                ParameterChanged();
                OnPropertyChanged("TrainType");
            }
        }

        private int _vehicleType;
        public int VehicleType
        {
            get
            {
                return _vehicleType;
            }
            set
            {
                _vehicleType = value;
                ParameterChanged();
                OnPropertyChanged("VehicleType");
            }
        }

        private int _vehicleNumber;
        public int VehicleNumber
        {
            get
            {
                return _vehicleNumber;
            }
            set
            {
                _vehicleNumber = value;
                ParameterChanged();
                OnPropertyChanged("VehicleNumber");
            }
        }

        private int _vehicleSpeed;
        public int VehicleSpeed
        {
            get
            {
                return _vehicleSpeed;
            }
            set
            {
                _vehicleSpeed = value;
                ParameterChanged();
                OnPropertyChanged("VehicleSpeed");
            }
        }

        private double _tachYPP;
        public double TachYPP
        {
            get
            {
                return _tachYPP;
            }
            set
            {
                _tachYPP = value;
                ParameterChanged();
                OnPropertyChanged("TachYPP");
            }
        }

        private double _tachPPR;
        public double TachPPR
        {
            get
            {
                return _tachPPR;
            }
            set
            {
                _tachPPR = value;
                ParameterChanged();
                OnPropertyChanged("TachPPR");
            }
        }

        private double _wheelDiameter;
        public double WheelDiameter
        {
            get
            {
                return _wheelDiameter;
            }
            set
            {
                _wheelDiameter = value;
                ParameterChanged();
                OnPropertyChanged("WheelDiameter");
            }
        }

        private int _frontAntOffset;
        public int FrontAntOffset
        {
            get
            {
                return _frontAntOffset;
            }
            set
            {
                _frontAntOffset = value;
                ParameterChanged();
                OnPropertyChanged("FrontAntOffset");
            }
        }

        private int _rearAntOffset;
        public int RearAntOffset
        {
            get
            {
                return _rearAntOffset;
            }
            set
            {
                _rearAntOffset = value;
                ParameterChanged();
                OnPropertyChanged("RearAntOffset");
            }
        }
        private int _datalogger;
        public int Datalogger
        {
            get
            {
                return _datalogger;
            }
            set
            {
                _datalogger = value;
                ParameterChanged();
                OnPropertyChanged("Datalogger");
            }
        }
        private int _atcs;
        public int ATCS
        {
            get
            {
                return _atcs;
            }
            set
            {
                _atcs = value;
                ParameterChanged();
                OnPropertyChanged("ATCS");
            }
        }

        private int _decelDirection;
        public int DecelDirection
        {
            get
            {
                return _decelDirection;
            }
            set
            {
                _decelDirection = value;
                ParameterChanged();
                OnPropertyChanged("DecelDirection");
            }
        }

        #endregion

        public ConfigDisplay()
        {
            try
            {
                _myConfig = new Config();
                Console.WriteLine(_myConfig.ToString());

                if (_myConfig.Elements.Find(x => x.Name == "Status").Value != 0xFF)
                {
                    _myConfig.ChangeConfigPermission = true;

                    InitializeComponent();

                    this.DataContext = this;

                    createConfigDisplay();
                    _eState = Config.CONFIG_CHANGE_STATE.NO_ACTION;

                    ResetConfig();

                    UpdateConfig.IsEnabled = false;
                    CancelConfig.IsEnabled = false;

                    AppWindow = SystemView.MainWindow._appWindow;                   
                }
                else
                {
                    MessageBox.Show("Permanent Suppression and VZero must be active to configure vehicle. Check operator settings and try again.",
                                    "Config Error", MessageBoxButton.OK);
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConfigDisplay::Constructor threw exception: {0}", ex);
            }            
        }

        private void InitCheckboxItems()
        {
            foreach(string st in Globals.CONFIGCHECKBOXPARAMSMTA)
            {
                CheckBox CB = new CheckBox()
                {
                    Content = st,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                CB.Click += CheckBoxChecked;

                this.ConfigCheckBoxItems.Children.Add(CB);
            }

            readFeatureSets();
        }

        private CheckBox checkboxFromName(string st)
        {
            foreach(CheckBox CB in this.ConfigCheckBoxItems.Children)
            {
                if (CB.Content == st)
                {
                    return CB;
                }
            }
            return null;
        }

        private void readFeatureSets()
        {
            readFeatureSet1();
            readFeatureSet2();
            readFeatureSet3();
        }

        private void readFeatureSet3()
        {
            int FS3 = this._myConfig.Find("FeatureSet3").Value;
            TempFeatureSet3 = FS3;

            checkboxFromName("HST Tilt Bypass").IsChecked = intToBool(FS3, Globals.BIT0);
            checkboxFromName("DT Antenna Bypass").IsChecked = intToBool(FS3, Globals.BIT1);
            checkboxFromName("Radio DT Bypass").IsChecked = intToBool(FS3, Globals.BIT2);
            DecelDirection = ConvertDecelDirection(false, (FS3 & Globals.BIT3));
            checkboxFromName("Single Cab Vehicle").IsChecked = intToBool(FS3, Globals.BIT4);
            checkboxFromName("AIU Decel Communications").IsChecked = intToBool(FS3, Globals.BIT5);
            checkboxFromName("Reverse Operation Allowed").IsChecked = intToBool(FS3, Globals.BIT6);
            checkboxFromName("Short Hood Forward").IsChecked = intToBool(FS3, Globals.BIT7);
        }

        private void readFeatureSet2()
        {
            int FS2 = this._myConfig.Find("FeatureSet2").Value;
            TempFeatureSet2 = FS2;

            checkboxFromName("Long Hood Forward").IsChecked = intToBool(FS2, Globals.BIT0);
            checkboxFromName("Married Pair").IsChecked = intToBool(FS2, Globals.BIT1);
            checkboxFromName("BA Equipped").IsChecked = intToBool(FS2, Globals.BIT2);
            checkboxFromName("Pulse Emergency Brake").IsChecked = intToBool(FS2, Globals.BIT3);
            updateDependentCheckboxes();
            checkboxFromName("Tunnel Enable").IsChecked = intToBool(FS2, Globals.BIT4);
            checkboxFromName("3 Second Alarm Delay").IsChecked = intToBool(FS2, Globals.BIT6);
            checkboxFromName("Slip/Slide Bypass Before Crosscheck").IsChecked = intToBool(FS2, Globals.BIT7);
        }

        private void readFeatureSet1()
        {
            int FS1 = this._myConfig.Find("FeatureSet1").Value;
            TempFeatureSet1 = FS1;

            checkboxFromName("Total Slip/Slide Bypass").IsChecked = intToBool(FS1, Globals.BIT0);
            checkboxFromName("2 Second Emergency Brake Pulse").IsChecked = intToBool(FS1, Globals.BIT1);
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            updateTempFeatureSets();
            this.CancelConfig.IsEnabled = true;
            this.UpdateConfig.IsEnabled = true;
        }

        private void updateTempFeatureSets()
        {
            updateTempFeatureSet1();
            updateTempFeatureSet2();
            updateTempFeatureSet3();
        }

        private void updateTempFeatureSet3()
        {
            int rc = 0;

            rc += checkBoxCheckedToInt(checkboxFromName("HST Tilt Bypass"), Globals.BIT0);
            rc += checkBoxCheckedToInt(checkboxFromName("DT Antenna Bypass"), Globals.BIT1);
            rc += checkBoxCheckedToInt(checkboxFromName("Radio DT Bypass"), Globals.BIT2);
            rc += ConvertDecelDirection(true, DecelDirection);
            rc += checkBoxCheckedToInt(checkboxFromName("Single Cab Vehicle"), Globals.BIT4);
            rc += checkBoxCheckedToInt(checkboxFromName("AIU Decel Communications"), Globals.BIT5);
            rc += checkBoxCheckedToInt(checkboxFromName("Reverse Operation Allowed"), Globals.BIT6);
            rc += checkBoxCheckedToInt(checkboxFromName("Short Hood Forward"), Globals.BIT7);
            
            TempFeatureSet3 = rc;
        }

        private void updateTempFeatureSet2()
        {
            int rc = 0;

            rc += checkBoxCheckedToInt(checkboxFromName("Long Hood Forward"), Globals.BIT0);
            rc += checkBoxCheckedToInt(checkboxFromName("Married Pair"), Globals.BIT1);
            rc += checkBoxCheckedToInt(checkboxFromName("BA Equipped"), Globals.BIT2);
            rc += checkBoxCheckedToInt(checkboxFromName("Pulse Emergency Brake"), Globals.BIT3);
            updateDependentCheckboxes();
            rc += checkBoxCheckedToInt(checkboxFromName("Tunnel Enable"), Globals.BIT4);
            rc += checkBoxCheckedToInt(checkboxFromName("3 Second Alarm Delay"), Globals.BIT6);
            rc += checkBoxCheckedToInt(checkboxFromName("Slip/Slide Bypass Before Crosscheck"), Globals.BIT7);
            
            TempFeatureSet2 = rc;
        }

        private void updateTempFeatureSet1()
        {
            int rc = 0;

            rc += checkBoxCheckedToInt(checkboxFromName("Total Slip/Slide Bypass"), Globals.BIT0);
            rc += checkBoxCheckedToInt(checkboxFromName("2 Second Emergency Brake Pulse"), Globals.BIT1);

            TempFeatureSet1 = rc;
        }


        private void updateDependentCheckboxes()
        {
            if((bool)checkboxFromName("Pulse Emergency Brake").IsChecked)
            {
                checkboxFromName("2 Second Emergency Brake Pulse").IsEnabled = true;
            }
            else
            {
                checkboxFromName("2 Second Emergency Brake Pulse").IsEnabled = false;
                checkboxFromName("2 Second Emergency Brake Pulse").IsChecked = false;
            }
        }

        private int checkBoxCheckedToInt(CheckBox CB, int check)
        {
            if ((bool)CB.IsChecked)
            {
                return check;
            }
            else
            {
                return 0;
            }
        }        

        private bool intToBool(int parameter, int check)
        {
            if ((parameter & check) == check)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitComboBoxItems()
        {
            try
            {
                foreach(string st in Globals.RAILROADS)
                {
                    RadComboBoxItem Item = new RadComboBoxItem()
                    {
                        Content = st
                    };

                    this.RailroadCombo.Items.Add(Item);
                }                
            }
            catch
            {

            }
        }
        
        private void UpdateRailroadDependentItems()
        {
            try
            {
                this.TrainTypesCombo.Items.Clear();
                this.VehicleTypesCombo.Items.Clear();
                this.DataloggerCombo.Items.Clear();
                this.DecelDirectCombo.Items.Clear();

                List<string> VType = new List<string>();
                List<string> TType = new List<string>();
                List<string> Data = new List<string>();

                if (this.Railroad == 1)
                {
                    VType = Globals.VEHICLETYPESMNR;
                    TType = Globals.TRAINTYPESMNR;
                    Data = Globals.DATALOGDEVICESMNR;
                    
                }
                else if (this.Railroad == 0)
                {

                    VType = Globals.VEHICLETYPESLIRR;
                    TType = Globals.TRAINTYPESLIRR;
                    Data = Globals.DATALOGDEVICESLIRR;                    
                }

                foreach (string st in TType)
                {
                    RadComboBoxItem Item = new RadComboBoxItem()
                    {
                        Content = st
                    };

                    this.TrainTypesCombo.Items.Add(Item);
                }

                foreach (string st in VType)
                {
                    RadComboBoxItem Item = new RadComboBoxItem()
                    {
                        Content = st
                    };

                    this.VehicleTypesCombo.Items.Add(Item);
                }

                foreach (string st in Data)
                {
                    RadComboBoxItem Item = new RadComboBoxItem()
                    {
                        Content = st
                    };

                    this.DataloggerCombo.Items.Add(Item);
                }

                foreach(string st in Globals.DECELDIRECTION)
                {
                    RadComboBoxItem Item = new RadComboBoxItem()
                    {
                        Content = st
                    };

                    this.DecelDirectCombo.Items.Add(Item);
                }

            }
            catch
            {

            }
        }

        public void createConfigDisplay()
        {
            try
            {
                InitComboBoxItems();
                InitCheckboxItems();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConfigDisplay::createConfigDisplay threw exception: {0}", ex);
            }     

        }        

        /// Textbox event handlers
        /// 

        public void ParameterChanged()
        {

            try
            {
                if (InitComplete)
                {
                    UpdateConfig.IsEnabled = true;
                    CancelConfig.IsEnabled = true;

                }                
            }
            catch
            {

            }           
        }

        private void ResetConfig()
        {
            InitComplete = false;

            if (this._myConfig.Find("Owner").Value == 95)
            {
                Railroad = 1;
            }
            else
            {
                Railroad = 0;
            }
            
            VehicleType = this._myConfig.Find("VehicleType").Value;
            VehicleNumber = this._myConfig.Find("LocoNumber").Value;
            VehicleSpeed = this._myConfig.Find("MaxSpeed").Value;
            TachYPP = ((double)this._myConfig.Find("YardsPerPulse").Value/100000);
            TachPPR = this._myConfig.Find("PulsesPerRevolution").Value;
            WheelDiameter = (double)ConvertWheelDiameter(false, this._myConfig.Find("WheelDiameter").Value);
            TrainType = ConvertTrainType(false, this._myConfig.Find("TrainType").Value);            
            FrontAntOffset = this._myConfig.Find("FrontOffset").Value;
            RearAntOffset = this._myConfig.Find("RearOffset").Value;
            Datalogger = ConvertDatalogger(false, this._myConfig.Find("DatalogDevice").Value);
            ATCS = this._myConfig.Find("ATCSAddress").Value;

            readFeatureSets();

            InitComplete = true;
        }

        private void UpdateConfigItems()
        {
            if (Railroad == 1)
            {
                this._myConfig.Find("Owner").Value = 95;
            }
            else
            {
                this._myConfig.Find("Owner").Value = 0;
            }            

            this._myConfig.Find("VehicleType").Value = VehicleType;
            this._myConfig.Find("LocoNumber").Value = VehicleNumber;
            this._myConfig.Find("MaxSpeed").Value = VehicleSpeed;
            this._myConfig.Find("WheelDiameter").Value = (int)ConvertWheelDiameter(true, WheelDiameter);
            this._myConfig.Find("TrainType").Value = ConvertTrainType(true, TrainType);            
            this._myConfig.Find("FrontOffset").Value = FrontAntOffset;
            this._myConfig.Find("RearOffset").Value = RearAntOffset;            
            this._myConfig.Find("DatalogDevice").Value = ConvertDatalogger(true, Datalogger);
            this._myConfig.Find("ATCSAddress").Value = ATCS;
            this._myConfig.Find("FeatureSet1").Value = TempFeatureSet1;
            this._myConfig.Find("FeatureSet2").Value = TempFeatureSet2;
            this._myConfig.Find("FeatureSet3").Value = TempFeatureSet3;
        }

        private int ConvertDatalogger(bool ToReading, int Value)
        {
            try
            {
                if (ToReading)
                {
                    if (Value == 3)
                    {
                        return 4;
                    }                    
                    else
                    {
                        return Value;
                    }
                }
                else
                {
                    if (Value == 4)
                    {
                        return 3;
                    }
                    else
                    {
                        return Value;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        private int ConvertTrainType(bool ToReading, int Value)
        {
            try
            {
                if (ToReading)
                {

                    switch(Value)
                    {
                        case 0:
                            return 2;
                        case 1:
                            return 3;
                        case 2:
                            return 5;
                        default:
                            return 0;
                    }
                }
                else
                {
                    switch (Value)
                    {
                        case 2:
                            return 0;
                        case 3:
                            return 1;
                        case 5:
                            return 2;
                        default:
                            return 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        private object ConvertWheelDiameter(bool ToReading, object Value)
        {
            try
            {
                if (ToReading)
                {
                    return (int)(((double)Value) * 100);                     
                }
                else
                {
                    return (double)(((int)Value) / 100.0);
                }
            }
            catch
            {
                return 0;
            }
        }

        private int ConvertDecelDirection(bool ToReading, int Value)
        {
            if (ToReading)
            {
                if (Value == 1)
                {
                    return Globals.BIT3;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (Value == Globals.BIT3)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// Button methods
        /// 

        // Send/Update Config button
        public void ProgressConfigState(object sender, RoutedEventArgs e)
        {
            try
            {
                switch(_eState)
                {
                    case Config.CONFIG_CHANGE_STATE.NO_ACTION:

                        UpdateConfigItems();

                        _eState = Config.CONFIG_CHANGE_STATE.REQUEST_CHANGE;
                        _myConfig.submitRequest(_eState);

                        ResetConfig();

                        Thread.Sleep(2000);

                        CancelConfig.IsEnabled = true;
                        UpdateConfig.Content = "Confirm";
                        UpdateConfig.Background = System.Windows.Media.Brushes.Yellow;

                        break;

                    case Config.CONFIG_CHANGE_STATE.REQUEST_CHANGE:

                        UpdateConfig.Content = "OK";
                        UpdateConfig.Background = System.Windows.Media.Brushes.IndianRed;

                        _eState = Config.CONFIG_CHANGE_STATE.SAVE_REQUEST;
                        _myConfig.submitRequest(_eState);

                        break;

                    case Config.CONFIG_CHANGE_STATE.SAVE_REQUEST:
                        CancelConfig.IsEnabled = false;
                        UpdateConfig.IsEnabled = false;
                        _eState = Config.CONFIG_CHANGE_STATE.NO_ACTION;

                        ResetConfig();

                        UpdateConfig.Content = "Send";
                        UpdateConfig.Background = System.Windows.Media.Brushes.LimeGreen;                                            

                        break;

                    case Config.CONFIG_CHANGE_STATE.ABORT_REQUEST:

                        _myConfig.submitRequest(_eState);
                        _eState = Config.CONFIG_CHANGE_STATE.NO_ACTION;

                        ResetConfig();

                        UpdateConfig.Content = "Send";
                        UpdateConfig.Background = System.Windows.Media.Brushes.LimeGreen;
                                                
                        CancelConfig.IsEnabled = false;
                        UpdateConfig.IsEnabled = false;

                        break;                   

                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConfigDisplay::ProgressConfigState threw exception: {0}", ex);
            }
        }

        /// Cancel Button
        /// 

        public void CancelConfigUpdate(object sender, RoutedEventArgs e)
        {
            _eState = Config.CONFIG_CHANGE_STATE.ABORT_REQUEST;
            _myConfig.submitRequest(_eState);
            _eState = Config.CONFIG_CHANGE_STATE.NO_ACTION;

            ResetConfig();
            
            CancelConfig.IsEnabled = false;
            UpdateConfig.IsEnabled = false;
            UpdateConfig.Content = "Send";
            UpdateConfig.Background = System.Windows.Media.Brushes.LimeGreen;
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
