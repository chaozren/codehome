using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Windows.Threading;

namespace mmc_production
{
    public class SerialComm
    {
        public delegate void EventHandle(byte[] readBuffer);
        public event EventHandle DataReceived;

        public delegate void MessageUpdate(int type, string msg);
        public event MessageUpdate onUpdate;
        //private static UserFunctionCB 

        public SerialPort serialPort;
        Thread thread;
        volatile bool _keepReading;

        private static readonly byte COMMAND_START = 0x21;
        private static readonly byte COMMAND_END = 0x2A;
        private static readonly int COMMAND_LENGTH = 25; //from 0 to 24
        private static readonly int SN_LENGTH = 11;
        private static readonly int HW_LENGTH = 7;
        private static readonly string COMMAND_HELLO = "I AM MMC!";
        private const int COMMAND_TYPE_READ_SYSTEM_ID = 100;
        private const int COMMAND_TYPE_WRITE_NTC = 1;
        private const int COMMAND_TYPE_WRITE_SN = 2;
        private const int COMMAND_TYPE_WRITE_HW = 3;
        private const int COMMAND_TYPE_WRITE_MODEL = 4;
        private const int COMMAND_TYPE_DONE = 5;

        private const int COMMAND_RECEIVE_TIMEOUT = 100; 


        private string hw_ver;
        //public static string sn;
        //public static string mac;

        private int lastCommand;
        private int reSendCount;

        private byte[] readBuffer = null;
        private int mCurLen = 0;
        private DispatcherTimer dispatcherTimer; //time for read command 
        private bool hasStart;

        private DispatcherTimer retryTimer;
        private DispatcherTimer doneWaitTimer;
        private int mCurCommand;

        public SerialComm()
        {
            serialPort = new SerialPort();
            thread = null;
            _keepReading = false;

        }

        public bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        private void StartReading()
        {
            if (!_keepReading)
            {
                _keepReading = true;
                thread = new Thread(new ThreadStart(ReadPort));
                //thread.
                thread.Start();
            }
        }

        private void StopReading()
        {
            if (_keepReading)
            {
                _keepReading = false;
                thread.Join();
                thread = null;
            }
        }

        private void ReadPort()
        {
            while (_keepReading)
            {
                if (serialPort.IsOpen)
                {
                    int count = serialPort.BytesToRead;
                    if (count > 0)
                    {
                        byte[] readBuffer = new byte[count];
                        try
                        {
                            //Application.DoEvents();
                            serialPort.Read(readBuffer, 0, count);
                            if (DataReceived != null)
                                DataReceived(readBuffer);
                            Thread.Sleep(100);
                        }
                        catch (TimeoutException)
                        {
                        }
                    }
                }
            }
        }

        public bool Open(string portname)
        {
            Close();
            serialPort.PortName = portname;
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            try
            {
                serialPort.Open();
            }catch(Exception e)
            {

                return false;
            }

            if (serialPort.IsOpen)
            {

                Console.WriteLine("port is open");
                //DataReceived += new EventHandle(OnDataReceived);
                //StartReading();

                serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(SerailDataRead);
                return true;
            }
            else
            {
                //MessageBox.Show("串口打开失败！");
                return false;
            }
        }

        public void Close()
        {
            //StopReading();
            serialPort.Close();
        }

        public void WritePort(byte[] send, int offSet, int count)
        {
            if (IsOpen)
            {
                serialPort.Write(send, offSet, count);
            }
        }

