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
        private MainWindow AppWindow;
        private Session seActiveSession;
        private List<Session> lSessionList;


        /// <summary>
        /// CONSTRUCTORS
        /// </summary>
        public SessionManager()
        {
            AppWindow = SystemView.MainWindow._appWindow;
            lSessionList = new List<Session>();

            AddStartPage();
        }

        /*
         * ACCESSORS
         */

        public Session ActiveSession
        {
            get
            {
                return seActiveSession;
            }
            set
            {
                seActiveSession = value;
            }
        }

        public List<Session> SessionList
        {
            get
            {
                return lSessionList;
            }
        }


        /*
         * METHODS
         */

        public void AddSession()
        {
            string SessionName;
            
            int StartIndex = 0;
            SessionName = CreateName(StartIndex);           

            Session SessionToAdd = new Session(SessionName);

            lSessionList.Add(SessionToAdd);
            AppWindow.SessionContainer.Items.Add(SessionToAdd.GetSessionContainer);

            ActiveSession = SessionToAdd;
        }

        private void AddStartPage()
        {
            Session SessionToAdd = new Session("Start Page");
            lSessionList.Add(SessionToAdd);
            AppWindow.SessionContainer.Items.Add(SessionToAdd.GetSessionContainer);

            ActiveSession = SessionToAdd;
        }

        public void AddDisplay(DISPLAY_TYPE Type)
        {
            try
            {
                bool bDiag = true;
                
                if (AppWindow._myPTEConnection != null || bDiag)
                {
                    if (SessionList.Count <= 1 && lSessionList.Find(x => x.GetName == "Start Page") != null )
                    {
                        AddSession();
                    }

                    ActiveSession.NewDisplay(Type);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Error: OBC connection not detected. Please establish connection and try again.", "Error", MessageBoxButtons.OKCancel); 
                }           
            }
            catch
            {

            }
            
        }

        private string CreateName(int NameIndex)
        {
            string SessionName = string.Format("Session {0}", NameIndex + 1);

            if (lSessionList.Find(x => x.GetName == SessionName) != null)
            {
                SessionName = CreateName(NameIndex + 1);
            }

            return SessionName;
        }

        public void LoadSession()
        {
            AddSession();

            OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Filter = "XML Files (*.xml)|*.xml";

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                string FDReturn = FileDialog.FileName;
                seActiveSession.GetLayoutPath = FDReturn;
                seActiveSession.LoadSessionLayout();
            }
        }

        public void RemoveSession(RadPane CloseablePane)
        {
            try
            {
                Session SessionToRemove = lSessionList.Find(x => x.GetName == CloseablePane.Header);
                lSessionList.Remove(SessionToRemove);

                SessionToRemove.GetSessionContainer.RemoveFromParent();

                if (lSessionList.Count == 0)
                {
                    AddSession();
                }

            }
            catch
            {

            }

        }

        public void SaveSession()
        {
            try
            {
                if (seActiveSession.GetLayoutPath == null)
                {
                    SaveSessionAs();
                }
                else
                {
                    seActiveSession.SaveSessionLayout();
                }
            }
            catch
            {

            }
        }

        public void SaveSessionAs()
        {
            SaveFileDialog FileDialog = new SaveFileDialog();
            FileDialog.Filter = "XML Files (*.xml)|*.xml";

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                string FDReturn = FileDialog.FileName;
                seActiveSession.GetLayoutPath = FDReturn;
                seActiveSession.SaveSessionLayout();
            }
            Console.WriteLine(seActiveSession.GetLayoutPath);

        }

        public void SetActiveSession(RadPane Pane)
        {
            Session SelectedSession = lSessionList.Find(x => x.GetName == Pane.Header);
            seActiveSession = SelectedSession;
        }

        public void SetActiveSession(Session Session)
        {
            seActiveSession = Session;
        }

        public override string ToString()
        {
            StringBuilder toString = new StringBuilder();
            toString.Append(string.Format("Class SessionManager - Number of Sessions: {0}, Active Session: {1}", lSessionList.Count, seActiveSession.GetName));
            return toString.ToString();
        }
    }

}