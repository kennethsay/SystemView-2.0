using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Forms;



namespace SystemView
{

    // To call SessionManager = new SessionManager();
    //
    // CLASS: SessionManager
    //
    // Description: This class acts as the organizer and structure container for Session objects. 
    //
    // Private Data:
    //      MainWindow _appWindow           - Reference to MainWindow component of the application
    //      Session _activeSession          - Variable to hold the instance of the currently selected Session
    //      List<Session>  _sessionList     - List containing all active Sessions created by the user and managed by the Session Manager
    //
    // Public Get/Set Accessors:
    //      Session ActiveSession(get/set)  - Accessor method for _activeSession variable
    //      List<Session> SessionList(get)  - Accessor method for _sessionList List
    //
    // Public Methods:
    //      AddSession()                    - Add a new Session to the Session Manager
    //      AddDisplay(DISPLAY_TYPE Type)   - Add a new Display of specified Type to the Active Session
    //      LoadSession()                   - Loads a Session selected by the user by opening a FileDialog and allowing the user to choose an XML file
    //      RemoveSession()                 - Deletes the specified Session from the Session Manager. Session is selected by specifying a Pane associated with a particular Session
    //      SaveSession()                   - Saves the active Session as an XML file to the current selected save path
    //      SaveSessionAs()                 - Saves the active Session as an XML file. Propmts the user for a save location through a File Dialog 
    //      SetActiveSession(RadPane Pane)  - Searches the SessionList for a Session containing the Pane instance specified and sets the active Session to that Session instance
    //      SetActiveSession(Session Session) - Searches the SessionList for the Session instance specified and sets the active Session to that Session instance
    //      string ToString()               - ToString method override for SessionManager class
    //
    // Private Methods:
    //      addStartPage()                  - Create a new Start Page Session that contains a Pane displaying the revision history of SystemView. Adds this page to SessionList
    //      string createName()             - Creates a Session name using the specified index. Name takes the form "Session X" where X is the specified index.
    // Constructors:
    //      SessionManager()
    //
    // Other DataTypes:
    //      enum DISPLAY_TYPE               - Enumerated type representing different types of SystemView Displays
    //

    public enum DISPLAY_TYPE
    {
        BATLEVEL,
        COMM_TEST,
        CONFIG,
        DATAPRESENT,
        DOWNLOAD,
        DISPLAYFILTER,
        PLAYBACK,
        RADIOTP,        
        REALTIMECLOCK,
        REALTIMEDATA,
        REVISION,       
        TRIGGERFILTER, 
        NONE
    };
    public class SessionManager
    {
        #region PrivateMembers
        private MainWindow _appWindow;
        private Session _activeSession;
        private List<Session> _sessionList;
        #endregion

