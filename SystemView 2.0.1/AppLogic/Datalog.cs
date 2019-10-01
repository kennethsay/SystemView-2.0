using System;
using ClosedXML.Excel;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Transport;
using System.IO;
using System.Net.Sockets;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace AppLogic
{
    // To call Datalog nameDatalog = new Datalog();
    // Then must call CreateExcelFile() and UserDownloadParameters()


    //
    // CLASS: Datalog
    //
    // Description: This class obtains the Data download for the user based on the users selections and converts the data into a DAT or CSV file (if the user has permissions).
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues. Computer must reply to Ping requests or OBC will end communication.
    //            Build Properties must be targetted on a 64 bit machine. This will not work for more than 100,000 records.
    // 
    // Private Data:
    //      DateTime _startDateTime                                 - User Defined Start Download from this Time in OBC
    //      DateTime _endDateTime                                   - User Defined End Download from this Time in OBC
    //      string _fileName                                        - User defined File Name with extension
    //      bool _canUploadCSV                                      - Extra User Permissions to obtain a .CSV of the Datalog
    //      _const TAG_VALUE_TYPES TAG_VALUE_TYPES = default        - Defines to user the default tag type of each Tag
    //      TagList Tags                                            - TagList to fill our List of TagLists
    //      List<TagList> fullDatalogTagList                        - List of TagLists
    //      TagList fullTagListReceived                             - Holds the Last full Data Tag List recieved
    //      int tagIndex                                            - Holds the number of TagLists read 
    //      int start                                               - Start Datetime as an int
    //      int current                                             - Current download DateTime as an int
    //      int end                                                 - End Datetime as an int
    //      TagList referenceList                                   - A taglist to reference throughout the class
    //      List<TagList> listOfTagList                             - The List of TagLists to be used to write rows of data
    //      const int eachTagBufferSize = 4096                      - Defines the maximum allowed buffer size
    //      const int maxMsgSize = 1024                             - Found in the ICD: MSBs 00-03 and LSBs 00-FF
    //      ConcurrentQueue<byte[]> buffQueue                       - Queue the reads the buffer messages and stores FIFO
    //      List<object> tagObjectList                              - Lists of Tags as an object list
    //      byte[] buffer                                           - Buffer read from Socket Manager
    //      byte[] buff                                             - Current Buffer message the class is reading
    //      int byteCount                                           - Amount of bytes read from Socket Manager
    //      int bytesLeft                                           - Number of bytes left within the buff message
    //      bool moreData                                           - False when an ACK is read from remote device, otherwise true
    //      bool goAgain                                            - True if the buff message has more than one PTE Message
    //      bool skipThis                                           - True if the next buffer had to be read to complete the current PTE Message from the current buff
    //      bool parseMSG                                           - True if more than one message is received from buff
    //      bool finalACK                                           - True is an ACK is received from the remote device
    //      int packetLoss                                          - Keeps track of all the packet loss
    //
    // Public Get/Set Accessors:
    //      bool CanUploadCSV (Gets & Sets)
    //      DateTime StartDT (Gets & Sets)
    //      DateTime EndDT (Gets & Sets)
    //      String SetFileName (Sets only)
    //
    // Public Methods:
    //      void UserDownloadParameters()           - Parameters chosen from User in how they want to download the data
    //      void CreateExcelFile()                  - Creates the Excel File with appropriate header columns, header, footer, and file name
    //
    // Private Methods:
    //      void uploadAllAvailable()                                           - Handles the Upload All Available Parameter
    //      void uploadRecentByTime()                                           - Handles the Time Selected Paramter
    //      void uploadRecentBySize()                                           - Handles the Upload by Size Parameter
    //      uploadByRange(DateTime startDT, DateTime endDT)                     - Handles the Upload by Time Range Parameter
    //      void fetch()                                                        - Initiates the Upload Process with the OBC and retrieves the first message
    //      void continueOBCComm()                                              - Continues Connection with OBC until Upload is complete, and retrieves messages
    //      void traverseTagMessage(PTEMessage msgBase)                         - Traverses the tag messages coming from the OBC
    //      TagList fullTagList(TagList thisTagList)                            - Holds the Full Tag Lists
    //      TagList turnPartialToFull(TagList partialTL, TagList fullTL)        - Turns the Partial Tag Lists into Full Tag Lists
    //      int percentDownloadComplete(TagList currentTagList)                 - Returns the percent download to the user
    //      int computeSecondsTicks(DateTime thisDateTime)                      - Helper function for percentDownloadComplete
    //      void writeToDatalogFile(List<TagList> allMyTags)                    - Primary function of the class
    //      byte[] parseBuffMsg(int msgLength)                                  - Parses the buffer message appropriately
    //      void getEachTagList(List<TagList> thisTagList)                      - Converts each TagList to a List<object> to write each row of data efficiently
    //      void checkPTEmsg(PTEMessage dataMsg)                                - Checks the PTE Message to ensure the message is complete and with the correct Command ID
    //      void datalogWorkerComplete()                                       - Converts the Excel file to .csv (if user is able) and .dat
    //      String getTagType(TAG_VALUE_TYPES tagType)                          - Gets the Tag Types for the DataTable to use
    //      static int bytesToInt(byte[] byteInt, int Length)                   - Converts the byte array to an Integer for the DataTable
    //      static DateTime bytesToDateTime(byte[] byteDateTime)                - Convert Byte Array of seconds from 1,1,2000 to the actual time it represents in DateTime format
    //      static string byteArryToString(byte[] byteToString, int Length)     - Converts byte array to String to represents the bytes
    //      static string byteArrayToStringHex(byte[] byteToHex)                - Turns Hex number into Strings
    //
    // Constructors:
    //      Datalog()           - Default Constructor
    //
    // Public Overrides:
    //
    public static class InitialDatalogResponse
    {
        private static byte[] _initialBuff;

        public static byte[] InitialBuff
        {
            get
            {
                return _initialBuff;
            }
            set
            {
                _initialBuff = value;
            }
        }
    }
    public class Datalog
    {
        #region Public enum
        public enum DATALOG_TIME_SELECTION
        {
            ALL_AVAILABLE,
            RECENT_BY_TIME,
            RECENT_BY_SIZE,
            SELECT_RANGE,
        };
        public enum DATA_BYTIME_SELECTED
        {
            HR_48,
            HR_24,
            HR_08,
            HR_04,
            HR_01,
            MIN_30,
            MIN_15,
        };
        public enum DATALOG_SOURCE
        {
            INTERNAL = 0,
            EXTERNAL = 1,
        };
        #endregion

        #region Member Variables
        public DATALOG_TIME_SELECTION _userSelection { get; set; }
        public DATA_BYTIME_SELECTED _userTimeSelected { get; set; }
        public DATALOG_SOURCE _userDatalogSource { get; set; }
        public int _percentBlockUpload { get; set; }

        // tagIndex Holds the number records in the excel sheet (index column)
        private int tagIndex;

        private int start;
        private int current;
        private int end;

        private string _fileName;
        private bool _canUploadCSV;

        private const TAG_VALUE_TYPES TAG_VALUE_TYPES = default;
        private TagList Tags = new TagList();
        private TagList referenceList = new TagList();
        private static TagList fullTagListReceived = new TagList();
        private List<TagList> listOfTagList = new List<TagList>();
        private const int eachTagBufferSize = 4096;
        private const int maxMsgSize = 1024;        // Found in the ICD: MSBs 00-03 and LSBs 00-FF

        private ConcurrentQueue<byte[]> buffQueue = new ConcurrentQueue<byte[]>();

        private List<string> tagObjectList = new List<string>();

        private byte[] buffer = new byte[eachTagBufferSize];
        private byte[] buff = new byte[eachTagBufferSize];
        // Holds the number of bytes read from buffer, and keeps track of number of bytes left in buff
        private int byteCount;
        private int bytesLeft;
        private bool moreData;
        private bool goAgain;
        private bool skipThis;
        private bool parseMSG;
        private bool finalACK;
        private int packetLoss;

        private DateTime _startDateTime;
        private DateTime _endDateTime;
        #endregion

        #region Public Accessors
        public bool CanUploadCSV
        {
            get
            {
                return _canUploadCSV;
            }
            set
            {
                _canUploadCSV = value;
            }
        }
        public string SetFileName
        {
            set
            {
                _fileName = value;
            }
        }
        public DateTime StartDT
        {
            get
            {
                return _startDateTime;
            }
            set
            {
                _startDateTime = value;
            }
        }
        public DateTime EndDT
        {
            get
            {
                return _endDateTime;
            }
            set
            {
                _endDateTime = value;
            }
        }
        #endregion

        /// <summary>
        /// Default Constructor initializes all of the variables used from start to finish of the 
        /// Datalog Download process.
        /// </summary>
        public Datalog()
        {
            try
            {
                tagIndex = 0;
                start = 0;
                current = 0;
                end = 0;
                finalACK = false;

                buffer = new byte[eachTagBufferSize];
                buff = new byte[eachTagBufferSize];
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::Datalog-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// This Method creates the excel sheet with the headers column. It also includes
        /// headers for the sheet: Train Company name, revision number, and configurations.
        /// The footer includes the Start Time and End Time of the download.
        /// To make sure these are accurate-- The start and end times must be initialized in
        /// the UI before the userDownloadParameters() method is called. Also, the configurations,
        /// revision, and company name must also be initialized in the UI.
        /// Made this method public so the UI can create the excel sheet before calling 
        /// UserDownloadParameters so that messages don't fall behind.
        /// </summary>
        public void CreateExcelFile(string savePath)
        {
            try
            {
                _fileName = savePath;

                Revision _myRevision = new Revision();
                List<string> RevisionString = new List<string>();

                Config _myConfig = new Config();
                List<string> ConfigString = new List<string>();

                foreach (RevisionElement e in _myRevision.GetRevisions())
                {
                    RevisionString.Add(e.RevisionData.ToString());
                }

                foreach (ConfigElement c in _myConfig.Elements)
                {
                    ConfigString.Add(c.ToString());
                }

                using (StreamWriter writer = new StreamWriter(savePath + ".SDF"))
                {
                    writer.WriteLine("Datalog File: " + savePath);
                    writer.WriteLine("Downloaded: " + DateTime.Now + " using SystemView");
                    writer.WriteLine("Configurations: ");
                    ConfigString.ForEach(i => writer.WriteLine("{0} ", i));
                    writer.WriteLine("Revisions: ");
                    RevisionString.ForEach(i => writer.WriteLine("{0} ", i));
                    writer.WriteLine("Data: ");
                    writer.Write("Index,");
                    for (int l = 0; l < 92; l++)
                    {
                        writer.Write((referenceList.Tags.Find(X => X.TagID == l).Name) + ",");
                    }
                    writer.WriteLine(" ");
                    writer.Close();
                }

                if(CanUploadCSV)
                {
                    using (StreamWriter writer = new StreamWriter(savePath + ".CSV"))
                    {
                        writer.WriteLine("Datalog File: " + savePath);
                        writer.WriteLine("Downloaded: " + DateTime.Now + " using SystemView");
                        writer.WriteLine("Configurations: ");
                        ConfigString.ForEach(i => writer.WriteLine("{0} ", i));
                        writer.WriteLine("Revisions: ");
                        RevisionString.ForEach(i => writer.WriteLine("{0} ", i));
                        writer.WriteLine("Data: ");
                        writer.Write("Index,");
                        for (int l = 0; l < 92; l++)
                        {
                            writer.Write((referenceList.Tags.Find(X => X.TagID == l).Name) + ",");
                        }
                        writer.WriteLine(" ");
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::createExcelFile-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Parameters (button) chosen by the user for how the user would like to download the datalog.
        /// </summary>
        public void UserDownloadParameters()
        {
            try
            {
                switch (_userSelection)
                {
                    case DATALOG_TIME_SELECTION.ALL_AVAILABLE:
                        uploadAllAvailable();
                        break;
                    case DATALOG_TIME_SELECTION.RECENT_BY_TIME:
                        uploadRecentByTime();
                        break;
                    case DATALOG_TIME_SELECTION.RECENT_BY_SIZE:
                        uploadRecentBySize();
                        break;
                    case DATALOG_TIME_SELECTION.SELECT_RANGE:
                        uploadByRange(StartDT, EndDT);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::userDownloadParameters-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// User Selection to upload all available data. Then calls writeToDatalogFile() so
        /// the first message can be written to the file().
        /// </summary>
        private void uploadAllAvailable()
        {
            try
            {
                // Get the Async Client being used from OBCCommunications 
                AsynchronousClient datalogAsyncReceive = PTEConnection.Comm.GetAsyncClient;
                InitialDatalogResponse.InitialBuff = (PTEConnection.Comm.RequestUploadAllAvailable((int)_userDatalogSource));

                // Update the amount of bytes read every time a buffer is receieved
                byteCount = datalogAsyncReceive.BytesRecieved;

                BackgroundWorker _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.DoWork += writeToDatalogFile;
                _worker.RunWorkerCompleted += datalogWorkerComplete;
                _worker.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::downloadAllAvailable-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// User selection to upload recent by Time, with each time button as it's
        /// own case in a switch statment. The start time is the current time, and
        /// the end time is configure based on the user selection. Then calls 
        /// fetch().
        /// </summary>
        private void uploadRecentByTime()
        {
            try
            {
                DateTime currentDt = DateTime.Now;
                EndDT = currentDt;

                switch (_userTimeSelected)
                {
                    case DATA_BYTIME_SELECTED.HR_48:
                        StartDT = currentDt.AddHours(-48);
                        break;
                    case DATA_BYTIME_SELECTED.HR_24:
                        StartDT = currentDt.AddHours(-24);
                        break;
                    case DATA_BYTIME_SELECTED.HR_08:
                        StartDT = currentDt.AddHours(-8);
                        break;
                    case DATA_BYTIME_SELECTED.HR_04:
                        StartDT = currentDt.AddHours(-4);
                        break;
                    case DATA_BYTIME_SELECTED.HR_01:
                        StartDT = currentDt.AddHours(-1);
                        break;
                    case DATA_BYTIME_SELECTED.MIN_30:
                        StartDT = currentDt.AddMinutes(-30);
                        break;
                    case DATA_BYTIME_SELECTED.MIN_15:
                        StartDT = currentDt.AddMinutes(-15);
                        break;
                    default:
                        break;
                }
                fetch();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::uploadRecentByTime-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// User selection to upload by a percent block of data. _percentBlockUpload needs to be set
        /// by the user before this can be called. This directly calls the RequestUploadByBlockPercent method 
        /// in OBCCommunications.cs and obtains the initial PTEMessage. Then calls writeToDatalogFile so
        /// the first message can be written to the file.
        /// </summary>
        private void uploadRecentBySize()
        {
            try
            {
                // Get the Async Client being used from OBCCommunications 
                AsynchronousClient datalogAsyncReceive = PTEConnection.Comm.GetAsyncClient;
                InitialDatalogResponse.InitialBuff = (PTEConnection.Comm.RequestUploadByBlockPercent(_percentBlockUpload, (int)_userDatalogSource));

                // Update the amount of bytes read every time a buffer is receieved
                byteCount = datalogAsyncReceive.BytesRecieved;

                BackgroundWorker _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.DoWork += writeToDatalogFile;
                _worker.RunWorkerCompleted += datalogWorkerComplete;
                _worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::uploadRecentBySize-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// User selection to upload by a defined time range. This method secures that the Start time
        /// comes before the end time, and if not, then it switches the two. If they are the same, then assigns
        /// the Start time to the user defined time, and End time to the current time. Then calls fetch().
        /// </summary>
        /// <param name="startDT">User Defined Start Time</param>
        /// <param name="endDT">User Defined End Time</param>
        private void uploadByRange(DateTime startDT, DateTime endDT)
        {
            try
            {
                int result = DateTime.Compare(startDT, endDT);
                // TimeSpan differenceofSTandET = endDT.Subtract(startDT) ------ If you want to check to make sure the user is not uploading years of data

                // startDT is earlier than endDT; which is what we want
                if (result < 0)
                {
                    StartDT = startDT;
                    EndDT = endDT;
                }
                // startDT is later than endDT; so just switch them so we can still send a request
                else if (result > 0)
                {
                    StartDT = endDT;
                    EndDT = startDT;
                }
                else
                {
                    // They are equal.. Do nothing? Throw Exception? Put the EndDT to current time? why not....
                    DateTime currentDT = DateTime.Now;
                    StartDT = startDT;
                    EndDT = currentDT;
                }
                fetch();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::uploadByRange-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Begins communication with the OBC to obtain the user defined datalog data.
        /// Once the PTEMessage is returned, the InitialMessage is updated. Then calls
        /// writeToDatalogFile(), to write the first message to the file.
        /// </summary>
        private void fetch()
        {
            try
            {
                // Get the Async Client being used from OBCCommunications 
                AsynchronousClient datalogAsyncReceive = PTEConnection.Comm.GetAsyncClient;
                InitialDatalogResponse.InitialBuff = (PTEConnection.Comm.SendFullDataUploadRequest(StartDT, EndDT, (int)_userDatalogSource));

                // Update the amount of bytes read every time a buffer is receieved
                byteCount = datalogAsyncReceive.BytesRecieved;

                BackgroundWorker _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.DoWork += writeToDatalogFile;
                _worker.RunWorkerCompleted += datalogWorkerComplete;
                _worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Handles and receives all incoming OBC data as PTEMessages and places in a Queue.
        /// </summary>
        private void continueOBCComm()
        {
            try
            {
                if (finalACK == false)
                {
                    AsynchronousClient datalogAsyncReceive = PTEConnection.Comm.GetAsyncClient;
                    bool recieveFinished = false;

                    datalogAsyncReceive.Receive(buffer);

                    Thread.Sleep(1);

                    if (datalogAsyncReceive.ReceiveComplete)
                    {
                        recieveFinished = true;
                    }

                    if (recieveFinished)
                    {
                        // Update bytes read
                        byteCount = datalogAsyncReceive.BytesRecieved;
                        byte[] buffMsg = new byte[byteCount];
                        Buffer.BlockCopy(buffer, 0, buffMsg, 0, byteCount);
                        buffQueue.Enqueue(buffMsg);
                        if (byteCount == 2)
                        {
                            finalACK = true;
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::continueOBCComm-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Traverses through the recieved Tag message from the OBC, and sends
        /// each TagList to writeToDatalogFile().
        /// </summary>
        /// <param name="msgBase">Incoming Tag messages from the OBC</param>
        private List<TagList> traverseTagMessage(PTEMessage msgBase)
        {
            listOfTagList = new List<TagList>();

            try
            {
                bool anotherTag = true;
                int msgIndex = 0;

                // First make sure message is valid! If not skip while loop
                if (msgBase.Length < 4 || msgBase.CommandID != 0x05)
                {
                    continueOBCComm();
                    anotherTag = false;
                }
                while (anotherTag)
                {
                    Tags = new TagList();
                    int tagIDindex;
                    int lengthOfTag;

                    // Parse each buffer into a collection and run a for each.. 
                    while (msgBase.Data[msgIndex] != 0xFF)
                    {
                        // Get each Tag ID first inside the message, then move index to first byte of data for that tag
                        tagIDindex = msgBase.Data[msgIndex];
                        msgIndex++;

                        lengthOfTag = Tags.Tags.Find(X => X.TagID == tagIDindex).Length;

                        byte[] dataToAdd = new byte[lengthOfTag];

                        // Place the data in data to add
                        for (int i = 0; i < lengthOfTag; i++)
                        {
                            // Have to recheck everytime it is incremented so to exit the while loop appropriately
                            if (msgBase.Data[msgIndex] == 0xFF)
                            {
                                msgIndex++;
                                // Two consecutive 0xFF's is the end of the message
                                if (msgBase.Data[msgIndex] == 0xFF)
                                {
                                    msgIndex++;
                                    anotherTag = false;
                                    break;
                                }
                                break;
                            }
                            // Check for conjoining of nibbles
                            else if (msgBase.Data[msgIndex] == 0xF0)
                            {
                                byte highNibble = (byte)(msgBase.Data[msgIndex] & 0xF0);
                                msgIndex++;
                                byte lowNibble = (byte)(msgBase.Data[msgIndex] & 0x0F);
                                // Conjoin the nibbles to form byte of data
                                dataToAdd[i] = (byte)(highNibble | lowNibble);
                            }
                            else
                            {
                                dataToAdd[i] = msgBase.Data[msgIndex];
                            }
                            msgIndex++;
                        }
                        // Add each Tag and Data to the List of TagLists
                        Tags.Tags.Find(X => X.TagID == tagIDindex).AbsoluteDataWrite(dataToAdd);

                    } // End While for each Tag message read until 0xFF!!
                    msgIndex++;
                    // Checks that every tag in list has Data
                    bool allHaveData = Tags.Tags.TrueForAll(X => X.HasData);

                    if (allHaveData)
                    {
                        // Add the Full Tag List to the List of TagLists
                        fullTagListReceived = fullTagList(Tags);
                        listOfTagList.Add(fullTagListReceived);
                    }
                    else
                    {
                        // Turn the Partial Recieved TagList to a Full TagList and Store into fullTagListReceived
                        fullTagListReceived = turnPartialToFull(Tags, fullTagListReceived);
                        listOfTagList.Add(fullTagListReceived);
                    }

                    if (msgIndex >= (msgBase.Data.Length - 2))
                    {
                        anotherTag = false;
                        break;
                    }
                } // End While, read the entire message

                // Only use the Last Tag receieved to find the percent completed

                percentDownloadComplete(fullTagListReceived);

                return listOfTagList;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::traverseTagMessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return listOfTagList;
            }
        }

        /// <summary>
        /// Returns the percent download for the user to know how far they are in the process.
        /// This function uses the Start time, End time, and Current time. The start and end times
        /// are the times that were send to the OBC, and the current time is obtained from the current tag
        /// being receieved.
        /// </summary>
        /// <param name="currentTagList">Each tag list is sent for the current time to be evaluated</param>
        /// <returns>Int as a percent of the download</returns>
        private decimal percentDownloadComplete(TagList currentTagList)
        {
            try
            {
                decimal percentCompleted = 0;

                // Find where the download currently is
                DateTime currentDT = bytesToDateTime(currentTagList.Tags.Find(X => X.TagID == 0).Data());

                current = computeSecondsTicks(currentDT);

                // Only Compute start and end on the first iteration, no point finding any other time for they stay the same
                if (tagIndex == 0)
                {
                    end = computeSecondsTicks(EndDT);

                    // Set start to Current, since the OBC typically begins sending data before the start date time sent
                    start = current;
                }

                if (current >= start)
                {
                    // To Compute than current must be greater than the start time
                    percentCompleted = (Decimal.Divide((current - start), (end - start))) * 100;
                }

                Console.WriteLine("Percent Here: " + percentCompleted);
                return percentCompleted;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::percentDownloadComplete-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

        /// <summary>
        /// This is a helped function for percentDownloadComplete, it takes the DateTime
        /// and converts it to local time of the users computer. Then it measures it as
        /// an integer to find the percent of the download.
        /// </summary>
        /// <param name="thisDateTime">DateTime being processed</param>
        /// <returns>Seconds since 1,1,2000 as an integer</returns>
        private int computeSecondsTicks(DateTime thisDateTime)
        {
            try
            {
                DateTime computeDT = new DateTime(2000, 1, 1);

                long elapsedTicks = thisDateTime.Ticks - computeDT.Ticks;

                TimeSpan elapsed = new TimeSpan(elapsedTicks);

                int totalSecs = Convert.ToInt32(elapsed.TotalSeconds);

                return totalSecs;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::computeSecondsTicks-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Saves a TagList that has all of the tags filled with data.
        /// </summary>
        /// <param name="thisTagList">Received full TagList from OBC</param>
        /// <returns>Full Tag List</returns>
        private TagList fullTagList(TagList thisTagList)
        {
            try
            {
                TagList tagListFull;
                tagListFull = thisTagList;
                return tagListFull;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::fullTagList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Traveres through the Partial Tag List for missing Data, and fills it in from the previously 
        /// stored full TagList.
        /// </summary>
        /// <param name="partialTL">Recieved Partial TagList from OBC</param>
        /// <param name="fullTL">Last Saved Full TagList</param>
        /// <returns>Updated Full Tag List</returns>
        private TagList turnPartialToFull(TagList partialTL, TagList fullTL)
        {
            try
            {
                for (int i = 0; i < 92; i++)
                {
                    if (partialTL.Tags.Find(X => X.TagID == i).HasData)
                    {
                        // Do nothing for data exists
                    }
                    else    // No data exists
                    {
                        // So write old data to the newer data list at that index
                        partialTL.Tags.Find(X => X.TagID == i).AbsoluteDataWrite(fullTL.Tags.Find(X => X.TagID == i).Data());
                    }
                }
                return partialTL;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::turnPartialToFull-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Creates a file (and saves) using the DataTable for the user to 
        /// have all of the data as reference, and re-create the events from the OBC.
        /// </summary>
        /// <param name="allMyTags">The Full List of TagLists</param>
        private void writeToDatalogFile(object sender, DoWorkEventArgs e)
        {
            try
            {
                continueOBCComm();
                PTEMessage msgBase = new PTEMessage();
                List<TagList> theseTags = new List<TagList>();
                moreData = true;
                goAgain = true;
                skipThis = false;
                packetLoss = 0;

                // Gather all of the data, until no data is left
                while (moreData)
                {
                    continueOBCComm();

                    if (tagIndex == 0)
                    {
                        // Represents the first Datalog Message from the OBC
                        msgBase = new PTEMessage(InitialDatalogResponse.InitialBuff);
                        bytesLeft = byteCount;
                    }
                    else
                    {
                        bool poppedOff = buffQueue.TryDequeue(out buff);
                        // Retrieves all Datalog Messages from OBC after the initial one
                        while (!poppedOff)
                        {
                            continueOBCComm();
                            Thread.Sleep(2);
                            poppedOff = buffQueue.TryDequeue(out buff);
                        }
                        bytesLeft = buff.Count();
                        msgBase = new PTEMessage(buff);
                        // Message may need parsed, but let's make sure the first message does not have a Command ID of ACK, then parsing is not needed
                    }
                    checkPTEmsg(msgBase);

                    // This loop manages the TagLists, buff parsing, and adding data to file
                    // Exits when no data is left and new buff needs read
                    while (goAgain)
                    {
                        continueOBCComm();
                        // Must create a new instance every time
                        theseTags = new List<TagList>();
                        theseTags = traverseTagMessage(msgBase);
                    
                        getEachTagList(theseTags);

                        using (StreamWriter writer = new StreamWriter(_fileName + ".SDF", true))
                        {
                            tagObjectList.ForEach(i => writer.Write("{0} ", (i)));
                            writer.Close();
                        }

                        if (CanUploadCSV)
                        {
                            using (StreamWriter writer = new StreamWriter(_fileName + ".CSV", true))
                            {
                                tagObjectList.ForEach(i => writer.Write("{0} ", (i)));
                                writer.Close();
                            }
                        }
                        tagObjectList.Clear();

                        continueOBCComm();
                        int Length = msgBase.Data.Length + 4;

                        if (bytesLeft > Length)
                        {
                            parseMSG = true;
                        }
                        else
                        {
                            parseMSG = false;
                            continueOBCComm();
                        }
                        // Now check if message needs parsed, This bool is set when checking SocketManager
                        if (parseMSG && (bytesLeft > 0))
                        {
                            goAgain = true;
                            byte[] newBuff = parseBuffMsg(Length);

                            if (newBuff.Count() > 1)
                            {
                                // Now call PTEMessage with the full PTEMessage in newBuff
                                msgBase = new PTEMessage(newBuff);
                                checkPTEmsg(msgBase);
                            }
                            else
                            {
                                continueOBCComm();
                                goAgain = false;
                                moreData = true;
                                parseMSG = false;
                                Array.Clear(buff, 0, buff.Count());
                            }
                        }
                        else
                        {
                            continueOBCComm();
                            // Don't need to Parse the Buffer, so read next buffer
                            goAgain = false;
                            moreData = true;
                            Array.Clear(buff, 0, buff.Count());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::writeToDatalogFile-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// This method handles parsing the buffer messages for writeToDatalogFile.
        /// </summary>
        /// <param name="msgLength">Length of the PTEMessage that has just been used</param>
        /// <returns>The next byte[] containing the next PTEMessage</returns>
        private byte[] parseBuffMsg(int msgLength)
        {
            byte[] nullByte = new byte[1];
            nullByte[0] = 0;
            try
            {
                if (!skipThis)
                {
                    // Skip this part when messages need conjoined!
                    // When messages are conjoined, the first message is read in and then deleted for buff
                    // And this logic deletes the first message in buff to read the next message
                    int Length1 = msgLength;

                    // Update the number of bytes left in the message buff
                    bytesLeft = bytesLeft - Length1;

                    buff = buff.Skip(Length1).ToArray();
                }
                skipThis = false;

                int Length2 = 4;

                if (bytesLeft > 3)
                {
                    Length2 = ((buff[2] << 8) + buff[3]) + 4;
                }

                if (buff[0] == OBCCommunication.PRODUCT_ID)
                {
                    if (bytesLeft > 1)
                    {
                        if (buff[1] == 0x81)        // TODO - HARD CODE
                        {
                            goAgain = false;
                            moreData = false;
                            parseMSG = false;
                            finalACK = true;
                            return buff;
                        }
                        else if (buff[1] == 0x05)   // TODO - HARD CODE
                        {
                            // Move on to the next IF statement
                        }
                        else
                        {
                            goAgain = false;
                            moreData = true;
                            Array.Clear(buff, 0, buff.Count());
                            return nullByte;
                        }
                    }
                    // Check to make sure that the full PTEMessage is in this buffer, 
                    // If it is not, then the next buffer must be read and conjoined
                    // Make sure Length2 is not bigger than maxMsgSize, this would only
                    // happen if a packet is mixed up or junk, this check ensures no exceptions
                    // These restrictions were all found during debugging...
                    if ((Length2 > bytesLeft) && (Length2 < maxMsgSize))
                    {
                        bool poppedOff = buffQueue.TryDequeue(out byte[] secBuff);
                        int countTry = 0;
                        while(!poppedOff)
                        {
                            poppedOff = buffQueue.TryDequeue(out secBuff);
                            if (buffQueue.IsEmpty)
                            {
                                continueOBCComm();
                                countTry++;
                                Thread.Sleep(3);
                            }
                            if(countTry > 5)
                            {
                                break;
                            }
                        }
                        
                        int secBLength = secBuff.Count();

                        if (bytesLeft == 1)
                        {
                            Length2 = ((secBuff[1] << 8) + secBuff[2]) + 4;
                        }
                        else if (bytesLeft == 2)
                        {
                            Length2 = ((secBuff[0] << 8) + secBuff[1]) + 4;
                        }
                        else if (bytesLeft == 3)
                        {
                            Length2 = ((buff[2] << 8) + secBuff[0]) + 4;
                        }
                        else
                        {
                            // Length2 stays the same
                        }
                        // No need to make this newBuff bigger than needed
                        byte[] newBuff = new byte[Length2];

                        // Copy what is left in buff to newBuff
                        Buffer.BlockCopy(buff, 0, newBuff, 0, bytesLeft);

                        int skipByte = Length2 - bytesLeft;

                        Buffer.BlockCopy(secBuff, 0, newBuff, bytesLeft, skipByte);

                        secBuff = secBuff.Skip(skipByte).ToArray();

                        // Now bytes Left can finally be updated!
                        bytesLeft = secBLength - skipByte;

                        // Empty buff and copy secBuff to buff
                        buff = new byte[eachTagBufferSize];

                        Buffer.BlockCopy(secBuff, 0, buff, 0, bytesLeft);

                        // Set this boolean so the next time through, buff isn't reduced since it's new data
                        skipThis = true;
                        goAgain = true;
                        parseMSG = true;
                        return newBuff;
                    }
                    else if ((Length2 > maxMsgSize) || (buff[0] != OBCCommunication.PRODUCT_ID))
                    {
                        // First check to make sure the length read for the message isn't bigger
                        // than the maximum allowed buffer size because this is an error! And
                        // it should be found before the buff is structured into a PTEMessage
                        // Just clear the buff and start over
                        // Want to keep track of the accurate/ authentic number of Packet losses..
                        goAgain = false;
                        moreData = true;
                        packetLoss++;
                        Array.Clear(buff, 0, buff.Count());
                        return nullByte;
                    }
                    else
                    {
                        return buff;
                    }
                }
                else
                {
                    return nullByte;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::parseMsg-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return nullByte;
            }
        }

        /// <summary>
        /// Checks the CommandID, ProductID, and Length of the PTEMessage.
        /// Then the method updates the booleans for writeToDatalogFile.
        /// </summary>
        /// <param name="dataMsg">Current PTEMessage</param>
        private void checkPTEmsg(PTEMessage dataMsg)
        {
            try
            {
                // The Message has the right Product ID, check Command IDs
                if (dataMsg.ProductID == OBCCommunication.PRODUCT_ID)
                {
                    // The following IF statements check for reasons to break from the inner/outer while loop(s)
                    if (dataMsg.CommandID == 0x81)       // TODO - HARD CODE
                    {
                        goAgain = false;
                        moreData = false;
                        finalACK = true;
                        parseMSG = false;
                        return;
                    }
                    else if (dataMsg.CommandID == 0x05 && (dataMsg.Length > 0))
                    {
                        goAgain = true;
                        moreData = true;
                    }
                    else
                    {
                        continueOBCComm();
                        packetLoss++;
                        goAgain = false;
                        moreData = true;
                        parseMSG = false;
                        return;
                    }
                }
                else
                {
                    continueOBCComm();
                    // This is incase of an error for creating PTEMessage, 
                    goAgain = false;
                    moreData = true;
                    packetLoss++;
                    Array.Clear(buff, 0, buff.Count());
                    return;
                }

                // Check if message needs parsed
                if ((dataMsg.Length + 4) < bytesLeft)
                {
                    parseMSG = true;
                }

                return;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::checkBuffValidity-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// This function takes a single TagList from the List of TagLists obtained from a single PTEMessage.
        /// Then it sets each row cell with the Data and Data Type, and returns the row.
        /// </summary>
        /// <param name="thisTagList">TagList currently being read</param>
        /// <returns>Row of Data for the File to write</returns>
        private void getEachTagList(List<TagList> thisTagList)
        {
            try
            {
                for (int l = 0; l < thisTagList.Count; l++) //thisTagList.Count
                {
                    string dataArray;

                    // Add one for index
                    dataArray = tagIndex.ToString() + ", ";
                    tagObjectList.Add(dataArray);

                    for (int k = 0; k < 92; k++)    // TODO : Needs changed (all need changed in this file.)
                    {
                        // Looking for types of Hex, since C# doesn't define this data type
                        TAG_VALUE_TYPES holder = referenceList.Tags.Find(X => X.TagID == k).TagType; //thisTagList[l]

                        if (holder == TAG_VALUE_TYPES.HEX)
                        {
                            int byteLength = thisTagList[l].Tags.Find(X => X.TagID == k).Length;
                            dataArray = (byteArrayToStringHex((thisTagList[l].Tags.Find(X => X.TagID == k)).Data(), byteLength)).ToString() + ", ";
                        }
                        else if (holder == TAG_VALUE_TYPES.BYTES)
                        {
                            int byteLength = thisTagList[l].Tags.Find(X => X.TagID == k).Length;
                            dataArray = (byteArryToString((thisTagList[l].Tags.Find(X => X.TagID == k)).Data(), byteLength)).ToString() + ", ";
                        }
                        else if (holder == TAG_VALUE_TYPES.DATE)
                        {
                            dataArray = (bytesToDateTime(thisTagList[l].Tags.Find(X => X.TagID == k).Data())).ToString() + ", ";
                        }
                        else if (holder == TAG_VALUE_TYPES.DECIMAL)
                        {
                            int byteLength = thisTagList[l].Tags.Find(X => X.TagID == k).Length;
                            dataArray = (bytesToInt((thisTagList[l].Tags.Find(X => X.TagID == k)).Data(), byteLength)).ToString() + ", ";
                        }
                        else
                        {
                            dataArray = (thisTagList[l].Tags.Find(X => X.TagID == k).Data()).ToString() + ", ";
                        }
                        
                        tagObjectList.Add(dataArray);
                    }

                    tagObjectList.Add("\n");

                    tagIndex++;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::getEachTagList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Creates a .csv (if the user has proper permissions) of the Datalog Download.
        /// </summary>
        private void datalogWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::datalogWorkerComplete-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Converts the byte array to an Integer for the DataTable.
        /// </summary>
        /// <param name="byteInt">Byte Array</param>
        /// <param name="Length">Length of Byte Array</param>
        /// <returns>Integer</returns>
        private static int bytesToInt(byte[] byteInt, int Length)
        {
            try
            {
                ushort value;

                if (Length == 1)
                {
                    value = byteInt[0];
                }
                else if (Length == 2)
                {
                    value = (ushort)((byteInt[0] << 8) + byteInt[1]);
                }
                else
                {
                    value = 0;
                }
                return value;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::bytesToInt-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }

        }

        /// <summary>
        /// Convert Byte Array of seconds from 1,1,2000 to the actual time it represents in DateTime format.
        /// </summary>
        /// <param name="byteDateTime">Byte Array of seconds since 1,1,2000</param>
        /// <returns>DateTime byte array represents</returns>
        private static DateTime bytesToDateTime(byte[] byteDateTime)
        {
            try
            {
                DateTime returnedDT;
                long t, tt;
                Byte b;
                tt = 0;

                DateTime dtWork = new DateTime(2000, 1, 1);

                for (int i = 0; i < 4; i++)
                {
                    tt = (tt << 8);
                    b = byteDateTime[i];
                    t = Convert.ToInt32(b);
                    // tt is now a 32-bit integer representing the number of seconds since 1/1/2000
                    tt += t;
                }
                // Add the number of seconds to the DateTime object, resulting in a DateTime objectthat represents the current OBC time
                dtWork = dtWork.AddSeconds(tt);

                // The last byte of Date Time from OBC is milliseconds, Add them
                b = byteDateTime[4];
                t = Convert.ToInt32(b);

                returnedDT = dtWork.AddMilliseconds(t);

                returnedDT = returnedDT.ToLocalTime();

                return returnedDT;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::bytesToDateTime-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Converts byte array to String to represents the bytes.
        /// </summary>
        /// <param name="byteToString">Byte Array</param>
        /// <returns>String that Represents the Bytes</returns>
        private static string byteArryToString(byte[] byteToString, int Length)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Length; i++)
                {
                    sb.Append(String.Format("{0:X2}", byteToString[i]));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::getTagType-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Creates a String for the output for the Hex Data Type defined for the specific Tag.
        /// </summary>
        /// <param name="byteToHex"></param>
        /// <returns></returns>
        private static string byteArrayToStringHex(byte[] byteToHex, int Length)
        {
            try
            {
                if (Length == 1)
                {
                    return String.Format("0x{0:X2}", byteToHex[0]);
                }
                else if (Length == 2)
                {
                    return String.Format("0x{0:X2}{1:X2}", byteToHex[0], byteToHex[1]);
                }
                else
                {
                    return "Not Supported";
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Datalog::byteArrayToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }
}