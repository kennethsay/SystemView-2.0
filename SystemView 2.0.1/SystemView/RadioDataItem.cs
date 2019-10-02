using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;

using AppLogic;
using SystemView.ContentDisplays;

namespace SystemView
{
    public class RadioDataItem : DynamicObject, INotifyPropertyChanged
    {
        readonly IDictionary<string, object> data;
        private object Message;
        readonly private byte[] MsgData;

        public RadioDataItem()
        {
            data = new Dictionary<string, object>();
            
        }

        public RadioDataItem(object Msg)
        {
            if (Msg != null)
            {
                this.data = new Dictionary<string, object>();
                this.Message = new MTA_RR_Radio_Message();
                this.Message = Msg;

                MTA_RR_Radio_Message basicMsg = Msg as MTA_RR_Radio_Message;

                MsgData = basicMsg.Data;

                data.Add("Event #", basicMsg.Event.ToString());
                data.Add("Timestamp", basicMsg.TimeStamp.ToString());
                data.Add("Pseudorandom Timestamp", basicMsg.PsuedoRandomTimeStamp.ToString());
                data.Add("Message Type", msgTypeToString());
                data.Add("Message", HexToString(MsgData));
            }
        }
        

        private string msgTypeToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                if ((Message as MTA_RR_Radio_Message) != null)
                {
                    switch ((Message as MTA_RR_Radio_Message).Type)
                    {
                        case MessageType.MSG21:
                            sb.Append(string.Format("IXL Status Request (#21)"));
                            break;

                        case MessageType.MSG22:
                            sb.Append(string.Format("IXL Status Response w/LoMA (#22)"));
                            break;

                        case MessageType.MSG23:
                            sb.Append(string.Format("IXL Status Response w/o LoMA (#23)"));
                            break;

                        case MessageType.MSG11:
                            sb.Append(string.Format("TSR List Request (#11)"));
                            break;

                        case MessageType.MSG12:
                            sb.Append(string.Format("TSR List Response (#12)"));
                            break;

                        case MessageType.MSG19:
                            sb.Append(string.Format("TSR List No Change Response (#19)"));
                            break;

                        case MessageType.MSG34:
                            sb.Append(string.Format("OBC Maintenance (#34)"));
                            break;

                        case MessageType.MSG35:
                            sb.Append(string.Format("OBC Maintenance ACK (#35)"));
                            break;

                        default:
                            sb.Append(string.Format("Invalid Message Type!"));
                            break;
                    }

                    return sb.ToString();
                }
                else
                {
                    sb.Append(string.Format("Invalid Message Detected!"));
                    return sb.ToString();
                }
            }
            catch
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Error Parsing Message Type!"));
                return sb.ToString();
            }
        }

        public string GetMessage()
        {
            return (string)data["Message"];
        }
        public string HexToString(byte[] Hex)
        {
            return BitConverter.ToString(Hex).Replace("-", string.Empty);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.data.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;

            return true;
        }


        public object this[string columnName]
        {
            get
            {
                if (this.data.ContainsKey(columnName))
                {
                    return this.data[columnName];
                }

                return null;
            }
            set
            {
                if (!this.data.ContainsKey(columnName))
                {
                    this.data.Add(columnName, value);

                    this.OnPropertyChanged(columnName);
                }
                else
                {
                    if (this.data[columnName] != value)
                    {
                        this.data[columnName] = value;

                        this.OnPropertyChanged(columnName);
                    }
                }
            }
        }
    }




}
