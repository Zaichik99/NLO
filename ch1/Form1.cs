using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;


namespace ch1
{

    public partial class Form1 : Form
    {
        public const int N_max = 200;                // Максимальное количество НЛО на экране
        public Player player = new Player();         // Игрок, который сбивает НЛО (объект)
        public Boolean laser = false;                // Его оружие — бластер
        public Bitmap imageP;                        //  Изображения игрока
        public int Result = 0;                       // Количество сбитых НЛО (счет игры)
        public Graphics g;                           // холст для битвы
        public BrushColor bc = new BrushColor();     // набор кистей и цветов
        public Enemies nlo = new Enemies();          // Все НЛО

        //игрок
        public class Player
        {
            public Point point;              // положение игрока в 2D-пространстве
            public Size size;                // размеры игрока
            public Region reg;               // занимаемая им область в пространстве
            public Pen laser_pen;            // свойство оружия

            public void New_player(Form1 F)
            {
               
                size = F.imageP.Size; // ширина и высота кортинки
                point.X = 0;
                point.Y = 0;
                Rectangle rec = new Rectangle(point, size);
                reg = new Region(rec);
                laser_pen = new Pen(new HatchBrush(HatchStyle.DashedUpwardDiagonal,
                //Цвет луча лазера
                F.bc.LaserColor = Color.Red, F.bc.LaserColor), 1);
            }

            public void Show_player(Form1 F, int x, int y)
            {
                F.g.ResetClip();
                F.g.FillRegion(new SolidBrush(F.BackColor), reg);
                point.X = x - size.Width / 2;
                point.Y = y;
                Rectangle rec = new Rectangle(point, size);
                reg = new Region(rec);
                F.g.DrawImage(F.imageP, point);
                F.g.ExcludeClip(reg);
            }
        }

        //жучки
        public class Bugs
        {
            public Point point;                // положение НЛО в 2D-пространстве
            public Size size;                  // размеры НЛО
            int veloX;                         // скорость смещения по X
            int veloY;                         // скорость_падения по Y
            public HatchBrush br;              // кисть для покраски НЛО
            public Region reg = new Region();  // занимаемая им область в пространстве
            public Boolean life = true;        // НЛО жив (true) или мертв (false)

            public void New_bug(Form1 F, int rch)
            {
                Random rv = new Random(rch);
                point.X = rv.Next(10, Form1.ActiveForm.Width - 40);
                point.Y = rv.Next(10, Form1.ActiveForm.Height/5);
                size.Width = rv.Next(10, 30);
                size.Height = size.Width;// * 2/3;
                veloX = rv.Next(7) - 1;
                veloY = rv.Next(3, 10);
                br = F.bc.New_br(rch);
                reg = Form_bug();
            }

            public Region Form_bug()
            {
                Point pt = new Point();
                Size st = new Size();
                pt.X = point.X;
                pt.Y = point.Y + size.Height / 4;
                st.Width = size.Width;
                st.Height = size.Height / 2;
                Rectangle rec = new Rectangle(pt, st);
                GraphicsPath path1 = new GraphicsPath();
                path1.AddEllipse(rec);
                Region reg = new Region(path1);
                rec.X = point.X + size.Width / 4;
                rec.Y = point.Y;
                rec.Width = size.Width / 2;
                rec.Height = size.Height;
                path1.AddEllipse(rec);
                reg.Union(path1);
                return reg;
            }

            public void Move_bug()
            {
                point.X += veloX;
                point.Y += veloY;
                reg = Form_bug();
            }
        }

        //противник
        public class Enemies
        {
            public int Delta_N;                          // количество НЛО в серии
            public int N_generation;                     // число генераций — серий
            public int k_generation;                     // номер серии
            public int N;                                // актуальное количество НЛО на экране
            public Bugs[] bugs = new Bugs[Form1.N_max];  //массив  НЛО-объектов

            public void New_Enemies(Form1 F)
            {
                N_generation = 10;
                Delta_N = Form1.N_max / N_generation;
                k_generation = 0;
                N = 0;
                for (int j = 0; j < Form1.N_max; j++)
                    bugs[j] = new Bugs();
            }

            public int Select_bugs()
            {
                int k = 0;
                for (int j = 0; j < N; j++)
                {
                    if (!bugs[j].life)
                        k++;
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (!bugs[j].life)
                        {
                            for (int j1 = j; j1 < (N - 1); j1++)
                                bugs[j1] = bugs[j1 + 1];
                            break;
                        }
                    }
                    N--;
                }
                return k;    // счетчик подбитых НЛО
            }

