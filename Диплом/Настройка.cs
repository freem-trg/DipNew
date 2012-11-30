﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Диплом
{
    public partial class Настройка : Form
    {
        public Настройка()
        {
            InitializeComponent();
            Ok = false;
        }
        public bool Ok;

        public class PhazCoeffW
        {
            public PhazCoeffW(double membershipValue1, string fazzySetName1)
            {
                MembershipValue = membershipValue1;
                FazzySetName = fazzySetName1;
            }
            public double MembershipValue;
            public string FazzySetName;
        }
        public class WNl
        {
            public WNl(List<PhazCoeffW> phazW1)
            {
                PhazWw1 = phazW1;
            }
            public double DePhazW;
            public List<PhazCoeffW> PhazWw1;
            public double W;
        }
        public double Round(double valueToRound, int count)
        {
            var d = (double)Math.Pow(10, count);
            valueToRound = valueToRound * d;
            var celValue = Math.Truncate(valueToRound);
            return (valueToRound - celValue) >= 0.5 ? Math.Ceiling(valueToRound) / d : celValue / d;
        }
        //функции принадлежн угла
        public double AlphaMaliyX(double x)
        {
            return x >= 60 ? 0 : 1 - x / 60;
        }
        public double AlphaSredniyX(double x)
        {
            return x >= 135 ? 0 : (x <= 60 ? x / 60 : 1.8 - x / 75);
        }
        public double AlphaBolshoyX(double x)
        {
            return x <= 60 ? 0 : (x > 60 && x <= 135 ? x / 75 - 0.8 : 1);
        }
        //средние максимумы
        public double WMaliy(double m)
        {
            return 0.5 - m * 0.5;
        }
        public double WSredniy1(double m)
        {
            return m * 0.25 + 0.25;
        }
        public double WSredniy2(double m)
        {
            return 0.75 - m * 0.25;
        }
        public double WBolshoy(double m)
        {
            return m * 0.5 + 0.5;
        }
        //МЦТ
        public double WMaliyX(double x)
        {
            return x >= 0.4 ? 0 : 1 - x * 2.5;
        }
        public double WSredniyX(double x)
        {
            return x <= 0.2 || x >= 0.6 ? 0 :
                (x > 0.2 && x <= 0.4 ? x * 5 - 1 : 3 - x * 5);
        }
        public double WBolshoyX(double x)
        {
            return x < 0.4 ? 0 : x * 2.55 - 1;
        }
        //phazification
        public List<PhazCoeffW> PhazW(double alpha)
        {
            return new List<PhazCoeffW>
                           {
                               new PhazCoeffW(AlphaBolshoyX(alpha), "maliy"),
                               new PhazCoeffW(AlphaSredniyX(alpha), "sred"),
                               new PhazCoeffW(AlphaMaliyX(alpha), "bolshoy")
                           };
        }
        //dephazification
        public double MItogW(double x, double m1, double m2, double m3)
        {
            var listM = new List<double> { Math.Min(WMaliyX(x), m1), Math.Min(WSredniyX(x), m2), Math.Min(WBolshoyX(x), m3) };
            return listM.Max(y => y);
        }
        public double SimpsonXw(double a, double b, double m1, double m2, double m3)
        {
            const double delta = (double)0.01;
            double fxi = 0;
            double fxi1 = 0;
            double fxixi1 = 0;
            var xi = a;
            var xi1 = a + delta;
            while (xi1 <= b)
            {
                fxi += MItogW(xi, m1, m2, m3);
                fxi1 += MItogW(xi1, m1, m2, m3);
                fxixi1 += MItogW((xi + xi1) / 2, m1, m2, m3);
                xi = xi1;
                xi1 += delta;
            }
            return delta / 6 * (fxi + fxi1 + 4 * fxixi1);
        }
        public double SimpsonXxW(double a, double b, double m1, double m2, double m3)
        {
            const double delta = 0.001;
            double fxi = 0;
            double fxi1 = 0;
            double fxixi1 = 0;
            var xi = a;
            var xi1 = a + delta;
            while (xi1 <= b)
            {
                fxi += MItogW(xi, m1, m2, m3) * xi;
                fxi1 += MItogW(xi1, m1, m2, m3) * xi1;
                fxixi1 += MItogW((xi + xi1) / 2, m1, m2, m3) * (xi + xi1) / 2;
                xi = xi1;
                xi1 += delta;
            }
            return delta / 6 * (fxi + fxi1 + 4 * fxixi1);
        }
        public double DePhazWSrMax(WNl ls, int funcPrin)
        {
            if (funcPrin == 0)
            {
                return (0 + WMaliy(ls.PhazWw1[0].MembershipValue)) / 2;
            }
            if (funcPrin == 1)
            {
                return (WSredniy1(ls.PhazWw1[1].MembershipValue) + WSredniy2(ls.PhazWw1[1].MembershipValue)) / 2;
            }
            if (funcPrin == 2)
            {
                return (WBolshoy(ls.PhazWw1[2].MembershipValue) + 1) / 2;
            }
            return 0;
        }
        public double DePhazWMct(WNl ls)
        {
            var znam = SimpsonXw(0, 1, ls.PhazWw1[0].MembershipValue, ls.PhazWw1[1].MembershipValue,
                                       ls.PhazWw1[2].MembershipValue);
            return znam == 0 ? 0
                       : SimpsonXxW(0, 1, ls.PhazWw1[0].MembershipValue, ls.PhazWw1[1].MembershipValue,
                                      ls.PhazWw1[2].MembershipValue)/znam;
        }
        //поправка на ветер
        public double GetK(double alpha, double u)
        {
            if (alpha >= 120)
            {
                return 0;
            }
            var k = 2.145 - 2.81 * alpha + 1.05 * u + 0.946 * alpha * alpha - 0.079 * u * u - 0.444 * alpha * u;
            return k < 0 ? 0 : k;
        }
        public double GetAlpha(int gamma, int omega)
        {
            var alpha1 = Math.Abs(omega - gamma);
            return alpha1 > 180 ? 360 - alpha1 : alpha1;
        }
        //загрузка формы
        private void Настройка_Load(object sender, EventArgs e)
        {
                textBox1.Text = Properties.Settings.Default.N;

                textBox2.Text = Properties.Settings.Default.MapSize;
                textBox18.Text = Properties.Settings.Default.CellSize;

                label17.Text = Properties.Settings.Default.Map;
                //pictureBox1.Image = Image.FromFile(Properties.Settings.Default.Map);
                openFileDialog1.FileName = Properties.Settings.Default.Map;

                textBox3.Text = Properties.Settings.Default.C;

                textBox16.Text = Properties.Settings.Default.Gamma;
                textBox17.Text = Properties.Settings.Default.u;

                textBox6.Text = Properties.Settings.Default.w1;
                textBox7.Text = Properties.Settings.Default.w2;
                textBox8.Text = Properties.Settings.Default.w3;
                textBox9.Text = Properties.Settings.Default.w4;
                textBox10.Text = Properties.Settings.Default.w5;
                textBox11.Text = Properties.Settings.Default.w6;
                textBox12.Text = Properties.Settings.Default.w7;
                textBox13.Text = Properties.Settings.Default.w8;
                textBox14.Text = Properties.Settings.Default.w9;

                textBox4.Text = Properties.Settings.Default.Kv;
                textBox19.Text = Properties.Settings.Default.phi;

                textBox15.Text = Properties.Settings.Default.round;

                checkBox1.Checked = Properties.Settings.Default.ScenarCheck;

                var row = Properties.Settings.Default.Scenariy.Split('.');
                dataGridView1.RowCount = row.Count();
                for (var i = 0; i < row.Count(); i++)
                {
                    if (row[i] == "")
                        break;
                    var col = row[i].Split(' ');
                    dataGridView1.Rows[i].Cells[0].Value = col[0];
                    dataGridView1.Rows[i].Cells[1].Value = col[1];
                    dataGridView1.Rows[i].Cells[2].Value = col[2];
                }
        }
        //Загрузка карты
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            pictureBox1.Image = new Bitmap(Image.FromFile(openFileDialog1.FileName), pictureBox1.Width, pictureBox1.Height);
            label17.Text = openFileDialog1.FileName;//Невидимый для хранения пути к изображению
        }
        //Настройка ветра
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox16.Text = trackBar1.Value.ToString();
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            textBox17.Text = (trackBar2.Value / 10).ToString();
        }
        //
        public List<WNl> GetW(double u, int omega)
        {
            var listW = new List<WNl>();
            //фазификация
            var al1 = GetAlpha(315, omega);
            var al2 = GetAlpha(270, omega);
            var al3 = GetAlpha(225, omega);
            var al4 = GetAlpha(0, omega);
            var al6 = GetAlpha(180, omega);
            var al7 = GetAlpha(45, omega);
            var al8 = GetAlpha(90, omega);
            var al9 = GetAlpha(135, omega);

            listW.Add(new WNl(PhazW(al1)));//1
            listW.Add(new WNl(PhazW(al2)));//2
            listW.Add(new WNl(PhazW(al3)));//3
            listW.Add(new WNl(PhazW(al4)));//4
            listW.Add(new WNl(new List<PhazCoeffW> { new PhazCoeffW(0, "maliy"), new PhazCoeffW(0, "sred"), new PhazCoeffW(1, "bolshoy") })); //5
            listW.Add(new WNl(PhazW(al6))); //6
            listW.Add(new WNl(PhazW(al7)));//7
            listW.Add(new WNl(PhazW(al8)));//8
            listW.Add(new WNl(PhazW(al9)));//9
            //дефазификация
            foreach (var ls in listW)
            {
                var i = ls.PhazWw1.FindIndex(x => x.MembershipValue == ls.PhazWw1.Max(z => z.MembershipValue));
                ls.DePhazW = ls.PhazWw1[i].MembershipValue >= 0.8 ? DePhazWSrMax(ls, i) : DePhazWMct(ls);
            }

            listW[0].W = Round(listW[0].DePhazW * GetK(al1 / 180 * (double)Math.PI, u), 2);
            listW[1].W = Round(listW[1].DePhazW * GetK(al2 / 180 * (double)Math.PI, u), 2);
            listW[2].W = Round(listW[2].DePhazW * GetK(al3 / 180 * (double)Math.PI, u), 2);
            listW[3].W = Round(listW[3].DePhazW * GetK(al4 / 180 * (double)Math.PI, u), 2);

            listW[5].W = Round(listW[5].DePhazW * GetK(al6 / 180 * (double)Math.PI, u), 2);
            listW[6].W = Round(listW[6].DePhazW * GetK(al7 / 180 * (double)Math.PI, u), 2);
            listW[7].W = Round(listW[7].DePhazW * GetK(al8 / 180 * (double)Math.PI, u), 2);
            listW[8].W = Round(listW[8].DePhazW * GetK(al9 / 180 * (double)Math.PI, u), 2);

            listW[4].W = listW.Max(x => x.W);
            return listW;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox16.Text.CompareTo("") == 0) return;
            if (textBox17.Text.CompareTo("") == 0) return;

            var u = Convert.ToDouble( textBox17.Text );
            var omega = Convert.ToInt32(textBox16.Text);
            var listW =  GetW(u, omega);
            textBox6.Text = listW[0].W.ToString();
            textBox7.Text = listW[1].W.ToString();
            textBox8.Text = listW[2].W.ToString();
            textBox9.Text = listW[3].W.ToString();
            textBox10.Text = listW[4].W.ToString();
            textBox11.Text = listW[5].W.ToString();
            textBox12.Text = listW[6].W.ToString();
            textBox13.Text = listW[7].W.ToString();
            textBox14.Text = listW[8].W.ToString();
        }
        //Ok
        private void button1_Click(object sender, EventArgs e)
        {
                Properties.Settings.Default.N = textBox1.Text;

                Properties.Settings.Default.Map = label17.Text;

                Properties.Settings.Default.MapSize = textBox2.Text;
                Properties.Settings.Default.CellSize = textBox18.Text;

                Properties.Settings.Default.C = textBox3.Text;

                Properties.Settings.Default.Gamma = textBox16.Text;
                Properties.Settings.Default.u = textBox17.Text;

                Properties.Settings.Default.w1 = textBox6.Text;
                Properties.Settings.Default.w2 = textBox7.Text;
                Properties.Settings.Default.w3 = textBox8.Text;
                Properties.Settings.Default.w4 = textBox9.Text;
                Properties.Settings.Default.w5 = textBox10.Text;
                Properties.Settings.Default.w6 = textBox11.Text;
                Properties.Settings.Default.w7 = textBox12.Text;
                Properties.Settings.Default.w8 = textBox13.Text;
                Properties.Settings.Default.w9 = textBox14.Text;

                Properties.Settings.Default.Kv = textBox4.Text;
                Properties.Settings.Default.phi = textBox19.Text;

                Properties.Settings.Default.round = textBox15.Text;
                Properties.Settings.Default.ScenarCheck = checkBox1.Checked;

                Properties.Settings.Default.Scenariy = null;
                var i = 0;
                while ( i < dataGridView1.RowCount-1)
                {
                    Properties.Settings.Default.Scenariy += dataGridView1.Rows[i].Cells[0].Value + " " + dataGridView1.Rows[i].Cells[1].Value + " " + dataGridView1.Rows[i].Cells[2].Value;
                    if (i < dataGridView1.RowCount - 1)
                    {
                        Properties.Settings.Default.Scenariy += ".";
                    }
                    i++;
                }
                Properties.Settings.Default.Save();
                Ok = true;
            Close();
        }
        //Close
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        //масштаб
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                textBox18.Text = Round( Convert.ToDouble( textBox2.Text ) / Convert.ToDouble( textBox1.Text ), 3 ).ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Неверно введено количество клеток или размер карты");
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.Enabled = checkBox1.Checked;
        }
    }
}