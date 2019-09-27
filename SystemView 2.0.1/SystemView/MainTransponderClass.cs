using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SystemView
{
    // To call MainTransponderClass nameTPClass = new MainTransponderClass(string tpTag);
    // Send the hex string to the constructor for parsing...

    //
    // CLASS: MainTransponderClass
    //
    // Description: This class creates the Transponder Messages to display to the user through the Tags received from the OBC.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //  const int _infoLength = 180         - Length of the TP message
    //  const int _baseLength = 44          - Base Package length that is included at the beginning of every message
    //  const int _sizeNcenter = 3          - Length of bits to calculate package size
    //  const int _typeLength = 4           - Length of bits to calculate package type
    //  const int _pkgHeader = 8            - Length of bits for the header
    //  BitArray _TPmessage                 - The full TP message received
    //  int pkgLength                       - The current package (being parsed) length
    //  int _nibbles = 16                   - 4 Nibbles are included at the beginning of each package and need taken out
    //  int _length                         - Keeps track of the Length left over
    //  string _packageName                 - Package Name (number)
    //  ushort _size                        - Package Size
    //  ushort _type                        - Package Type
    //
    // Public Get/Set Accessors:
    //  int Length (Get & Set)
    //  string PackageName (Get & Set)
    //  ushort Size (Get & Set)
    //  ushort Type (Get & Set)
    //
    // Public Methods:
    //  PackageType:
    //      static string FindPackage(int type, int size)                           - Determines the Package based on the Type and Size
    //
    // Private Methods:
    //  BitArray hexStringToBits(string hexString)                                  - Converts a hex string to a Bit Array
    //  BitArray adjustBitContents(BitArray tpMsg, int len, int prevLength)         - Adjusts the TP BitArray to skip over the already read package
    //  BitArray readPkgBits(BitArray tpMsg, int pkgLength)                         - Copies the package bits into its own BitArray
    //  void readHeader(BitArray tpMsg)                                             - Reads the 8 header bits and configures the Type and Size of the package
    //  ushort turnBitsToShort(BitArray bittys, int numOfBits)                      - Converts the bits to a unsigned 16 bit integer
    //  List<TPEntry> readNodes(string package)                                     - Reads the XML child nodes of the Package and calls createEntryList
    //  IEnumerable<TPEntry> createEntryList(string package)                        - Reads the child nodes of TPEntry in the XML file and creates a List of the parameters
    //  void parseTPmessage(List<TPEntry> tpList, BitArray tpbits)                  - Parses the package bits with the TPEntry List into a string[] to display to user
    //  bool checkFunctionNum(BitArray tpbits, int func)                            - Checks to see if the parameter should be used (for Package Type 12)
    //  string findEquation(string equation, ushort x, ushort y, BitArray thisTP)   - Find the equation to use, solves the equation, and places the result in a string
    //  int determineNote(string note)                                              - Determines the note from XML and returns an integer corresponding to the note
    //
    // Constructors:
    //  MainTransponderClass(string tpTag)        - Constructor
    //  TPEntry:
    //      TPEntry()                             - Default Constructor that sets all the TPEntry Parameters to null or 0
    //
    // Public Overrides:
    //

    public class MainTransponderClass
    {
        private const int _infoLength = 180;
        private const int _baseLength = 44;
        private const int _sizeNcenter = 3;
        private const int _typeLength = 4;
        private const int _pkgHeader = 8;
        private BitArray _TPmessage;
        private int pkgLength;
        private int _nibbles = 16;

        private int _length;
        private string _packageName;

        private ushort _size;
        private ushort _type;

        public List<string> pkgMsg;

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

        public string PackageName
        {
            get
            {
                return _packageName;
            }
            set
            {
                _packageName = value;
            }
        }

        public ushort Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        public ushort Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructor for the MainTransponderClass. Reads the byte[], and subtracts the 4 beginning nibbles.
        /// Then checks to make sure the message has contents, reads the base package first and loops through the 
        /// message to read all of the packages within the message.
        /// </summary>
        /// <param name="tpTag">String of Hex bytes without spaces</param>
        public MainTransponderClass(string tpTag)
        {
            try
            {
                pkgMsg = new List<string>();

                // Length starts at the Length of the information bits (180)
                Length = _infoLength;
                _TPmessage = new BitArray(_infoLength);

                BitArray subtract4 = hexStringToBits(tpTag);
                // Drop the first 4 nibbles of the message
                for (int i = 0; i < _infoLength; i++)
                {
                    _TPmessage[i] = subtract4[i + _nibbles];
                }

                // Check to make sure the Bits contain a message
                var allZeros = _TPmessage.Cast<bool>().Contains(true);
                if (!allZeros)
                {
                    Console.WriteLine("No Transponder Message Found.");
                    return;
                }

                // Read the Base Package Bits
                BitArray baseBits = new BitArray(readPkgBits(_TPmessage, _baseLength));
                // Create the TPEntry List from the XML Sheet for the Base Package and parse the message
                List<TPEntry> tpList = new List<TPEntry>(readNodes("BaseInformationPackage"));
                parseTPmessage(tpList, baseBits);
                // Adjust the bits for the next package is in index 0
                _TPmessage = adjustBitContents(_TPmessage, Length, _baseLength);

                // Read the TP message Bits until the End Of Message
                while (Length > 0)
                {
                    // Read Type and Size of Package
                    readHeader(_TPmessage);
                    // Find the Package Number based on the Type and Size
                    string package = PackageType.FindPackage(Type, Size);
                    // Create the TPEntry List from the XML Sheet for the package and parse the message
                    tpList = new List<TPEntry>(readNodes(package));

                    // The only package this can happen for is the End/Error Package for there is no defined length,
                    // but the first 16 bits of the package contain information so the XML file states 16, but that 
                    // doesn't mean all 16 bits are included.
                    if (pkgLength > Length)
                    {
                        pkgLength = Length;
                    }
                    // Read package bits into a new Bit Array and parse the message
                    BitArray pkgBits = new BitArray(readPkgBits(_TPmessage, pkgLength));
                    parseTPmessage(tpList, pkgBits);

                    // Only Adjust the Bit Array if there are more bits to parse
                    if (Length > 0)
                    {
                        _TPmessage = adjustBitContents(_TPmessage, Length, pkgLength);
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::MainTransponderClass-threw exception {0}", ex.ToString()));

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
                sb.Append(String.Format("SystemView.Transponder::hexStringToBits-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Adjusts the bits of the Transponder bit message by deleting the bits from the package that has been read,
        /// and placing the next package at the first index of the array.
        /// </summary>
        /// <param name="tpMsg">TP bit message</param>
        /// <param name="len">Length after the package bits are removed</param>
        /// <param name="prevLength">Length of bits from the previously read package</param>
        /// <returns>The newly adjusting Bit Array</returns>
        private BitArray adjustBitContents(BitArray tpMsg, int len, int prevLength)
        {
            BitArray newMsg = new BitArray(len);
            try
            {
                // Copy the bits that have not been read to a new BitArray starting at index = 0
                for (int i = 0; i < len; i++)
                {
                    newMsg[i] = tpMsg[i + prevLength];
                }
                return newMsg;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::adjustBitContents-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return newMsg;
            }
        }

        /// <summary>
        /// Creates a new array containing only the current package bits.
        /// </summary>
        /// <param name="tpMsg">TP bit message</param>
        /// <param name="pkgLength">Length of current package being read</param>
        /// <returns>The package bits in a Bit Array</returns>
        private BitArray readPkgBits(BitArray tpMsg, int pkgLength)
        {
            BitArray pkgBits = new BitArray(pkgLength);
            try
            {
                // Read only the Package bits to the new BitArray
                for (int i = 0; i < pkgLength; i++)
                {
                    pkgBits[i] = tpMsg[i];
                }
                return pkgBits;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::readPkgBits-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return pkgBits;
            }
        }

        /// <summary>
        /// Reads the header bits of the current package to detemine which package needs parsed.
        /// The header bits contain the type of package, direction of travel, and size of the package.
        /// This method determines the type and size to find the package.
        /// </summary>
        /// <param name="tpMsg">TP bit message</param>
        private void readHeader(BitArray tpMsg)
        {
            try
            {
                // Read the Type and Size of the Package
                BitArray typeBits = new BitArray(_typeLength);
                BitArray sizeBits = new BitArray(_sizeNcenter);
                // Reads the Type bits
                for (int i = 0; i < _typeLength; i++)
                {
                    typeBits[i] = tpMsg[i];
                }

                int t = 0;
                // i = 5..7
                // Reads the Size bits
                for (int i = (_pkgHeader - _sizeNcenter); i < _pkgHeader; i++)
                {
                    sizeBits[t] = tpMsg[i];
                    t++;
                }
                Type = turnBitsToShort(typeBits, _typeLength);
                Size = turnBitsToShort(sizeBits, _sizeNcenter);

                if (Size >= 7)
                {   // Add Extension bits if the size is >= to 7, Should never be more than 8 with the Extension added
                    BitArray extBits = new BitArray(_typeLength);
                    t = 0;
                    for (int i = _pkgHeader; i < _pkgHeader + _typeLength; i++)
                    {
                        extBits[t] = tpMsg[i];
                    }
                    // Add Extension bits
                    Size += turnBitsToShort(extBits, _typeLength);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::readHeader-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
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
                sb.Append(String.Format("SystemView.Transponder::turnBitsToShort-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return theseBits;
            }
        }

        /// <summary>
        /// Reads the child nodes of the package in the Transponder XML file.
        /// Then calls createEntryList to create the TPEntry list to assist with parsing
        /// the package bits.
        /// </summary>
        /// <param name="package">Package Number</param>
        /// <returns>TPEntry List to assist with parsing the package bits</returns>
        private List<TPEntry> readNodes(string package)
        {
            try
            {
                // Package Error then do not read XML
                if (package == "Error")
                {
                    Length = 0;
                    return null;
                }
                // Load XML Sheet using LINQ
                XDocument xmlDoc = XDocument.Load(@"TransponderDictionary.xml");
                // Obtain the Length and Description of the package
                var packageNode = xmlDoc.Descendants("TPPackage").Descendants(package).Select(x => new
                {
                    pl = Int32.Parse(x.Element("LengthOfPackage").Value),
                    pn = x.Element("Description").Value
                }).ToList();

                pkgLength = packageNode[0].pl;
                PackageName = packageNode[0].pn;

                // Create a TPEntry List to be used for parsing
                List<TPEntry> newList = new List<TPEntry>(createEntryList(package));
                return newList;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::readNodes-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Reads the child nodes of the TPEntry node, and creates a List of the parameters for use.
        /// </summary>
        /// <param name="package">Pakcage Number</param>
        /// <returns>List of TPEntry parameters to assist with parsing</returns>
        private IEnumerable<TPEntry> createEntryList(string package)
        {
            try
            {
                // Load XML file with LINQ
                XDocument xmlDoc = XDocument.Load(@"TransponderDictionary.xml");

                // Create a List of each TPEntry Node with the TPEntry Class, and check for null to handle exceptions
                var query = xmlDoc.Descendants("TPPackage").Descendants(package).Descendants("TPEntry").Select(x => new TPEntry
                {
                    ID = PackageName,
                    Name = x.Element("Name") != null ? x.Element("Name").Value : "",
                    ExtraInfo = x.Element("Note") != null ? x.Element("Note").Value : "",
                    DataType = x.Element("DataType") != null ? x.Element("DataType").Value : "",
                    StartIndex = x.Element("Start") != null ? Int32.Parse(x.Element("Start").Value) : 0,
                    EndIndex = x.Element("End") != null ? Int32.Parse(x.Element("End").Value) : 0,
                    DictList = x.Elements("Dict").ToDictionary(p => p.Element("Key").Value, p => p.Element("Value").Value),
                    Equation = x.Element("Equation") != null ? x.Element("Equation").Value : "",
                }).ToList();

                return query;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::createEntryList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Parses the TP bit message package using the TPEntry List from reading the XML nodes.
        /// </summary>
        /// <param name="tpList">TPEntry List (from XML)</param>
        /// <param name="tpbits">Current Package Bits</param>
        private void parseTPmessage(List<TPEntry> tpList, BitArray tpbits)
        {
            
            try
            {
                ushort yy = 0;      // Y for Chainage
                ushort xx = 0;      // X for Chainage
                int si = 0;         // Start Index of Current TPEntry
                int ei = 0;         // End Index of Current TPEntry
                pkgMsg.Add(tpList[0].ID + ": ");        // Package Description
                int index = 0;                          // Index of the TPEntry List created from the XML

                // Parse until the List is empty or the bits have all been read
                while (tpList.Count > index && tpbits.Length > ei + 1)
                {
                    if (tpList[index].Name == "EOM")
                    {   // This is in the End/Error Package
                        Length = 0;
                        break;
                    }
                    si = tpList[index].StartIndex;
                    ei = tpList[index].EndIndex;
                    // Read the parameter bits into a new array and convert to ushort
                    BitArray deesBits = new BitArray((ei - si) + 1);
                    for (int i = 0; i < (ei - si + 1); i++)
                    {
                        deesBits[i] = tpbits[i + si];
                    }
                    ushort data = turnBitsToShort(deesBits, (ei - si + 1));

                    // Change to nulls for ease in the below checks
                    if (tpList[index].ExtraInfo == "0")
                    {
                        tpList[index].ExtraInfo = null;
                    }
                    if (tpList[index].Equation == "0")
                    {
                        tpList[index].Equation = null;
                    }
                    if (tpList[index].DictList.ContainsKey("NA"))
                    {
                        tpList[index].DictList.Clear();
                    }

                    // If Keys exist in the Distionary List
                    if (tpList[index].DictList.Count > 0)
                    {
                        // Get the associated value from the key
                        tpList[index].DictList.TryGetValue(data.ToString(), out var tpData);

                        // Now check for Notes, Equation, and if the Key is associated with a Value
                        if (tpList[index].ExtraInfo == null && tpList[index].Equation == null && tpData != null)
                        {
                            // No Notes, Equation, and a Value exists; ADD TO STRING
                            pkgMsg.Add(tpList[index].Name + ": " + tpData);
                        }
                        else if (tpList[index].ExtraInfo != null && tpList[index].Equation == null)
                        {
                            // Check the Note, and parse accordingly
                            int info = determineNote(tpList[index].ExtraInfo);
                            bool useBits;
                            bool useBits2;
                            switch (info)
                            {
                                case 1: // Y For Chainage
                                    yy = data;
                                    break;
                                case 3: // Check Active bits
                                    pkgMsg.Add(tpList[index].Name + ": ");
                                    for (int i = 0; i < (ei - si + 1); i++)
                                    {
                                        if (deesBits[i])
                                        {
                                            tpList[index].DictList.TryGetValue(i.ToString(), out var active);
                                            pkgMsg.Add(active + " ");
                                        }
                                    }
                                    break;
                                case 4: // Function == 7 or Function == 11
                                    useBits = checkFunctionNum(tpbits, 7);
                                    useBits2 = checkFunctionNum(tpbits, 11);
                                    if (useBits || useBits2)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": " + tpData);
                                    }
                                    break;
                                case 5: // Function == 14
                                    useBits = checkFunctionNum(tpbits, 14);
                                    if (useBits)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": " + tpData);
                                    }
                                    break;
                                case 6: // Function == 1
                                    useBits = checkFunctionNum(tpbits, 1);
                                    if (useBits)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": " + tpData);
                                    }
                                    break;
                                case 7: // Function == 11
                                    useBits = checkFunctionNum(tpbits, 11);
                                    if (useBits)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": " + tpData);
                                    }
                                    break;
                                case 8: // Fucntion == 2
                                    useBits = checkFunctionNum(tpbits, 2);
                                    if (useBits)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": " + tpData);
                                    }
                                    break;
                                case 9: // Function == 11 & Check AB
                                    useBits = checkFunctionNum(tpbits, 11);
                                    if (useBits)
                                    {
                                        pkgMsg.Add(tpList[index].Name + ": ");
                                        for (int i = 0; i < (ei - si + 1); i++)
                                        {
                                            if (deesBits[i])
                                            {
                                                tpList[index].DictList.TryGetValue(i.ToString(), out var active);
                                                pkgMsg.Add(active + " ");
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (tpList[index].ExtraInfo == null && tpList[index].Equation != null)
                        {
                            // Obtain string for the Equation
                            pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                        }
                        else
                        {
                            // NOTE AND EQUATION AND DICTIONARY; Only X in Chainage! Obtain string
                            int info = determineNote(tpList[index].ExtraInfo);
                            switch (info)
                            {
                                case 2: // X for chainage
                                    xx = data;
                                    pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, xx, yy, tpbits));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    // The Dictionary is EMPTY
                    else
                    {
                        // No Dictionary, Notes, or Equation; ADD ushort TO string[]
                        if (tpList[index].ExtraInfo == null && tpList[index].Equation == null)
                        {
                            pkgMsg.Add(tpList[index].Name + ": " + data.ToString());
                        }
                        else
                        {
                            // Determine note and parse accordingly; Each Case checks for Equations for ease
                            if (tpList[index].ExtraInfo != null)
                            {
                                int info = determineNote(tpList[index].ExtraInfo);
                                bool useBits;
                                bool useBits2;
                                switch (info)
                                {
                                    case 4: // Function == 7 or Function == 11
                                        useBits = checkFunctionNum(tpbits, 7);
                                        useBits2 = checkFunctionNum(tpbits, 11);
                                        if (useBits || useBits2)
                                        {
                                            if (tpList[index].Equation != null)
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                                            }
                                            else
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + data);
                                            }
                                        }
                                        break;
                                    case 5: // Function == 14
                                        useBits = checkFunctionNum(tpbits, 14);
                                        if (useBits)
                                        {
                                            if (tpList[index].Equation != null)
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                                            }
                                            else
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + data);
                                            }
                                        }
                                        break;
                                    case 6: // Function == 1
                                        useBits = checkFunctionNum(tpbits, 1);
                                        if (useBits)
                                        {
                                            if (tpList[index].Equation != null)
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                                            }
                                            else
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + data);
                                            }
                                        }
                                        break;
                                    case 7: // Function == 11
                                        useBits = checkFunctionNum(tpbits, 11);
                                        if (useBits)
                                        {
                                            if (tpList[index].Equation != null)
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                                            }
                                            else
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + data);
                                            }
                                        }
                                        break;
                                    case 8: // Fucntion == 2
                                        useBits = checkFunctionNum(tpbits, 2);
                                        if (useBits)
                                        {
                                            if (tpList[index].Equation != null)
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                                            }
                                            else
                                            {
                                                pkgMsg.Add(tpList[index].Name + ": " + data);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                // No Note or Dictionary, but an Equation Exists; Obtain string and add to array
                                pkgMsg.Add(tpList[index].Name + ": " + findEquation(tpList[index].Equation, data, 0, tpbits));
                            }
                        }
                    }
                    // Update the TPEntry List Index
                    index++;
                }// End while
                // Write the string[] for the package to the user
                pkgMsg.Add(" ");
                //pkgMsg.ForEach(i => Console.WriteLine("{0} ", i));
                // Update the Length
                Length = Length - pkgLength;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::parseTPmessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Used mostly for Packages of Type 12, to check if the current parameter 
        /// being read is to be used. 
        /// </summary>
        /// <param name="tpbits">Current Package Bits</param>
        /// <param name="func">The function being checked against</param>
        /// <returns>True if parameter should be used, else false</returns>
        private bool checkFunctionNum(BitArray tpbits, int func)
        {
            bool enter = false;
            try
            {
                // The Function bits are always 8-11 indexes
                BitArray funcBits = new BitArray(4);
                funcBits[0] = tpbits[8];
                funcBits[1] = tpbits[9];
                funcBits[2] = tpbits[10];
                funcBits[3] = tpbits[11];

                // Find the functon number
                ushort funNum = turnBitsToShort(funcBits, 4);

                // Determine if the function number matches the number needed to use the current parameter
                if (funNum == func)
                {
                    enter = true;
                }

                return enter;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::checkFunctionNum-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return enter;
            }
        }

        /// <summary>
        /// Determines which equation should be used, solves the equation, and places the result in a string.
        /// </summary>
        /// <param name="equation">Equation String from XML</param>
        /// <param name="x">Parameter x</param>
        /// <param name="y">Parameter y</param>
        /// <param name="thisTP">Current Package Bits</param>
        /// <returns>String with the result</returns>
        private string findEquation(string equation, ushort x, ushort y, BitArray thisTP)
        {
            string result = null;
            try
            {
                int z = 0;
                // z = increment, x = data, y = base
                //LINKING DISTANCE
                if (equation == "(Increment*(X-2)+Base)*3")
                {
                    if (x == 0 || x == 1 || x == 255)
                    {
                        result = "Error";
                        return result;
                    }
                    else if (x == 254)
                    {
                        result = "No Link";
                        return result;
                    }
                    else
                    {
                        BitArray LD = new BitArray(2);
                        LD[0] = thisTP[7];
                        LD[1] = thisTP[8];
                        ushort kv = turnBitsToShort(LD, 2);
                        if (kv == 0)
                        {
                            y = 10;
                            z = 5;
                        }
                        else if (kv == 1)
                        {
                            y = 1270;
                            z = 10;
                        }
                        else if (kv == 2)
                        {
                            y = 3790;
                            z = 15;
                        }
                        else if (kv == 3)
                        {
                            y = 7570;
                            z = 20;
                        }
                        else
                        {
                            result = "Error";
                            return result;
                        }
                    }
                    int link = (((z * (x - 2)) + y) * 3);
                    result = link + " feet";
                }
                // CHAINAGE/MILEPOST
                else if (equation == "(((Y-2)*250+(X-2))*10)*3")
                {
                    int chain = (((y - 2) * 250 + (x - 2)) * 10 * 3);
                    result = chain + " feet (" + String.Format("{0:N2}", ((double)chain) / 5280) + " mp)";
                }
                // DISTANCE
                else if (equation == "Value Range: [1-(X-2)] Distance: X*10")
                {
                    int dist = x * 10;
                    if (dist == 10220)
                    {
                        result = "Distance: 0 yards, 0 feet";
                    }
                    else
                    {
                        result = "Distance: " + dist + " yards, " + dist * 3 + " feet";
                    }
                }
                else if (equation == "Speed * 5")
                {
                    int speed = 5 * x;
                    result = speed + " mph";
                }
                else
                {
                    result = "Error";
                }
                return result;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Transponder::findEquation-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return result;
            }
        }

        /// <summary>
        /// Determines which Note is currently being read in the TPEntry List created from the XML file.
        /// </summary>
        /// <param name="note">Note String obtained from the XML file</param>
        /// <returns>Integer representing the next steps</returns>
        private int determineNote(string note)
        {
            try
            {
                int def = 0;

                if (note == "Y")
                {
                    def = 1;
                }
                else if (note == "X")
                {
                    def = 2;
                }
                else if (note == "ActiveBits")
                {
                    def = 3;
                }
                else if (note == "if(Function == 7 | Function == 11)")
                {
                    def = 4;
                }
                else if (note == "if (Function == 14)")
                {
                    def = 5;
                }
                else if (note == "if (Function == 1)")
                {
                    def = 6;
                }
                else if (note == "if (Function == 11)")
                {
                    def = 7;
                }
                else if (note == "if (Function == 2)")
                {
                    def = 8;
                }
                else if (note == "if (Function == 11) - ActiveBits")
                {
                    def = 9;
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
                sb.Append(String.Format("SystemView.Transponder::determineNote-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }
    }
}
