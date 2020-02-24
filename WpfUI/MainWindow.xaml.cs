using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraLibrary;
using Window = System.Windows.Window;
using System.Drawing;

namespace WpfUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CameraOperate CameraOperator;
        private IntPtr Ptr_ImageData;
        private BitmapImage bitmapImage_Left;
        private BitmapImage bitmapImage_Right;
        //private BitmapSource bitmapSource_Left;
        //private BitmapSource bitmapSource_Right;

        public enum StreamNumber
        {
            StreamLeft = 1,
            StreamRight = 2,
            StreamDepth = 3,
            StreamColor = 4
        }

        private int width = 640;
        private int height = 360;
        private int fps = 30;

        private Thread th;
        private ThreadStart thread_CameraOneInitialRef;
        private Thread thread_CameraOneInitial;
        private ThreadStart thread_ImageDisplayRef;
        private Thread thread_ImageDisplay;

        public MainWindow()
        {
            InitializeComponent();

            CameraOperator = new CameraOperate();
            Text_MessageTwo.Text = " ";
            // Set the name of main thread
            th = Thread.CurrentThread;
            th.Name = "Thread_Main";
        }

        private void Button_DisplayChange_Click(object sender, RoutedEventArgs e)
        {
            if (CameraOperator.flag_camera_close == false)
            {
                CameraOperator.flag_image_display = !CameraOperator.flag_image_display;
                if (CameraOperator.flag_image_display)
                {
                    Text_MessageTwo.Text = "！";
                    thread_ImageDisplayRef = new ThreadStart(CallToImageDisplayThread);
                    thread_ImageDisplay = new Thread(thread_ImageDisplayRef) { Name = "Thread_thread_ImageDisplay" };
                    try
                    {
                        thread_ImageDisplay.Start();
                        Text_MessageTwo.Text = "用于展示相机图像的子线程 Thread_ImageDisplay 创建成功";
                    }
                    catch
                    {
                    }
                }
                else
                {
                    try
                    {
                        thread_ImageDisplay.Abort();
                    }
                    catch { };
                }
            }
            else
            {
                Text_MessageTwo.Text = "相机未启动！";
            }
        }

        public void CallToImageDisplayThread()
        {
            // 只在flag_image_display为true时执行循环展示图片的程序，其他时间停止展示
            while (CameraOperator.flag_image_display)
            {
                // 获取左侧红外传感器的图像cv::Mat.data，Ptr_ImageData是IntPtr类型，可以与C++/CLI中的 usigned char* 互相传递
                Ptr_ImageData = CameraOperator.GetFrameData(CameraOperator.NUM_FRAME_LEFT);
                int step = CameraOperator.GetFrameStep(CameraOperator.NUM_FRAME_LEFT);
                // 将图像转换为Bitmap
                Bitmap bmp_Left = new Bitmap(width, height, step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                    Ptr_ImageData);
                // 再转换为BitmapImage
                bitmapImage_Left = ConvertBitmap2BitmapImage(bmp_Left);
                // 释放Bitmap内存
                bmp_Left.Dispose();

                // 获取右侧图像并转成Bitmap
                Ptr_ImageData = CameraOperator.GetFrameData(CameraOperator.NUM_FRAME_RIGHT);
                step = CameraOperator.GetFrameStep(CameraOperator.NUM_FRAME_RIGHT);
                Bitmap bmp_Right = new Bitmap(width, height, step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                    Ptr_ImageData);
                bitmapImage_Right = ConvertBitmap2BitmapImage(bmp_Right);
                bmp_Right.Dispose();

                //bitmapImage_Left = GetBitmapImageFromCamera(StreamNumber.StreamLeft);
                //bitmapImage_Right = GetBitmapImageFromCamera(StreamNumber.StreamRight);
                //Dispatcher.BeginInvoke(new RefleshUI(UIThreadRefeshImage));

                //Dispatcher.Invoke(() =>
                //{
                //    ImgLeft.Source = bitmapImage_Left;
                //    ImgRight.Source = bitmapImage_Right;
                //});
                //bitmapImage_Left.Freeze();
                //bitmapImage_Right.Freeze();

                DelegateUIRefresh();
                Thread.Sleep(20);
                //Task.Delay(500);
            }
        }

        private BitmapImage GetBitmapImageFromCamera(StreamNumber StreamNum)
        {
            // 根据选择的传感器类型，获得图像数据区指针以及图像的Stride
            Ptr_ImageData = CameraOperator.GetFrameData((int)StreamNum);
            int step = CameraOperator.GetFrameStep((int)StreamNum);
            // 根据选择的传感器类型，设置不同的像素格式
            // 不同类型的传感器，像素格式有单字节灰度、双字节灰度、3字节RGB等不同格式
            System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.DontCare;
            switch (StreamNum)
            {
                case StreamNumber.StreamLeft:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    break;
                case StreamNumber.StreamRight:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    break;
                case StreamNumber.StreamDepth:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                    break;
                case StreamNumber.StreamColor:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    break;
                default:
                    break;
            }
            // 根据上述信息，提取图像数据并转换为Bitmap，然后再转换为BitmapImage
            Bitmap bitmap = new Bitmap(width, height, step, pixelFormat, Ptr_ImageData);
            BitmapImage bitmapImage = ConvertBitmap2BitmapImage(bitmap);
            bitmap.Dispose();
            // 返回转换完成的BitmapImage
            return bitmapImage;
        }

        // 如果要使用DeleteObject()来释放IntPtr，就要使用以下代码
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        private BitmapSource ConvertBitmap2BitmapSource(Bitmap bmp)
        {
            // 将Bitmap转换为BitmapSource以在WPF的Image控件上显示，这种方法需要调用非托管代码，尽量不用
            IntPtr Ptr_Hbitmap = bmp.GetHbitmap();
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Ptr_Hbitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(Ptr_Hbitmap);    // 释放内存，否则会越用越多

            return bs;
        }

        private BitmapImage ConvertBitmap2BitmapImage(Bitmap bmp)
        {
            // 将Bitmap转换为BitmapImage
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp); //bmp_Left.RawFormat
                ms.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private void Button_ClearMessageBox_Click(object sender, RoutedEventArgs e)
        {
            Text_MessageTwo.Text = "";
            Text_MessageOne.Text = "";
        }

        private void Button_KillThreadImageDisplay_Click(object sender, RoutedEventArgs e)
        {
            // 终止图像展示线程
            try
            {
                thread_CameraOneInitial.Abort();
            }
            catch { };
            CameraOperator.flag_camera_close = true;
        }

        private void Button_CameraInitial_Click(object sender, RoutedEventArgs e)
        {
            Text_MessageOne.Text = $"主线程为 {th.Name.ToString()}";

            var number_of_devices = CameraOperator.DeviceDetect();
            var number_of_sensors = CameraOperator.SensorDetect();

            if (number_of_devices == 0)
            {
                Text_MessageTwo.Text = "未检测到相机设备！";
                return;
            }
            else if (number_of_sensors == 0)
            {
                Text_MessageTwo.Text = "检测到相机设备，但未检测到传感器组件！";
                return;
            }
            else
            {
                Text_MessageTwo.Text = "找到 " + number_of_devices.ToString() + " 个相机和 " + number_of_sensors.ToString() + " 个传感器！";
                thread_CameraOneInitialRef = new ThreadStart(CallToCameraOneInitialThread);
                thread_CameraOneInitial = new Thread(thread_CameraOneInitialRef) { Name = "Thread_CameraOneInitial" };
                try
                {
                    thread_CameraOneInitial.Start();
                    Text_MessageTwo.Text = "用于获取相机图像的子线程 Thread_CameraOneInitial 创建成功";
                }
                catch
                {
                }
            }
        }

        private void CallToCameraOneInitialThread()
        {
            CameraOperator.CameraInitial(width, height, fps);
        }
        // 调用委托类型，可以实现子线程申请主线程更新UI控件
        private void DelegateUIRefresh()
        {
            this.Dispatcher.BeginInvoke(new RefleshUI(UIThreadRefeshImage));
            //this.Dispatcher.Invoke(new RefleshUI(MiddleFun));
        }

        public delegate void RefleshUI();
        // 使用一个中间方法来调用了UI方法，这样当程序有多个UI方法时，我们可以在这个中间方法中做一些处理，然后决定引用那些UI方法）
        private void MiddleFun()
        {
            UIThreadRefeshImage();
            // 还可以在这里增加更多的UI更新方法
        }
        private void UIThreadRefeshImage()    //UI线程要做的事情
        {
            // 这里更新Image控件所需要的BitmapImage对象，可以采用非控件的类成员来在不同线程之间传递数据
            // 也可以直接作为本函数的输入参数传递进来，不过这样的话就需要比较多的参数了，或许可以自定义一个结构体成员用于刷新UI
            ImgLeft.Source = bitmapImage_Left;
            ImgRight.Source = bitmapImage_Right;

            //// 把左侧图像转为灰度显示
            //FormatConvertedBitmap grayBitmap_Left = new FormatConvertedBitmap();
            //grayBitmap_Left.BeginInit();
            //grayBitmap_Left.Source = bitmapImage_Left;
            //grayBitmap_Left.DestinationFormat = PixelFormats.Gray8;
            //grayBitmap_Left.EndInit();
            //// Set Source property of Image
            //ImgLeft.Source = grayBitmap_Left;

            //// 把右侧图像转为灰度显示
            //FormatConvertedBitmap grayBitmap_Right = new FormatConvertedBitmap();
            //grayBitmap_Right.BeginInit();
            //grayBitmap_Right.Source = bitmapImage_Right;
            //grayBitmap_Right.DestinationFormat = PixelFormats.Gray32Float;
            //grayBitmap_Right.EndInit();
            //// Set Source property of Image
            //ImgRight.Source = grayBitmap_Right;
        }
    }
}
