using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Transport
{
    // To call AsynchronousClient nameAsynchronousClient = new AsynchronousClient();


    // CLASS: SocketManager/ AsynchronousClient/ StateObject
    //
    // Description: This is the base class to establish the TCP/IP Socket connection for the user and OBC to exchange information.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Prvate Data:
    //      Socket _clientSocket     - OBC Specific Value
    //      Byte[] myBuffer         - Length of Message
    //      const int port = 50125      - Defines Port number for the OBC
    //      static string ipAddressString = "10.1.18.108"       - Defines IP Address for OBC
    //      static ManualResetEvent connectDone     - Connection is complete
    //      static ManualResetEvent sendDone        - Message has been sent
    //      static ManualResetEvent receiveDone     - Message has been received
    //      bool _receiveDone             - True if message is receieved
    //
    // Public Get/Set Accessors:
    //      Socket MySocket (Get only)
    //      bool ReceiveComplete (Get only)
    //      bool Connected (Get only)
    //
    // Public Methods:
    //      bool Connect()      - Established connection with the OBC and returns true if successful
    //      void Disconnect()       - Disconnects the connection if there is a connection
    //      void Receive(Byte[] buffer)     - Begins to asynchronously recieve message from OBC
    //      void Send(String data)      - Converts string of data to byte[] and begins sending to OBC
    //      void Send(Byte[] data)      - Sends the byte[] of data to OBC
    //      bool PingHost()             - Pings the system
    //
    // Private Methods:
    //      void connectCallback(IAsyncResult ar)       - Asynchrounous Call Back Routine; called once connection is established
    //      void receiveCallback(IAsyncResult ar)       - Asynchronous Call Back Routine; called once message is received
    //      void sendCallback(IAsyncResult ar)          - Asynchronous Call Back Routine; called once message is sent
    //      IPAddress parse(string ipAddress)           - Checks the IP Adress string, and returns an IPAdress as IPAdress
    //
    // Constructors:
    //      AsynchronousClient()        - Default Constructor, Creates a TCP/IP Socket
    //
    // Public Overrides:
    //

    /// <summary>
    /// State Object for recieving data from the OBC.
    /// Defines Socket, BufferSize, byte[] buffer, and StringBuilder Object.
    /// </summary>
    public class StateObject
    {
        // Client socket
        public Socket workSocket = null;

        // Size of receive buffer
        public const int BufferSize = 4096;

        // Receive buffer
        public byte[] buffer = new byte[BufferSize];

        // Received data string
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousClient
    {
        #region Member Variables
        private static Socket _clientSocket;
        private Byte[] myBuffer;

        // The port number for the remote device
        private const int port = 50125;

        // The IP Address for the remote device
        private static string ipAddressString = "10.1.18.108";

        // ManualResetEvent instances signal completion
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device
        public Byte[] response = new Byte[1];
        private int _bytesReceived;
        private bool _extraPTEMessages;
        private bool _receiveDone;
        #endregion

        #region Accessors
        public Socket MySocket
        {
            get
            {
                return _clientSocket;
            }
        }
        public bool Connected
        {
            get
            {
                return _clientSocket.Connected;
            }
        }
        public bool ReceiveComplete
        {
            get
            {
                return _receiveDone;
            }
        }

        public bool ExtraPTEMessages
        {
            get
            {
                return _extraPTEMessages;
            }
        }

        public int BytesRecieved
        {
            get
            {
                return _bytesReceived;
            }
            set
            {
                _bytesReceived = value;
            }
        }
        #endregion

        /// <summary>
        /// Default Consructor, Creates a TCP/IP Socket.
        /// </summary>
        public AsynchronousClient()
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::AsynchronousClient-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Established Connection for the User with the OBC.
        /// </summary>
        /// <returns>Returns True if Connection is Established</returns>
        public bool Connect()
        {
            // Connect to a remote device
            try
            {
                // Establish the remote endpoint for the socket
                IPAddress ipAddress = parse(ipAddressString);

                // Connect to the remote endpoint
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                _clientSocket.BeginConnect(remoteEP, new AsyncCallback(connectCallback), _clientSocket);
                connectDone.WaitOne();

                return true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::Connect-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return false;
            }
        }

        /// <summary>
        /// Disconnects the connection once established.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::Disconnect-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Called once the Asynchronous Operation completes (Connection is established).
        /// </summary>
        /// <param name="ar">Status of Asynchronous Connection</param>
        private void connectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made
                connectDone.Set();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::connectCallback-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Begins to asynchronously receive data from the OBC. 
        /// </summary>
        /// <param name="buffer">Message from OBC</param>
        public void Receive(Byte[] buffer)
        {
            try
            {
                myBuffer = buffer;
                // Create the state object
                StateObject state = new StateObject();
                state.workSocket = _clientSocket;
                _receiveDone = false;
                _extraPTEMessages = false;

                // Begin receiving the data from the remote device
                _clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(receiveCallback), state);
                receiveDone.WaitOne();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::Receive-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Called once the Asynchronous Operation Completes (Received OBC Message).
        /// </summary>
        /// <param name="ar">Status of Asynchronous Operation</param>
        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // From the asynchronous state object
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device
                // This is the only accurate representation of how many bytes are obtained in the buffer, 
                // for .Count() does not accurately represent the number of actual elements
                // Therefore, this will be used in other classes to keep track of the number of bytes read
                BytesRecieved = client.EndReceive(ar);

                // Length of a PTEMessage is the 3rd and 4th byte plus 4 for the header bytes
                int Length = (((state.buffer[2] << 8) + state.buffer[3]) + 4);
                
                if (BytesRecieved > 0)
                {
                    //Console.WriteLine("Received {0} bytes.", BytesRecieved);

                    Buffer.BlockCopy(state.buffer, 0, myBuffer, 0, BytesRecieved);

                    //Console.WriteLine("Buffer: " + BitConverter.ToString(myBuffer));

                    if (BytesRecieved > Length)
                    {   // More than one PTEMessage has been received
                        // Alert the class that parsing is needed
                        _extraPTEMessages = true;
                    }
                    receiveDone.Set();
                    _receiveDone = true;
                }
                else
                {
                    receiveDone.Set();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::receiveCallback-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Converts String of Data to bytes of data and begins to send data to the OBC.
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void Send(String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device
                _clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(sendCallback), _clientSocket);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::Send-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
        
        /// <summary>
        /// Sends the byte[] to the OBC
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void Send(Byte[] data)
        {
            try
            {
                // Begin sending the data to the remote device.
                _clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(sendCallback), _clientSocket);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::Send-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Called once the Asynchronous Operation Completes (Data is sent)
        /// </summary>
        /// <param name="ar">Status of Asynchronous Operation</param>
        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent
                sendDone.Set();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SocketManager::sendCallback-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Pings the system.
        /// </summary>
        /// <returns>True is Pinged, otherwise false</returns>
        public bool PingHost()
        {
            Ping NewPing = null;
            bool Pingable = false;

            try
            {
                Console.WriteLine("Pinging Host");
                NewPing = new Ping();
                PingReply reply = NewPing.Send(ipAddressString);
                Pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException e)
            {
                Console.WriteLine("PingException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            finally
            {
                if (NewPing != null)
                {
                    NewPing.Dispose();
                }
            }
            return Pingable;
        }

        /// <summary>
        /// This method calls the IPAddress.Parse method to check the ipAddress 
        /// input string. If the ipAddress argument represents a syntatically correct IPv4 or
        /// IPv6 address, the method displays the Parse output into quad-notation or
        /// colon-hexadecimal notation, respectively. Otherwise, it displays an 
        /// error message.
        /// </summary>
        /// <param name="ipAddress">IP Address as string</param>
        /// <returns>IP Address as IPAddress</returns>
        private IPAddress parse(string ipAddress)
        {
            try
            {
                // Create an instance of IPAddress for the specified address string (in dotted-quad, or colon-hexadecimal notation)
                IPAddress address = IPAddress.Parse(ipAddress);

                // Display the address in standard notation
                Console.WriteLine("Parsing your input string: " + "\"" + ipAddress + "\"" + " produces this address (shown in its standard notation): " + address.ToString());
                return address;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (FormatException e)
            {
                Console.WriteLine("FormatException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            return null;
        }
    }
}

