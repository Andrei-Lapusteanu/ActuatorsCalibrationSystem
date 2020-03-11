#pragma once

#include "../DCAM/DcamCore.h"

using namespace System;
using namespace System::Windows::Media::Imaging;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Windows;

[System::Runtime::InteropServices::DllImportAttribute("gdi32.dll")]
extern bool DeleteObject(IntPtr handle);

namespace DcamWrapper {
	public ref class DcamCoreWrapper
	{
		// TODO: Add your methods for this class here.
	public:
		DcamCoreWrapper();
		~DcamCoreWrapper();
		!DcamCoreWrapper();
		void Init();
		void Close();
		BitmapSource^ GetBitmapSourceFrame();
		Bitmap^ GetBitmapFrame();
		Bitmap^ GetBitmapFrameFromBuffer();

	private:
		BitmapSource^ MatToBitmapSource(cv::Mat image);
		Bitmap^ MatToBitmap(cv::Mat image);
		Bitmap^ BufferToBitmap(uchar* buffer);
		void DownsampleMat(cv::Mat &image);

		DcamCore* _core;
	};
}
