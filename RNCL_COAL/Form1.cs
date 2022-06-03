using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UHF;
using System.Threading;
using System.IO;
using System.Net;
using System.Diagnostics;




namespace RNCL_COAL
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        Bitmap bmp = new Bitmap(384, 288);

        int max_i, max_j, min_i, min_j;

        ushort[,] palleteArr = new ushort[1001, 3];
        Bitmap paletteImg = new Bitmap("Iron.png");
        bool liveStream = true;

       

        public Form1()
        {
            InitializeComponent();





        }
        

        private void btnConnect_Click(object sender, EventArgs e)
        {
            IntPtr hWnd = this.Handle;

            int bRes = DLLHelper.UsbOpenDevice(hWnd, 0);
            if (bRes == 1)
            {
                textBox1.Text = "Connected!";
            }
            else
            {
                textBox1.Text = "Err:" + bRes.ToString();
                return;
            }

            bRes = DLLHelper.ReadFlashData(0);
            if (bRes == 1)
            {
                textBox1.Text = "Flash read completed";
                btnStream.Enabled = true;
                btnDisconnect.Enabled = true;
                btnCalibrate.Enabled = true;
            }
            else
            {
                textBox1.Text = "Err:" + bRes.ToString();
            }

            //tempTimer.Enabled = true;
        }

        private void btnStream_Click(object sender, EventArgs e)
        {
            
            liveStream = true;
            StartStreaming streamData = new StartStreaming(StreamData);
            streamData.BeginInvoke(null, null);
            

            Thread threadTemp = new Thread(() => GetTemp());
            threadTemp.Start();

            if (lblHigh.Visible == false) lblHigh.Visible = true;
            if (lblLow.Visible == false) lblLow.Visible = true;



            //tempTimer.Enabled = true;



        }
       

        delegate void StartStreaming();

        void StreamData()
        {
            ushort[] imageData = new ushort[288 * 384];
            byte[] bArr = new byte[288 * 384];

            byte[] bArr_Color;// = new byte[288 * 384*3];

            Bitmap b = new Bitmap(384, 288, PixelFormat.Format8bppIndexed);

            ColorPalette ncp = b.Palette;
            Rectangle BoundsRect = new Rectangle(0, 0, 384, 288);
            BitmapData bmpData;
            IntPtr ptr;

            Bitmap Colored = new Bitmap(384, 288, PixelFormat.Format24bppRgb);

            //=============Color Bar Bitmap=============

            byte[] bArr_Colorbar;//= new byte[288 * 24*3];
            Bitmap bmapColorBar = new Bitmap(24, 288, PixelFormat.Format24bppRgb);

            Bitmap bmapColorBar_Binary = new Bitmap(24, 288, PixelFormat.Format8bppIndexed);
            ColorPalette ncp_ = bmapColorBar_Binary.Palette;

            Rectangle ColorBarBoundsRect = new Rectangle(0, 0, 24, 288);
            IntPtr ptr_bar;
            BitmapData bmapColorBarData;

            //==========================================


            for (int i = 0; i < 256; i++)
                ncp.Entries[i] = Color.FromArgb(255, i, i, i);
            b.Palette = ncp;


            for (int i = 0; i < 256; i++)
                ncp_.Entries[i] = Color.FromArgb(255, i, i, i);
            bmapColorBar_Binary.Palette = ncp_;


            while (liveStream)
            {
                DLLHelper.RecvImage(imageData, 0);



                for (int n = 0; n < 288 * 384; n++)
                {
                    bArr[n] = (byte)(imageData[n] >> 8);
                }

                try
                {
                    if (ColorMap.colorMap_Type == ColorMap_Type.WhiteHot)
                    {

                        bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        ptr = bmpData.Scan0;
                        Marshal.Copy(bArr, 0, ptr, bmpData.Stride * 288);
                        b.UnlockBits(bmpData);
                        RefreshImage(b);



                        //     ========= Color Bar ========================

                        // bArr_Colorbar = new byte[288 * 24];

                        ColorMap.ColorBar(bArr, out bArr_Colorbar, max_i, max_j, min_i, min_j);

                        bmapColorBarData = bmapColorBar_Binary.LockBits(ColorBarBoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        ptr_bar = bmapColorBarData.Scan0;
                        Marshal.Copy(bArr_Colorbar, 0, ptr_bar, bmapColorBarData.Stride * 288);
                        bmapColorBar_Binary.UnlockBits(bmapColorBarData);

                        RefreshImage_Bar(bmapColorBar_Binary);


                    }
                    else if (ColorMap.colorMap_Type == ColorMap_Type.BlackHot)
                    {

                        ColorMap.Change_ColorMap_BlackHot(bArr, out bArr_Color);

                        bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        ptr = bmpData.Scan0;
                        Marshal.Copy(bArr_Color, 0, ptr, bmpData.Stride * 288);
                        b.UnlockBits(bmpData);
                        RefreshImage(b);

                        //     ========= Color Bar ========================
                        //  bArr_Colorbar = new byte[288 * 24];


                        ColorMap.ColorBar(bArr, out bArr_Colorbar, max_i, max_j, min_i, min_j);

                        bmapColorBarData = bmapColorBar_Binary.LockBits(ColorBarBoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        ptr_bar = bmapColorBarData.Scan0;
                        Marshal.Copy(bArr_Colorbar, 0, ptr_bar, bmapColorBarData.Stride * 288);
                        bmapColorBar_Binary.UnlockBits(bmapColorBarData);

                        RefreshImage_Bar(bmapColorBar_Binary);

                    }
                    else
                    {
                        // bArr_Colorbar = new byte[288 * 24*3];

                        if (ColorMap.colorMap_Type == ColorMap_Type.Iron)
                            ColorMap.Change_ColorMap_IRon(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.Medical)
                            ColorMap.Change_ColorMap_Medical(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.Rainbow)
                            ColorMap.Change_ColorMap_Rainbow(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.BlueRed)
                            ColorMap.Change_ColorMap_BlueRed(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.Purple)
                            ColorMap.Change_ColorMap_Purple(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.PurpleYellow)
                            ColorMap.Change_ColorMap_PurpleYellow(bArr, out bArr_Color);
                        else if (ColorMap.colorMap_Type == ColorMap_Type.DarkBlue)
                            ColorMap.Change_ColorMap_DarkBlue(bArr, out bArr_Color);
                        else ColorMap.Change_ColorMap_Cyan(bArr, out bArr_Color);


                        //     ========= Color Bar ========================


                        ColorMap.ColorBar(bArr, out bArr_Colorbar, max_i, max_j, min_i, min_j);

                        bmapColorBarData = bmapColorBar.LockBits(ColorBarBoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb); ;
                        ptr_bar = bmapColorBarData.Scan0;
                        Marshal.Copy(bArr_Colorbar, 0, ptr_bar, bmapColorBarData.Stride * 288);
                        bmapColorBar.UnlockBits(bmapColorBarData);

                        RefreshImage_Bar(bmapColorBar);


                        //      ===================================

                        bmpData = Colored.LockBits(BoundsRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        ptr = bmpData.Scan0;
                        Marshal.Copy(bArr_Color, 0, ptr, bmpData.Stride * 288);
                        Colored.UnlockBits(bmpData);
                        RefreshImage(Colored);


                    }
                }
                catch { }

            }
        }

        delegate void RefreshImageDelegate(Bitmap bmpData);
        delegate void RefreshImage_Bar_Delegate(Bitmap bmpData);

        double[,] temp = new double[384, 288];

        public void GetTemp()
        {
            while (liveStream)
            {
                for (int i = 0; i < 384; i++)
                {
                    for (int j = 0; j < 288; j++)
                    {
                        temp[i, j] = DLLHelper.CalcTemp(i, j, false, 0);

                    }
                  
                }
                Thread.Sleep(50);
            }
           
        }

        void GetTempData()
        {

            for(int i = 0; i < 384; i++)
            {
                for(int j = 0; j < 288; j++)
                {
                    temp[i,j] = DLLHelper.CalcTemp(i, j, false, 0);
                }
            }

        }

        void RefreshImage(Bitmap bmpData)
        {
            if (pictureBox1.InvokeRequired == false)
            {
                pictureBox1.Image = bmpData;
                if (btnStop.Enabled == false) btnStop.Enabled = true;
                if (btnCalibrate.Enabled == true && liveStream == true) btnCalibrate.Enabled = false;
                double high, low;
                // int i, j, k, l;
                // MaxMin_Temp(out high, out low, out i, out j, out k, out l) ;
                //GetTempData();

                MaxMin_Temp(out high, out low);
                lblHigh.Text = Convert.ToString(Math.Round(high, 3) + "℃");
                lblLow.Text = Convert.ToString(Math.Round(low, 3) + "℃");
                //======== visual============
                lblHighTmp.Left = pictureBox1.Left + max_i;
                lblHighTmp.Top = pictureBox1.Top + max_j;

                lblLowTemp.Left = pictureBox1.Left + min_i;
                lblLowTemp.Top = pictureBox1.Top + min_j;

                //========================



            }
            else
            {
                RefreshImageDelegate showProgress = new RefreshImageDelegate(RefreshImage);
                BeginInvoke(showProgress, new object[] { bmpData });
            }
        }

        //=====================color Bar=============================

        void RefreshImage_Bar(Bitmap bmpData)
        {
            if (picColorBar.InvokeRequired == false)
            {
                picColorBar.Image = bmpData;
                if (btnStop.Enabled == false) btnStop.Enabled = true;
                if (btnCalibrate.Enabled == true && liveStream == true) btnCalibrate.Enabled = false;


            }
            else
            {
                RefreshImage_Bar_Delegate showProgress = new RefreshImage_Bar_Delegate(RefreshImage_Bar);
                BeginInvoke(showProgress, new object[] { bmpData });
            }
        }
        //===============================================================

        private void btnStop_Click(object sender, EventArgs e)
        {
            liveStream = false;
            btnCalibrate.Enabled = true;

            
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DLLHelper.UsbCloseDevice(0);
            textBox1.Text = "Disconected!";
        }

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            bool bRes = DLLHelper.ShutterCalibrationOn(0);
            if (bRes)
            {
                textBox1.Text = "Calibration completed";
            }
            else
            {
                textBox1.Text = "Err:" + bRes.ToString();
            }
        }

        private void tempTimer_Tick(object sender, EventArgs e)
        {
            GetTempData();
            //Point mousePos = new Point();
            //double temp=DLLHelper.CalcTemp(Cursor.Position.X, Cursor.Position.Y,false,0);
            //tempBox.Text = Convert.ToString(temp);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;

            label4.Text = Convert.ToString(e.X + " , " + pictureBox1.Width);
           
            
                double temp = DLLHelper.CalcTemp(e.X, e.Y, false, 0);
                tempBox.Text = Convert.ToString(Math.Round(temp, 1) + "℃");
            
     
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            tempBox.Text = "None";
        }


        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image == null) return;


            label1.Text = Convert.ToString(max_i) + "," + Convert.ToString(max_j) + "," + Convert.ToString(min_i) + "," + Convert.ToString(min_j);
            liveStream = false;
            btnCalibrate.Enabled = true;

            string saveFolder = @"D:\imagesave";
            if (!System.IO.Directory.Exists(saveFolder))
                System.IO.Directory.CreateDirectory(saveFolder);


            pictureBox1.Image.Save(saveFolder + "\\" + FileUploadName(saveFolder, "image.png"), System.Drawing.Imaging.ImageFormat.Png);
            pictureBox1.Image.Save(saveFolder + "\\" + FileUploadName(saveFolder, "image.bmp"), System.Drawing.Imaging.ImageFormat.Bmp);

            Save_TempData();

            liveStream = true;
            StartStreaming streamData = new StartStreaming(StreamData);
            streamData.BeginInvoke(null, null);

        }

        void Save_TempData()
        {

            string saveFolder = @"D:\imagesave";

            if (!System.IO.Directory.Exists(saveFolder))
                System.IO.Directory.CreateDirectory(saveFolder);

            System.IO.StreamWriter file = new System.IO.StreamWriter(saveFolder + "\\" + FileUploadName(saveFolder, "temperature.csv"));

            file.WriteLine("Row,Column,temperature");

            for (int i = 0; i < 384; i++)
            {
                for (int j = 0; j < 288; j++)
                {
                    double temp = DLLHelper.CalcTemp(i, j, false, 0);
                    file.WriteLine("{0},{1},{2}", i, j, temp);

                }
            }

            file.Close();


        }

        public string FileUploadName(String dirPath, String fileN)
        {
            string fileName = fileN;
            if (fileN.Length > 0)
            {
                int indexOfDot = fileName.LastIndexOf(".");
                string strName = fileName.Substring(0, indexOfDot);
                string strExt = fileName.Substring(indexOfDot);
                bool bExist = true;
                int fileCount = 0;
                string dirMapPath = string.Empty;
                while (bExist)
                {
                    dirMapPath = dirPath;
                    string pathCombine = System.IO.Path.Combine(dirMapPath, fileName);
                    if (System.IO.File.Exists(pathCombine))
                    {
                        fileCount++;
                        fileName = strName + "(" + fileCount + ")" + strExt;

                    }
                    else
                    {
                        bExist = false;
                    }
                }

            }

            return fileName;
        }

        #region TestCode
        Bitmap Change_ColorMap(Bitmap bitmap) // 망함 너무 느려짐.. -> bitmap 그대로 사용해서 그럼. bitmapdata, lockbits를 사용해야함.
        {
            Bitmap Outbitmap = new Bitmap(384, 288);

            int[,] temp = new int[256, 3]{ { 255, 255, 250 },{ 255, 255, 247 },{ 255, 255, 242 },{ 255, 255, 235 },{ 255, 254, 229 },{ 255, 254, 223 },{ 255, 253, 218 },
    { 255, 253, 215 },{ 255, 252, 210 },{ 255, 251, 204 },{ 255, 250, 197 },{ 255, 250, 191 },{ 255, 250, 184 },{ 255, 250, 176 },
    { 255, 249, 169 },{ 255, 248, 162 },{ 255, 247, 156 },{ 255, 247, 149 },{ 255, 245, 142 },{ 255, 245, 134 },{ 255, 244, 126 },
    { 255, 244, 118 },{ 255, 242, 111 },{ 255, 242, 104 },{ 255, 241, 98 },{ 255, 241, 93 },{ 255, 239, 85 },{ 255, 238, 77 },
    { 255, 237, 71 },{ 255, 236, 67 },{ 255, 234, 62 },{ 255, 233, 56 },{ 255, 232, 49 },{ 255, 231, 45 },{ 255, 230, 42 },{ 255, 228, 36 },
    { 255, 227, 32 },{ 255, 226, 28 },{ 255, 225, 23 },{ 255, 224, 19 },{ 255, 222, 18 },{ 255, 221, 15 },{ 255, 218, 11 },{ 255, 216, 10 },
    { 255, 215, 9 },{ 255, 213, 8 },{ 255, 211, 6 },{ 255, 209, 5 },{ 255, 208, 3 },{ 255, 207, 1 },{ 255, 206, 1 },{ 255, 205, 0 },
    { 255, 202, 0 },{ 255, 201, 0 },{ 255, 200, 1 },{ 255, 197, 0 },{ 255, 196, 1 },{ 255, 194, 1 },{ 255, 191, 1 },{ 255, 189, 0 },
    { 255, 187, 1 },{ 255, 186, 0 },{ 255, 184, 0 },{ 255, 182, 0 },{ 255, 180, 1 },{ 255, 180, 1 },{ 255, 178, 0 },{ 255, 177, 0 },
    { 255, 173, 0 },{ 255, 171, 0 },{ 255, 169, 1 },{ 255, 167, 2 },{ 255, 165, 2 },{ 255, 163, 0 },{ 255, 161, 1 },{ 255, 159, 1 },
    { 255, 157, 0 },{ 255, 155, 0 },{ 255, 153, 0 },{ 255, 150, 0 },{ 255, 149, 1 },{ 255, 147, 0 },{ 255, 145, 0 },{ 255, 143, 0 },
    { 255, 141, 0 },{ 255, 139, 0 },{ 255, 137, 0 },{ 255, 135, 0 },{ 255, 134, 0 },{ 255, 133, 0 },{ 255, 131, 0 },{ 255, 129, 0 },
    { 255, 127, 0 },{ 254, 125, 0 },{ 254, 124, 0 },{ 253, 123, 0 },{ 253, 122, 0 },{ 252, 121, 0 },{ 252, 120, 0 },{ 251, 119, 0 },
    { 251, 118, 0 },{ 250, 117, 0 },{ 250, 116, 0 },{ 249, 115, 0 },{ 249, 114, 0 },{ 248, 112, 0 },{ 248, 110, 0 },{ 247, 108, 0 },
    { 247, 106, 0 },{ 246, 104, 1 },{ 246, 102, 2 },{ 245, 100, 3 },{ 245, 98, 4 },{ 244, 96, 5 },{ 244, 94, 6 },{ 243, 92, 7 },
    { 243, 90, 8 },{ 242, 88, 9 },{ 242, 86, 10 },{ 241, 84, 11 },{ 241, 82, 12 },{ 240, 80, 13 },{ 240, 78, 14 },{ 239, 76, 15 },
    { 239, 74, 16 },{ 238, 72, 17 },{ 238, 70, 18 },{ 237, 68, 19 },{ 237, 66, 23 },{ 236, 64, 27 },{ 236, 62, 31 },{ 235, 60, 36 },
    { 235, 58, 41 },{ 234, 56, 46 },{ 233, 54, 51 },{ 232, 52, 56 },{ 231, 50, 61 },{ 230, 48, 66 },{ 229, 46, 71 },{ 228, 44, 76 },
    { 227, 42, 81 },{ 226, 40, 86 },{ 225, 38, 91 },{ 224, 36, 96 },{ 223, 34, 99 },{ 222, 32, 102 },{ 221, 30, 105 },{ 220, 28, 108 },
    { 219, 26, 111 },{ 218, 25, 114 },{ 217, 24, 117 },{ 216, 23, 120 },{ 215, 22, 123 },{ 214, 21, 126 },{ 213, 20, 129 },{ 212, 19, 132 },
    { 210, 18, 135 },{ 209, 17, 138 },{ 208, 16, 140 },{ 208, 15, 142 },{ 207, 14, 144 },{ 206, 13, 146 },{ 205, 12, 147 },{ 204, 11, 147 },
    { 202, 10, 148 },{ 200, 9, 148 },{ 198, 8, 149 },{ 196, 7, 149 },{ 194, 6, 150 },{ 192, 5, 150 },{ 190, 4, 151 },{ 188, 3, 151 },
    { 186, 2, 152 },{ 184, 1, 152 },{ 182, 0, 153 },{ 180, 0, 153 },{ 178, 0, 154 },{ 176, 0, 154 },{ 174, 0, 155 },{ 172, 0, 155 },
    { 170, 0, 156 },{ 168, 0, 156 },{ 166, 0, 157 },{ 164, 0, 157 },{ 162, 0, 158 },{ 160, 0, 158 },{ 158, 0, 159 },{ 156, 0, 159 },
    { 154, 0, 160 },{ 152, 0, 160 },{ 150, 0, 161 },{ 148, 0, 161 },{ 146, 0, 162 },{ 144, 1, 162 },{ 141, 0, 163 },{ 138, 0, 163 },
    { 135, 0, 164 },{ 132, 0, 164 },{ 129, 0, 165 },{ 126, 0, 165 },{ 123, 0, 166 },{ 120, 0, 166 },{ 117, 0, 165 },{ 114, 0, 164 },
    { 111, 0, 163 },{ 108, 0, 162 },{ 105, 0, 161 },{ 102, 0, 160 },{ 99, 0, 159 },{ 96, 0, 158 },{ 93, 0, 157 },{ 90, 0, 156 },
    { 87, 0, 155 },{ 84, 0, 154 },{ 81, 0, 153 },{ 78, 0, 153 },{ 75, 0, 152 },{ 72, 0, 152 },{ 69, 0, 151 },{ 66, 0, 151 },{ 63, 0, 150 },
    { 60, 0, 150 },{ 57, 0, 148 },{ 54, 0, 148 },{ 51, 0, 146 },{ 48, 0, 144 },{ 45, 0, 142 },{ 42, 0, 140 },{ 39, 0, 138 },{ 36, 0, 136 },
    { 34, 0, 134 },{ 32, 0, 132 },{ 30, 0, 130 },{ 28, 0, 128 },{ 26, 0, 126 },{ 24, 0, 124 },{ 22, 0, 122 },{ 20, 0, 120 },{ 18, 0, 118 },
    { 16, 0, 116 },{ 14, 0, 111 },{ 12, 0, 106 },{ 10, 0, 101 },{ 8, 0, 96 },{ 6, 0, 91 },{ 4, 0, 86 },{ 2, 0, 81 },{ 0, 0, 76 },
    { 0, 0, 71 },{ 0, 0, 66 },{ 0, 0, 61 },{ 0, 0, 56 },{ 1, 0, 51 },{ 1, 1, 46 },{ 0, 0, 41 },{ 0, 0, 36 } };


            for (int i = 0; i < 384; i++)
            {
                for (int j = 0; j < 288; j++)
                {
                    Color tmp = Color.FromArgb(temp[(int)bitmap.GetPixel(i, j).R, 0], temp[(int)bitmap.GetPixel(i, j).R, 1], temp[(int)bitmap.GetPixel(i, j).R, 2]);

                    Outbitmap.SetPixel(i, j, tmp);


                }
            }

            return Outbitmap;


        }

     
        void Change_ColorMap2(byte[] bArr, out byte[] rgbValues)
        {
            //  BitmapData Outbitmap;

            byte[,] temp = new byte[256, 3]{ { 0, 0, 0 },{ 8, 0, 8 },{ 16, 0, 16 },{ 24, 0, 24 },{ 32, 0, 32 },{ 40, 0, 40 },{ 48, 0, 48 },{ 56, 0, 56 },{ 65, 0, 65 },
    { 73, 0, 73 },{ 81, 0, 81 },{ 89, 0, 89 },{ 97, 0, 97 },{ 105, 0, 105 },{ 113, 0, 113 },{ 121, 0, 121 },{ 130, 0, 130 },
    { 129, 0, 130 },{ 127, 0, 130 },{ 125, 0, 130 },{ 123, 0, 131 },{ 121, 0, 131 },{ 119, 0, 131 },{ 117, 0, 132 },{ 115, 0, 132 },
    { 114, 0, 132 },{ 112, 0, 133 },{ 110, 0, 133 },{ 108, 0, 133 },{ 106, 0, 134 },{ 104, 0, 134 },{ 102, 0, 134 },{ 100, 0, 135 },
    { 98, 0, 135 },{ 95, 0, 135 },{ 93, 0, 135 },{ 90, 0, 136 },{ 88, 0, 136 },{ 85, 0, 136 },{ 83, 0, 137 },{ 80, 0, 137 },{ 78, 0, 137 },
    { 75, 0, 138 },{ 73, 0, 138 },{ 70, 0, 138 },{ 68, 0, 139 },{ 65, 0, 139 },{ 63, 0, 139 },{ 60, 0, 140 },{ 59, 0, 140 },{ 57, 0, 140 },
    { 55, 0, 140 },{ 53, 0, 141 },{ 51, 0, 141 },{ 49, 0, 141 },{ 47, 0, 142 },{ 45, 0, 142 },{ 44, 0, 142 },{ 42, 0, 143 },{ 40, 0, 143 },
    { 38, 0, 143 },{ 36, 0, 144 },{ 34, 0, 144 },{ 32, 0, 144 },{ 30, 0, 145 },{ 29, 0, 145 },{ 27, 0, 145 },{ 25, 0, 145 },{ 23, 0, 146 },
    { 21, 0, 146 },{ 19, 0, 146 },{ 17, 0, 147 },{ 15, 0, 147 },{ 14, 0, 147 },{ 12, 0, 148 },{ 10, 0, 148 },{ 8, 0, 148 },{ 6, 0, 149 },
    { 4, 0, 149 },{ 2, 0, 149 },{ 0, 0, 150 },{ 0, 5, 150 },{ 0, 11, 151 },{ 0, 16, 152 },{ 0, 22, 153 },{ 0, 28, 154 },{ 0, 33, 155 },
    { 0, 39, 156 },{ 0, 45, 157 },{ 0, 50, 158 },{ 0, 56, 159 },{ 0, 61, 160 },{ 0, 67, 161 },{ 0, 73, 162 },{ 0, 78, 163 },{ 0, 84, 164 },
    { 0, 90, 165 },{ 0, 95, 165 },{ 0, 101, 166 },{ 0, 106, 167 },{ 0, 112, 168 },{ 0, 118, 169 },{ 0, 123, 170 },{ 0, 129, 171 },
    { 0, 135, 172 },{ 0, 140, 173 },{ 0, 146, 174 },{ 0, 151, 175 },{ 0, 157, 176 },{ 0, 163, 177 },{ 0, 168, 178 },{ 0, 174, 179 },
    { 0, 180, 180 },{ 0, 180, 175 },{ 0, 181, 170 },{ 0, 181, 165 },{ 0, 182, 160 },{ 0, 183, 155 },{ 0, 183, 150 },{ 0, 184, 145 },
    { 0, 185, 140 },{ 0, 185, 135 },{ 0, 186, 130 },{ 0, 186, 125 },{ 0, 187, 120 },{ 0, 188, 115 },{ 0, 188, 110 },{ 0, 189, 105 },
    { 0, 190, 100 },{ 0, 190, 99 },{ 0, 191, 97 },{ 0, 191, 96 },{ 0, 192, 94 },{ 0, 193, 93 },{ 0, 193, 91 },{ 0, 194, 90 },
    { 0, 195, 88 },{ 0, 195, 86 },{ 0, 196, 85 },{ 0, 196, 83 },{ 0, 197, 82 },{ 0, 198, 80 },{ 0, 198, 79 },{ 0, 199, 77 },{ 0, 200, 75 },
    { 8, 201, 74 },{ 16, 203, 72 },{ 24, 205, 71 },{ 32, 207, 69 },{ 40, 209, 68 },{ 48, 211, 66 },{ 56, 213, 65 },{ 65, 215, 63 },
    { 73, 216, 61 },{ 81, 218, 60 },{ 89, 220, 58 },{ 97, 222, 57 },{ 105, 224, 55 },{ 113, 226, 54 },{ 121, 228, 52 },{ 130, 230, 50 },
    { 137, 231, 50 },{ 145, 233, 50 },{ 153, 234, 50 },{ 161, 236, 50 },{ 169, 237, 50 },{ 176, 239, 50 },{ 184, 240, 50 },{ 192, 242, 50 },
    { 200, 244, 50 },{ 208, 245, 50 },{ 215, 247, 50 },{ 223, 248, 50 },{ 231, 250, 50 },{ 239, 251, 50 },{ 247, 253, 50 },{ 255, 255, 50 },
    { 255, 252, 50 },{ 255, 249, 50 },{ 255, 245, 50 },{ 255, 242, 50 },{ 255, 238, 50 },{ 255, 235, 50 },{ 255, 231, 50 },{ 255, 228, 50 },
    { 255, 225, 50 },{ 255, 221, 50 },{ 255, 218, 50 },{ 255, 214, 50 },{ 255, 211, 50 },{ 255, 207, 50 },{ 255, 204, 50 },{ 255, 200, 50 },
    { 255, 197, 50 },{ 255, 194, 50 },{ 255, 191, 50 },{ 255, 188, 50 },{ 255, 185, 50 },{ 255, 182, 50 },{ 255, 179, 50 },{ 255, 175, 50 },
    { 255, 172, 50 },{ 255, 169, 50 },{ 255, 166, 50 },{ 255, 163, 50 },{ 255, 160, 50 },{ 255, 157, 50 },{ 255, 154, 50 },{ 255, 150, 50 },
    { 255, 147, 50 },{ 255, 144, 50 },{ 255, 141, 50 },{ 255, 138, 50 },{ 255, 135, 50 },{ 255, 132, 50 },{ 255, 129, 50 },{ 255, 125, 50 },
    { 255, 122, 50 },{ 255, 119, 50 },{ 255, 116, 50 },{ 255, 113, 50 },{ 255, 110, 50 },{ 255, 107, 50 },{ 255, 104, 50 },{ 255, 100, 50 },
    { 255, 97, 50 },{ 255, 94, 50 },{ 255, 91, 50 },{ 255, 88, 50 },{ 255, 85, 50 },{ 255, 82, 50 },{ 255, 79, 50 },{ 255, 75, 50 },
    { 255, 72, 50 },{ 255, 69, 50 },{ 255, 66, 50 },{ 255, 63, 50 },{ 255, 60, 50 },{ 255, 57, 50 },{ 255, 54, 50 },{ 255, 50, 50 },
    { 246, 50, 50 },{ 236, 50, 50 },{ 226, 50, 50 },{ 217, 50, 50 },{ 207, 50, 50 },{ 197, 50, 50 },{ 188, 50, 50 },{ 178, 50, 50 },
    { 168, 50, 50 },{ 159, 50, 50 },{ 149, 50, 50 },{ 139, 50, 50 },{ 130, 50, 50 },{ 120, 50, 50 },{ 110, 50, 50 } };


            rgbValues = new byte[288 * 384 * 3];

            int numBytes = 0;


            for (int i = 0; i < 384; i++)
            {
                for (int j = 0; j < 288; j++)
                {
                    numBytes = (i * (288 * 1)) + (j * 1);

                    rgbValues[numBytes * 3] = temp[bArr[numBytes], 2];
                    rgbValues[numBytes * 3 + 1] = temp[bArr[numBytes], 1];
                    rgbValues[numBytes * 3 + 2] = temp[bArr[numBytes], 0];

                }
            }


        }
        #endregion TestCode

        private void pictureBox2_Click(object sender, EventArgs e)
        {

            Bitmap bitmap = new Bitmap(30, 255);



            int[,] temp = new int[256, 3]{ { 0, 0, 0 },{ 8, 0, 8 },{ 16, 0, 16 },{ 24, 0, 24 },{ 32, 0, 32 },{ 40, 0, 40 },{ 48, 0, 48 },{ 56, 0, 56 },{ 65, 0, 65 },
    { 73, 0, 73 },{ 81, 0, 81 },{ 89, 0, 89 },{ 97, 0, 97 },{ 105, 0, 105 },{ 113, 0, 113 },{ 121, 0, 121 },{ 130, 0, 130 },
    { 129, 0, 130 },{ 127, 0, 130 },{ 125, 0, 130 },{ 123, 0, 131 },{ 121, 0, 131 },{ 119, 0, 131 },{ 117, 0, 132 },{ 115, 0, 132 },
    { 114, 0, 132 },{ 112, 0, 133 },{ 110, 0, 133 },{ 108, 0, 133 },{ 106, 0, 134 },{ 104, 0, 134 },{ 102, 0, 134 },{ 100, 0, 135 },
    { 98, 0, 135 },{ 95, 0, 135 },{ 93, 0, 135 },{ 90, 0, 136 },{ 88, 0, 136 },{ 85, 0, 136 },{ 83, 0, 137 },{ 80, 0, 137 },{ 78, 0, 137 },
    { 75, 0, 138 },{ 73, 0, 138 },{ 70, 0, 138 },{ 68, 0, 139 },{ 65, 0, 139 },{ 63, 0, 139 },{ 60, 0, 140 },{ 59, 0, 140 },{ 57, 0, 140 },
    { 55, 0, 140 },{ 53, 0, 141 },{ 51, 0, 141 },{ 49, 0, 141 },{ 47, 0, 142 },{ 45, 0, 142 },{ 44, 0, 142 },{ 42, 0, 143 },{ 40, 0, 143 },
    { 38, 0, 143 },{ 36, 0, 144 },{ 34, 0, 144 },{ 32, 0, 144 },{ 30, 0, 145 },{ 29, 0, 145 },{ 27, 0, 145 },{ 25, 0, 145 },{ 23, 0, 146 },
    { 21, 0, 146 },{ 19, 0, 146 },{ 17, 0, 147 },{ 15, 0, 147 },{ 14, 0, 147 },{ 12, 0, 148 },{ 10, 0, 148 },{ 8, 0, 148 },{ 6, 0, 149 },
    { 4, 0, 149 },{ 2, 0, 149 },{ 0, 0, 150 },{ 0, 5, 150 },{ 0, 11, 151 },{ 0, 16, 152 },{ 0, 22, 153 },{ 0, 28, 154 },{ 0, 33, 155 },
    { 0, 39, 156 },{ 0, 45, 157 },{ 0, 50, 158 },{ 0, 56, 159 },{ 0, 61, 160 },{ 0, 67, 161 },{ 0, 73, 162 },{ 0, 78, 163 },{ 0, 84, 164 },
    { 0, 90, 165 },{ 0, 95, 165 },{ 0, 101, 166 },{ 0, 106, 167 },{ 0, 112, 168 },{ 0, 118, 169 },{ 0, 123, 170 },{ 0, 129, 171 },
    { 0, 135, 172 },{ 0, 140, 173 },{ 0, 146, 174 },{ 0, 151, 175 },{ 0, 157, 176 },{ 0, 163, 177 },{ 0, 168, 178 },{ 0, 174, 179 },
    { 0, 180, 180 },{ 0, 180, 175 },{ 0, 181, 170 },{ 0, 181, 165 },{ 0, 182, 160 },{ 0, 183, 155 },{ 0, 183, 150 },{ 0, 184, 145 },
    { 0, 185, 140 },{ 0, 185, 135 },{ 0, 186, 130 },{ 0, 186, 125 },{ 0, 187, 120 },{ 0, 188, 115 },{ 0, 188, 110 },{ 0, 189, 105 },
    { 0, 190, 100 },{ 0, 190, 99 },{ 0, 191, 97 },{ 0, 191, 96 },{ 0, 192, 94 },{ 0, 193, 93 },{ 0, 193, 91 },{ 0, 194, 90 },
    { 0, 195, 88 },{ 0, 195, 86 },{ 0, 196, 85 },{ 0, 196, 83 },{ 0, 197, 82 },{ 0, 198, 80 },{ 0, 198, 79 },{ 0, 199, 77 },{ 0, 200, 75 },
    { 8, 201, 74 },{ 16, 203, 72 },{ 24, 205, 71 },{ 32, 207, 69 },{ 40, 209, 68 },{ 48, 211, 66 },{ 56, 213, 65 },{ 65, 215, 63 },
    { 73, 216, 61 },{ 81, 218, 60 },{ 89, 220, 58 },{ 97, 222, 57 },{ 105, 224, 55 },{ 113, 226, 54 },{ 121, 228, 52 },{ 130, 230, 50 },
    { 137, 231, 50 },{ 145, 233, 50 },{ 153, 234, 50 },{ 161, 236, 50 },{ 169, 237, 50 },{ 176, 239, 50 },{ 184, 240, 50 },{ 192, 242, 50 },
    { 200, 244, 50 },{ 208, 245, 50 },{ 215, 247, 50 },{ 223, 248, 50 },{ 231, 250, 50 },{ 239, 251, 50 },{ 247, 253, 50 },{ 255, 255, 50 },
    { 255, 252, 50 },{ 255, 249, 50 },{ 255, 245, 50 },{ 255, 242, 50 },{ 255, 238, 50 },{ 255, 235, 50 },{ 255, 231, 50 },{ 255, 228, 50 },
    { 255, 225, 50 },{ 255, 221, 50 },{ 255, 218, 50 },{ 255, 214, 50 },{ 255, 211, 50 },{ 255, 207, 50 },{ 255, 204, 50 },{ 255, 200, 50 },
    { 255, 197, 50 },{ 255, 194, 50 },{ 255, 191, 50 },{ 255, 188, 50 },{ 255, 185, 50 },{ 255, 182, 50 },{ 255, 179, 50 },{ 255, 175, 50 },
    { 255, 172, 50 },{ 255, 169, 50 },{ 255, 166, 50 },{ 255, 163, 50 },{ 255, 160, 50 },{ 255, 157, 50 },{ 255, 154, 50 },{ 255, 150, 50 },
    { 255, 147, 50 },{ 255, 144, 50 },{ 255, 141, 50 },{ 255, 138, 50 },{ 255, 135, 50 },{ 255, 132, 50 },{ 255, 129, 50 },{ 255, 125, 50 },
    { 255, 122, 50 },{ 255, 119, 50 },{ 255, 116, 50 },{ 255, 113, 50 },{ 255, 110, 50 },{ 255, 107, 50 },{ 255, 104, 50 },{ 255, 100, 50 },
    { 255, 97, 50 },{ 255, 94, 50 },{ 255, 91, 50 },{ 255, 88, 50 },{ 255, 85, 50 },{ 255, 82, 50 },{ 255, 79, 50 },{ 255, 75, 50 },
    { 255, 72, 50 },{ 255, 69, 50 },{ 255, 66, 50 },{ 255, 63, 50 },{ 255, 60, 50 },{ 255, 57, 50 },{ 255, 54, 50 },{ 255, 50, 50 },
    { 246, 50, 50 },{ 236, 50, 50 },{ 226, 50, 50 },{ 217, 50, 50 },{ 207, 50, 50 },{ 197, 50, 50 },{ 188, 50, 50 },{ 178, 50, 50 },
    { 168, 50, 50 },{ 159, 50, 50 },{ 149, 50, 50 },{ 139, 50, 50 },{ 130, 50, 50 },{ 120, 50, 50 },{ 110, 50, 50 } };



            for (int j = 0; j < 255; j++)
            {
                for (int i = 0; i < 30; i++)
                {
                    Color tmp = Color.FromArgb(temp[j, 0], temp[j, 1], temp[j, 2]);

                    bitmap.SetPixel(i, j, tmp);


                }
            }

            pictureBox2.Image = bitmap;
            //  bitmap
        }

        private void btnGetPotint_Click(object sender, EventArgs e)
        {
            if(lblHighTmp.Visible != true)
            {
                lblHighTmp.Visible = true;
                lblLowTemp.Visible = true;
            }
           else
            {
                lblHighTmp.Visible = false;
                lblLowTemp.Visible = false;
            }
         
            

        }
  

        private void btn_ConnectRF_Click(object sender, EventArgs e)
        {
            int portNum = Convert.ToInt32(txtPort.Text);
        }


     
        private void comboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedIndex > -1)
            {
                if (cb.SelectedIndex == 0) //White Hot
                    ColorMap.colorMap_Type = ColorMap_Type.WhiteHot;
                else if (cb.SelectedIndex == 1) //Black Hot
                    ColorMap.colorMap_Type = ColorMap_Type.BlackHot;
                else if (cb.SelectedIndex == 2) //Iron
                    ColorMap.colorMap_Type = ColorMap_Type.Iron;
                else if (cb.SelectedIndex == 3) //BlueRed
                    ColorMap.colorMap_Type = ColorMap_Type.BlueRed;
                else if (cb.SelectedIndex == 4) //Medical
                    ColorMap.colorMap_Type = ColorMap_Type.Medical;
                else if (cb.SelectedIndex == 5) //Purple
                    ColorMap.colorMap_Type = ColorMap_Type.Purple;
                else if (cb.SelectedIndex == 6) //PurpleYellow
                    ColorMap.colorMap_Type = ColorMap_Type.PurpleYellow;
                else if (cb.SelectedIndex == 7) //DarkBlue
                    ColorMap.colorMap_Type = ColorMap_Type.DarkBlue;
                else if (cb.SelectedIndex == 8) //Cyan
                    ColorMap.colorMap_Type = ColorMap_Type.Cyan;
                else if (cb.SelectedIndex == 9) //Rainbow
                    ColorMap.colorMap_Type = ColorMap_Type.Rainbow;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboColor.SelectedIndex = 0;

            com_Q.SelectedIndex = 4;
            com_Target.SelectedIndex = 0;
            com_S.SelectedIndex = 0;
            for (int i = 0x03; i <= 0xff; i++)
            {
                com_scantime.Items.Add(Convert.ToString(i) + "*100ms");
            }
               com_scantime.SelectedIndex = 7;
        }

        void MaxMin_Temp(out double max, out double min)//,out int max_i, out int max_j, out int min_i, out int min_j)
        {
            double max_temp = -999;
            double min_temp = 999;

           // double temp = 0;

            for (int i = 0; i < 384; i++)
            {
                for (int j = 0; j < 288; j++)
                {
                    
                  //  double temp = DLLHelper.CalcTemp(i, j, false, 0); // 매 tick 마다 구하려면 너무 느려짐!

                    if (max_temp < temp[i,j])
                    {
                        max_i = i;
                        max_j = j;
                        max_temp = temp[i, j];
                    }
                    if (min_temp > temp[i, j])
                    {
                        min_i = i;
                        min_j = j;
                        min_temp = temp[i, j];
                    }
                }
            }
            max = max_temp;
            min = min_temp;

        }


        #region RFID

       
        //byte fComAdr = 0xff; // 현재 동작하는 ComAdr
        //int fCmdRet = 30; //모든 실행 명령의 반환값
        //private int frmcomportindex;

        private byte fComAdr = 0xff; //현재 동작하는 ComAdr
        private int ferrorcode;
        private byte fBaud;
        private double fdminfre;
        private double fdmaxfre;
        private int fCmdRet = 30; //모든 실행 명령의 반환값
        private bool fIsInventoryScan;
        private bool fisinventoryscan_6B;
        private byte[] fOperEPC = new byte[100];
        private byte[] fPassWord = new byte[4];
        private byte[] fOperID_6B = new byte[10];
       // ArrayList list = new ArrayList();
        private int CardNum1 = 0;
        private string fInventory_EPC_List; //存贮询查列表（如果读取的数据没有变化，则不进行刷新）
        private int frmcomportindex;
        private bool SeriaATflag = false;
        private byte Target = 0;
        private byte InAnt = 0;
        private byte Scantime = 0;
        private byte FastFlag = 0;
        private byte Qvalue = 0;
        private byte Session = 0;
        private int total_turns = 0;//轮数
        private int total_tagnum = 0;//标签数量
        private int total_time = 0;//总时间 
        private int targettimes = 0;
        private int CommunicationTime = 0;
        private byte TIDFlag = 0;
        public static byte antinfo = 0;
        private int AA_times = 0;

        private delegate void WriteLogUnSafe(RichTextBox logRichTxt, string strLog, int nType);
        private void WriteLog(RichTextBox logRichTxt, string strLog, int nType)
        {
            if (this.InvokeRequired)
            {
                WriteLogUnSafe InvokeWriteLog = new WriteLogUnSafe(WriteLog);
                this.Invoke(InvokeWriteLog, new object[] { logRichTxt, strLog, nType });
            }
            else
            {
                if (nType == 0)
                {
                    logRichTxt.AppendText(strLog+'\n');
                }
                else
                {
                    logRichTxt.AppendText(strLog+ '\n');
                }

                logRichTxt.Select(logRichTxt.TextLength, 0);
                logRichTxt.ScrollToCaret();
            }
        }


        private void btGetInformation_Click(object sender, EventArgs e)
        {
            byte TrType = 0;
            byte[] VersionInfo = new byte[2];
            byte ReaderType = 0;
            byte ScanTime = 0;
            byte dmaxfre = 0;
            byte dminfre = 0;
            byte powerdBm = 0;
            byte FreBand = 0;
            byte Ant = 0;
            byte BeepEn = 0;
            byte OutputRep = 0;
            byte CheckAnt = 0;
            text_RDVersion.Text = "";
            int ctime = System.Environment.TickCount;
            fCmdRet = RWDev.GetReaderInformation(ref fComAdr, VersionInfo, ref ReaderType, ref TrType, ref dmaxfre, ref dminfre, ref powerdBm, ref ScanTime, ref Ant, ref BeepEn, ref OutputRep, ref CheckAnt, frmcomportindex);
            if (fCmdRet != 0)
            {
                string strLog = "Get Reader Information failed: " + GetReturnCodeDesc(fCmdRet);
                WriteLog(lrtxtLog, strLog, 1);
            }
            else
            {
                CommunicationTime = System.Environment.TickCount - ctime;
                text_RDVersion.Text = Convert.ToString(VersionInfo[0], 10).PadLeft(2, '0') + "." + Convert.ToString(VersionInfo[1], 10).PadLeft(2, '0');

                ComboBox_PowerDbm.SelectedIndex = Convert.ToInt32(powerdBm);
                text_address.Text = Convert.ToString(fComAdr, 16).PadLeft(2, '0');
                FreBand = Convert.ToByte(((dmaxfre & 0xc0) >> 4) | (dminfre >> 6));
                switch (FreBand)
                {
                    case 1:
                        {
                            radioButton_band1.Checked = true;
                            fdminfre = 920.125 + (dminfre & 0x3F) * 0.25;
                            fdmaxfre = 920.125 + (dmaxfre & 0x3F) * 0.25;
                        }
                        break;
                    case 2:
                        {
                            radioButton_band2.Checked = true;
                            fdminfre = 902.75 + (dminfre & 0x3F) * 0.5;
                            fdmaxfre = 902.75 + (dmaxfre & 0x3F) * 0.5;
                        }
                        break;
                    case 3:
                        {
                            radioButton_band3.Checked = true;
                            fdminfre = 917.1 + (dminfre & 0x3F) * 0.2;
                            fdmaxfre = 917.1 + (dmaxfre & 0x3F) * 0.2;
                        }
                        break;
                    case 4:
                        {
                            radioButton_band4.Checked = true;
                            fdminfre = 865.1 + (dminfre & 0x3F) * 0.2;
                            fdmaxfre = 865.1 + (dmaxfre & 0x3F) * 0.2;
                        }
                        break;
                }
                if (fdmaxfre != fdminfre)
                    CheckBox_SameFre.Checked = false;
              //  ComboBox_dminfre.SelectedIndex = dminfre & 0x3F;
             //   ComboBox_dmaxfre.SelectedIndex = dmaxfre & 0x3F; // hmm...

                string strLog = "Get Reader Information success ";
                WriteLog(lrtxtLog, strLog, 0);
            }
        }

        private void btConnect232_Click(object sender, EventArgs e)
        {
            if (txtPort.Text == "") return;

            int portNum = Convert.ToInt32(txtPort.Text);
            int FrmPortIndex = 0;
            string strException = string.Empty;

            fComAdr = 255;
            fCmdRet = RWDev.OpenComPort(portNum, ref fComAdr, 0, ref FrmPortIndex);

            string strLo2g = "click";
            WriteLog(lrtxtLog, strLo2g, 1);

            if (fCmdRet != 0)
            {
                string strLog = "Connect failed: " + GetReturnCodeDesc(fCmdRet);
                WriteLog(lrtxtLog, strLog, 1);
                return;
            }
            else
            {
                frmcomportindex = FrmPortIndex;
                string strLog = "Connected " + ComboBox_COM.Text;
                WriteLog(lrtxtLog, strLog, 0);
            }

            //계면 요소의 유효성 처리 여부
            EnabledForm();
            btConnect232.Enabled = false;
            btDisConnect232.Enabled = true;
            //설정 단추 글꼴 색상
            btConnect232.ForeColor = Color.Black;
            btDisConnect232.ForeColor = Color.Indigo;
            SetButtonBold(btConnect232);
            SetButtonBold(btDisConnect232);
            btGetInformation_Click(null, null);// 판독기 정보 가져오기
        }
        private void EnabledForm()
        {
            //panel1.Enabled = true;
            //panel3.Enabled = true;
            //panel4.Enabled = true;
            //panel5.Enabled = true;
            //panel6.Enabled = true;
            gpb_address.Enabled = true;
            groupBox3.Enabled = true;
            gpb_RDVersion.Enabled = true;
            gpb_DBM.Enabled = true;
            gpb_Serial.Enabled = true;
            gpb_Freq.Enabled = true;
            btDefault.Enabled = true;
            btGetInformation.Enabled = true;
            gbp_buff.Enabled = true;
        }
        private void DisabledForm()
        {
            ////应答模式下
            //lxLedControl1.Text = "0";
            //lxLedControl2.Text = "0";
            //lxLedControl3.Text = "0";
            //lxLedControl4.Text = "0";
            //lxLedControl5.Text = "0";
            dataGridView1.Rows.Clear();
            //dataGridView2.Rows.Clear();
            //comboBox_EPC.Items.Clear();
            text_RDVersion.Text = "";
            text_Serial.Text = "";
            timer_answer.Enabled = false;
            btIventoryG2.Text = "Start";
            //panel1.Enabled = false;
            //panel3.Enabled = false;
            //panel4.Enabled = false;
            //panel5.Enabled = false;
            //panel6.Enabled = false;
            gpb_address.Enabled = false;
            groupBox3.Enabled = false;
            gpb_RDVersion.Enabled = false;
            gpb_DBM.Enabled = false;
            gpb_Serial.Enabled = false;
            gpb_Freq.Enabled = false;
            btDefault.Enabled = false;
            btGetInformation.Enabled = false;
            gbp_buff.Enabled = false;
        }

        private void SetButtonBold(Button btnBold)
        {
            Font oldFont = btnBold.Font;
            Font newFont = new Font(oldFont, oldFont.Style ^ FontStyle.Bold);
            btnBold.Font = newFont;
        }

        private void btDisConnect232_Click(object sender, EventArgs e)
        {
            if (frmcomportindex > 0)
                fCmdRet = RWDev.CloseSpecComPort(frmcomportindex);
            if (fCmdRet == 0) frmcomportindex = -1;
            DisabledForm();
            btConnect232.Enabled = true;
            btDisConnect232.Enabled = false;

            btConnect232.ForeColor = Color.Indigo;
            btDisConnect232.ForeColor = Color.Black;
            SetButtonBold(btConnect232);
            SetButtonBold(btDisConnect232);
        }

        private void btDefault_Click(object sender, EventArgs e)
        {

        }

        private void BT_DBM_Click(object sender, EventArgs e)
        {
            byte powerDbm = (byte)ComboBox_PowerDbm.SelectedIndex;
            fCmdRet = RWDev.SetRfPower(ref fComAdr, powerDbm, frmcomportindex);
            if (fCmdRet != 0)
            {
                string strLog = "Set power failed: " + GetReturnCodeDesc(fCmdRet);
                WriteLog(lrtxtLog, strLog, 1);
            }
            else
            {
                string strLog = "Set power success ";
                WriteLog(lrtxtLog, strLog, 0);
            }
        }


        private void inventory()
        {
            byte Ant = 0;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen, m;
            byte[] EPC = new byte[50000]; //핵심 친구
            int CardIndex;
            string temps, temp;
            temp = "";
            string sEPC;
            byte MaskMem = 0;
            byte[] MaskAdr = new byte[2];
            byte MaskLen = 0;
            byte[] MaskData = new byte[100];
            byte MaskFlag = 0;
            byte AdrTID = 0;
            byte LenTID = 0;
            AdrTID = 0;
            LenTID = 6;
            MaskFlag = 0;
            int cbtime = System.Environment.TickCount;
            DataGridViewRow rows = new DataGridViewRow();
            CardNum = 0;
            fCmdRet = RWDev.Inventory_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref CardNum, frmcomportindex);
            total_turns = total_turns + 1;
            int x_time = System.Environment.TickCount - cbtime;//命令时间 명령시간
            string strLog = "Inventory: " + GetReturnCodeDesc(fCmdRet);
            WriteLog(lrtxtLog, strLog, 0);

            if ((fCmdRet == 1) | (fCmdRet == 2) | (fCmdRet == 3) | (fCmdRet == 4))//代表已查找结束， 검색이 끝났습니다.
            {
                byte[] daw = new byte[Totallen];
                Array.Copy(EPC, daw, Totallen);
                temps = ByteArrayToHexString(daw);
                m = 0;
                if (CardNum == 0)
                {
                    if (Session > 1)
                        AA_times = AA_times + 1;
                    return;
                }
                AA_times = 0;
                //antstr = Convert.ToString(Ant, 2).PadLeft(4, '0');
                for (CardIndex = 0; CardIndex < CardNum; CardIndex++)
                {
                    EPClen = daw[m] + 1;
                    temp = temps.Substring(m * 2 + 2, EPClen * 2);
                    sEPC = temp.Substring(0, temp.Length - 2);
                    string RSSI = Convert.ToInt32(temp.Substring(temp.Length - 2, 2), 16).ToString();
                    m = m + EPClen + 1;
                    if (sEPC.Length != (EPClen - 1) * 2)
                    {
                        return;
                    }
                    bool isonlistview = false;
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        if ((dataGridView1.Rows[i].Cells[1].Value != null) && (sEPC == dataGridView1.Rows[i].Cells[1].Value.ToString()))
                        {
                            rows = dataGridView1.Rows[i];
                            int ntime = Convert.ToInt32(rows.Cells[2].Value.ToString());
                            ntime = ntime + 1;
                            if (ntime == 99999) ntime = 1;
                            rows.Cells[2].Value = ntime;

                            rows.Cells[3].Value = RSSI;

                            if (rows.Cells[4].Value != null)
                            {


                                int delta = (Convert.ToInt32(rows.Cells[2].Value.ToString()) - Convert.ToInt32(rows.Cells[4].Value.ToString()));
                                rows.Cells[5].Value = Convert.ToString(delta);

                              
                                Find_MAX2();
                                Find_MAX_RSSI();
                        


                            }


                            isonlistview = true;
                            break;
                        }
                    }



                    if (!isonlistview)
                    {
                        string[] arr = new string[4];
                        arr[0] = (dataGridView1.RowCount + 1).ToString();
                        arr[1] = sEPC;
                        arr[2] = "1";
                        arr[3] = RSSI;
                        dataGridView1.Rows.Insert(dataGridView1.RowCount, arr);
                        if (rb_epc.Checked) { };
                            
                            // comboBox_EPC.Items.Add(sEPC);

                        //  NowTag_lbl.Text = sEPC;
                    }
                }
            }


           // lxLedControl3.Text = x_time.ToString();
            if (x_time > CommunicationTime)
                x_time = x_time - CommunicationTime;//减去通讯时间等于标签的实际时间 통신시간을 빼면 라벨의 실제시간과 같다.
            int sulv = (CardNum * 1000) / x_time;//速度等于张数/时间 속도는 장수/시간과 같다.
          //  lxLedControl2.Text = sulv.ToString();
            total_tagnum = total_tagnum + CardNum;
          //  lxLedControl4.Text = total_tagnum.ToString();
            x_time = System.Environment.TickCount - total_time;//总时间 총시간
           // lxLedControl5.Text = x_time.ToString();
           // lxLedControl1.Text = dataGridView1.RowCount.ToString();


        }
        private void btIventoryG2_Click(object sender, EventArgs e)
        {
            Timer_Intensity.Enabled = !Timer_Intensity.Enabled; // done by JH
            timer_answer.Enabled = !timer_answer.Enabled;
            if (timer_answer.Enabled)
            {
                //lxLedControl1.Text = "0";
                //lxLedControl2.Text = "0";
                //lxLedControl3.Text = "0";
                //lxLedControl4.Text = "0";
                //lxLedControl5.Text = "0";
                dataGridView1.Rows.Clear();
                //comboBox_EPC.Items.Clear();
                AA_times = 0;
                Scantime = Convert.ToByte(com_scantime.SelectedIndex + 3);
                Qvalue = Convert.ToByte(com_Q.SelectedIndex);
                Session = Convert.ToByte(com_S.SelectedIndex);
                if (rb_epc.Checked)
                    TIDFlag = 0;
                else
                    TIDFlag = 1;
                total_turns = 0;
                total_tagnum = 0;
                targettimes = Convert.ToInt32(text_target.Text);
                total_time = System.Environment.TickCount;
                fIsInventoryScan = false;
                Target = 0;
                btIventoryG2.BackColor = Color.Indigo;
                btIventoryG2.Text = "Stop";
            }
            else
            {
                btIventoryG2.BackColor = Color.Transparent;
                btIventoryG2.Text = "Start";
            }
        }

        private void SetRadioButtonBold(CheckBox ckBold)
        {
            Font oldFont = ckBold.Font;
            Font newFont = new Font(oldFont, oldFont.Style ^ FontStyle.Bold);
            ckBold.Font = newFont;
        }


        #region UTIL
        private string GetReturnCodeDesc(int cmdRet)
        {
            switch (cmdRet)
            {
                case 0x00:
                    return "successfully";
                case 0x01:
                    return "Return before Inventory finished";
                case 0x02:
                    return "the Inventory-scan-time overflow";
                case 0x03:
                    return "More Data";
                case 0x04:
                    return "Reader module MCU is Full";
                case 0x05:
                    return "Access Password Error";
                case 0x09:
                    return "Destroy Password Error";
                case 0x0a:
                    return "Destroy Password Error Cannot be Zero";
                case 0x0b:
                    return "Tag Not Support the command";
                case 0x0c:
                    return "Use the commmand,Access Password Cannot be Zero";
                case 0x0d:
                    return "Tag is protected,cannot set it again";
                case 0x0e:
                    return "Tag is unprotected,no need to reset it";
                case 0x10:
                    return "There is some locked bytes,write fail";
                case 0x11:
                    return "can not lock it";
                case 0x12:
                    return "is locked,cannot lock it again";
                case 0x13:
                    return "Parameter Save Fail,Can Use Before Power";
                case 0x14:
                    return "Cannot adjust";
                case 0x15:
                    return "Return before Inventory finished";
                case 0x16:
                    return "Inventory-Scan-Time overflow";
                case 0x17:
                    return "More Data";
                case 0x18:
                    return "Reader module MCU is full";
                case 0x19:
                    return "Not Support Command Or AccessPassword Cannot be Zero";
                case 0x1A:
                    return "Perform error tag custom function";
                case 0xF8:
                    return "Antenna connection detect errors";
                case 0xF9:
                    return "Command execution error";
                case 0xFA:
                    return "Get Tag,Poor Communication,Inoperable";
                case 0xFB:
                    return "No Tag Operable";
                case 0xFC:
                    return "Tag Return ErrorCode";
                case 0xFD:
                    return "Command length wrong";
                case 0xFE:
                    return "Illegal command";
                case 0xFF:
                    return "Parameter Error";
                case 0x30:
                    return "Communication error";
                case 0x31:
                    return "CRC checksummat error";
                case 0x32:
                    return "Return data length error";
                case 0x33:
                    return "Communication busy";
                case 0x34:
                    return "Busy,command is being executed";
                case 0x35:
                    return "ComPort Opened";
                case 0x36:
                    return "ComPort Closed";
                case 0x37:
                    return "Invalid Handle";
                case 0x38:
                    return "Invalid Port";
                case 0xEE:
                    return "Return Command Error";
                default:
                    return "";
            }
        }

        private void timer_answer_Tick(object sender, EventArgs e)
        {

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                {
                    try
                    {
                        if (dataGridView1.Rows[i].Cells[2].Value == null || dataGridView1.Rows[i].Cells[4].Value == null) continue;
                        int delta = (Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value.ToString()) - Convert.ToInt32(dataGridView1.Rows[i].Cells[4].Value.ToString()));
                        dataGridView1.Rows[i].Cells[5].Value = Convert.ToString(delta);
                    }
                    catch { }
                }


            }

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                {
                    try
                    {

                        dataGridView1.Rows[i].Cells[3].Value = 0;
                    }
                    catch { }
                }


            }

            if (fIsInventoryScan)
                return;
            fIsInventoryScan = true;
            FastFlag = 1;
            InAnt = 0x80;
            if (Session == 0 || Session == 1)
            {
                Target = (byte)com_Target.SelectedIndex;
            }
            else
            {
                if ((check_num.Checked) && (AA_times + 1 > targettimes))
                {
                    Target = Convert.ToByte(1 - Target);  //如果连续2次未读到卡片，A/B状态切换。
                }
                else if (!check_num.Checked)
                {
                    Target = (byte)com_Target.SelectedIndex;
                }
            }
            inventory();
            //================

            //================
            fIsInventoryScan = false;
        }

        void Find_MAX2() //done By JH 느린가..? RSSI가 낮아도 현재 읽히고 있는 친구일수있음.
        {
            int[] delta = new int[100];
            int[] RSSI = new int[100];
            string[] EPC = new string[100];

            string max_EPC = "NaN";
         

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                if (dataGridView1.Rows[i].Cells[5] == null || dataGridView1.Rows[i].Cells[5].Value==null) continue;
                try
                {
                    delta[i] = Convert.ToInt32(dataGridView1.Rows[i].Cells[5].Value.ToString());
                }
                catch { }

                RSSI[i] = Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value.ToString());
                EPC[i] = (dataGridView1.Rows[i].Cells[1].Value.ToString());

            }

            int temp;
            string temps;
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                for (int j = 0; j < dataGridView1.RowCount - 1 - i; j++)
                {
                    if (delta[j] < delta[j + 1])
                    {
                        temp = delta[j];
                        delta[j] = delta[j + 1];
                        delta[j + 1] = temp;

                        temp = RSSI[j];
                        RSSI[j] = RSSI[j + 1];
                        RSSI[j + 1] = temp;

                        temps = EPC[j];
                        EPC[j] = EPC[j + 1];
                        EPC[j + 1] = temps;


                    }
                }
            }
            int a = 0;

            int tmp = delta[0];
            int num = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (delta[i] != delta[i + 1]) num = i;

            }

            int max = 0;
            for (int i = 0; i < num; i++)
            {
                if (max < RSSI[i])
                {
                    max = RSSI[i];
                    max_EPC = EPC[i];
                }
            }

            if (num == 0) max_EPC = EPC[0];

            try
            {
                int ttmp = Convert.ToInt32(max_EPC);
                max_EPC = Convert.ToString(ttmp);
                label40.Text = max_EPC;
            }
            catch
            {

            }


        }

        void Find_MAX_RSSI() //done By JH 느린가..? RSSI가 낮아도 현재 읽히고 있는 친구일수있음.
        {
            int max = 0;
            string max_EPC = "NaN";
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (max < Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value.ToString()))
                {
                    max = Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value.ToString());
                    max_EPC = dataGridView1.Rows[i].Cells[1].Value.ToString();
                }
            }
            try
            {
                int ttmp = Convert.ToInt32(max_EPC);
                max_EPC = Convert.ToString(ttmp);
                label40.Text = max_EPC;
                txtCurTag.Text = max_EPC;
            }
            catch
            {

            }
        }

        private void Timer_Intensity_Tick(object sender, EventArgs e)
        {

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Cells[4].Value = Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value.ToString());
            }

        }

        private void groupBoxDebug_Enter(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tileMain_Click(object sender, EventArgs e)
        {
            mainTab.SelectedTab = this.tabPage1;
        }

        private void tileRFID_Click(object sender, EventArgs e)
        {
            mainTab.SelectedTab = this.tabPage2;
        }

        private void tileSETTING_Click(object sender, EventArgs e)
        {
            mainTab.SelectedTab = this.tabPage3;
        }
        private void tabWiFi_Click(object sender, EventArgs e)
        {
            mainTab.SelectedTab = this.tabWiFi;
        }

        private void metroTile4_Click(object sender, EventArgs e)
        {
            mainTab.SelectedTab = this.tabPage4;
        }

        private void gpb_rs232_Enter(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }







        private string GetErrorCodeDesc(int cmdRet)
        {
            switch (cmdRet)
            {
                case 0x00:
                    return "Other error";
                case 0x03:
                    return "Memory out or pc not support";
                case 0x04:
                    return "Memory Locked and unwritable";
                case 0x0b:
                    return "No Power,memory write operation cannot be executed";
                case 0x0f:
                    return "Not Special Error,tag not support special errorcode";
                default:
                    return "";
            }
        }
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }

        #endregion



        #endregion

        #region WIFI

        WiFiServer server = new WiFiServer();
        private void btn_browse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "All files|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    lbl_file_upload.Text = ofd.FileName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string fileName = lbl_file_upload.Text;
            //fileName = fileName.Split('\\').Last();
            string folderName = txt_folder.Text;
            //server.startUpload(fileName, folderName);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string uri = "http://192.168.0.150/download";
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(uri + "?download=" + textBox1.Text, @"d:\myfile.jpg");
            }
        }

        private void btn_motor_Click(object sender, EventArgs e)
        {
            string uri = "http://192.168.0.150/download";
            using (WebClient webClient = new WebClient()) 
            {
                string foldername = txt_folder.Text;
                string filePath = Application.StartupPath + @"\\" + foldername + "\\filename.txt";
                //startDownload("filename.txt", foldername);
                webClient.DownloadFile(uri + "?download=" + foldername + "/filename.txt", filePath);
                string[] filenames = File.ReadAllLines(filePath);
                if (filenames.Length > 0)
                {
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        lbl_datacount.Text = "[" + (i + 1).ToString() + "/" + filenames.Length.ToString() + "]";
                        //server.startDownload(filenames[i], foldername);
                        //webClient.DownloadFile(uri + "?download=" + foldername + "/" + filenames[i], Application.StartupPath + @"\\" + foldername + "\\" + filenames[i]);
                    }
                }
            }
        }



        #endregion WIFI


    }
}
