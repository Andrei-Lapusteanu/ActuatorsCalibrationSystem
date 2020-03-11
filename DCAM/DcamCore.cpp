#include "DcamCore.h"

BOOL copy_targetarea(HDCAM hdcam, int32 iFrame, void* buf, int32 rowbytes, int32 ox, int32 oy, int32 cx, int32 cy)
{

	DCAMERR err;

	// prepare frame param
	DCAMBUF_FRAME bufframe;
	memset(&bufframe, 0, sizeof(bufframe));
	bufframe.size = sizeof(bufframe);
	bufframe.iFrame = iFrame;

#if USE_COPYFRAME
	// set user buffer information and copied ROI
	bufframe.buf = buf;
	bufframe.rowbytes = rowbytes;
	bufframe.left = ox;
	bufframe.top = oy;
	bufframe.width = cx;
	bufframe.height = cy;

	// access image
	err = dcambuf_copyframe(hdcam, &bufframe);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcambuf_copyframe()");
		return FALSE;
}
#else
	// access image
	err = dcambuf_lockframe(hdcam, &bufframe);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcambuf_lockframe()");
		return FALSE;
	}

	if (bufframe.type != DCAM_PIXELTYPE_MONO16)
	{
		printf("not implement pixel type\n");
		return FALSE;
	}


	// copy target ROI
	int32 copyrowbytes = cx * 2;
	char* pSrc = (char*)bufframe.buf + oy * bufframe.rowbytes + ox * 2;
	char* pDst = (char*)buf;

	int y;
	for (y = 0; y < cy; y++)
	{
		memcpy_s(_secure_ptr(pDst, rowbytes), pSrc, copyrowbytes);

		pSrc += bufframe.rowbytes;
		pDst += rowbytes;
	}
#endif

	return TRUE;
};

void get_image_information(HDCAM hdcam, int32& pixeltype, int32& width, int32& rowbytes, int32& height)
{
	DCAMERR err;

	double v;

	// image pixel type(DCAM_PIXELTYPE_MONO16, MONO8, ... )
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue(DCAM_IDPROP_IMAGE_PIXELTYPE)");
		return;
	}
	else
		pixeltype = (int32)v;

	// image width
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_WIDTH, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue(DCAM_IDPROP_IMAGE_WIDTH)");
		return;
	}
	else
		width = (int32)v;

	// image row bytes
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_ROWBYTES, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue(DCAM_IDPROP_IMAGE_ROWBYTES)");
		return;
	}
	else
		rowbytes = (int32)v;

	// image height
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue(DCAM_IDPROP_IMAGE_HEIGHT)");
		return;
	}
	else
		height = (int32)v;
};

DcamCore::DcamCore()
{
	_dummyFrame = cv::imread("dummy.jpg").clone();
}

DcamCore::~DcamCore()
{
	CloseDcam();
}

