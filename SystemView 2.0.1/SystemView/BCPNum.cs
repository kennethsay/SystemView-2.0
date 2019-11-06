using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppLogic;
using SystemView.ContentDisplays;

namespace SystemView
{
    class BCPNum
    {
        const int RADIO_LENGTH = 39;        // size of the radio message in bytes
        private int num;

        public BCPNum()
        {
            num = 0;
        }

        /// <summary>
        /// Computes the BCP number to be displayed for a particular data record
        /// This is determined by the radio and transponder messages
        /// </summary>
        public string getBCPNum(TagList record)
        {
            byte[] TPmsg = record.Tags.Find(X => X.TagID == 90).Data();
            byte[] TPDirData = record.Tags.Find(X => X.TagID == 29).Data();
            byte TPDir = TPDirData[0];

            if (TPDir == 255)
            {
                TPDir = 0;
            }

            byte[] RDmsg = record.Tags.Find(X => X.TagID == 91).Data();
            byte rDir = (byte)DecodeBits(RDmsg, 52, 1);
            UInt16 rdBCP = (UInt16)DecodeBits(RDmsg, 104, 16);

            if (RDmsg[7] == 0x40)
            {
                if (TPDir != rDir)
                {
                    num = ATCS_BCD_To_Binary(rdBCP);
                }
            }

            else if (RDmsg[7] == 0xC0)
            {
                if (TPDir != rDir)
                {
                    num = ATCS_BCD_To_Binary(rdBCP);
                }
            }

            else if (RDmsg[7] == 0x30 & (RDmsg[RADIO_LENGTH - 2] == 0))
            {
                if (TPDir != rDir)
                {
                    num = ATCS_BCD_To_Binary(rdBCP);
                }
            }

            else if (TPmsg[0] != 0)
            {
                int pkgBase = 60;
                int TP_PKG = TP_PKG = DecodeBits(TPmsg, pkgBase, 4);

                while (TP_PKG != 0)
                {
                    byte pkgDir = (byte)DecodeBits(TPmsg, pkgBase + 4, 1);
                    int size = DecodeBits(TPmsg, pkgBase + 5, 3);

                    if (TP_PKG == 6)
                    {
                        if (size == 2)
                        {
                            if (TPDir != pkgDir)
                            {
                                byte TP_MPy = (byte)DecodeBits(TPmsg, pkgBase + 13, 6);
                                byte TP_MPx = (byte)DecodeBits(TPmsg, pkgBase + 19, 8);

                                num = ((TP_MPy - 2) * 250) + ((TP_MPx - 2) * 100);
                            }
                        }
                    }

                    else if (TP_PKG == 7)
                    {
                        if (size == 1)
                        {
                            if (TPDir != pkgDir)
                            {
                                num = DecodeBits(TPmsg, pkgBase + 11, 12);
                            }
                        }

                        else if (size == 3)
                        {
                            if (TPDir != pkgDir)
                            {
                                num = DecodeBits(TPmsg, pkgBase + 28, 12);
                            }
                        }

                        else if (size == 4)
                        {
                            if (TPDir != pkgDir)
                            {
                                num = DecodeBits(TPmsg, pkgBase + 34, 12);
                            }
                        }

                        else if (size == 5)
                        {
                            if (TPDir != pkgDir)
                            {
                                num = DecodeBits(TPmsg, pkgBase + 34, 12);
                            }
                        }
                    }

                    else if (TP_PKG == 12)
                    {
                        if (size == 3)
                        {
                            if (TPDir != pkgDir)
                            {
                                num = DecodeBits(TPmsg, pkgBase + 27, 12);
                            }
                        }
                    }

                    pkgBase += 16 + (size * 8);
                    TP_PKG = DecodeBits(TPmsg, pkgBase, 4);
                }
            }
            
            if (num != 0)
            {
                DataPlaybackPresentation.PlaybackBCPNum = num;
            }
            
            // print a blank if BCP number is 0
            if (DataPlaybackPresentation.PlaybackBCPNum == 0)
            {
                return " ";
            }
            else
            {
               
                return DataPlaybackPresentation.PlaybackBCPNum.ToString();
            }
        }

        /// <summary>
        /// Converts a particular set of bits within an array of bytes into an integer value
        /// </summary>
        private int DecodeBits(byte[] bytes, int bitStart, int numBits)
        {
            BitArray allBits = new BitArray(bytes.Length * 8);
            BitArray theseBits = new BitArray(numBits);
            int b = 0;

            for (int y = 0; y < bytes.Length; y++)
            {
                for (int i = 7; i >= 0; i--)
                {
                    int bit = (((bytes[y]) >> i) & 0x01);
                    if (bit == 1)
                    {
                        allBits[b] = true;
                    }
                    else
                    {
                        allBits[b] = false;
                    }

                    b++;
                }
            }

            for (int i = 0; i < numBits; i++)
            {
                theseBits[i] = allBits[i + bitStart];
            }

            return getIntFromBitArray(theseBits);
        }

        /// <summary>
        /// Converts an array of bits to an integer
        /// </summary>
        private UInt16 getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            UInt16 num = 0;
            double bit;
            int ind = 0;

            for (int i = bitArray.Length - 1; i >= 0; i--)
            {
                if (bitArray[ind])
                {
                    bit = 1;
                }

                else
                {
                    bit = 0;
                }

                num += (UInt16)(bit * Math.Pow(2, i));
                ind++;
            }

            return num;
        }

        /// <summary>
        /// Converts a BCD-encoded pair of bytes to binary
        /// </summary>
        int ATCS_BCD_To_Binary(UInt16 bcd_val)
        {
            UInt16 bin_val;

            if ((bcd_val & 0xA000) == 0xA000)       // Substitute 0 for A in BCD representation!
            {
                bcd_val = (UInt16)(bcd_val & 0x0FFF);
            }
            if ((bcd_val & 0x0A00) == 0x0A00)
            {
                bcd_val = (UInt16)(bcd_val & 0xF0FF);
            }
            if ((bcd_val & 0x00A0) == 0x00A0)
            {
                bcd_val = (UInt16)(bcd_val & 0xFF0F);
            }
            if ((bcd_val & 0x000A) == 0x000A)
            {
                bcd_val = (UInt16)(bcd_val & 0xFFF0);
            }

            bin_val = (UInt16)(((bcd_val & 0xF000) >> 12) * 1000);
            bin_val += (UInt16)(((bcd_val & 0x0F00) >> 8) * 100);
            bin_val += (UInt16)(((bcd_val & 0x0F0) >> 4) * 10);
            bin_val += (UInt16)(bcd_val & 0x000F);

            return bin_val;
        }

    }
}
