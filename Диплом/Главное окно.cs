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


        //By FREEM:
        //Избавимся и от этих листов:
        //public List<Lsi> Cij;
        //public List<Lsi> NewCij;
        public Lsi[] Cij;
        public Lsi[] NewCij;

        public double C;
        public double Gamma;
        public double Phi;
        public double U;
        public double CellSize;

        public double W1;
        public double W2;
        public double W3;
        public double W4;
        public double W5;
        public double W6;
        public double W7;
        public double W8;
        public double W9;

        private bool _roulStart;
        private bool _paintStart;
        private bool _start;

        public int N;
        private int _steps;
        private double _time;
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
        private double _maxConcentationForPaint;
        private double _maxConcentationForFunc;
        public Color Clr;

        private  Настройка _properties;
        public ПрофилиКонцентраций ProfileConc;

        public class Lsi
        {
            public Lsi(Lsj[] l1)
            {
                Lj = l1;
            }
            public Lsj[] Lj;
        }

        public class Lsj
        {
            public Lsj(ConcentrationNl dConcNl1)
            {
                ConcNl = dConcNl1;
            }
            public Color Clr;
            public int Wall;
            public double Delta;
            public double ConcX;
            public ConcentrationNl ConcNl;
            public List<DataPoint> AlConcentration=new List<DataPoint>();
        }

        //функции
        private void SetListZero()
        {
            Cij     = new Lsi[N];
            NewCij  = new Lsi[N];

            for (var i = 0; i < N; i++)
            {
                Cij[ i ]    = new Lsi( new Lsj[ N ] );
                NewCij[ i ] = new Lsi( new Lsj[ N ] );
                for (var j = 0; j < N; j++)
                {
                    //By FREEM:
                    //В связи со сменой листа -> массив меняем вызов конструктора:
                    Cij[ i ].Lj[j] = new Lsj( new ConcentrationNl(new double[3]{ 0, 0, 0 } ) );
                    //Cij[i].Lj.Add(new Lsj(new ConcentrationNl(new List<PhazConcentration>
                    //                        {
                    //                            //By FREEM:
                    //                            //И тут сносим:
                    //                            //new PhazConcentration(0, "Nizkie"),
                    //                            //new PhazConcentration(0, "sred"),
                    //                            //new PhazConcentration(0, "Visok")
                    //                            new PhazConcentration(0),
                    //                            new PhazConcentration(0),
                    //                            new PhazConcentration(0)
                    //                        }
                    //               )));
                    //By FREEM:
                    //В связи со сменой листа -> массив меняем вызов конструктора:
                    NewCij[ i ].Lj[ j ] = new Lsj( new ConcentrationNl( new double[ 3 ] { 0, 0, 0 } ) );
                    //NewCij[i].Lj.Add(new Lsj(new ConcentrationNl(new List<PhazConcentration>
                    //                        {
                    //                            //By FREEM:
                    //                            //ну и тут:
                    //                            //new PhazConcentration(0, "Nizkie"),
                    //                            //new PhazConcentration(0, "sred"),
                    //                            //new PhazConcentration(0, "Visok")
                    //                            new PhazConcentration(0),
                    //                            new PhazConcentration(0),
                    //                            new PhazConcentration(0)
                    //                        }
                    //               )));
                }
            }
        }
        private double GetSum()
        {
            return  Cij.Sum(x => x.Lj.Sum(y => y.ConcNl.Conc));
        }
        public double Round(double valueToRound, int count)
        {
            var d = (double)Math.Pow(10, count);
            valueToRound = valueToRound*d;
            var celValue = Math.Truncate(valueToRound);
            return (valueToRound - celValue) >= .5f ? (double)(Math.Ceiling(valueToRound)/d) : (double)(celValue/d);
        }
       //отрисовка
        public void PaintHand(int x1, int y1, int x2, int y2)
        {
            if (_rightMouse)
            {
                try
                {
                    var delta = Convert.ToDouble(textBox1.Text);
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
                            var t = Convert.ToDouble(8 - Cij[j - 1].Lj[i - 1].Wall -
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
                double pointcount = 0;
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
            //for (var i = 0; i < N; i++)
            System.Threading.Mutex mt = new System.Threading.Mutex();
            Parallel.For(0, N, (i, loopState) =>
                {
                    for (var j = 0; j < N; j++)
                    {
                        if (Cij[i].Lj[j].Wall == 1) gr.FillRectangle(new SolidBrush(Cij[i].Lj[j].Clr), j, i, 1, 1);
                        else
                        {
                            if (Cij[i].Lj[j].ConcNl.Conc == 0) continue;
                            var y = Convert.ToInt32(Round(Cij[i].Lj[j].ConcNl.Conc * 255 / _maxConcentationForPaint, 0));
                            mt.WaitOne();
                            gr.FillRectangle(new SolidBrush(Color.FromArgb(y, Clr)), j, i, 1, 1);
                            mt.ReleaseMutex();
                        }
                    }
                });
            pictureBox1.Invalidate();
        }

        //четкая логика
        public double RuleDifWind(int i, int j)
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
            //By FREEM:
            //Сносим стринги, т.к. хранить кучу одинаковых стрингов не нужно
            //public PhazConcentration(double stepenPrinadl1, string s1)
            public PhazConcentration(double stepenPrinadl1)
            {
                StepenPrinadl = stepenPrinadl1;
                //NameFuncPrinadl = s1;
            }
            public double StepenPrinadl;
            //public string NameFuncPrinadl;
        }

        public class ConcentrationNl
        {
            //By FREEM:
            //Замена листа обычным массивом:            
            //public ConcentrationNl(List<PhazConcentration> phazConc1)
            public ConcentrationNl(double[] phazConc1)
            {
                PhazConc = phazConc1;
            }
            public double Conc;
            public double DePhazConc;
            //public List<PhazConcentration> PhazConc;
            public double[] PhazConc;
        }
        //средние максимумы
        public double WNizkie(double m)
        {
            var p = _maxConcentationForFunc / 2;
            return p - m * p;
        }
        public double WSrednie1(double m)
        {
            var p = _maxConcentationForFunc / 4;
            return m * p + p;
        }
        public double WSrednie2(double m)
        {
            var p1 = 3 * _maxConcentationForFunc / 4;
            var p2 = _maxConcentationForFunc / 4;
            return p1 - m * p2;
        }
        public double WVisokie(double m)
        {
            var p = _maxConcentationForFunc / 2;
            return m * p + p;
        }
        //МЦТ
        public double WNizkieX(double x)
        {
            var p = _maxConcentationForFunc / 2;
            return x >= p || x < 0 ? 0 : 1 - x / p;
        }
        public double WSrednieX(double x)
        {
            var p1 = _maxConcentationForFunc / 4;
            var p2 = _maxConcentationForFunc / 2;
            var p3 = 3 * _maxConcentationForFunc / 4;
            return x <= p1 || x >= p3 ? 0 : (x > p1 && x <= p2 ? x / p1 - 1 : 3 - x / p1);
        }
        public double WVisokieX(double x)
        {
            var p = _maxConcentationForFunc / 2;
            return x <= p ? 0 : (x >= _maxConcentationForFunc ? 1 : x / p - 1);
        }

        //фазификация
        //public List<PhazConcentration> PhazConc(double conc)
        //{
        //    return new List<PhazConcentration>
        //                   {
        //                       //By FREEM:
        //                       //Тут тоже сносим лишние стринги:
        //                       //new PhazConcentration(WNizkieX(conc), "Nizkie"),
        //                       //new PhazConcentration(WSrednieX(conc), "sred"),
        //                       //new PhazConcentration(WVisokieX(conc), "Visok")
        //                       new PhazConcentration(WNizkieX(conc)),
        //                       new PhazConcentration(WSrednieX(conc)),
        //                       new PhazConcentration(WVisokieX(conc))
        //                   };
        //}

        public double[] PhazConc( double conc ) {
            return new double[ 3 ] { WNizkieX( conc ), WSrednieX( conc ), WVisokieX( conc ) };
        }

        //дефазификация
        public double MItog(double x, double m1, double m2, double m3)
        {
            var listM = new List<double> { Math.Min(WNizkieX(x), m1), Math.Min(WSrednieX(x), m2), Math.Min(WVisokieX(x), m3) };
            return listM.Max(y => y);
        }
        public double SimpsonX(double a, double b, double m1, double m2, double m3)
        {
            const double delta = (double)0.1;
            double fxi = 0;
            double fxi1 = 0;
            double fxixi1 = 0;
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
        public double SimpsonXx(double a, double b, double m1, double m2, double m3)
        {
            const double delta = (double)0.1;
            double fxi = 0;
            double fxi1 = 0;
            double fxixi1 = 0;
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

        //By FREEM:
        //Меняем конструктор тк List -> double[3]
        //public double DePhazConcSrMax(List<PhazConcentration> listphazconc, int funcPrin)
        public double DePhazConcSrMax( double[] listphazconc, int funcPrin )
        {
            //By FREEM:
            //В связи с переходом от листа объектов к массиву меняем обращение:
            //if (funcPrin == 0)  return (0 + WNizkie(listphazconc[0].StepenPrinadl))/2;
            //if (funcPrin == 1) return (WSrednie1(listphazconc[1].StepenPrinadl) +
            //                             WSrednie2(listphazconc[1].StepenPrinadl)) / 2;
            //if (funcPrin == 2) return (WVisokie(listphazconc[2].StepenPrinadl) + 1) / 2;
            if ( funcPrin == 0 ) return ( 0 + WNizkie( listphazconc[ 0 ] ) ) / 2;
            if ( funcPrin == 1 ) return ( WSrednie1( listphazconc[ 1 ] ) +
                                           WSrednie2( listphazconc[ 1 ] ) ) / 2;
            if ( funcPrin == 2 ) return ( WVisokie( listphazconc[ 2 ] ) + 1 ) / 2;
            return 0;
        }

        //By FREEM:
        //Меняем конструктор тк List -> double[3]
        //public double DePhazConcMct(List<PhazConcentration> listphazconc)
        public double DePhazConcMct( double[] listphazconc )
        {
            var znamenat = SimpsonX( 0, 20, listphazconc[0], listphazconc[1], listphazconc[2] );
            return znamenat == 0 ? 0 : SimpsonXx( 0, 20, listphazconc[0], listphazconc[1], listphazconc[2] )/znamenat;
        }


        //правило нечеткого автомата
        //By FREEM меняем шапку:
        //public List<PhazConcentration> RuleDifWindNl(int i, int j)//void
        public double[] RuleDifWindNl( int i, int j )//void
        {
            //сумма double'ов = double. Заменяем динамическое определеине типа:
            //var sumW = W1 + W2 + W3 + W4 + W6 + W7 + W8 + W9;
            double sumW = W1 + W2 + W3 + W4 + W6 + W7 + W8 + W9;
            //By FREEM:
            //Меняем тк List -> double[3]
            //сумма double'ов = double. Заменяем динамическое определеине типа:
            double nizkie = ( 
                             Cij[ i - 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 0 ] * W1 + Cij[ i - 1 ].Lj[ j ].ConcNl.PhazConc[ 0 ] * W2 + Cij[ i - 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 0 ] * W3 +
                             Cij[ i ].Lj[ j - 1 ].ConcNl.PhazConc[ 0 ] * W4 + Cij[ i ].Lj[ j + 1 ].ConcNl.PhazConc[ 0 ] * W6 + Cij[ i + 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 0 ] * W7 + 
                             Cij[ i + 1 ].Lj[ j ].ConcNl.PhazConc[ 0 ] * W8 + Cij[ i + 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 0 ] * W9 
                             ) / sumW;
            double srednie = ( 
                             Cij[ i - 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 1 ] * W1 + Cij[ i - 1 ].Lj[ j ].ConcNl.PhazConc[ 1 ] * W2 + Cij[ i - 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 1 ] * W3 +
                             Cij[ i ].Lj[ j - 1 ].ConcNl.PhazConc[ 1 ] * W4 + Cij[ i ].Lj[ j + 1 ].ConcNl.PhazConc[ 1 ] * W6 + Cij[ i + 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 1 ] * W7 +
                             Cij[ i + 1 ].Lj[ j ].ConcNl.PhazConc[ 1 ] * W8 + Cij[ i + 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 1 ] * W9 
                             ) / sumW;
            double visokie = ( 
                             Cij[ i - 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 2 ] * W1 + Cij[ i - 1 ].Lj[ j ].ConcNl.PhazConc[ 2 ] * W2 + Cij[ i - 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 2 ] * W3 +
                             Cij[ i ].Lj[ j - 1 ].ConcNl.PhazConc[ 2 ] * W4 + Cij[ i ].Lj[ j + 1 ].ConcNl.PhazConc[ 2 ] * W6 + Cij[ i + 1 ].Lj[ j - 1 ].ConcNl.PhazConc[ 2 ] * W7 +
                             Cij[ i + 1 ].Lj[ j ].ConcNl.PhazConc[ 2 ] * W8 + Cij[ i + 1 ].Lj[ j + 1 ].ConcNl.PhazConc[ 2 ] * W9 
                             ) / sumW;
            //var nizkie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W1 +
            //             Cij[i - 1].Lj[j].ConcNl.PhazConc[0].StepenPrinadl*W2 +
            //             Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W3 +
            //             Cij[i].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W4 +
            //             Cij[i].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W6 +
            //             Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[0].StepenPrinadl*W7 +
            //             Cij[i + 1].Lj[j].ConcNl.PhazConc[0].StepenPrinadl*W8 +
            //             Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[0].StepenPrinadl*W9) / sumW;
            //var srednie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W1 +
            //             Cij[i - 1].Lj[j].ConcNl.PhazConc[1].StepenPrinadl * W2 +
            //             Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W3 +
            //             Cij[i].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W4 +
            //             Cij[i].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W6 +
            //             Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[1].StepenPrinadl * W7 +
            //             Cij[i + 1].Lj[j].ConcNl.PhazConc[1].StepenPrinadl * W8 +
            //             Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[1].StepenPrinadl * W9) / sumW;
            //var visokie = (Cij[i - 1].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W1 +
            //             Cij[i - 1].Lj[j].ConcNl.PhazConc[2].StepenPrinadl * W2 +
            //             Cij[i - 1].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W3 +
            //             Cij[i].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W4 +
            //             Cij[i].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W6 +
            //             Cij[i + 1].Lj[j - 1].ConcNl.PhazConc[2].StepenPrinadl * W7 +
            //             Cij[i + 1].Lj[j].ConcNl.PhazConc[2].StepenPrinadl * W8 +
            //             Cij[i + 1].Lj[j + 1].ConcNl.PhazConc[2].StepenPrinadl * W9) / sumW;

            //By FREEM:
            //Т.к работаем с массивом - меняем return:
            //return new List<PhazConcentration>
            //           {
            //               //By FREEM:
            //               //Тут тоже сносим лишние стринги:
            //               //new PhazConcentration(nizkie, "Nizkie"),
            //               //new PhazConcentration(srednie, "sred"),
            //               //new PhazConcentration(visokie, "Visok")   
            //               new PhazConcentration(nizkie),
            //               new PhazConcentration(srednie),
            //               new PhazConcentration(visokie)
            //           };
            return new double[ 3 ] { nizkie, srednie, visokie };
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

                C = (double)Convert.ToDouble(_properties.textBox3.Text);

                pictureBox1.Image = new Bitmap(Image.FromFile(_properties.label17.Text), pictureBox1.Width,
                                                  pictureBox1.Height);

                U = Convert.ToDouble( _properties.textBox17.Text );

                W1 = Convert.ToDouble( _properties.textBox6.Text );
                W2 = Convert.ToDouble( _properties.textBox7.Text );
                W3 = Convert.ToDouble( _properties.textBox8.Text );
                W4 = Convert.ToDouble( _properties.textBox9.Text );
                W5 = Convert.ToDouble( _properties.textBox10.Text );
                W6 = Convert.ToDouble( _properties.textBox11.Text );
                W7 = Convert.ToDouble( _properties.textBox12.Text );
                W8 = Convert.ToDouble( _properties.textBox13.Text );
                W9 = Convert.ToDouble( _properties.textBox14.Text );

                //Поправка коэффициента разбавления за счет скорости ветра
                Gamma = Convert.ToDouble( _properties.textBox4.Text ) * ( 1.001111 - 0.002222 * U );
                Phi = Convert.ToDouble( _properties.textBox19.Text );

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
           
            ParallelOptions ops = new ParallelOptions();
            ops.MaxDegreeOfParallelism = 64;

            // *********** MAXIM **************
            PutTestingPoint();
            // ********************************

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
                    double sumc = 0;
                    double sumcnew = 0;
                    
                    //фазификация
                    //Parallel.For(0, N, (i, loopState) =>
                    /// -------------------------------------------------------
                    Parallel.For( 0, N, ( i, loopState ) =>
                    {
                        double conc;
                        for (var j = 0; j < N; j++)
                        {
                            conc = Cij[ i ].Lj[ j ].ConcNl.Conc;
                            if (conc == 0)
                            {
                                //By FREEM:
                                //Cij[i].Lj[j].ConcNl.PhazConc[0].StepenPrinadl = 1;
                                //Cij[i].Lj[j].ConcNl.PhazConc[1].StepenPrinadl = 0;
                                //Cij[i].Lj[j].ConcNl.PhazConc[2].StepenPrinadl = 0;
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 0 ] = 1;
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 1 ] = 0;
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 2 ] = 0;
                            }
                            else
                            {          
                                //By FREEM:
                                //Cij[i].Lj[j].ConcNl.PhazConc[0].StepenPrinadl = WNizkieX(conc);
                                //Cij[i].Lj[j].ConcNl.PhazConc[1].StepenPrinadl = WSrednieX(conc);
                                //Cij[i].Lj[j].ConcNl.PhazConc[2].StepenPrinadl = WVisokieX(conc);
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 0 ] = WNizkieX( conc );
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 1 ] = WSrednieX( conc );
                                Cij[ i ].Lj[ j ].ConcNl.PhazConc[ 2 ] = WVisokieX( conc );
                                sumc += conc;
                            }
                        }
                    });

                    //поле нечетких концентраций с учетом коэффициентов w
                    Parallel.For(1, N - 1, ops, (i, loopState) =>
                    {
                   // for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            NewCij[i].Lj[j].ConcNl.PhazConc = RuleDifWindNl(i, j);//lll
                        }
                    });

                    //дефазификация
                    Parallel.For(1, N - 1, ops, (i, loopState) =>
                    //for (var i = 1; i < N - 1; i++)
                    {
                        for (var j = 1; j < N - 1; j++)
                        {
                            //By FREEM:
                            //if (NewCij[i].Lj[j].ConcNl.PhazConc[0].StepenPrinadl == 1 &&
                            //    NewCij[i].Lj[j].ConcNl.PhazConc[1].StepenPrinadl == 0 &&
                            //    NewCij[i].Lj[j].ConcNl.PhazConc[2].StepenPrinadl == 0)
                            //{
                            //    NewCij[i].Lj[j].ConcNl.DePhazConc = 0;
                            //}
                            //else
                            //{
                            //    var i1 =
                            //        NewCij[i].Lj[j].ConcNl.PhazConc.FindIndex(
                            //            x => x.StepenPrinadl == NewCij[i].Lj[j].ConcNl.PhazConc.Max(z => z.StepenPrinadl));
                            //    NewCij[i].Lj[j].ConcNl.DePhazConc = NewCij[i].Lj[j].ConcNl.PhazConc[i1].StepenPrinadl >=
                            //                                       (double)0.8
                            //                                           ? DePhazConcSrMax(
                            //                                               NewCij[i].Lj[j].ConcNl.PhazConc, i1)
                            //                                           : DePhazConcMct(NewCij[i].Lj[j].ConcNl.PhazConc);
                            //}

                            

                            if ( NewCij[ i ].Lj[ j ].ConcNl.PhazConc[ 0 ] == 1 && NewCij[ i ].Lj[ j ].ConcNl.PhazConc[ 1 ] == 0 && NewCij[ i ].Lj[ j ].ConcNl.PhazConc[ 2 ]== 0 ) {
                                NewCij[ i ].Lj[ j ].ConcNl.DePhazConc = 0;
                            } else {
                                //Переписываем запрос, т.к. для массива он невозможен:
                                int i1 = 0;
                                double mx = NewCij[i].Lj[j].ConcNl.PhazConc.Max();
                                for (int f = 0; f < 3; f++ ){
                                    if ( NewCij[ i ].Lj[ j ].ConcNl.PhazConc[f] == mx ){
                                        i1 = f;
                                        break;
                                    }
                                }                                     
                                NewCij[ i ].Lj[ j ].ConcNl.DePhazConc = NewCij[ i ].Lj[ j ].ConcNl.PhazConc[ i1 ] >=
                                                                   ( double )0.8
                                                                       ? DePhazConcSrMax(
                                                                           NewCij[ i ].Lj[ j ].ConcNl.PhazConc, i1 )
                                                                       : DePhazConcMct( NewCij[ i ].Lj[ j ].ConcNl.PhazConc );
                            }
                        }
                    });
                    //коэффициент фи
                    //Не параллелить пока:
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            NewCij[i].Lj[j].ConcNl.Conc = (1 - Phi)*Cij[i].Lj[j].ConcNl.Conc +
                                                         Phi*NewCij[i].Lj[j].ConcNl.DePhazConc;
                            sumcnew += NewCij[i].Lj[j].ConcNl.Conc;
                        }
                    
                    var sumw = W1 + W2 + W3 + W4 + W6 + W7 + W8 + W9;
                    //нормировка
                    //Не параллелить:
                    for (var i = 1; i < N - 1; i++)
                        for (var j = 1; j < N - 1; j++)
                        {
                            if (NewCij[i].Lj[j].ConcNl.Conc == 0)
                            {
                                Cij[i].Lj[j].ConcNl.Conc = 0;
                            }
                            else
                            {
                                //Копирование в старый массив:
                                Cij[i].Lj[j].ConcNl.Conc = sumcnew != 0
                                                              ? Round(NewCij[i].Lj[j].ConcNl.Conc * sumc / sumcnew, _round)
                                                              : 0;
                            }
                        }
                    //коэффициент, учитывающий уменьшение концентрации
                    //Не параллелить:
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
                    //Не параллелить:
                           for (var i = 0; i < N; i++)
                                for (var j = 0; j < N; j++)
                                {
                                    if (NewCij[i].Lj[j].ConcX == 0 && Cij[i].Lj[j].ConcNl.Conc == 0) continue;
                                    Cij[i].Lj[j].ConcNl.Conc += NewCij[i].Lj[j].ConcX;
                                    if (Cij[i].Lj[j].ConcNl.Conc > _maxConcentationForPaint)
                                        _maxConcentationForPaint = Cij[i].Lj[j].ConcNl.Conc;
                                    NewCij[i].Lj[j].ConcX = 0;
                                }

                    //-----------------------------------------------------------
                    _maxConcentationForFunc = _maxConcentationForPaint * 2;
                    label2.Text = Round(GetSum(), _round).ToString();
                    _steps++;
                    _time += Round((double)0.5 / U /4, 2);//коэффициент 4 учитывает положение облака через час при 1 метре в сек
                    label5.Text = _time.ToString();
                    //сценарий
                    if ( _properties.checkBox1.Checked && _time >= Convert.ToDouble( _properties.dataGridView1.Rows[ _scenariyK ].Cells[ 0 ].Value ) && 
                        _scenariyK < _properties.dataGridView1.RowCount - 1)
                    {
                        U = Convert.ToDouble( _properties.dataGridView1.Rows[ _scenariyK ].Cells[ 2 ].Value );

                        var omega = Convert.ToInt32(_properties.dataGridView1.Rows[_scenariyK].Cells[1].Value);
                        var listW = _properties.GetW(U, omega);

                        W1 = Convert.ToDouble( listW[ 0 ].W );
                        W2 = Convert.ToDouble( listW[ 1 ].W );
                        W3 = Convert.ToDouble( listW[ 2 ].W );
                        W4 = Convert.ToDouble( listW[ 3 ].W );
                        W5 = Convert.ToDouble( listW[ 4 ].W );
                        W6 = Convert.ToDouble( listW[ 5 ].W );
                        W7 = Convert.ToDouble( listW[ 6 ].W );
                        W8 = Convert.ToDouble( listW[ 7 ].W );
                        W9 = Convert.ToDouble( listW[ 8 ].W );

                        //Поправка коэффициента разбавления за счет скорости ветра
                        Gamma = Convert.ToDouble( _properties.textBox4.Text ) *
                                ((double) 1.001111 - (double) 0.002222*U);

                        _scenariyK++;
                    }
                    //Parallel.For(0, N , ops, (i, loopState) =>
                    //    {
                            for (var i = 0; i < N; i++)
                                for (var j = 0; j < N; j++)
                                {
                                    if (Cij[i].Lj[j].ConcNl.Conc > 0)
                                    {
                                        Cij[i].Lj[j].AlConcentration.Add(new DataPoint(Convert.ToDouble(Round(_time, 4)),
                                                                                       Convert.ToDouble(Cij[i].Lj[j].ConcNl.Conc)));
                                    }
                                }
                    //    });
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
            System.Diagnostics.Trace.WriteLine("time :" + time);
            label5.Text = time;
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
                var maxX=new List<double>();
                var maxY = new List<double>();
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
                    ProfileConc.label7.Text += " X = " + e.X * CellSize + " Y = " + e.Y * CellSize + " : C = " + Round( Convert.ToDouble( Cij[ e.Y ].Lj[ e.X ].AlConcentration[ i1 ].YValues[ 0 ] * 47208 ), 2 ) + " мг/м3" + " в момент времени Т = " + Round( Convert.ToDouble( Cij[ e.Y ].Lj[ e.X ].AlConcentration[ i1 ].XValue ), 2 ) + " мин";
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
            // ****************** MAXIM ************************
            System.Diagnostics.Trace.WriteLine(Cij[15].Lj[20].ConcNl.Conc);
        }

        // ***************** MAXIM **********************
        void PutTestingPoint()
        {
            Cij[50].Lj[50].ConcNl.Conc = 100;
            _roulStart = true;
        }
    }  
}