using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using static System.Math;

using AppLogic;




namespace SystemView
{
    class DataExtensions 
    {
        #region Member Variables       

        const int NUM_EXTRA_PARAMS = 73;
        const int RADIO_LENGTH = 39; 
        
        #endregion

       

        #region Constructor
        public DataExtensions()
        {            
        }
        #endregion              
        

        public string fillExtraData(int id, int paramNum, TagList record)
        {
            switch (id)
            {
                case 6:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x20) == 0x20))
                                {
                                    return "A";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        case 1:
                            {
                                byte[] aux1 = record.Tags.Find(X => X.TagID == id).Data();
                                byte rearCab = (byte)(aux1[0] & 0x08);

                                byte[] tach1 = record.Tags.Find(X => X.TagID == 8).Data();
                                byte frontCab = (byte)(tach1[0] & 0x02);

                                if ((rearCab == 0x08 & frontCab == 0x02) | (rearCab != 0x08 & frontCab != 0x02))
                                {
                                    return "N";
                                }

                                else if (frontCab == 0x02)
                                {
                                    return "F";
                                }

                                else if (rearCab == 0x08)
                                {
                                    return "R";
                                }

                                else
                                {
                                    // shouldn't exist
                                    return "error activecab";
                                }

                            }

                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0x80);

                                if (paramByte == 0x80)
                                {
                                    return "B";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 8:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data0 = record.Tags.Find(X => X.TagID == id).Data();
                                byte hw = (byte)(data0[0] & 0x80);

                                byte[] data1 = record.Tags.Find(X => X.TagID == 11).Data();
                                byte phw1 = (byte)(data1[0] & 0x02);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte phw2 = (byte)(data2[0] & 0x02);

                                if (hw == 0x80 | phw1 == 0x02 | phw2 == 0x02)
                                {
                                    return "CI";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        case 1:
                            {
                                byte[] data0 = record.Tags.Find(X => X.TagID == id).Data();
                                byte hw = (byte)(data0[0] & 0x80);

                                byte[] data1 = record.Tags.Find(X => X.TagID == 11).Data();
                                byte phw1 = (byte)(data1[0] & 0x02);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte phw2 = (byte)(data2[0] & 0x02);

                                if (hw == 0x80 | phw1 == 0x02 | phw2 == 0x02)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "CO";
                                }

                            }

                        default:
                            return "error";
                    }

