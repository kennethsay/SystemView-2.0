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

                    UpdateConfigItems();
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
                    return (double)(((int)Value) / 100);
                }
            }
            catch
            {
                return 0;
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
