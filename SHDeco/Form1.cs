using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.DataSets;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace SHDeco
{
    public partial class Form1 : Form
    {
        System.Drawing.Image temp = System.Drawing.Image.FromFile("x.jpg");

        List<PointF> clickedPoints = new List<PointF>();
        List<SHRegion> RegionsList = new List<SHRegion>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = temp;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs ee = (MouseEventArgs)e;

            if (ee.Button == MouseButtons.Left)
            {
                PointF rpoint = pictureBox1.PointToClient(Cursor.Position);
                clickedPoints.Add(rpoint);

                using (Graphics g = Graphics.FromImage(temp))
                {
                    g.DrawEllipse(new Pen(Color.Red, 3), rpoint.X, rpoint.Y, 2, 2);
                    if (clickedPoints.Count > 1)
                        g.DrawLine(new Pen(Color.Red, 1), clickedPoints[clickedPoints.Count - 2], clickedPoints[clickedPoints.Count - 1]);
                    g.Save();
                }
                pictureBox1.Image = temp;
            }
            else if (ee.Button == MouseButtons.Right)
            {
                PointF[] points = clickedPoints.ToArray();
                using (Graphics g = Graphics.FromImage(temp))
                {
                    if (clickedPoints.Count > 2)
                        g.DrawLine(new Pen(Color.Red, 1), clickedPoints[clickedPoints.Count - 1], clickedPoints[0]);

                    GraphicsPath fillPath = new GraphicsPath();
                    fillPath.AddLines(points);
                    Region toFill = new Region(fillPath);

                    g.FillRegion(new SolidBrush(Color.FromArgb(50, 255, 0, 0)), toFill);
                    g.Save();

                    SHRegion sh = new SHRegion()
                    {
                        ClickedPoints = clickedPoints
                    };
                    RegionsList.Add(sh);
                }
                pictureBox1.Image = temp;

                clickedPoints = new List<PointF>();
            }
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            if(RegionsList.Count > 0)
            {
                System.Drawing.Image f = System.Drawing.Image.FromFile("x.jpg");
                using (Graphics g = Graphics.FromImage(f))
                {
                    g.Clear(Color.White);
                    Brush typical = new SolidBrush(Color.Red);
                    foreach (SHRegion sh in RegionsList)
                    {
                        List<byte> types = new List<byte>();
                        for (int i = 0; i < sh.ClickedPoints.Count; i++)
                            types.Add(1);

                        Region reg = new Region(new GraphicsPath(sh.ClickedPoints.ToArray(), types.ToArray()));
                        g.FillRegion(typical, reg);
                    }
                    g.Save();
                }

                SaveFileDialog dialog = new SaveFileDialog();

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SaveRegionsData(RegionsList, dialog.FileName);
                    f.Save(dialog.FileName + ".jpg", ImageFormat.Jpeg);
                }
            }
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            clickedPoints = new List<PointF>();
            RegionsList = new List<SHRegion>();

            temp = System.Drawing.Image.FromFile("x.jpg");
            pictureBox1.Image = temp;
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            clickedPoints = new List<PointF>();
            RegionsList = new List<SHRegion>();

            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadRegionsData(out RegionsList, dialog.FileName);

                temp = Accord.Imaging.Image.FromFile("x.jpg");
                pictureBox1.Image = temp;

                foreach(SHRegion sh in RegionsList)
                {
                    List<byte> types = new List<byte>();
                    for (int i = 0; i < sh.ClickedPoints.Count; i++)
                        types.Add(1);

                    Region reg = new Region(new GraphicsPath(sh.ClickedPoints.ToArray(), types.ToArray()));

                    foreach(PointF point in sh.ClickedPoints)
                    {
                        using (Graphics g = Graphics.FromImage(temp))
                        {
                            g.DrawEllipse(new Pen(Color.Red, 3), point.X, point.Y, 2, 2);
                            g.Save();
                        }
                    }

                    using (Graphics g = Graphics.FromImage(temp))
                    {
                        g.DrawLines(new Pen(Color.Red, 1), sh.ClickedPoints.ToArray());
                        g.FillRegion(new SolidBrush(Color.FromArgb(30, 255, 0, 0)), reg);
                        g.Save();
                    }
                    
                    pictureBox1.Image = temp;
                }
            }
        }

        public static System.Drawing.Point IntPointToPoint(IntPoint intPoint)
        {
            return new System.Drawing.Point(intPoint.X, intPoint.Y);
        }

        public static void SaveRegionsData(List<SHRegion> regions, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(regions, Formatting.Indented));
        }
        public static void LoadRegionsData(out List<SHRegion> regions, string path)
        {
            regions = JsonConvert.DeserializeObject<List<SHRegion>>(File.ReadAllText(path));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if(RegionsList.Count > 0)
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    temp = Accord.Imaging.Image.FromFile("x.jpg");
                    pictureBox1.Image = temp;

                    foreach (SHRegion sh in RegionsList)
                    {
                        using (Graphics g = Graphics.FromImage(temp))
                        {
                            List<byte> types = new List<byte>();
                            for (int i = 0; i < sh.ClickedPoints.Count; i++)
                                types.Add(1);

                            Region r = new Region(new GraphicsPath(sh.ClickedPoints.ToArray(), types.ToArray()));

                            g.FillRegion(new SolidBrush(Color.FromArgb(150, colorDialog1.Color)), r);
                            g.Save();
                        }
                    }

                    pictureBox1.Image = temp;
                }
            }
        }
    }
    public class SHRegion
    {
        public List<PointF> ClickedPoints { get; set; }
    }
}
