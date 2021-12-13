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
        //Класс для симуляции действий мыши
        public class MouseOperations
        {
            [Flags]
            public enum MouseEventFlags
            {
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                Move = 0x00000001,
                Absolute = 0x00008000,
                RightDown = 0x00000008,
                RightUp = 0x00000010
            }

            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetCursorPos(int x, int y);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

            public static void SetCursorPosition(int x, int y)
            {
                SetCursorPos(x, y);
            }

            public static void SetCursorPosition(MousePoint point)
            {
                SetCursorPos(point.X, point.Y);
            }

            public static MousePoint GetCursorPosition()
            {
                MousePoint currentMousePoint;
                var gotPoint = GetCursorPos(out currentMousePoint);
                if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
                return currentMousePoint;
            }

            public static void MouseEvent(MouseEventFlags value,MousePoint mousePoint)
            {
                mouse_event
                    ((int)value,
                     mousePoint.X,
                     mousePoint.Y,
                     0,
                     0)
                    ;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MousePoint
            {
                public int X;
                public int Y;

                public MousePoint(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        //Для отслеживания ПКМ и ЛКМ и фиксации координат
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        bool isLButtonDown()
        {
            Int16 state = GetAsyncKeyState(Keys.LButton);
            return (state & 0x8000) != 0;
        }

        public int count_pos;
        IntPtr windows_Handle;
        string name_window;
        int timer1;
        int timer2;
        //Переменная типа MousePoint для запоминания нужного местоположения курсора
        MouseOperations.MousePoint lkm_mousepoint;
        MouseOperations.MousePoint pkm_mousepoint; 

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
            //Координаты для перемещений курсора мыши
            //int pkm_x = 63;
            //int pkm_y = 416;
            //int lkm_x = 126;
            //int lkm_y = 495;

            //Инициализируем значения элементов mousePoint из настроек приложения
            int pX = Properties.Settings.Default.par_pkm_x;
            int pY = Properties.Settings.Default.par_pkm_y;
            pkm_mousepoint = new MouseOperations.MousePoint(pX,pY);
            int lX = Properties.Settings.Default.par_lkm_x;
            int lY = Properties.Settings.Default.par_lkm_y;
            lkm_mousepoint = new MouseOperations.MousePoint(lX, lY);
            //Иницилизация длинны циклов из настроек программы
            textBox1.Text = Properties.Settings.Default.timer1.ToString();
            textBox2.Text = Properties.Settings.Default.timer2.ToString();
            textBox3.Text = Properties.Settings.Default.count_iter.ToString();


            //MessageBox.Show($"lX:{lX}, lY:{lY}, pX:{pX}, pY:{pY}");
            toolStripStatusLabel1.Text = ("Программа не активна");
            button1.Text = $"ЛКМ {lkm_mousepoint.X}:{lkm_mousepoint.Y}";
            button5.Text = $"ПКМ {pkm_mousepoint.X}:{pkm_mousepoint.Y}";
            //Получение списка всех процессов
            Process[] processlist = Process.GetProcesses();
            //Получение списка открытых окон и наполнение combobox'a
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    comboBox1.Items.Add(Convert.ToString(process.MainWindowTitle));
                }
            }
            //Предвыбор нужного окна
            //int index = comboBox1.FindString("Портал муниципальной");
            //comboBox1.SelectedIndex = index;
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
            //Очистка listBox3
            listBox3.Items.Clear();
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
                    if (this.listBox2.Items[i].ToString() == "-ЛКМ-")
                    {
                        this.listBox3.Items.Add(this.listBox2.Items[i].ToString());
                        MouseOperations.SetCursorPosition(lkm_mousepoint);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown | MouseOperations.MouseEventFlags.LeftUp, pkm_mousepoint);
                    }
                    if (this.listBox2.Items[i].ToString() == "-ПКМ-")
                    {
                        this.listBox3.Items.Add(this.listBox2.Items[i].ToString());
                        MouseOperations.SetCursorPosition(pkm_mousepoint);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown | MouseOperations.MouseEventFlags.RightUp, pkm_mousepoint);
                    }
                    Thread.Sleep(Convert.ToInt32(timer1));
                }
                else
                {
                    SendKeys.Send(this.listBox2.Items[i].ToString());
                    this.listBox3.Items.Add(this.listBox2.Items[i].ToString());
                    Thread.Sleep(Convert.ToInt32(timer1));
                }
                //если команда последняя, то не выполнять задержку потока
                if (i != this.listBox2.Items.Count - 1)
                {
                    //Задержка выполнения потока
                    //listBox3.Items.Add("Нажата клавиша" + this.listBox2.Items[i].ToString());
                    Thread.Sleep(Convert.ToInt32(timer1));
                }
                //Установка фокуса на только что добавленной строке
                listBox3.SelectedIndex = listBox3.Items.Count - 1;
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

        //Запуск отслеживания ЛКМ
        private void button1_Click_1(object sender, EventArgs e)
        {
            mouse_L_Timer.Start();
        }
        //Запуск отслеживания ПКМ
        private void button5_Click(object sender, EventArgs e)
        {
            mouse_R_Timer.Start();
        }

        //Таймер на отслеживание ЛКМ
        private void mouse_L_Timer_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = $"Начало фиксации координат ЛКМ";
            if (GetAsyncKeyState(Keys.LButton) != 0)
            {
                lkm_mousepoint = MouseOperations.GetCursorPosition();
                toolStripStatusLabel1.Text = $"Координаты ЛКМ зафиксированы: {lkm_mousepoint.X}:{lkm_mousepoint.Y}";
                Properties.Settings.Default.par_lkm_x = lkm_mousepoint.X;
                Properties.Settings.Default.par_lkm_y = lkm_mousepoint.Y;
                Properties.Settings.Default.Save();
                mouse_L_Timer.Stop();
            }
        }

        //Таймер на отслеживания ПКМ
        private void mouse_R_Timer_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = $"Начало фиксации координат ПКМ";

            if (GetAsyncKeyState(Keys.RButton) != 0)
            {
                pkm_mousepoint = MouseOperations.GetCursorPosition();
                toolStripStatusLabel1.Text = $"Координаты ПКМ зафиксированы: {pkm_mousepoint.X}:{pkm_mousepoint.Y}";
                Properties.Settings.Default.par_pkm_x = pkm_mousepoint.X;
                Properties.Settings.Default.par_pkm_y = pkm_mousepoint.Y;
                Properties.Settings.Default.Save();
                mouse_R_Timer.Stop();
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.timer1 = Convert.ToInt32(textBox1.Text);
            Properties.Settings.Default.Save();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.timer2 = Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.Save();
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.count_iter = Convert.ToInt32(textBox3.Text);
            Properties.Settings.Default.Save();
        }
    }
}
