#include "pch.h"
#include "CameraLibrary.h"

using namespace System;
using namespace cv;

// MessageBox需要用到System.Windows.Forms.dll
#using <System.Windows.Forms.dll>
using namespace System::Windows::Forms;

void CameraLibrary::CameraOperate::MessageShowTest()
{
    //System::Windows
    MessageBox::Show("Succeed!");
}
// Detect devices connected to this computer, return the number of connected devices. 
int CameraLibrary::CameraOperate::DeviceDetect()
{
    rs2::context ctx;

    auto list = ctx.query_devices();

    return (list.size());
}
// Detect sensors connected to this computer, return the number of connected devices. 
int CameraLibrary::CameraOperate::SensorDetect()
{
    rs2::context ctx;

    auto list = ctx.query_all_sensors();

    return (list.size());
}

// 相机初始化，并开始捕获和展示图像
bool CameraLibrary::CameraOperate::CameraInitial(int ImageWidth, int ImageHeight, int FPS)
{
    // rs2::config 定义相机的设置参数
    rs2::config config;
    // 启用相机的左右红外和深度图像
    config.enable_stream(RS2_STREAM_INFRARED, 1, ImageWidth, ImageHeight, RS2_FORMAT_Y8, FPS);
    config.enable_stream(RS2_STREAM_INFRARED, 2, ImageWidth, ImageHeight, RS2_FORMAT_Y8, FPS);
    config.enable_stream(RS2_STREAM_DEPTH, ImageWidth, ImageHeight, RS2_FORMAT_Z16, FPS);
    config.enable_stream(RS2_STREAM_COLOR, ImageWidth, ImageHeight, RS2_FORMAT_RGB8, FPS);
    // start pipeline with config
    rs2::pipeline pipe;
    rs2::pipeline_profile pipe_line_profile = NULL;
    pipe_line_profile = pipe.start(config);

    rs2::device selected_device = pipe_line_profile.get_device();    // select the started device
    auto depth_sensor = selected_device.first<rs2::depth_sensor>();
    // 如果当前的深度传感器支持调节红外光发射器，就把发射器打开或关闭
    if (depth_sensor.supports(RS2_OPTION_EMITTER_ENABLED))
    {
        depth_sensor.set_option(RS2_OPTION_EMITTER_ENABLED, 0); // Disable emitter
    }

    if (pipe_line_profile == NULL)
    {
        // 相机启动失败，设置状态标志为false，并将长宽高等参数归零
        MessageBox::Show("相机启动失败！");
        flag_camera_opened = false;
        flag_camera_opened = !flag_camera_opened;
        width = 0;
        height = 0;
        fps = 0;
    }
    else
    {
        // 相机启动成功，设置状态标志为true，并设置长宽高等参数
        MessageBox::Show("相机启动成功！");
        flag_camera_opened = true;
        flag_camera_close = !flag_camera_opened;
        width = ImageWidth;
        height = ImageHeight;
        fps = FPS;

        // 下面是开始接收图像，也可以改为其他功能
        while ((waitKey(1) < 0) && flag_camera_opened)
        {
            if (flag_image_display)
            {
                rs2::colorizer color_map;
                rs2::frameset frames = pipe.wait_for_frames(); // Wait for next set of frames from the camera
                rs2::frame frame_left = frames.get_infrared_frame(1);
                rs2::frame frame_right = frames.get_infrared_frame(2);
                //rs2::frame frame_depth = frames.get_depth_frame().apply_filter(color_map);
                //rs2::frame frame_depth = frames.get_depth_frame();
                //rs2::frame frame_color = frames.get_color_frame();

                //Mat mat_left(Size(ImageWidth, ImageHeight), CV_8UC1, (void*)frame_left.get_data());
                //Mat mat_Right(Size(ImageWidth, ImageHeight), CV_8UC1, (void*)frame_right.get_data());
                //Mat mat_Depth(Size(ImageWidth, ImageHeight), CV_8UC3, (void*)frame_depth.get_data());
                //mat_left.copyTo(*_ptr_CurrentFrameLeft);
                //mat_Right.copyTo(*_ptr_CurrentFrameRight);
                //mat_Depth.copyTo(*_ptr_CurrentFrameDepth);
                (*_ptr_CurrentFrameLeft) = cv::Mat(Size(ImageWidth, ImageHeight), CV_8UC1, (void*)frame_left.get_data());
                (*_ptr_CurrentFrameRight) = cv::Mat(Size(ImageWidth, ImageHeight), CV_8UC1, (void*)frame_right.get_data());
                //(*_ptr_CurrentFrameDepth) = cv::Mat(Size(ImageWidth, ImageHeight), CV_16UC1, (void*)frame_depth.get_data());
                //(*_ptr_CurrentFrameColor) = cv::Mat(Size(ImageWidth, ImageHeight), CV_8UC3, (void*)frame_color.get_data());

                //_ptr_CurrentFrameLeft = &mat_left;
                const auto window_img_left = "WindowImageLeft";
                //const auto window_img_right = "WindowImageRight";
                //const auto window_img_depth = "WindowImageDepth";
                cv::imshow(window_img_left, (*_ptr_CurrentFrameLeft));
                //destroyWindow(window_img_left);
            }
            if (flag_camera_close)
            {
                pipe.stop();
                flag_camera_opened = false;
            }
        }
    }
    return flag_camera_opened;
}

IntPtr CameraLibrary::CameraOperate::GetFrameData(int num)
{
    //IntPtr _Mat_data = (IntPtr) _ptr_CurrentFrameLeft->data;
    if (num == 1)
        return (IntPtr)_ptr_CurrentFrameLeft->data;
    else if (num == 2)
        return (IntPtr)_ptr_CurrentFrameRight->data;
    else if (num == 3)
        return (IntPtr)_ptr_CurrentFrameDepth->data;
    else if (num == 4)
        return (IntPtr)_ptr_CurrentFrameColor->data;
    else
        return (IntPtr)_ptr_CurrentFrameLeft->data;
}
int CameraLibrary::CameraOperate::GetFrameStep(int num)
{
    if (num == 1)
        return _ptr_CurrentFrameLeft->step;
    else if (num == 2)
        return _ptr_CurrentFrameRight->step;
    else if (num == 3)
        return _ptr_CurrentFrameDepth->step;
    else if (num == 4)
        return _ptr_CurrentFrameColor->step;
    else
        return 0;
}