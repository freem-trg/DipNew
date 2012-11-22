using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading.Tasks;


namespace Диплом
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _roulStart = false;//
            _paintStart = false;//
            _start = false;
            Clr = Color.Red;//
            _maxConcentationForPaint = 0;//
            _time = 0;
        }

        public List<Lsi> Cij;
        public List<Lsi> NewCij;

        public decimal C;
        public decimal Gamma;
        public decimal Phi;
        public decimal U;
        public double CellSize;

        public decimal W1;
        public decimal W2;
        public decimal W3;
        public decimal W4;
        public decimal W5;
        public decimal W6;
        public decimal W7;
        public decimal W8;
        public decimal W9;

        private bool _roulStart;
        private bool _paintStart;
        private bool _start;

        public int N;
        private int _steps;
        private decimal _time;
        private int _round;
        private int _scenariyK;

        public Graphics Gr;
        private int _minX;
        private int _minY;
        private int _maxX;
        private int _maxY;
        private bool _leftMouse;
        private bool _rightMouse;
        public Point StartPaintXy;
        private List<Point> _pointForWall;
        private decimal _maxConcentationForPaint;
        private decimal _maxConcentationForFunc;
        public Color Clr;

        private  Настройка _properties;
        public ПрофилиКонцентраций ProfileConc;

        public class Lsi
        {
            public Lsi(IEnumerable<Lsj> l1)
            {
                Lj = new List<Lsj>(l1);
            }
            public List<Lsj> Lj;
        }
        public class Lsj
        {
            public Lsj(ConcentrationNl dConcNl1)
            {
                ConcNl = dConcNl1;
            }
            public Color Clr;
            public int Wall;
            public decimal Delta;
            public decimal ConcX;
            public ConcentrationNl ConcNl;
            public List<DataPoint> AlConcentration=new List<DataPoint>();
        }

        //функции
        private void SetListZero()
        {
            Cij = new List<Lsi>();
            NewCij = new List<Lsi>();
            for (var i = 0; i < N; i++)
            {
                Cij.Add(new Lsi(new List<Lsj>()));
                NewCij.Add(new Lsi(new List<Lsj>()));
                for (var j = 0; j < N; j++)
                {
                    Cij[i].Lj.Add(new Lsj(new ConcentrationNl(new List<PhazConcentration>
                                            {
                                                new PhazConcentration(0, "Nizkie"),
                                                new PhazConcentration(0, "sred"),
                                                new PhazConcentration(0, "Visok")
                                            }
                                   )));
                   NewCij[i].Lj.Add(new Lsj(new ConcentrationNl(new List<PhazConcentration>
                                            {
                                                new PhazConcentration(0, "Nizkie"),
                                                new PhazConcentration(0, "sred"),
                                                new PhazConcentration(0, "Visok")
                                            }
                                   )));
                }
            }
        }
        private decimal GetSum()
        {
            return  Cij.Sum(x => x.Lj.Sum(y => y.ConcNl.Conc));
        }
        public decimal Round(decimal valueToRound, int count)
        {
            var d = (decimal)Math.Pow(10, count);
            valueToRound = valueToRound*d;
            var celValue = Math.Truncate(valueToRound);
            return (valueToRound - celValue) >= (decimal) .5 ? Math.Ceiling(valueToRound)/d : celValue/d;
        }
       //отрисовка
        public void PaintHand(int x1, int y1, int x2, int y2)
        {
            if (_rightMouse)
            {
                try
                {
                    var delta = Convert.ToDecimal(textBox1.Text);
                    for (var i = x1; i <= x2; i++)
                        for (var j = y1; j <= y2; j++)
                        {
                            if ((((((Bitmap) pictureBox1.Image).GetPixel(i, j).A != Clr.A) ||
                                  (((Bitmap) pictureBox1.Image).GetPixel(i, j).B != Clr.B)) ||
                                 (((Bitmap) pictureBox1.Image).GetPixel(i, j).G != Clr.G)) ||
                                (((Bitmap) pictureBox1.Image).GetPixel(i, j).R != Clr.R)) continue;
                            if (_rightMouse)
                                Cij[j].Lj[i].Wall = 1;
                        }
                    for (var i = x1; i <= x2; i++)
                        for (var j = y1; j <= y2; j++)
                        {
                            var t = Convert.ToDecimal(8 - Cij[j - 1].Lj[i - 1].Wall -
                                                      Cij[j - 1].Lj[i].Wall -
                                                      Cij[j - 1].Lj[i + 1].Wall -
                                                      Cij[j].Lj[i - 1].Wall -
                                                      Cij[j].Lj[i + 1].Wall -
                                                      Cij[j + 1].Lj[i - 1].Wall -
                                                      Cij[j + 1].Lj[i].Wall -
                                                      Cij[j + 1].Lj[i + 1].Wall) / 8;
                            if (t == 0) t = 1;
                            Cij[j].Lj[i].Delta = delta * t;
                            Cij[j].Lj[i].ConcNl.Conc = 0;

                            Cij[j].Lj[i].Clr = Clr;
                        }
                }
                catch (Exception)
                {
                    MessageBox.Show("Коэффициент Delta задан неверно,");
                }  
            }
            if (_leftMouse)
            {
                decimal pointcount = 0;
                for (var i = x1; i <= x2; i++)
                    for (var j = y1; j <= y2; j++)
                    {
                        if ((((((Bitmap)pictureBox1.Image).GetPixel(i, j).A != Clr.A) ||
                              (((Bitmap)pictureBox1.Image).GetPixel(i, j).B != Clr.B)) ||
                             (((Bitmap)pictureBox1.Image).GetPixel(i, j).G != Clr.G)) ||
                            (((Bitmap)pictureBox1.Image).GetPixel(i, j).R != Clr.R)) continue;

                            Cij[j].Lj[i].ConcNl.Conc = -1;
                            pointcount++;
                    }
                if (pointcount != 0)
                {
                    pointcount = C / pointcount;
                    if (_maxConcentationForPaint < pointcount)
                    {
                        _maxConcentationForPaint = pointcount;
                        _maxConcentationForFunc = _maxConcentationForPaint * 2;
                    }
                }
                for (var i = x1; i <= x2; i++)
                    for (var j = y1; j <= y2; j++)
                    {
                        if (Cij[j].Lj[i].ConcNl.Conc != -1) continue;
                        if (_leftMouse)
                        {
                            Cij[j].Lj[i].ConcNl.Conc = pointcount;
                        }
                    }
            }
        }
        public void PaintDifWind(Graphics gr)
        {
            for (var i = 0; i < N; i++)
                for (var j = 0; j < N; j++)
                {
                        if (Cij[i].Lj[j].Wall == 1) gr.FillRectangle(new SolidBrush(Cij[i].Lj[j].Clr), j, i, 1, 1);
                        else
                        {
                            if (Cij[i].Lj[j].ConcNl.Conc == 0) continue;
                            var y = Convert.ToInt32(Round(Cij[i].Lj[j].ConcNl.Conc * 255 / _maxConcentationForPaint, 0));
                            gr.FillRectangle(new SolidBrush(Color.FromArgb(y, Clr)), j, i, 1, 1);
                        }
                }
            pictureBox1.Invalidate();
        }

        //четкая логика
        public decimal RuleDifWind(int i, int j)
        {
            if (Cij[i].Lj[j].Wall == 1)
            {
                var x = 1 - Cij[i].Lj[j].Delta;
                NewCij[i - 1].Lj[j - 1].ConcNl.Conc += Cij[i - 1].Lj[j - 1].ConcNl.Conc * x * W1;
                NewCij[i - 1].Lj[j].ConcNl.Conc += Cij[i - 1].Lj[j].ConcNl.Conc * x * W2;
                NewCij[i - 1].Lj[j + 1].ConcNl.Conc += Cij[i - 1].Lj[j + 1].ConcNl.Conc * x * W3;
                NewCij[i].Lj[j - 1].ConcNl.Conc += Cij[i].Lj[j - 1].ConcNl.Conc * x * W4;
                NewCij[i].Lj[j + 1].ConcNl.Conc += Cij[i].Lj[j + 1].ConcNl.Conc * x * W6;
                NewCij[i + 1].Lj[j - 1].ConcNl.Conc += Cij[i + 1].Lj[j - 1].ConcNl.Conc * x * W7;
                NewCij[i + 1].Lj[j].ConcNl.Conc += Cij[i + 1].Lj[j].ConcNl.Conc * x * W8;
                NewCij[i + 1].Lj[j + 1].ConcNl.Conc += Cij[i + 1].Lj[j + 1].ConcNl.Conc * x * W9;
                return (Cij[i - 1].Lj[j - 1].ConcNl.Conc * W1 + Cij[i - 1].Lj[j].ConcNl.Conc * W2 + Cij[i - 1].Lj[j + 1].ConcNl.Conc * W3 +
                            Cij[i].Lj[j - 1].ConcNl.Conc * W4 + Cij[i].Lj[j + 1].ConcNl.Conc * W6
                            + Cij[i + 1].Lj[j - 1].ConcNl.Conc * W7 + Cij[i + 1].Lj[j].ConcNl.Conc * W8 + Cij[i + 1].Lj[j + 1].ConcNl.Conc * W9) * Cij[i].Lj[j].Delta + Cij[i].Lj[j].ConcNl.Conc * W5;
            }
            return Cij[i - 1].Lj[j - 1].ConcNl.Conc * W1 + Cij[i - 1].Lj[j].ConcNl.Conc * W2 + Cij[i - 1].Lj[j + 1].ConcNl.Conc * W3 +
                    Cij[i].Lj[j - 1].ConcNl.Conc * W4 + Cij[i].Lj[j].ConcNl.Conc * W5 + Cij[i].Lj[j + 1].ConcNl.Conc * W6
                    + Cij[i + 1].Lj[j - 1].ConcNl.Conc * W7 + Cij[i + 1].Lj[j].ConcNl.Conc * W8 + Cij[i + 1].Lj[j + 1].ConcNl.Conc * W9;
        }

        //Нечеткая логика
        public class PhazConcentration
        {
            public PhazConcentration(decimal stepenPrinadl1, string s1)
            {
                StepenPrinadl = stepenPrinadl1;
                NameFuncPrinadl = s1;
            }
            public decimal StepenPrinadl;
            public string NameFuncPrinadl;
        }
        public class ConcentrationNl
        {
            public ConcentrationNl(List<PhazConcentration> phazConc1)
            {
                PhazConc = phazConc1;
            }
            public decimal Conc;
            public decimal DePhazConc;
            public List<PhazConcentration> PhazConc;
        }
        //средние максимумы
        public decimal WNizkie(decimal m)
        {
            var p = _maxConcentationForFunc / 2;
            return p - m * p;
        }
        public decimal WSrednie1(decimal m)
        {
            var p = _maxConcentationForFunc / 4;
            return m * p + p;
        }
        public decimal WSrednie2(decimal m)
        {
            var p1 = 3 * _maxConcentationForFunc / 4;
            var p2 = _maxConcentationForFunc / 4;
            return p1 - m * p2;
        }
        public decimal WVisokie(decimal m)
        {
            var p = _maxConcentationForFunc / 2;
            return m * p + p;
        }
        //МЦТ
        public decimal WNizkieX(decimal x)
        {
            var p = _maxConcentationForFunc / 2;
            return x >= p || x < 0 ? 0 : 1 - x / p;
        }
        public decimal WSrednieX(decimal x)
        {
            var p1 = _maxConcentationForFunc / 4;
            var p2 = _maxConcentationForFunc / 2;
            var p3 = 3 * _maxConcentationForFunc / 4;
            return x <= p1 || x >= p3 ? 0 : (x > p1 && x <= p2 ? x / p1 - 1 : 3 - x / p1);
        }
        public decimal WVisokieX(decimal x)
        {
            var p = _maxConcentationForFunc / 2;
            return x <= p ? 0 : (x >= _maxConcentationForFunc ? 1 : x / p - 1);
        }
        //фазификация
        public List<PhazConcentration> PhazConc(decimal conc)
        {
            return new List<PhazConcentration>
                           {
                               new PhazConcentration(WNizkieX(conc), "Nizkie"),
                               new PhazConcentration(WSrednieX(conc), "sred"),
                               new PhazConcentration(WVisokieX(conc), "Visok")
                           };
        }
        //дефазификация
        public decimal MItog(decimal x, decimal m1, decimal m2, decimal m3)
        {
            var listM = new List<decimal> { Math.Min(WNizkieX(x), m1), Math.Min(WSrednieX(x), m2), Math.Min(WVisokieX(x), m3) };
            return listM.Max(y => y);
        }
        public decimal SimpsonX(decimal a, decimal b, decimal m1, decimal m2, decimal m3)
        {
            const decimal delta = (decimal)0.1;
            decimal fxi = 0;
            decimal fxi1 = 0;
            decimal fxixi1 = 0;
            var xi = a;
            var xi1 = a + delta;
            while (xi1 <= b)
            {
                fxi += MItog(xi, m1, m2, m3);
                fxi1 += MItog(xi1, m1, m2, m3);
                fxixi1 += MItog((xi + xi1) / 2, m1, m2, m3);
                xi = xi1;
                xi1 += delta;
            }
            return delta / 6 * (fxi + fxi1 + 4 * fxixi1);
        }
        public decimal SimpsonXx(decimal a, decimal b, decimal m1, decimal m2, decimal m3)
        {
            const decimal delta = (decimal)0.1;
            decimal fxi = 0;
            decimal fxi1 = 0;
            decimal fxixi1 = 0;
            var xi = a;
            var xi1 = a + delta;
            while (xi1 <= b)
            {
                fxi += MItog(xi, m1, m2, m3) * xi;
                fxi1 += MItog(xi1, m1, m2, m3) * xi1;
                fxixi1 += MItog((xi + xi1) / 2, m1, m2, m3) * (xi + xi1) / 2;
                xi = xi1;
                xi1 += delta;
            }
            return delta / 6 * (fxi + fxi1 + 4 * fxixi1);
        }
        public decimal DePhazConcSrMax(List<PhazConcentration> listphazconc, int funcPrin)
        {
            if (funcPrin == 0)  return (0 + WNizkie(listphazconc[0].StepenPrinadl))/2;
            if (funcPrin == 1) return (WSrednie1(listphazconc[1].StepenPrinadl) +
                                         WSrednie2(listphazconc[1].StepenPrinadl)) / 2;
            if (funcPrin == 2) return (WVisokie(listphazconc[2].StepenPrinadl) + 1) / 2;
            return 0;
        }
        public decimal DePhazConcMct(List<PhazConcentration> listphazconc)
        {
            var znamenat = SimpsonX(0, 20, listphazconc[0].StepenPrinadl, listphazconc[1].StepenPrinadl,
                                        listphazconc[2].StepenPrinadl);
            return znamenat == 0 ? 0 : SimpsonXx(0, 20, listphazconc[0].StepenPrinadl, listphazconc[1].StepenPrinadl,
                                       listphazconc[2].StepenPrinadl)/znamenat;
        }
        //правило нечеткого автомата
        public List<PhazConcentration> RuleDifWindNl(int i, int j)
        {
            var sumW = W1 + W2 + W3 + W4 + W6 + W7 + W8 + W9;
            var nizkie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W1 +
                         Cij[i - 1].Lj[j].ConcNl.PhazConc[0].StepenPrinadl*W2 +
                         Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W3 +
                         Cij[i].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W4 +
                         Cij[i].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W6 +
                         Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W7 +
                         Cij[i + 1].Lj[j].ConcNl.PhazConc[0].StepenPrinadl*W8 +
                         Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W9) / sumW;
            var srednie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W1 +
                         Cij[i - 1].Lj[j].ConcNl.PhazConc[1].StepenPrinadl * W2 +
                         Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W3 +
                         Cij[i].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W4 +
                         Cij[i].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W6 +
                         Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W7 +
                         Cij[i + 1].Lj[j].ConcNl.PhazConc[1].StepenPrinadl * W8 +
                         Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W9) / sumW;
            var visokie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W1 +
                         Cij[i - 1].Lj[j].ConcNl.PhazConc[2].StepenPrinadl * W2 +
                         Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W3 +
                         Cij[i].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W4 +
                         Cij[i].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W6 +
                         Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W7 +
                         Cij[i + 1].Lj[j].ConcNl.PhazConc[2].StepenPrinadl * W8 +
                         Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W9) / sumW;
            return new List<PhazConcentration>
                       {
                           new PhazConcentration(nizkie, "Nizkie"),
                           new PhazConcentration(srednie, "sred"),
                           new PhazConcentration(visokie, "Visok")
                       };
        }
        //Properties
        private void button3_Click(object sender, EventArgs e)
        {
            _properties=new Настройка();
            _properties.ShowDialog();
            try
            {
                if (_properties.Ok && N != Convert.ToInt32(_properties.textBox1.Text))
                {
                    N = Convert.ToInt32(_properties.textBox1.Text);
                    pictureBox1.Height = N;
                    pictureBox1.Width = N;
                    panel1.Height = N + 4;
                    panel1.Width = N + 4;
                    SetListZero();
                    _steps = 0;
                    _maxConcentationForPaint = 0;
                    label5.Text = _steps.ToString(); //time
                    label2.Text = GetSum().ToString();

                    _paintStart = true;
                }

                CellSize = Convert.ToDouble(_properties.textBox18.Text);

                C = Convert.ToDecimal(_properties.textBox3.Text);

                pictureBox1.Image = new Bitmap(Image.FromFile(_properties.label17.Text), pictureBox1.Width,
                                                  pictureBox1.Height);

                U = Convert.ToDecimal(_properties.textBox17.Text);

                W1 = Convert.ToDecimal(_properties.textBox6.Text);
                W2 = Convert.ToDecimal(_properties.textBox7.Text);
                W3 = Convert.ToDecimal(_properties.textBox8.Text);
                W4 = Convert.ToDecimal(_properties.textBox9.Text);
                W5 = Convert.ToDecimal(_properties.textBox10.Text);
                W6 = Convert.ToDecimal(_properties.textBox11.Text);
                W7 = Convert.ToDecimal(_properties.textBox12.Text);
                W8 = Convert.ToDecimal(_properties.textBox13.Text);
                W9 = Convert.ToDecimal(_properties.textBox14.Text);

                //Поправка коэффициента разбавления за счет скорости ветра
                Gamma = Convert.ToDecimal(_properties.textBox4.Text)*((decimal)1.001111-(decimal)0.002222*U);
                Phi = Convert.ToDecimal(_properties.textBox19.Text);

                //Delta = Convert.ToDecimal(_properties.textBox5.Text);
                _round = Convert.ToInt32(_properties.textBox15.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Настройки заданы неверно");
            }
        }
        //clear
        private void button2_Click(object sender, EventArgs e)
        {
            if (Cij == null) return;
            SetListZero();
            pictureBox1.Image = new Bitmap(Image.FromFile(_properties.label17.Text), pictureBox1.Width, pictureBox1.Height);
            _steps = 0;
            _time = 0;
            _scenariyK = 0;
            label5.Text = _steps.ToString(); //time
            label2.Text = GetSum().ToString();
            _maxConcentationForPaint = 0;
            _paintStart = true;
        }
        //start
        private void button1_Click(object sender, EventArgs e)
        {
            //Для замера времени:
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            // Задаем переменные для параллелизма:
           // System.Threading.ThreadPool.SetMinThreads(128, 128);
           
           // ParallelOptions ops = new ParallelOptions();
           // ops.MaxDegreeOfParallelism = 64;
            
            if (_roulStart)
            {
                _start = !_start;
                _paintStart = !_paintStart;
                Bitmap bmp = new Bitmap(Image.FromFile(_properties.label17.Text), pictureBox1.Width,
                                                   pictureBox1.Height);
                while (_start)
                {
                    sw.Start();
                    pictureBox1.Image = bmp;
                    Gr = Graphics.FromImage(pictureBox1.Image);
                    decimal sumc = 0;
                    decimal sumcnew = 0;

                    //фазификация
                    //Parallel.For(0, N, (i, loopState) =>
                    /// -------------------------------------------------------
                    //Parallel.For( 0, N, ops, (i, loopState) =>
                    for (var i = 0; i < N; i++)
                    //{
                        for (var j = 0; j < N; j++)
                        {
                            if (Cij[i].Lj[j].ConcNl.Conc == 0)
                            {
                                Cij[i].Lj[j].ConcNl.PhazConc[0].StepenPrinadl = 1;
                                Cij[i].Lj[j].ConcNl.PhazConc[1].StepenPrinadl = 0;
                                Cij[i].Lj[j].ConcNl.PhazConc[2].StepenPrinadl = 0;
                            }
                            else
                            {
                                Cij[i].Lj[j].ConcNl.PhazConc = PhazConc(Cij[i].Lj[j].ConcNl.Conc);
                                sumc += Cij[i].Lj[j].ConcNl.Conc;
                            }
                        }
                    //});

                    //поле нечетких концентраций с учетом коэффициентов w
                    //Parallel.For(1, N - 1, ops, (i, loopState) =>
                    //{
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            NewCij[i].Lj[j].ConcNl.PhazConc = RuleDifWindNl(i, j);
                        }
                    //});

                    //дефазификация
                    //Parallel.For(1, N - 1, ops, (i, loopState) =>
                    //for (var i = 1; i < N - 1; i++)
                    //{
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            if (NewCij[i].Lj[j].ConcNl.PhazConc[0].StepenPrinadl == 1 &&
                                NewCij[i].Lj[j].ConcNl.PhazConc[1].StepenPrinadl == 0 &&
                                NewCij[i].Lj[j].ConcNl.PhazConc[2].StepenPrinadl == 0)
                            {
                                NewCij[i].Lj[j].ConcNl.DePhazConc = 0;
                            }
                            else
                            {
                                var i1 =
                                    NewCij[i].Lj[j].ConcNl.PhazConc.FindIndex(
                                        x => x.StepenPrinadl == NewCij[i].Lj[j].ConcNl.PhazConc.Max(z => z.StepenPrinadl));
                                NewCij[i].Lj[j].ConcNl.DePhazConc = NewCij[i].Lj[j].ConcNl.PhazConc[i1].StepenPrinadl >=
                                                                   (decimal)0.8
                                                                       ? DePhazConcSrMax(
                                                                           NewCij[i].Lj[j].ConcNl.PhazConc, i1)
                                                                       : DePhazConcMct(NewCij[i].Lj[j].ConcNl.PhazConc);
                            }
                        }
                    //});
                    //коэффициент фи
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            NewCij[i].Lj[j].ConcNl.Conc = (1 - Phi)*Cij[i].Lj[j].ConcNl.Conc +
                                                         Phi*NewCij[i].Lj[j].ConcNl.DePhazConc;
                            sumcnew += NewCij[i].Lj[j].ConcNl.Conc;
                        }
                    var sumw = W1 + W2 + W3 + W4 + W6 + W7 + W8 + W9;
                    //нормировка
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            if (NewCij[i].Lj[j].ConcNl.Conc == 0)
                            {
                                Cij[i].Lj[j].ConcNl.Conc = 0;
                            }
                            else
                            {
                                Cij[i].Lj[j].ConcNl.Conc = sumcnew != 0
                                                              ? Round(NewCij[i].Lj[j].ConcNl.Conc*sumc/sumcnew, _round)
                                                              : 0;
                            }
                        }
                    //коэффициент, учитывающий уменьшение концентрации
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            Cij[i].Lj[j].ConcNl.Conc = Gamma * Cij[i].Lj[j].ConcNl.Conc; 
                        }
                   
                    //стена и преграды
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            if (Cij[i].Lj[j].Wall != 1) continue;
                            var x = (1 - Cij[i].Lj[j].Delta) * Cij[i].Lj[j].ConcNl.Conc;
                            NewCij[i - 1].Lj[j - 1].ConcX += x * W1 / sumw;
                            NewCij[i - 1].Lj[j].ConcX += x * W2 / sumw;
                            NewCij[i - 1].Lj[j + 1].ConcX += x * W3 / sumw;
                            NewCij[i].Lj[j - 1].ConcX += x * W4 / sumw;
                            NewCij[i].Lj[j + 1].ConcX += x * W6 / sumw;
                            NewCij[i + 1].Lj[j - 1].ConcX += x * W7 / sumw;
                            NewCij[i + 1].Lj[j].ConcX += x * W8 / sumw;
                            NewCij[i + 1].Lj[j + 1].ConcX += x * W9 / sumw;
                            NewCij[i].Lj[j].ConcX -= x;
                        }
                    _maxConcentationForPaint = 0;
                    for (var i = 0; i < N; i++)
                        for (var j = 0; j < N; j++)
                        {
                            if (NewCij[i].Lj[j].ConcX == 0 && Cij[i].Lj[j].ConcNl.Conc==0) continue;
                            Cij[i].Lj[j].ConcNl.Conc += NewCij[i].Lj[j].ConcX;
                            if (Cij[i].Lj[j].ConcNl.Conc > _maxConcentationForPaint)
                                _maxConcentationForPaint = Cij[i].Lj[j].ConcNl.Conc;
                            NewCij[i].Lj[j].ConcX = 0;
                        }
                    //-----------------------------------------------------------
                    _maxConcentationForFunc = _maxConcentationForPaint * 2;
                    label2.Text = Round(GetSum(), _round).ToString();
                    _steps++;
                    _time += Round((decimal)0.5 / U /4, 2);//коэффициент 4 учитывает положение облака через час при 1 метре в сек
                    label5.Text = _time.ToString();
                    //сценарий
                    if (_properties.checkBox1.Checked  && _time >= Convert.ToDecimal(_properties.dataGridView1.Rows[_scenariyK].Cells[0].Value) && 
                        _scenariyK < _properties.dataGridView1.RowCount - 1)
                    {
                        U = Convert.ToDecimal(_properties.dataGridView1.Rows[_scenariyK].Cells[2].Value);

                        var omega = Convert.ToInt32(_properties.dataGridView1.Rows[_scenariyK].Cells[1].Value);
                        var listW = _properties.GetW(U, omega);

                        W1 = Convert.ToDecimal(listW[0].W);
                        W2 = Convert.ToDecimal(listW[1].W);
                        W3 = Convert.ToDecimal(listW[2].W);
                        W4 = Convert.ToDecimal(listW[3].W);
                        W5 = Convert.ToDecimal(listW[4].W);
                        W6 = Convert.ToDecimal(listW[5].W);
                        W7 = Convert.ToDecimal(listW[6].W);
                        W8 = Convert.ToDecimal(listW[7].W);
                        W9 = Convert.ToDecimal(listW[8].W);

                        //Поправка коэффициента разбавления за счет скорости ветра
                        Gamma = Convert.ToDecimal(_properties.textBox4.Text)*
                                ((decimal) 1.001111 - (decimal) 0.002222*U);

                        _scenariyK++;
                    }
                    for (var i = 0; i < N; i++)
                        for (var j = 0; j < N; j++)
                        {
                            if (Cij[i].Lj[j].ConcNl.Conc > 0)
                            {
                                Cij[i].Lj[j].AlConcentration.Add(new DataPoint(Convert.ToDouble(Round(_time, 4)),
                                                                               Convert.ToDouble(Cij[i].Lj[j].ConcNl.Conc)));
                            }
                        }
                    PaintDifWind(Gr);
                    if (_time>= 5)
                    {
                        _start = !_start;
                        _paintStart = !_paintStart;
                    }
                Application.DoEvents();
            }
        }
            else    MessageBox.Show("Необходимо настроить параметры автомата. Настройка");
            sw.Stop();
            String time = sw.Elapsed.TotalSeconds.ToString();
        }
