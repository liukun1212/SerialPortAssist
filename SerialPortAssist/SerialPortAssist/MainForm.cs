using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortAssist
{
    public partial class MainForm : Form
    {
        public static int btnSerialNumber;
        public bool bState = false;
        private long received_count = 0;//接收计数     
        private long send_count = 0;//发送计数 
        private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。  
        private static StringBuilder sbSTH = new StringBuilder();
        private static StringBuilder sbHTS = new StringBuilder();

        private static bool isHex = false;
        //public static string btnSerialNumberIdentification;
        public MainForm()
        {
            InitializeComponent();
            btnSaveDate.Enabled = false;
            SerialPortInit();
        }

        private void SerialPortInit()
        {
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MessageBox.Show("没有可用串口号！", "Error");
                return;
            }
            foreach (string s in SerialPort.GetPortNames())
            {
                cBoxSerialPort.Items.Add(s);
            }
            cBoxSerialPort.SelectedIndex = 0;
        }

     

        private void btn9_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//按下左侧按钮就是发送
            {
                string temp = ((Button)sender).Name.ToString();
                string[] strArray = temp.Split('n');
                //btnSerialNumber = Convert.ToInt32(strArray[1]);

                SendData(DataSaveing.LoadIniFile(strArray[1]));

            }
            else //按下右侧按钮，输入新的按钮名称
            {
                if (tBoxNewName.Text == string.Empty)
                {
                    MessageBox.Show("请在左下角输入框输入新的按键名称！", "提示");
                }
                else
                {
                    ((Button)sender).Text = tBoxNewName.Text.ToString();
                    string temp = ((Button)sender).Name.ToString();
                    string[] strArray = temp.Split('n');
                    DataSaveing.SaveButtonSetText(strArray[1].ToString(), tBoxNewName.Text.ToString());
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cBoxBaudRate.SelectedIndex = 7;
            cBoxDataBits.SelectedIndex = 3;
            cBoxStopBits.SelectedIndex = 0;
            cBoxParity.SelectedIndex = 0;
            pictureBoxFlag.BackColor = Color.Gray;
            checkBoxHexRecive.Checked = true;
            checkBoxHexSend.Checked = true;
           
            LoadDataInfor();
            // CreatDefaultButtonText();
            CheckForIllegalCrossThreadCalls = false;
            serialPortTempColle.DataReceived += new SerialDataReceivedEventHandler(serialPortTempColle_DataReceived);

            FormLoadButtonText();
        }


        private void FormLoadButtonText()
        {
            foreach (Control c in this.groupBox1.Controls)
            {
                if (c is Button)
                {
                    string temp1 = c.Name;
                    string[] temp2;
                    temp2 = temp1.Split('n');
                    c.Text = DataSaveing.LoadButtonText(temp2[1]);
                }
            }
        }    

        void serialPortTempColle_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = serialPortTempColle.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致     
            byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据     
            received_count += n;//增加接收计数     
            serialPortTempColle.Read(buf, 0, n);//读取缓冲数据     
            builder.Clear();//清除字符串构造器的内容     
                            //因为要访问ui资源，所以需要使用invoke方式同步ui。 

            this.BeginInvoke((EventHandler)(delegate
            {
                //判断是否是显示为16进制   
                if (checkBoxHexRecive.Checked)
                {
                    //依次的拼接出16进制字符串     
                    foreach (byte b in buf)
                    {
                        builder.Append(b.ToString("X2") + " ");
                    }
                }
                else
                {
                    //直接按ASCII规则转换成字符串     
                    //UTF8Encoding
                    builder.Append(Encoding.ASCII.GetString(buf));
                    //builder.Append(Encoding.UTF8.GetString(buf));
                }
                //追加的形式添加到文本框末端，并滚动到最后。     
                this.tBoxRecive.AppendText(builder.ToString());
                //修改接收计数     
                labelGetCount.Text = "接收次数：" + received_count.ToString();
            }));
        }
        private void LoadDataInfor()
        {
            for (int i = 1; i < 65; i++)
            {
                string[] value = { Convert.ToString(i), DataSaveing.LoadIniFile(Convert.ToString(i)) };
                dataGridView1.Rows.Add(value);
            }
        }

        /// <summary>
        /// 将当前datagridview里面的数据保存到inifile里面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveDate_Click(object sender, EventArgs e)
        {
            if (btnSaveDate.Enabled)
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)        //循环将所有行的数据保存到inifile里面
                {
                    string tempSerialNumber = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    string tempCommadn = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    DataSaveing.SaveDate(tempSerialNumber, tempCommadn);
                }
                MessageBox.Show("保存成功！", "提示");
                btnSaveDate.Enabled = false;
            }
        }

        private void btnONOFFSerialPort_Click(object sender, EventArgs e)
        {
            try
            {
                //根据当前串口对象，来判断操作     
                if (serialPortTempColle.IsOpen)
                {
                    //打开时点击，则关闭串口     
                    serialPortTempColle.Close();
                    cBoxSerialPort.Enabled = true;
                    cBoxBaudRate.Enabled = true;
                    cBoxDataBits.Enabled = true;
                    cBoxStopBits.Enabled = true;
                    cBoxParity.Enabled = true;
                    pictureBoxFlag.BackColor = Color.Gray;
                }
                else
                {
                    try
                    {
                        InitSerialPort();
                        serialPortTempColle.Open();
                        //设置必要控件不可用
                        cBoxSerialPort.Enabled = false;
                        cBoxBaudRate.Enabled = false;
                        cBoxDataBits.Enabled = false;
                        cBoxStopBits.Enabled = false;
                        cBoxParity.Enabled = false;

                        pictureBoxFlag.BackColor = Color.LightGreen;
                    }
                    catch (Exception ex)
                    {
                        //捕获到异常信息，创建一个新的comm对象，之前的不能用了。     
                        serialPortTempColle = new SerialPort();
                        //现实异常信息给客户。     
                        MessageBox.Show(ex.Message);
                    }
                }
                //设置按钮的状态     
                btnONOFFSerialPort.Text = serialPortTempColle.IsOpen ? "关闭串口" : "打开串口";
                btnSend.Enabled = serialPortTempColle.IsOpen;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitSerialPort()
        {
            if (!serialPortTempColle.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cBoxSerialPort.SelectedItem.ToString();
                    serialPortTempColle.PortName = serialName;


                    string strBaudRate = cBoxBaudRate.Text.ToString();
                    string strDateBits = cBoxDataBits.Text.ToString();
                    string strStopBits = cBoxStopBits.Text.ToString();
                    string strParity = cBoxParity.Text.ToString();

                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    //serialPortTempColle.BaudRate = iBaudRate;       //波特率
                    //serialPortTempColle.DataBits = iDateBits;       //数据位

                    switch (iBaudRate)
                    {
                        case 300:
                            serialPortTempColle.BaudRate = 300;
                            break;
                        case 600:
                            serialPortTempColle.BaudRate = 600;
                            break;
                        case 1200:
                            serialPortTempColle.BaudRate = 1200;
                            break;
                        case 2400:
                            serialPortTempColle.BaudRate = 2400;
                            break;
                        case 4800:
                            serialPortTempColle.BaudRate = 4800;
                            break;
                        case 9600:
                            serialPortTempColle.BaudRate = 9600;
                            break;
                        case 19200:
                            serialPortTempColle.BaudRate = 19200;
                            break;
                        case 38400:
                            serialPortTempColle.BaudRate = 38400;
                            break;
                        case 115200:
                            serialPortTempColle.BaudRate = 115200;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    switch (iDateBits)
                    {
                        case 5:
                            serialPortTempColle.DataBits = 5;
                            break;
                        case 6:
                            serialPortTempColle.DataBits = 6;
                            break;
                        case 7:
                            serialPortTempColle.DataBits = 7;
                            break;
                        case 8:
                            serialPortTempColle.DataBits = 8;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    switch (strStopBits)            //停止位
                    {
                        case "1":
                            serialPortTempColle.StopBits = StopBits.One;
                            break;
                        case "1.5":
                            serialPortTempColle.StopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            serialPortTempColle.StopBits = StopBits.Two;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    switch (strParity)             //校验位
                    {
                        case "无":
                            serialPortTempColle.Parity = Parity.None;
                            break;
                        case "奇校验":
                            serialPortTempColle.Parity = Parity.Odd;
                            break;
                        case "偶校验":
                            serialPortTempColle.Parity = Parity.Even;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    if (serialPortTempColle.IsOpen == true)//如果打开状态，则先关闭一下
                    {
                        serialPortTempColle.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }


        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            btnSaveDate.Enabled = true;
        }

        private void btnClearSend_Click(object sender, EventArgs e)
        {
            tBoxSend.Text = string.Empty;
        }

        private void btnClearRecive_Click(object sender, EventArgs e)
        {
            tBoxRecive.Text = string.Empty;

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                //定义一个变量，记录发送了几个字节     
                int n = 0;
                //16进制发送     
                if (checkBoxHexSend.Checked)
                {
                    //我们不管规则了。如果写错了一些，我们允许的，只用正则得到有效的十六进制数     
                    MatchCollection mc = Regex.Matches(tBoxSend.Text, @"(?i)[\da-f]{2}");
                    List<byte> buf = new List<byte>();//填充到这个临时列表中     
                    //依次添加到列表中     
                    foreach (Match m in mc)
                    {
                        buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
                    }
                    //转换列表为数组后发送     
                    serialPortTempColle.Write(buf.ToArray(), 0, buf.Count);
                    //记录发送的字节数     
                    n = buf.Count;
                }
                else//ascii编码直接发送     
                {
                    serialPortTempColle.Write(tBoxSend.Text);
                    n = tBoxSend.Text.Length;
                }
                send_count += n;//累加发送字节数     
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendData(string CommandData)
        {
            try
            {
                tBoxSend.Text = CommandData;
                //tBoxSend.AppendText(CommandData+"\r\n");
                //定义一个变量，记录发送了几个字节     
                int n = 0;
                //16进制发送     
                if (checkBoxHexSend.Checked)
                {
                    //我们不管规则了。如果写错了一些，我们允许的，只用正则得到有效的十六进制数     
                    MatchCollection mc = Regex.Matches(CommandData, @"(?i)[\da-f]{2}");
                    List<byte> buf = new List<byte>();//填充到这个临时列表中     
                    //依次添加到列表中     
                    foreach (Match m in mc)
                    {
                        buf.Add(byte.Parse(m.Value, System.Globalization.NumberStyles.HexNumber));
                    }
                    //转换列表为数组后发送     
                    serialPortTempColle.Write(buf.ToArray(), 0, buf.Count);
                    //记录发送的字节数     
                    n = buf.Count;

                }
                else//ascii编码直接发送     
                {
                    serialPortTempColle.Write(CommandData);
                    n = CommandData.Length;
                }
                send_count += n;//累加发送字节数     
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkBoxHexSend_CheckedChanged(object sender, EventArgs e)
        {
            if (!isHex)     //如果是十进制，则变成十六进制
            {
                tBoxSend.Text = StringToHex(tBoxSend.Text.Trim()).ToString(); //转换成16进制
                isHex = true;
                sbSTH.Clear();
            }
            else            //如果是十六进制，则变成十进制
            {
                tBoxSend.Text = HexToString(tBoxSend.Text.Trim()).ToString();
                isHex = false;
                sbHTS.Clear();
            }
        }

        /// <summary>
        /// 字符串转十六进制
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static StringBuilder StringToHex(string msg)
        {
            // string input = "Hello World!";
            char[] values = msg.ToCharArray();
            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                string hexOutput = String.Format("{0:X}", value);
                sbSTH.Append(hexOutput + " ");
            }
            return sbSTH;
        }
        /// <summary>
        /// 十六进制转字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static StringBuilder HexToString(string msg)
        {
            string[] hexValuesSplit = msg.Split(' ');
            foreach (String hex in hexValuesSplit)
            {
                int value = Convert.ToInt32(hex, 16);
                string stringValue = Char.ConvertFromUtf32(value);
                //char charValue = (char)value;
                sbHTS.Append(stringValue);

            }
            return sbHTS;
        }

        private void checkBoxHexSend_MouseDown(object sender, MouseEventArgs e)
        {
            if (tBoxSend.Text == string.Empty)
            {
                MessageBox.Show("发送框内容为空！", "提示");
                return;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < 65; i++)
            {
                DataSaveing.CreatButtonSetTextSection(Convert.ToString(i), Convert.ToString(i));
            }

            FormLoadButtonText();
        }
    }
}
