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
using System.Dynamic;
using Telerik.Windows.Controls;

using AppLogic;


namespace SystemView.ContentDisplays
{
    public enum MessageType
    {
        MSG11,
        MSG12,
        MSG19,
        MSG21,
        MSG22,
        MSG23,
        MSG24,
        MSG34,
        MSG35,
        None
    }






    /// <summary>
    /// Interaction logic for RadioTransponderData.xaml
    /// </summary>
    public partial class RadioData : UserControl
    {


        RadioDataItem DataItem;
        private MainRadioClass RadioParser;

        object RR_Radio_Message;
        public object MTAMessage
        {
            get
            {
                return this.RR_Radio_Message;
            }
            set
            {
                this.RR_Radio_Message = value;
            }
        }


        public RadioData()
        {
            InitializeComponent();
        }

        public void ProcessDetailDisplayOutput(byte[] HexString)
        {
            string Converted = HexToString(HexString);
            this.RadioParser = new MainRadioClass(Converted);

            this.RadioDetailDisplay.Children.Clear();

            foreach (string st in RadioParser.radioParsed)
            {
                Console.WriteLine(st);

                TextBlock radioDetail = new TextBlock()
                {
                    Text = st,
                    FontFamily = new FontFamily("Segoe UI Light"),
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                this.RadioDetailDisplay.Children.Add(radioDetail);
                this.RadioDetailDisplay.UpdateLayout();
            }
        }

        public string HexToString(byte[] Hex)
        {
            return BitConverter.ToString(Hex).Replace("-", string.Empty);
        }

        public MTA_RR_Radio_Message ProcessMessage(TagList Tags, UInt32 Index)
        {
            try
            {

                Byte[] Data = Tags.GetTag(Globals.RadioMsgTagIndex).Data();

                /* Determine the message type and create an instance of that type. For MTA Railroad projects, 
                 * the message type is determined by the 2nd and 3rd bytes of the radio message. 
                 */


                UInt16 MsgHeader = (UInt16)(Data[1] << 8 | Data[2]);

                switch (MsgHeader)
                {
                    case Globals.IXLSTATUSREQMSG:
                    case Globals.IXLSTATUSRESWLOMAMSG:
                    case Globals.IXLSTATUSRESWOUTLOMAMSG:
                        this.RR_Radio_Message = new EncoderMsg(MsgHeader, Data);
                        break;
                    case Globals.TSRLISTREQMSG:
                    case Globals.TSRLISTRESMSG:
                    case Globals.TSRNOCHANGERESMSG:
                        this.RR_Radio_Message = new TSRMsg(MsgHeader, Data);
                        break;
                    case Globals.MAINTENANCEMSGREQ:
                    case Globals.MAINTENANCEMSGACK:
                        this.RR_Radio_Message = new MTAMsg(MsgHeader, Data);
                        break;

                    default:
                        break;
                }

                (RR_Radio_Message as MTA_RR_Radio_Message).ProcessGeneralMessageInfo(Tags, Index);

                return (RR_Radio_Message as MTA_RR_Radio_Message);

            }
            catch (Exception ex)
            {
                return null;
            }
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

    public class RadioMessages : RadioData, INotifyPropertyChanged
    {

    }



    public class MTA_RR_Radio_Message : DynamicObject, INotifyPropertyChanged
    {
        #region VariablesAndProperties

        private MessageType _Type;
        private int _Length;
        private DateTime _TimeStamp;
        private UInt16 _PseudoRandomTimestamp;
        private UInt32 _EventNum;
        private UInt16 _ManufacturerCode;
        private UInt16 _TrainID;
        private UInt16 _Location;


        public MTA_RR_Radio_Message()
        {

        }

        public MTA_RR_Radio_Message(MessageType Type, Byte[] Data)
        {
            try
            {
                if (Type != MessageType.None && Data != null)
                {
                    this._Type = Type;
                    this._messageData = Data;
                }
            }
            catch
            {

            }
        }

        public MTA_RR_Radio_Message(Byte[] Data)
        {
            try
            {
                if (Data != null)
                {
                    this._messageData = Data;
                }
            }
            catch
            {

            }

        }

        public MTA_RR_Radio_Message(UInt32 index)
        {
            try
            {
                if (index != null)
                {
                    this.Event = index;
                }
            }
            catch
            {

            }
        }

        private Byte[] _messageData;

        public Byte[] Data
        {
            get
            {
                return this._messageData;
            }

            set
            {
                this._messageData = value;
            }
        }


        public UInt32 Event
        {
            get
            {
                return this._EventNum;
            }
            set
            {
                this._EventNum = value;
            }
        }

        public MessageType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }

        public UInt16 PsuedoRandomTimeStamp
        {
            get
            {
                return this._PseudoRandomTimestamp;
            }
            set
            {
                this._PseudoRandomTimestamp = value;
            }
        }

        public int Length
        {
            get
            {
                return this._Length;
            }
            set
            {
                this._Length = value;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return this._TimeStamp;
            }
            set
            {
                this._TimeStamp = value;
            }
        }

        #endregion



        public void ProcessGeneralMessageInfo(TagList List, UInt32 Index)
        {
            this._TimeStamp = (DateTime)(List.GetTag(0).TypeConvertedValue());
            this.Length = _messageData.Length;
            this.Event = Index;

            processPseudoRandomTime();
        }

        private void processPseudoRandomTime()
        {
            try
            {
                if (this._messageData != null)
                {
                    UInt16 result = (UInt16)(this._messageData[8] << 8 | this._messageData[9]);
                    this._PseudoRandomTimestamp = result;
                }
            }
            catch
            {

            }
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

    public class EncoderMsg : MTA_RR_Radio_Message
    {
        private int _Encoder;
        public int Encoder
        {
            get
            {
                return _Encoder;
            }
            set
            {
                _Encoder = value;
            }
        }

        private int _Signal;
        public int Signal
        {
            get
            {
                return _Signal;
            }
            set
            {
                _Signal = value;
            }
        }
        private UInt16 _BCP;
        private Direction _Direction;
        private int _rrLineNum;
        private int _trackNum;
        private int _exitTrack;
        private UInt16 _distancetoSpeedIncr;
        private UInt16 _distHStoXL;
        private UInt16 _distHStoExit;
        private int _tracksCrossed;
        private Direction _exitTrackDir;
        private UInt16 _distToLoMA;
        private int _Grade;
        private int _nextEncoder;
        private int _nextSignal;
        private UInt16 _nextBCP;
        private int _nextRRLineNum;

        public EncoderMsg()
        {

        }


        public EncoderMsg(UInt16 MsgType, Byte[] MsgData)
        {
            try
            {
                /* Parse the Message Header into a valid OBC Message Type. Set the Message type and 
                 * process the message contents. This single encoder message can encompass several different IXL
                 * request/response types. The types are as follows:
                 * 
                 *  #21: Interlocking Status Request 
                 *  #22: Interlocking Status Request Response with LoMA
                 *  #23: Interlocking Status Request Response without LoMA
                 */

                this.Data = MsgData;

                switch (MsgType)
                {
                    // Message type is IXL Status Request.
                    case Globals.IXLSTATUSREQMSG:
                        this.Type = MessageType.MSG21;
                        break;
                    // Message type is IXL Status Request Response with LoMA.
                    case Globals.IXLSTATUSRESWLOMAMSG:
                        this.Type = MessageType.MSG22;
                        break;
                    case Globals.IXLSTATUSRESWOUTLOMAMSG:
                        this.Type = MessageType.MSG23;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }



    public class TSRMsg : MTA_RR_Radio_Message
    {


        public TSRMsg()
        {

        }


        public TSRMsg(UInt16 MsgType, Byte[] MsgData)
        {
            try
            {
                /* Parse the Message Header into a valid OBC Message Type. Set the Message type and 
                 * process the message contents. This single TSR message can encompass several different IXL
                 * request/response types. The types are as follows:
                 * 
                 *  #11: TSR List Request 
                 *  #12: TSR List Response
                 *  #19: TSR List No Change Response
                 */

                this.Data = MsgData;

                switch (MsgType)
                {
                    // Message type is TSR List Request.
                    case Globals.TSRLISTREQMSG:
                        this.Type = MessageType.MSG11;
                        break;
                    // Message type is TSR List Response
                    case Globals.TSRLISTRESMSG:
                        this.Type = MessageType.MSG12;
                        break;
                    case Globals.TSRNOCHANGERESMSG:
                        this.Type = MessageType.MSG19;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

    }

    public class MTAMsg : MTA_RR_Radio_Message
    {
        string _Details;

        public MTAMsg()
        {

        }

        public MTAMsg(UInt16 MsgType, Byte[] MsgData)
        {
            try
            {
                /* Parse the Message Header into a valid OBC Message Type. Set the Message type and 
                 * process the message contents. This single encoder message can encompass several different MTA
                 * request/response types. The types are as follows:
                 * 
                 *  #34: MTA Message Send
                 *  #35: MTA Message Response 
                 */

                this.Data = MsgData;

                switch (MsgType)
                {
                    // Message type is MTA Request
                    case Globals.MAINTENANCEMSGREQ:
                        this.Type = MessageType.MSG34;
                        break;
                    // Message type is MTA Ack
                    case Globals.MAINTENANCEMSGACK:
                        this.Type = MessageType.MSG35;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }


    }

}
