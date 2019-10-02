using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using System.ComponentModel;
using AppLogic;

namespace SystemView
{
    public class IOIndicationList : INotifyPropertyChanged
    {
        public enum IndicationType
        {
            ACSES,
            ATC, 
            AIU, 
            CAB, 
            COMM,
            DT, 
            SS, 
            OTHER
        }

        private ObservableCollection<IOIndicator> _selectableACSESStatusIndications;
        public ObservableCollection<IOIndicator> SelectableACSESStatusIndications
        {
            get
            {
                return _selectableACSESStatusIndications;
            }
            set
            {
                this._selectableACSESStatusIndications = value;
                OnPropertyChanged("SelectableACSESStatusIndications");
            }
        }
        private ObservableCollection<IOIndicator> _selectableATCStatusIndications;
        public ObservableCollection<IOIndicator> SelectableATCStatusIndications
        {
            get
            {
                return _selectableATCStatusIndications;
            }
            set
            {
                this._selectableATCStatusIndications = value;
                OnPropertyChanged("SelectableATCStatusIndications");
            }
        }

        private ObservableCollection<IOIndicator> _selectableAIUStatusIndications;
        public ObservableCollection<IOIndicator> SelectableAIUStatusIndications
        {
            get
            {
                return _selectableAIUStatusIndications;
            }
            set
            {
                this._selectableAIUStatusIndications = value;
                OnPropertyChanged("SelectableAIUStatusIndications");
            }
        }
        private ObservableCollection<IOIndicator> _selectableCabStatusIndications;
        public ObservableCollection<IOIndicator> SelectableCabStatusIndications
        {
            get
            {
                return _selectableCabStatusIndications;
            }
            set
            {
                this._selectableCabStatusIndications = value;
                OnPropertyChanged("SelectableCabStatusIndications");
            }
        }
        private ObservableCollection<IOIndicator> _selectableCommStatusIndications;
        public ObservableCollection<IOIndicator> SelectableCommStatusIndications
        {
            get
            {
                return _selectableCommStatusIndications;
            }
            set
            {
                this._selectableCommStatusIndications = value;
                OnPropertyChanged("SelectableCommStatusIndications");
            }
        }

        private ObservableCollection<IOIndicator> _selectableDTStatusIndications;
        public ObservableCollection<IOIndicator> SelectableDTStatusIndications
        {
            get
            {
                return _selectableDTStatusIndications;
            }
            set
            {
                this._selectableDTStatusIndications = value;
                OnPropertyChanged("SelectableDTStatusIndications");
            }
        }
        private ObservableCollection<IOIndicator> _selectableSSStatusIndications;
        public ObservableCollection<IOIndicator> SelectableSSStatusIndications
        {
            get
            {
                return _selectableSSStatusIndications;
            }
            set
            {
                this._selectableSSStatusIndications = value;
                OnPropertyChanged("SelectableSSStatusIndications");
            }
        }
        private ObservableCollection<IOIndicator> _selectableOtherStatusIndications;
        public ObservableCollection<IOIndicator> SelectableOtherStatusIndications
        {
            get
            {
                return _selectableOtherStatusIndications;
            }
            set
            {
                this._selectableOtherStatusIndications = value;
                OnPropertyChanged("SelectableOtherStatusIndications");
            }
        }

        private ObservableCollection<IOIndicator> _displayedIndications;
        public ObservableCollection<IOIndicator> DisplayedIndications
        {
            get
            {
                return _displayedIndications;
            }
            set
            {
                this._displayedIndications = value;
                OnPropertyChanged("DisplayedIndications");
            }
        }
        public IOIndicationList()
        {
            
        }
        

        public void UpdateIORibbon(TagList tList)
        {

        }

        public void Init()
        {
            SelectableACSESStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableATCStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableAIUStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableCabStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableCommStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableDTStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableSSStatusIndications = new ObservableCollection<IOIndicator>();
            SelectableOtherStatusIndications = new ObservableCollection<IOIndicator>();
            DisplayedIndications = new ObservableCollection<IOIndicator>();

            foreach (string st in Globals.IORIBBONACSESITEMS)
            {
                this.SelectableACSESStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONATCITEMS)
            {
                this.SelectableATCStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONAIUITEMS)
            {
                this.SelectableAIUStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONCABITEMS)
            {
                this.SelectableCabStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONCOMMITEMS)
            {
                this.SelectableCommStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONDTITEMS)
            {
                this.SelectableDTStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONSSITEMS)
            {
                this.SelectableSSStatusIndications.Add(new IOIndicator(st));
            }
            foreach (string st in Globals.IORIBBONOTHERITEMS)
            {
                this.SelectableOtherStatusIndications.Add(new IOIndicator(st));
            }

        }

        public void UpdateIOState(string IOParam, LEDIndicator.LEDState eState, IndicationType Type)
        {         

            FindIndicator(this.DisplayedIndications, IOParam).Value = eState;
        }
        private IOIndicator FindIndicator(ObservableCollection<IOIndicator> Collection, string name)
        {
            foreach(IOIndicator IO in Collection)
            {
                if (IO.Name == name)
                {
                    return IO;
                }
            }

            return null;
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

    public class IOIndicator
    {
        public string Name
        {
            get;
            set;
        }
        
        public LEDIndicator.LEDState Value
        {
            get;
            set;
        }
        
        public IOIndicator(string TileName)
        {
            if (TileName != null)
            {
                this.Name = TileName;
            }
        }                 
    }
}