int DcamCore::InitDcamCapture()
{
	int	ret = 0;
	DCAMERR err;

	// !!!!!!!!!!!!!!!!!!!!!!!
	// DCAM_IDPROP_IMAGEDETECTOR_PIXELWIDTH
	//dcamprop_setvalue(hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, DCAM_PIXELTYPE::DCAM_PIXELTYPE_MONO8);

	// initialize DCAM-API and open device
	hdcam = dcamcon_init_open();
	if (hdcam != NULL)
	{
		// show device information
		dcamcon_show_dcamdev_info(hdcam);

		// open wait handle
		DCAMWAIT_OPEN	waitopen;
		memset(&waitopen, 0, sizeof(waitopen));
		waitopen.size = sizeof(waitopen);
		waitopen.hdcam = hdcam;

		err = dcamwait_open(&waitopen);
		if (failed(err))
		{
			dcamcon_show_dcamerr(hdcam, err, "dcamwait_open()");
			ret = 1;
		}
		else
		{
			HDCAMWAIT hwait = waitopen.hwait;

			// allocate buffer
			int32 number_of_buffer = 10;
			err = dcambuf_alloc(hdcam, number_of_buffer);
			if (failed(err))
			{
				dcamcon_show_dcamerr(hdcam, err, "dcambuf_alloc()");
				ret = 1;
			}
			else
			{
				// start capture
				err = dcamcap_start(hdcam, DCAMCAP_START_SEQUENCE);
				if (failed(err))
				{
					dcamcon_show_dcamerr(hdcam, err, "dcamcap_start()");
					ret = 1;
				}
				else
				{
					printf("\nStart Capture\n");

					// set wait param
					DCAMWAIT_START waitstart;
					memset(&waitstart, 0, sizeof(waitstart));
					waitstart.size = sizeof(waitstart);
					waitstart.eventmask = DCAMWAIT_CAPEVENT_FRAMEREADY;
					waitstart.timeout = 1000;

					// wait image
					err = dcamwait_start(hwait, &waitstart);
					if (failed(err))
					{
						dcamcon_show_dcamerr(hdcam, err, "dcamwait_start()");
						ret = 1;
					}
				}
			}

			// close wait handle
			dcamwait_close(hwait);
		}
	}
	else
	{
		ret = 1;
	}
	return ret;
}

void DcamCore::CloseDcam()
{
	// close DCAM handle
	dcamdev_close(hdcam);

	// finalize DCAM-API
	dcamapi_uninit();
}

cv::Mat DcamCore::GetDcamFrameMat()
{
	if (GetDcamFrame() == 0)
		return _imageMat;
	else
		return cv::Mat();
}

char* DcamCore::GetDcamFrameBuffer()
{
	if (GetDcamFrame() == 0)
		return _frameBuffer;
	else
		return NULL;
}

uchar* DcamCore::GetDummyFrameBuffer()
{
	frameSize.height = _dummyFrame.rows;
	frameSize.width = _dummyFrame.cols;
	return _dummyFrame.data;
}

cv::Mat DcamCore::ReadDummyImage()
{
	return _dummyFrame;
}

int DcamCore::GetDcamFrame()
{
	DCAMERR err;
	// transferinfo param

	DCAMCAP_TRANSFERINFO transferinfo;
	memset(&transferinfo, 0, sizeof(transferinfo));
	transferinfo.size = sizeof(transferinfo);


	// get number of captured image
	err = dcamcap_transferinfo(hdcam, &transferinfo);

	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamcap_transferinfo()");
		return 1;
	}

	// get image information
	int32 pixeltype = 0, width = 0, rowbytes = 0, height = 0;


	get_image_information(hdcam, pixeltype, width, rowbytes, height);


	if (pixeltype != DCAM_PIXELTYPE_MONO16)
	{
		printf("not implemented\n");
		return 1;
	}

	int32 cx = width;
	int32 cy = height;
	if (cx < 10)	cx = 10;
	if (cy < 10)	cy = 10;
	if (cx > width || cy > height)
	{
		printf("frame is too small\n");
		return 1;
	}

	int32 ox = (width - cx);
	int32 oy = (height - cy);

	char* buf = new char[cx * 2 * cy * 2];
	memset(buf, 0, cx * 2 * cy * 2);

	// copy image
	copy_targetarea(hdcam, 1, buf, cx * 2, ox, oy, cx, cy);

	// TO DO ELI
	// Works for 16 bit ine channel. See if other formats available
	_imageMat = cv::Mat(cv::Size(cx, cy), CV_16UC1, buf).clone();

	_frameBuffer = buf;

	delete buf;

	// release buffer
	dcambuf_release(hdcam);

	return 0;
}

DLLEXP cv::Mat* DCAMDLL_GetMatFrame()
{
	_matFrame = _dummyFrame.clone();
	_matFramePtr = &_matFrame;
	return _matFramePtr;
}

DLLEXP void DCAMDLL_FreeMatPtr()
{
	_matFramePtr = NULL;
	_matFrame = cv::Mat();
}

int main(int argc, char* const argv[])
{

}