        #region Accessors
        public Session ActiveSession
        {
            get
            {
                return _activeSession;
            }
            set
            {
                _activeSession = value;
            }
        }
        public List<Session> SessionList
        {
            get
            {
                return _sessionList;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// SessionManager constructor method
        /// </summary>
        public SessionManager()
        {
            try
            {
                // Constructor - set MainWindow instance and create a new Session List instance
                _appWindow = SystemView.MainWindow._appWindow;
                _sessionList = new List<Session>();

                // Add the startup page that displays the application revision history
                addStartPage();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::Constructor-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
        #endregion        

        #region PublicMethods


        /// <summary>
        /// Adds a new "blank" Session to the Session Manager list of Sessions
        /// </summary>
        public void AddSession()
        {

            try
            {
                string SessionName;

                int StartIndex = 0;

                // Create a new name for the Session to be displayed in the Session Tab
                // Name takes the form of "Session X" where X is the number of Sessions in the Session list when the Session is added
                SessionName = createName(StartIndex);

                // Create a new Session instance from the Session Name generated above
                Session SessionToAdd = new Session(SessionName);

                // Add this new Session to the Session List and display it to the Session Container Docking Panel
                SessionList.Add(SessionToAdd);
                _appWindow.SessionContainer.Items.Add(SessionToAdd.GetSessionContainer);

                // Set the active Session to be this new Session
                ActiveSession = SessionToAdd;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::AddSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Adds a new display of the specified type to the currently active Session
        /// </summary>
        /// <param name="Type">Type of display to be added. See DISPLAY_TYPE enumerated type</param>
        public void AddDisplay(DISPLAY_TYPE Type)
        {
            try
            {
                bool bDiag = true;

                // Before we do anything, check to see if there is an active connection!
                if (_appWindow._myPTEConnection != null || bDiag)
                {
                    // If a connection is established then check to see if a Session already exists. 
                    if (SessionList.Count <= 1 && SessionList.Find(x => x.Name == "Start Page") != null)
                    {
                        // This is a user friendly function. This allows the user to click any of the AddDisplay buttons without 
                        // having to first open a Session. You're welcome. 

                        // First add a new Session if we don't have any active
                        AddSession();
                    }

                    // Add the new display to the active Session
                    ActiveSession.NewDisplay(Type);
                }
                else
                {
                    // If we do not have a connection then display an error to the user
                    System.Windows.Forms.MessageBox.Show("Error: OBC connection not detected. Please establish connection and try again.", 
                                                         "Error", 
                                                          MessageBoxButtons.OKCancel);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::AddDisplay-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
        
        /// <summary>
        /// Loads a Session selected by the user by opening a FileDialog and allowing the user to choose an XML file
        /// </summary>
        /// 
        public void LoadSession()
        {
            try
            {
                // We want to always open the saved Session within a new Session window, so create one now
                AddSession();

                // Display a File Dialog and allow the user to select the saved Session (XML file)
                OpenFileDialog FileDialog = new OpenFileDialog();
                FileDialog.Filter = "XML Files (*.xml)|*.xml";

                // Verify the dialog result and load the saved Session
                if (FileDialog.ShowDialog() == DialogResult.OK)
                {
                    string FDReturn = FileDialog.FileName;
                    ActiveSession.GetLayoutPath = FDReturn;
                    ActiveSession.LoadSessionLayout();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::LoadSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Deletes the specified Session from the Session Manager. Session is selected by specifying a Pane associated with a particular Session
        /// </summary>
        /// <param name="CloseablePane">Pane contained within the Session to be closed</param>
        public void RemoveSession(RadPane CloseablePane)
        {
            try
            {
                // Iterate through each Session and find the Session containing the specified Pane
                Session SessionToRemove = SessionList.Find(x => x.Name == CloseablePane.Header);
                SessionList.Remove(SessionToRemove);

                // Remove the Session that was identified by the above code
                SessionToRemove.GetSessionContainer.RemoveFromParent();

                // If we just removed the last Session then go ahead and add a new Session. They are precious to us
                // so we must always have at least 1 Session
                if (SessionList.Count == 0)
                {
                    AddSession();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::RemoveSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Saves the active Session as an XML file to the current selected save path
        /// </summary>
        public void SaveSession()
        {
            try
            {
                // Check to see if we have a current path for the save. 
                if (ActiveSession.GetLayoutPath == null)
                {
                    // If not then we need the user to specify the path. 
                    SaveSessionAs();
                }
                else
                {
                    // If so then just update the save file. 
                    ActiveSession.SaveSessionLayout();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::SaveSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Saves the active Session as an XML file. Propmts the user for a save location through a File Dialog 
        /// </summary>
        public void SaveSessionAs()
        {
            try
            {
                // Allow the user select the save location by providing a File Dialog
                SaveFileDialog FileDialog = new SaveFileDialog();
                FileDialog.Filter = "XML Files (*.xml)|*.xml";

                // Now see if the save path is 'ight 
                if (FileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Yep so go ahead and save the file 
                    string FDReturn = FileDialog.FileName;
                    ActiveSession.GetLayoutPath = FDReturn;
                    ActiveSession.SaveSessionLayout();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::SaveSessionAs-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Searches the SessionList for a Session containing the Pane instance specified and sets the active Session to that Session instance
        /// </summary>
        /// <param name="Pane">Pane to be used in the search for a Session</param>
        public void SetActiveSession(RadPane Pane)
        {
            try
            {
                // Find the Session containing the specified Pane 
                Session NewActiveSession = SessionList.Find(x => x.Name == Pane.Header);

                // Check to see if we were able to find the Session. If so, then make it the new active Session
                if (NewActiveSession != null)
                {
                    ActiveSession = NewActiveSession;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::SetActiveSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Searches the SessionList for the Session instance specified and sets the active Session to that Session instance
        /// </summary>
        /// <param name="Session"> Session to be used as the new active Session</param>
        public void SetActiveSession(Session Session)
        {
            try
            {
                // Make sure the Session instance is not null
                if (Session != null)
                {
                    // Set the new active Session
                    ActiveSession = Session;
                }                
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::SetActiveSession-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// ToString method override for SessionManager class
        /// </summary>
        /// <returns>String for debug containing Number of Sessions and Active Session Instance</returns>
        public override string ToString()
        {
            StringBuilder toString = new StringBuilder();
            toString.Append(string.Format("Class SessionManager - Number of Sessions: {0}, Active Session: {1}", SessionList.Count, ActiveSession.Name));
            return toString.ToString();           
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Create a new Start Page Session that contains a Pane displaying the revision history of SystemView. Adds this page to SessionList
        /// </summary>
        private void addStartPage()
        {
            try
            {
                // Create a new Session to hold the Start Page items. The formate of the Start page is specified as a resource in the MainWindow.XAML file
                Session SessionToAdd = new Session("Start Page");
                SessionList.Add(SessionToAdd);
                _appWindow.SessionContainer.Items.Add(SessionToAdd.GetSessionContainer);

                // Make the new Start Page the active Session
                ActiveSession = SessionToAdd;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::addStartPage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
                
        /// <summary>
        /// Creates a Session name using the specified index. Name takes the form "Session X" where X is the specified index.
        /// </summary>
        /// <param name="NameIndex">The index to be used to name the Session</param>
        /// <returns>The new Session name as a string</returns>
        private string createName(int NameIndex)
        {
            try
            {
                /*  
                 *  This method is used to generate the name for a new Session. The Session name takes the form of "Session X" where X is the specified index. 
                 *  The application generates Sessions with Names starting at Session 1 and going through Session X where X is the number of Sessions. If there are 
                 *  X Sessions and a new Session is created, then the Session Name generated is "Session X + 1"
                 *  
                 *  If a Session, say Session # X - 3, is removed by the user, then the next Session Name to be generated is "Session X - 3". This is to prevent our list
                 *  of Sessions from being structured as: Session 1, Session 2, Session 4, Session 5, etc...
                 */

                // Create the Session name using the specified index
                string SessionName = string.Format("Session {0}", NameIndex + 1);

                // Now check to see if this Session name already exists in the SessionList
                if (SessionList.Find(x => x.Name == SessionName) != null)
                {
                    // If so, then add 1 to the index and create the new name
                    SessionName = createName(NameIndex + 1);
                }

                return SessionName;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SessionManager::createName-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
        #endregion
    }
}