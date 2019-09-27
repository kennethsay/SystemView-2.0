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
    
    /// <summary>
    /// 
    /// </summary>
    public class Session
    {
        private MainWindow AppWindow = SystemView.MainWindow._appWindow;

        private string Name;


        private List<DISPLAY_TYPE> _activeDisplaysByType;

        private ObservableCollection<DisplayView> _itemsDisplayed;
        private RadDocking InnerDock;
        private RadPane SessionPane;
        private string LayoutPath;
        public Type ContentType;
        


        /// <summary>
        /// CONSTRUCTORS
        /// </summary>

        public Session(string Name)
        {
            _activeDisplaysByType = new List<DISPLAY_TYPE>();

            this.Name = Name;
            this.LayoutPath = null;

            this.SessionPane = new RadPane();
            
            this._itemsDisplayed = new ObservableCollection<DisplayView>();

            CreateInnerDock();

            if (this.Name != null)
            {
                if (this.Name == "Start Page")
                {
                    FormatStartTab(SessionPane);
                }
                else
                {
                    FormatTab(SessionPane);
                }
            }         

        }

        /// <summary>
        /// ACCESSORS
        /// </summary>
                       
        public Type GetContent()
        {
            return this.ContentType;
        }

        public string GetLayoutPath
        {
            get
            {
                return this.LayoutPath;
            }

            set
            {
                if (this.LayoutPath != value)
                {
                    this.LayoutPath = value;
                }
            }
        }
        public string GetName
        {
            get
            {
                return this.Name;
            }
        }

        public RadPane GetSessionContainer
        {
            get
            {
                return this.SessionPane;
            }
        }



        /// <summary>
        /// METHODS
        /// </summary>
        /// 
      
        private void CreateInnerDock()
        {           
            RadPaneGroup Right = new RadPaneGroup() { Name = "rightGroup"};
            RadPaneGroup Left = new RadPaneGroup() { Name = "leftGroup"};
            RadPaneGroup Bottom = new RadPaneGroup() { Name = "bottomGroup" };
            RadPaneGroup Top = new RadPaneGroup() { Name = "topGroup" };

            RadSplitContainer SplitLeft = new RadSplitContainer { InitialPosition = DockState.DockedLeft, Height = 200, Width = 400};
            RadSplitContainer SplitRight = new RadSplitContainer { InitialPosition = DockState.DockedRight, Height = 200, Width = 400 };
            RadSplitContainer SplitBottom = new RadSplitContainer { InitialPosition = DockState.DockedBottom };
            RadSplitContainer SplitTop = new RadSplitContainer { InitialPosition = DockState.DockedTop, Height = 400};

            SplitRight.Items.Add(Right);
            SplitLeft.Items.Add(Left);
            SplitBottom.Items.Add(Bottom);
            SplitTop.Items.Add(Top);

            InnerDock = new RadDocking()
            {
                DockingPanesFactory = new CustomDockingPanesFactory(),
                
                CloseButtonPosition = Telerik.Windows.Controls.Docking.CloseButtonPosition.InPane,
                PanesSource = _itemsDisplayed,                
            };
            InnerDock.Items.Add(SplitRight);
            InnerDock.Items.Add(SplitLeft);
            InnerDock.Items.Add(SplitBottom);
            InnerDock.Items.Add(SplitTop);
                        
            SessionPane.Content = InnerDock;
        }
        private void FormatTab(RadPane Pane)
        {
            Pane.Header = this.Name;
            Pane.CanUserClose = true;

            Pane.Content = InnerDock;
            

        }
        private void FormatStartTab(RadPane Pane)
        {
            Pane.Header = "Start Page";
            Pane.CanUserClose = true;

            Pane.Content = AppWindow.MainDock.FindResource("StartPageContent");
            Pane.IsActive = true;
        }
        public void NewDisplay(DISPLAY_TYPE Type)
        {
            try
            {
                if (!_activeDisplaysByType.Contains(Type))
                {
                    _activeDisplaysByType.Add(Type);

                    switch (Type)
                    {
                        case DISPLAY_TYPE.BATLEVEL:
                            this._itemsDisplayed.Add(new DisplayView(typeof(BattLvlDisplay)) { Header = "Battery Level", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.BATLEVEL });
                            break;

                        case DISPLAY_TYPE.COMM_TEST:
                            this._itemsDisplayed.Add(new DisplayView(typeof(CommTestDisplay)) { Header = "Communication Test", InitialPosition = DockState.DockedTop, DisplayType = DISPLAY_TYPE.BATLEVEL });
                            break;

                        case DISPLAY_TYPE.CONFIG:
                            this._itemsDisplayed.Add(new DisplayView(typeof(ConfigDisplay)) { Header = "Vehicle Configuration", InitialPosition = DockState.DockedTop });
                            break;

                        case DISPLAY_TYPE.DATAPRESENT:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DataPresentation)) { Header = "Data Presentation Window", InitialPosition = DockState.DockedTop });
                            break;

                        case DISPLAY_TYPE.DOWNLOAD:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DatalogDownload)) { Header = "Download OBC Log", InitialPosition = DockState.DockedTop });
                            break;

                        case DISPLAY_TYPE.PLAYBACK:
                            this._itemsDisplayed.Add(new DisplayView(typeof(DataPlaybackPresentation)) { Header = "Playback Download", InitialPosition = DockState.DockedTop });
                            break;

                        case DISPLAY_TYPE.RADIOTP:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RadioData)) { Header = "Radio Data", InitialPosition = DockState.DockedBottom });
                            break;

                        case DISPLAY_TYPE.REVISION:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RevisionDisplay)) { Header = "Revision", InitialPosition = DockState.DockedTop });
                            break;

                        case DISPLAY_TYPE.REALTIMECLOCK:
                            this._itemsDisplayed.Add(new DisplayView(typeof(RTCDisplay)) { Header = "Sync Real Time Clock", InitialPosition = DockState.DockedTop });
                            break;

                        default:
                            break;
                    }
                }
                else
                {
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

        public void OnClose(StateChangeEventArgs args)
        {
            var allPanes = args.Panes.ToList();

            foreach (RadPane rp in allPanes)
            {
                rp.DataContext = null;
                rp.Content = null;
                rp.ClearValue(RadDocking.SerializationTagProperty);
                rp.RemoveFromParent();
            }
        }

        public void SaveSessionLayout()
        {

            using (FileStream fs = File.Open(this.LayoutPath, FileMode.Create))
            {
                this.InnerDock.SaveLayout(fs);
            }
        }

        public void LoadSessionLayout()
        {
            using (FileStream fs = File.Open(this.LayoutPath, FileMode.Open))
            {
                this.InnerDock.LoadLayout(fs);
            }
        }

        private void RemovePane(object sender, Telerik.Windows.Controls.Docking.StateChangeEventArgs e)
        {
            RadPane Pane = e.Panes.First();

            //_mySessionMgr.RemoveSession(Pane);

            Pane.DataContext = null;
            Pane.Content = null;
            Pane.ClearValue(RadDocking.SerializationTagProperty);
            Pane.RemoveFromParent();
        }
        public override string ToString()
        {
            StringBuilder toString = new StringBuilder();
            toString.Append(string.Format("Class Session - Name: {0}", this.Name));
            return toString.ToString();

        }
    }
}