            public void Show_bugs(Form1 F)
            {
                for (int j = 0; j < N; j++)
                {
                    bugs[j].Move_bug();
                    F.g.FillRegion(bugs[j].br, bugs[j].reg);
                }
            }

            public void Enemy(Form1 F)
            {
                int N0 = N;
                N = N + Delta_N;
                int rch;
                Random rnd = new Random();
                for (int j = N0; j < N; j++)
                {
                    bugs[j] = new Bugs();
                    rch = rnd.Next();
                    bugs[j].New_bug(F, rch);
                    F.g.FillRegion(bugs[j].br, bugs[j].reg);
                }
            }

            public void Killed_bugs(Form1 F, int x, int y)
            {
                for (int j = 0; j < N; j++)
                {
                    Rectangle r = new Rectangle(x - bugs[j].size.Width / 2, 0, bugs[j].size.Width, y);
                    if (bugs[j].reg.IsVisible(r, F.g) & F.laser)
                    {
                        bugs[j].br = new HatchBrush(HatchStyle.DarkHorizontal, F.bc.KilledBug, F.bc.KilledBug);
                        F.g.FillRegion(bugs[j].br, bugs[j].reg);
                        bugs[j].life = false;
                    }
                }
            }
        }

        public class BrushColor
        {
            public Color FonColor;             // цвет фона
            public Color LaserColor;           // цвет лазера
            public Color DashBug;              // цвет штриховки НЛО
            public Color KilledBug;            // цвет сбитого НЛО

            public HatchBrush New_br(int rch)
            {
                return new HatchBrush(HatchStyle.DashedUpwardDiagonal, DashBug, RandomColor(rch));
            }

            //цвет НЛО
            public Color RandomColor(int rch)      // rch - случайное число
            {
                int r, g, b;
                byte[] bytes1 = new byte[3];        // массив 3 цветов
                Random rnd1 = new Random(rch);
                rnd1.NextBytes(bytes1);             // генерация в массив
                r = Convert.ToInt16(bytes1[0]);
                g = Convert.ToInt16(bytes1[1]);
                b = Convert.ToInt16(bytes1[2]);
                return Color.FromArgb(r, g, b);     // возврат цвета
            }
        }
               
        public Form1()
        {
            InitializeComponent();
        }

        //при загрузке формы
        private void Form1_Load(object sender, EventArgs e)
        {
           
            g = this.CreateGraphics();          // инициализация холста
            //BackColor = bc.FonColor;            // цвет фона
            BackColor = Color.Black;            // цвет фона
            imageP = new Bitmap(imageList1.Images[0], 50, 50);  //размер самолета
            player.New_player(this);            // инициализация игрока
            nlo = new Enemies();                // инициализация противника
            nlo.New_Enemies(this);              // инициализация НЛО как объектов
        }

        //старт игры
        private void button1_Click(object sender, EventArgs e)
        {
            nlo.k_generation = 0;
            nlo.Enemy(this);
            timer1.Start();
            timer2.Start();
            button3.Enabled = true;
            button1.Enabled = false;
        }

        //включение/отключение лазера
        private void button2_Click(object sender, EventArgs e)
        {
            if (laser)
            {
                laser = false;
                button2.Text = "Включить Лазер";
            }
            else
            {
                laser = true;
                button2.Text = "Отключить Лазер";
            }
        }

        //стоп игры, результат
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            imageP = new Bitmap(imageList1.Images[1], 100, 100);
            int procent = Result * 100 / (nlo.Delta_N * nlo.N_generation);
            string msg = "Подбито " + Result.ToString() + " НЛО, " + procent.ToString() + "% результат";
            MessageBox.Show(msg, "Ваш результат", MessageBoxButtons.OK);
            player.Show_player(this, 50, 50);
            nlo.N = 0;
            button1.Enabled = true;
            Result = 0;
            textBox1.Text = Result.ToString();
        }

        //один временной такт игры
        private void timer1_Tick(object sender, EventArgs e)
        {
            g.Clear(BackColor);
            Result = Result + nlo.Select_bugs();
            nlo.Show_bugs(this);
            textBox1.Text = Result.ToString();
        }

        //генерация серий
        private void timer2_Tick(object sender, EventArgs e)
        {
            nlo.k_generation++;
            timer2.Interval -= 100;
            if (nlo.k_generation < nlo.N_generation)
                nlo.Enemy(this);
            else
                timer2.Stop();
        }

        //попадание НЛО под вертикальный обстрел лазером
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            player.Show_player(this, e.X, e.Y);
            if (laser)
                g.DrawLine(player.laser_pen, player.point.X + player.size.Width / 2, player.point.Y, player.point.X + player.size.Width / 2, 0);
            nlo.Killed_bugs(this, e.X, e.Y);
        }
    }
}
