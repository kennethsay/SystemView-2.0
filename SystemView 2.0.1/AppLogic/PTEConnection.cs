using System;
using System.Collections.Generic;
using System.Text;

using Transport;

namespace AppLogic
{
    // To call PTEConnection namePTEConnection = new PTEConnection();


    //
    // CLASS: PTEConnection
    //
    // Description: This class obtains a connection with the OBC.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //      static OBCCommunication _comm       - OBCCommunication Variable
    //      BatteryLevel _batteryLevel          - BatteryLevel Variable
    //      RTC _rtc                            - RTC Variable
    //      Config _config                      - Config Variable
    //      TagList Tags                        - TagList Variable
    //
    // Public Get/Set Accessors:
    //      OBCCommunication Comm (Get only)
    //      BatteryLevel BatteryLevel (Get only)
    //      RTC RealTimeClock (Get Only)
    //      Config ConfigSettings (Get only)
    //
    // Public Methods:
    //      bool IsConnected()                  - Checks if OBC is connected
    //
    // Private Methods:
    //
    // Constructors:
    //      PTEConnection()                     - Default constructor
    //
    // Public Overrides:
    //   
    
    public class PTEConnection
    {
        private static OBCCommunication _comm;
        
        /// <summary>
        /// Default Constructor obtains a connection with the OBC
        /// </summary>
        public PTEConnection()
        {
            try
            {
                _comm = new OBCCommunication();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PTEConnection::PTEConnection-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// STATIC Comm connection - only one allowed
        /// </summary>
        public static OBCCommunication Comm
        {
            get
            {
                return _comm;
            }
        }
                
                
        /// <summary>
        /// Checks if the Application is connected to the OBC.
        /// </summary>
        /// <returns>True if connected, otherwise false</returns>
        public bool IsConnected()
        {
            return Comm.Connected;
        }

        /// <summary>
        /// Returns IP as a string
        /// </summary>
        /// <returns>IP as string</returns>
        /*public string IPAddress()
        {
            return Comm.IP;
        }*/

        /// <summary>
        /// Ceates a new connection with a connected OBC
        /// </summary>
        public void NewConnection()
        {
            _comm.Connect();
        }

        /// <summary>
        /// Attempts to detect a connection interface to an OBC
        /// </summary>
        /// /// <returns>Detection status</returns>
        public bool Detect()
        {
            return _comm.CanDetect();
        }
    }
}