#region Paint
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_paintStart==false) return;
            StartPaintXy.X = e.X;
            StartPaintXy.Y = e.Y;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    _leftMouse = true;
                    _rightMouse = false;
                    break;
                case MouseButtons.Right:
                    _leftMouse = false;
                    _rightMouse = true;
                    _minX = e.X;
                    _minY = e.Y;
                    _maxX = e.X;
                    _maxY = e.Y;
                    _pointForWall = new List<Point>();
                    break;
            }
        }
#endregion
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.X >= N || e.Y >= N || e.X < 0 || e.Y < 0) return;
            if (checkBox1.Checked)
            {
                ProfileConc = new ПрофилиКонцентраций();
                ProfileConc.chart1.Series[0].Points.Clear();
                ProfileConc.chart1.Series[1].Points.Clear();
                var maxX=new List<decimal>();
                var maxY = new List<decimal>();
                for (var i = 0; i < N; i++)
                {
                    if (Cij[e.Y].Lj[i].ConcNl.Conc > 0)
                    {
                        ProfileConc.chart1.Series[0].Points.Add(new DataPoint(i * CellSize, Convert.ToDouble(Cij[e.Y].Lj[i].ConcNl.Conc * 47208)));//47208 для настройки концентраций
                        maxX.Add(Cij[e.Y].Lj[i].ConcNl.Conc * 47208);
                    }
                    if (Cij[i].Lj[e.X].ConcNl.Conc > 0)
                    {
                        ProfileConc.chart1.Series[1].Points.Add(new DataPoint(i * CellSize, Convert.ToDouble(Cij[i].Lj[e.X].ConcNl.Conc * 47208)));
                        maxY.Add(Cij[i].Lj[e.X].ConcNl.Conc * 47208);
                    }
                }
                if (maxX.Count>0)
                {
                    ProfileConc.label4.Text += " C = " + Round(maxX.Max(),2) + " мг/м3 в точке X = " + e.X * CellSize + " м";    
                }
                if (maxY.Count > 0)
                {
                    ProfileConc.label5.Text += " C = " + Round(maxY.Max(),2) + " мг/м3 в точке Y = " + e.Y*CellSize + " м";
                }
                if (Cij[e.Y].Lj[e.X].AlConcentration.Count>0)
                {
                    foreach (var v in Cij[e.Y].Lj[e.X].AlConcentration)
                    {
                        ProfileConc.chart3.Series[0].Points.Add(new DataPoint(v.XValue, v.YValues[0] * 47208));
                    }
                    var i1 = Cij[e.Y].Lj[e.X].AlConcentration.FindIndex(x => x.YValues[0] == Cij[e.Y].Lj[e.X].AlConcentration.Max(z => z.YValues[0]));
                    ProfileConc.label7.Text += " X = " + e.X * CellSize + " Y = " + e.Y * CellSize + " : C = " + Round(Convert.ToDecimal(Cij[e.Y].Lj[e.X].AlConcentration[i1].YValues[0] * 47208), 2) + " мг/м3" + " в момент времени Т = " + Round(Convert.ToDecimal(Cij[e.Y].Lj[e.X].AlConcentration[i1].XValue), 2) + " мин";
                }
                ProfileConc.ShowDialog();
            }
            else
            {
                Gr = Graphics.FromImage(pictureBox1.Image);
                if ((StartPaintXy.X == e.X && StartPaintXy.Y == e.Y))
                {
                    Gr.FillRectangle(new SolidBrush(Clr), StartPaintXy.X, StartPaintXy.Y, 1, 1);
                } 
                else
                {
                    if (_leftMouse)   Gr.FillEllipse(new SolidBrush(Clr), StartPaintXy.X, StartPaintXy.Y, e.X - StartPaintXy.X,
                                       e.Y - StartPaintXy.Y);
                }
                Gr.Dispose();
                pictureBox1.Invalidate();
                if (_leftMouse)     PaintHand(StartPaintXy.X, StartPaintXy.Y, e.X, e.Y);
                if (_rightMouse)      PaintHand(_minX, _minY, _maxX, _maxY);
                _rightMouse = false;
                _leftMouse = false;
                _roulStart = true;
                label2.Text = Round(GetSum(),_round).ToString();
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rightMouse && e.X < N && e.Y < N && e.X >= 0 && e.Y >= 0)
            {
                Gr = Graphics.FromImage(pictureBox1.Image);
                if (e.X < _minX)     _minX = e.X;
                if (e.Y < _minY)     _minY = e.Y;
                if (e.X > _maxX)     _maxX = e.X;
                if (e.Y > _maxY)     _maxY = e.Y;
                _pointForWall.Add(new Point(e.X, e.Y));
                if (_pointForWall.Count == 1) Gr.FillRectangle(new SolidBrush(Clr), e.X, e.Y, 1, 1);
                else
                {
                    var pt = _pointForWall.ToArray();
                    Gr.DrawCurve(new Pen(Clr), pt);
                    Gr.FillPolygon(new SolidBrush(Clr), pt);
                }
                Gr.Dispose();
                pictureBox1.Invalidate();
            }
            if (!_paintStart) return;
            label9.Text = (e.X*CellSize).ToString();
            label10.Text = (e.Y*CellSize).ToString();
            label6.Text = Round(Cij[e.Y].Lj[e.X].ConcNl.Conc, _round).ToString();
            label16.Text = Cij[e.Y].Lj[e.X].Delta.ToString();
        }
        //Выбор цвета
        private void label3_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            Clr = colorDialog1.Color;
            label3.BackColor = Clr;
        }
        //отрисовка по всему полю
        private void button5_Click(object sender, EventArgs e)
        {
            if (_start == false && _roulStart)
            {
            pictureBox1.Image = new Bitmap(Image.FromFile(_properties.label17.Text), pictureBox1.Width,pictureBox1.Height);
            Gr = Graphics.FromImage(pictureBox1.Image);

            const int zmin = 0;
            var zmax = _maxConcentationForPaint;
            var r4 = (zmax - zmin)/4;
            var z14 = zmin+r4;
            var z24 = z14+r4;
            var z34 = z24+r4;
                for (var i = 0; i < N; i++)
                for (var j = 0; j < N; j++)
                {
                    if (Cij[i].Lj[j].Wall == 1)
                    {
                        Gr.FillRectangle(new SolidBrush(Cij[i].Lj[j].Clr), j, i, 1, 1);
                    }
                    else
                    {
                        int t;
                        if (Cij[i].Lj[j].ConcNl.Conc > zmin && Cij[i].Lj[j].ConcNl.Conc < z14)
                        {
                            t = Convert.ToInt32(63 + 192 * (Cij[i].Lj[j].ConcNl.Conc - zmin) / r4);
                            Gr.FillRectangle(new SolidBrush(Color.FromArgb(0, t, 0)), j, i, 1, 1);
                        }
                        if (Cij[i].Lj[j].ConcNl.Conc >= z14 && Cij[i].Lj[j].ConcNl.Conc < z24)
                        {
                            t = Convert.ToInt32(255 * (Cij[i].Lj[j].ConcNl.Conc - z14) / r4);
                            Gr.FillRectangle(new SolidBrush(Color.FromArgb(t, 255, 0)), j, i, 1, 1);
                        }
                        if (Cij[i].Lj[j].ConcNl.Conc >= z24 && Cij[i].Lj[j].ConcNl.Conc < z34)
                        {
                            t = Convert.ToInt32(255 * (1 - (Cij[i].Lj[j].ConcNl.Conc - z24) / r4));
                            Gr.FillRectangle(new SolidBrush(Color.FromArgb(255, t, 0)), j, i, 1, 1);
                        }
                        if (Cij[i].Lj[j].ConcNl.Conc >= z34 && Cij[i].Lj[j].ConcNl.Conc <= zmax)
                        {
                            t = Convert.ToInt32(63 + 192 * (1 - (Cij[i].Lj[j].ConcNl.Conc - z34) / r4));
                            Gr.FillRectangle(new SolidBrush(Color.FromArgb(t, 0, 0)), j, i, 1, 1);
                        }
                    }
                }
            pictureBox1.Invalidate();
            }
        }
    }  
}