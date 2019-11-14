using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AppLogic
{
    // To call TagList nameTagList = new TagList();


    //
    // CLASS: Tag/ TagList
    //
    // Description: This class creates the Tag definition, sets attributes, and methods for the TagLists to be easily parsed from the OBC.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //  Tag:
    //      Byte _tagID             - Id of Tag in TagList AND the PTE Tag Message
    //      string _name            - Name of Tag
    //      bool _bData             - True if the Tag has Data
    //      int _length             - Length of the Tag Data
    //      TAG_VALUE_TYPES eType   - Value type of Tag Data
    //      Byte[] _data            - The Data of the Tag
    //
    // Public Get/Set Accessors:
    //  Tag:
    //      Byte[] Data() (Get only)
    //      Byte TagID (Get & Set)
    //      String Name (Get & Set)
    //      bool HasData (Get & Set)
    //      int Length (Get only)
    //      TAG_VALUE_TYPES TagType (Get only)

    // Public Methods:
    //  Tag:
    //      void Data(Byte[] newData)                   - Checks if Tag hasData, and writes data to the tag
    //      void AbsoluteDataWrite(Byte[] newData)      - Writes Data to the Tag
    //  TagList:
    //      Tag GetTag(Byte id)                         - Finds the Tag based on the TagID
    //
    // Private Methods:
    //  Tag:
    //      string asHexString()            - Changes Hex Value to String to display
    //      string asDecimalString()        - Changes Decimal to String to display
    //      DateTime asDateTime()           - Changes OBC DateTime to Human Readable DateTime
    //
    // Constructors:
    //      Tag(Byte tag, String name, int length, TAG_VALUE_TYPES type)        - Constructor
    //      Tag(Byte tag, String name, TAG_LEN eLen, TAG_VALUE_TYPES type)      - Constructor
    //      TagList()                                                           - Default Constructor
    //      TagList(TagList copy)                                               - Constructor
    //
    // Public Overrides:
    //      string ToString()
    //

    public enum TAG_VALUE_TYPES
    {
        DECIMAL,
        HEX,
        BYTES,
        DATE,
        NO_TYPE
    };
    public enum TAG_LEN
    {
        BYTE,
        TWO_BYTES,
        UNDEFINED
    };

    public class Tag
    {
        private Byte _tagID;
        private string _name;
        private bool _bData;
        private int _length;
        private TAG_VALUE_TYPES eType;
        private Byte[] _data;
        private bool _extended;


        public Tag()
        {
            
        }

        /// <summary>
        /// Tag Constructor
        /// </summary>
        /// <param name="tag">TagID</param>
        /// <param name="name">Name of Tag</param>
        /// <param name="length">Length of Tag Data as int</param>
        /// <param name="type">Type of Value for Data</param>
        public Tag(Byte tag, String name, int length, TAG_VALUE_TYPES type)
        {
            try
            {
                TagID = tag;
                Name = name;
                _length = length;
                _data = new Byte[Length];
                eType = type;
                HasData = false;
                _extended = false;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::Tag-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Tag Constructor Sets all of the Tag parameters except Data
        /// </summary>
        /// <param name="tag">TagID</param>
        /// <param name="name">Name of Tag</param>
        /// <param name="eLen">Length of Tag Data as enum</param>
        /// <param name="type">Type of Value for Data</param>
        public Tag(Byte tag, String name, TAG_LEN eLen, TAG_VALUE_TYPES type)
        {
            try
            {
                TagID = tag;
                Name = name;

                if (eLen == TAG_LEN.BYTE)
                {
                    _length = 1;
                }
                else if (eLen == TAG_LEN.TWO_BYTES)
                {
                    _length = 2;
                }
                else
                {
                    _length = 0;
                }
                eType = type;
                _data = new Byte[Length];
                HasData = false;
                _extended = false;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::Tag-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Copies new Data to Tag
        /// </summary>
        /// <param name="newData">Data for Tag</param>
        public void Data(Byte[] newData)
        {
            try
            {
                if (HasData)
                {
                    //Console.WriteLine("Tag {0} is being written over.", Name);
                    Array.Copy(newData, _data, Length);
                    HasData = true;
                }
                else
                {
                    Array.Copy(newData, _data, Length);
                    HasData = true;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::Data-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Copies new Data to Tag
        /// </summary>
        /// <param name="newData">Data for Tag</param>
        public void AbsoluteDataWrite(Byte[] newData)
        {
            try
            {
                Array.Copy(newData, _data, Length);
                HasData = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::AbsoluteDataWrite-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        #region Accessors
        public Byte[] Data()
        {
            return _data;
        }

        public Byte[] PData
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
            
        }
        public Byte TagID
        {
            get
            {
                return _tagID;
            }
            set
            {
                _tagID = value;
            }
        }
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public bool HasData
        {
            get
            {
                return _bData;
            }
            set
            {
                _bData = value;
            }
        }
        public int Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }
        public TAG_VALUE_TYPES TagType
        {
            get
            {
                return eType;
            }
            set
            {
                eType = value;
            }
        }

        public bool Extended
        {
            get
            {
                return _extended;
            }
            set
            {
                _extended = value;
            }
        }
        #endregion
        public object TypeConvertedValue()
        {            
            switch (eType)
            {
                default:
                case TAG_VALUE_TYPES.NO_TYPE:
                    return "Type Not Supported?";

                case TAG_VALUE_TYPES.HEX:
                    if (_length == 1)
                    {
                        return asInteger();
                    }
                    else if (_length == 2)
                    {
                        return asUInt16();
                    }
                    else
                    {
                        return null;
                    }

                case TAG_VALUE_TYPES.DECIMAL:
                    if (_length == 1)
                    { 
                        return asInteger();
                    }
                    else if (_length == 2)
                    {
                        return asUInt16();
                    }
                    else
                    {
                        return null;
                    }
                case TAG_VALUE_TYPES.BYTES:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(string.Format("Conversion not supported for this type!"));
                        return sb.ToString();
                    }

                case TAG_VALUE_TYPES.DATE:
                    return asDateTime().ToLocalTime();
            }

        }

        public string ValueToString()
        {
            switch (eType)
            {
                default:
                case TAG_VALUE_TYPES.NO_TYPE:
                    return "Type Not Supported?";

                case TAG_VALUE_TYPES.HEX:
                    return valueAsHexString();

                case TAG_VALUE_TYPES.DECIMAL:
                    return valueAsDecimalString();

                case TAG_VALUE_TYPES.BYTES:
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < Length; i++)
                        {
                            sb.Append(String.Format("{0:X2}", _data[i]));
                        }
                        return sb.ToString();
                    }

                case TAG_VALUE_TYPES.DATE:
                    return String.Format("{0}", asDateTime().ToLocalTime().ToString("MM/dd/yy hh:mm:ss.fff tt"));
            }
        }

        /// <summary>
        /// Returns a Hex as a String for Display.
        /// </summary>
        /// <returns>Hex as String</returns>
        private string valueAsHexString()
        {
            try
            {
                if (Length == 1)
                    return String.Format("0x{0:X2}", _data[0]);
                else if (Length == 2)
                    return String.Format("0x{0:X2}{1:X2}", _data[0], _data[1]);
                else
                    return String.Format("0x{0:X2}", 0);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::asHexString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Returns the Decimal to a String for Display.
        /// </summary>
        /// <returns>Decimal as a String</returns>
        private string valueAsDecimalString()
        {
            try
            {
                if (Length == 1)
                    return String.Format("{0}", _data[0]);
                else if (Length == 2)
                {
                    Int16 x = (Int16)((_data[0] << 8) + _data[1]);
                    return String.Format("{0}", x);
                }
                else
                    return String.Format("{0}", 0);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::asDecimalString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Overrides ToSring
        /// </summary>
        /// <returns>Strings of the values</returns>
        public override string ToString()
        {
           
            switch (eType)
            {
                default:
                case TAG_VALUE_TYPES.NO_TYPE:
                    return "Type Not Supported?";

                case TAG_VALUE_TYPES.HEX:
                    return asHexString();

                case TAG_VALUE_TYPES.DECIMAL:
                    return asDecimalString();

                case TAG_VALUE_TYPES.BYTES:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(String.Format("{0}: [", Name));
                        for (int i = 0; i < Length; i++)
                        {
                            sb.Append(String.Format("{0:X2}", _data[i]));
                        }
                        sb.Append("]");
                        return sb.ToString();
                    }

                case TAG_VALUE_TYPES.DATE:
                    return String.Format("{0}: {1}", Name, asDateTime().ToString());
            }
        }

        /// <summary>
        /// Changes the data bytes to Hex string
        /// </summary>
        /// <returns>Name, TagID, Hex Data</returns>
        private string asHexString()
        {
            if (Length == 1)
                return String.Format("{0} [{1}][{2:X2}]: 0x{3:X2}", Name, TagID, TagID, _data[0]);
            else if (Length == 2)
                return String.Format("{0} [{1}][{2:X2}]: 0x{3:X2}{4:X2}", Name, TagID, TagID, _data[0], _data[1]);
            else
                return String.Format("{0} [{1}][{2:X2}]: Data Formatting Error.>", Name, TagID, TagID);
        }

        /// <summary>
        /// Tuens Data bytes into Decimal String
        /// </summary>
        /// <returns>Name, TagID, Decimal Data</returns>
        private string asDecimalString()
        {
            if (Length == 1)
                return String.Format("{0} [{1}][{2:X2}]: {3}", Name, TagID, TagID, _data[0]);
            else if (Length == 2)
            {
                Int16 x = (Int16)((_data[0] << 8) + _data[1]);
                return String.Format("{0} [{1}][{2:X2}]: {3}", Name, TagID, TagID, x);
            }
            else
                return String.Format("{0} [{1}][{2:X2}]: Data Formatting Error.>", Name, TagID, TagID);
        }

        /// <summary>
        /// Takes DateTime from OBC (number of seconds from 1/1/2000) and converts to readable DateTime.
        /// </summary>
        /// <returns>Month/Day/Year Hour:Minute:Seconds</returns>
        private DateTime asDateTime()
        {
            try
            {
                byte hh, h, l, ll;
                long t;

                DateTime dtWork = new DateTime(2000, 1, 1);
                hh = _data[0];
                h = _data[1];
                l = _data[2];
                ll = _data[3];

                t = (hh << 24) + (h << 16) + (l << 8) + ll;
                dtWork = dtWork.AddSeconds(t);
                dtWork = dtWork.AddMilliseconds(_data[4] * 100);
                return dtWork;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::asDateTime-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return DateTime.Now;
            }
        }
        private uint asInteger()
        {
            return (uint)(_data[0]);
        }

        private UInt16 asUInt16()
        {
            UInt16 rc = (UInt16)(_data[0] << 8 | _data[1]);
            return rc;
        }
    }

    public class ExtendedTag : Tag
    {

        #region Properties

        private int _baseTag;
        public int BaseTag
        {
            get
            {
                return _baseTag;
            }
            set
            {
                _baseTag = value;
            }
        }

        private int _offset;
        public int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
            }
        }

        #endregion  

        public ExtendedTag()
        {

        }

        public ExtendedTag(int baseTag, int offset)
        {
            try
            {
                if (baseTag != null && offset != null)
                {
                    this._baseTag = baseTag;
                    this._offset = offset;
                    this.Extended = true;
                }
            }
            catch
            {

            }
            
        }

        /// <summary>
        /// Tag Constructor
        /// </summary>
        /// <param name="tag">TagID</param>
        /// <param name="name">Name of Tag</param>
        /// <param name="length">Length of Tag Data as int</param>
        /// <param name="type">Type of Value for Data</param>
        public ExtendedTag(Byte tag, String name, int baseTag, int offset)
        {
            try
            {
                TagID = tag;
                Name = name;
                PData = new Byte[Length];
                HasData = false;
                this.Extended = true;

                if (baseTag != null && offset != null)
                {
                    this._baseTag = baseTag;
                    this._offset = offset;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::Tag-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
    }


    /// <summary>
    /// Creates a List of Tags
    /// </summary>
    public class TagList
    {
        public List<Tag> Tags;

        public const int TPMSGLEN = 25;
        public const int RDMSGLEN = 39;

        /// <summary>
        /// Creates a list of Tags without any defined data yet.
        /// </summary>
        public TagList()
        {
            try
            {
                Tags = new List<Tag>();
                Tags.Add(new Tag(0, "DATETIME", 5, TAG_VALUE_TYPES.DATE));
                Tags.Add(new Tag(1, "LOCATION", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(2, "CHAINAGE", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(3, "SPEED", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(4, "RR LINE #", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(5, "TRACK #", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(6, "X|AUX IO IN 1", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(7, "X|AUX IO IN 2", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(8, "X|TACH IO IN 1", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(9, "X|TACH IO IN 2", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(10, "X|TACH 1 STAT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(11, "X|ADU 1 INPUT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(12, "X|ADU 2 INPUT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(13, "Unused 13", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(14, "Unused 14", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(15, "Unused 15", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(16, "X|TACH IO OUT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(17, "X|MAIN IO OUT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(18, "X|DT STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(19, "DT EVENTS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(20, "G LINE SPEED", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(21, "TRACK LIMIT", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(22, "Mag Valve State", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(23, "TRAIN TYPE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(24, "VEHICLE TYPE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(25, "ACSES MODE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(26, "ALERT CURVE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(27, "BRAKE CURVE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(28, "TP COUNT", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(29, "TP DIRECTION", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(30, "X|TRIG STAT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(31, "VEHICLE MAX SPEED", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(32, "Unused 32", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(33, "TTSS INPUT", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(34, "X|RADIO LINK", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(35, "X|DECEL CONTROL", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(36, "PTS ALERT CURVE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(37, "PTS BRAKE CURVE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(38, "X|DASH CAUSE", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(39, "SIGNAL STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(40, "HS EXIT TRACK", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(41, "DECEL STATE", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(42, "X|DECEL STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(43, "FAULT CODE ID", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(44, "FAULT SW ID", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(45, "FAULT CODE", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(46, "FAULT ARG", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(47, "X|PTS STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(48, "ACSES ACK REQUIRED", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(49, "ACSES ACK DONE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(50, "CONFIG STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(51, "TARGET SPEED", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(52, "TP READER STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(53, "COMMAND MESSAGE STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(54, "TAG CPTR STATUS", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(55, "MCP LinkStatus", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(56, "X|FTR SET 3", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(57, "X|FTR SET 2", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(58, "X|FTR SET 1", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(59, "F ANT OFFSET", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(60, "R ANT OFFSET", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(61, "DATALOG DEVICE", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(62, "TRAIN DIRECTION", TAG_LEN.BYTE, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(63, "Unused 63", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(64, "Unused 64", TAG_LEN.BYTE, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(65, "ACSES PENALTY", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(66, "YARDS PER PULSE CF", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(67, "LINK TARGET", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(68, "RUNNING DIST", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(69, "LOCO NUMBER", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(70, "Unused 70", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(71, "PULSE COUNT", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(72, "WHEEL DIAMETER", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(73, "DECEL VALUE", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(74, "X|AIU STATUS", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(75, "X|AIU ASPECT", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(76, "X|SPI LINK DOWN", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(77, "X|ASYNC LINK", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(78, "X|MTA FLAGS", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(79, "DIST TO PTS", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(80, "X|ADU 1 ASPECT", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(81, "X|ADU 2 ASPECT", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(82, "ADU 1 AIU DISP", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(83, "ADU 2 AIU DISP", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(84, "X|ADU OUT 1", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(85, "X|ADU OUT 2", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(86, "TARGET DIST ORIGINAL", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(87, "TARGET DIST MODIFIED", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.HEX));
                Tags.Add(new Tag(88, "TARGET DIST FINAL", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(89, "TACH PPR", TAG_LEN.TWO_BYTES, TAG_VALUE_TYPES.DECIMAL));
                Tags.Add(new Tag(90, "TP MESSAGE", TPMSGLEN, TAG_VALUE_TYPES.BYTES));
                Tags.Add(new Tag(91, "RADIO MESSAGE", RDMSGLEN, TAG_VALUE_TYPES.BYTES));

                Tags.Add(new ExtendedTag(92, "ACKNOWLEDGE", 6, 0));
                Tags.Add(new ExtendedTag(93, "ACTIVECAB", 6, 1));
                Tags.Add(new ExtendedTag(94, "STOPBYPASS", 6, 2));
                Tags.Add(new ExtendedTag(95, "ATC CIO", 8, 0));
                Tags.Add(new ExtendedTag(96, "ATC CO", 8, 1));
                Tags.Add(new ExtendedTag(97, "ACSES CIO", 9, 0));
                Tags.Add(new ExtendedTag(98, "ACSES CO OUT", 9, 1));
                Tags.Add(new ExtendedTag(99, "PERMSUPPR", 9, 2));
                Tags.Add(new ExtendedTag(100, "SPEEDSENSOR", 10, 0));
                Tags.Add(new ExtendedTag(101, "PTSALARMSUP", 11, 0));
                Tags.Add(new ExtendedTag(102, "VFBREAK", 16, 0));
                Tags.Add(new ExtendedTag(103, "PHABREAK", 16, 1));
                Tags.Add(new ExtendedTag(104, "TILTAUTH", 16, 2));
                Tags.Add(new ExtendedTag(105, "DT STEP",  18, 0));
                Tags.Add(new ExtendedTag(106, "DT SUSPENDED", 18, 1));
                Tags.Add(new ExtendedTag(107, "DT SUCCESS", 18, 2));
                Tags.Add(new ExtendedTag(108, "DT RUNNING", 18, 3));
                Tags.Add(new ExtendedTag(109, "TPFOUND", 30, 0));
                Tags.Add(new ExtendedTag(110, "TPINWIN",  30, 1));
                Tags.Add(new ExtendedTag(111, "TRAILINGOPR", 30, 2));
                Tags.Add(new ExtendedTag(112, "ALARM", 30, 3));
                Tags.Add(new ExtendedTag(113, "OVERSPEED", 30, 4));
                Tags.Add(new ExtendedTag(114, "ACSES PEN", 30, 5));
                Tags.Add(new ExtendedTag(115, "ANTENNAPOWER", 30, 6));
                Tags.Add(new ExtendedTag(117, "DASHDASH", 30, 7));
                Tags.Add(new ExtendedTag(118, "RADIOCHAN", 34, 0));
                Tags.Add(new ExtendedTag(119, "BCPCOVERAGE", 34, 1));
                Tags.Add(new ExtendedTag(120, "RADIO STATUS", 34, 2));
                Tags.Add(new ExtendedTag(121, "ACSES MVCO", 38, 0));
                Tags.Add(new ExtendedTag(122, "TPOUTOFWIN", 38, 1));
                Tags.Add(new ExtendedTag(123, "DATABASE OK", 38, 2));
                Tags.Add(new ExtendedTag(124, "TPMISSING", 38, 3));
                Tags.Add(new ExtendedTag(125, "TSRLISTOK", 38, 4));
                Tags.Add(new ExtendedTag(126, "PTSZONE", 47, 0));
                Tags.Add(new ExtendedTag(127, "RADIORELEASE", 47, 1));
                Tags.Add(new ExtendedTag(128, "C SIGNAL", 47, 2));
                Tags.Add(new ExtendedTag(129, "DECEL DIRECT", 58, 0));
                Tags.Add(new ExtendedTag(130, "ROLLAWAY PEN", 65, 0));
                Tags.Add(new ExtendedTag(131, "FAULT AUXIO", 76, 0));
                Tags.Add(new ExtendedTag(132, "FAULT TACHIO", 76, 1));
                Tags.Add(new ExtendedTag(133, "FAULT CONFIG", 76, 2));
                Tags.Add(new ExtendedTag(134, "FAULT ENET", 76, 3));
                Tags.Add(new ExtendedTag(135, "FAULT ADU 1", 76, 4));
                Tags.Add(new ExtendedTag(136, "FAULT ADU 2", 76, 5));
                Tags.Add(new ExtendedTag(137, "FAULT DECEL", 76, 6));
                Tags.Add(new ExtendedTag(138, "FAULT ATC", 76, 7));
                Tags.Add(new ExtendedTag(139, "FAULT AIU", 76, 8));
                Tags.Add(new ExtendedTag(140, "FAULT TPR", 77, 0));
                Tags.Add(new ExtendedTag(141, "BRAKE ASR", 78, 0));
                Tags.Add(new ExtendedTag(142, "ABS CMB TER", 78, 1));
                Tags.Add(new ExtendedTag(143, "ZS TSR IP", 78, 2));
                Tags.Add(new ExtendedTag(144, "TUNNEL AVOID", 78, 3));
                Tags.Add(new ExtendedTag(145, "TB PTS EN", 78, 4));
                Tags.Add(new ExtendedTag(146, "TB PTS ACT", 78, 5));
                Tags.Add(new ExtendedTag(147, "SAR TSR STS", 78, 6));
                Tags.Add(new ExtendedTag(148, "TSR ENFORCED", 78, 7));
                Tags.Add(new ExtendedTag(149, "DB SLIP IP", 78, 8));
                Tags.Add(new ExtendedTag(150, "DB SLIDE IP", 78, 9));
                Tags.Add(new ExtendedTag(151, "TCP/IP", 78, 10));
                Tags.Add(new ExtendedTag(152, "ACSESPTSREQ", 84, 0));
                Tags.Add(new ExtendedTag(153, "ACKREQ", 84, 1));
                Tags.Add(new ExtendedTag(154, "ACSES CO FLS", 84, 2));
                Tags.Add(new ExtendedTag(155, "ADUVZ", 84, 3));
                Tags.Add(new ExtendedTag(156, "DASHDASH_2", 84, 4));
                Tags.Add(new ExtendedTag(157, "ATSSEQNUM", 91, 0));
                Tags.Add(new ExtendedTag(158, "BCPNUM", 91, 1));
                Tags.Add(new ExtendedTag(159, "RADIORXERROR", 91, 2));
                Tags.Add(new ExtendedTag(160, "RADIORXINF", 91, 3));
                Tags.Add(new ExtendedTag(161, "RADIOTRAFFIC", 91, 4));
                Tags.Add(new ExtendedTag(162, "X|TSRTYPE", 91, 5));
                Tags.Add(new ExtendedTag(163, "RADIOTXRXENC", 91, 6));
                Tags.Add(new ExtendedTag(164, "RADIOTXRXTSR", 91, 7));
                Tags.Add(new ExtendedTag(165, "RADIOTXRXMTA", 91, 8));
                Tags.Add(new ExtendedTag(166, "SERVICE PEN", 17, 0));
                Tags.Add(new ExtendedTag(167, "EM PENALTY", 17, 1));
                Tags.Add(new ExtendedTag(168, "LINK ACTUAL", 28, 0));
                Tags.Add(new ExtendedTag(169, "REVHANDLE", 6, 3));
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::TagList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Copies the TagList to a new TagList.
        /// </summary>
        /// <param name="copy">The List to copy from</param>
        public TagList(TagList copy)
        {
            try
            {
                Tags = new List<Tag>();

                foreach (var tag in copy.Tags)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::TagListCopy-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        public void copyData(TagList Copy)
        {
            foreach (var tag in Tags)
            {
                tag.Data(Copy.GetTag(tag.TagID).Data());
            }
        }

        /// <summary>
        /// Returns the Tag based on the TagID.
        /// </summary>
        /// <param name="id">Tag ID used to identify Tag</param>
        /// <returns>The Tag</returns>
        public Tag GetTag(Byte id)
        {
            try
            {
                foreach (var tag in Tags)
                {
                    if (tag.TagID == id)
                        return tag;
                }
                Console.WriteLine("Tag::GetTag - Tag {0} was not supported.", id);

                Tag defaultTag = new Tag(id, "<Not Supported!>", 0, TAG_VALUE_TYPES.NO_TYPE);
                return defaultTag;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Tags::GetTag-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        public Byte TagIDByName(string Name)
        {
            foreach (var tag in Tags)
            {
                if (tag.Name == Name)
                    return tag.TagID;
            }

            Console.WriteLine("Tag::GetTag - Tag {0} was not supported.", 255);

            Tag defaultTag = new Tag(255, "<Not Supported!>", 0, TAG_VALUE_TYPES.NO_TYPE);
            return defaultTag.TagID;
        }

        /// <summary>
        /// Compares the differences between tag lists and returns a list containing the mismatched tags.
        /// </summary>
        /// <param name="Comparator"></param>
        public List<Byte> CompareDifferences(TagList Comparator)
        {
            List<Byte> MisMatchedTags = new List<Byte>();
            List<Byte> MatchedTags = new List<Byte>();

            foreach (var T in Tags)
            {
                Byte[] ComparatorData = T.Data();
                Byte[] NewTagData = Comparator.GetTag(T.TagID).Data();

                if (ComparatorData.SequenceEqual(NewTagData))
                {
                    MatchedTags.Add(T.TagID);
                }
                else
                {
                    MisMatchedTags.Add(T.TagID);
                }
            }
            return MisMatchedTags;
        }
    }
}
