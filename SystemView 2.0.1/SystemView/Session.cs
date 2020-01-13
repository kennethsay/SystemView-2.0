using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;
using Telerik.Windows.Controls;
using AppLogic;
using SystemView.ContentDisplays;
using Telerik.Windows.Controls.Docking;

namespace SystemView
{
    // To call Session = new Session();
    //
    // CLASS: Session
    //
    // Description: This class acts as the organizer and structure container for Display Panes that make up the ordinary PTE functions. 
    //
    // Private Data:
    //      MainWindow AppWindow                                - Local reference for MainWindow Component
    //      string _name                                        - Session Name
    //      List<DISPLAY_TYPE> _activeDisplaysByType            - List of Session Pane displays
    //      ObservableCollection<DisplayView> _itemsDisplayed   - List of the items currently displayed
    //      RadDocking _innerDock                               - RadDocking docking mechanism for Session
    //      RadPane _sessionPane                                - Pane for default Session layout
    //      string _layoutPath                                  - Path to Session layout save/load location
    //      Type _contentType                                   - Type of content for Pane Display
    //
    // Public Get/Set Accessors:
    //      List<DISPLAY_TYPE> ActiveDisplaysByType(get)
    //      string GetLayoutPath(get/set)
    //      string Name(get/set)
    //      RadPane GetSessionContainer(get)
    //      Type GetContent(get)
    //
    // Public Methods:
    //      NewDisplay(DISPLAY_TYPE Type)                       - Create a new Pane content display specified by the Type argument inside the Session Dock
    //      OnClose(StateChangeEventArgs args)                  - Event handler for the Session close event. 
    //      SaveSessionLayout()                                 - Saves the current Session layout by opening a filestream to the current save path.
    //      LoadSessionLayout()                                 - Loads a saved XML Session layout and populates the open Session Docker with the saved layout
    //      string ToString()                                   - Override of Session ToString method
    //          
    // Private Methods:
    //      createInnerDock()                                   - Creates an Dock structure local to the Session instance. Pane objects can be dropped and rearranged onto the Dock.
    //      formatTab(RadPane Pane)                             - Sets the default Pane settings for each Session tab
    //      formatStartTab(RadPane Pane)                        - Sets the default Pane settings for the start page Session tab.
    //      removePane(object sender, Docking.StateChangeEventArgs e) - Event handler that is called when the user clicks the pane close button. 
    //
    // Constructors:
    //      Session(string SessionName)

    public class Session
    {
        #region PrivateMembers
        private MainWindow AppWindow = SystemView.MainWindow._appWindow;
        private string _name;        
        private List<DISPLAY_TYPE> _activeDisplaysByType;
        private ObservableCollection<DisplayView> _itemsDisplayed;
        private RadDocking _innerDock;
        private RadPane _sessionPane;
        private string _layoutPath;
        private Type _contentType;
        #endregion
        
        #region Accessors

        public List<DISPLAY_TYPE> ActiveDisplaysByType
        {
            get
            {
                return _activeDisplaysByType;
            }
        }        
        public string GetLayoutPath
        {
            get
            {
                return this._layoutPath;
            }
            set
            {                
                 this._layoutPath = value;                
            }
        }
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
        public RadPane GetSessionContainer
        {
            get
            {
                return this._sessionPane;
            }
        }
        public Type GetContent
        {
            get
            {
                return this._contentType;
            }            
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Contstructor method for the Session class. 
        /// </summary>
        /// <param name="SessionName">Name for the new Session</param>
        public Session(string SessionName)
        {
            try
            {
                // Create the new active Displays list
                _activeDisplaysByType = new List<DISPLAY_TYPE>();

                // Setup the default Session settings 
                Name = SessionName;
                this._layoutPath = null;
                this._sessionPane = new RadPane();
                this._sessionPane.IsActive = true;

                // Create the displayed items list for the Session. 
                this._itemsDisplayed = new ObservableCollection<DisplayView>();

                // Create the inner Dock for the Session 
                createInnerDock();

                // Check to see if the name was specified. If not then create the
                // start page 
                if (this.Name != null)
                {
                    if (this.Name == "Start Page")
                    {
                        formatStartTab(_sessionPane);
                    }
                    else
                    {
                        formatTab(_sessionPane);
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::Constructor method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }
        #endregion

        #region PublicMethods        

        /// <summary>
        /// Create a new Pane content display specified by the Type argument inside the Session Dock
        /// </summary>
        /// <param name="Type">Type of Display to be created. See enumerated type DISPLAY_TYPE for more information</param>
        public void NewDisplay(DISPLAY_TYPE Type)
        {
            try
            {
                // Check to see if the Session already contains a Pane of the requested type
                if (!_activeDisplaysByType.Contains(Type))
                {
                    // If it does not then add the new Display Type to the list of active Displays
                    _activeDisplaysByType.Add(Type);

                    // Now generate a new Pane of the specified Display type
                    switch (Type)
                    {
                        case DISPLAY_TYPE.BATLEVEL:
                            this._itemsDisplayed.Add(new DisplayView(typeof(BattLvlDisplay)) { Header = "Battery Level", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.BATLEVEL });
                            break;

                        case DISPLAY_TYPE.COMM_TEST:
                            this._itemsDisplayed.Add(new DisplayView(typeof(CommTestDisplay)) { Header = "Communication Test", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.COMM_TEST });
                            break;

                        case DISPLAY_TYPE.CONFIG:
                            this._itemsDisplayed.Add(new DisplayView(typeof(ConfigDisplay)) { Header = "Vehicle Configuration", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.CONFIG });
                            break;

                        case DISPLAY_TYPE.DATAPRESENT:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DataPresentation)) { Header = "Data Presentation Window", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.DATAPRESENT });
                            break;

                        case DISPLAY_TYPE.DOWNLOAD:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DatalogDownload)) { Header = "Download OBC Log", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.DOWNLOAD });
                            break;

                        case DISPLAY_TYPE.PLAYBACK:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DataPlaybackPresentation)) { Header = "Playback Download", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.PLAYBACK });
                            break;

