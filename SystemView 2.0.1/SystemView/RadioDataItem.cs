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

    // To call RadioDataItem = new RadioDataItem();
    //
    // CLASS: RadioDataItem
    //
    // Description: This class implements the data structure used to generate Radio Data Presentation table real-time records. 
    //              Each structure represents a single radio message that is included as an instance of the RR_Radio_Message class. 
    //              A data binding to the presentation data table is used to add records to the radio view table. 
    //
    // Private Data:
    //      IDictionary<string, object> _data           - Radio data dictionary used to store data table information in the form of a column header and row data
    //      object _message                             - Object to store Radio Message 
    //      byte[] _msgData                             - Object to store Data parsed from the Radio Message
    //
    // Public Get/Set Accessors:
    //      object Message(get)                         - Accessor for the _message variable
    //      byte[] MsgData(get)                         - Accesssor for the _msgData variable
    //
    // Public Methods:
    //      string GetMessage()                         - Returns Radio Message Data as stored in the radio data dictionary
    //      string HexToString(byte[] Hex)              - Converts a hexidecimal array value argument to a string 
    //      string ToString()                           - Override of ToString method for RadioDataItem Class
    //
    // Private Methods:
    //      string msgTypeToString()                    - Converts the radio message type to a string.
    //
    // Constructors:
    //      RadioDataItem() 
    //      RadioDataItem(object Msg)
    //
    // Other DataTypes:
    //

    public class RadioDataItem : DynamicObject, INotifyPropertyChanged
    {
        #region PrivateMembers
        readonly IDictionary<string, object> _data;
        private object _message;
        readonly private byte[] _msgData;
        #endregion

        #region Accessors
        public object Message
        {
            get
            {
                return _message;
            }
        }

        public byte[] MsgData
        {
            get
            {
                return _msgData;
            }
        }
        #endregion

        #region Constructors

        // This is the default constructor. Just create a new blank data object to hold our radio information
        // The dictionary created here is used to tie a piece of data with a data table column header
        //
        // The string placed in this dictionary index is used to create the the following radio data column headers:
        //                 Event #, Timestamp, Pseudorandom Timestamp, Message Type, Message
        //
        // The object associated with each string is the value of the parameter specified by the string. This data is
        // populated in each row of the radio data table with its corresponding column. 

        /// <summary>
        /// Default constructor creates an empty RadioDataItem dictionary
        /// </summary>
        public RadioDataItem()
        {
            _data = new Dictionary<string, object>();            
        }

        /// <summary>
        /// This constructor takes a radio message as an argument and parses out the information necessary to create a 
        /// Radio Data Item. 
        /// </summary>
        /// <param name="Msg">Railroad Specific Radio Message to be parsed into a RadioDataItem</param>
        public RadioDataItem(object Msg)
        {
            try
            {
                // First check to make sure that the Msg object is not null
                if (Msg != null)
                {
                    // Looks good, so create a new data dictionary and a new railroad specific radio message object
                    this._data = new Dictionary<string, object>();
                    this._message = new MTA_RR_Radio_Message();
                    this._message = Msg;

                    // This just typecasts the Msg object to a railroad specific object for ease of use
                    MTA_RR_Radio_Message basicMsg = Msg as MTA_RR_Radio_Message;

                    // Copy off the radio message data for future use
                    _msgData = basicMsg.Data;

                    // Finally, populate the dictionary with the parsed data from the Radio Mesage
                    _data.Add("Event #", basicMsg.Event.ToString());
                    _data.Add("Timestamp", basicMsg.TimeStamp.ToString());
                    _data.Add("Pseudorandom Timestamp", basicMsg.PsuedoRandomTimeStamp.ToString());
                    _data.Add("Message Type", msgTypeToString());
                    _data.Add("Message", HexToString(_msgData));
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::Constrcutor-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        #endregion
        
        #region PublicMethods

        /// <summary>
        /// Returns Radio Message Data as stored in the radio data dictionary
        /// </summary>
        /// <returns>The radio data message as a string</returns>
        public string GetMessage()
        {
            try
            {
                // Return the radio message data from the dictionary typecast to a string
                return (string)_data["Message"];
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::GetMessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Converts a hexidecimal array value argument to a string 
        /// </summary>
        /// <param name="Hex">Hexidecimal byte array</param>
        /// <returns>String representing hexidecimal argument</returns>
        public string HexToString(byte[] Hex)
        {
            try
            {
                // Use bit converter to turn the hex argument into a string and return
                return BitConverter.ToString(Hex).Replace("-", string.Empty);
            }            
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::HexToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Override of ToString method for RadioDataItem Class
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem Properties - Event #: {0}, Timestamp: {1}, Pseudorandom Timestamp: {2}, Message Type: {3}, Message: {4}",
                                        (string)_data["Event #"], (string)_data["Timestamp"], (string)_data["Pseudorandom Timestamp"], 
                                        (string)_data["Message Type"], (string)_data["Message"]));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
        #endregion

        #region PrivateMethods

        /// <summary>
        /// Converts the radio message type to a string.
        /// Message type is a Siemens defined type that is used to differntiate radio messages. This is a 
        /// railroad specific type and parsing logic will vary
        /// </summary>
        /// <returns>String representing the radio message type</returns>
        private string msgTypeToString()
        {
            try
            {
                // Create a new stringbuilder to hold our string
                StringBuilder sb = new StringBuilder();

                // Now verify that our _message variable is not null
                if ((_message as MTA_RR_Radio_Message) != null)
                {
                    // Everything looks good so parse out the message. See the MessageType enumerated type for more information
                    switch ((_message as MTA_RR_Radio_Message).Type)
                    {
                        // Encoder Request
                        case MessageType.MSG21:
                            sb.Append(string.Format("IXL Status Request (#21)"));
                            break;
                        //Encoder Response with LoMA
                        case MessageType.MSG22:
                            sb.Append(string.Format("IXL Status Response w/LoMA (#22)"));
                            break;
                        // Encoder Response without LoMA
                        case MessageType.MSG23:
                            sb.Append(string.Format("IXL Status Response w/o LoMA (#23)"));
                            break;
                        // TSR List Request
                        case MessageType.MSG11:
                            sb.Append(string.Format("TSR List Request (#11)"));
                            break;
                        // TSR List Response
                        case MessageType.MSG12:
                            sb.Append(string.Format("TSR List Response (#12)"));
                            break;
                        // TSR List No Change Response
                        case MessageType.MSG19:
                            sb.Append(string.Format("TSR List No Change Response (#19)"));
                            break;
                        // OBC Maintenance Request
                        case MessageType.MSG34:
                            sb.Append(string.Format("OBC Maintenance (#34)"));
                            break;
                        // OBC Maintenance Acknowledge
                        case MessageType.MSG35:
                            sb.Append(string.Format("OBC Maintenance ACK (#35)"));
                            break;
                        // Default - Invalid message type
                        default:
                            sb.Append(string.Format("Invalid Message Type!"));
                            break;
                    }

                    // Return the stringbuilder
                    return sb.ToString();
                }
                else
                {
                    // We did not find any message type to parse, so return an error
                    sb.Append(string.Format("Invalid Message Detected!"));
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::msgTypeToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
        #endregion

        #region PropertyChangeManager     
        
        // This class implements the property changed manager feature of C#/WPF
        // The property changed manager is a UI paradigm used to automatically forward updates to background logic
        // to their respective presentation devices within the presentation layer. The property changed event handler 
        // must be implemented to cause a property to send a UI update request when the property has been modified

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region DataStructureOverrides

        // The following data structure overrides are required by Telerik to successfully bind a data dictionary with
        // the UI data table implemented in the UI
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this._data.Keys;
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
                if (this._data.ContainsKey(columnName))
                {
                    return this._data[columnName];
                }

                return null;
            }
            set
            {
                if (!this._data.ContainsKey(columnName))
                {
                    this._data.Add(columnName, value);

                    this.OnPropertyChanged(columnName);
                }
                else
                {
                    if (this._data[columnName] != value)
                    {
                        this._data[columnName] = value;

                        this.OnPropertyChanged(columnName);
                    }
                }
            }
        }
        #endregion
    }
}
