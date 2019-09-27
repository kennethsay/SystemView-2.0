using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemView
{
    class RadioEntry
    {
        public string ID;
        public string ExtraInfo;
        private string _name;
        private string _dataType;
        private int _startIndex;
        private int _endIndex;
        private IDictionary<string, string> _dictList;

        #region Accessors
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        public int StartIndex
        {
            get
            {
                return _startIndex;
            }
            set
            {
                _startIndex = value;
            }
        }

        public int EndIndex
        {
            get
            {
                return _endIndex;
            }
            set
            {
                _endIndex = value;
            }
        }

        public IDictionary<string, string> DictList
        {
            get
            {
                return _dictList;
            }
            set
            {
                _dictList = value;
            }
        }
        #endregion

        public RadioEntry()
        {
            ID = "None";
            _name = "No Name";
            _dataType = "string";
            _startIndex = 0;
            _endIndex = 0;
            ExtraInfo = null;
        }
    }
}
