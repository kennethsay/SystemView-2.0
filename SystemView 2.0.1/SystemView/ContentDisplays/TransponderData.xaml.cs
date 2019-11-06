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

using AppLogic;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for TransponderData.xaml
    /// </summary>
    public partial class TransponderData : UserControl, INotifyPropertyChanged
    {
        private MainTransponderClass TPParser;

        public enum TPDIRECTION
        {
            DN, 
            UP, 
            NONE
        };

        public enum LINKINGDIST
        {
            NOTEXACT, 
            EXACT, 
            NONE
        };

        public TransponderData()
        {
            InitializeComponent();
        }

        
        public void ProcessDetailDisplayOutput(string HexString)
        {
           // string Converted = HexToString(HexString);
            this.TPParser = new MainTransponderClass(HexString);

            this.TPDetailDisplay.Children.Clear();

            foreach (string st in TPParser.pkgMsg)
            {
                TextBlock tpDetail = new TextBlock()
                {
                    Text = st,
                    FontFamily = new FontFamily("Segoe UI Light"),
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 0)                    
                };
                this.TPDetailDisplay.Children.Add(tpDetail);
            }
        }

        public string HexToString(byte[] Hex)
        {
            return BitConverter.ToString(Hex).Replace("-", string.Empty);
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


    public class TPMessage
    {
        private UInt32 _chainage;
        private double _milepost;
        private int _trackNumber;
        private int _tpNumber;
        private int _tpTotal;
        private int _rrLineNum;
        private UInt32 _linkingDistance;
        private TransponderData.LINKINGDIST _linkingDistType;
        private TransponderData.TPDIRECTION _direction;
        private List<Object> _packages;

        private Byte[] _data;
        public Byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
            }
        }

        private UInt32 _eventNum;
        public UInt32 EventNum
        {
            get { return _eventNum; }
            set
            {
                _eventNum = value;
            }
        }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
            }
        }

        private int _length;

        public int Length
        {
            get { return _length; }
            set
            {
                _length = value;
            }
        }


        public TPMessage()
        {

        }

        public TPMessage(UInt32 Event)
        {
            try
            {
                if (Event != null)
                {
                    this.EventNum = Event;
                }
            }
            catch
            {

            }
        }
        public void ProcessMessage(TagList list)
        {
            this.Timestamp = (DateTime)(list.GetTag(0).TypeConvertedValue());
            this.Data = list.GetTag(90).Data();
            this.Length = Data.Length;

        }        
    }
}
