using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Transport
{

    // CLASS: OBCCommunication
    //
    // Description: This class checks if client is connected to OBC and establishes all of the message attributes.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //      bool _bDiag                     - Default Constructor Initializes to False
    //      AsynchronousClient myAsyncClient    -Socket Class Variable
    //
    // Public Get/Set Accessors:
    //      bool Connected (Get only)
    //
    // Public Methods:
    //      bool Connect()                      - Connects the AsynchronousClient, Returns true if attempt is successful
    //      Byte[] WaitReceive()                - Receives the bytes form the socket buffer
    //      Byte[] SendStatusRequest()          - Sends status request to OBC
    //      Byte[] SendStartRTMRequest()        - RTM Request
    //      Byte[] SendRTMRequest()             - Single RTM Request
    //      Byte[] SendFullDataUploadRequest(DateTime startDateTime, DateTime endDateTime, int datalogSource)       - Initial Upload Datalog Request Based on Start and End times
    //      Byte[] RequestUploadAllAvailable()        - Request to Upload All Available Data
    //      Byte[] RequestUploadByBlockPercent(int percentBlock, int datalogSource)     - Upload Request by Percent Block of Data
    //      Byte[] AckAndContinue()             - Acknowledge OBC Response and ask to continue
    //      Byte[] AckAndCancel()               - Acknowledge and Cancel further responses from OBC
    //      Byte[] RequestResend()             - Request Resend
    //      Byte[] SendFlashEraseRequest()      - Request for all Logged Data to be Erased
    //      Byte[] SendGetConfigRequest()       - Request for Current Configuration
    //      Byte[] SendSetConfigRequest(Byte[] ConfigRequestByteStream)     - Change Configuration Request
    //      Byte[] SendGetBatteryLevel()        - Request for Current Battery Level
    //      Byte[] SendGetDateTimeRequest()     - Request for OBC DateTime
    //      Byte[] SendSetDateTimeRequest(DateTime currentDateTime)     - Request to set OBC DateTime
    //      Byte[] SendRevisionRequest()        - Request for the Revisions
    //      bool CanDetect()                    - Returns true of the host can be detected/pinged
    //
    // Private Methods:
    //      Byte[] sendAndWait(Byte[] data, uint nTimeout = 250)        - Sends message to OBC and times out if no response in specified time
    //      Byte toBCD(int value)           - Converts an Int to a Byte
    //
    // Constructors:
    //      OBCCommunication()         - Default Constructor
    //      OBCCommunication(OBCCommunication copy)     - Constructor
    //
    // Public Overrides:
    // 

    public class OBCCommunication
    {
        #region const Byte Definitions
        const Byte CC_OBC_ACK = 0x81;           /* acknowledge  */
        const Byte CC_OBC_NAK = 0x83;		    /* "Slave" negative acknowledge  */
        const Byte CC_OBC_RESEND = 0x85;        /* request resend last message   */
        const Byte CC_OBC_DISCONNECT = 0x87;

        const Byte CC_OBC_REVISION = 0x01;      /* revision response */
        const Byte CC_OBC_CLEAR_FLASH = 0x03;   /* reserved (use acknowledge) */
        const Byte CC_OBC_UPLOAD = 0x05;        /* start upload response */
        const Byte CC_OBC_START_RTM = 0x07;     /* reserved (use acknowledge) */
        const Byte CC_OBC_INTERROGATE = 0x11;   /* realtime data upload response */
        const Byte CC_OBC_GET_DATETIME = 0x13;  /* RTC date/time response */
        const Byte CC_OBC_SET_DATETIME = 0x15;  /* reserved (use acknowledge) */
        const Byte CC_OBC_EVENT_TIMES = 0x17;   /* event times logged response */
        const Byte CC_OBC_GET_CONFIG = 0x19;    /* configuration response */
        const Byte CC_OBC_SET_CONFIG = 0x1B;    /* reserved (use acknowledge) */
        const Byte CC_OBC_BATTERY = 0x1D;       /* RTC battery level response */
        const Byte CC_OBC_STATUS = 0x25;        /* Status Response */

        const Byte PC_INTERROGATE = 0x10;

        const Byte PC_ACK_CONTINUE = 0x82;      /*Acknowledge and continue*/
        const Byte PC_ACK_CANCEL = 0x80;        /*Acknowledge and cancel*/
        const Byte PC_RESEND = 0x84;            /*Resend*/


        const Byte PC_START_RTM = 0x06;
        const Byte PC_CLEAR_FLASH_MEMORY = 0x02;
        const Byte PC_UPLOAD_REQUEST = 0x04;
        const Byte PC_REQUEST_EVENT_TIMES_LOGGED = 0x16;    /*Requesting all of Event Times Logged*/
        const Byte PC_PERCENT_BLOCK = 0x32;                 /*Upload based on percent from user*/

        const Byte PC_GET_DATETIME = 0x12;
        const Byte PC_SET_DATETIME = 0x14;
        const Byte PC_GET_CONFIG = 0x18;
        const Byte PC_SET_CONFIG = 0x1A;
        const Byte PC_GET_REVISIONS = 0x00;
        const Byte PC_GET_STATUS = 0x24;
        const Byte PC_GET_BATTERY_LEVEL = 0x1C;
        #endregion

        // The Product ID identifies the specific application this is connected to
        public const Byte PRODUCT_ID = 0x21;
        public const Byte END_OF_RECORD = 0xFF;

        private bool _bDiag;

        // Socket Class
        private AsynchronousClient myAsyncClient;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OBCCommunication()
        {
            try
            {
                _bDiag = false;
                myAsyncClient = new AsynchronousClient();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("OBCCommunication::OBCCommunication-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OBCCommunication(OBCCommunication copy)
        {
            try
            {
                _bDiag = copy._bDiag;
                myAsyncClient = new AsynchronousClient();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("OBCCommunication::OBCCommunication-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Connected via TCP/IP or not
        /// </summary>
        public bool Connected
        {
            get
            {
                return myAsyncClient.Connected;
            }
        }

        /// <summary>
        /// Connect to OBC
        /// </summary>
        /// <returns>True is success</returns>
        public bool Connect()
        {
            bool rc = false;

            myAsyncClient = new AsynchronousClient();

            myAsyncClient.Connect();
            return rc;
        }

        /// <summary>
        /// Disconnect from the OBC
        /// </summary>
        /// <returns>True is success</returns>
        public bool Disconnect()
        {
            bool rc = false;

            myAsyncClient.Disconnect();
            return rc;
        }

        public AsynchronousClient GetAsyncClient
        {
            get
            {
                return myAsyncClient;
            }
        }

        /// <summary>
        /// Send via Async client and wait for timeout or completion
        /// </summary>
        /// <param name="data"> data to send</param>
        /// <param name="nTimeout"> Defaults to 250ms</param>
        /// <returns>Response</returns>
        private Byte[] sendAndWait(Byte[] data, uint nTimeout = 5000)
        {
            uint nCount = 0;
            bool bTimeout = false;

            Byte[] buffer = new Byte[4096];

            try
            {
                if (myAsyncClient.Connected)
                {
                    bool bDone = false;
                    myAsyncClient.Send(data);

                    myAsyncClient.Receive(buffer);

                    while (!bDone)
                    {
                        // Sleep 10ms
                        Thread.Sleep(10);

                        if (myAsyncClient.ReceiveComplete)
                        {
                            bDone = true;
                        }

                        // Check for timeout
                        if (!bDone && (nTimeout > 0))
                        {
                            nCount += 10;

                            // Timeout check
                            if (nCount >= nTimeout)
                            {
                                bTimeout = true;
                                bDone = true;

                                if (_bDiag)
                                {
                                    Console.WriteLine("Transport - Send timeout");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TransportException exception = new TransportException("Transport::sendAndWait exception {0}", e);
                Console.WriteLine("Transport: Send Exception encountered.");

                //throw exception;
            }
            finally
            {
                // Throw an exception if the timeout was triggered. This can be indicative of a connection fail.
                if (bTimeout)
                {
                    TransportException exception = new TransportException("Transport::sendAndWait timeout");
                    //throw exception;
                }
            }
            return buffer;
        }

        /// <summary>
        /// Waits and Recieves the bytes from the buffer.
        /// </summary>
        /// <returns>Data from socket buffer</returns>
        public Byte[] WaitReceive()
        {
            uint nCount = 0;
            uint nTimeOut = 5000;

            Byte[] buffer = new Byte[4096];

            try
            {
                if (myAsyncClient.Connected)
                {
                    bool bDone = false;

                    myAsyncClient.Receive(buffer);

                    while (!bDone)
                    {
                        Thread.Sleep(1);

                        if (myAsyncClient.ReceiveComplete)
                        {
                            bDone = true;
                        }

                        // Check for timeout
                        if (!bDone && (nTimeOut > 0))
                        {
                            nCount += 10;

                            // Timeout check
                            if (nCount >= nTimeOut)
                            {
                                bDone = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TransportException exception = new TransportException("Transport::sendAndWait exception {0}", e);
                Console.WriteLine("Transport: Send Exception encountered.");

                //throw exception;
            }

            return buffer;
        }

        /// <summary>
        /// Sends a Status Request to the OBC
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendStatusRequest()
        {
            try
            {
                Byte[] data = { PRODUCT_ID, PC_GET_STATUS, 0x02 };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }

                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendStatusRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request to start the real-time monitor
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendStartRTMRequest()
        {
            try
            {
                // Build the message
                Byte[] data = { PRODUCT_ID, PC_START_RTM, 0x02 };

                // Add the CRC
                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }

                return sendAndWait(requestData);
            }
            catch
            {
                // Return empty data if there's an exception
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendStartRTMRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request for a single real Time Record
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendRTMRequest()
        {
            try
            {
                Byte[] requestData = { PRODUCT_ID, PC_INTERROGATE, 0x02, 0x4C, 0x63 };
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendRTMRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends the initial request to upload the full datalog. 
        /// </summary>
        /// <param name="startDateTime">Start time for Data Upload</param>
        /// <param name="endDateTime">End time for Data Upload</param>
        /// <returns>Response</returns>
        public Byte[] SendFullDataUploadRequest(DateTime startDateTime, DateTime endDateTime, int datalogSource)
        {
            try
            {
                const int msgLength = 11;

                // Compute Start and End times in seconds since 1,1,2000
                DateTime computeDT = new DateTime(2000, 1, 1);

                startDateTime = startDateTime.ToUniversalTime();
                endDateTime = endDateTime.ToUniversalTime();

                long elapsedStartTicks = startDateTime.Ticks - computeDT.Ticks;
                long elapsedEndTicks = endDateTime.Ticks - computeDT.Ticks;
                // Change elapsed number to a timespan
                TimeSpan elapsedStart = new TimeSpan(elapsedStartTicks); 
                TimeSpan elapsedEnd = new TimeSpan(elapsedEndTicks);
                // Convert the total seconds to 32 bits.
                Int32 startSeconds = Convert.ToInt32(elapsedStart.TotalSeconds);
                Int32 endSeconds = Convert.ToInt32(elapsedEnd.TotalSeconds);

                byte[] startBytes = new byte[4];
                byte[] endBytes = new byte[4];

                startBytes[0] = (byte)startSeconds; // LSB Start Time Seconds
                startBytes[1] = (byte)(startSeconds >> 8);
                startBytes[2] = (byte)(startSeconds >> 16);
                startBytes[3] = (byte)(startSeconds >> 24); // MSB Start Time Seconds

                endBytes[0] = (byte)endSeconds; // LSB End Time Seconds
                endBytes[1] = (byte)(endSeconds >> 8);
                endBytes[2] = (byte)(endSeconds >> 16);
                endBytes[3] = (byte)(endSeconds >> 24); // MSB End Time Seconds

                byte source = Convert.ToByte(datalogSource);

                Byte[] data = { PRODUCT_ID, PC_UPLOAD_REQUEST, msgLength, startBytes[3], startBytes[2], startBytes[1], startBytes[0], endBytes[3], endBytes[2], endBytes[1], endBytes[0], source };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[14];

                for (int i = 0; i < 12; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 12] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendFullDataUploadRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Requests the the Times of all the Event Logged Data.
        /// </summary>
        /// <returns>Slave Event Response</returns>
        public Byte[] RequestUploadAllAvailable(int datalogSource)
        {
            try
            {
                const int msgLength = 11;

                byte source = Convert.ToByte(datalogSource);

                Byte[] data = { PRODUCT_ID, PC_UPLOAD_REQUEST, msgLength, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, source };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[14];

                for (int i = 0; i < 12; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 12] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::RequestEventTimesLogged had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Allows user to choose the percent of data they would like to upload.
        /// </summary>
        /// <param name="percentBlock">Percent of Data 0-100</param>
        /// <param name="datalogSource">internal recorder/ external logging device 0 or 1</param>
        /// <returns>Upload Response</returns>
        public Byte[] RequestUploadByBlockPercent(int percentBlock, int datalogSource)
        {
            try
            {
                const int msgLength = 4;

                byte blkPercent = Convert.ToByte(percentBlock);
                byte source = Convert.ToByte(datalogSource);

                Byte[] data = { PRODUCT_ID, PC_PERCENT_BLOCK, msgLength, blkPercent, source };

                // Add the CRC
                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[7];

                for (int i = 0; i < 5; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 5] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::RequestUploadByBlockPercent had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// The Acknowledge & Continue message is used by the PC to acknowledge receipt of the last data response from the system.
        /// It also instructs the system that additional data may be sent in response to the Acknowledge & Continue message.  
        /// This message maintains the current process of uploading or acquiring real-time data.
        /// </summary>
        /// <returns>Slave Response</returns>
        public Byte[] AckAndContinue()
        {
            try
            {
                const int msgLength = 2;

                Byte[] data = { PRODUCT_ID, PC_ACK_CONTINUE, msgLength };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::AckAndContinue had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// The Acknowledge & Cancel message is used by the PC to acknowledge 
        /// receipt of the last data response from the system.  
        /// It also instructs the  system that additional data is no longer requested.  
        /// This cancels the current process of uploading or acquiring real-time data.
        /// </summary>
        /// <returns>Slave Ack</returns>
        public Byte[] AckAndCancel()
        {
            try
            {
                const int msgLength = 2;

                Byte[] data = { PRODUCT_ID, PC_ACK_CANCEL, msgLength };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::AckAndCancel had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// The Resend message is used by the PC to request that the system resend the last data response.  
        /// The generation of this message implies that the PC did not properly receive or interpret the last data response.
        /// </summary>
        /// <returns>Slave Resend = 0x85</returns>
        public Byte[] RequestResend()
        {
            try
            {
                const int msgLength = 2;
                Byte[] data = { PRODUCT_ID, PC_RESEND, msgLength };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::RequestResend had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request to clear the datalogger's flash memory
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendFlashEraseRequest()
        {
            try
            {
                Byte[] data = { PRODUCT_ID, PC_CLEAR_FLASH_MEMORY, 0x02 };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendFlashEraseRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Send A request for the current system configuration
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendGetConfigRequest()
        {
            try
            {
                Byte[] data = { PRODUCT_ID, PC_GET_CONFIG, 0x02 };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendGetConfigRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request to change a configuration item.
        /// </summary>
        /// <param name="ConfigRequestByteStream">Array of Bytes with configurations</param>
        /// <returns>PRODUCT_ID + Slave ACK if successful</returns>
        public Byte[] SendSetConfigRequest(Byte[] ConfigRequestByteStream)
        {
            try
            {
                const int msgLength = 24;

                Byte[] data = { PRODUCT_ID, PC_SET_CONFIG, msgLength };
                Byte[] fullData = new byte[data.Length + ConfigRequestByteStream.Length];
                // Append the two Byte arrays together, Saved in fullData
                Buffer.BlockCopy(data, 0, fullData, 0, data.Length);   
                Buffer.BlockCopy(ConfigRequestByteStream, 0, fullData, data.Length, ConfigRequestByteStream.Length);

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(fullData);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[27];

                for (int i = 0; i < 25; i++)
                {
                    requestData[i] = fullData[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 25] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendSetConfigRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request for the current battery level
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendGetBatteryLevel()
        {
            try
            {
                Byte[] data = { PRODUCT_ID, PC_GET_BATTERY_LEVEL, 0x02 };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendGetBatteryLevelRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request to retrieve the current RTC value from the OBC
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendGetDateTimeRequest()
        {
            try
            {
                // TODO : CHANGE HARD CODE TO DEFINED CODE
                Byte[] requestData = { PRODUCT_ID, PC_GET_DATETIME, 0x02, 0x2A, 0x01 };
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendGetDateTimeRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request to retrive the current RTC value from the OBC
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendSetDateTimeRequest(DateTime currentDateTime)
        {
            try
            {
                const int msgLength = 9;

                // The number of years until the next leap year
                Byte leapYearByte;
                // If the current year is a leap year, this value is 0
                if (DateTime.IsLeapYear(currentDateTime.Year))
                {
                    leapYearByte = toBCD(0);
                }
                // This computes the number of years until the next leap year
                else
                {
                    leapYearByte = toBCD(4 - (((currentDateTime.Year) - 2000) % 4));
                }

                // Convert DateTime values into BCD bytes
                // BCD = binary coded decimal; tis is the format expected by the OBC
                Byte yearByte = toBCD(currentDateTime.Year - 2000);
                Byte monthByte = toBCD(currentDateTime.Month);
                Byte dayByte = toBCD(currentDateTime.Day);
                Byte hourByte = toBCD(currentDateTime.Hour);
                Byte minuteByte = toBCD(currentDateTime.Minute);
                Byte secondByte = toBCD(currentDateTime.Second);

                Byte[] requestData = { PRODUCT_ID, PC_SET_DATETIME, msgLength, yearByte, monthByte, dayByte, hourByte, minuteByte, secondByte, leapYearByte };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(requestData);

                Byte[] data = new byte[12];
                for (int i = 0; i < 10; i++)
                {
                    data[i] = requestData[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    data[i + 10] = crcBytes[i];
                }
                return sendAndWait(data);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendSetDateTimeRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// Sends a request for all board Revisions from the OBC
        /// </summary>
        /// <returns>Response</returns>
        public Byte[] SendRevisionRequest()
        {
            try
            {
                Byte[] data = { PRODUCT_ID, PC_GET_REVISIONS, 0x02 };

                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);

                if (_bDiag)
                {
                    Console.WriteLine("CRC 0x{0:X2} 0x{1:X2}", crcBytes[0], crcBytes[1]);
                }

                Byte[] requestData = new Byte[5];

                for (int i = 0; i < 3; i++)
                {
                    requestData[i] = data[i];
                }
                for (int i = 0; i < 2; i++)
                {
                    requestData[i + 3] = crcBytes[i];
                }
                return sendAndWait(requestData);
            }
            catch
            {
                Byte[] requestData = new Byte[0];

                if (_bDiag)
                {
                    Console.WriteLine("Transport::SendRevisionRequest had an exception.");
                }
                return requestData;
            }
        }

        /// <summary>
        /// This method sends a test ping to the specified obc interface
        /// </summary>
        /// <returns></returns>
        public bool CanDetect()
        {
            bool Detected = false;
            try
            {
                Detected = myAsyncClient.PingHost();
            }
            catch
            {

            }

            return Detected;

        }

        /// <summary>
        /// This method converts int values from a DateTime object into the Binary Coded Decimal bytes expected by the OBC
        ///  Example: 19 is returned as 0001 1001
        /// </summary>
        /// <returns></returns>
        private Byte toBCD(int value)
        {
            try
            {
                // separate the two digits 
                int lowerNibble = (value % 10);
                int upperNibble = (value / 10);

                // convert the two digits to bytes
                byte lowerByte = Convert.ToByte(lowerNibble);
                byte upperByte = Convert.ToByte(upperNibble);

                // shift upper half of byte by 4
                upperByte = (byte)(upperByte << 4);

                // clear any possible 1's that would corrupt the value of the combined byte
                lowerByte = (byte)(lowerByte & 0x0f);
                upperByte = (byte)(upperByte & 0xf0);

                // combine two nibbles into one byte
                byte returnByte = (byte)(upperByte | lowerByte);

                return returnByte;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("OBCCommunication::toBCD-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }


        ///////////////////////////////////////////////////////////////////////////
        //
        // OPERATOR OVERLOADS
        //
        //////////////////////////////////////////////////////////////////////////



        /*************************************
        public void Process()
        {
            Process(Message);
        }

        public void Process(PTEMessage msg)
        {
            int i, nFrom, nTo;

            // Check CRC
            Byte[] data = new Byte[msg.Length - 2];
            Array.Copy(msg.Data, data, msg.Length - 2);

            Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
            Byte[] crcBytes = crc.ComputeChecksumBytes(data);


            switch (msg.CommandID)
            {
                case CC_OBC_INTERROGATE:
                    nFrom = 0;
                    nTo = 0;

                    for (i = 0; i < msg.Length; i++)
                    {
                        if (msg.Data[i] == END_OF_RECORD)
                        {
                            // Each message ends with FF; parse 
                            nTo = i;
                            PTERealTimeData rt = new PTERealTimeData(msg, nFrom, nTo);
                            nFrom = nTo + 1;
                        }
                    }
                    break;

                case CC_OBC_REVISION:
                    RevRecords = new RevisionParse(msg);
                    break;

                case CC_OBC_STATUS:
                    // Only Byte 2 has non-zero info. It is 1 if flashing chips
                    for (i = 0; i < msg.Length; i++)
                    {
                        Console.Write("0x{0:X2} ", msg.Data[i]);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("Status Information Received and Parsed.");
                    break;

                case CC_OBC_GET_DATETIME:
                    PTETime = new PTEDateTime(msg.Data);
                    break;

                case CC_OBC_GET_CONFIG:
                    ConfigData = new ConfigData(msg);
                    break;

                case CC_OBC_BATTERY:
                    BatteryLevel = new BatteryLevel(msg);
                    break;

                default:
                    Console.WriteLine("Received Command 0x{0:X} from PTE.", msg.CommandID);
                    break;
            }
        }
        ******************************/
    }
}
