#pragma once
#include <librealsense2/rs.hpp> // Include RealSense Cross Platform API
#include <opencv2/opencv.hpp>   // Include OpenCV API

using namespace System;
using namespace cv;

namespace CameraLibrary {
	public ref class CameraOperate
	{
		// TODO: 在此处为此类添加方法。
	private:
		bool flag_camera_opened;
	public:
		const int NUM_FRAME_LEFT = 1;
		const int NUM_FRAME_RIGHT = 2;
		const int NUM_FRAME_DEPTH = 3;
		const int NUM_FRAME_COLOR = 4;
		// 相机相关默认参数
		int width = 640;
		int height = 360;
		int fps = 30;
		bool emmiter = true;
		// 记录相机是否已经成功开启的标志
		bool flag_image_display;
		bool flag_camera_close;

		// 当前帧的处理相关
		cv::Mat* _ptr_CurrentFrameLeft;
		cv::Mat* _ptr_CurrentFrameRight;
		cv::Mat* _ptr_CurrentFrameDepth;
		cv::Mat* _ptr_CurrentFrameColor;



		//rs2::pipeline* pipe_line;
		//rs2::pipeline_profile* pipe_profile;
		CameraOperate()
		{
			//pipe_line = new rs2::pipeline();
			//pipe_profile = new rs2::pipeline_profile();
			flag_camera_opened = false;
			flag_image_display = false;
			flag_camera_close = !flag_camera_opened;
			_ptr_CurrentFrameLeft = new cv::Mat(Size(width, height), CV_8UC1);
			_ptr_CurrentFrameRight = new cv::Mat(Size(width, height), CV_8UC1);
			_ptr_CurrentFrameDepth = new cv::Mat(Size(width, height), CV_8UC3);
			_ptr_CurrentFrameColor = new cv::Mat(Size(width, height), CV_8UC3);
		}
		~CameraOperate()
		{
			delete _ptr_CurrentFrameLeft;
			delete _ptr_CurrentFrameRight;
			delete _ptr_CurrentFrameDepth;
			delete _ptr_CurrentFrameColor;
		}
		void MessageShowTest();

		// Detect devices connected to this computer, return the number of connected devices. 
		int DeviceDetect();
		// Detect sensors connected to this computer, return the number of connected devices. 
		int SensorDetect();
		// 相机初始化，并开始捕获和展示图像
		bool CameraInitial(int ImageWidth, int ImageHeight, int FPS);
		IntPtr GetFrameData(int num);
		int GetFrameStep(int num);
	};
}
