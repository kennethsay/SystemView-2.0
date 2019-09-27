using System;
using System.Collections.Generic;
using System.Text;

using Transport;


namespace AppLogic
{
    // To call BatteryLevel nameBatteryLevel = new BatteryLevel();


    //
    // CLASS: BatteryLevel
    //
    // Description: This class obtains the BatteryLevel and displays it to user.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //      int _nRawBatteryLevel           - 1-10 Volt Scaled
    //      float _fReportedBatteryLevel    - 5 Volt scale
    //
    // Public Get/Set Accessors:
    //      int Raw (Get only)
    //      float Level (Get only)
    //
    // Public Methods:
    //
    // Private Methods:
    //      void fetch()                    - Obtains the BatteryLevel from the OBC
    //
    // Constructors:
    //      BatteryLevel()                  - Default constructor
    //
    // Public Overrides:
    //      string ToString()
    //

    public class BatteryLevel
    {
        private int _nRawBatteryLevel;
        private float _fReportedBatteryLevel;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BatteryLevel()
        {
            try
            {
                fetch();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("BatteryLevel::BatteryLevel-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the Raw reading of the last reading
        /// </summary>
        public int Raw
        {
            get
            {
                fetch();
                return _nRawBatteryLevel;
            }
        }

        /// <summary>
        /// Returns the Battery level on a 5 volt scale. 
        /// </summary>
        public float Level
        {
            get
            {
                fetch();
                return _fReportedBatteryLevel;
            }
        }

        /// <summary>
        /// Read a new value from the OBC
        /// </summary>
        private void fetch()
        {
            try
            {
                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendGetBatteryLevel());

                // Battery level is reported on a 100x scale of 1-10 volts.
                // Convert to a 5 volt scale.
                _nRawBatteryLevel = msgBase.Data[0];
                _fReportedBatteryLevel = (float)((_nRawBatteryLevel * 2) / 100.0);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("BatteryLevel::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                _nRawBatteryLevel = 0;
                _fReportedBatteryLevel = 0;
            }
        }



        /// <summary>
        /// Overrides
        /// </summary>
        /// <returns>Raw and Scaled BatteryLevels</returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Battery Level Raw 0x{0:X}, Scaled {1}", _nRawBatteryLevel, _fReportedBatteryLevel));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("BatteryLevel::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }
}