                        case DISPLAY_TYPE.RADIOTP:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RadioData)) { Header = "Radio Data", InitialPosition = DockState.DockedBottom, DisplayType = DISPLAY_TYPE.RADIOTP });
                            break;

                        case DISPLAY_TYPE.REVISION:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RevisionDisplay)) { Header = "Revision", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.REVISION });
                            break;

                        case DISPLAY_TYPE.REALTIMECLOCK:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RTCDisplay)) { Header = "Sync Real Time Clock", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.REALTIMECLOCK });
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    // We already have a Pane of this type, so display an error
                    Console.WriteLine("Display type already found!");
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::NewDisplay method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Event handler for the Session close event. 
        /// </summary>
        /// <param name="args">State change event arguments</param>
        public void OnClose(StateChangeEventArgs args)
        {
            try
            {
                // Get a list of all the Panes in the closing Session
                var allPanes = args.Panes.ToList();

                // Now loop through this list and close each Pane! Make sure to clear the data context and content, 
                // and remove each Pane from the parent
                foreach (RadPane rp in allPanes)
                {
                    rp.DataContext = null;
                    rp.Content = null;
                    rp.ClearValue(RadDocking.SerializationTagProperty);
                    rp.RemoveFromParent();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::OnClose method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Saves the current Session layout by opening a filestream to the current save path. 
        /// Writes the Session layout into the file using the XML format 
        /// </summary>
        public void SaveSessionLayout()
        {
            try
            {
                // Write the Session layout by opening a new XML file and using the RadDocking.SaveLayout method
                using (FileStream fs = File.Open(this._layoutPath, FileMode.Create))
                {
                    this._innerDock.SaveLayout(fs);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::SaveSessionLayout method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Loads a saved XML Session layout and populates the open Session Docker with the saved layout
        /// </summary>
        public void LoadSessionLayout()
        {
            try
            {
                // Open the specified file and read the XML file into the new Docker
                using (FileStream fs = File.Open(this._layoutPath, FileMode.Open))
                {
                    this._innerDock.LoadLayout(fs);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::LoadSessionLayout method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Override of Session ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder toString = new StringBuilder();
                toString.Append(string.Format("Class Session - Name: {0}", this.Name));
                return toString.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::NewDisplay method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                return null;
            }
        }
        #endregion        

        #region PrivateMethods

        /// <summary>
        /// Creates an Dock structure local to the Session instance. Pane objects can be dropped and rearranged onto the Dock. 
        /// </summary>
        private void createInnerDock()
        {
            try
            {
                /*  First setup the RadPanegroups to contain Panes. We create 4 groups that can be rearranged that make up the 
            *  application screen as follows:
            *  -----------------------------------------------------------------
            *  |                          TOP GROUP                            |
            *  |_______________________________________________________________|
            *  |                               |                               |
            *  |                               |                               |
            *  |                               |                               |
            *  |        LEFT GROUP             |         RIGHT GROUP           |
            *  |                               |                               |
            *  |                               |                               |
            *  |_______________________________|_______________________________|
            *  |                         BOTTOM GROUP                          |
            *  -----------------------------------------------------------------
            */

                RadPaneGroup Right = new RadPaneGroup() { Name = "rightGroup" };
                RadPaneGroup Left = new RadPaneGroup() { Name = "leftGroup" };
                RadPaneGroup Bottom = new RadPaneGroup() { Name = "bottomGroup" };
                RadPaneGroup Top = new RadPaneGroup() { Name = "topGroup" };

                // The RadPaneGroups must be contained within RadSplitContainers to divide Panes within the group. Create these now. 
                RadSplitContainer SplitLeft = new RadSplitContainer { InitialPosition = DockState.DockedLeft, Height = 200, Width = 400 };
                RadSplitContainer SplitRight = new RadSplitContainer { InitialPosition = DockState.DockedRight, Height = 200, Width = 400 };
                RadSplitContainer SplitBottom = new RadSplitContainer { InitialPosition = DockState.DockedBottom };
                RadSplitContainer SplitTop = new RadSplitContainer { InitialPosition = DockState.DockedTop, Height = 400 };

                // Now add the groups to the split containers
                SplitRight.Items.Add(Right);
                SplitLeft.Items.Add(Left);
                SplitBottom.Items.Add(Bottom);
                SplitTop.Items.Add(Top);

                // Now for the Docking functionality. Create a new RadDocking instance to be able to Dock within the groups we just created
                _innerDock = new RadDocking()
                {
                    // We must use a custom Docking Panes Factory to control the behavior of the dockable Panes. Tweak some other settings 
                    // here as well
                    DockingPanesFactory = new CustomDockingPanesFactory(),
                    CloseButtonPosition = Telerik.Windows.Controls.Docking.CloseButtonPosition.InPane,
                    PanesSource = _itemsDisplayed,
                };

                // Set the InnerDock.Close method event to the custom removePane event handler. This will make sure that all Panes are removed 
                // from the InnerDock when the instance is closed. 
                _innerDock.Close += removePane;

                // Finally, add each split container to the InnerDock object and set the SessionPane content to the InnerDock instance.
                // This makes the SessionPane object use the InnerDock object to display any content placed within the InnerDock
                _innerDock.Items.Add(SplitRight);
                _innerDock.Items.Add(SplitLeft);
                _innerDock.Items.Add(SplitBottom);
                _innerDock.Items.Add(SplitTop);

                _sessionPane.Content = _innerDock;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::createInnerDock method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Sets the default Pane settings for each Session tab. 
        /// This sets the Header to the Session header, Content to the InnerDock, and sets the CanUserClose parameter to true
        /// </summary>
        /// <param name="Pane">The RadPane used to change settings</param>
        private void formatTab(RadPane Pane)
        {
            try
            {
                // Change the Pane settings
                Pane.Header = this.Name;
                Pane.CanUserClose = true;
                Pane.Content = _innerDock;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::formatTab method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Sets the default Pane settings for the start page Session tab. 
        /// This sets the Header to the Session header, Content to the InnerDock, and sets the CanUserClose parameter to true.
        /// The Pane is also made the Active Pane and is brought to the foreground by the Docking mechanism.
        /// </summary>
        /// <param name="Pane">The RadPane used to change settings</param>
        private void formatStartTab(RadPane Pane)
        {
            try
            {
                // Change the Pane settings
                Pane.Header = "Start Page";
                Pane.CanUserClose = true;
                Pane.Content = AppWindow.MainDock.FindResource("StartPageContent");
                Pane.IsActive = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::formatStartTab method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Event handler that is called when the user clicks the pane close button. 
        /// Causes the pane to be destroyed and removed from the parent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">State change event argument containing the list of currently active Panes within the InnerDock</param>
        private void removePane(object sender, Telerik.Windows.Controls.Docking.StateChangeEventArgs e)
        {
            try
            {
                // The first Pane in the InnerDock Pane list will be the Pane that received the most recent user action
                // This will be the active Pane when the close button is pressed, so retrieve the instance of this Pane
                RadPane Pane = e.Panes.First();

                // Now remove the Pane from the active display list
                _activeDisplaysByType.Remove((Pane.DataContext as DisplayView).DisplayType);

                // Finally, clear the Pane data context and content and remove the Pane from the parent Dock
                Pane.DataContext = null;
                Pane.Content = null;
                Pane.ClearValue(RadDocking.SerializationTagProperty);
                Pane.RemoveFromParent();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Session::removePane method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }

        }
        #endregion        
    }
}

