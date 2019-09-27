using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Transport;

namespace AppLogic
{
    // To call Revision nameRevision = new Revision();


    //
    // CLASS: Revision/ RevisionElement/ GenericAppRevision/ BasicAppRevision/ ExtendedAppRevision
    //
    // Description: This class obtains the revisions from the OBC and parses them based on their type.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //  GenericAppRevision:
    //      protected _rev          - Software Assembly Revision Number, 2 bytes
    //      protected _date         - Software Release Date, 11 bytes
    //      protected _build        - Software Build Version, 10 bytes
    //      protected _partNumber   - Software Part Number, 11 bytes
    //  RevisionElement:
    //      _sName                  - Name of component for each revision
    //      _nPosition              - Offset
    //      _eType                  - Type of revision, Basic or Extended
    //      _id                     - ID (Position order in Revision Message from PTE)
    //      _baseRev                - Revision of type Basic
    //      _extRev                 - Revision of type Extended
    //  Revision:
    //      List<RevisionElement>   - Full list of each revision
    //      REVISION_SIZE           - The number of total revisions (how many different names, not including the parameters of each name)
    //
    // Public Get/Set Accessors:
    //  GenericAppRevision:
    //      abstract int Length (Get only)
    //      String Rev (Get only)
    //      String Build (Get only)
    //      String Date (Get only)
    //      String PartNumber (Get only)
    //  RevisionElement:
    //      String Name (Get only)
    //      REVISION_TYPE Type (Get only)
    //      int Position (Get only)
    //      int Length (Get only)
    //      int ID (Get & Set)
    //
    // Public Methods:
    //  GenericAppRevision:
    //      abstract void Parse(PTEMessage msgBase, int nOffset)    - Parses revision message
    //
    // Private Methods:
    //  Revision:
    //      void fetch()                                - Obtains PTE Message that contains the revisions and sends it to revisionParse
    //      void revisionParse(PTEMessage msgBase)      - Sends message to RevisionElement for parsing and than adds all of the parameters to the list
    //
    // Constructors:
    //      GenericAppRevision()    - Default constructor
    //      BasicAppRevision(PTEMessage msgBase, int nOffset)                                       - Constructor that calls the message to be parsed
    //      ExtendedAppRevision(PTEMessage msgBase, int nOffset)                                    - Constructor that calls the message to be parsed
    //      RevisionElement()   - Default Constructor
    //      RevisionElement(String sName, REVISION_TYPE eType, PTEMessage msgBase, int Position)    - Constructor that sends message to be parsed based on type
    //      Revision()  - Default constructor; calls fetch
    //
    // Public Overrides:
    //  BasicAppRevision & ExtendedAppRevision:
    //      int Length (Get only)
    //      void Parse(PTEMessage msgBase, int nOffset)                                             - The method that does the parsing
    //  RevisionElement:
    //      string ToString()
    //      

    /// <summary>
    /// The Revision types being used should be Basic or Extended.
    /// Extended includes the Build Number, while Basic does not.
    /// </summary>
    public enum REVISION_TYPE
    {
        BASIC,
        EXTENDED,
        CUSTOM,
        UNKNOWN
    }

    /// <summary>
    /// Paramaters of each Revision. Note: Basic Type does not include Build Number.
    /// </summary>
    public abstract class GenericAppRevision
    {
        protected String _rev;
        protected String _date;
        protected String _build;
        protected String _partNumber;

