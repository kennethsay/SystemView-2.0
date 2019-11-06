using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Transport;

namespace AppLogic
{
    // To call RTM nameRTM = new RTM();


    //
    // CLASS: RTM
    //
    // Description: This class implements the application-level logic for starting and executing the Data Presentation functionality of SystemView.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //      bool _rtmStarted                    - Boolean value indicating whether or not the RTM has been started by the OBC
    //      static TagList myTagList            - Static TagList object to hold a single record of data
    //
    // Public Get/Set Accessors:
    //      bool RTMStarted (Get & Set)
    //
    // Public Methods:
    //      TagList CollectRecord()             - Calls fetch() (may also verify that RTM has been started first)
    //
    // Private Methods:
    //      void startRTM()                     - Sends the message to start the RTM, verifies the response, and sets the values of _rtmStarted
    //      void fetch()                        - Sends the Interrogate message to the OBC and checks the reponse. If the response contains data, it is written to myTagList
    //
    // Constructors:
    //      RTM()                               - Default constructor that creates the TagList object and calls startRTM()
    //
    // Public Overrides:
    //      string ToString()
    //

    public class RTM
    {
        private bool _rtmStarted;
        private static TagList myTagList;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RTM()
        {
            try
            {
                myTagList = new TagList();
                // Send the command when instantiated
                startRTM();
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM::RTM-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the RTM has been started by the OBC
        /// </summary>
        public bool RTMStarted
        {
            get
            {
                try
                {
                    return _rtmStarted;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("RTM::RTMStarted-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    return _rtmStarted;
                }
            }
            set
            {
                try
                {
                    _rtmStarted = value;
                }
                catch (Exception ex)
                {
                    // print a message to indicate where the exception occurred
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("RTM::RTMStarted.get-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Sends the OBC a message to start the real-time monitor
        /// </summary>
        private void startRTM()
        {
            try
            {
                // This should be false initially
                RTMStarted = false;

                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendStartRTMRequest());

                if (msgBase.CommandID == 0x81)      // TODO - CHANGE HARD CODE TO DEFINED VARIABLE
                {
                    RTMStarted = true;
                }
                else
                {
                    // Should already be false in this case but this is extra insurance
                    RTMStarted = false;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM::startRTM-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Gets a single RTM record from the OBC
        /// </summary>
        public TagList CollectRecord()
        {
            try
            {
                if (RTMStarted)
                {
                    return fetch();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM::CollectRecord-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Sends the Interrogate command to the OBC
        /// </summary>
        private TagList fetch()
        {
            try
            {
                // Send the Interrogate Command to request a data record from the OBC
                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendRTMRequest());

                if (msgBase.CommandID == 0x11)      // TODO - CHANGE HARD CODE TO DEFINED VARIABLE
                {
                    // Index of byte array msgBase.Data
                    int msgIndex = 0;
                    // Boolean variable to signal whether or not to continue reading
                    bool getDataEnable = true;

                    while (getDataEnable)
                    {
                        // Int representing a single byte of data from OBC
                        int DataByte = 0;
                        // Assign data to dataByte
                        DataByte = (int)msgBase.Data[msgIndex];
                        // 0xFF signals the end of the message
                        if (DataByte == 0xFF)
                        {
                            getDataEnable = false;
                        }
                        else
                        {
                            int lengthOfTag = 0;
                            lengthOfTag = myTagList.Tags.Find(x => x.TagID == DataByte).Length;

                            if (lengthOfTag != 0)
                            {
                                // Add the Data to the TagList
                                byte[] dataToAdd = new byte[lengthOfTag];

                                msgIndex++;
                                for (int i = 0; i < lengthOfTag; i++)
                                {
                                    // If 0xF0 Escape character is encountered, the next two bytes collide nibbles
                                    if (msgBase.Data[(msgIndex)] == 0xF0)
                                    {
                                        Byte High = (byte)(msgBase.Data[(msgIndex)]);
                                        msgIndex++;
                                        Byte Low = (msgBase.Data[(msgIndex)]);
                                        byte byteToAdd = (byte)(High | Low);
                                        dataToAdd[i] = byteToAdd;
                                    }
                                    else
                                    {
                                        dataToAdd[i] = msgBase.Data[(msgIndex)];
                                    }
                                    msgIndex++;
                                }
                                myTagList.Tags.Find(x => x.TagID == DataByte).AbsoluteDataWrite(dataToAdd);
                            }
                        }
                    }
                    return myTagList;
                }
                else
                {
                    // No event returned
                    return null;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());

                TagList myTagList = new TagList();
                return null;
            }
        }

        /// <summary>
        /// Returns a string reporting the status of _rtmStarted
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM Started: {0}", _rtmStarted));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTM::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
        }
    }
}
