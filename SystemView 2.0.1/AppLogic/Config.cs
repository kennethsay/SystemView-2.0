using System;
using System.Collections.Generic;
using System.Text;

using Transport;

namespace AppLogic
{
    // To call Config nameConfig = new Config();


    //
    // CLASS: Config/ ConfigElement
    //
    // Description: This class obtains the Configurations from the OBC and allows the user with proper permissions to change the Configuration as needed.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //  ConfigElements:
    //      String _sName                   - Name of the Configuration Item
    //      CONFIG_DATA_TYPES _eType        - Type of Config Item (1 byte or 2 bytes)
    //      int _nData                      - Data held in the Config Item
    //      int _nBytePosition              - BytePosition of Data in the OBC string
    //  Config:
    //      bool _canChangeConfig           - If the user has the permission to change the Config Item
    //
    // Public Get/Set Accessors:
    //  ConfigElement:
    //      String Name (Get only)
    //      CONFIG_DATA_TYPES Type (Get only)
    //      int Value (Get & Set)
    //      int BytePosition (Get only)
    //  Config:
    //      bool ChangeConfigPermission (Get & Set)
    //
    // Public Methods:
    //  Config:
    //      ConfigElement Find(String s)                             - Finds the Config Elements by the Name in the List
    //      void submitRequest(CONFIG_CHANGE_STATE configMode)       - Checks if user has the proper permissions to change configs and updates the status of the change request
    //      String OutputString()                                    - Builds an output string based on the config elements and their values
    //
    // Private Methods:
    //  Config:
    //      void fetch()                - Obtains the Config message from the OBC, updates the data for the config elements, and parses the Config string
    //      void sync(int CFGMode)      - Is the user has the permissions to change the config, then this method sends the changes to the OBC to be updated
    //
    // Constructors:
    //      Config()                                                                - Default constructor
    //      ConfigElement()                                                         - Default Constructor
    //      ConfigElement(String sName, CONFIG_DATA_TYPES eType, int bytePosition)  - Constructor that creates Config Elements with the name, type, and byte positions (not the data)
    //
    // Public Overrides:
    //  ConfigElement:
    //      string ToString()
    //  Config:
    //      string ToString()
    //

    public enum CONFIG_DATA_TYPES
    {
        SINGLE_BYTE,
        DOUBLE_BYTE,
        NO_TYPE,
    };

    public class ConfigElement
    {
        private String _sName;
        private CONFIG_DATA_TYPES _eType;
        private int _nData;
        private int _nBytePosition;

        /// <summary>
        /// Default Constructor that initialtizes the Config items with nothing useful.
        /// </summary>
        public ConfigElement()
        {
            _sName = "Unnamed";
            _eType = CONFIG_DATA_TYPES.NO_TYPE;
            _nData = 0;
            _nBytePosition = 0;
        }

        /// <summary>
        /// Constructor that Updates the Configuration Element List with the Name, Type, and Byte Position.
        /// </summary>
        /// <param name="sName">Name of Configuration</param>
        /// <param name="eType">1 byte or 2 bytes long</param>
        /// <param name="bytePosition">Position in Config string from OBC</param>
        public ConfigElement(String sName, CONFIG_DATA_TYPES eType, int bytePosition)
        {
            _sName = sName;
            _eType = eType;
            _nData = 0;
            _nBytePosition = bytePosition;

        }

        #region Accessors
        public String Name
        {
            get
            {
                return _sName;
            }
        }
        public CONFIG_DATA_TYPES Type
        {
            get
            {
                return _eType;
            }
        }
        public int Value
        {
            get
            {
                return _nData;
            }
            set
            {
                _nData = value;
            }
        }
        public int BytePosition
        {
            get
            {
                return _nBytePosition;
            }
        }
        #endregion

        /// <summary>
        /// Overrides 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                String s;

