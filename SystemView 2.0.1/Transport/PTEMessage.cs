using System;
using System.Collections.Generic;
using System.Text;

namespace Transport
{
    // To call PTEMessage namePTEMessage = new PTEMesaage(PTEConnection.Comm.____);


    // CLASS: PTEMessage
    //
    // Description: This is the base class for all PTE related classes.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Prvate Data:
    //      Byte _nProductID     - OBC Specific Value
    //      int _nLength        - Length of Message
    //      Byte _nCommandID     - ID of the command associated with the message.
    //
    // Public Get/Set Accessors:
    //      Byte[] Data (Get only)
    //      Byte[] CRC (Get only)
    //      Byte CommandID (Get only)
    //      Byte ProductID (Get only)
    //      Byte Length (Get only)
    //
    // Public Methods:
    //
    // Private Methods:
    //      bool validateCRC(Byte[] _tcpBuffer)     - Verfifies the message is not corrupted using checksum values
    //
    // Constructors:
    //      PTEMessage()        - Default Constructor
    //      PTEMessage(Byte[] tcpBuffer)    - Constructor that parses the message (ProductID, CommandID, Length, Data, 2 CRC Bytes)
    //
    // Public Overrides:
    //      string ToString()
    //

    public class PTEMessage
    {
        private Byte _nProductID;
        private int  _nLength;
        private Byte _nCommandID;

        protected Byte[] _crc = new Byte[2];
        protected Byte[] _data = new Byte[1];
        public const Byte END_OF_RECORD = 0xFF;

        #region Accessors
        public Byte[] Data
        {
            get
            {
                return _data;
            }
        }
        public Byte[] CRC
        {
            get
            {
                return _crc;
            }
        }
        public Byte CommandID
        {
            get
            {
                return _nCommandID;
            }
        }
        public int Length
        {
            get
            {
                return _nLength;
            }
        }
        public Byte ProductID
        {
            get
            {
                return _nProductID;
            }
        }
        #endregion

        /// <summary>
        /// Default Constructor Sets ProductID, Length, CommandID all to zero.
        /// </summary>
        public PTEMessage()
        {
            try
            {
                _nProductID = 0;
                _nLength = 0;
                _nCommandID = 0;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PTEMessage::PTEMessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Parses the Message Received to ProductID, CommandID, Length, Data, and 2 CRC Bytes.
        /// </summary>
        /// <param name="_tcpBuffer">Message Received by OBC</param>
        public PTEMessage(Byte[] _tcpBuffer)
        {
            try
            {
                // First parse the message for Product ID, Command ID, Data, and 2 CRC bytes
                if (_tcpBuffer.Length > 4)
                {
                    _nProductID = _tcpBuffer[0];

                    // Check the Product IDs match also, or else there was a message error
                    // Checking these will reduce exception errors for unexpected packet loss
                    if (_nProductID == OBCCommunication.PRODUCT_ID)
                    {
                        _nCommandID = _tcpBuffer[1];

                        _nLength = (_tcpBuffer[2] << 8) + _tcpBuffer[3];

                        _data = new Byte[_nLength];
                        for (int i = 0; i < _nLength - 2; i++)
                        {
                            _data[i] = _tcpBuffer[i + 4];
                        }

                        _crc[0] = _tcpBuffer[_nLength + 4 - 2];
                        _crc[1] = _tcpBuffer[_nLength + 4 - 1];

                        // Now that Product ID matches, and length is not less than 4, check CRCs
                        bool checksumConfirmed = validateCRC(_tcpBuffer);

                        if (checksumConfirmed == false)
                        {
                            _nProductID = _tcpBuffer[0];
                            _nCommandID = _tcpBuffer[1];
                            _nLength = 0;
                        }
                    }
                    else
                    {   // Product IDs don't match
                        _nProductID = 0;
                        _nCommandID = 0;
                        _nLength = 0;
                    }

                }
                else
                {   // Length is less than 4
                    _nProductID = _tcpBuffer[0];
                    _nCommandID = _tcpBuffer[1];
                    _nLength = 0;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PTEMessage::PTEMessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Validates the checksum value to make sure the message is not corrupted.
        /// </summary>
        /// <param name="_tcpBuffer"> The message from the remote device</param>
        /// <returns>True if CRC matches, otherwise false</returns>
        private bool validateCRC(Byte[] _tcpBuffer)
        {
            try
            {
                bool verified = false;

                // Subtract 2 from the length to account for the two end CRC bytes
                byte[] data = new byte[Length + 2];
                Buffer.BlockCopy(_tcpBuffer, 0, data, 0, Length + 2);
                Crc16Ccitt crc = new Crc16Ccitt(INITIAL_CRC_VALUE.Zeros);
                Byte[] crcBytes = crc.ComputeChecksumBytes(data);
                if (crcBytes[1] == CRC[0])
                {
                    if(crcBytes[0] == CRC[1])
                    {
                        verified = true;
                    }
                }

                return verified;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PTEMessage::validateCRC-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return false;
            }
        }

        /// <summary>
        /// Overrides
        /// </summary>
        /// <returns>Displays ProductID, CommandID, Length, Data, & CRC Bytes</returns>
        public override string ToString()
        {
            try
            {
                Console.WriteLine("PTE Message for Product 0x{0:X}, Command 0x{1:X} _nLength {2}", _nProductID, _nCommandID, _nLength);
                for (int i = 0; i < _nLength - 2; i++)
                {
                    Console.Write("{0:X2}", _data[i]);
                }
                Console.WriteLine("");
                Console.WriteLine("CRC: {0:X}{1:X}", _crc[0], _crc[1]);
                return "";
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PTEMessage::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }
}
