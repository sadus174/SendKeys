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
        int timer1;
        int timer2;

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
            toolStripStatusLabel1.Text = ("Программа не активна");

            //this.FormBorderStyle = FormBorderStyle.None;
            //this.Location = new Point(0, 980);

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
               
        //Выбор целевого окна для симуляции
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            name_window = comboBox1.SelectedItem.ToString();
        }

        //Выбор клавишь из списка для симуляции
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected_keys = listBox1.SelectedItem.ToString();
            listBox2.Items.Add(selected_keys);
        }
        //Выбор клавишь из списка для симуляции
        private void button1_Click(object sender, EventArgs e)
        {
            string selected_keys = listBox1.SelectedItem.ToString();
            listBox2.Items.Add(selected_keys);
        }

        //Запуск симуляции
        private void button3_Click_1(object sender, EventArgs e)
        {
            //Задержка между отправками клавиш
            timer1 = set_dec_separation(textBox1.Text);
            //Задержка между командами
            timer2 = set_dec_separation(textBox2.Text);
            //Установка величины задержки между отправками команды (в миллисекундах)
            //MessageBox.Show("Сработка таймера через "+timer2.ToString()+" миллесекунд.");
            timer2_command.Interval = timer2;
            //Количество посещений для снятия
            count_pos = Convert.ToInt32(textBox3.Text);
            //ProgressBar
            progressbar1.Value = 0;
            progressbar1.Maximum = count_pos;
            // Получение handle окна по его имени из combobox
            windows_Handle = FindWindow(null, name_window);
            // Проверка на существование окна
            if (windows_Handle == IntPtr.Zero)
            {
                toolStripStatusLabel1.Text = "Приложение " + name_window + " не найдено, выполнение остановлено.";
                return;
            }
            else
            {
                toolStripStatusLabel1.Text = "Приложение " + name_window + " найдено, Начато выполнение.";
                //MessageBox.Show("Handle выбранного окна " + windows_Handle.ToString());
                SetForegroundWindow(windows_Handle);
                //Запуск таймера
                button3.Enabled = false;
                timer2_command.Enabled = true;
            }
        }

        //Метод для симуляции нажатия клавишь и формирования задержки между тактами и циклами
        private void timer2_command_Tick(object sender, EventArgs e)
        {
            //Перебор списка симуляционных клавиш
            for (int i = 0; i < this.listBox2.Items.Count; i++)
            {
                if ((this.listBox2.Items[i].ToString() == "-ЛКМ-") || (this.listBox2.Items[i].ToString() == "-ПКМ-"))
                {
                    SendKeys.Send("Должна быть нажата какая кнопка мыши");
                    Thread.Sleep(Convert.ToInt32(timer1));

                }
                else
                {
                    SendKeys.Send(this.listBox2.Items[i].ToString());
                    this.listBox3.Items.Add(this.listBox2.Items[i].ToString());  
                }
                //если команда последняя, то не выполнять задержку потока
                if (i != this.listBox2.Items.Count - 1)
                {
                    //Задержка выполнения потока
                    //listBox3.Items.Add("Нажата клавиша" + this.listBox2.Items[i].ToString());
                    Thread.Sleep(Convert.ToInt32(timer1));
                }

            }
            //Количество оставшихся итераций
            count_pos = count_pos - 1;
            //Выполнение кода определённое количество раз, в случае превышения количества - остановка таймера
            if (count_pos != 0)
            {
                toolStripStatusLabel1.Text = "Программа активена. Осталось итераций: " + Convert.ToString(count_pos);
                progressbar1.Value = progressbar1.Maximum - count_pos;
            }
            else
            {
                timer2_command.Enabled = false;
                button3.Enabled = true;
                toolStripStatusLabel1.Text = ("Программа не активна");
                progressbar1.Value = progressbar1.Maximum;
            }
        }

        //Остановка таймера
        private void button2_Click(object sender, EventArgs e)
        {
            timer2_command.Enabled = false;
            button3.Enabled = true;
            toolStripStatusLabel1.Text = ("Программа не активна");
            progressbar1.Value = 0;
        }

        //Удаление клавишь из назначения симуляции
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            else
                toolStripStatusLabel1.Text = ("Выберите элемент");
        }
    }
}
