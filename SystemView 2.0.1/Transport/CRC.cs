using System;
using System.Collections.Generic;
using System.Text;

namespace Transport
{
    // To call Crc16Ccitt nameCRC = new Crc16Ccitt(INITIAL_CRC_VALUE.___);
    // Byte[] crcBytes = nameCRC.ComputeChecksumBytes(data);

    
    // CLASS: CRC
    //
    // Description: This class Creates the Two Bytes of CRC for the end of each OBC Message.
    //
    // Usability: OBC must be connected to computer through an ethernet without any firewall issues.
    // 
    // Private Data:
    //
    // Public Get/Set Accessors:
    //
    // Public Methods:
    //      ushort ComputeChecksum(byte[] bytes, bool bDiag = false)            - Computes the Checksum Bytes as ushort to check the data
    //      byte[] ComputeChecksumBytes(byte[] bytes, bool bDiag = false)       - Turns ushort Checksum Bytes to a Byte[]
    //
    // Private Methods:
    //
    // Constructors:
    //      Crc16Ccitt(INITIAL_CRC_VALUE initialValue)         - Constructor
    //
    // Public Overrides:
    //  

    public enum INITIAL_CRC_VALUE { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

    public class Crc16Ccitt
    {
        // UShort can hold 16 bits (2 bytes) 0-65535, so 1/65535 that two sets of data will have same CRC
        // Bits 12, 5, and 0 is the CCITT CRC 16 Bit Polynomial = 0x1021 = 4129; with even parity
        const ushort poly = 4129;
        // Initializes the table array
        ushort[] table = new ushort[256];
        ushort initialValue = 0;

        /// <summary>
        /// Creates two checksum bytes as ushorts.
        /// </summary>
        /// <param name="bytes">Bytes to Compute the Checksum</param>
        /// <param name="bDiag">ALWAYS FALSE</param>
        /// <returns>ushort Checksum</returns>
        public ushort ComputeChecksum(byte[] bytes, bool bDiag = false)
        {
            try
            {
                ushort crc = this.initialValue;
                try
                {
                    if (bDiag)
                    {
                        Console.WriteLine("CRC Check on:");
                    }
                    for (int i = 0; i < bytes.Length; ++i)
                    {
                        if (bDiag)
                        {
                            Console.Write("0x{0:X2} ", bytes[i]);
                        }
                        // Computation of CRC
                        crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
                    }
                    if (bDiag)
                    {
                        Console.WriteLine("");
                    }
                }
                catch
                {
                    Console.WriteLine("CRC16::ComputeChecksome exception");
                }
                return crc;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CRC::ComputeChecksum-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Converts ushort checksum to two bytes.
        /// </summary>
        /// <param name="bytes">Bytes to Compute the Checksum</param>
        /// <param name="bDiag">ALWAYS FALSE</param>
        /// <returns>2 bytes that represent the checksum</returns>
        public byte[] ComputeChecksumBytes(byte[] bytes, bool bDiag = false)
        {
            try
            {
                ushort crc = ComputeChecksum(bytes, bDiag);
                return BitConverter.GetBytes(crc);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CRC::ComputeChecksumBytes-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Constructor for Crc16Ccitt takes in the Initial CRC Value and generates the table array for each instance.
        /// </summary>
        /// <param name="initialValue">Initial CRC Value</param>
        public Crc16Ccitt(INITIAL_CRC_VALUE initialValue)
        {
            try
            {
                this.initialValue = (ushort)initialValue;
                ushort temp, a;
                for (int i = 0; i < table.Length; ++i)
                {
                    temp = 0;
                    // Move Byte to MSB
                    a = (ushort)(i << 8);
                    for (int j = 0; j < 8; ++j)
                    {
                        // Check for MSB (bit 15)
                        if (((temp ^ a) & 0x8000) != 0)
                        {
                            // Perform binary division to obtain the remainder as 16 bits
                            temp = (ushort)((temp << 1) ^ poly);
                        }
                        else
                        {
                            temp <<= 1;
                        }
                        a <<= 1;
                    }
                    // Holds the remainders in the table array
                    table[i] = temp;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CRC::Crc16Ccitt-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }
        }
    }
}