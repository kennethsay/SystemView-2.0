using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.ComponentModel;

using AppLogic;
using SystemView.ContentDisplays;

namespace SystemView
{

    // To call TPDataItem = new TPDataItem();
    //
    // CLASS: TPDataItem
    //
    // Description: This class implements the data structure used to generate Transponder Data Presentation table real-time records. 
    //              Each structure represents a single transponder message that is included as an instance of the TPMessage class. 
    //              A data binding to the presentation data table is used to add records to the transponder view table. 
    //
    // Private Data:
    //      IDictionary<string, object> _data           - Transponder data dictionary used to store data table information in the form of a column header and row data
    //      TPMessage _tpMsg                            - Object to store the Transponder Message 
    //      string _msgData                             - Object to store Data parsed from the Transponder Message
    //
    // Public Get/Set Accessors:
    //      object Message(get)                         - Accessor for the _tpMsg variable
    //      byte[] MsgData(get)                         - Accesssor for the _msgData variable
    //
    // Public Methods:
    //      string GetMessage()                         - Returns Transponder Message Data as stored in the Transponder data dictionary
    //      string HexToString(byte[] Hex)              - Converts a hexidecimal array value argument to a string 
    //      string ToString()                           - Override of ToString method for TPDataItem Class
    //
    // Private Methods:
    //
    // Constructors:
    //      TransponderDataItem() 
    //      TransponderDataItem(object Msg)
    //
    // Other DataTypes:
    //
    public class TPDataItem : DynamicObject, INotifyPropertyChanged
    {
        #region PrivateMembers
        private string _msgData;
        private TPMessage _tpMsg;
        readonly IDictionary<string, object> _data;
        #endregion

        #region Accessors
        public TPMessage Message
        {
            get
            {
                return this._tpMsg;
            }
        }
        #endregion

        #region Constructors

        // This is the default constructor. Just create a new blank data object to hold our transponder information
        // The dictionary created here is used to tie a piece of data with a data table column header
        //
        // The string placed in this dictionary index is used to create the the following radio data column headers:
        //                 Event #, Timestamp, Info, Message
        //
        // The object associated with each string is the value of the parameter specified by the string. This data is
        // populated in each row of the transponder data table with its corresponding column. 

        /// <summary>
        /// Default TPDataItem constructor that creates an empty Dictionary
        /// </summary>
        public TPDataItem()
        {
            _data = new Dictionary<string, object>();
        }


        /// <summary>
        /// This constructor takes a transponder message as an argument and parses out the information necessary to create a
        /// Transponder Data Item. 
        /// </summary>
        /// <param name="Msg">Transponder message to be parsed into a TPDataItem</param>
        public TPDataItem(TPMessage Msg)
        {
            try
            {
                // First check to make sure that the Msg object is not null
                if (Msg != null)
                {
                    // Looks good, so create a new data dictionary and a new railroad specific Transponder message object
                    this._data = new Dictionary<string, object>();
                    this._tpMsg = Msg;
                    
                    // Now convert the Transponder message data from the hex format to a string for display
                    this._msgData = BitConverter.ToString(Msg.Data).Replace("-", string.Empty);

                    // Finally, populate the dictionary with the parsed data from the Radio Mesage
                    _data.Add("Event #", Msg.EventNum.ToString());
                    _data.Add("Timestamp", Msg.Timestamp.ToString());

                    // The info column displays the message size and is a placeholder for future additions. Use
                    // stringbuilder to construct a parameter displaying the size and add it to the info dictionary entry
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format("Size: {0}", Msg.Length.ToString()));

                    _data.Add("Info", sb);
                    _data.Add("Message", _msgData);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("TPDataItem::Constrcutor-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
        #endregion        

        #region PublicMethods
        /// <summary>
        /// Returns Transponder Message Data as stored in the TRansponder data dictionary
        /// </summary>
        /// <returns>The Transponder data message as a string</returns>
        public string GetMessage()
        {
            try
            {
                // Return the Transponder message data from the dictionary typecast to a string
                return (string)_data["Message"];
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("TPDataItem::GetMessage-threw exception {0}", ex.ToString()));

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
                sb.Append(String.Format("TPDataItem::HexToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Override of ToString method for TransponderDataItem Class
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("TPDataItem Properties - Event #: {0}, Timestamp: {1}, Info: {2}, Message: {3}",
                                        (string)_data["Event #"], (string)_data["Timestamp"], (string)_data["Info"], (string)_data["Message"]));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("TPDataItem::ToString-threw exception {0}", ex.ToString()));

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
