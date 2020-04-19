using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawningControlSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //Сохдаем нашу канву.
            var canvas1 = new MyCanvas();

            //Клапдем ее в панель на окне (чтобы было удобно управлять ее размерами)
            this.pLeft.Controls.Add(canvas1);

            //Даем есть установку - всегда заполнять всю возможную область
            canvas1.Dock = DockStyle.Fill;
            
            



        }
    }


    //А вот наша канва, лучше в отдельном файле создавать подобное.
    public class MyCanvas : Control
    {
        //Таймер для ее обновления
        private Timer timer;

        //битмапы на которых будем рисовать в 2 слоя.
        //На первом будет само содержаение
        //На втором курсор.
        private Bitmap layer1;
        private Bitmap layer2;

        //Графиксы для этих битмапов
        private Graphics g_layer1;
        private Graphics g_layer2;

        private bool painting_mode = false;

        Pen pen = new Pen(Color.FromArgb(255, 0, 80, 0), 1);

        public MyCanvas()
        {
            //Включаем режим двойной буферизации, чтобы рисовка не мерцала.
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);

            //Опеределяем в нашей канве события
            this.Paint += MyCanvas_Paint;
            this.MouseDown += MyCanvas_MouseDown;
            this.MouseUp += MyCanvas_MouseUp;
            this.MouseMove += MyCanvas_MouseMove;
            
            this.SizeChanged += MyCanvas_SizeChanged;
            
            //Запускаем таймер на перерисовку
            timer = new Timer();
            timer.Interval=25;
            timer.Tick += (s, a) =>  this.Refresh();
            timer.Start();
        }


        ~MyCanvas()
        {
            
            if (g_layer1 != null)
                layer1.Dispose();
            if (g_layer2 != null)
                layer2.Dispose();

            if (layer1 != null)
                layer1.Dispose();
            if (layer2 != null)
                layer2.Dispose();
            
            timer.Dispose();
            pen.Dispose();
        }

        private void MyCanvas_SizeChanged(object sender, EventArgs e)
        {
            var _sender = sender as MyCanvas;

            //При изменении размера у нас должны пересоздатся битмапы (так как нельзя изменить
            // размер битмапа во время работы)
            //По этому мы сначала создаем новые, если старые есть (при создании конвы их нет,
            //вот тут они и создадутся при первом отображении) - рисуем их содержимое на новых, удаляем старые.

            Bitmap new_layer1 = new Bitmap(_sender.Size.Width, _sender.Size.Height, PixelFormat.Format32bppArgb);
            Bitmap new_layer2 = new Bitmap(_sender.Size.Width, _sender.Size.Height, PixelFormat.Format32bppArgb);
            Graphics new_g_layer1 = Graphics.FromImage(new_layer1);
            Graphics new_g_layer2 = Graphics.FromImage(new_layer2);

            if (g_layer1 != null)
            { 
                new_g_layer1.DrawImageUnscaled(layer1,0,0);
                layer1.Dispose();
            }
            if (layer1 != null)
                layer1.Dispose();

            
            if (g_layer2 != null)
                layer2.Dispose();
            if (layer2 != null)
                layer2.Dispose();

            layer1 = new_layer1;
            g_layer1 = new_g_layer1;
            layer2 = new_layer2;
            g_layer2 = new_g_layer2;


        }

        private void MyCanvas_Paint(object sender, PaintEventArgs e)
        {
            
            //Всегда рисуем зеленый кружок под мышкой, чтобы видеть как будет рисоватся линия.

            var mouse_pos = PointToClient(MousePosition);
            int r = 50;
            g_layer2.Clear(Color.FromArgb(0,0,0,0));
            g_layer2.DrawEllipse( pen,mouse_pos.X - r/2, mouse_pos.Y-r/2,r,r);

            e.Graphics.DrawImageUnscaled(layer1, 0, 0);
            e.Graphics.DrawImageUnscaled(layer2,0,0);
            
        }

        private void MyCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            //при отпускании ЛКМ отключаем режим рисования
            if (e.Button == MouseButtons.Left)
                painting_mode = false;
        }

        private void MyCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            //при нажании ЛКМ включаем режим рисования
            if (e.Button == MouseButtons.Left)
                painting_mode = true;
        }



        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //Если есть режим рисования, то нарисовать красный круг под мышкой.
            //ф-ция вызывается при движении мыши по канве
            if (painting_mode)
            {
                var mouse_pos = PointToClient(MousePosition);
                int r = 50;
                g_layer1.FillEllipse(Brushes.Red, mouse_pos.X - r / 2, mouse_pos.Y - r / 2, r, r);
            }

        }


    }
}
