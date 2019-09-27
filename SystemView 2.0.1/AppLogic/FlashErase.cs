using System;
using System.Collections.Generic;
using System.Text;

using Transport;

namespace AppLogic
{
    // To call FlashErase nameFlashErase = new FlashErase();


    // CLASS: FlashErase
    //
    // Description: This class implements the application-level logic for clearing the datalogger flash memory.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    //
    // Private Data:
    //     bool _flashEraseStarted          - holds the bool value indicating whether or not the flash erase command has been started by the OBC
    //                                      NOTE: ACSES responds to this command immediately with a slave acknowledge to indicate that the command was accepted and is started.  
    //                                      Clearing flash memory takes as long as 60 seconds per flash chip (2 chips configured at present).
    //
    // Public Get/Set Accessors:
    //      bool FlashEraseStarted (Get only)
    //
    // Public Methods:
    //
    // Private Methods:
    //      void fetch()                    - Sends the message containing the flash erase command to the OBC, assigns the value of _flashEraseStarted accordingly
    //
    // Constructors:
    //      FlashErase()                    - Default constructor that calls fetch()
    //
    // Public Overrides:
    //      string ToString()
    //      

    public class FlashErase
    {
        private bool _flashEraseStarted;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FlashErase()
        {
            try
            {
                // Send the command when instantiated
                fetch();
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("FlashErase::FlashErase-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the flash erase has been started by the OBC
        /// </summary>
        public bool FlashEraseStarted
        {
            get
            {
                try
                {
                    return _flashEraseStarted;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("FlashErase::FlashEraseStarted-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    return _flashEraseStarted;
                }
            }
            set
            {
                try
                {
                    // Set value of _flashEraseStarted
                    _flashEraseStarted = value;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("FlashErase::FlashEraseStarted.set-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Sends a flash erase command to the OBC, verifies the response, and sets the value of _flashEraseStarted accordingly
        /// </summary>
        private void fetch()
        {
            // Send the command
            try
            {
                // This should be false initially
                FlashEraseStarted = false;

                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendFlashEraseRequest());

                // Verify response from OBC
                // The command code 0x81 is the MTA-specific response for this message
                if (msgBase.CommandID == 0x81)      // TODO - CHANGE HARD CODE TO DEFINED VALUE
                {
                    FlashEraseStarted = true;
                }
                else
                {
                    // Should already be false in this case but this is extra insurance
                    FlashEraseStarted = false;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("FlashErase::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns a string reporting the status of _flashEraseStarted
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Flash Erase Started: {0}", FlashEraseStarted));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("FlashErase::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }
        }
    }
}