                case 9:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {
                                    return "CI";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {
                                    return " ";
                                }

                                else
                                {
                                    return "CI";
                                }
                            }

                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x04) == 0x04)
                                {
                                    return "PS";
                                }

                                else
                                {
                                    return " ";
                                }
                            }



                        default:
                            return "error";
                    }

                case 10:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0xC0);

                                if (paramByte > 0)
                                {
                                    return "S";
                                }
                                else
                                {
                                    paramByte = (byte)(data[0] & 0x3E);

                                    if (paramByte > 0)
                                    {
                                        return "E";
                                    }
                                    else
                                    {
                                        return " ";
                                    }
                                }
                            }

                        default:
                            return "error";
                    }

                case 11:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data1 = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte1 = (byte)(data1[0] & 0x10);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte paramByte2 = (byte)(data2[0] & 0x10);

                                if (paramByte1 == 0x10 | paramByte2 == 0x10)
                                {
                                    return "S";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 16:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x00)
                                {
                                    return "V";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x00)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 1)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }
                case 18:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte dtStep = data[0];

                                dtStep = (byte)((dtStep) & 0x0F);

                                int stepInt = Convert.ToInt16(dtStep);

                                return stepInt.ToString();
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "R";
                                }
                                else
                                {
                                    return " ";
                                }


                            }
                        default:
                            return "error";
                    }
                case 30:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "W";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return "L";
                                }

                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "O";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)(paramByte & 0x80);

                                if (paramByte == 0x80)
                                {
                                    return "-";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        default:
                            return "error";
                    }

                case 34:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte radioChan = (byte)((data[0] >> 5) & 0xE0);

                                int radiochanInt = Convert.ToInt16(radioChan);

                                return radiochanInt.ToString();
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "C";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte radioStat = (byte)((data[0]) & 0x05);

                                if (radioStat == 0x05)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "E";
                                }
                            }
                        default:
                            return "error";

                    }

                case 38:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "CO";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x40) == 0x40))
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "D";
                                }
                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x40) == 0x40))
                                {
                                    return "M";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "T";
                                }
                            }
                        default:
                            return "error";
                    }

                case 47:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte ptsZone = (byte)(data[0] & 0x0F);

                                if (ptsZone == 0x00)
                                {
                                    return " ";
                                }
                                else if ((ptsZone & 0x08) == 0x08)
                                {
                                    return "IXL";
                                }
                                else if ((ptsZone & 0x02) == 0x02)
                                {
                                    return "PDS";
                                }
                                else if ((ptsZone & 0x04) == 0x04)
                                {
                                    return "DS";
                                }
                                else if ((ptsZone & 0x01) == 0x01)
                                {
                                    return "PTS";
                                }
                                else
                                {
                                    return "?";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "R";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x03);

                                if (paramByte == 0x03)
                                {
                                    return "C";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 58:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)(paramByte & 0x08);

                                if (paramByte != 0x08)
                                {
                                    return "F";
                                }

                                else
                                {
                                    return "R";
                                }

                            }

                        default:
                            return "error";
                    }

                case 65:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 76:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 8:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 77:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 78:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "O";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "IP";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte tunnelAvoid = (data[1]);

                                if ((tunnelAvoid & 0x40) == 0x40)
                                {
                                    return "IG";
                                }
                                else if ((tunnelAvoid & 0x20) == 0x20)
                                {
                                    return "LZ";
                                }
                                else if ((tunnelAvoid & 0x10) == 0x10)
                                {
                                    return "IP";
                                }
                                else if ((tunnelAvoid & 0x04) == 0x04)
                                {
                                    return "DS";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x03);

                                int paramInt = Convert.ToInt16(paramByte);

                                return paramInt.ToString();
                            }
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 8:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "S";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 9:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "S";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 10:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "D";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 84:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "Q";
                                }
                            }
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "VZ";
                                }
                            }
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "-";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 91:
                    switch (paramNum)
                    {
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] >> 1);

                                return paramByte.ToString();
                            }

                        case 1:
                            {


                                return "TODO";
                            }

                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                int RRXerr = 0;

                                if (((data[RADIO_LENGTH - 1] & 0x01) == 0x01))
                                {
                                    switch (data[RADIO_LENGTH - 2])
                                    {
                                        case 0:
                                            RRXerr = 0;
                                            break;

                                        case 11:
                                            {
                                                if (data[7] == 0x90)
                                                {
                                                    RRXerr = 0;
                                                }
                                                else
                                                {
                                                    RRXerr = 1;
                                                }
                                                break;
                                            }
                                        case 3:
                                            RRXerr = 0;
                                            break;

                                        default:
                                            RRXerr = 0;
                                            break;
                                    }
                                }

                                if (RRXerr == 1)
                                    return "E";

                                else if (RRXerr == 0)
                                    return " ";

                                else
                                    return "PTE bug";
                            }

                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                int RRXinf = 0;

                                if (((data[RADIO_LENGTH - 1] & 0x01) == 0x01))
                                {
                                    switch (data[RADIO_LENGTH - 2])
                                    {
                                        case 9:
                                            RRXinf = 0;
                                            break;

                                        case 11:
                                            {
                                                if (data[7] == 0x90)
                                                {
                                                    RRXinf = 2;
                                                }
                                                else
                                                {
                                                    RRXinf = 1;
                                                }
                                                break;
                                            }
                                        case 3:
                                            RRXinf = 2;
                                            break;

                                        default:
                                            RRXinf = 2;
                                            break;
                                    }
                                }

                                if (RRXinf == 0)
                                    return " ";

                                else if (RRXinf == 1)
                                    return "R";

                                else if (RRXinf == 2)
                                    return "I";

                                else
                                    return "PTE bug";
                            }

                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte radioTraffic = data[RADIO_LENGTH - 1];

                                if (radioTraffic == 0x02)
                                {
                                    return "T";
                                }

                                else if ((radioTraffic & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }

                            }

                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte radioTraffic = data[RADIO_LENGTH - 1];

                                if (radioTraffic == 0x02)
                                {
                                    return "??";
                                }

                                else if ((radioTraffic & 0x01) == 0x01)
                                {
                                    return "%02X";
                                }

                                else
                                {
                                    return "??";
                                }

                            }

                        case 6:
                            {
                                int rtXrXenc = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 5 || msgtype == 11 || msgtype == 8 || msgtype == 2 || msgtype == 13)
                                {
                                    rtXrXenc = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXenc == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXenc & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }

                            }

                        case 7:
                            {
                                int rtXrXtsr = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 3 || msgtype == 4 || msgtype == 12)
                                {
                                    rtXrXtsr = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXtsr == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXtsr & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        case 8:
                            {
                                int rtXrXmta = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 9)
                                {
                                    rtXrXmta = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXmta == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXmta & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                default:
                    return "error";
            }
        }
    }
}
