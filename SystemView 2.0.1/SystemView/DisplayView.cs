using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using System.ComponentModel;

namespace SystemView
{
    class DisplayView : INotifyPropertyChanged
    {
        private string header;
        private Type contentType;
        private DockState initialPosition;
        private DISPLAY_TYPE displayType;

        public DisplayView()
        {
        }

        public DisplayView(Type contentType)
        {
            this.ContentType = contentType;
        }
        public DISPLAY_TYPE DisplayType
        {
            get
            {
                return this.displayType;
            }
            set
            {
                if (this.displayType != value)
                {
                    this.displayType = value;
                }
            }

        }
        public Type ContentType
        {
            get
            {
                return this.contentType;
            }
            set
            {
                if (this.contentType != value)
                {
                    this.contentType = value;
                    this.OnPropertyChanged("ContentType");
                }
            }
            
        }

        public string Header
        {
            get
            {
                return this.header;
            }
            set
            {
                if (this.header != value)
                {
                    this.header = value;
                    this.OnPropertyChanged("Header");
                }
            }

        }

        public DockState InitialPosition
        {
            get
            {
                return this.initialPosition;
            }
            set
            {
                if (this.initialPosition != value)
                {
                    this.initialPosition = value;
                    this.OnPropertyChanged("InitialPosition");
                }
            }

        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
