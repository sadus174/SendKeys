using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace send_keys
{
    public partial class MainForm : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public int count_pos;
        IntPtr windows_Handle;
        string name_window;

        //Использование в качестве разделителя целой и дробной части , и .
        public int set_dec_separation (string value)
        {         
            float millisecond = Single.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture) * 1000;
            return Convert.ToInt32(millisecond);
        }

        //Инициализация формы и ее объектов
        public MainForm()
        {
            TopMost = true;
            InitializeComponent();
        }
        
   
   
        
        //Загрузка формы. Установление статуса и полей значениями по умолчанию. Так же получение списка открытых окон
        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Location = new Point(0, 980);

            Process[] processlist = Process.GetProcesses();

            //Получение списка открытых окон и наполнение combobox'a
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    comboBox1.Items.Add(Convert.ToString(process.MainWindowTitle));
                }
            }

            int index = comboBox1.FindString("Портал муниципальной");
            comboBox1.SelectedIndex = index;
        }
        
        //Закрытие формы
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        //Выбор целевого окна для симуляции
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            name_window = comboBox1.SelectedItem.ToString();
        }
        



        private void button4_Click(object sender, EventArgs e)
        {
            name_window = comboBox1.SelectedItem.ToString();
            windows_Handle = FindWindow(null, name_window);
            // Проверка на существование окна
            if (windows_Handle == IntPtr.Zero)
            {
                return;
            }
            else
            {
                //MessageBox.Show("Handle выбранного окна " + windows_Handle.ToString());
                SetForegroundWindow(windows_Handle);
                SendKeys.SendWait("{F5}");
            }

            




        }
    }
}
