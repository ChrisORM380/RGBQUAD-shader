using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using static tutorial_SHADER.Imports;
namespace tutorial_SHADER
{
    internal class mainClass
    {
        [STAThread]
        static void Main()
        {
            while (true) {
                IntPtr hdc = makeDC();
                BITMAPINFO bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(bmi);
                bmi.bmiHeader.biWidth = x;
                bmi.bmiHeader.biHeight = -y;
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = 0;
                byte[] ptr = new byte[x * y * 4];
                Marshal.Copy(getScreen().LockBits(new Rectangle(0, 0, x, y), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb).Scan0, ptr, 0, ptr.Length);
                int ee = 0;
                for (int xx = 0; xx < x; xx++) {
                    for (int yy = 0; yy < y; yy++)
                    {
                        hsv hsv = new hsv(new rgb(ptr[ee + 2], ptr[ee + 1], ptr[ee])); //This will be used to make a HSV shader.
                        hsv.changeSAT(0);
                        rgb rgb = new rgb(hsv);
                        ptr[ee] = rgb.b; //This is the blue channel.
                        ptr[ee + 1] = rgb.g; //This is the green channel.
                        ptr[ee + 2] = rgb.r; //This is the red channel.
                        ee += 4;
                    }
                }
                SetDIBitsToDevice(hdc, 0, 0, (uint)x, (uint)y, 0, 0, 0, (uint)y, ptr, ref bmi, 0);
                RemoveDC(hdc);
            }
        }
    }
}
