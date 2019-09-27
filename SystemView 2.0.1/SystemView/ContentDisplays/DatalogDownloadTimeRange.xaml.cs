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

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for DatalogDownloadTimeRange.xaml
    /// </summary>
    public partial class DatalogDownloadTimeRange : UserControl, INotifyPropertyChanged, IDataErrorInfo
    {
        private DateTime _startDate;
        public DateTime StartDateTime
        {
            get
            {
                return this._startDate;
            }
            set
            {
                this._startDate = value;
            }
        }
        private DateTime _endDate;
        public DateTime EndDateTime
        {
            get
            {
                return this._endDate;
            }
            set
            {
                this._endDate = value;
            }
        }


        public DatalogDownloadTimeRange()
        {
            InitializeComponent();

            DataContext = this;

            this.EndDateTime = DateTime.Now;
            this.StartDateTime = EndDateTime.Subtract(TimeSpan.FromMinutes(15));
        }


        private string ValidateStartTime()
        {
            try
            {
                if (StartDateTime.CompareTo(DateTime.Now) >= 1)
                {
                    return "Start Datetime cannot be in the future";
                }
                else if (StartDateTime.CompareTo(EndDateTime) >= 0)
                {
                    return "End Datetime must be later than the start time";
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string ValidateEndTime()
        {
            try
            {
                if (EndDateTime.CompareTo(DateTime.Now) >= 1)
                {
                    return "End Datetime cannot be in the future";
                }
                else if (EndDateTime.CompareTo(StartDateTime) <= 0)
                {
                    return "End Datetime must be later than the start time";
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public string Error
        {
            get
            {
                return ValidateStartTime() ?? ValidateEndTime();
            }
        }
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "StartDateTime": return this.ValidateStartTime();
                    case "EndDateTime": return this.ValidateEndTime();
                }
                return null;
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
