using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DiskLoadMonitor {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            for(int i=0; i<16; i++) {
                //data.Add(0);
                readData.Add(0);
                writeData.Add(0);
            }

            bitmap = new Bitmap(size, size);
            // graphics = Graphics.FromImage(bitmap);
        }

        //private readonly List<int> data = new List<int>();
        private readonly List<int> readData = new List<int>();
        private readonly List<int> writeData = new List<int>();
        // private readonly PerformanceCounter counter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        private readonly PerformanceCounter readCounter = new PerformanceCounter("PhysicalDisk", "% Disk Read Time", "_Total");
        private readonly PerformanceCounter writeCounter = new PerformanceCounter("PhysicalDisk", "% Disk Write Time", "_Total");
        const int size = 16;
        private readonly Bitmap bitmap;
        // private readonly Graphics graphics;

        private void updateTimer_Tick(object sender, EventArgs e) {
            // Opdater data (man kunne overveje at bruge noget kø-agtigt)
            float readDataPoint = OpdaterData(readCounter, readData);
            float writeDataPoint = OpdaterData(writeCounter, writeData);

            string tooltipText =
                       "Disk Time: " + Math.Round(readDataPoint+writeDataPoint, 0) + "%\n"+
                       "Disk Read Time: "+Math.Round(readDataPoint, 0)+"%\n"+
                       "Disk Write Time: "+Math.Round(writeDataPoint, 0)+"%";
            notifyIcon.Text = LimitText(63, tooltipText);

            
            // Tegn pixels på bitmappen
            for (int i = 0; i < size; i++) {
                int rd = readData[i];
                int wd = writeData[i];
                int md = Math.Max(rd, wd);
                for (int j = 0; j < size; j++) {
                    Color color;
                    if (j >= md) color = Color.Black;
                    else if (j < rd && j < wd) color = Color.Yellow;
                    else if (j < rd) color = Color.Lime;
                    else if (j < wd) color = Color.Red;
                    else throw new Exception("Burde ikke ske: " + j + ", " + rd + ", " + md);
                    bitmap.SetPixel(i, HeightToY(j), color);
                }
            }
            //// Tegn linjer på bitmappen via graphics-objektet, der tegner på bitmappen
            //for (int i = 0; i < size - 1; i++) {
            //    int d1 = data[i];
            //    int d2 = data[i + 1];
            //    graphics.DrawLine(Pens.LightGreen, i, HeightToY(d1), i + 1, HeightToY(d2));
            //}
            //graphics.Flush(); // Sørg for at graphics får tegnet alt ned på bitmappen

            // Lav et smukt icon ud fra bitmap
            //IntPtr hicon = bitmap.GetHicon();
            //Icon icon = Icon.FromHandle(hicon);

            Icon icon = Icon.FromHandle(bitmap.GetHicon());

            // Frigiv det gamle icon
            NativeMethods.DestroyIcon(notifyIcon.Icon.Handle);
            // notifyIcon.Icon.Dispose();

            // Sæt det nye icon
            notifyIcon.Icon = icon;
        }

        private static string LimitText(int maxLength, string s) {
            if(s.Length <= maxLength) return s;
            return s.Substring(0, maxLength);
        }

        private static float OpdaterData(PerformanceCounter counter, IList<int> data) {
            data.RemoveAt(0);
            float dataPoint = counter.NextValue();
            int pixelHeight = (int)(dataPoint / 100 * size);
            data.Add(pixelHeight);
            return dataPoint;
        }

        private static int HeightToY(int j) {
            return size-1-j;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            Application.Exit();
        }

        // p/invoke definition for DestroyIcon
        internal class NativeMethods {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public extern static bool DestroyIcon(IntPtr handle);
        }
    }
}