        public void writeCommand(int type)
        {
            Console.WriteLine("Write COMMAND=" + getCommandName(type));
            byte[] sender = new byte[COMMAND_LENGTH];
            sender[0] = COMMAND_START;
            sender[COMMAND_LENGTH-1] = COMMAND_END;
            lastCommand = type;
            switch (type)
            {
                case COMMAND_TYPE_READ_SYSTEM_ID:
                    sender[1] = 0xC0;
                    sender[2] = 1;
                    break;
                case COMMAND_TYPE_WRITE_NTC:
                    sender[1] = 0xC1;
                    sender[2] = 1;
                    sender[3] = (byte)int.Parse(ProdDataHandler.ntc);
                    break;
                case COMMAND_TYPE_WRITE_SN:
                    sender[1] = 0xC1;
                    sender[2] = 2;
                    string datestr = System.DateTime.Now.ToString("yyyyMMdd").Substring(3);
                    Common.sn = datestr + string.Format("{0:D4}", ExcelHandler.index) + ProdDataHandler.work_loc + ProdDataHandler.factory;
                    Common.sn = Common.sn.ToUpper();
                    byte[] array_sn = System.Text.Encoding.Default.GetBytes(Common.sn);
                    Buffer.BlockCopy(array_sn, 0, sender, 3, SN_LENGTH);
                    break;
                case COMMAND_TYPE_WRITE_HW:
                    sender[1] = 0xC1;
                    sender[2] = 3;
                    //hw_ver = hw_ver.ToUpper();
                    hw_ver = ProdDataHandler.hw_ver + "-" + ProdDataHandler.battery.Substring(0, 1) + "-" + string.Format("{0:D2}", int.Parse(ProdDataHandler.ntc));
                    hw_ver = hw_ver.ToUpper();
                    byte[] array_hw = System.Text.Encoding.Default.GetBytes(hw_ver);
                    Buffer.BlockCopy(array_hw, 0, sender, 3, array_hw.Length);
                    break;
                case COMMAND_TYPE_WRITE_MODEL:
                    sender[1] = 0xC1;
                    sender[2] = 4;
                    byte[] array_model = System.Text.Encoding.Default.GetBytes(ProdDataHandler.model_num);
                    Buffer.BlockCopy(array_model, 0, sender, 3, array_model.Length);
                    break;
                case COMMAND_TYPE_DONE:
                    sender[1] = 0xC1;
                    sender[2] = 5;
                    break;
            }

            WritePort(sender, 0, 25);

        }

        
        private void SerailDataRead(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (readBuffer == null)
                {
                    readBuffer = new byte[COMMAND_LENGTH];
                }
                //while (serialPort.BytesToRead > 0)
                while (mCurLen<COMMAND_LENGTH)
                {
                    int count = serialPort.BytesToRead;
                    byte[] tmpBuffer = new byte[count];
                    serialPort.Read(tmpBuffer, 0, count);
                    int j = 0;
                    if (hasStart == false)
                    {
                        while (j < count)
                        {
                            if (tmpBuffer[j] == COMMAND_START)
                            {
                                hasStart = true;
                                if ((count - j) > COMMAND_LENGTH)
                                {
                                    Buffer.BlockCopy(tmpBuffer, j, readBuffer, 0, COMMAND_LENGTH);
                                    mCurLen = COMMAND_LENGTH;
                                }
                                else
                                {
                                    Buffer.BlockCopy(tmpBuffer, j, readBuffer, 0, count - j);
                                    mCurLen += count - j;
                                }
                                startTimer();
                                break;
                            }
                            j++;
                        }
                    }else
                    {
                        if ((mCurLen + count) > COMMAND_LENGTH)
                        {
                            Console.Write("total received count=" + (mCurLen + count)+"\n");
                            count = COMMAND_LENGTH - mCurLen;
                        }
                        
                        Buffer.BlockCopy(tmpBuffer, 0, readBuffer, mCurLen, count);
                        mCurLen += count;
                    }
                    
                }
                Console.Write("Received 25 or more bytes, disable time\n");
                stopTimer();

                if (hasStart && (readBuffer[COMMAND_LENGTH-1] == COMMAND_END))
                {
                    //serialPort.Read(readBuffer, 0, 25);
                    //在这里对接收到的数据进行处理     
                    OnDataReceived(readBuffer);
                }

                mCurLen = 0;
                hasStart = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        //one command should be received in 100ms
        private void startTimer()
        { 
            if (dispatcherTimer == null)
            {
                Console.Write("startTimer\n");
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
            }else
            {
                Console.Write("enableTimer\n");
                dispatcherTimer.Start();
            }
        }

        private void stopTimer()
        {
            if (dispatcherTimer != null && dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Console.Write("Timer is over, diable time\n");
            mCurLen = 0;
            hasStart = false;
            dispatcherTimer.Stop();
        }


        private void OnDataReceived(byte[] buffer)
        {
            Console.WriteLine("OnDataReceived");
            //Console.WriteLine(System.Text.Encoding.Default.GetString(buffer));
            /*bool hasStart = false;
            int i;
            for (i = 0; i < buffer.Length; ++i)
            {
                if ((buffer[i] == COMMAND_START))
                {
                    int endloc = i + COMMAND_LENGTH - 1;
                    if ((endloc < buffer.Length) && (buffer[endloc] == COMMAND_END))
                    {
                        hasStart = true;
                        break;
                    }
                }
            }*/
            //if (hasStart)
            int i = 0;
            if (true) //already check in SerailDataRead
            {
                Console.WriteLine("Valid command, start=" + i);
                i = i + 1; //the byte after "!"
                if (buffer[i] == 0x49 && buffer[i + 2] == 0x41 && buffer[i + 3] == 0x4D &&
                    buffer[i + 5] == 0x4D && buffer[i + 6] == 0x4D && buffer[i + 7] == 0x43)
                {
                    Console.WriteLine(COMMAND_HELLO);
                    onUpdate(Common.LOG_TYPE_CLEAR, null);
                    onUpdate(Common.LOG_TYPE_INFO, COMMAND_HELLO);
                    writeCommand(COMMAND_TYPE_READ_SYSTEM_ID);
                }
                else if (buffer[i] == 0xC0 && buffer[i + 1] == 1)
                {
                    //read system i
                    i = i + 2; //i is the start of mac
                    Common.mac = ByteToString(buffer, i, 6);
                    Console.WriteLine("Read system id, mac="+Common.mac);
                    onUpdate(Common.LOG_TYPE_INFO, "mac=" + Common.mac);
                    writeCommand(COMMAND_TYPE_WRITE_NTC);
                    reSendCount = 0;
                }
                else if (buffer[i] == 0xC1 && buffer[i + 1] == 1 && lastCommand == COMMAND_TYPE_WRITE_NTC)
                {
                    //check ntc write response
                    writeCommandLog(buffer[i + 2] == int.Parse(ProdDataHandler.ntc), COMMAND_TYPE_WRITE_NTC);
                }
                else if (buffer[i] == 0xC1 && buffer[i + 1] == 2 && lastCommand == COMMAND_TYPE_WRITE_SN)
                {
                    //check sn write response
                    string snresp = getResponse(buffer, i+2);
                    if (snresp != null && Common.sn != null && snresp.Equals(Common.sn)) {
                        writeCommandLog(true, COMMAND_TYPE_WRITE_SN);
                    }
                    else
                    {
                        writeCommandLog(false, COMMAND_TYPE_WRITE_SN);
                    }
                }
                else if (buffer[i] == 0xC1 && buffer[i + 1] == 3 && lastCommand == COMMAND_TYPE_WRITE_HW)
                {
                    //check sn write response, not done yet
                    /*byte[] hwba = new byte[HW_LENGTH];
                    Buffer.BlockCopy(buffer, i+2, hwba, 0, HW_LENGTH);
                    string hwresp = System.Text.Encoding.Default.GetString(hwba);*/
                    string hwresp = getResponse(buffer, i + 2);
                    if (hwresp != null && hw_ver != null && hwresp.Equals(hw_ver))
                    {
                        writeCommandLog(true, COMMAND_TYPE_WRITE_HW);
                    }
                    else
                    {
                        writeCommandLog(false, COMMAND_TYPE_WRITE_HW);
                    }
                }
                else if (buffer[i] == 0xC1 && buffer[i + 1] == 4 && lastCommand == COMMAND_TYPE_WRITE_MODEL)
                {
                    /*int num = 0;
                    i = i + 2;
                    while (buffer[i + num] != 0)
                    {
                        num++;
                    }
                    byte[] modelba = new byte[num];
                    Buffer.BlockCopy(buffer, i, modelba, 0, num);
                    string modelresp = System.Text.Encoding.Default.GetString(modelba);*/
                    string modelresp = getResponse(buffer, i + 2);
                    if (modelresp!=null && ProdDataHandler.model_num!=null && modelresp.Equals(ProdDataHandler.model_num))
                    {
                        writeCommandLog(true, COMMAND_TYPE_WRITE_MODEL);
                    }
                    else
                    {
                        writeCommandLog(false, COMMAND_TYPE_WRITE_MODEL);
                    }
                }
                else if (buffer[i] == 0xC1 && buffer[i + 1] == 5 && lastCommand == COMMAND_TYPE_DONE)
                {
                    //writeCommandLog(true, COMMAND_TYPE_DONE);
                    writeCommandDone(true);
                }
            }/*else
            {
                onUpdate(Common.LOG_TYPE_ERROR, "Received buffer:" + ByteToString(buffer, 0, buffer.Length));
            }*/
        }

        private void writeCommandDone(bool isOK)
        {
            Console.Write("writeCommandDone " + isOK + "\n");
            stopDoneTimer();
            if (isOK)
            {
                onUpdate(Common.LOG_TYPE_WRITE_COMMAND_DONE, "Write " + getCommandName(COMMAND_TYPE_DONE) + " OK");
                if (ProdDataHandler.factory.ToUpper() == Common.XINWANDA_FACTORY_CODE)
                {
                    Hashtable res = Mesnet.WeldInputTest("", "", "");
                    ProdDataHandler.mesinputtest = res[Mesnet.RESP_MSG].ToString();
                    bool ret_mes = (bool)res[Mesnet.RESP_RESULT];
                    onUpdate(ret_mes ? Common.LOG_TYPE_INFO : Common.LOG_TYPE_ERROR, "WeldInputTest result=" + ProdDataHandler.mesinputtest);
                }
                //print sn
                bool printres = Common.print_sn();
                if (printres == false)
                {
                    onUpdate(Common.LOG_TYPE_ERROR, "Print sn error, please check the location");
                }
                //write excel
                int insertres = ExcelHandler.insertDeviceInfo(Common.mac, hw_ver, ProdDataHandler.model_num, Common.sn, "ok", ProdDataHandler.mesinputtest);
                if (insertres == ExcelHandler.ERROR_EXCEL_INSERT_INFO)
                {
                    onUpdate(Common.LOG_TYPE_ERROR, "Insert device info fail");
                }
            }else
            {
                onUpdate(Common.LOG_TYPE_WRITE_COMMAND_FAIL, "Write " + getCommandName(COMMAND_TYPE_DONE) + " Fail");
                if (ProdDataHandler.factory.ToUpper() == Common.XINWANDA_FACTORY_CODE)
                {
                    Hashtable res = Mesnet.WeldInputTest("", "", "");
                    ProdDataHandler.mesinputtest = res[Mesnet.RESP_MSG].ToString();
                    bool ret_mes = (bool)res[Mesnet.RESP_RESULT];
                    onUpdate(ret_mes ? Common.LOG_TYPE_INFO : Common.LOG_TYPE_ERROR, "WeldInputTest result=" + ProdDataHandler.mesinputtest);
                }
                //print sn
                bool printres = Common.print_sn();
                if (printres == false)
                {
                    onUpdate(Common.LOG_TYPE_ERROR, "Print sn error, please check the location");
                }
                //write excel
                int insertres = ExcelHandler.insertDeviceInfo(Common.mac, hw_ver, ProdDataHandler.model_num, Common.sn, "fail", ProdDataHandler.mesinputtest);
                if (insertres == ExcelHandler.ERROR_EXCEL_INSERT_INFO)
                {
                    onUpdate(Common.LOG_TYPE_ERROR, "Insert device info fail");
                }
            }
        }

        //resend command if response is not received in 1 second
        private void startRetryTime(int com_type)
        {
            mCurCommand = com_type;
            if (retryTimer == null)
            {
                Console.Write("start retryTimer\n");
                retryTimer = new System.Windows.Threading.DispatcherTimer();
                retryTimer.Tick += new EventHandler(retryTimer_Tick);
                retryTimer.Interval = new TimeSpan(0, 0, 1);
                retryTimer.Start();
            }
            else
            {
                Console.Write("enable retryTimer\n");
                retryTimer.Start();
            }
        }

        private void retryTimer_Tick(object sender, EventArgs e)
        {
            Console.Write("Timer is over, mCurCommand="+mCurCommand+"\n");
            writeCommandLog(false, mCurCommand);
            stopRetryTimer();
        }


        private void stopRetryTimer()
        {
            Console.Write("stop retry timer\n");
            if (retryTimer != null && retryTimer.IsEnabled)
            {
                
                retryTimer.Stop();
            }
        }


        //resend command if response is not received in 1 second
        private void startDoneTime()
        {
            if (doneWaitTimer == null)
            {
                Console.Write("start doneTimer\n");
                doneWaitTimer = new System.Windows.Threading.DispatcherTimer();
                doneWaitTimer.Tick += new EventHandler(retryTimer_Tick);
                doneWaitTimer.Interval = new TimeSpan(0, 0, 3);
                doneWaitTimer.Start();
            }
            else
            {
                Console.Write("stop doneTimer\n");
                doneWaitTimer.Start();
            }
        }

        private void DoneTimer_Tick(object sender, EventArgs e)
        {
            Console.Write("DoneTimer_Tick\n");
            //writeCommandLog(false, mCurCommand);
            //doneWaitTimer.Stop();
            writeCommandDone(false);
            stopDoneTimer();
        }


        private void stopDoneTimer()
        {
            Console.Write("stopDoneTimer\n");
            if (doneWaitTimer != null && doneWaitTimer.IsEnabled)
            {
                doneWaitTimer.Stop();
            }
        }


        public void writeCommandLog(bool isOK, int type)
        {
            if (isOK)
            {
                reSendCount = 0;
                Console.WriteLine("Write OK, command=" + getCommandName(type));
                onUpdate(Common.LOG_TYPE_INFO, "Write " + getCommandName(type) + " OK");
                if (type < COMMAND_TYPE_DONE)
                {
                    stopRetryTimer();
                    if ((type + 1) < COMMAND_TYPE_DONE)
                    {
                        startRetryTime(type + 1);
                    }else
                    {
                        startDoneTime();
                    }
                    writeCommand(type + 1); //write next one
                }
            }
            else
            {
                stopDoneTimer();
                stopRetryTimer();
                if (reSendCount == 0)
                {
                    reSendCount++;
                    writeCommand(type);
                    Console.WriteLine("retry command=" + getCommandName(type));
                    onUpdate(Common.LOG_TYPE_INFO, "retry " + getCommandName(type));
                }
                else
                {
                    Console.WriteLine("Write fail, command=" + getCommandName(type));
                    onUpdate(Common.LOG_TYPE_ERROR, "Write " + getCommandName(type)+ " fail");
                    //write excel
                    ExcelHandler.insertDeviceInfo(Common.mac, hw_ver, ProdDataHandler.model_num, Common.sn, "fail"+getCommandName(type), ProdDataHandler.mesinputtest);
                }
            }
        }


        public string getCommandName(int type)
        {
            string result = "wrong command";
            switch (type)
            {
                case COMMAND_TYPE_READ_SYSTEM_ID:
                    result = "COMMAND_TYPE_READ_SYSTEM_ID";
                    break;
                case COMMAND_TYPE_WRITE_NTC:
                    result = "COMMAND_TYPE_WRITE_NTC";
                    break;
                case COMMAND_TYPE_WRITE_SN:
                    result = "COMMAND_TYPE_WRITE_SN";
                    break;
                case COMMAND_TYPE_WRITE_HW:
                    result = "COMMAND_TYPE_WRITE_HW";
                    break;
                case COMMAND_TYPE_WRITE_MODEL:
                    result = "COMMAND_TYPE_WRITE_MODEL";
                    break;
                case COMMAND_TYPE_DONE:
                    result = "COMMAND_TYPE_DONE";
                    break;
                default:
                    result = "wrong command, command=" + type;
                    break;
            }
            return result;
        }

        //hex to string
        public string ByteToString(byte[] InBytes, int start, int len)
        {
            string StringOut = "";
            for (int i = start; i < start + len; i++)
            {
                StringOut = String.Format("{0:X2}", InBytes[i]) + StringOut;
            }
            return StringOut;
        }

        //string to hex
        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
            }
            return ByteOut;
        }


        public void updateMsg(int type, string str)
        {
            if (onUpdate != null)
            {
                onUpdate(type, str);
            }
        }

        //start: remove the command_start, only convert command body to string 
        private string getResponse(byte[] src, int start)
        {
            string str = null;
            int count = 0;
            //remove 0 at the end of the buffer
            for(count = 0; count<25; ++count) {
                if (src[count] == 0)
                {
                    break;
                }
            }
            byte[] des = new byte[count-start];
            for(int i=0; i<(count-start); ++i)
            {
                des[i] = src[start + i];
            }
            try
            {
                str = System.Text.Encoding.Default.GetString(des);
            }catch(Exception e)
            {

            }
            return str;
        }
    }

    
    


    /*
     * string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < portList.Length; ++i)
            {
                string name = portList[i];
                comboBox1.Items.Add(name);
            }
     */
}