                switch (_eType)
                {
                    case CONFIG_DATA_TYPES.DOUBLE_BYTE:
                        s = string.Format("{0}: Double Byte,  Byte Position {1}, Value [0x{2:X2}]", _sName, _nBytePosition, _nData);
                        sb.Append(s);
                        break;

                    case CONFIG_DATA_TYPES.SINGLE_BYTE:
                        s = string.Format("{0}: Single Byte, Byte Position {1}, Value [0x{2:X2}]", _sName, _nBytePosition, _nData);
                        sb.Append(s);
                        break;

                    default:
                        s = string.Format("{0}: Undefined Type, Byte Position {1}, Value [0x{2:X2}]", _sName, _nBytePosition, _nData);
                        sb.Append(s);
                        break;
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("ConfigElement::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }

    public class Config
    {
        private List<ConfigElement> _elements;

                       // TODO - XML FILE!!!!!
        // List of configuration elements
        public List<ConfigElement> Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                _elements = value;
            }
        }



        // Extra Permissions for Siemens' employees
        private bool _canChangeConfig;
        // Public accessor for being able to change the configurations
        public bool ChangeConfigPermission
        {
            get
            {
                return _canChangeConfig;
            }
            set
            {
                _canChangeConfig = value;
            }
        }
        // enum for state change of the Configuration Change Status
        public enum CONFIG_CHANGE_STATE
        {
            NO_ACTION,
            REQUEST_CHANGE,
            SAVE_REQUEST,
            ABORT_REQUEST,
            COMPLETE_CONFIG,
        };

        /// <summary>
        /// Default Constructor for Config; fetches the PTE Message and shows the user all of the configurations.
        /// </summary>
        public Config()
        {
            try
            {
                Elements = new List<ConfigElement>();

            //
            // TODO - Convert to Data Driven method
            //
            Elements.Add(new ConfigElement("Status", CONFIG_DATA_TYPES.SINGLE_BYTE, 0));
            Elements.Add(new ConfigElement("VehicleType", CONFIG_DATA_TYPES.SINGLE_BYTE, 1));
            Elements.Add(new ConfigElement("LocoNumber", CONFIG_DATA_TYPES.DOUBLE_BYTE, 2));
            Elements.Add(new ConfigElement("MaxSpeed", CONFIG_DATA_TYPES.SINGLE_BYTE, 4));
            Elements.Add(new ConfigElement("YardsPerPulse", CONFIG_DATA_TYPES.DOUBLE_BYTE, 5));
            Elements.Add(new ConfigElement("PulsesPerRevolution", CONFIG_DATA_TYPES.DOUBLE_BYTE, 7));
            Elements.Add(new ConfigElement("WheelDiameter", CONFIG_DATA_TYPES.DOUBLE_BYTE, 9));
            Elements.Add(new ConfigElement("TrainType", CONFIG_DATA_TYPES.SINGLE_BYTE, 11));
            Elements.Add(new ConfigElement("Owner", CONFIG_DATA_TYPES.DOUBLE_BYTE,12));
            Elements.Add(new ConfigElement("ATCSAddress", CONFIG_DATA_TYPES.DOUBLE_BYTE, 14));
            Elements.Add(new ConfigElement("FrontOffset", CONFIG_DATA_TYPES.SINGLE_BYTE, 16));
            Elements.Add(new ConfigElement("RearOffset", CONFIG_DATA_TYPES.SINGLE_BYTE, 17));
            Elements.Add(new ConfigElement("DatalogDevice", CONFIG_DATA_TYPES.SINGLE_BYTE, 18));
            Elements.Add(new ConfigElement("FeatureSet1", CONFIG_DATA_TYPES.SINGLE_BYTE, 19));
            Elements.Add(new ConfigElement("FeatureSet2", CONFIG_DATA_TYPES.SINGLE_BYTE, 20));
            Elements.Add(new ConfigElement("FeatureSet3", CONFIG_DATA_TYPES.SINGLE_BYTE, 21));

                fetch();

            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::Config-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Finds a config element by name
        /// </summary>
        /// <param name="s">Name of the configurations.</param>
        /// <returns></returns>
        public ConfigElement Find(String s)
        {
            try
            {
                foreach (ConfigElement e in Elements)
                {
                    if (e.Name == s)
                    {
                        return e;
                    }
                }
                return new ConfigElement();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("ConfigElement::Find-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Reads the configuration data from the OBC, updates the data in the ConfigElement List and parses the Config String.
        /// </summary>
        private void fetch()
        {
            try
            {
                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendGetConfigRequest());

                // Walk through the List and parse the elements
                Int16 low, high;

                if (msgBase.CommandID == 0x19)      // TODO - CHANGE HARD CODE TO DEFINED VALUE
                { 
                    foreach (ConfigElement e in Elements)
                    {
                        switch (e.Type)
                        {
                            case CONFIG_DATA_TYPES.DOUBLE_BYTE:
                                high = msgBase.Data[e.BytePosition];
                                low = msgBase.Data[e.BytePosition + 1];
                                e.Value = ((high << 8) + low);
                                break;

                            case CONFIG_DATA_TYPES.SINGLE_BYTE:
                                e.Value = msgBase.Data[e.BytePosition];
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Submits the Configuration request to sync based on the CONFIG_CHANGE_STATE.
        /// Pretty much handles the button clicks on the UI.
        /// </summary>
        /// <param name="configMode">CONFIG_CHANGE_STATE, default 0.</param>
        public void submitRequest(CONFIG_CHANGE_STATE configMode)
        {   
            // By default 0, will only look at this if the user has permission to change the configuration settings
            try
            {
                // If user is logged in as Employyee or Admin, this should be true
                if (ChangeConfigPermission == true)
                {
                    // User has permission to change config elements, so allow, Goes through each step to change config
                    switch (configMode)
                    {
                        case CONFIG_CHANGE_STATE.REQUEST_CHANGE:
                            sync(1);
                            break;
                        case CONFIG_CHANGE_STATE.SAVE_REQUEST:
                            sync(2);
                            break;
                        case CONFIG_CHANGE_STATE.ABORT_REQUEST:
                            sync(3);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::submitRequest-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// If the user has Permission to change the configurations, then sync is called.
        /// The CONFIG_CHANGE_STATE will be updated with the UI button clicks. 
        /// This method sends the configuration changes to the OBC.
        /// </summary>
        /// <param name="CFGMode">an integer that represents the CONFIG_CHANGE_STATE</param>
        private void sync(int CFGMode)
        {
            try
            {
                // User Data is 24 in msgLength!
                Byte[] userData = new byte[23];
                // Starting the index at 1, because index = 0 is reserved for the CFGMode (found below)
                int i = 1;

                foreach (ConfigElement e in Elements)
                {
                    if (e.Name != "Status")
                    {
                        switch (e.Type)
                        {
                            case CONFIG_DATA_TYPES.DOUBLE_BYTE:
                                userData[i] = Convert.ToByte((e.Value & 0xFF00) >> 8);
                                i++;
                                userData[i] = Convert.ToByte(e.Value & 0x00FF);
                                i++;
                                break;

                        case CONFIG_DATA_TYPES.SINGLE_BYTE:
                            userData[i] = Convert.ToByte(e.Value);
                            i++;
                            break;

                            default:
                                break;
                        }
                    }
					
                }
                // REQUEST change
                if (CFGMode == 1)
                {
                    userData[0] = 1;
                    PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendSetConfigRequest(userData));

                    if (msgBase.CommandID != 0x81)  // TODO - CHANGE HARD CODE TO DEFINED VALUE
                    {
                        Console.WriteLine("OBCCommunication.SendSetConfigRequest called by Config.Sync returned wrong command ID.");
                    }
                }
                // SAVE config change
                else if (CFGMode == 2)
                {
                    userData[0] = 2;
                    PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendSetConfigRequest(userData));

                    if (msgBase.CommandID != 0x81)  // TODO - CHANGE HARD CODE TO DEFINED VALUE
                    {
                        Console.WriteLine("OBCCommunication.SendSetConfigRequest called by Config.Sync returned wrong command ID.");
                    }
                }
                // ABORT change
                else if (CFGMode == 3)
                {
                    userData[0] = 3;
                    PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendSetConfigRequest(userData));

                    if (msgBase.CommandID != 0x81)  // TODO - CHANGE HARD CODE TO DEFINED VALUE
                    {
                        Console.WriteLine("OBCCommunication.SendSetConfigRequest called by Config.Sync returned wrong command ID.");
                    }
                }
                else
                {
                    // Do Nothing. Invalid. Can't happen. Sorry.
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::sync-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Overrides
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Configuration Settings:");

                foreach (ConfigElement e in Elements)
                {
                    sb.Append("     ");
                    sb.AppendLine(e.ToString());
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Builds an output string based on the Config Elements and their current values.
        /// </summary>
        /// <returns></returns>
        public String OutputString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                String s;

                foreach (ConfigElement e in Elements)
                {
                    switch (e.Type)
                    {
                        case CONFIG_DATA_TYPES.DOUBLE_BYTE:
                            s = String.Format("{0:X2}", e.Value);
                            sb.Append(s);
                            break;

                        case CONFIG_DATA_TYPES.SINGLE_BYTE:
                            s = String.Format("{0:X1}", e.Value);
                            sb.Append(s);
                            break;

                        default:
                            break;
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Config::OutputString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        // checks that permanent suppression and vzero are set before allowing user to access config screen
        public bool permSuppVZCheck()
        {
            RTM rtm = new RTM();

            if (!rtm.RTMStarted)
            {
                return false;
            }

            TagList record = rtm.CollectRecord();

            byte[] tachin2 = record.Tags.Find(X => X.TagID == 9).Data();
            byte[] tach1Stat = record.Tags.Find(X => X.TagID == 10).Data();
            byte[] aduOut1 = record.Tags.Find(X => X.TagID == 84).Data();

            if (((tachin2[0] & 0x04) == 0x04) & !(((tach1Stat[0] & 0x01) == 0x01) | ((aduOut1[0] >> 3) & 0x01) == 0x01))
            {
                return true;
            }

            else
            {
                return false;
            }
        }
    }
}
