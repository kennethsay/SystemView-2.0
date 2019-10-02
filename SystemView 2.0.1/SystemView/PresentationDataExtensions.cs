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
                        // ACKNOWLEDGE
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte rearAck = data[0];

                                byte[] frontdata = record.Tags.Find(X => X.TagID == 9).Data();
                                byte frontAck = frontdata[0];


                                if (((rearAck & 0x10) == 0x10) | ((rearAck & 0x20) == 0x20) | ((frontAck & 0x20) == 0x20) | ((frontAck & 0x40) == 0x40))
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACK", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.CAB);
                                    return "A";
                                }

                                else
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACK", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.CAB);
                                    return " ";
                                }
                            }

                        // ACTIVECAB
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

                                if (frontCab == 0x02)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("FRONT CAB ACTIVE", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.CAB);
                                    return "F";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("FRONT CAB ACTIVE", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.CAB);
                                }

                                if (rearCab == 0x08)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("REAR CAB ACTIVE", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.CAB);
                                    return "R";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("REAR CAB ACTIVE", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.CAB);
                                }

                                return " ";

                            }

                        // STOPBYPASS
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0x80);

                                if (paramByte == 0x80)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ASB", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.CAB);
                                    return "B";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ASB", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.CAB);
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 8:
                    switch (paramNum)
                    {
                        // ATC CIO
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
                                    
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC CUTIN VIA ADU", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ATC);
                                    return "CI";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC CUTIN VIA ADU", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ATC);
                                    return " ";
                                }

                            }

                        // ATC CO
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
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC CUTOUT VIA ADU", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ATC);
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC HARDWIRE CUTOUT", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ATC);
                                    return " ";
                                }
                                else
                                {
                                    if (hw == 0x80)
                                    {
                                        SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC HARDWIRE CUTOUT", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ATC);
                                    }
                                    else
                                    {
                                        SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ATC CUTOUT VIA ADU", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ATC);
                                    }                                   
                                    
                                    return "CO";
                                }

                            }

                        default:
                            return "error";
                    }

                case 9:
                    switch (paramNum)
                    {
                        // ACSES CIO
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {                                    
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACSES CUTIN/OPR", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ACSES);
                                    return "CI";
                                }

                                else
                                {                                    
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACSES CUTIN/OPR", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ACSES);
                                    return " ";
                                }
                            }

                        // ACSES CO OUT
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACSES CUTOUT", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ACSES);
                                    return " ";
                                }

                                else
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("ACSES CUTOUT", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ACSES);
                                    return "CO";
                                }
                            }

                        // PERMSUPPR
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x04) == 0x04)
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PERM SUPPR", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.OTHER);
                                    return "PS";
                                }

                                else
                                {
                                    //SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PERM SUPPR", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.OTHER);
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 10:
                    switch (paramNum)
                    {
                        // SPEEDSENSOR
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0xC0);

                                if ((paramByte & 0x80) == 0x80)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("SLD", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.SS);
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("SLD", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.SS);
                                }

                                if ((paramByte & 0x40) == 0x40)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("SLP", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.SS);
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("SLP", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.SS);
                                }

                                if ((paramByte & 0x20) == 0x20)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PE", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.SS);
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PE", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.SS);
                                }

                                if ((paramByte & 0x10) == 0x10)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("TE", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.SS);
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("TE", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.SS);
                                }

                                if ((paramByte & 0x08) == 0x08)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("WE", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.SS);
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("WE", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.SS);
                                }
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
                        // PTSALARMSUP
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
                        // VFBREAK
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

                        // PHABREAK
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

                        // TILTAUTH
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

                case 17:
                    switch (paramNum)
                    {
                        // SERVICE PEN
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // EM PENALTY (LIRR only)
                        case 1:
                            {
                                // if LIRR
                                /*if (railRoad == 1)
                                {
                                    byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                    byte paramByte = data[0];

                                    paramByte = (byte)((paramByte >> 1) & 0x01);

                                    if (paramByte == 0)
                                    {
                                        return "P";
                                    }
                                    else
                                    {
                                        return " ";
                                    }
                                }
                                else
                                {
                                    return " ";
                                }*/

                                return " ";
                            }
                        default:
                            return "error";
                    }
                case 18:
                    switch (paramNum)
                    {
                        // DT STEP
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte dtStep = data[0];

                                dtStep = (byte)((dtStep) & 0x0F);

                                int stepInt = Convert.ToInt16(dtStep);

                                return stepInt.ToString();
                            }

                        // DT SUSPENDED
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

                        // DT SUCCESS
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

                        // DT RUNNING
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
                        // TPFOUND
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

                        // TPINWIN
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

                        // TRAILINGOPR
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("TRAILING", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ACSES);
                                    return "T";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("TRAILING", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ACSES);
                                    return "L";
                                }

                            }

                        // ALARM
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

                        // OVERSPEED
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

                        // ACSES PEN
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

                        // ANTENNAPOWER
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

                        // DASHDASH
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
                        // RADIOCHAN
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte radioChan = (byte)((data[0] >> 5) & 0xE0);

                                int radiochanInt = Convert.ToInt16(radioChan);

                                return radiochanInt.ToString();
                            }

                        // BCPCOVERAGE
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

                        // RADIO STATUS
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
                        // ACSES MVCO
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

                        // TPOUTOFWIN
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

                        // DATABASE OK
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

                        // TPMISSING
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

                        // TSRLISTOK
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
                        // PTSZONE
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

                        // RADIORELEASE
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

                        // C SIGNAL
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
                        // DECEL DIRECT
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
                        // ROLLAWAY PEN
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

                        // GEARTEETH
                        case 1:
                            {
                                return " ";
                                //return ((int)(header[24])).ToString();
                            }

                        default:
                            return "error";
                    }

                case 76:
                    switch (paramNum)
                    {
                        // FAULT AUXIO
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

                        // FAULT TACHIO
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

                        // FAULT CONFIG
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

                        // FAULT ENET
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

                        // FAULT ADU 1
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

                        // FAULT ADU 2
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

                        // FAULT DECEL
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

                        // FAULT ATC
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

                        // FAULT AIU
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
                        // FAULT TPR
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
                        // BRAKE ASR
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

                        // ABS CMB TER
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

                        // ZS TSR IP
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

                        // TUNNEL AVOID
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

                        // TB PTS EN
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

                        // TB PTS ACT
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

                        // SAR TSR STS
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x03);

                                int paramInt = Convert.ToInt16(paramByte);

                                return paramInt.ToString();
                            }

                        // TRS ENFORCED
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

                        // DB SLIP IP
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

                        // DB SLIDE IP
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

                        // TCP/IP
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
                        // ACSESPTSREQ
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PTS (ABS. STOP REQ)", LEDIndicator.LEDState.ON, IOIndicationList.IndicationType.ACSES);
                                    return "P";
                                }
                                else
                                {
                                    SystemView.MainWindow._appWindow._ioIndications.UpdateIOState("PTS (ABS. STOP REQ)", LEDIndicator.LEDState.OFF, IOIndicationList.IndicationType.ACSES);
                                    return " ";
                                }
                            }

                        // ACKREQ
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "Q";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // ACSES CO FLS
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

                        // ADUVZ
                        case 3:
                            {
                                byte[] tach = record.Tags.Find(X => X.TagID == 10).Data();
                                byte tachByte = tach[0];

                                tachByte = (byte)((tachByte) & 0x01);

                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01 | tachByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "VZ";
                                }
                            }

                        // ACSES DASHDASH
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
                        // ATCSEQNUM
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] >> 1);

                                return paramByte.ToString();
                            }

                        // BCPNUM
                        case 1:
                            {


                                return " ";
                                //return BCPNUM(record);

                            }

                        // RADIORXERROR
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

                        // RADIORXINF
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

                        // RADIOTRAFFIC
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

                        // X|TSRTYPE
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

                        // RADIOTXRXENC
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

                        // RADIOTXRXTSR
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

                        // RADIOTXRXMTA
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
