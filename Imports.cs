using System;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace tutorial_SHADER
{
    class Imports
    {
        public static Bitmap FlipBitmap(Bitmap original, RotateFlipType flipType)
        {
            Bitmap flippedBitmap = new Bitmap(original);
            flippedBitmap.RotateFlip(flipType);
            return flippedBitmap;
        }
        [DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);
        [DllImport("ntdll.dll")]
        public static extern uint NtSetSystemPowerState(POWER_ACTION paction, SYSTEM_POWER_STATE pst, ulong flags);
        [DllImport("ntdll.dll")]
        public static extern uint NtShutdownSystem(SHUTDOWN_ACTION action);
        [DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);
        public static ulong SHTDN_REASON_MAJOR_HARDWARE = 0x00010000;
        public static ulong SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a;
        public enum SYSTEM_POWER_STATE
        {
            PowerSystemUnspecified = 0,
            PowerSystemWorking = 1,
            PowerSystemSleeping1 = 2,
            PowerSystemSleeping2 = 3,
            PowerSystemSleeping3 = 4,
            PowerSystemHibernate = 5,
            PowerSystemShutdown = 6,
            PowerSystemMaximum = 7
        }
        public enum SHUTDOWN_ACTION
        {
            ShutdownNoReboot,
            ShutdownReboot,
            ShutdownPowerOff
        }
        public enum POWER_ACTION
        {
            PowerActionNone = 0,
            PowerActionReserved,
            PowerActionSleep,
            PowerActionHibernate,
            PowerActionShutdown,
            PowerActionShutdownReset,
            PowerActionShutdownOff,
            PowerActionWarmEject,
            PowerActionDisplayOff
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetProcessId(IntPtr handle);


        public static int GetProcessIdFromHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return 0;
            }
            try
            {
                return GetProcessId(handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting process id from handle: {ex.Message}");
                return 0;
            }
        }
        public static Process GetProcessFromHandle(IntPtr handle)
        {
            try
            {
                // Get the process ID from the handle (requires Windows API calls)
                // This is a simplified example and might not work in all cases
                int processId = GetProcessIdFromHandle(handle);
                if (processId == 0) return null;

                // Get the process using process id
                Process process = Process.GetProcessById(processId);

                return process;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting process from handle: {ex.Message}");
                return null;
            }
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(
      IntPtr hWnd,
      int X,
      int Y,
      int nWidth,
      int nHeight,
      bool bRepaint);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);
        public struct rgb
        {
            public byte r;
            public byte g;
            public byte b;
            public rgb(byte r, byte g, byte b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }
            public rgb(RGBQUAD rgbq)
            {
                r = rgbq.rgbRed;
                g = rgbq.rgbGreen;
                b = rgbq.rgbBlue;
            }
            public rgb(Color rgbcol)
            {
                r = rgbcol.R;
                g = rgbcol.G;
                b = rgbcol.B;
            }
            public rgb(int rgbwin32)
            {
                r = (byte)(rgbwin32 & 255);
                g = (byte)(rgbwin32 >> 8 & 255);
                b = (byte)(rgbwin32 >> 16 & 255);
            }
            public rgb(hsv hsv)
            {
                if (((hsv.h >= 0) && (hsv.h < 360)) && ((hsv.s >= 0) && (hsv.s <= 1)) && ((hsv.v >= 0) && (hsv.v <= 1)))
                {
                    double C = hsv.v * hsv.s;
                    double X = C * (1 - abs(((hsv.h / 60d) % 2) - 1));
                    double m = hsv.v - C;
                    double Rp = 0;
                    double Gp = 0;
                    double Bp = 0;
                    if ((hsv.h >= 0) && (hsv.h < 60))
                    {
                        Rp = C;
                        Gp = X;
                        Bp = 0;
                    }
                    else if ((hsv.h >= 60) && (hsv.h < 120))
                    {
                        Rp = X;
                        Gp = C;
                        Bp = 0;
                    }
                    else if ((hsv.h >= 120) && (hsv.h < 180))
                    {
                        Rp = 0;
                        Gp = C;
                        Bp = X;
                    }
                    else if ((hsv.h >= 180) && (hsv.h < 240))
                    {
                        Rp = 0;
                        Gp = X;
                        Bp = C;
                    }
                    else if ((hsv.h >= 240) && (hsv.h < 300))
                    {
                        Rp = X;
                        Gp = 0;
                        Bp = C;
                    }
                    else if ((hsv.h >= 300) && (hsv.h < 360))
                    {
                        Rp = C;
                        Gp = 0;
                        Bp = X;
                    }
                    r = (byte)((Rp + m) * 255);
                    g = (byte)((Gp + m) * 255);
                    b = (byte)((Bp + m) * 255);
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            public uint towin32()
            {
                return _RGB(r, g, b);
            }
            public Color tocol()
            {
                return RGB(r, g, b);
            }
        }
        public static double fmod(double xx, double yy)
        {
            long xxx = (long)(xx * 10000000);
            long yyy = (long)(yy * 10000000);
            xxx %= yyy;
            return xxx / 10000000d;
        }
        public struct hsv
        {
            public double h;
            public double s;
            public double v;
            public hsv(double h, double s, double l)
            {
                this.h = h;
                this.s = s;
                v = l;
            }
            public hsv(hsv k)
            {
                h = k.h;
                s = k.s;
                v = k.v;
            }
            public hsv(rgb rgb)
            {
                double R = rgb.r / 255d;
                double G = rgb.g / 255d;
                double B = rgb.b / 255d;
                double CM = max(max(R, G), B);
                double Cm = min(min(R, G), B);
                double DELTA = CM - Cm;
                double HUE = 0;
                double SAT = 0;
                double VAL = 0;
                if (DELTA == 0)
                {
                    HUE = 0;
                }
                else
                {
                    if (CM == R)
                    {
                        HUE = 60 * ((G - B) / DELTA % 6);
                    }
                    else if (CM == G)
                    {
                        HUE = 60 * ((B - R) / DELTA + 2);
                    }
                    else if (CM == B)
                    {
                        HUE = 60 * ((R - G) / DELTA + 4);
                    }
                }
                SAT = CM == 0 ? 0 : (DELTA / CM);
                VAL = CM;
                h = HUE;
                s = SAT;
                v = VAL;
            }
            public hsv addhue(double hue)
            {
                h += hue;
                h %= 360;
                h = abs(h);
                return this;
            }
            public hsv changehue(double hue)
            {
                h = hue;
                h %= 360;
                h = abs(h);
                return this;
            }
            public hsv changebright(double hue)
            {
                v = abs(fmod(hue, 1d));
                return this;
            }
            public hsv changeSAT(double sat)
            {
                s = abs(fmod(sat, 1d));
                return this;
            }

        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        public static readonly long SRCCOPY = 0x00CC0020;
        public static readonly long SRCINVERT = 0x00660046;
        public static readonly long SRCERASE = 0x00440328;
        public static readonly long NOTSRCCOPY = 0x00330008;
        public static readonly long NOTSRCERASE = 0x001100A6;
        public static readonly long MERGECOPY = 0x00C000CA;
        public static readonly long MERGEPAINT = 0x00BB0226;
        public static readonly long PATCOPY = 0x00F00021;
        public static readonly long PATPAINT = 0x00FB0A09;
        public static readonly long PATINVERT = 0x005A0049;
        public static readonly long DSTINVERT = 0x00550009;
        public static readonly long BLACKNESS = 0x00000042;
        public static readonly long WHITENESS = 0x00FF0062;
        public static readonly long CAPTUREBLT = 0x40000000;
        public static readonly long CUSTOM = 0x00100C85;
        public static readonly long RGBCOPY = 107385454862;
        public static readonly long SRCOR = 0x00EE0086;
        public static readonly long SRCXOR = 0x00660046;
        public static readonly long SRCAND = 0x008800C6;
        public static void CopyToDC(IntPtr hdcDest, IntPtr hdcSrc)
        {
            BitBlt(hdcDest, 0, 0, x, y, hdcSrc, 0, 0, SRCCOPY);
        }
        public static void CopyBlt(IntPtr hdcDest, IntPtr hdcSrc, long dwRop)
        {
            BitBlt(hdcDest, 0, 0, x, y, hdcSrc, 0, 0, dwRop);
        }
        public static double sin(double v)
        {
            if (double.IsNaN((float)v) || double.IsInfinity((float)Math.Abs(v)))
            {
                throw new NotImplementedException("really?");
            }
            return Math.Sin(v);
        }
        public static double cos(double v)
        {
            if (double.IsNaN((float)v) || double.IsInfinity((float)Math.Abs(v)))
            {
                throw new NotImplementedException("really?");
            }
            return Math.Cos(v);
        }
        public static int abs(int j)
        {
            return Math.Abs(j);
        }
        public static long abs(long j)
        {
            return Math.Abs(j);
        }

        public static double abs(double j)
        {
            return Math.Abs(j);
        }
        public static float abs(float j)
        {
            return Math.Abs(j);
        }
        public static double min(double v, double k)
        {
            return Math.Min(v, k);
        }
        public static double max(double v, double k)
        {
            return Math.Max(v, k);
        }
        public static void CircleBlt(IntPtr hdc, int x, int y, int radius, long dwRop)
        {
            IntPtr hrgn = CreateEllipticRgn(x - radius, y - radius, x + radius, y + radius);
            SelectClipRgn(hdc, hrgn);
            BitBlt(hdc, 0, 0, 879235498, 43673567, hdc, 0, 0, dwRop);
            DeleteObject(hrgn);
            IntPtr newrgn = CreateRectRgn(0, 0, x, y);
            SelectClipRgn(hdc, newrgn);
            DeleteObject(newrgn);
        }
        public static void BitBltf(IntPtr hdcDest, double xx, double yy, double sx, double sy, IntPtr hdcSrc, double xsr, double ysr, long dwrop)
        {
            BitBlt(hdcDest, (int)xx, (int)yy, (int)sx, (int)sy, hdcSrc, (int)xsr, (int)ysr, dwrop);
        }
        public static void StretchBltf(IntPtr hdcDest, double xx, double yy, double sx, double sy, IntPtr hdcSrc, double xsr, double ysr, double wsr, double hsr, long dwrop)
        {
            StretchBlt(hdcDest, (int)xx, (int)yy, (int)sx, (int)sy, hdcSrc, (int)xsr, (int)ysr, (int)wsr, (int)hsr, dwrop);
        }

        [DllImport("msimg32.dll")]
        public static extern bool TransparentBlt(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest, IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, uint crTransparent);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePolygonRgn(POINT[] lppt, int cPoints,
   int fnPolyFillMode);
        [DllImport("gdi32.dll")]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);
        public static void PolyBlt(IntPtr hdc, int sides, int xx, int yy, int radius, long dwRop, int ang, int sexp)
        {
            POINT[] p = new POINT[sides];
            for (int i = 0; i < sides; i++)
            {
                p[i] = new POINT((int)(xx + Math.Sin(Math.PI * 2 / sides * i * sexp + Math.PI / 180 * ang) * radius), (int)(yy + Math.Cos(Math.PI * 2 / sides * sexp * i + Math.PI / 180f * ang) * radius));
            }
            IntPtr hrgn = CreatePolygonRgn(p, sides, 2);
            SelectClipRgn(hdc, hrgn);
            BitBlt(hdc, xx - radius, yy - radius, 35677356, 36736755, hdc, xx - radius, yy - radius, dwRop);
            DeleteObject(hrgn);
            IntPtr newrgn = CreateRectRgn(0, 0, x, y);
            SelectClipRgn(hdc, newrgn);
            DeleteObject(newrgn);
        }
        public class XorRandom : Random
        {
            public int nextXor()
            {
                return XorYeet32();
            }
            public int nextXor(int max)
            {
                return XorYeet32() % max;
            }
            public int nextXor(int min, int max)
            {
                return (XorYeet32() % (max - min)) + min;
            }
        }
        public static int x = Screen.PrimaryScreen.Bounds.Width;
        public static int y = Screen.PrimaryScreen.Bounds.Height;
        public static XorRandom r = new XorRandom();
        public static int j = new Random().Next(int.MinValue, int.MaxValue);
        public static int rs = new Random().Next(int.MinValue, int.MaxValue);
        public static int rand()
        {
            long v = DateTime.Now.Millisecond;
            int v32 = (int)(v & int.MaxValue);
            int r1 = r.Next(int.MaxValue);
            r1 ^= r1 >> 7 | (r1 << 1 & int.MaxValue);
            r1 |= r1 >> 4 ^ (r1 >> 3);
            r1 ^= v32;
            rs ^= r.Next(int.MinValue, int.MaxValue) ^ r1;
            return rs %= int.MaxValue;
        }
        public static int seedXY(int seed)
        {
            return j = seed;
        }
        public static int XorYeet32()
        {
            long v = DateTime.Now.Ticks;
            int v32 = (int)(v & int.MaxValue);
            int t = v32;
            int INT32_MAX = int.MaxValue;
            seedXY(rand() % 7358321);
            j ^= rand() % 2774265;
            j |= rand() % 1697837;
            j ^= j >> 7 | j << 3;
            j ^= t & INT32_MAX;
            j += 1;
            j &= INT32_MAX;
            return j & INT32_MAX;
        }
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect,
   int nRightRect, int nBottomRect);
        public static object _XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_HWHEEL = 0x01000;

        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            XDOWN = 0x00000080,
            XUP = 0x00000100
        }

        public enum MouseEventDataXButtons : uint
        {
            XBUTTON1 = 0x00000001,
            XBUTTON2 = 0x00000002
        }


        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(PenStyle fnPenStyle, int nWidth, uint crColor);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool MaskBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, IntPtr hbmMask, int xMask, int yMask, uint rop);
        [DllImport("gdi32.dll")]
        public static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, long dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        long dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool PlgBlt(IntPtr hdcDest, POINT[] lpPoint, IntPtr hdcSrc,
        int nXSrc, int nYSrc, int nWidth, int nHeight, IntPtr hbmMask, int xMask,
        int yMask);
        [DllImport("gdi32.dll")]
        public static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, long dwRop);
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr Ellipse(IntPtr hdc, int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect);
        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        BLENDFUNCTION blendFunction);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, byte[] lpvBits);
        [DllImport("gdi32.dll", EntryPoint = "CreateBitmap")]
        public static extern IntPtr CreateBitmapI(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, int[] lpvBits);
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern bool FloodFill(IntPtr hdc, int nXStart, int nYStart, uint crFill);
        [DllImport("gdi32.dll", EntryPoint = "GdiGradientFill", ExactSpelling = true)]
        public static extern unsafe bool GradientFill(
        IntPtr hdc,           // handle to DC
        TRIVERTEX[] pVertex,    // array of vertices
        uint dwNumVertex,     // number of vertices
        GRADIENT_TRIANGLE* pMesh, // array of gradient triangles, that each one keeps three indices in pVertex array, to determine its bounds
        uint dwNumMesh,       // number of gradient triangles to draw
        GRADIENT_FILL dwMode);           // Use only GRADIENT_FILL.TRIANGLE. Both values GRADIENT_FILL.RECT_H and GRADIENT_FILL.RECT_V are wrong in this overload!

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll")]
        public static extern bool FillRgn(IntPtr hdc, IntPtr hrgn, IntPtr hbr);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect,
        int nBottomRect);
        [DllImport("gdi32.dll")]
        public static extern bool Pie(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect,
        int nBottomRect, int nXRadial1, int nYRadial1, int nXRadial2, int nYRadial2);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        [DllImport("gdi32.dll")]
        public static extern uint SetPixel(IntPtr hdc, int X, int Y, int crColor);
        [DllImport("gdi32.dll")]
        public static extern IntPtr GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("gdi32.dll")]
        public static extern bool AngleArc(IntPtr hdc, int X, int Y, uint dwRadius,
        float eStartAngle, float eSweepAngle);
        [DllImport("gdi32.dll")]
        public static extern bool RoundRect(IntPtr hdc, int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteMetaFile(IntPtr hmf);
        [DllImport("gdi32.dll")]
        public static extern bool CancelDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern bool Polygon(IntPtr hdc, POINT[] lpPoints, int nCount);
        [DllImport("gdi32.dll")]

        public static extern int SetBitmapBits(IntPtr hbmp, int cBytes, RGBQUAD[] lpBits);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Beep(uint dwFreq, uint dwDuration);

        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool block);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
        int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibraryEx(IntPtr lpFileName, IntPtr hFile, LoadLibraryFlags dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadBitmap(IntPtr hInstance, string lpBitmapName);

        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        public static extern bool EndPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, StretchBltMode iStretchMode);

        [DllImport("gdi32.dll")]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest,
        int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth,
        int nSrcHeight, RGBQUAD rgbq, [In] ref BITMAPINFO lpBitsInfo, DIB_Color_Mode dib_mode,
        TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll")]
        public static extern int GetDIBits([In] IntPtr hdc, in IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] int[] lpvBits, ref BITMAPINFO lpbi, DIB_Color_Mode uUsage);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("Gdi32", EntryPoint = "GetBitmapBits")]
        public extern static long GetBitmapBits([In] IntPtr hbmp, [In] int cbBuffer, RGBQUAD[] lpvBits);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateHatchBrush(int iHatch, uint Color);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePatternBrush(IntPtr hbmp);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBitmap(IntPtr hdc, [In] ref BITMAPINFOHEADER
        lpbmih, uint fdwInit, byte[] lpbInit, [In] ref BITMAPINFO lpbmi,
        uint fuUsage);

        [DllImport("gdi32.dll")]
        public static extern int SetDIBitsToDevice(IntPtr hdc, int XDest, int YDest, uint
        dwWidth, uint dwHeight, int XSrc, int YSrc, uint uStartScan, uint cScanLines,
        byte[] lpvBits, [In] ref BITMAPINFO lpbmi, uint fuColorUse);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint line, int[] lpBits, [In] ref BITMAPINFO lpbmi, DIB_Color_Mode ColorUse);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Blue;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            /// <summary>
            /// A BITMAPINFOHEADER structure that contains information about the dimensions of color format.
            /// </summary>
            public BITMAPINFOHEADER bmiHeader;

            /// <summary>
            /// An array of RGBQUAD. The elements of the array that make up the color table.
            /// </summary>
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public RGBQUAD[] bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
            public uint biCompression;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }

        enum BitmapCompressionMode : uint
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        public enum DIB_Color_Mode : uint
        {
            DIB_RGB_COLORS = 0,
            DIB_PAL_COLORS = 1
        }

        public enum StretchBltMode : int
        {
            STRETCH_ANDSCANS = 1,
            STRETCH_ORSCANS = 2,
            STRETCH_DELETESCANS = 3,
            STRETCH_HALFTONE = 4,
        }

        /// <summary>
        /// The default flag; it does nothing. All it means is "not LR_MONOCHROME".
        /// </summary>
        public const int LR_DEFAULTCOLOR = 0x0000;

        /// <summary>
        /// Loads the image in black and white.
        /// </summary>
        public const int LR_MONOCHROME = 0x0001;

        /// <summary>
        /// Returns the original hImage if it satisfies the criteria for the copy—that is, correct dimensions and color depth—in
        /// which case the LR_COPYDELETEORG flag is ignored. If this flag is not specified, a new object is always created.
        /// </summary>
        public const int LR_COPYRETURNORG = 0x0004;

        /// <summary>
        /// Deletes the original image after creating the copy.
        /// </summary>
        public const int LR_COPYDELETEORG = 0x0008;

        /// <summary>
        /// Specifies the image to load. If the hinst parameter is non-NULL and the fuLoad parameter omits LR_LOADFROMFILE,
        /// lpszName specifies the image resource in the hinst module. If the image resource is to be loaded by name,
        /// the lpszName parameter is a pointer to a null-terminated string that contains the name of the image resource.
        /// If the image resource is to be loaded by ordinal, use the MAKEINTRESOURCE macro to convert the image ordinal
        /// into a form that can be passed to the LoadImage function.
        ///  
        /// If the hinst parameter is NULL and the fuLoad parameter omits the LR_LOADFROMFILE value,
        /// the lpszName specifies the OEM image to load. The OEM image identifiers are defined in Winuser.h and have the following prefixes.
        ///
        /// OBM_ OEM bitmaps
        /// OIC_ OEM icons
        /// OCR_ OEM cursors
        ///
        /// To pass these constants to the LoadImage function, use the MAKEINTRESOURCE macro. For example, to load the OCR_NORMAL cursor,
        /// pass MAKEINTRESOURCE(OCR_NORMAL) as the lpszName parameter and NULL as the hinst parameter.
        ///
        /// If the fuLoad parameter includes the LR_LOADFROMFILE value, lpszName is the name of the file that contains the image.
        /// </summary>
        public const int LR_LOADFROMFILE = 0x0010;

        /// <summary>
        /// Retrieves the color value of the first pixel in the image and replaces the corresponding entry in the color table
        /// with the default window color (COLOR_WINDOW). All pixels in the image that use that entry become the default window color.
        /// This value applies only to images that have corresponding color tables.
        /// Do not use this option if you are loading a bitmap with a color depth greater than 8bpp.
        ///
        /// If fuLoad includes both the LR_LOADTRANSPARENT and LR_LOADMAP3DCOLORS values, LRLOADTRANSPARENT takes precedence.
        /// However, the color table entry is replaced with COLOR_3DFACE rather than COLOR_WINDOW.
        /// </summary>
        public const int LR_LOADTRANSPARENT = 0x0020;

        /// <summary>
        /// Uses the width or height specified by the system metric values for cursors or icons,
        /// if the cxDesired or cyDesired values are set to zero. If this flag is not specified and cxDesired and cyDesired are set to zero,
        /// the function uses the actual resource size. If the resource contains multiple images, the function uses the size of the first image.
        /// </summary>
        public const int LR_DEFAULTSIZE = 0x0040;

        /// <summary>
        /// Uses true VGA colors.
        /// </summary>
        public const int LR_VGACOLOR = 0x0080;

        /// <summary>
        /// Searches the color table for the image and replaces the following shades of gray with the corresponding 3-D color: Color Replaced with
        /// Dk Gray, RGB(128,128,128) COLOR_3DSHADOW
        /// Gray, RGB(192,192,192) COLOR_3DFACE
        /// Lt Gray, RGB(223,223,223) COLOR_3DLIGHT
        /// Do not use this option if you are loading a bitmap with a color depth greater than 8bpp.
        /// </summary>
        public const int LR_LOADMAP3DCOLORS = 0x1000;

        /// <summary>
        /// When the uType parameter specifies IMAGE_BITMAP, causes the function to return a DIB section bitmap rather than a compatible bitmap.
        /// This flag is useful for loading a bitmap without mapping it to the colors of the display device.
        /// </summary>
        public const int LR_CREATEDIBSECTION = 0x2000;

        /// <summary>
        /// Tries to reload an icon or cursor resource from the original resource file rather than simply copying the current image.
        /// This is useful for creating a different-sized copy when the resource file contains multiple sizes of the resource.
        /// Without this flag, CopyImage stretches the original image to the new size. If this flag is set, CopyImage uses the size
        /// in the resource file closest to the desired size. This will succeed only if hImage was loaded by LoadIcon or LoadCursor,
        /// or by LoadImage with the LR_SHARED flag.
        /// </summary>
        public const int LR_COPYFROMRESOURCE = 0x4000;

        /// <summary>
        /// Shares the image handle if the image is loaded multiple times. If LR_SHARED is not set, a second call to LoadImage for the
        /// same resource will load the image again and return a different handle.
        /// When you use this flag, the system will destroy the resource when it is no longer needed.
        ///
        /// Do not use LR_SHARED for images that have non-standard sizes, that may change after loading, or that are loaded from a file.
        ///
        /// When loading a system icon or cursor, you must use LR_SHARED or the function will fail to load the resource.
        ///
        /// Windows 95/98/Me: The function finds the first image with the requested resource name in the cache, regardless of the size requested.
        /// </summary>
        public const int LR_SHARED = 0x8000;

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        public enum PenStyle : int
        {
            PS_SOLID = 0, //The pen is solid.
            PS_DASH = 1, //The pen is dashed.
            PS_DOT = 2, //The pen is dotted.
            PS_DASHDOT = 3, //The pen has alternating dashes and dots.
            PS_DASHDOTDOT = 4, //The pen has alternating dashes and double dots.
            PS_NULL = 5, //The pen is invisible.
            PS_INSIDEFRAME = 6,// Normally when the edge is drawn, it’s centred on the outer edge meaning that half the width of the pen is drawn
                               // outside the shape’s edge, half is inside the shape’s edge. When PS_INSIDEFRAME is specified the edge is drawn
                               //completely inside the outer edge of the shape.
            PS_USERSTYLE = 7,
            PS_ALTERNATE = 8,
            PS_STYLE_MASK = 0x0000000F,

            PS_ENDCAP_ROUND = 0x00000000,
            PS_ENDCAP_SQUARE = 0x00000100,
            PS_ENDCAP_FLAT = 0x00000200,
            PS_ENDCAP_MASK = 0x00000F00,

            PS_JOIN_ROUND = 0x00000000,
            PS_JOIN_BEVEL = 0x00001000,
            PS_JOIN_MITER = 0x00002000,
            PS_JOIN_MASK = 0x0000F000,

            PS_COSMETIC = 0x00000000,
            PS_GEOMETRIC = 0x00010000,
            PS_TYPE_MASK = 0x000F0000
        };
        public enum TernaryRasterOperations : long
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000,
            CUSTOM = 0x00100C85,
            RGBCOPY = 107385454862,
            SRCOR = SRCPAINT,
            SRCXOR = SRCINVERT
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static implicit operator Point(POINT p)
            {
                return new Point(p.x, p.y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            byte BlendOp;
            byte BlendFlags;
            byte SourceConstantAlpha;
            byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }

        //
        // currently defined blend operation
        //
        const int AC_SRC_OVER = 0x00;

        //
        // currently defined alpha format
        //
        const int AC_SRC_ALPHA = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        public struct GRADIENT_RECT
        {
            public uint UpperLeft;
            public uint LowerRight;

            public GRADIENT_RECT(uint upLeft, uint lowRight)
            {
                this.UpperLeft = upLeft;
                this.LowerRight = lowRight;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TRIVERTEX
        {
            public int x;
            public int y;
            public ushort Red;
            public ushort Green;
            public ushort Blue;
            public ushort Alpha;

            public TRIVERTEX(int x, int y, ushort red, ushort green, ushort blue, ushort alpha)
            {
                this.x = x;
                this.y = y;
                this.Red = red;
                this.Green = green;
                this.Blue = blue;
                this.Alpha = alpha;
            }
        }
        public enum GRADIENT_FILL : uint
        {
            RECT_H = 0,
            RECT_V = 1,
            TRIANGLE = 2,
            OP_FLAG = 0xff
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct GRADIENT_TRIANGLE
        {
            public uint Vertex1;
            public uint Vertex2;
            public uint Vertex3;

            public GRADIENT_TRIANGLE(uint vertex1, uint vertex2, uint vertex3)
            {
                this.Vertex1 = vertex1;
                this.Vertex2 = vertex2;
                this.Vertex3 = vertex3;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RGBTRIPLE
        {
            public byte rgbtBlue;
            public byte rgbtGreen;
            public byte rgbtRed;
        }

        public static Color RGB(int r, int g, int b)
        {
            return Color.FromArgb(r & 255, g & 255, b & 255);
        }
        public static uint _RGB(byte r, byte g, byte b)
        {
            return (uint)(r | g << 8 | b << 16);
        }
        public static uint _RGB(int r, int g, int b)
        {
            r &= 255;
            g &= 255;
            b &= 255;
            return (uint)(r | g << 8 | b << 16);
        }
        public static void RemoveDC(IntPtr hdc)
        {
            ReleaseDC(IntPtr.Zero, hdc);
            DeleteDC(hdc);
        }
        public static void invertsquare(IntPtr hdc, int X, int Y, int i, bool usecols, uint col)
        {
            int xX = X;
            int yY = Y;
            if (X == -1)
            {
                xX = x / 2;
            }
            if (Y == -1)
            {
                yY = y / 2;
            }
            TernaryRasterOperations v = TernaryRasterOperations.NOTSRCCOPY;
            IntPtr brush = CreateSolidBrush(col);
            SelectObject(hdc, brush);
            if (usecols)
            {
                v = TernaryRasterOperations.PATINVERT;
            }
            StretchBlt(hdc, xX - i / 2, yY - i / 2, i, i, hdc, xX - i / 2, yY - i / 2, i, i, (long)v); //Inverts all colors
            DeleteObject(brush);
        }
        [DllImport("gdi32.dll")]
        public static extern uint SetBkColor(IntPtr hdc, uint crColor);
        public static IntPtr makeDC()
        {
            return GetDC(IntPtr.Zero);
        }
        public static IntPtr Mdc(IntPtr hdc)
        {
            return CreateCompatibleDC(hdc);
        }
        public static IntPtr bitmap(IntPtr hdc)
        {
            return CreateCompatibleBitmap(hdc, x, y);
        }
        public static int antiNegative(int xx, int yy)
        {
            int res = xx % yy;
            while (res < 0)
            {
                res += yy;
            }
            return res;
        }
        [Flags]
        public enum RedrawWindowFlags : uint
        {
            Invalidate = 1u,
            InternalPaint = 2u,
            Erase = 4u,
            Validate = 8u,
            NoInternalPaint = 0x10u,
            NoErase = 0x20u,
            NoChildren = 0x40u,
            AllChildren = 0x80u,
            UpdateNow = 0x100u,
            EraseNow = 0x200u,
            Frame = 0x400u,
            NoFrame = 0x800u
        }
        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);
        [DllImport("user32.dll")]
        public static extern int DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);
        [DllImport("user32.dll")]
        public static extern int DrawIconEx(IntPtr hDC, int X, int Y, IntPtr hIcon, int cw, int ch, uint istepifanicur, IntPtr hbrflg, uint diflags);
        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);
        public static IntPtr IDI_APPLICATION = MAKEINTRESOURCE(32512);
        public static IntPtr IDI_ERROR = MAKEINTRESOURCE(32513);
        public static IntPtr IDI_QUESTION = MAKEINTRESOURCE(32514);
        public static IntPtr IDI_WARNING = MAKEINTRESOURCE(32515);
        public static IntPtr IDI_ASTERISK = MAKEINTRESOURCE(32516);
        public static IntPtr MAKEINTRESOURCE(int ñ32513)
        {
            return new IntPtr(ñ32513);
        }
        public static Bitmap getScreen()
        {
            try
            {
                Bitmap bmp = new Bitmap(x, y);
                Graphics.FromImage(bmp).CopyFromScreen(0, 0, 0, 0, bmp.Size);
                return bmp;
            }
            catch { }
            return null;
        }
        public static void changecols(IntPtr hdc, int rgbcol32)
        {
            try
            {
                BITMAPINFO bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmi.bmiHeader.biWidth = x;
                bmi.bmiHeader.biHeight = -y;
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = 0;
                byte[] pixelData = new byte[x * y * 4];
                Marshal.Copy(getScreen().LockBits(new Rectangle(0, 0, x, y), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb).Scan0, pixelData, 0, pixelData.Length);
                for (int yy = 0; yy < y; yy++)
                {
                    for (int xx = 0; xx < x; xx++)
                    {
                        int index = (yy * x + xx) * 4;
                        pixelData[index] += (byte)(rgbcol32);
                        pixelData[index + 1] += (byte)(rgbcol32 >> 8);
                        pixelData[index + 2] += (byte)(rgbcol32 >> 16);
                    }
                }
                SetDIBitsToDevice(hdc, 0, 0, (uint)x, (uint)y, 0, 0, 0, (uint)y, pixelData, ref bmi, 0);
            }
            catch { }
        }
        public static byte[] randByteArray(int size, byte maxi)
        {
            byte[] result = new byte[size];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(XorYeet32() % (maxi + 1));
            }
            return result;
        }
        public static int[] randIntArray(int size, int maxi)
        {
            int[] result = new int[size];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (XorYeet32() % (maxi + 1));
            }
            return result;
        }
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                if (codec.MimeType == mimeType)
                    return codec;

            return null;
        }
        public MemoryStream compress(Image img, long quality)
        {
            ImageCodecInfo codec = GetEncoderInfo("image/jpeg");
            if (codec == null)
            {
                throw new NoNullAllowedException("uhh why is my codec null?");
            }

            EncoderParameters parames = new EncoderParameters(1);
            parames.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            var ms = new MemoryStream();
            img.Save(ms, codec, parames);
            return ms;
        }
        public static Image SetOpacity(Image image, float opacity)
        {
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = opacity;
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            var output = new Bitmap(image.Width, image.Height);
            using (var gfx = Graphics.FromImage(output))
            {
                gfx.DrawImage(
                    image,
                    new Rectangle(0, 0, image.Width, image.Height),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
            return output;
        }
        public static void pixelateDC(IntPtr hdc, int amount)
        {
            IntPtr mdc = Mdc(hdc);
            IntPtr bmp = bitmap(hdc);
            SelectObject(mdc, bmp);
            CopyToDC(mdc, hdc);
            SetStretchBltMode(hdc, StretchBltMode.STRETCH_DELETESCANS);
            SetStretchBltMode(mdc, StretchBltMode.STRETCH_DELETESCANS);
            StretchBlt(mdc, 0, 0, x / amount, y / amount, mdc, 0, 0, x, y, SRCCOPY);
            StretchBlt(hdc, 0, 0, x, y, mdc, 0, 0, x / amount, y / amount, SRCCOPY);
            DeleteObject(bmp);
            RemoveDC(mdc);
        }
        public static string Corrupt_String(string str)
        {
            if (str == "" || str == null)
            {
                return str;
            }
            char[] chh = str.ToCharArray();
            byte[] randb = randByteArray(chh.Length, byte.MaxValue);
            for (int i = 0; i < chh.Length; i++)
            {
                if ((rand() & 3) == 0)
                {
                    char[] att = Encoding.ASCII.GetChars(randb);
                    chh[i] = att[XorYeet32() % att.Length];
                }
            }
            return new string(chh);
        }
        public enum typesChr : int
        {
            VERTICAL = 0,
            HORIZONTAL = 1,
            DIAGONAL1 = 2,
            DIAGONAL2 = 3
        }
        public static void chroma(IntPtr hdc, typesChr type, int intensity)
        {
            IntPtr mdc = Mdc(hdc);
            IntPtr bmp = bitmap(hdc);
            SelectObject(mdc, bmp);
            CopyToDC(mdc, hdc);
            if (type == typesChr.VERTICAL)
            {
                IntPtr bru = CreateSolidBrush(_RGB(255, 0, 0));
                SelectObject(mdc, bru);
                BitBlt(mdc, 0, intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru);
                IntPtr bru2 = CreateSolidBrush(_RGB(0, 0, 255));
                SelectObject(mdc, bru2);
                BitBlt(mdc, 0, -intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru2);
            }
            else if (type == typesChr.HORIZONTAL)
            {
                IntPtr bru = CreateSolidBrush(_RGB(255, 0, 0));
                SelectObject(mdc, bru);
                BitBlt(mdc, intensity, 0, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru);
                IntPtr bru2 = CreateSolidBrush(_RGB(0, 0, 255));
                SelectObject(mdc, bru2);
                BitBlt(mdc, -intensity, 0, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru2);
            }
            else if (type == typesChr.DIAGONAL1)
            {
                IntPtr bru = CreateSolidBrush(_RGB(255, 0, 0));
                SelectObject(mdc, bru);
                BitBlt(mdc, intensity, intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru);
                IntPtr bru2 = CreateSolidBrush(_RGB(0, 0, 255));
                SelectObject(mdc, bru2);
                BitBlt(mdc, -intensity, -intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru2);
            }
            else if (type == typesChr.DIAGONAL2)
            {
                IntPtr bru = CreateSolidBrush(_RGB(255, 0, 0));
                SelectObject(mdc, bru);
                BitBlt(mdc, intensity, -intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru);
                IntPtr bru2 = CreateSolidBrush(_RGB(0, 0, 255));
                SelectObject(mdc, bru2);
                BitBlt(mdc, -intensity, intensity, x, y, mdc, 0, 0, RGBCOPY);
                DeleteObject(bru2);
            }
            CopyToDC(hdc, mdc);
            DeleteObject(bmp);
            RemoveDC(mdc);
        }
        public static void PixelateBlt(IntPtr hdc, IntPtr source, int amount)
        {
            IntPtr mdc = Mdc(hdc);
            IntPtr bmp = bitmap(hdc);
            SelectObject(mdc, bmp);
            CopyToDC(mdc, source);
            SetStretchBltMode(mdc, StretchBltMode.STRETCH_DELETESCANS);
            SetStretchBltMode(hdc, StretchBltMode.STRETCH_DELETESCANS);
            StretchBlt(mdc, 0, 0, x / amount, y / amount, mdc, 0, 0, x, y, SRCCOPY);
            StretchBlt(hdc, 0, 0, x, y, mdc, 0, 0, x / amount, y / amount, SRCCOPY);
            DeleteObject(bmp);
            RemoveDC(mdc);
        }
        public static void weird(IntPtr hdc)
        {
            try
            {
                BITMAPINFO bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmi.bmiHeader.biWidth = x;
                bmi.bmiHeader.biHeight = -y;
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = 0;
                byte[] ptr = new byte[x * y * 4];
                Marshal.Copy(getScreen().LockBits(new Rectangle(0, 0, x, y), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb).Scan0, ptr, 0, ptr.Length);
                int ee = 0;
                byte r = XYB();
                byte g = XYB();
                byte b = XYB();
                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        if (ptr[ee + 2] < r)
                        {
                            ptr[ee + 2] += r; //Red
                        }
                        if (ptr[ee + 1] < g)
                        {
                            ptr[ee + 1] += g; //Green
                        }
                        if (ptr[ee] < b)
                        {
                            ptr[ee] += b; //Blue
                        }
                        ee += 4;
                    }
                }
                SetDIBitsToDevice(hdc, 0, 0, (uint)x, (uint)y, 0, 0, 0, (uint)y, ptr, ref bmi, 0);
            }
            catch { }
        }
        public static byte XYB()
        {
            return (byte)XorYeet32();
        }
    }
}
