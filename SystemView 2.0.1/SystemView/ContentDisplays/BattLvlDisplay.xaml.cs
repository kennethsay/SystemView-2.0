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
using System.Windows.Shapes;
using AppLogic;
using System.ComponentModel;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for BattLvlDisplay.xaml
    /// </summary>
    public partial class BattLvlDisplay : UserControl, INotifyPropertyChanged
    {
        private BatteryLevel myBattLvl;
        private double BATTERY_LOW_LEVEL = 3.0;

        private LEDIndicator.LEDState _healthyLEDState;
        public LEDIndicator.LEDState HealthyLEDState
        {
            get
            {
                return this._healthyLEDState;
            }
            set
            {
                this._healthyLEDState = value;
                OnPropertyChanged("HealthyLEDState");
            }
        }

        private LEDIndicator.LEDState _maintLEDState;
        public LEDIndicator.LEDState MaintLEDState
        {
            get
            {
                return this._maintLEDState;
            }
            set
            {
                this._maintLEDState = value;
                OnPropertyChanged("MaintLEDState");
            }
        }

        public BattLvlDisplay()
        {
            try
            {
                InitializeComponent();

                this.DataContext = this;

                myBattLvl = new BatteryLevel();
                updateVoltageReading();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BattLvlDisplay::Constructor threw exception: {0}", ex);
            }
            
        }
        private void updateVoltageReading()
        {
            try
            {
                VoltageDisplay.Text = myBattLvl.Level.ToString() + " Volts";

                if (myBattLvl.Level < BATTERY_LOW_LEVEL)
                {
                    MaintLEDState = LEDIndicator.LEDState.ON;
                    HealthyLEDState = LEDIndicator.LEDState.OFF;
                }
                else
                {
                    MaintLEDState = LEDIndicator.LEDState.OFF;
                    HealthyLEDState = LEDIndicator.LEDState.ON;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("BattLvlDisplay::updateVoltageReading threw exception: {0}", ex);
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
    }
}
