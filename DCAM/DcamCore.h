#pragma once


#include "../3rd Party/dcamsdk4/misc\console4.h"
#include "../3rd Party/dcamsdk4/misc\common.h"

#include "opencv2/core.hpp"
#include "opencv2/imgcodecs.hpp"
#include "opencv2/imgproc/types_c.h"
#include "opencv2/imgproc.hpp"
#include "opencv2/core/neon_utils.hpp"
#include "opencv2/core/sse_utils.hpp"


#define DLLEXP __declspec(dllexport)

static cv::Mat* _matFramePtr;
static cv::Mat _matFrame;
static cv::Mat _dummyFrame;

extern "C"
{
	DLLEXP cv::Mat* DCAMDLL_GetMatFrame();
	DLLEXP void DCAMDLL_FreeMatPtr();
}

class DLLEXP DcamCore
{
public:
	DcamCore();
	~DcamCore();

	int InitDcamCapture();
	void CloseDcam();

	cv::Mat GetDcamFrameMat();
	char* GetDcamFrameBuffer();

	uchar* GetDummyFrameBuffer();
	cv::Mat ReadDummyImage();

	struct FrameSize {
		int width;
		int height;
	};
	FrameSize frameSize;


private:
	int GetDcamFrame();

	HDCAM hdcam;
	cv::Mat _imageMat;
	char* _frameBuffer;
};

