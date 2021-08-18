using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mmc_production
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProdDataHandler dataHandler;

        private bool isModifiedInfo;

        private SerialComm mSerial;
        private bool isSerialOpen;

        private Paragraph logPara;

        private bool mesLoadOK = false;
        
        private static string BUTTON_MODIFY_PROD_INIT = "修改产品信息";
        private static string BUTTON_MODIFY_PROD_CONFIRM = "确认产品信息";

        public MainWindow()
        {
            InitializeComponent();

            initUI();
        }

        private void initUI()
        {
            //init mes login componet
            dataHandler = new ProdDataHandler();
            dataHandler.readData();
            this.mes_account.Text = ProdDataHandler.mes_account;
            this.mes_device_num.Text = ProdDataHandler.mes_device;
            this.mes_make_order.Text = ProdDataHandler.mes_make_order;
            //mes login ok, show mes status, hide mes login button
            this.mes_login_button.Visibility = Visibility.Visible;
            this.mes_status.Visibility = Visibility.Hidden;

            this.print_loc.Text = ProdDataHandler.print_loc;
            this.print_loc.LostFocus += new RoutedEventHandler(onFocusLost);

            isModifiedInfo = false;
            this.ntc.Text = ProdDataHandler.ntc;
            foreach (string str in Enum.GetNames(typeof(battery_enum)))
            {
                this.battery.Items.Add(str);
            }
            if (ProdDataHandler.battery == "")
            {
                this.battery.SelectedIndex = 0;
            }
            else
            {
                //Enum.GetValue(typeof(battery_enum), "test");
                this.battery.SelectedIndex = (int)Enum.Parse(typeof(battery_enum), ProdDataHandler.battery, true);
            }
            this.hw.Text = ProdDataHandler.hw_ver;
            this.modelnum.Text = ProdDataHandler.model_num;
            this.workloc.Text = ProdDataHandler.work_loc;
            this.factory.Text = ProdDataHandler.factory;
            this.modify.Content = BUTTON_MODIFY_PROD_INIT;

            //
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < portList.Length; ++i)
            {
                string name = portList[i];
                s_com.Items.Add(name);
            }

            //init serial port status
            mSerial = new SerialComm();
            mSerial.onUpdate += new SerialComm.MessageUpdate(addLog);
            setSerialOpenStatus(false);

            //init log text box
            this.log.Document.Blocks.Clear();
            logPara = new Paragraph();
            this.log.Document.Blocks.Add(logPara);

            //Init log
            //Common.mExcelHandler = new ExcelHandler();
        }

        private void onFocusLost(object sender, EventArgs e)
        {
            if (this.print_loc.Text != "" && !this.print_loc.Text.Trim().Equals(ProdDataHandler.print_loc))
            {
                Console.Write("On focus lost");
                ProdDataHandler.print_loc = this.print_loc.Text.Trim();
                dataHandler.writePrintLoc(ProdDataHandler.print_loc);
            }
        }

        /*private string getBatteryName(battery_enum id)
        {
            string result = string.Empty;
            switch (id)
            {
                case battery_enum.Maxwell:
                    result = "Maxwell";
                    break;
                case battery_enum.Panasonic:
                    result = "Panasonic";
                    break;
                default:
                    result = "未知";
                    break;
            }
            return result;

        }*/

        private void mes_login_button_Click(object sender, RoutedEventArgs e)
        {
            this.mes_login_button.IsEnabled = false;
            string accstr = this.mes_account.Text;
            if (accstr == "")
            {
                MessageBox.Show("操作员工号不能为空");
                this.mes_login_button.IsEnabled = true;
                return;
            }

            IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.mes_pwd.SecurePassword);
            string pwdstr = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);      
            if (pwdstr == "")
            {
                MessageBox.Show("密码不能为空");
                this.mes_login_button.IsEnabled = true;
                return;
            }

            string device = this.mes_device_num.Text;
            if (device == "")
            {
                MessageBox.Show("设备编号不能为空");
                this.mes_login_button.IsEnabled = true;
                return;
            }

            string order = this.mes_make_order.Text;
            if (order == "")
            {
                MessageBox.Show("制令单号不能为空");
                this.mes_login_button.IsEnabled = true;
                return;
            }

            //check user does not need order, order is for further use
            Hashtable result = Mesnet.CheckUserDo(accstr, pwdstr, device);
            if ((bool)result[Mesnet.RESP_RESULT])
            {
                MessageBox.Show("成功");
                dataHandler.writeMesData(accstr, device, order);
                mesLoadOK = true;
                this.mes_login_button.Visibility = Visibility.Hidden;
                this.mes_status.Visibility = Visibility.Visible;
            }else
            {
                MessageBox.Show("失败");
                mesLoadOK = false;
                this.mes_login_button.IsEnabled = true;
                this.mes_login_button.Visibility = Visibility.Visible;
                this.mes_status.Visibility = Visibility.Hidden;
            }
       
        }

        private void print_button_Click(object sender, RoutedEventArgs e)
        {
            dataHandler.writePrintLoc(this.print_loc.Text.Trim());
            MessageBox.Show("地址已保存");
        }

        private void modify_Click(object sender, RoutedEventArgs e)
        {
            if (!isModifiedInfo)
            { 
                pwdwindow win = new pwdwindow();
                win.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                //win.ShowDialog();
                if (win.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //password verify ok
                    isModifiedInfo = true;
                    this.ntc.IsEnabled = true;
                    this.battery.IsEnabled = true;
                    this.hw.IsEnabled = true;
                    this.modelnum.IsEnabled = true;
                    this.workloc.IsEnabled = true;
                    this.factory.IsEnabled = true;
                    this.modify.Content = BUTTON_MODIFY_PROD_CONFIRM;
                }
                
            }else
            {
                if (this.ntc.Text=="" || this.hw.Text=="" || this.modelnum.Text=="" || this.workloc.Text=="" || this.factory.Text == "")
                {
                    MessageBox.Show("请完善生产信息");
                }
                else
                {
                    //user modified the info and press button again, save info and disable all fields
                    dataHandler.writeProductInfo(this.ntc.Text, this.battery.Text, this.hw.Text, this.modelnum.Text, this.workloc.Text, this.factory.Text);
                    isModifiedInfo = false;
                    this.ntc.IsEnabled = false;
                    this.battery.IsEnabled = false;
                    this.hw.IsEnabled = false;
                    this.modelnum.IsEnabled = false;
                    this.workloc.IsEnabled = false;
                    this.factory.IsEnabled = false;
                    this.modify.Content = BUTTON_MODIFY_PROD_INIT;
                    //create new excel is product info changed
                    openExcel();
                }
            }
        }

        private void openport_Click(object sender, RoutedEventArgs e)
        {
            if (checkProductInfo() == false)
            {
                return;
            }
            if (mSerial == null)
            {
                mSerial = new SerialComm();
            }
            if (isSerialOpen == false)
            {
                bool res = mSerial.Open(this.s_com.Text);
                if (res == false)
                {
                    addLog(Common.LOG_TYPE_ERROR, "打开串口失败，串口拒绝访问");
                }
                else
                {
                    setSerialOpenStatus(res);
                    //open excel for write info into
                    openExcel();
                    //for test
                    //Common.mac = "1234";
                    //Common.print_sn();
                }
            }
        }

        private void closeport_Click(object sender, RoutedEventArgs e)
        {
            if (isSerialOpen == true)
            {
                mSerial.Close();
                ExcelHandler.closeExcel();
                setSerialOpenStatus(false);
            }
        }

        private void setSerialOpenStatus(bool isOpen)
        {
            isSerialOpen = isOpen;
            if (isSerialOpen)
            {
                this.openport.IsEnabled = false;
                this.closeport.IsEnabled = true;
                addLog(Common.LOG_TYPE_INFO, "串口已打开");
            }else
            {
                this.openport.IsEnabled = true;
                this.closeport.IsEnabled = false;
                addLog(Common.LOG_TYPE_INFO, "串口已关闭");
            }
        }

        private void addLog(int type, string str)
        {
            if (logPara != null)
            {
                this.logPara.Dispatcher.Invoke(new Action(
                delegate
                {
                    if (type == Common.LOG_TYPE_CLEAR)
                    {
                        logPara.Inlines.Clear();
                    }
                    else
                    {
                        Run newRun = new Run();
                        if (type == Common.LOG_TYPE_INFO || type == Common.LOG_TYPE_WRITE_COMMAND_DONE)
                        {
                            newRun.Foreground = Brushes.Black;
                            if (type == Common.LOG_TYPE_WRITE_COMMAND_DONE)
                            {
                                System.Media.SystemSounds.Beep.Play(); //提示写设备成功
                            }
                        }
                        else if (type == Common.LOG_TYPE_ERROR || type == Common.LOG_TYPE_WRITE_COMMAND_FAIL)
                        {
                            newRun.Foreground = Brushes.Red;
                            if (type == Common.LOG_TYPE_WRITE_COMMAND_FAIL)
                            {
                                for (int i = 0; i < 5; ++i)
                                {
                                    //提示写设备失败
                                    System.Media.SystemSounds.Hand.Play();
                                    //System.Media.SystemSounds.Beep.Play();
                                    System.Threading.Thread.Sleep(200);

                                }
                            }
                        }
                        newRun.Text = str + "\n";

                        logPara.Inlines.Add(newRun);
                        log.ScrollToEnd();
                    }
                }));
                
                //this.log.ScrollToEnd();
            }
            //Console.Write(str + "\n");   
        }

        private void clearLog()
        {
            if (logPara != null)
            {
                this.logPara.Dispatcher.Invoke(new Action(
                delegate
                {
                    this.logPara.Inlines.Clear();
                }));

                //this.log.ScrollToEnd();
            }
        }

        public bool checkProductInfo()
        {
            if (ProdDataHandler.factory.ToUpper()== Common.XINWANDA_FACTORY_CODE && mesLoadOK == false)
            {
                MessageBox.Show("MES尚未登陆");
                return false;
            }

            //ProdDataHandler.print_loc = this.print_loc.Text; 
            if (this.print_loc.Text == "")
            {
                MessageBox.Show("打印地址未设置");
                return false;
            }else
            {
                if (!this.print_loc.Text.Trim().Equals(ProdDataHandler.print_loc))
                {
                    ProdDataHandler.print_loc = this.print_loc.Text.Trim();
                    dataHandler.writePrintLoc(ProdDataHandler.print_loc);
                }
            }

            if (ProdDataHandler.battery!="" && ProdDataHandler.factory!=""
                && ProdDataHandler.hw_ver!="" && ProdDataHandler.model_num!=""
                && ProdDataHandler.ntc!="" && ProdDataHandler.work_loc != "")
            {
                return true;

            }else
            {
                MessageBox.Show("请完善生产信息");
                return false;
            }
        }

        public void openExcel()
        {
            if (isSerialOpen && checkProductInfo())
            {
                //for get last index
                int res = ExcelHandler.openExcel();
                if (res == ExcelHandler.ERROR_EXCEL_OPEN)
                {
                    addLog(Common.LOG_TYPE_ERROR, "Open excel fail");
                }else if (res == ExcelHandler.ERROR_EXCEL_CREATE)
                {
                    addLog(Common.LOG_TYPE_ERROR, "Create excel fail");
                }
                ExcelHandler.closeExcel();
            }
        }

        public void onClose()
        {
            ExcelHandler.closeExcel();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.Write("selected index="+tabControl.SelectedIndex+"\n");
            if (tabControl.SelectedIndex == 1)
            {
                Common.array[0] = line1.Text.Trim();
                Common.array[1] = line2.Text.Trim();
                Common.array[2] = line3.Text.Trim();
                Common.array[3] = line4.Text.Trim();
                Common.array[4] = line5.Text.Trim();
                Common.array[5] = line6.Text.Trim();
                Common.array[6] = line7.Text.Trim();
                Common.array[7] = line8.Text.Trim();
                Common.array[8] = line9.Text.Trim();
                Common.array[9] = line10.Text.Trim();
                Common.array[10] = line11.Text.Trim();
            }
        }

    }
}
