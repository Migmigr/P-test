using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
namespace Bin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public class Part : IEquatable<Part>, IComparable<Part>
        {
            public string Name { get; set; }

            public Decimal Count { get; set; }

            public override string ToString()
            {
                return Regex.Replace(Name, "\\,", ".") + " " + Regex.Replace(Count.ToString(), "\\,", ".");
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Part objAsPart = obj as Part;
                if (objAsPart == null) return false;
                else return Equals(objAsPart);
            }
            public int SortByNameAscending(string name1, string name2)
            {
                return name1.CompareTo(name2);
            }

            // Default comparer for Part type.
            public int CompareTo(Part comparePart)
            {
                // A null value means that this object is greater.
                if (comparePart == null)
                    return 1;
                else
                    return this.Count.CompareTo(comparePart.Count);
            }
            public override int GetHashCode()
            {
                return Convert.ToInt32(Count);
            }
            public bool Equals(Part other)
            {
                if (other == null) return false;
                return (this.Count.Equals(other.Count));
            }
            // Should also override == and != operators.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MersenneTwister mt = new MersenneTwister();
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            Decimal c = Convert.ToDecimal(Regex.Replace(textBox1.Text, "\\.", ","));
            Double R = Convert.ToDouble(Regex.Replace(textBox2.Text, "\\.", ","));
            int xmin = Convert.ToInt32(textBox3.Text);
            List<List<string>> LI = new List<List<string>>();
            LI.Add(Directory.GetFiles(@"New Value").ToList());

            int counta = 0;
            List<int> cof = new List<int>();

            int ccc = 0;
            int sc = 1000;
            sc = 1;
            for (int t = 0; t < LI[0].Count; t++)
            {
                List<string> tt = new List<string>();
                List<Part> p = new List<Part>();
                for (int j = 0; j < LI.Count; j++)
                {
                    StreamReader myStream = null;
                    if ((myStream = new StreamReader(LI[j][t])) != null)
                    {
                        using (myStream)
                        {
                            string line;
                            while ((line = myStream.ReadLine()) != null)
                            {
                                string[] parts = Regex.Split(line, @" ");
                                string[] par = line.Split('\\');
                                Double n = Convert.ToDouble(parts.Last());
                                xmin = 0;//лишнее
                                if (n > xmin)
                                {
                                    if (j != 1)
                                    {
                                        if (n > 0)
                                        {
                                            counta++;
                                            p.Add(new Part { Name = "", Count = Convert.ToDecimal(n) * sc });
                                        }
                                    }
                                    else
                                    {

                                        tt.Add(line);
                                        if (ccc >= 0)
                                        {
                                            Double pa = 5000.0 / 75545.0;
                                            if (mt.genrand_real2() <= pa)
                                            {
                                                cof.Add(ccc);
                                            }
                                            ccc++;
                                        }

                                    }

                                }
                            }
                        }
                    }
                }

                decimal F = p.Last().Count;
                int colvos = 0;
                List<Part> PDF = new List<Part>();
                List<Part> hr = new List<Part>();
                int pos = p.FindIndex(x => x.Count == xmin);
                int firt = p.FindIndex(x => x.Count == xmin);
                Decimal df = p.Last().Count;
                Decimal sum = 0;

                int nb = 1;
                Decimal bj = Math.Floor(c * (Decimal)Math.Pow(R, nb));
                Decimal bj1 = Math.Floor(c * (Decimal)Math.Pow(R, nb - 1));
                for (int i = 1; i <= 49; i++)
                {
                    hr.Add(new Part { Name = i.ToString(), Count = 0 });
                }
                for (int i = 0; i < p.Count; i++)
                {
                    hr[(int)p[i].Count - 1].Count++;
                }
                pos = 0;
                firt = 0;
                Decimal pp = p[0].Count;
                int co = 0;
                for (int i = 0; i < p.Count; i++)
                {
                    if (p[i].Count == pp)
                    {
                        co++;
                    }
                    else
                    {
                        colvos += co;
                        if (co != 0)
                        {
                            hr.Add(new Part { Name = pp.ToString(), Count = (Convert.ToDecimal(co) / (p.Count - firt)) });
                            sum += (Convert.ToDecimal(co) / (p.Count - firt));
                        }
                        pp = p[i].Count;
                        co = 1;
                    }

                }
                if (p[p.Count - 1].Count == pp)
                {
                    colvos += co;
                    hr.Add(new Part { Name = pp.ToString(), Count = (Convert.ToDecimal(co) / (p.Count - firt)) });
                    sum += (Convert.ToDecimal(co) / (p.Count - firt));
                }

                pos = 0;
                Decimal co2 = 0;
                nb = 1;

                bj = Math.Floor(c * (Decimal)Math.Pow(R, nb));
                bj1 = Math.Floor(c * (Decimal)Math.Pow(R, nb - 1));

                for (int j = pos; j < hr.Count; j++)
                {
                    if (Convert.ToDecimal(hr[j].Name) <= Math.Max(bj - 1, bj1))
                    {
                        Decimal ri = 1;
                        if (j == hr.Count - 1)
                        {
                            ri = (Convert.ToDecimal(hr[j].Name) - Convert.ToDecimal(hr[j - 1].Name));
                        }
                        else
                        {
                            ri = (Convert.ToDecimal(hr[j + 1].Name) - ((j == 0) ?
                                Convert.ToDecimal(hr[j].Name) : (Convert.ToDecimal(hr[j - 1].Name)) / 2));
                        }
                        ri = Convert.ToDecimal(1);
                        co2 += hr[j].Count * ri;
                    }
                    else
                    {
                        Decimal razm = bj1 * ((Decimal)R - 1);
                        Decimal pointa = c * (Decimal)Math.Pow(R, nb - 0.5);
                        if (co2 != 0)
                        {
                            PDF.Add(new Part { Name = (pointa / sc).ToString(), Count = (Convert.ToDecimal(co2) / (razm)) });
                            chart1.Series[1].Points.AddXY(Math.Log10((Double)pointa), Math.Log10((Double)PDF.Last().Count));
                        }

                        nb++;
                        bj1 = bj;
                        bj = Math.Floor(c * (Decimal)Math.Pow(R, nb));
                        j--;
                        co2 = 0;
                    }
                }
                Decimal razm1 = bj1 * ((Decimal)R - 1);
                Decimal pointa1 = c * (Decimal)Math.Pow(R, nb - 0.5);
                PDF.Add(new Part { Name = (pointa1 / sc).ToString(), Count = (Convert.ToDecimal(co2) / (razm1)) });
                chart1.Series[1].Points.AddXY(Math.Log10((Double)pointa1), Math.Log10((Double)PDF.Last().Count));

                StreamWriter mySt = null;
                if ((mySt = new StreamWriter("Binned" + LI[0][t].Split('\\').Last())) != null)
                {
                    using (mySt)
                    {
                        foreach (Part s in PDF)
                            mySt.WriteLine(s.ToString());

                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
