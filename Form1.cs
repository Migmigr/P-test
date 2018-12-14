using PseudoRandom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KS
{
    public partial class Form1 : Form
    {
        public string path;
        string code, path1;
        List<double[]> co;
        List<double[]> dd;
        MersenneTwister MT;

        public Form1()
        {
            InitializeComponent();
            MT = new MersenneTwister();
        }
        public class KS
        {
            public double alfa;
            public double Dnmin;
            public double xmin;
            public KS(double xmin, double alfa, double Dnmin)
            {
                this.xmin = xmin;
                this.alfa = alfa;
                this.Dnmin = Dnmin;
            }
        }
        string fileNAME;
        double[] FromFile2(string p, bool sc)
        {
            List<double> d = new List<double>();
            StreamReader myStream = null;
            List<string> st = new List<string>();
            if ((myStream = new StreamReader(Application.StartupPath + "\\ВЫБОРКИ\\" + p)) != null)
            {
                using (myStream)
                {
                    string line;
                    while ((line = myStream.ReadLine()) != null)
                    {
                        string[] chasti = line.Split(' ');
                        if (Convert.ToDouble(chasti[1]) != 0)
                        {
                            d.Add(Convert.ToDouble(chasti[1]));
                            st.Add(line);
                        }

                    }
                }
            }

            double[] da = d.ToArray();
            IComparer revComparer = new ReverseComparerP();
            Array.Sort(da, revComparer);
            return da;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            code = System.DateTime.Now.Day + "-" + System.DateTime.Now.Month + "(" + System.DateTime.Now.Ticks + ")";
            path1 = Application.StartupPath + "\\p-test\\" + code;

            Start(2.5, 2500, 0);

        }
        void Start(double alfa, int N, double pra)
        {
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            dd = new List<double[]>();

            fileNAME = "EnPoe.txt";
            code = System.DateTime.Now.Day + "-" + System.DateTime.Now.Month + "(" + System.DateTime.Now.Ticks + ")(" + fileNAME + ")";
            code = code + "S";
            path = Application.StartupPath + "\\p-test\\" + code;

            double[] P = FromFile2(fileNAME, true);

            co = coun(P);
            double m = P[P.Length - 1];
            List<double[]> rasp = dist2(co, true, P[0]);

            chart1.SaveImage(Application.StartupPath + "\\p-test\\Dist\\" + code + ".jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            InFile(rasp);
            chart1.Series.RemoveAt(0);
            chart1.Series.Add("P(x)");
            KS ED = ks2(P, true, false);

            double[][] ks = dd.ToArray();
            Array.Sort(ks, new ReverseComparer0());
            dd = ks.ToList();

            double ldmin = dd[0][1];
            double lxmin = dd[0][0];
            bool spad = true;
            foreach (double[] di in dd)
            {
                if (di[0] < ED.xmin)
                {
                    if (spad == true)
                    {
                        if (di[1] <= ldmin)
                        {
                            ldmin = di[1];
                            lxmin = di[0];
                        }
                        else
                        {
                            InFile("xmin = " + lxmin + "; alfa = " + ot(lxmin, P) + "; Dnmin = " + ldmin + "; % = " + (1 - rasp[rasp.FindIndex(x => x[0] == lxmin)][1]), "data");
                            spad = false;
                            ldmin = di[1];
                            lxmin = di[0];
                        }
                    }
                    else
                    {
                        if (di[1] >= ldmin)
                        {
                            ldmin = di[1];
                            lxmin = di[0];
                        }
                        else
                        {
                            spad = true;
                            ldmin = di[1];
                            lxmin = di[0];
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            label1.Text = "alfa = " + ED.alfa.ToString() + "; xmin = " + ED.xmin.ToString();
            InFile(" ", "data");
            InFile("MINIMORUM: xmin = " + ED.xmin + "; alfa = " + ED.alfa + "; Dnmin = " + ED.Dnmin + "; % = " + (1 - rasp[rasp.FindIndex(x => x[0] == ED.xmin)][1]), "data");
            InFile(" ", "data");
            Goodness(ED.alfa, ED.xmin, ED.Dnmin, P);
        }


        int kol;
        double Dnmin;
        double Xmin;
        KS ks2(double[] P, bool ris, bool test)
        {
            double xmin;
            Dnmin = -2;
            kol = 0;
            List<double> disti = P.Distinct().ToList();
            System.ComponentModel.BackgroundWorker[] BW = new System.ComponentModel.BackgroundWorker[disti.Count];
            Xmin = 0;
            int i = 0;
            foreach (double p in disti)
            {
                xmin = p;

                double alfi = ot(xmin, P);

                BW[i] = new System.ComponentModel.BackgroundWorker();
                BW[i].DoWork += new System.ComponentModel.DoWorkEventHandler(BackgroundWorker1_DoWork);
                BW[i].RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);

                List<double[]> rasp = dist2(co, test, xmin);

                if (rasp[rasp.Count - 1][1] == 1)
                {
                    break;
                }
                Find fd = new Find(xmin, alfi, rasp, ris);

                BW[i].RunWorkerAsync(fd);

                if (double.IsNaN(fd.D))
                {
                    break;
                }
                i++;
            }
            while (kol != BW.Length - 1)
            {
                Application.DoEvents();
            }
            if (ris == true)
            {
                chart2.SaveImage(Application.StartupPath + "\\p-test\\DN\\" + code + ".jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            KS res = new KS(Xmin, ot(Xmin, P), Dnmin);
            return res;
        }
        void Goodness(double alfa, double xmin, double Dmin, double[] V)
        {
            double pv = 0;
            int N = V.Length;

            int Ntail = 0;
            for (int i = 0; i < N; i++)
            {
                if (V[i] < xmin)
                {
                    Ntail++;
                }
                else
                {
                    break;
                }
            }

            Ntail = N - Ntail;
            double v = (double)Ntail / (double)N;

            int kl = 2500;

            for (int k = 1; k <= kl; k++)
            {
                double[] W = GenVib(alfa, N, xmin, v, Ntail, V);
                co.Clear();
                co = coun(W);
                InFile("Test " + k.ToString() + ":", "ptest");
                KS SD = ks2(W, false, false);
                W = new double[0];

                InFile("xmin = " + SD.xmin + "; alfa = " + SD.alfa + "; Dnmin = " + SD.Dnmin, "ptest");
                if (SD.Dnmin > Dmin)
                {
                    pv++;
                }
                InFile("p = " + pv.ToString(), "ptest");
            }
            pv = pv / kl;
            InFile("p = " + pv.ToString(), "ptest");
            label1.Text = "p = " + pv.ToString();
        }

        double ot(double xmin, double[] P)
        {
            double summ = 0;
            double su = 0;
            int col = 0;
            foreach (double p in P)
            {
                if (p >= xmin)
                {
                    summ += Math.Log(p / (xmin - 0.5));
                    //summ += Math.Log(p / (xmin));   ///ВТОРОЙ ВАРИАНТ ПОЛУЧЕНИЯ ЗНАЧЕНИЙ
                    su += Math.Log(p);
                    col++;
                }
            }
         
            Double r = (double)1 + ((double)col / summ);
            return (double)1 + ((double)col / summ);

            ///ВТОРОЙ ВАРИАНТ ПОЛУЧЕНИЯ ЗНАЧЕНИЙ
            //double max = double.NegativeInfinity;
            //double a = 0;
            //List<double> L = new List<double>();
            //for (double alfa = 1.0; alfa <= 4.0; alfa = Math.Round(alfa + 0.001, 3))
            //{

            //    double prom = max;
            //    max = Math.Max(max, -col * Math.Log(zeta1(alfa, xmin)) - alfa * su);
            //    if (max != prom)
            //    {
            //        a = alfa;
            //    }
            //    // L.Add(-col * Math.Log(zeta1(alfa, xmin)) - alfa*su);
            //}
            //return a;
        }

        List<double[]> coun(double[] P)
        {
            List<double[]> c = new List<double[]>();
            List<double> disti = P.Distinct().ToList();

            double X = P[0];
            int cco = 0;
            for (int i = 0; i < P.Length; i++)
            {
                if (P[i] == X)
                    cco++;
                else
                {
                    double[] p = new double[2];
                    p[0] = X;
                    p[1] = P.Length - ((double)i - cco);
                    c.Add(p);
                    X = P[i];
                    cco = 1;
                }
            }
            c.Add(new double[2] { X, cco });
            return c;
        }
        List<double[]> dist2(List<double[]> c, bool ris, double xmin)
        {
            List<double[]> rasp = new List<double[]>();
            int ind = c.FindIndex(x => x[0] == xmin);
            double col = c[ind][1];
            for (int i = ind; i < c.Count; i++)
            {
                double pr = c[i][1] / col;
                rasp.Add(new double[2] { c[i][0], pr });
                if (ris)
                    chart1.Series[0].Points.AddXY(Math.Log10(c[i][0]), -Math.Log10(pr));
            }
            return rasp;
        }
        double[] GenVib(double alfa, int N, int xmin, double proc)
        {
            double[] P = new double[N];
            double I1 = xmin / (alfa - 1);
            double I2 = (xmin / alfa) * (Math.Exp(-alfa * ((1.0 / xmin) - 1)) - 1);
            double C2 = 1 / (I1 + I2);
            double C = zeta1(alfa, xmin);

            double C1 = 1 / ((xmin / alfa) * (Math.Exp(-alfa * ((1.0 / xmin) - 1)) - 1));
            for (int i = 0; i < Math.Floor(N * proc); i++)
            {
                P[i] = Math.Floor(((xmin - 0.5) * (((Math.Log((alfa / (xmin)) * ((1.0 - MT.genrand_real1()) / C1) + 1)) / (-alfa)) + 1)) + 0.5);
            }
            for (int i = (int)Math.Floor(N * proc); i < N; i++)
            {
                double r = MT.genrand_real1();
                double x2 = xmin;
                double x1 = x2;
                do
                {
                    x1 = x2;
                    x2 = 2 * x1;
                } while (zeta1(alfa, x2) / C >= 1 - r);

                double pol = 0;

                do
                {
                    pol = ((x2 - x1) / 2) + x1;
                    double f = zeta1(alfa, pol) / C;
                    if (f > 1 - r)
                    {
                        x1 = pol;
                    }
                    else
                    {
                        if (f < 1 - r)
                            x2 = pol;
                        else
                            break;
                    }
                }
                
                while (Math.Floor(x2) != Math.Floor(x1));
                P[i] = Math.Floor(x1);
            }

            Array.Sort(P, new ReverseComparerP());
            return P;
        }
        double[] GenVib(double alfa, int N, double xmin, double v, int Ntail, double[] W)
        {
            double[] P = new double[N];
            List<double> gv = new List<double>();
            double C = zeta1(alfa, xmin);
            double Nt = 0;
            for (int i = 0; i < N; i++)
                if (MT.genrand_real1() > v)  Nt++;

            for (int i = 0; i < Nt; i++)
                P[i] = W[(int)Math.Floor((N - Ntail) * MT.genrand_real1())];

            for (int i = (int)Nt; i < N; i++)             
                P[i] = Math.Floor(((xmin - 0.5) * Math.Pow(1.0 - MT.genrand_real1(), (-1 / (alfa - 1)))) + 0.5);
           
            Array.Sort(P, new ReverseComparerP());
            double m = P[P.Length - 1];
            return P;
        }
        #region znach
        public static double zeta1(double x, double q)
        {
            // * Check arguments
            if (x == 1.0)
                return Double.PositiveInfinity;
            if (q < 1.0)
                // return Double.NaN;
                if (q <= 0.0)
                    if (q == Math.Floor(q))
                        return Double.PositiveInfinity;
                    else
                        return Double.NaN;

            double s = Math.Pow(q, -x);
            double a = q;
            double b = 0.0;

            int i = 0;

            bool done = false;
            while ((i < 9 || a <= 9.0) && !done)
            {
                i++;

                a++;
                b = Math.Pow(a, -x);
                s += b;

                if (Math.Abs(b / s) < 1E-12)
                    done = true;
            }

            double k = 0.0;
            double w = a;
            s += b * w / (x - 1.0);
            s -= 0.5 * b;
            a = 1.0;

            double t;
            for (i = 0; i < 12 && !done; i++)
            {
                a *= x + k;
                b /= w;
                t = a * b / m[i];
                s += t;

                t = Math.Abs(t / s);
                if (t < 1E-12)
                    done = true;

                k += 1.0;
                a *= x + k;
                b /= w;
                k += 1.0;
            }

            return s;
        }
        private static double[] m =
        {
         12.0,
        -720.0,
         30240.0,
        -1209600.0,
         47900160.0,
        -1.8924375803183791606e9,  /*1.307674368e12/691*/
         7.47242496e10,
        -2.950130727918164224e12,  /*1.067062284288e16/3617*/
         1.1646782814350067249e14,  /*5.109094217170944e18/43867*/
        -4.5979787224074726105e15, /*8.028576626982912e20/174611*/
         1.8152105401943546773e17,  /*1.5511210043330985984e23/854513*/
        -7.1661652561756670113e18  /*1.6938241367317436694528e27/236364091*/
         };
        #endregion   
        #region load
        private void Form1_Load(object sender, EventArgs e)
        {
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoom(1, 6);
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;

            //chart2.ChartAreas[0].AxisX.ScaleView.Zoom(-50000,-20000);
            //chart2.ChartAreas[0].AxisX.Interval = 2;

            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart2.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            //chart2.ChartAreas[0].AxisY.ScaleView.Zoom(0, 1);
            chart2.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart2.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;

        }
        #endregion
        #region file
        void InFile(string S, string tipe)
        {
            StreamWriter myStream = null;
            if ((myStream = File.AppendText(path + "." + tipe)) != null)
            {
                using (myStream)
                {
                    myStream.WriteLine(S);
                }
            }
        }
        void InFile(string S, string Name, string tipe)
        {
            StreamWriter myStream = null;
            if ((myStream = File.AppendText(path1 + Name + "." + tipe)) != null)
            {
                using (myStream)
                {
                    myStream.WriteLine(S);
                }
            }
        }
        void InFile(int xmin, double beta, double Dn)
        {
            StreamWriter myStream = null;
            if ((myStream = File.AppendText(path + ".data1")) != null)
            {
                using (myStream)
                {
                    myStream.WriteLine("xmin = " + xmin + "; alfa = " + beta + "; Dn = " + Dn);
                }
            }

        }
        void InFile(List<double[]> rasp)
        {
            StreamWriter myStream = null;
            if ((myStream = File.AppendText(path + ".data1")) != null)
            {
                using (myStream)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    myStream.WriteLine("Distribution:");
                    for (int i = 0; i < rasp.Count; i++)
                    {
                        if (i > 0)
                        {
                            for (int d = 1; d <= (int)(rasp[i][0] - rasp[i - 1][0] - 1); d++)
                            {
                                sb.AppendLine((rasp[i - 1][0] + d).ToString() + " " + rasp[i][1].ToString());
                            }
                        }
                        sb.AppendLine(rasp[i][0].ToString() + " " + rasp[i][1].ToString());

                    }
                    myStream.WriteLine(sb.ToString());
                }
            }
            myStream.Close();
        }
        double[] FromFile(string p)
        {
            double[] d = new double[0]; ;
            StreamReader myStream = null;
            if ((myStream = new StreamReader(p)) != null)
            {
                using (myStream)
                {
                    string line;
                    line = myStream.ReadLine();
                    string[] Numbers = line.Split(' ');
                    d = new double[Numbers.Length];
                    for (int i = 0; i < d.Length; i++)
                    {
                        d[i] = Convert.ToDouble(Numbers[i]);
                    }
                }
            }
            IComparer revComparer = new ReverseComparerP();
            Array.Sort(d, revComparer);
            return d;
        }

        #endregion
        #region DN
        public class Find
        {
            public double xmin;
            public double alfa;
            public bool ris;
            public List<double[]> raspr;
            public double D;

            public Find(double xmin, double alfa, List<double[]> raspr, bool ris)
            {
                this.xmin = xmin;
                this.alfa = alfa;
                this.raspr = raspr;
                this.ris = ris;
            }
            public double FindDn()
            {
                double c = zeta1(alfa, xmin);
                double Ds = 0;

                for (int i = 0; i < raspr.Count; i++)
                {
                    if (raspr[i][0] >= xmin)
                    {
                        double asr = Math.Abs(raspr[i][1] - (zeta1(alfa, raspr[i][0]) / c));
                        Ds = Math.Max(Ds, Math.Abs(raspr[i][1] - (zeta1(alfa, raspr[i][0]) / c)));
                        if (i != raspr.Count - 1)
                        {
                            Ds = Math.Max(Ds, Math.Abs(raspr[i + 1][1] - (zeta1(alfa, raspr[i][0] + 1) / c)));
                        }
                    }
                }

                D = Ds;
                return Ds;
            }
            public double FindDn(List<double[]> DNS)
            {
                double c = zeta1(alfa, xmin);
                double Ds = 0;

                for (int i = 0; i < raspr.Count; i++)
                {
                    if (raspr[i][0] >= xmin)
                    {

                        double asr = Math.Abs(raspr[i][1] - (zeta1(alfa, raspr[i][0]) / c));
                        Ds = Math.Max(Ds, Math.Abs(raspr[i][1] - (zeta1(alfa, raspr[i][0]) / c)));
                        DNS.Add(new double[2] { raspr[i][0], asr });
                        if (i != raspr.Count - 1)
                        {
                            Ds = Math.Max(Ds, Math.Abs(raspr[i + 1][1] - (zeta1(alfa, raspr[i][0] + 1) / c)));
                            if (raspr[i + 1][0] != raspr[i][0] + 1)
                            {
                                DNS.Add(new double[2] { raspr[i + 1][0], Math.Abs(raspr[i + 1][1] - (zeta1(alfa, raspr[i][0] + 1) / c)) });
                            }

                        }
                    }
                }
                D = Ds;
                return Ds;
            }
        }
        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Find Fdn = (Find)e.Argument;

            resul re = new resul();
            re.xmin = Fdn.xmin;
            re.Dn = Fdn.FindDn();
            re.ris = Fdn.ris;
            Fdn.raspr.Clear();

            e.Result = re;
        }
        private void BackgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // Access the result through the Result property.
            resul A = (resul)e.Result;
            kol++;
            if (A.ris == true)
            {
                dd.Add(new double[] { A.xmin, A.Dn });
                chart2.Series[0].Points.AddXY(A.xmin, A.Dn);
                InFile("xmin = " + A.xmin + "; Dn = " + A.Dn, "data1");
            }
            if (double.IsNaN(A.Dn) != true)
            {
                if (Dnmin == -2)
                {
                    Dnmin = A.Dn;
                    Xmin = (double)A.xmin;
                }
                else
                {
                    double Dnm = Math.Min(Dnmin, A.Dn);
                    if (Dnm != Dnmin)
                    {
                        Dnmin = Dnm;
                        Xmin = (double)A.xmin;
                    }

                }
            }

        }
        #endregion
    }
}
