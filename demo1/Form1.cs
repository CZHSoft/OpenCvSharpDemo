using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace demo1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择导入文件";
            dialog.Filter = "All|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Mat source = new Mat(dialog.FileName, ImreadModes.Color);
            Mat copy = source.Clone();
            Mat gray = new Mat();
            Mat blurred = new Mat();
            Mat binary = new Mat();

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchys;
            
            //gray
            Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
            //Cv2.ImShow("gray", gray);
            Cv2.GaussianBlur(gray, blurred,new OpenCvSharp.Size(5,5), 0);
            //Cv2.ImShow("blurred", blurred);
            var ret = Cv2.Threshold(blurred, binary, 60, 255, ThresholdTypes.Binary);
            //Cv2.ImShow("binary", binary);
            Cv2.FindContours(binary, out contours, out hierarchys, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours != null && contours.Length > 0)
            {
                
                for (int i = 0; i< contours.Length; i++)
                {
                    var m = Cv2.Moments(contours[i]);
                    var x = m.M10 / m.M00;
                    var y = m.M01 / m.M00;

                    if(double.IsNaN(x) || double.IsNaN(y))
                    {
                        continue;
                    }

                    //轮廓逼近
                    var epsilon = 0.01 * Cv2.ArcLength(contours[i], true);
                    var approx = Cv2.ApproxPolyDP(contours[i], epsilon, true);
                    
                    //分析形状
                    var len = approx.Length;
                    string shape = string.Empty;
                    if(len == 3)
                    {
                        shape = "T";
                    }
                    else if(len == 4)
                    {
                        shape = "R";
                    }
                    else if(len > 4 && len < 10)
                    {
                        shape = "P";
                    }
                    else if(len >= 10)
                    {
                        shape = "C";
                    }
                    else
                    {
                        shape = "?";
                    }
                    Cv2.PutText(copy, shape, new OpenCvSharp.Point(x - 10, y - 10),
                            HersheyFonts.HersheyScriptSimplex, 0.6, new Scalar(255, 255, 255), 1);

                    //颜色分析
                    var color = copy.At<Vec3b>((int)y ,(int)x);

                    // 周长 面积
                    var p = Cv2.ArcLength(contours[i], true);
                    var area = Cv2.ContourArea(contours[i]);

                    Trace.WriteLine($"第{i}: 形状;{shape} 中心位置 x {x} y {y} 周长 {p} 面积 {area} \r\n" +
                        $" color: b {color.Item0} g {color.Item1} r {color.Item2} ");

                    //中心分析
                    Cv2.Circle(copy, (int)x, (int)y, 1, new Scalar(255, 255, 255), -1);

                    
                }

                //提取与绘制轮廓
                Cv2.DrawContours(copy, contours, -1, new Scalar(0, 255, 0), 1);

                Cv2.ImShow("source", source);
                Cv2.ImShow("copy", copy);
            }

            Cv2.WaitKey(0);
        }
    }
}
