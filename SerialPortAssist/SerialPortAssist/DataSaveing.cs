using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortAssist
{
    class DataSaveing
    {
        private static IniFile _file;
        private static string Section="Command";
        private static string tempCommand;
        public static string LoadIniFile(string Number)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            _file = new IniFile(strPath + "Command.ini");
            tempCommand = _file.ReadString(Section, Number, "NULL");            //读返回的指令
            return tempCommand;
        }

        public static string LoadButtonText(string Number)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            _file = new IniFile(strPath + "Command.ini");
            tempCommand = _file.ReadString("ButtonText", Number, "NULL");            //读返回的指令
            return tempCommand;
        }

        public static void SaveButtonSetText(string Number, string Value)
        {
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory;
                _file = new IniFile(strPath + "Command.ini");
                _file.WriteString("ButtonText", Number, Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public static void CreatButtonSetTextSection(string Number, string Value)
        {
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory;
                _file = new IniFile(strPath + "Command.ini");
                _file.WriteString("ButtonText", Number, Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void SaveDate(string serialNumber, string Value)
        {
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory;
                _file = new IniFile(strPath + "Command.ini");
                _file.WriteString("Command", serialNumber, Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }

}
