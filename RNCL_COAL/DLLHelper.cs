using System.Runtime.InteropServices;

namespace RNCL_COAL
{
    class DLLHelper
    {
        // i3system_TE_dll

        /// Return Type: int
        ///pRecvImage: ushort[]
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "RecvImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecvImage(ushort[] pRecvImage, int hnd_dev);


        /// Return Type: int
        ///pRecvImage: double[]
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "RecvImageDouble", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecvImageDouble(double[] pRecvImage, int hnd_dev);


        /// Return Type: double
        ///_x: int
        ///_y: int
        ///isAmbientCalibOn: boolean
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "CalcTemp", CallingConvention = CallingConvention.Cdecl)]
        public static extern double CalcTemp(int _x, int _y, [MarshalAsAttribute(UnmanagedType.I1)] bool isAmbientCalibOn, int hnd_dev);


        /// Return Type: void
        ///pRecvTemp: double*
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "CalcEntireTemp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CalcEntireTemp(ref double pRecvTemp, int hnd_dev);


        /// Return Type: int
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "ReadFlashData", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReadFlashData(int hnd_dev);


        /// Return Type: BOOL->int
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "ShutterCalibrationOn", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool ShutterCalibrationOn(int hnd_dev);


        /// Return Type: boolean
        ///hnd_dev: int
        [DllImportAttribute("i3system_TE_dll.dll", EntryPoint = "UpdateDead", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool UpdateDead(int hnd_dev);


        // i3system_USB_DLL_V2_1


        /// Return Type: int
        ///iMsg: unsigned int
        ///wParam: WPARAM->UINT_PTR->unsigned int
        ///lParam: LPARAM->LONG_PTR->int
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbWindowProc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UsbWindowProc(uint iMsg, [MarshalAsAttribute(UnmanagedType.SysUInt)] uint wParam, [MarshalAsAttribute(UnmanagedType.SysInt)] int lParam);


        /// Return Type: int
        ///hwnd: HWND->HWND__*
        ///hnd_dev: int
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbOpenDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UsbOpenDevice(System.IntPtr hwnd, int hnd_dev);


        /// Return Type: int
        ///RecvBuf: unsigned char*
        ///ReadSize: unsigned int
        ///BytesRead: int*
        ///hnd_dev: int
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbRecvFunc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UsbRecvFunc(System.IntPtr RecvBuf, uint ReadSize, ref int BytesRead, int hnd_dev);


        /// Return Type: int
        ///SendBuf: unsigned char*
        ///WriteSize: unsigned int
        ///BytesWritten: int*
        ///hnd_dev: int
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbSendFunc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UsbSendFunc(System.IntPtr SendBuf, uint WriteSize, ref int BytesWritten, int hnd_dev);


        /// Return Type: void
        ///hnd_dev: int
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbCloseDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UsbCloseDevice(int hnd_dev);


        /// Return Type: int
        ///bCon_dev: BYTE*
        [DllImportAttribute("i3system_USB_DLL_V2_1.dll", EntryPoint = "UsbScan", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UsbScan(ref byte bCon_dev);
    }
}
