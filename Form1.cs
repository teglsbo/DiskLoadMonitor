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
                ReadData.Add(0);
                WriteData.Add(0);
            }

            Bitmap = new Bitmap(HistorySize, HistorySize);
        }

        private readonly List<int> ReadData = new List<int>();
        private readonly List<int> WriteData = new List<int>();
        private readonly PerformanceCounter ReadCounter = new PerformanceCounter("PhysicalDisk", "% Disk Read Time", "_Total");
        private readonly PerformanceCounter WriteCounter = new PerformanceCounter("PhysicalDisk", "% Disk Write Time", "_Total");
        const int HistorySize = 16;
        private readonly Bitmap Bitmap;

        private void UpdateTimerTick(object sender, EventArgs e) {
            // Update data
            float readDataPoint = UpdateData(ReadCounter, ReadData);
            float writeDataPoint = UpdateData(WriteCounter, WriteData);

            string tooltipText =
                       "Disk Time: " + Math.Round(readDataPoint+writeDataPoint, 0) + "%\n"+
                       "Disk Read Time: "+Math.Round(readDataPoint, 0)+"%\n"+
                       "Disk Write Time: "+Math.Round(writeDataPoint, 0)+"%";
            notifyIcon.Text = LimitText(63, tooltipText);

            // Draw pixels on the bitmap
            for (int i = 0; i < HistorySize; i++) {
                int rd = ReadData[i];
                int wd = WriteData[i];
                int md = Math.Max(rd, wd);
                for (int j = 0; j < HistorySize; j++) {
                    Color color;
                    if (j >= md) color = Color.Black;
                    else if (j < rd && j < wd) color = Color.Yellow;
                    else if (j < rd) color = Color.Lime;
                    else if (j < wd) color = Color.Red;
                    else throw new Exception("Burde ikke ske: " + j + ", " + rd + ", " + md);
                    Bitmap.SetPixel(i, HeightToY(j), color);
                }
            }

            // Create an icon the the bitmap
            Icon icon = Icon.FromHandle(Bitmap.GetHicon());

            // Release the old icon
            NativeMethods.DestroyIcon(notifyIcon.Icon.Handle);

            // Set the new icon
            notifyIcon.Icon = icon;
        }

        private static string LimitText(int maxLength, string s) {
            if(s.Length <= maxLength) return s;
            return s.Substring(0, maxLength);
        }

        private static float UpdateData(PerformanceCounter counter, IList<int> data) {
            // May do: Use the list as a cyclic buffer instead (with 16 entries it probably doesn't matter much)
            data.RemoveAt(0);
            float dataPoint = counter.NextValue();
            int pixelHeight = (int)(dataPoint / 100 * HistorySize);
            data.Add(pixelHeight);
            return dataPoint;
        }

        private static int HeightToY(int j) {
            return HistorySize-1-j;
        }

        private void NotifyIconMouseDoubleClick(object sender, MouseEventArgs e) {
            Application.Exit();
        }

        // p/invoke definition for DestroyIcon
        internal class NativeMethods {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public extern static bool DestroyIcon(IntPtr handle);
        }
    }
}