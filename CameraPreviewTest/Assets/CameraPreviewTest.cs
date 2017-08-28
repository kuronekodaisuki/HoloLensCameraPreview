using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CameraPreview;
// using HoloLensCameraStream;
using System;
using System.Linq;

public class CameraPreviewTest : MonoBehaviour {

    public TextMesh debugText;

    private CameraPreviewCapture  _cameraPreviewCapture;

    private Texture2D texture;

    public event EventHandler OnTextureGenerated;
    public Texture2D previewTexture {
        get { return texture; }
    }

    private byte[] _latestImageBytes;

    private void Awake()
    {
        CameraPreviewCapture.CreateAync(OnVideoCaptureInstanceCreated);
    }

    private void OnVideoCaptureInstanceCreated(CameraPreviewCapture captureObject)
    {

        if(captureObject == null)
        {
            return;
        }
        _cameraPreviewCapture = captureObject;

        var resolution = GetLowestResolution();
        var frameRate = GetHighestFrameRate(resolution);
        _cameraPreviewCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = resolution.height;
        cameraParams.cameraResolutionWidth = resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParams.rotateImage180Degrees = true;
        cameraParams.enableHolograms = false;

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            debugText.text = "Created";
            texture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false);
            OnTextureGenerated?.Invoke(this, new EventArgs());
        }, false);

        _cameraPreviewCapture.StartVideoModeAsync(false);
    }

    private void OnVideoModeStarted(VideoCaptureResult result)
    {
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            if (result.success == false)
            {
                debugText.text = "Start failed";
                throw new Exception("VideoStart error");
            }
            debugText.text = "Capture start";

        }, false);
    }

    int counter = 0;

    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
        {
            _latestImageBytes = new byte[sample.dataLength];
        }

        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
        sample.Dispose();
        

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            counter++;
            debugText.text = "arrived: " + counter;
            texture.LoadRawTextureData(_latestImageBytes);
            texture.Apply();
        }, false);
    }

    public CameraPreview.Resolution GetLowestResolution()
    {
        if(_cameraPreviewCapture == null)
        {
            throw new Exception("GetLowestResolution: no captureobject instance");
        }

        return _cameraPreviewCapture.GetSupportedResolutions().OrderBy(r => r.width * r.height).FirstOrDefault();
    }

    public float GetHighestFrameRate(CameraPreview.Resolution resolution)
    {
        if (_cameraPreviewCapture == null)
        {
            throw new Exception("GetHighestFrameRate: no captureobject instance");
        }

        return _cameraPreviewCapture.GetSupportedFrameRatesForResolution(resolution).OrderByDescending(r => r).FirstOrDefault();
    }
}
