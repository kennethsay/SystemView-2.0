using System;
using System.Collections.Generic;
using System.Text;

using Transport;

namespace AppLogic
{
    // To call RTC nameRTC = new RTC();


    // CLASS: RTC
    //
    // Description: This class implements the application-level logic for retrieving the date/time from the OBC and the PC, 
    //              and synchronizing the two.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    //
    // Private Data:
    //      DateTime _OBCdt             - Holds the DateTime value of the current OBC time
    //      DateTime _PCdt              - Holds the DateTime value of the current PC time
    //
    // Public Get/Set Accessors:
    //      DateTime OBCLocalTime (Get only)
    //      DateTime PCTime (Get only)
    //
    // Public Methods:
    //      void Sync()                 - Synchronizes the OBC time with the PC time
    //
    // Private Methods:
    //      void fetch()                - Retrieves the values of the date/times from the OBC and the PC and assigns them to appropriate variables
    //
    // Constructors:
    //      RTC()                       - Default constructor that calls fetch()
    //
    // Public Overrides:
    //      string ToString()
    //      

    public class RTC
    {
        private DateTime _OBCdt;
        private DateTime _PCdt;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RTC()
        {
            try
            {
                // Immediately update PC and OBC times when instantiated
                fetch();
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC::RTC-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the OBC RTC in LocalTime
        /// </summary>
        public DateTime OBCLocalTime
        {
            get
            {
                try
                {
                    // Update times and return OBC time - converted to local time
                    fetch();
                    return _OBCdt.ToLocalTime();
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("RTC::OBCLocalTime-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    return _OBCdt.ToLocalTime();
                }
            }
            set
            {
                try
                {
                    // Set value of _OBCdt
                    _OBCdt = value;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("RTM::RTMStarted.get-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Returns current PC time
        /// </summary>
        public DateTime PCTime
        {
            get
            {
                try
                {
                    // Update times and return current PC time
                    fetch();
                    return _PCdt;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("RTC::PCTime-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    return _PCdt;
                }
            }
        }

        /// <summary>
        /// Set the OBC time to the current PC time
        /// </summary>
        /// <returns></returns>
        public void Sync()
        {
            try
            {
                // Create current datetime and send to SendSetDateTimeRequest
                DateTime dateTime = DateTime.UtcNow;

                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendSetDateTimeRequest(dateTime));

                if (msgBase.CommandID != 0x81)      // TODO - CHANGE HARD CODE TO DEFINED VALUE
                {
                    // TODO - change this message to an exception when exception library is written
                    Console.WriteLine("OBCCommunication.SendSetDateTimeRequest called by RTC.Sync returned wrong command ID");
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC::sync-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Update values for OBC time and PC time
        /// </summary>
        /// <returns></returns>
        private void fetch()
        {
            try
            {
                long t, tt;
                Byte b;
                tt = 0;
                DateTime currentDT;

                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendGetDateTimeRequest());

                if (msgBase.CommandID != 0x13) // TODO - CHANGE HARD CODE TO DEFINED VALUE
                {
                    Console.WriteLine("OBCCommunication.SendGetDateTimeRequest called by RTC.fetch returned wrong command ID (this message will eventually become an exception)");
                }
                else
                {
                    // The message returned by the OBC contains the number of seconds since 1/1/2000 represented as a byte array
                    // This loop converts that byte array to a 32-bit integer
                    for (int i = 0; i < 4; i++)
                    {
                        tt = (tt << 8);
                        b = msgBase.Data[i];
                        t = Convert.ToInt32(b);
                        // tt is now a 32-bit integer representing the number of seconds since 1/1/2000
                        tt += t;
                    }
                    // Create a new DateTime object initialized to 1/1/2000
                    currentDT = new DateTime(2000, 1, 1);
                    // Add the number of seconds to the DateTime object, resulting in a DateTime objectthat represents the current OBC time
                    currentDT = currentDT.AddSeconds(tt);
                    // Set the OBC time to this value
                    OBCLocalTime = currentDT;
                }
                // Set the current PC time
                DateTime dateTime = DateTime.Now;
                _PCdt = dateTime;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Updates the RTC timestamp readings for PC and OBC clocks
        /// </summary>
        public void UpdateRTCReading()
        {
            try
            {
                fetch();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RTC::UpdateRTCReading-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// This method prints and returns a string indicating the current OBC and PC times
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                // First call fetch to ensure the reported times are updated
                fetch();

                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("OBC Time: {0}, PC Time: {1}", OBCLocalTime, PCTime));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CommTest::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
        }
    }
}
