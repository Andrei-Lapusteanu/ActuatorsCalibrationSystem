#include "DcamWrapper.h"

DcamWrapper::DcamCoreWrapper::DcamCoreWrapper()
{
	this->_core = new DcamCore();
}

DcamWrapper::DcamCoreWrapper::~DcamCoreWrapper()
{
	delete this->_core;
}

DcamWrapper::DcamCoreWrapper::!DcamCoreWrapper()
{
	
}

void DcamWrapper::DcamCoreWrapper::Init()
{
	this->_core->InitDcamCapture();
}

void DcamWrapper::DcamCoreWrapper::Close()
{
	this->_core->CloseDcam();
}

BitmapSource^ DcamWrapper::DcamCoreWrapper::GetBitmapSourceFrame()
{
	return MatToBitmapSource(_core->GetDcamFrameMat());
}

Bitmap^ DcamWrapper::DcamCoreWrapper::GetBitmapFrame()
{
	return MatToBitmap(_core->GetDcamFrameMat());
}

Bitmap^ DcamWrapper::DcamCoreWrapper::GetBitmapFrameFromBuffer()
{
	return BufferToBitmap(_core->GetDummyFrameBuffer());
}

BitmapSource^ DcamWrapper::DcamCoreWrapper::MatToBitmapSource(cv::Mat image)
{
	if (image.size().width != 0)
	{
		if (image.channels() != 3)
			DownsampleMat(image);

		bool isContinuous = image.isContinuous();
		int cols = image.cols;
		int rows = image.rows;
		int stride = image.cols * image.channels();
		int stride00 = image.step.buf[0];

		IntPtr pointer = IntPtr(const_cast<uchar*>(image.data));

		Bitmap^ bitmap = gcnew Bitmap(
			cols,
			rows,
			stride,
			PixelFormat::Format24bppRgb,
			pointer);

		IntPtr ip = bitmap->GetHbitmap();

		BitmapSource^ bitmapSource = System::Windows::Interop::Imaging::CreateBitmapSourceFromHBitmap(
			ip,
			IntPtr::Zero,
			Int32Rect::Empty,
			BitmapSizeOptions::FromEmptyOptions());

		DeleteObject(ip);
		return bitmapSource;
	}
}

static cv::Mat imgClone;

Bitmap^ DcamWrapper::DcamCoreWrapper::MatToBitmap(cv::Mat image)
{
	imgClone = image.clone();

	if (imgClone.size().width != 0)
	{
		if (imgClone.channels() != 3)
			DownsampleMat(imgClone);

		int channels = imgClone.channels();
		int imgCols = imgClone.cols;
		int imgRows = imgClone.rows;
		int imgStride = imgCols * channels;

		IntPtr imgPtr = IntPtr(const_cast<uchar*>(imgClone.data));

		Bitmap^ retImg = gcnew Bitmap(
			imgCols,
			imgRows,
			imgStride,
			PixelFormat::Format24bppRgb,
			imgPtr);

		return retImg;
	}
	else return nullptr;
}

Bitmap^ DcamWrapper::DcamCoreWrapper::BufferToBitmap(uchar* buffer)
{
	if (_core->frameSize.height != 0)
	{
		Bitmap^ retImg = gcnew Bitmap(
			_core->frameSize.width,
			_core->frameSize.height,
			_core->frameSize.width * 3,
			PixelFormat::Format24bppRgb,
			IntPtr(buffer));
		
		return retImg;
	}
	else return nullptr;
}

void DcamWrapper::DcamCoreWrapper::DownsampleMat(cv::Mat &image)
{
	cv::Mat auxImage = image.clone();
	image.create(image.rows, image.cols, CV_8UC1);
	auxImage /= (float)256;
	auxImage.convertTo(image, CV_8UC1);
	cv::cvtColor(image, image, CV_GRAY2BGR);
}

