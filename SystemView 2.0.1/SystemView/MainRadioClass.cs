using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SystemView
{
    // To call MainRadioClass nameRadioClass = new MainRadioClass(string radioMsg);
    // Send the hex string to the constructor for parsing...

    //
    // CLASS: MainRadioClass
    //
    // Description: This class creates the Radio Messages to display to the user through the Tags received from the OBC.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //  const int _radioLength = 312            - Length (in bits) of the Radio Tag Message
    //  const int _headerLength = 48            - Length of the Header bits
    //  int _length                             - Keeps track of the Length left over
    //  string _radioType                       - Holds the radio type string from FindRadioType
    //  UInt16 _code                            - Code for specific radio type
    //  UInt16 _label                           - Label for the specific radio type
    //
    // Public Get/Set Accessors:
    //  int Length (Get & Set)
    //  string RadioMsg (Get & Set)
    //  UInt16 Code (Get & Set)
    //  UInt16 Label (Get & Set)
    //
    // Public Methods:
    //  RadioTypeType:
    //      static string FindRadioMessage(UInt16 label, UInt16 code)               - Determines the Radio Message Type based on the Label and Code
    //
    // Private Methods:
    //  BitArray hexStringToBits(string hexString)                                  - Converts a hex string to a Bit Array
    //  ushort turnBitsToShort(BitArray bittys, int numOfBits)                      - Converts the bits to a unsigned 16 bit integer
    //  List<RadioEntry> readNodes(string radioType)                                - Reads the XML child nodes of the Radio Message and calls createEntryList
    //  IEnumerable<RadioEntry> createEntryList(string radoiType)                   - Reads the child nodes of Parameter in the XML file and creates a List of the parameters
    //  void parseRadioMsg(List<RadioEntry> radioList, BitArray radioBits)          - Parses the bits with the RadioEntry List into a string[] to display to user
    //  bool checkErrorNum(BitArray radioMsg, ushort err1, .. , ushort err8)        - Checks to see if the parameter should be used (for Type 34 & 35)
    //  int determineNote(string note)                                              - Determines the note from XML and returns an integer corresponding to the note
    //
    // Constructors:
    //  MainRadioClass(string radioMsg)                 - Constructor
    //  RadioEntry:
    //      RadioEntry()                                - Default Constructor that sets all the RadioEntry Parameters to null or 0
    //
    // Public Overrides:
    //

    class MainRadioClass
    {
        private const int _radioLength = 312;
        private const int _headerLength = 48;

        private int _length;
        private string _radioType;
        private UInt16 _code;
        private UInt16 _label;

        public List<string> radioParsed;

        #region Accessors
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

        public string RadioMsg
        {
            get
            {
                return _radioType;
            }
            set
            {
                _radioType = value;
            }
        }

        public UInt16 Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }

        public UInt16 Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructor for the MainRadioClass. Reads the byte[] of Hex string from the taglist obtained from the OBC.
        /// Then checks to make sure the message has contents, reads the header first and then reads the package within
        /// the message.
        /// </summary>
        /// <param name="radioMsg">Hex string representing the Radio Message (no spaces)</param>
        public MainRadioClass(string radioMsg)
        {
            try
            {
                Length = _radioLength;
                BitArray radioBits = new BitArray(Length);

                radioBits = hexStringToBits(radioMsg);

                // Check to make sure the Bits contain a message
                var allZeros = radioBits.Cast<bool>().Contains(true);
                if (!allZeros)
                {
                    Console.WriteLine("No Radio Message Found.");
                    return;
                }

                List<RadioEntry> theseNodes = new List<RadioEntry>();
                theseNodes = readNodes("RadioHeader");
                parseRadioMsg(theseNodes, radioBits);

                Length = Length - _headerLength;

                RadioMsg = RadioType.FindRadioMessage(Label, Code);
                if (RadioMsg == "Error")
                {
                    Console.WriteLine("Package Error.");
                }
                else
                {
                    theseNodes = readNodes(RadioMsg);
                    parseRadioMsg(theseNodes, radioBits);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::MainRadioClass-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Converts a hex string WITHOUT SPACES, to a BitArray.
        /// </summary>
        /// <param name="hexString">TP hex string from OBC</param>
        /// <returns>BitArray representing the hex string</returns>
        private BitArray hexStringToBits(string hexString)
        {
            try
            {
                // Converts Hex string to byte[]
                byte[] bytes = Enumerable.Range(0, hexString.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hexString.Substring(x, 2), 16)).ToArray();

                // Must reverse array because byte[] index look like "0,1,2,3,..,n" while bit array looks like "n,n-1,...,2,1,0"
                bytes = bytes.Reverse().ToArray();
                // bool to hold the 8 bits corresponding to the byte
                bool[] bits = new bool[8];
                BitArray biddys = new BitArray(bytes.Length * 8);

                // Holds the index of the BitArray
                int i = 0;
                for (int x = bytes.Length - 1; x >= 0; x--)
                {
                    // Convert one byte at a time to bits for simplicity
                    BitArray t = new BitArray(new byte[] { bytes[x] });
                    t.CopyTo(bits, 0);
                    // Must reverse AGAIN because of the above statement
                    if (BitConverter.IsLittleEndian) Array.Reverse(bits);
                    foreach (bool bit in bits)
                    {
                        // Copy the bits to the BitArray
                        biddys[i] = bit;
                        i++;
                    }
                }
                return biddys;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::hexStringToBits-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Converts the bits to a unsigned 16 bit integer.
        /// </summary>
        /// <param name="bittys">Bit Array to convert</param>
        /// <param name="numOfBits">The Length of Bit Array</param>
        /// <returns>Unsigned 16 bit integer represented by the bits</returns>
        private ushort turnBitsToShort(BitArray bittys, int numOfBits)
        {
            ushort theseBits = 0;
            try
            {
                numOfBits--;
                ushort power = 0;

                // If the bit is true than raise 2 to the power of the bit index
                // Add all of the powers up
                for (int i = 0; i <= numOfBits; i++)
                {
                    if (bittys[i] == true)
                    {
                        power = Convert.ToUInt16(Math.Pow(2, (numOfBits - i)));
                        theseBits = (ushort)(theseBits + power);
                    }
                }
                return theseBits;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::turnBitsToShort-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return theseBits;
            }
        }

        /// <summary>
        /// Reads the child nodes of the package in the Radio XML file.
        /// Then calls createEntryList to create the RadioEntry list to assist with parsing
        /// the package bits.
        /// </summary>
        /// <param name="radioType">RadioType Name</param>
        /// <returns>RadioEntry List to assist with parsing the package bits</returns>
        private List<RadioEntry> readNodes(string radioType)
        {
            try
            {
                // Load XML Sheet using LINQ
                XDocument xmlDoc = XDocument.Load(@"RadioDictionary.xml");
                // Obtain the Length and Description of the package
                var packageNode = xmlDoc.Descendants("RadioPackage").Descendants(radioType).Select(x => new
                {
                    pn = x.Element("Description").Value
                }).ToList();

                RadioMsg = packageNode[0].pn;

                // Create a TPEntry List to be used for parsing
                List<RadioEntry> newList = new List<RadioEntry>(createEntryList(radioType));
                return newList;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::readNodes-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Reads the child nodes of the RadioEntry node, and creates a List of the parameters for use.
        /// </summary>
        /// <param name="radioType">Pakcage Number</param>
        /// <returns>List of RadioEntry parameters to assist with parsing</returns>
        private IEnumerable<RadioEntry> createEntryList(string radioType)
        {
            try
            {
                // Load XML file with LINQ
                XDocument xmlDoc = XDocument.Load(@"RadioDictionary.xml");

                // Create a List of each TPEntry Node with the TPEntry Class, and check for null to handle exceptions
                var query = xmlDoc.Descendants("RadioPackage").Descendants(radioType).Descendants("Parameter").Select(x => new RadioEntry
                {
                    ID = RadioMsg,
                    Name = x.Element("Name") != null ? x.Element("Name").Value : "",
                    StartIndex = x.Element("Start") != null ? Int32.Parse(x.Element("Start").Value) : 0,
                    EndIndex = x.Element("End") != null ? Int32.Parse(x.Element("End").Value) : 0,
                    DataType = x.Element("DataType") != null ? x.Element("DataType").Value : "",
                    ExtraInfo = x.Element("Note") != null ? x.Element("Note").Value : null,
                    DictList = x.Elements("Dict").ToDictionary(p => p.Element("Key").Value, p => p.Element("Value").Value)
                }).ToList();

                return query;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::createEntryList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Parses the Radio Messages using the RadioEntry List and the bits obtained from
        /// the Taglist byte[] (in a string of hex). Displays the results to the user.
        /// </summary>
        /// <param name="radioList">RadioEntry List created from XML</param>
        /// <param name="radioBits">Radio Message Bits</param>
        private void parseRadioMsg(List<RadioEntry> radioList, BitArray radioBits)
        {
            try
            {
                radioParsed = new List<string>();

                int si = 0;     // Start Index
                int ei = 0;     // End Index
                int index = 0;  // Index of RadioEntry List
                ushort data = 0;
                BitArray deesBits = new BitArray(1);

                radioParsed.Add(radioList[0].ID + ": ");

                while (radioList.Count > index && radioBits.Length > ei + 1)
                {
                    si = radioList[index].StartIndex;
                    ei = radioList[index].EndIndex;

                    // Change to null for ease
                    if (radioList[index].DictList.ContainsKey("NA"))
                    {
                        radioList[index].DictList.Clear();
                    }

                    // Each Nibble above 9 is 0 for BCP number
                    if (radioList[index].DictList.ContainsKey("Nibble>9"))
                    {
                        data = 0;
                        radioList[index].DictList.Clear();
                        int power = (((ei - si) + 1) / 4) - 1;

                        while (si < ei)
                        {
                            deesBits = new BitArray(4);
                            for (int i = 0; i < 4; i++)
                            {
                                deesBits[i] = radioBits[i + si];
                            }
                            ushort bcpData = turnBitsToShort(deesBits, 4);

                            if (bcpData >= 10)
                            {
                                bcpData = 0;
                            }

                            ushort tensPower = Convert.ToUInt16(Math.Pow(10, power));
                            bcpData = (ushort)(bcpData * tensPower);
                            data = (ushort)(data + bcpData);

                            power--;
                            si = si + 4;
                        }
                    }
                    // Simply continue extracting the data
                    else
                    {
                        // Read the parameter bits into a new array and convert to ushort
                        deesBits = new BitArray((ei - si) + 1);
                        for (int i = 0; i < (ei - si + 1); i++)
                        {
                            deesBits[i] = radioBits[i + si];
                        }
                        data = turnBitsToShort(deesBits, (ei - si + 1));

                        if (radioList[index].DictList.ContainsKey("If 0, STOP"))
                        {
                            if (data == 0)
                            {
                                radioParsed.Add(radioList[index].Name + ": " + data);
                                break;
                            }
                        }
                    }

                    // If Keys exist in the Distionary List
                    if (radioList[index].DictList.Count > 0)
                    {
                        if (radioList[index].DictList.ContainsKey("MP"))
                        {
                            double thisMP = data * 30;
                            radioParsed.Add(radioList[index].Name + ": " + String.Format("{0:N2}", ((double)thisMP) / 5280) + " mp");
                        }
                        else
                        {
                            // Get the associated value from the key
                            radioList[index].DictList.TryGetValue(data.ToString(), out var radioData);

                            if (radioList[index].ExtraInfo == null && radioData != null)
                            {
                                radioParsed.Add(radioList[index].Name + ": " + radioData);
                            }
                            else if (radioList[index].ExtraInfo != null)
                            {
                                addToString(radioList[index], radioBits, data, deesBits, ei, si);
                            }
                            else
                            {
                                radioParsed.Add(radioList[index].Name + ": " + data);
                            }
                        }
                    }
                    // Dictionary is Empty
                    else
                    {
                        if (radioList[index].ExtraInfo != null)
                        {
                            addToString(radioList[index], radioBits, data, deesBits, ei, si);
                        }
                        else
                        {
                            radioParsed.Add(radioList[index].Name + ": " + data);

                            if (radioList[index].Name == "Message Label")
                            {
                                Label = data;
                            }

                            if (radioList[index].Name == "Manufacturer's Code")
                            {
                                Code = data;
                            }
                        }
                    }
                    index++;

                }// End While

                radioParsed.Add(" ");
                radioParsed.ForEach(i => Console.WriteLine("{0} ", i));
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::parseRadioMsg-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Determines the Note within the XML sheet and adds to the string accordingly.
        /// </summary>
        /// <param name="thisEntry">The Radio Entry</param>
        /// <param name="radioBits">The full bits from radio message</param>
        /// <param name="data">The current Data read</param>
        /// <param name="deesBits">The current Bits of data</param>
        /// <param name="ei">End Index</param>
        /// <param name="si">Start Index</param>
        private void addToString(RadioEntry thisEntry, BitArray radioBits, ushort data, BitArray deesBits, int ei, int si)
        {
            int info = determineNote(thisEntry.ExtraInfo);
            bool useBits;

            switch (info)
            {
                case 1: // Active bits
                    radioParsed.Add(thisEntry.Name + ": ");
                    for (int i = 0; i < (ei - si + 1); i++)
                    {
                        if (deesBits[i])
                        {
                            thisEntry.DictList.TryGetValue(i.ToString(), out var active);
                            radioParsed.Add(active + " ");
                        }
                    }
                    break;
                case 2: // X * 10
                    data = (ushort)(data * 10);
                    radioParsed.Add(thisEntry.Name + ": " + data);
                    break;
                case 3: // Error 1 2 3
                    useBits = checkErrorNum(radioBits, 1, 2, 3, 1, 2, 3, 1, 2);
                    if (useBits)
                    {
                        radioParsed.Add(thisEntry.Name + ": " + data);
                    }
                    break;
                case 4: // Error 4 5 6 7 8 9 10 11
                    useBits = checkErrorNum(radioBits, 4, 5, 6, 7, 8, 9, 10, 11);
                    if (useBits)
                    {
                        radioParsed.Add(thisEntry.Name + ": " + data);
                    }
                    break;
                case 5: // Error  4 5 6 8 10
                    useBits = checkErrorNum(radioBits, 4, 5, 6, 8, 10, 4, 5, 6);
                    if (useBits)
                    {
                        radioParsed.Add(thisEntry.Name + ": " + data);
                    }
                    break;
                case 6: // Error 5 11
                    useBits = checkErrorNum(radioBits, 5, 11, 5, 11, 5, 11, 5, 11);
                    if (useBits)
                    {
                        radioParsed.Add(thisEntry.Name + ": " + data);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Used for Radio Messages of Type 34 and 35, to check if the current parameter 
        /// being read is to be used for output. There is one parameter that will output for 8 seperate error codes, 
        /// therefore, checkErrorNum accepts 8 seperate error numbers.
        /// </summary>
        /// <param name="radioMsg">Radio Message Bits</param>
        /// <param name="err1">Error Code 1 to accept</param>
        /// <param name="err2">Error Code 2 to accept</param>
        /// <param name="err3">Error Code 3 to accept</param>
        /// <param name="err4">Error Code 4 to accept</param>
        /// <param name="err5">Error Code 5 to accept</param>
        /// <param name="err6">Error Code 6 to accept</param>
        /// <param name="err7">Error Code 7 to accept</param>
        /// <param name="err8">Error Code 8 to accept</param>
        /// <returns>True if parameter is to be used, otherwise false</returns>
        private bool checkErrorNum(BitArray radioMsg, ushort err1, ushort err2, ushort err3, ushort err4, ushort err5, ushort err6, ushort err7, ushort err8)
        {
            bool enter = false;
            try
            {
                // The Error bits are always 144 - 151 indexes
                BitArray errBits = new BitArray(8);
                errBits[0] = radioMsg[144];
                errBits[1] = radioMsg[145];
                errBits[2] = radioMsg[146];
                errBits[3] = radioMsg[147];
                errBits[4] = radioMsg[148];
                errBits[5] = radioMsg[149];
                errBits[6] = radioMsg[150];
                errBits[7] = radioMsg[151];

                // Find the error number
                ushort errNum = turnBitsToShort(errBits, 8);

                // Determine if the error number matches the number needed to use the current parameter
                if (errNum == err1 || errNum == err2 || errNum == err3 || errNum == err4 || errNum == err5 || errNum == err6 || errNum == err7 || errNum == err8)
                {
                    enter = true;
                }

                return enter;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::checkErrorNum-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return enter;
            }
        }

        /// <summary>
        /// Determines which Note is currently being read in the RadioEntry List created from the XML file.
        /// </summary>
        /// <param name="note">Note String obtained from the XML file</param>
        /// <returns>Integer representing the next steps</returns>
        private int determineNote(string note)
        {
            try
            {
                int def = 0;

                if (note == "ACTIVE BITS")
                {
                    def = 1;
                }
                else if (note == "X*10")
                {
                    def = 2;
                }
                else if (note == "Error Code 1 2 3")
                {
                    def = 3;
                }
                else if (note == "Error Code 4 5 6 7 8 9 10 11")
                {
                    def = 4;
                }
                else if (note == "Error Code 4 5 6 8 10")
                {
                    def = 5;
                }
                else if (note == "Error Code 5 11")
                {
                    def = 6;
                }
                else
                {
                    def = 0;
                }
                return def;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Messages::determineNote-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

    }
}
