using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.DragDrop;
using System.Threading;
using System.ComponentModel;
using AppLogic;
using Transport;
using System.Runtime.InteropServices;
using System.Collections;
using System.Security.Permissions;
using System.Diagnostics;

namespace SystemView
{
    /// <summary>
    /// SystemView logic that initializes the Main Window display of the application. The SystemView application uses an MVVM (Model-View View Model)
    /// approach to the UI design. The SessionManager class acts as the ViewModel of this architecture, with the Model containing data represented by 
    /// the Session Class. The Mainwindow provides a display docking container in which Session contents are displayed through the Session Manager.
    /// The user may control the number of Sessions and the currently active session through the Session Manager. 
    /// The Main Window also handles user input through toolbar buttons and menu items. 
    /// </summary>
    /// 


    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public static SessionManager _mySessionMgr;
        public static MainWindow _appWindow;
        public PTEConnection _myPTEConnection;
        public IOIndicationList _ioIndications;

        public IOIndicationList IOIndications
        {
            get
            {
                return _ioIndications;
            }
            set
            {
                _ioIndications = value;
            }
        }

        private string _connectionStatus;
        public bool _connected;
        public bool _ioEnabled;

        private InNetworkLogin AdminLogin;
        public string ConnectionStatus
        {   get
            {
                return _connectionStatus;
            }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged("ConnectionStatus");
            }               
        }
        

        #region Members for CardReaderState
        private SCardErrorCodes _errorCode;
        private SCardReadState[] _states;
        private BackgroundWorker _worker;
        private SCardSafeFileHandle _context;
        private SmartcardState _thisSmartCardState;

        public SmartcardState CurrentCardState
        {
            get
            {
                return _thisSmartCardState;
            }
            set
            {
                _thisSmartCardState = value;
            }
        }

        //Definitions for the CardStates used by SCardState
        internal enum CardState
        {
            None = 0,
            Ignore = 1,
            Changed = 2,
            Unknown = 4,
            Unavailable = 8,
            Empty = 16,
            Present = 32,
            AttributeMatch = 64,
            Exclusive = 128,
            InUse = 256,
            Mute = 512,
            Unpowered = 1024
        }
        public enum SmartcardState
        {
            None = 0,
            Inserted = 1,
            Ejected = 2,
        };
        //Built-In Structure, Kept all of the variables as the same names
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SCardReadState
        {
            #region Members
            [MarshalAs(UnmanagedType.LPTStr)]
            // The name(s) of the Smart Card Reader(s) being monitored
            private string szReader;
            // Not used for these purposes
            private IntPtr pvUserData;
            // Current State of the Reader(s)
            private CardState dwCurrentState;
            // Changed State of the Reader(s)
            private CardState dwEventState;
            // Number of bytes in the returned ATR (ATR is used by the system to identify the card)
            private uint cbAtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            // ATR of the inserted card, with extra alignment bytes
            private byte[] rgbAtr;
            #endregion

            #region Methods/Accessors
            public byte[] RGBAttribute()
            {
                return this.rgbAtr;
            }
            public string Reader
            {
                get { return this.szReader; }
                set { this.szReader = value; }
            }
            public IntPtr UserData
            {
                get { return this.pvUserData; }
                set { this.pvUserData = value; }
            }
            public CardState CurrentState
            {
                get { return this.dwCurrentState; }
                set { this.dwCurrentState = value; }
            }
            public CardState EventState
            {
                get { return this.dwEventState; }
            }
            #endregion
        }
        // Built in class, defines the scope for which to search for Readers
        public static class SmartCardScope
        {
            public static readonly Int32 User = 0;
            public static readonly Int32 Terminal = 1;
            public static readonly Int32 System = 2;
        }
        #endregion

        #region Built in Functions
        /// <summary>
        /// Establishes the resource manager context (the scope) within which database operations are performed.
        /// </summary>
        /// <param name="scope">Scope of the resource manager context</param>
        /// <param name="reserved1">Must be NULL for this application</param>
        /// <param name="reserved2">Must be NULL for this application</param>
        /// <param name="context">A handle to the established resource manager context</param>
        /// <returns>0 for Success or an Error code; also updates context for further use</returns>
        [DllImport("winscard.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static internal extern uint SCardEstablishContext(Int32 scope, IntPtr reserved1, IntPtr reserved2, ref SCardSafeFileHandle context);
        /// <summary>
        /// Provides the list of available readers within a set of named reader groups, eliminating duplicates.
        /// </summary>
        /// <param name="context">Handle that identifies the resource manager context for the query</param>
        /// <param name="groups">Names of the reader groups defined to the system</param>
        /// <param name="readers">Multi-string that lists the card readers within the supplied reader groups</param>
        /// <param name="size">Length of the readers buffer in characters</param>
        /// <returns>0 for Success or an Error code; updates size on the first call, then readers on the second call</returns>
        [DllImport("winscard.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static internal extern uint SCardListReaders(SCardSafeFileHandle context, string groups, string readers, ref int size);
        /// <summary>
        /// Closes an established resource manager context, freeing any resources allocated under that context.
        /// </summary>
        /// <param name="context">Handle that identifies the resource manager context</param>
        /// <returns>0 for Success or an Error code</returns>
        [DllImport("winscard.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static internal extern uint SCardReleaseContext(IntPtr context);
        /// <summary>
        /// Monitors the list of readers for changed states.
        /// </summary>
        /// <param name="context">A handle that identifies the resource manager context</param>
        /// <param name="timeoutMilliseconds">The maximum amount of time, in milliseconds, to wait for an action</param>
        /// <param name="readerStates">An array of SCardReadState structures that specify the readers to watch</param>
        /// <param name="readerCount">The number of elements in the readerStates array</param>
        /// <returns>0 for Success or and Error code; updates array of SCardReadState for each reader (does NOT update CurrentState)</returns>
        [DllImport("winscard.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static internal extern uint SCardGetStatusChange(SCardSafeFileHandle context, UInt32 timeoutMilliseconds, [In, Out] SCardReadState[] readerStates, Int32 readerCount);
        #endregion

        #region SafeHandle
        /// <summary>
        /// SafeHandle resolves object lifetime issues by assigning and releasing handles without interruption.
        /// Avoids recycle attack, threads aborting unexpectadly, stack overflow, corrupting of data, and security threats.
        /// </summary>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        internal sealed class SCardSafeFileHandle : SafeHandle
        {
            public SCardSafeFileHandle()
                : base(IntPtr.Zero, true)
            {   // Default constuctor returns IntPtr.Zero to handle
            }
            public override bool IsInvalid
            {   // True is handle is invalid, otherwise false
                [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
                get { return (this.handle == IntPtr.Zero); }
            }
            override protected bool ReleaseHandle()
            {   // Garbage collector calls ReleaseHandle after normal finalizers 
                // have been run for objects that were garbage collected at the same time
                // The garbage collector guarantees the resources to invoke this method 
                // and that the method will not be interrupted while it is in progress.
                // True the handle is released, false returns "releaseHandleFailed" Managed Debugging Assistant
                return ((SCardErrorCodes)MainWindow.SCardReleaseContext(handle) == SCardErrorCodes.None);
                // No error codes are geneterated when Context is released; then the handle can be released
            }
        }
        #endregion

        /// <summary>
        /// Main Window Constructor. Builds the Main Window, initializing instance variables and system theme. 
        /// Takes no arguments, Returns nothing. 
        /// </summary>
        public MainWindow()
        {
            try
            {
                // Set the Theme
                VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Light);

                // Initialize the main window component
                InitializeComponent();

                ApplySiemensDefault();

                // Object to provide an external reference to this window 
                _appWindow = this;

                // Set the current connection status display and internal variables to false
                ConnectionStatus = "Disconnected";
                _connected = false;

                // Create SessionManager
                _mySessionMgr = new SessionManager();

                // Create PTE Connection Manager
                _myPTEConnection = new PTEConnection();

                // Create IO Indications
                _ioIndications = new IOIndicationList();
                _ioIndications.Init();

                _ioEnabled = false;
                this.IODisplayDropdown.IsEnabled = false;
                this.IOSelectDropdown.IsEnabled = false;
                this.IOSelectDropdown.Visibility = Visibility.Collapsed;
                this.IODisplayDropdown.Visibility = Visibility.Collapsed;

                this.SelectableItems.DataContext = _ioIndications;
                this.SelectableATCItems.DataContext = _ioIndications;
                this.SelectableAIUItems.DataContext = _ioIndications;
                this.SelectableCabItems.DataContext = _ioIndications;
                this.SelectableCommItems.DataContext = _ioIndications;
                this.SelectableDTItems.DataContext = _ioIndications;
                this.SelectableSSItems.DataContext = _ioIndications;
                this.SelectableOtherItems.DataContext = _ioIndications;
                this.DisplayedItems.DataContext = _ioIndications;

                

                //this.SelectableItems.DataContext = _ioIndications;

                // Set the data context for binding purposes
                this.DataContext = this;

                // Create and start the automatic connection background worker
                StartAutoConnectManager();

                CardReaderState();

                bool DEBUGSECURITYOVERRIDE = true;

                if (DEBUGSECURITYOVERRIDE)
                {
                    EnableAdminFeatures();
                    EnableKeyUserFeatures();
                }
            }
            catch
            {

            }
        }

        #region Functions for CardReaderState

        public void CardReaderState()
        {
            try
            {
                this._context = new SCardSafeFileHandle();
                // Validates handle for use
                this.validContext();

                SCardErrorCodes checkCode = (SCardErrorCodes)SCardEstablishContext(SmartCardScope.System, IntPtr.Zero, IntPtr.Zero, ref this._context);

                // Create a list of the available card readers on the system
                ArrayList cardReaders = this.cardReaderList();
                this._states = new SCardReadState[cardReaders.Count];
                // Creating SCardReadState for each reader to be monitored
                for (int i = 0; i <= (cardReaders.Count - 1); i++)
                {
                    this._states[i].Reader = cardReaders[i].ToString();
                }
                // Begin background worker threads (to monitor card readers) IF card readers exist on system
                if (cardReaders.Count > 0)
                {
                    this._worker = new BackgroundWorker();
                    this._worker.WorkerSupportsCancellation = true;
                    this._worker.DoWork += watchStatusChange;
                    this._worker.RunWorkerCompleted += completeCardReaderBW;
                    this._worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("MainWindow::Constructor method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }            
        }

        /// <summary>
        /// Special methods for establishing connection with the OBC
        /// </summary>
        /// <returns>True if context is valid, otherwise an error code</returns>
        private bool validContext()
        {
            try
            {
                if (!this._context.IsInvalid)
                {
                    return true;
                }
                else
                {
                    return ((SCardErrorCodes)SCardEstablishContext(SmartCardScope.System, IntPtr.Zero, IntPtr.Zero, ref this._context) == SCardErrorCodes.None);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CardReaderState::validContext-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                return false;
            }
        }

        /// <summary>
        /// Background worker's work function. Checks every second for SCardReadState changes.
        /// If a card is inserted, then program calls CertificateLogin class,
        /// If a card is ejected, .....
        /// If the states haven't changed then the worker keeps on working.
        /// </summary>
        /// <param name="sender">The user</param>
        /// <param name="e">Will exit when closed</param>
        public void watchStatusChange(object sender, DoWorkEventArgs e)
        {
            try
            {
                Thread.Sleep(5000);
                while (!e.Cancel)
                {
                    SCardErrorCodes threadResult;

                    // Lock context pointer once one is obtained, aka our handle
                    lock (this)
                    {
                        // Not valid context then try again...
                        if (!this.validContext())
                        {
                            return;
                        }
                        
                        // This thread will execute every 1000 ms
                        threadResult = (SCardErrorCodes)SCardGetStatusChange(this._context, 1000, this._states, this._states.Length);
                        if (threadResult == SCardErrorCodes.Timeout)
                        {   // Time out has passed, but info is the same, continue on with the loop
                            continue;
                        }
                        // Checks to see if State has changed? Card Inserted or Ejected
                        for (int i = 0; i < this._states.Length; i++)
                        {
                            if (CurrentCardState == SmartcardState.Ejected | CurrentCardState == SmartcardState.None)
                            {
                                if (((this._states[i].EventState & CardState.Present) == CardState.Present)
                                    && ((this._states[i].CurrentState & CardState.Present) != CardState.Present))
                                {   // Card was just Inserted, prior state was not inserted
                                    CurrentCardState = SmartcardState.Inserted;
                                    if (ContentDisplays.EmployeeLogin.UserAuthenticated == false)
                                    {
                                        e.Cancel = true;
                                    }
                                }
                            }
                            else if (CurrentCardState == SmartcardState.None | CurrentCardState == SmartcardState.Inserted)
                            {
                                if (((this._states[i].EventState & CardState.Empty) == CardState.Empty)
                                    && ((this._states[i].CurrentState & CardState.Present) == CardState.Present))
                                {   // Card was Ejected, prior state was inserted
                                    if (ContentDisplays.EmployeeLogin.UserAuthenticated == true)
                                    {
                                        CurrentCardState = SmartcardState.Ejected;
                                        Thread.Sleep(200);
                                        MessageBox.Show("Extra privileges are now disabled.");
                                        ContentDisplays.EmployeeLogin.UserAuthenticated = false;
                                    }
                                }
                            }
                            else
                            {
                                CurrentCardState = SmartcardState.None;
                            }
                            // Update CurrentState, since SCardGetStatusChange does not do so
                            this._states[i].CurrentState = this._states[i].EventState;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CardReaderState::watchStatusChange-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// First gets the size of the list from bufferSize(), then gets the list of
        /// available readers (as a multi-string), then creates and Array List of the readers.
        /// </summary>
        /// <returns>An ArrayList of the available card readers on the system</returns>
        private ArrayList cardReaderList()
        {
            try
            {
                ArrayList readerList = new ArrayList();
                if (this.validContext())
                {
                    int buffSize = bufferSize();
                    // String to hold the Smart Card Reader Split String
                    string cardReaders = new string(' ', buffSize);

                    // Updates list of Smart Card Readers
                    this._errorCode = (SCardErrorCodes)SCardListReaders(this._context, null, cardReaders, ref buffSize);
                    // Make sure there aren't any errors
                    
                    string[] stringSeparators = new string[] { "\0" };
                    string[] eachReader = cardReaders.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    readerList = new ArrayList(eachReader);
                }
                return readerList;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CardReaderState::cardReaderList-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// This is the first call to SCardListReaders, which updates the size of the string,
        /// and returns the size to cardReaderList(). This is cardReaderList() helper function.
        /// </summary>
        /// <returns>Size of the string of readers</returns>
        private int bufferSize()
        {
            try
            {
                int result = 0;
                if (this._context.IsInvalid)
                {
                    return result;
                }
                else
                {
                    // Update error code, if any, and result will update with the "out" parameter
                    this._errorCode = (SCardErrorCodes)SCardListReaders(this._context, null, null, ref result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CardReaderState::watchStatusChange-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

        private void completeCardReaderBW(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CurrentCardState == SmartcardState.Inserted)
            {
                // Connect the employee
                ContentDisplays.EmployeeLogin thisLogin = new ContentDisplays.EmployeeLogin();
                thisLogin.Show();
                thisLogin.Topmost = true;
            }
        }

        #endregion

        #region AutoConnect Methods

        /// <summary>
        /// Begins autoconnect background worker to attempt to establish communication with an OBC
        /// </summary>
        public void StartAutoConnectManager()
        {
            BackgroundWorker _autoConnectWorker = new BackgroundWorker();
            _autoConnectWorker.WorkerSupportsCancellation = true;
            _autoConnectWorker.DoWork += AutoConnectMethod;
            _autoConnectWorker.RunWorkerCompleted += autoConnectWorkerCompletedMethod;
            _autoConnectWorker.RunWorkerAsync();
        }
        public void AttemptAutoConnect()
        {
            try
            {

                ConnectionStatus = "Detecting Connected Devices...";
                

                // Attempt to detect connection

                if (_myPTEConnection.Detect())
                {
                    // Connect to the detected device. Update the user with this information
                    ConnectionStatus = "Device Detected. Attempting Connection";

                    _myPTEConnection.NewConnection();
                    
                    Thread.Sleep(1000);

                    if (_myPTEConnection.IsConnected())
                    {
                        // Connection was accepted, so update the Window status to reflect this. 
                        ConnectionStatus = "Connected";
                        _connected = true;
                    }
                    else
                    {
                        // Connection was not accepted. Update the Window with this status. 
                        ConnectionStatus = "Unable to Connect";
                        _connected = false;
                    }
                }
                else
                {
                    // Device was not detected. Indicate this to the user. 
                    ConnectionStatus = "No Device Detected";
                    _connected = false;
                }

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("MainWindow::AttemptAutoConnect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Autoconnection background worker "work" function. Causes the background worker to attempt to connect an attached OBC. 
        /// </summary>

        private void AutoConnectMethod(object sender, DoWorkEventArgs e)
        {
            try
            {

                // Execute while the cancel order is not in effect
                while (!e.Cancel)
                {
                    // Call the automated connection function handle to automatically connect to the OBC
                    AttemptAutoConnect();

                    // If a connection is established then end the background worker task. 
                    if (_connected)
                    {
                        e.Cancel = true;
                    }

                    // Repeat every 1 seconds. 
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("MainWindow::AutoConnectMethod-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Background worker function to update the connection status in case of disconnect/re-connect. 
        /// </summary>
        
        private void UpdateConnectionMethod(object sender, DoWorkEventArgs e)
        {
            try
            {

                // Execute while the cancel order is not in effect
                while (!e.Cancel)
                {
                    // Test the PTE connection by calling the communication test handle. 
                    // This pings the OBC and expects a response
                    TestConnection();

                    // Evaluate whether the OBC connection is still valid. 
                    if (!_connected)
                    {
                        _myPTEConnection.EndConnection();
                        // If th connection has dropped then end the thread. This thread just checks to see if we are still
                        // connected to an OBC. If are not connected then there is no point in continuing to check the connection
                        e.Cancel = true;
                    }

                    // Execute every 1 second
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("MainWindow::UpdateConnectionMethod-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Background worker method that is run when the autoconnect background worker has been completed. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        // This method is run when the autoconnection thread has completed. On successful connection, this method
        // initalizes the update connection thread that tests the OBC connection and updates the connection status. 
        private void autoConnectWorkerCompletedMethod(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {

                // Only run if we have successfully connected to an OBC. 
                if (_connected)
                {
                    // Initialize connection status update background worker. 
                    BackgroundWorker _connectionUpdateWorker = new BackgroundWorker();
                    _connectionUpdateWorker.WorkerSupportsCancellation = true;
                    _connectionUpdateWorker.DoWork += UpdateConnectionMethod;
                    _connectionUpdateWorker.RunWorkerCompleted += connectionUpdateWorkerCompleted;
                    _connectionUpdateWorker.RunWorkerAsync();
                }                
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("MainWindow::autoConnectWorkerCompletedMethod-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Tests the link between the OBC and PTE through an ICMP ping. Updates connection status display with connection success/failure.  
        /// </summary>
        private void TestConnection()
        {
            try
            {
                

                // Attempt to detect an OBC connection, this function checks the computer port for a connected OBC by pinging the OBC. 
                // A successful ping causes the PTE to retain the connected status, whereas a failed ping sets the connection status to disconnected. 
                if (_myPTEConnection.Detect())
                {
                    // If an active connection is detected, set the connection and display status to connected. 
                    // The display status is shown in the horizontal status bar at the bottom of the MainWindow, and the connection status is a variable used
                    // to track the PTE connection status. 
                    ConnectionStatus = "Connected";
                    _connected = true;
                }
                else
                {
                    // If an active connection is not detected, set the connection and display status to disconnected.
                    ConnectionStatus = "Disconnected";
                    _connected = false;                    
                }
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("MainWindow::TestConnection-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Method executed when the update connection status background worker has completed. Method initializes the autoconnect background thread to attempt 
        /// an automatic connection while disconnected from the OBC. 
        /// </summary>
        private void connectionUpdateWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                // Only run if the PTE is not currently connected to an OBC. If a connection is established, then attempting to re-connect is pointless. 
                if (!_connected)
                {

                    // Create new instance of the autoconnect thread to continually attempt a reconnected. This method only runs when a disconnect has been detected. 
                    BackgroundWorker _autoConnectWorker = new BackgroundWorker();
                    _autoConnectWorker.WorkerSupportsCancellation = true;
                    _autoConnectWorker.DoWork += AutoConnectMethod;
                    _autoConnectWorker.RunWorkerCompleted += autoConnectWorkerCompletedMethod;
                    _autoConnectWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                // print a message to indicate where the exception occurred
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("MainWindow::connectionUpdateWorkerCompleted-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        #endregion
        /// <summary>
        /// INotifyPropertyChanged handler and Methods
        /// </summary>

        #region Property Changed Manager

        // The property changed event handler is necessary for the implementation of the INotifyPropertyChanged class. This class handles automatically 
        // variable value updates from the model to the view. The OnPropertyChanged function alerts the display manager of a change in a bound variable 
        // value and causes a MainWindow update to reflect the changes. 
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        /// <summary>
        /// Tab Control Methods
        /// </summary>
          
        // The methods in this region implement the button functionality of those features pertaining to the use of session tabs. 
        // Covered here are the controls to add, remove, save and load sessions,  and change the selected session. 
        #region Tab Controls
        public void TabControl_AddSession(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddSession();
        }

        public void TabControl_ChangeSelection(object Sender, RoutedEventArgs e)
        {
            RadPane SelectedPane = SessionContainer.SelectedPane;         

            if (SelectedPane != null)
            {
                _mySessionMgr.SetActiveSession(SelectedPane);
            }
        }
        public void TabControl_OpenSession(object Sender, RoutedEventArgs e)
        {           
            _mySessionMgr.LoadSession();
        }
        public void TabControl_RemoveSession(object Sender, EventArgs e)
        {
            RadDocking Pane = (RadDocking)Sender;
         
            //_mySessionMgr.RemoveSession();
        }

        public void TabControl_SaveSession(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.SaveSession();
        }

        public void TabControl_SaveSessionAs(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.SaveSessionAs();
        }

        public void Connect(object sender, RoutedEventArgs e)
        {
            _myPTEConnection = new PTEConnection();
        }

        private void MainDock_Close(object sender, Telerik.Windows.Controls.Docking.StateChangeEventArgs e)
        {
            RadPane Pane = e.Panes.First();

            _mySessionMgr.RemoveSession(Pane);

            Pane.DataContext = null;
            Pane.Content = null;
            Pane.ClearValue(RadDocking.SerializationTagProperty);
            Pane.RemoveFromParent();
        }

        #endregion

        /// <summary>
        /// Display Control Methods
        /// </summary>

        #region Display Controls

        private void DisplayControl_AddBatLvlDisplay(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.BATLEVEL);
        }
        public void DisplayControl_AddConfigDisplay(object Sender, RoutedEventArgs e)
        {
            CommTest commTest = new CommTest();
            //Config config = new Config();
            
           /* if (!commTest.ConnectionEstablished)
            {
                MessageBox.Show("SystemView must be connected to an OBC to access train configuration settings. Check the connection status and try again.",
                                    "Config Error", MessageBoxButton.OK);
            }*/

            /*else*/ /*if (!config.permSuppVZCheck()) // TODO: FIX COMMTEST ISSUE (ONLY WORKS FIRST TIME AFTER STARTUP
            {
                MessageBox.Show("Permanent Suppression and VZero must be active to configure vehicle. Check operator settings and try again.",
                                    "Config Error", MessageBoxButton.OK);
            }

            else
            {*/
                _mySessionMgr.AddDisplay(DISPLAY_TYPE.CONFIG);
            //} 
        }
        public void DisplayControl_AddCommTestDisplay(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.COMM_TEST);
        }
        public void DisplayControl_AddDatalogDownloadDisplay(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.DOWNLOAD);
        }
        public void DisplayControl_AddPlaybackPresentation(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.PLAYBACK);
        }
        public void DisplayControl_AddDataPresentation(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.DATAPRESENT);
        }
        public void DisplayControl_AddRevision(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.REVISION);
        }
        public void DisplayControl_AddRTCDisplay(object Sender, RoutedEventArgs e)
        {
            _mySessionMgr.AddDisplay(DISPLAY_TYPE.REALTIMECLOCK);
        }

        #endregion

        #region IORibbon
        private void ToggleIOView(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ioEnabled)
                {
                    _ioEnabled = false;
                    this.IODisplayDropdown.IsEnabled = false;
                    this.IOSelectDropdown.IsEnabled = false;
                    this.IOSelectDropdown.Visibility = Visibility.Collapsed;
                    this.IODisplayDropdown.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _ioEnabled = true;
                    this.IODisplayDropdown.IsEnabled = true;
                    this.IOSelectDropdown.IsEnabled = true;
                    this.IOSelectDropdown.Visibility = Visibility.Visible;
                    this.IODisplayDropdown.Visibility = Visibility.Visible;
                    this.IODisplayDropdown.IsExpanded = true;
                    this.IOSelectDropdown.IsExpanded = true;
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Theme Modifications
        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Dark);
            Main_Dock.Background = Brushes.Black;
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Light);
            Main_Dock.Background = Brushes.White;
        }

        private void BlueTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.LoadPreset(VisualStudio2013Palette.ColorVariation.Blue);
            Main_Dock.Background = Brushes.White;
        }

        private void ApplySiemensDefault()
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.Palette.AccentColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentDarkColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentMainColor = (Color)ColorConverter.ConvertFromString("#FF41AAAA");
            VisualStudio2013Palette.Palette.AlternativeColor = (Color)ColorConverter.ConvertFromString("#FFEBF0F5");
            VisualStudio2013Palette.Palette.BasicColor = (Color)ColorConverter.ConvertFromString("#FF879BAA");
            VisualStudio2013Palette.Palette.ComplementaryColor = (Color)ColorConverter.ConvertFromString("#FFBECDD7");
            VisualStudio2013Palette.Palette.DefaultForegroundColor = (Color)ColorConverter.ConvertFromString("#FF3C464B");
            VisualStudio2013Palette.Palette.HeaderColor = (Color)ColorConverter.ConvertFromString("#FF009999");
            VisualStudio2013Palette.Palette.MainColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.MarkerColor = (Color)ColorConverter.ConvertFromString("#FF2E3031");
            VisualStudio2013Palette.Palette.MouseOverColor = (Color)ColorConverter.ConvertFromString("#FFC8F5FF");
            VisualStudio2013Palette.Palette.PrimaryColor = (Color)ColorConverter.ConvertFromString("#FFDFE6ED");
            VisualStudio2013Palette.Palette.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.SemiBasicColor = (Color)ColorConverter.ConvertFromString("#66CCCEDB");
            VisualStudio2013Palette.Palette.StrongColor = (Color)ColorConverter.ConvertFromString("#FF2D373C");
            VisualStudio2013Palette.Palette.ValidationColor = (Color)ColorConverter.ConvertFromString("#FFFF3333");
            VisualStudio2013Palette.Palette.ReadOnlyBackgroundColor = (Color)ColorConverter.ConvertFromString("#FFF5F5F5");
            VisualStudio2013Palette.Palette.ReadOnlyBorderColor = (Color)ColorConverter.ConvertFromString("#FFCCCEDB");
            VisualStudio2013Palette.Palette.DisabledOpacity = 0.2;
            VisualStudio2013Palette.Palette.ReadOnlyOpacity = 0.4;
            Main_Dock.Background = Brushes.White;
        }

        private void SiemensLightTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.Palette.AccentColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentDarkColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentMainColor = (Color)ColorConverter.ConvertFromString("#FF41AAAA");
            VisualStudio2013Palette.Palette.AlternativeColor = (Color)ColorConverter.ConvertFromString("#FFEBF0F5");
            VisualStudio2013Palette.Palette.BasicColor = (Color)ColorConverter.ConvertFromString("#FF879BAA");
            VisualStudio2013Palette.Palette.ComplementaryColor = (Color)ColorConverter.ConvertFromString("#FFBECDD7");
            VisualStudio2013Palette.Palette.DefaultForegroundColor = (Color)ColorConverter.ConvertFromString("#FF3C464B");
            VisualStudio2013Palette.Palette.HeaderColor = (Color)ColorConverter.ConvertFromString("#FF009999");
            VisualStudio2013Palette.Palette.MainColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.MarkerColor = (Color)ColorConverter.ConvertFromString("#FF2E3031");
            VisualStudio2013Palette.Palette.MouseOverColor = (Color)ColorConverter.ConvertFromString("#FFC8F5FF");
            VisualStudio2013Palette.Palette.PrimaryColor = (Color)ColorConverter.ConvertFromString("#FFDFE6ED");
            VisualStudio2013Palette.Palette.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.SemiBasicColor = (Color)ColorConverter.ConvertFromString("#66CCCEDB");
            VisualStudio2013Palette.Palette.StrongColor = (Color)ColorConverter.ConvertFromString("#FF2D373C");
            VisualStudio2013Palette.Palette.ValidationColor = (Color)ColorConverter.ConvertFromString("#FFFF3333");
            VisualStudio2013Palette.Palette.ReadOnlyBackgroundColor = (Color)ColorConverter.ConvertFromString("#FFF5F5F5");
            VisualStudio2013Palette.Palette.ReadOnlyBorderColor = (Color)ColorConverter.ConvertFromString("#FFCCCEDB");
            VisualStudio2013Palette.Palette.DisabledOpacity = 0.2;
            VisualStudio2013Palette.Palette.ReadOnlyOpacity = 0.4;
            Main_Dock.Background = Brushes.White;
        }

        private void SiemensDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.Palette.AccentColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentDarkColor = (Color)ColorConverter.ConvertFromString("#FF00646E");
            VisualStudio2013Palette.Palette.AccentMainColor = (Color)ColorConverter.ConvertFromString("#FF41AAAA");
            VisualStudio2013Palette.Palette.AlternativeColor = (Color)ColorConverter.ConvertFromString("#FF414B50");
            VisualStudio2013Palette.Palette.BasicColor = (Color)ColorConverter.ConvertFromString("#FF555F69");
            VisualStudio2013Palette.Palette.ComplementaryColor = (Color)ColorConverter.ConvertFromString("#FF697882");
            VisualStudio2013Palette.Palette.DefaultForegroundColor = (Color)ColorConverter.ConvertFromString("#FF3C464B");
            VisualStudio2013Palette.Palette.HeaderColor = (Color)ColorConverter.ConvertFromString("#FF009999");
            VisualStudio2013Palette.Palette.MainColor = (Color)ColorConverter.ConvertFromString("#FF2D373C");
            VisualStudio2013Palette.Palette.MarkerColor = (Color)ColorConverter.ConvertFromString("#FFEBF0F5");
            VisualStudio2013Palette.Palette.MouseOverColor = (Color)ColorConverter.ConvertFromString("#FF004B55");
            VisualStudio2013Palette.Palette.PrimaryColor = (Color)ColorConverter.ConvertFromString("#FF3C464B");
            VisualStudio2013Palette.Palette.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.SemiBasicColor = (Color)ColorConverter.ConvertFromString("#66CCCEDB");
            VisualStudio2013Palette.Palette.StrongColor = (Color)ColorConverter.ConvertFromString("#FF879BAA");
            VisualStudio2013Palette.Palette.ValidationColor = (Color)ColorConverter.ConvertFromString("#FFFF3333");
            VisualStudio2013Palette.Palette.ReadOnlyBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF2D373C");
            VisualStudio2013Palette.Palette.ReadOnlyBorderColor = (Color)ColorConverter.ConvertFromString("#FF3C464B");
            VisualStudio2013Palette.Palette.DisabledOpacity = 0.2;
            VisualStudio2013Palette.Palette.ReadOnlyOpacity = 0.4;
            var bc = new BrushConverter();
            Main_Dock.Background = (Brush)bc.ConvertFrom("#FF2D373C");
        }

        private void SiemensRetroTheme_Click(object sender, RoutedEventArgs e)
        {
            this.MergeDictionaries("VisualStudio2013");
            VisualStudio2013Palette.Palette.AccentColor = (Color)ColorConverter.ConvertFromString("#FFFFFF00");
            VisualStudio2013Palette.Palette.AccentDarkColor = (Color)ColorConverter.ConvertFromString("#FF990000");
            VisualStudio2013Palette.Palette.AccentMainColor = (Color)ColorConverter.ConvertFromString("#FFB40000");
            VisualStudio2013Palette.Palette.AlternativeColor = (Color)ColorConverter.ConvertFromString("#FF7D94AA");
            VisualStudio2013Palette.Palette.BasicColor = (Color)ColorConverter.ConvertFromString("#FF7373AB");
            VisualStudio2013Palette.Palette.ComplementaryColor = (Color)ColorConverter.ConvertFromString("#FFDBDDE6");
            VisualStudio2013Palette.Palette.DefaultForegroundColor = (Color)ColorConverter.ConvertFromString("#FF1E1E1E");
            VisualStudio2013Palette.Palette.HeaderColor = (Color)ColorConverter.ConvertFromString("#FF000066");
            VisualStudio2013Palette.Palette.MainColor = (Color)ColorConverter.ConvertFromString("#FF000066");
            VisualStudio2013Palette.Palette.MarkerColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.MouseOverColor = (Color)ColorConverter.ConvertFromString("#33000066");
            VisualStudio2013Palette.Palette.PrimaryColor = (Color)ColorConverter.ConvertFromString("#FF8399B1");
            VisualStudio2013Palette.Palette.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2013Palette.Palette.SemiBasicColor = (Color)ColorConverter.ConvertFromString("#66CCCEDB");
            VisualStudio2013Palette.Palette.StrongColor = (Color)ColorConverter.ConvertFromString("#FFEFE4E4");
            VisualStudio2013Palette.Palette.ValidationColor = (Color)ColorConverter.ConvertFromString("#FFFF3333");
            VisualStudio2013Palette.Palette.ReadOnlyBackgroundColor = (Color)ColorConverter.ConvertFromString("#7159728D");
            VisualStudio2013Palette.Palette.ReadOnlyBorderColor = (Color)ColorConverter.ConvertFromString("#FF59728D");
            VisualStudio2013Palette.Palette.DisabledOpacity = 0.2;
            VisualStudio2013Palette.Palette.ReadOnlyOpacity = 0.4;

            var bc = new BrushConverter();
            Main_Dock.Background = (Brush)bc.ConvertFrom("#FF000066");
        }

        //private void FluentLightTheme_Click(object sender, RoutedEventArgs e)
        //{
        //    this.MergeDictionaries("Fluent");
        //    FluentPalette.LoadPreset(FluentPalette.ColorVariation.Light);
        //    Main_Dock.Background = Brushes.White;
        //}

        //private void FluentDarkTheme_Click(object sender, RoutedEventArgs e)
        //{
        //    this.MergeDictionaries("Fluent");
        //    FluentPalette.LoadPreset(FluentPalette.ColorVariation.Dark);
        //    Main_Dock.Background = Brushes.Black;
        //}


        private void MergeDictionaries(string theme)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Telerik.Windows.Themes." + theme + ";component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Telerik.Windows.Themes." + theme + ";component/Themes/Telerik.Windows.Controls.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Telerik.Windows.Themes." + theme + ";component/Themes/Telerik.Windows.Controls.Input.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Telerik.Windows.Themes." + theme + ";component/Themes/Telerik.Windows.Controls.Navigation.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Telerik.Windows.Themes." + theme + ";component/Themes/Telerik.Windows.Controls.Docking.xaml", UriKind.RelativeOrAbsolute)
            });

        }



        #endregion

        #region Authentication

        public void AuthenticateAdmin(object sender, RoutedEventArgs e)
        {
            try
            {
                this.AdminLogin = new InNetworkLogin();

                this.AdminLogin.Show();

            }
            catch
            {

            }
        }

        public void EnableAdminFeatures()
        {
            this.Config.Visibility = Visibility.Visible;
            this.RTCSync.Visibility = Visibility.Visible;

        }

        public void EnableKeyUserFeatures()
        {
            this.Monitoring.Visibility = Visibility.Visible;
            this.AutoTest.Visibility = Visibility.Visible;
            this.Analytics.Visibility = Visibility.Visible;
            this.Themes.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
