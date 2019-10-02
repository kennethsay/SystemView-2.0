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

            List<string> Apps = Globals.REVISIONAPPS;
            List<RevisionElement> Revs = _myRevision.GetRevisions();

            var AppRevs = Apps.Zip(Revs, (Name, Rev) => new { Name, Rev});

            foreach (var AR in AppRevs)
            {

                RevisionMember _member;

                if (AR.Rev.Type == REVISION_TYPE.EXTENDED)
                {
                    _member = new RevisionMember(AR.Rev.RevisionData as ExtendedAppRevision, AR.Name);
                }
                else if (AR.Rev.Type == REVISION_TYPE.BASIC)
                {
                    _member = new RevisionMember(AR.Rev.RevisionData as BasicAppRevision, AR.Name);
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

        public RevisionMember( ExtendedAppRevision DataField, string Name)
        {
            _appName = Name;
            _partNumber = DataField.PartNumber.Split('\0')[0]; 
            _appRev = DataField.Rev.Split('\0')[0]; 
            _buildDate = DataField.Date.Split('\0')[0]; 
            _build = DataField.Build.Split('\0')[0]; 
        }

        public RevisionMember(BasicAppRevision DataField, string Name)
        {

            _appName = Name;
            _partNumber = DataField.PartNumber.Split('\0')[0];
            _appRev = DataField.Rev.Split('\0')[0]; 
            _buildDate = DataField.Date.Split('\0')[0]; 
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
    }
}
