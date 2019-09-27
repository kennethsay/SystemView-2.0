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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for Revision.xaml
    /// </summary>
    public partial class RevisionDisplay : UserControl
    {
        private ObservableCollection<RevisionMember> _obcRevisions;
        private Revision _myRevision;

        public RevisionDisplay()
        {
            InitializeComponent();

            _obcRevisions = new ObservableCollection<RevisionMember>();

            RevisionGrid.DataContext = _obcRevisions;

            getRevision();
        }

        private void getRevision()
        {
            _myRevision = new Revision();

            foreach (RevisionElement e in _myRevision.GetRevisions())
            {

                RevisionMember _member;

                if (e.Type == REVISION_TYPE.EXTENDED)
                {
                    _member = new RevisionMember(e.RevisionData as ExtendedAppRevision);
                }
                else if (e.Type == REVISION_TYPE.BASIC)
                {
                    _member = new RevisionMember(e.RevisionData as BasicAppRevision);
                }
                else
                {
                    _member = new RevisionMember();
                }
                
                _obcRevisions.Add(_member);
            }
        }
        
    }

    public class RevisionMember : INotifyPropertyChanged
    {

        private string _appName;
        private string _partNumber;
        private string _appRev;
        private string _buildDate;
        private string _build;

        public string AppName
        {
            get { return _appName;  }
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                }
                OnPropertyChanged("AppName");
            }
        }

        public string PartNumber
        {
            get { return _partNumber; }
            set
            {
                if (_partNumber != value)
                {
                    _partNumber = value;
                }
                OnPropertyChanged("PartNumber");
            }
        }

        public string AppRev
        {
            get { return _appRev; }
            set
            {
                if (_appRev != value)
                {
                    _appRev = value;
                }
                OnPropertyChanged("AppRev");
            }
        }
        public string BuildDate
        {
            get { return _buildDate; }
            set
            {
                if (_buildDate != value)
                {
                    _buildDate = value;
                }
                OnPropertyChanged("BuildDate");
            }
        }
        public string Build        
        {
            get { return _build; }
            set
            {
                if (_build != value)
                {
                    _build = value;
                }

                OnPropertyChanged("Build");
            }
        }
        public RevisionMember()
        {

        }

        public RevisionMember( ExtendedAppRevision DataField)
        {
            _appName = _appName = appNameFromPartNumber(DataField.PartNumber);
            _partNumber = DataField.PartNumber;
            _appRev = DataField.Rev;
            _buildDate = DataField.Date;
            _build = DataField.Build;
        }

        public RevisionMember(BasicAppRevision DataField)
        {

            _appName = appNameFromPartNumber(DataField.PartNumber);
            _partNumber = DataField.PartNumber;
            _appRev = DataField.Rev;
            _buildDate = DataField.Date;
            _build = null;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string appNameFromPartNumber(string PartNumber)
        {
            string AppName = "";
            try
            {
                switch(PartNumber)
                {
                    case "A073G01-A01":
                    case "A073G01-A02":
                        AppName = "Main";
                        break;

                    case "A075G03-A02":
                        AppName = "Ethernet Netburner";
                        break;
                    case "A073G03-A92":
                        AppName = "Tach I/O";
                        break;
                    case "A073G03-A91":
                        AppName = "Decelerometer";
                        break;
                    case "A073G03-A94":
                        AppName = "Ethernet S12";
                        break;
                    case "A073G03-A97":
                        AppName = "AIU";
                        break;
                    case "A073G03-A101":
                        AppName = "CMM";
                        break;
                    case "A073G03-A93":
                        AppName = "Aux I/O";
                        break;
                    case "A073G03-A76":
                        AppName = "TP Reader";
                        break;
                    case "A073G03-A107":
                        AppName = "ARMM";
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }

            return AppName;
        }
    }
}
