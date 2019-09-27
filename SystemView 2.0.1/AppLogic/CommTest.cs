using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Transport;


namespace AppLogic
{
    // To call CommTest nameCommTest = new CommTest();


    // CLASS: CommTest
    //
    // Description: This class implements the application-level logic for testing the connection to the OBC.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    //
    // Private Data:
    //      bool _connectionEstablished         - Holds the bool value indicating whether or not the connection is established and valid
    //      string _elapsedTime                 - String indicating the time taken fo the test
    //
    // Public Get/Set Accessors:
    //      bool ConnectionEstablished (Get only)
    //      string ElapsedTime (Get only)
    //
    // Public Methods:
    //
    // Private Methods:
    //      void testConnection()               - Checks socket connection as first stage of test, uses stopwatch object to handle timekeeping
    //      void fetch()                        - Second stage of test; sends status request to OBC and checks response to verify that product IDs match
    //
    // Constructors:
    //      CommTest()                          - Default constructor that calls testConnection()
    //
    // Public Overrides:
    //      string ToString()
    //   

    public class CommTest
    {
        private bool _connectionEstablished;
        private string _elapsedTime;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CommTest()
        {
            try
            {
                // Start the test immediately when instantiated
                testConnection();
            }
            catch (Exception ex)
            {
                // Print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CommTest::CommTest-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Returns a boolean value indicating pass (true) or failure (false) of the connection test
        /// </summary>
        public bool ConnectionEstablished
        {
            get
            {
                try
                {
                    return _connectionEstablished;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("CommTest::ConnectionEstablished-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    // Return false if there's an exception 
                    return false;
                }
            }
            set
            {
                try
                {
                    // Set value of _connectionEstablished
                    _connectionEstablished = value;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("CommTest::ConectionEstablished.set-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Returns a string indicating the time taken for the test 
        /// </summary>
        public string ElapsedTime
        {
            get
            {
                try
                {
                    return _elapsedTime;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("CommTest::ElapsedTime-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                    return "error";
                }
            }
            set
            {
                try
                {
                    // Set value of _elapsedTime
                    _elapsedTime = value;
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("CommTest::Elapsedtime.set-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Tests the connection to the OBC and measures the time taken
        /// </summary>
        private void testConnection()
        {
            try
            {
                // Create and start stopwatch
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                // First check that socket is connected
                if (PTEConnection.Comm.Connected == false)
                {
                    ConnectionEstablished = false;
                }
                // Call function to send status request and verify response
                else
                {
                    fetch();
                }

                // Stop clock, get elapsed time 
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                // Convert time to string
                ElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CommTest::testConnection-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Sends a status request to the OBC, verifies the response, and sets the value of connectionEstablished accordingly
        /// </summary>
        private void fetch()
        {
            // Send a status request
            try
            {
                PTEMessage msgBase = new PTEMessage(PTEConnection.Comm.SendStatusRequest());

                // Byte 1 of the status response is the command code, should be 0x25 (placeholder for now)
                if (msgBase.CommandID == 0x25)  // TODO - CHANGE HARD CODE TO DEFINED VALUE
                {
                    ConnectionEstablished = true;
                }
                else
                {
                    ConnectionEstablished = false;
                }
            }
            catch (TransportException ex)
            {
                // Timeout occurred, product IDs do not match
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CommTest::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                ConnectionEstablished = false;
            }
            catch (Exception ex)
            {
                // Any exception other than a timeout
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CommTest::fetch-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                ConnectionEstablished = false;
            }
        }

        /// <summary>
        /// Returns a string reporting the status of connectionEstablished and elapsedTime
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Connection Established: {0}, Elapsed Time: {1}", ConnectionEstablished, ElapsedTime));

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