        /// <summary>
        /// Default Constructor initiates members.
        /// </summary>
        public GenericAppRevision()
        {
            try
            {
                _partNumber = "<Unknown>";
                _rev = "<Unknown>";
                _build = "<Unknown>";
                _date = "<Unknown>";
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::GenericAppRevision-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        #region Abstract Methods
        // Parses the Revisions to GenericAppRivision variables
        public abstract void Parse(PTEMessage msgBase, int nOffset);
        // Length of Revision
        public abstract int Length
        {
            get;
        }
        #endregion

        #region Accessor Methods       
        public String Rev
        {
            get
            {
                return _rev;
            }
        }
        public String Build
        {
            get
            {
                return _build;
            }
        }
        public String Date
        {
            get
            {
                return _date;
            }
        }
        public String PartNumber
        {
            get
            {
                return _partNumber;
            }
        }
        #endregion
    }

    /// <summary>
    /// Basic Revision Type Class; Handles all Basic Types.
    /// Base Class is GenericAppRevision.
    /// </summary>
    public class BasicAppRevision : GenericAppRevision
    {
        public const int PART_LENGTH = 11;
        public const int REV_LENGTH = 2;
        public const int DATE_LENGTH = 11;

        public BasicAppRevision(PTEMessage msgBase, int nOffset)
        {
            try
            {
                Parse(msgBase, nOffset);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::BasicAppRevision-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        public BasicAppRevision() : base()
        {

        }
        // Length of each Basic Revision (24 bytes)
        public override int Length
        {
            get
            {
                return PART_LENGTH + REV_LENGTH + DATE_LENGTH;
            }
        }
        /// <summary>
        /// Parses Basic Type Revisions to Part Number, Revision, and Date.
        /// </summary>
        /// <param name="msgBase">OBC Message of Revisions</param>
        /// <param name="nOffset">Offset to start reading</param>
        public override void Parse(PTEMessage msgBase, int nOffset)
        {
            StringBuilder sb = new StringBuilder();

            _partNumber = "";
            _rev = "";
            _date = "";

            try
            {
                // Parse Software Part Number
                char[] ch = new char[PART_LENGTH];
                for (int i = 0; i < PART_LENGTH; i++)
                {
                    if (msgBase.Data[nOffset + i] != 0)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                }
                sb.Append(ch);
                _partNumber = sb.ToString();
                nOffset += PART_LENGTH;
            }
            catch
            {
                _partNumber = "<Parse Error>";
            }
            if (!_partNumber.Contains("NOT USED"))
            {
                try
                {
                    // Parse Software Assembly Revision Level
                    char[] ch = new char[REV_LENGTH];
                    for (int i = 0; i < REV_LENGTH; i++)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                    sb.Clear();
                    sb.Append(ch);
                    _rev = sb.ToString();
                    nOffset += REV_LENGTH;
                }
                catch
                {
                    _rev = "<Parse Error>";
                }
                try
                {
                    // Parse Software Release Date
                    char[] ch = new char[DATE_LENGTH];
                    for (int i = 0; i < DATE_LENGTH; i++)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                    sb.Clear();
                    sb.Append(ch);
                    _date = sb.ToString();
                    nOffset += DATE_LENGTH;
                }
                catch
                {
                    _date = "<Parse Error>";
                }
            }
        }
    }

    /// <summary>
    /// Extended Revision Type Class; Handles parsing of Extended Data.
    /// </summary>
    public class ExtendedAppRevision : GenericAppRevision
    {
        public const int PART_LENGTH = 11;
        public const int REV_LENGTH = 2;
        public const int DATE_LENGTH = 11;
        public const int BUILD_LENGTH = 10;

        public ExtendedAppRevision(PTEMessage msgBase, int nOffset)
        {
            try
            {
                Parse(msgBase, nOffset);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::ExtendedAppRevision-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        public ExtendedAppRevision() : base()
        {

        }
        /// <summary>
        /// Parses Extended Revision Types to Part Number, Revision, Build Number, and Date.
        /// </summary>
        /// <param name="msgBase">OBC message of Revisions</param>
        /// <param name="nOffset">Offset where to start reading</param>
        public override void Parse(PTEMessage msgBase, int nOffset)
        {
            StringBuilder sb = new StringBuilder();

            _partNumber = "";
            _rev = "";
            _build = "";
            _date = "";

            try
            {
                // Parse Software Part Number
                char[] ch = new char[PART_LENGTH];
                for (int i = 0; i < PART_LENGTH; i++)
                {
                    if (msgBase.Data[nOffset + i] != 0)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                }
                sb.Append(ch);
                _partNumber = sb.ToString();
                nOffset += PART_LENGTH;
            }
            catch
            {
                _partNumber = "<Parse Error>";
            }
            if (!_partNumber.Contains("NOT USED"))
            {
                try
                {
                    // Parse Software Assembly Revision Label
                    char[] ch = new char[REV_LENGTH];
                    for (int i = 0; i < REV_LENGTH; i++)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                    sb.Clear();
                    sb.Append(ch);
                    _rev = sb.ToString();
                    nOffset += REV_LENGTH;
                }
                catch
                {
                    _rev = "<Parse Error>";
                }
                try
                {
                    // Parse Software Release Date
                    char[] ch = new char[DATE_LENGTH];
                    for (int i = 0; i < DATE_LENGTH; i++)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                    sb.Clear();
                    sb.Append(ch);
                    _date = sb.ToString();
                    nOffset += DATE_LENGTH;
                }
                catch
                {
                    _date = "<Parse Error>";
                }
                try
                {
                    // Parse Software Build Version
                    char[] ch = new char[BUILD_LENGTH];
                    for (int i = 0; i < BUILD_LENGTH; i++)
                    {
                        ch[i] = Convert.ToChar(msgBase.Data[nOffset + i]);
                    }
                    sb.Clear();
                    sb.Append(ch);
                    _build = sb.ToString();
                    nOffset += BUILD_LENGTH;
                }
                catch
                {
                    _build = "<Parse Error>";
                }
            }
        }
        // Length of each Extended Revision (34 bytes)
        public override int Length
        {
            get
            {
                return PART_LENGTH + REV_LENGTH + DATE_LENGTH + BUILD_LENGTH;
            }
        }
    }

    /// <summary>
    /// Sends OBC message to be parsed based on their types (Basic vs Extended)
    /// </summary>
    public class RevisionElement
    {
        private String _sName;
        private int _nPosition;
        private REVISION_TYPE _eType;
        private int _id;

        private BasicAppRevision _basicRev;
        private ExtendedAppRevision _extRev;

        /// <summary>
        /// Defualt Constructor
        /// </summary>
        public RevisionElement()
        {
            try
            {
                _sName = "Unnamed";
                _nPosition = 0;
                _eType = REVISION_TYPE.UNKNOWN;
                _basicRev = new BasicAppRevision();
                _extRev = new ExtendedAppRevision();
                _id = 0;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::RevisionElement-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Constructor using a PTE Message to create elements by 
        /// calling the Basic or Extended App Revision classes to parse message.
        /// </summary>
        /// <param name="sName">Name of revision</param>
        /// <param name="eType">Extended or Basic</param>
        /// <param name="msgBase">PTE message</param>
        /// <param name="Position">Offset</param>
        public RevisionElement(String sName, REVISION_TYPE eType, PTEMessage msgBase, int Position)
        {
            try
            {
                _sName = sName;
                _nPosition = Position;
                _eType = eType;
                _id = 0;

                if (_eType == REVISION_TYPE.BASIC)
                {
                    _basicRev = new BasicAppRevision(msgBase, Position);
                }
                else if (_eType == REVISION_TYPE.EXTENDED)
                {
                    _extRev = new ExtendedAppRevision(msgBase, Position);
                }
                else
                {
                    // Do nothing
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::RevisionElement-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
        #region Accessor Functions
        public String Name
        {
            get
            {
                return _sName;
            }
        }
        public REVISION_TYPE Type
        {
            get
            {
                return _eType;
            }
        }
        public int Position
        {
            get
            {
                return _nPosition;
            }
        }
        public int Length
        {
            get
            {
                switch (_eType)
                {
                    case REVISION_TYPE.BASIC:
                        return _basicRev.Length;

                    case REVISION_TYPE.EXTENDED:
                        return _extRev.Length;

                    default:
                        return 0;
                }
            }
        }
        public int ID
        {
            // ID (Position Order in Revision Message from PTE)
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Revision Data object. (Extended or Basic)
        /// </summary>
        public Object RevisionData
        {
            get
            {
                if (_eType == REVISION_TYPE.EXTENDED)
                {
                    return _extRev;
                }
                else if (_eType == REVISION_TYPE.BASIC)
                {
                    return _basicRev;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        /// <summary>
        /// Description of this Revision 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                //String s;

                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Reivision::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }

    public class Revision
    {
        private List<RevisionElement> Revisions;
        static int REVISION_SIZE;

        /// <summary>
        /// Default Constructor assumes connection and calls fetch.
        /// </summary>
        public Revision()
        {
            try
            {
                fetch();
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::Revision-trew exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Calls the PTE Message and sends it to revisionParse.
        /// </summary>
        private void fetch()
        {
            try
            {
                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendRevisionRequest());

                if (msgBase.CommandID == 0x01) // TODO - CHANGE HARD CODE TO DEFINED VALUE
                {
                    revisionParse(msgBase);
                }
            }
            catch (TransportException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::fetch-trew exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// This is Hard Coded for MTA without reading the XML file... FOR NOW.
        /// Takes the OBC message, and sends to RevisionElement to be parsed, and adds to the Revision List.
        /// </summary>
        /// <param name="msgBase">The PTE Message of the Revision data to be parsed above.</param>
        private void revisionParse(PTEMessage msgBase)
        {
            try
            {
                // TODO - XML FILE!!!!!!
                int i = 0;
                Revisions = new List<RevisionElement>();

                //XmlDocument doc = new XmlDocument();  // FOR XML DOCUMENT
                //doc.Load("*.xml");                    // FOR XML DOCUMENT
                //XmlNodeList elemList = doc.GetElementsByTagName("Revision");  // FOR XML DOCUMENT
                //REVISION_SIZE = elemList.Count;       //FOR XML DOCUMENT
                //int tagIndex = 0;      //Keeps track of Revision name list   // FOR XML DOCUMENT

                REVISION_SIZE = 64;         // Hard Coded for MTA

                // if extended
                Console.WriteLine("Revisions: {0}", REVISION_SIZE);
                // Data Length is 546, there are 16 revisions each with 34 bytes; 16*34 = 544
                while (i < msgBase.Length - 2) //&& tagIndex < elemList.Count
                {
                    int _num = Convert.ToInt32(msgBase.ProductID);    // Product ID
                    RevisionElement rev = new RevisionElement("Testing 1 2 3", REVISION_TYPE.EXTENDED, msgBase, i);  // elemList[tagIndex].ToString()   // Send message to be parsed
                    rev.ID = _num;
                    // Add revision to list and update index/offset
                    Revisions.Add(rev);
                    i += rev.Length;
                    //tagIndex++;
                }
                /* //if basic
                 Console.WriteLine("Revisions: {0}", (msgBase.Length / REVISION_SIZE) - 1);
                 while (i < msgBase.Length - 4)
                 {
                     int _num = 0; //TODO GET FROM XML
                     RevisionElement rev = new RevisionElement(elemList.ToString(), REVISION_TYPE.BASIC, msgBase, i);
                     rev.ID = _num;
                     Revisions.Add(rev);
                     i += rev.Length;
                 }
                 */
                //else nothing
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Revision::revisionParse-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the revision containing list associated with the Revision class. 
        /// </summary>

        public List<RevisionElement> GetRevisions()
        {
            return Revisions;
        }
    }
}
