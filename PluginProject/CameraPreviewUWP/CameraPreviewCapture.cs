using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace CameraPreview
{
    public delegate void OnCameraPreviewCreatedCallback(CameraPreviewCapture captureObject);
    public delegate void OnFrameArrivedCallback();

    public class CameraPreviewCapture
    {
        private MediaCapture _mediaCapture;
        private MediaFrameSourceInfo _frameSourceInfo;
        private MediaFrameReader _mediaFrameReader;

        public event OnFrameArrivedCallback FrameArrived;
        public bool IsStreaming {
            get { return _mediaFrameReader != null; }
        }

        static readonly Guid ROTATION_KEY = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        /// <summary>
        /// CameraPreviewCaptureのファクトリメソッド
        /// </summary>
        /// <param name="onCreatedCallback"></param>
        /// <returns></returns>
        public static async Task CreateInstanceAsync(OnCameraPreviewCreatedCallback onCreatedCallback)
        {
            // デバイスがサポートしている同時利用可能なメディアフレームのソースのグループをすべて取得する。
            var allFrameSourcwGroups = await MediaFrameSourceGroup.FindAllAsync();
            // 取得したグループから、カメラプレビューを取得可能なグループを抽出する
            var candidateFrameSourceGroups = allFrameSourcwGroups.Where(group =>
                group.SourceInfos.Any(sourceInfo =>
                    // VideoPreviewストリームを取り扱うことができ
                    sourceInfo.MediaStreamType == MediaStreamType.VideoPreview &&
                        // RGBデータを取得できるMediaFrameSource
                        sourceInfo.SourceKind == MediaFrameSourceKind.Color
                )
            );
            // 抽出したグループのリストから先頭のものを取得
            var selectedFrameSourceGroup = candidateFrameSourceGroups.FirstOrDefault();
            // 取得できなかった
            if(selectedFrameSourceGroup == null)
            {
                onCreatedCallback?.Invoke(null);
                return;
            }

            var selectedFrameSourceInfo = selectedFrameSourceGroup.SourceInfos.FirstOrDefault();
            if(selectedFrameSourceInfo == null)
            {
                onCreatedCallback?.Invoke(null);
            }

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var deviceInformation = devices.FirstOrDefault();
            // 取得できなかった
            if(deviceInformation == null)
            {
                onCreatedCallback?.Invoke(null);
                return; 
            }

            var captureObject = new CameraPreviewCapture(selectedFrameSourceInfo);
            // MediaCaptureのインスタンスを作成する

            var result = await captureObject.CreateMediaCaptureAsync(deviceInformation, selectedFrameSourceGroup);
            if (result)
            {
                // コールバックを経由して、インスタンスを返す
                onCreatedCallback?.Invoke(captureObject);
            } else
            {
                onCreatedCallback?.Invoke(null);
            }
        }

        public async Task<bool> StartCameraPreviewCapture(bool IsCapturedHologram)
        {
            var mediaFrameSource = _mediaCapture.FrameSources[_frameSourceInfo.Id];
            if(mediaFrameSource == null)
            {
                //
                return false;
            }

            // Unityでテクスチャに変換するときはこれ
            string pixelFormat = MediaEncodingSubtypes.Bgra8;

            _mediaFrameReader = await _mediaCapture.CreateFrameReaderAsync(mediaFrameSource, pixelFormat);

            _mediaFrameReader.FrameArrived += HandleFrameArrived;

            await _mediaFrameReader.StartAsync();

            var allPropertySets = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview)
                .Select(x => x as VideoEncodingProperties)
                .Where(x =>
                {
                    if (x == null) return false;
                    if (x.FrameRate.Denominator == 0) return false;

                    double calculateFrameRate = (double)x.FrameRate.Numerator / (double)x.FrameRate.Denominator;

                    return
                    x.Width == 1280 &&
                    x.Height == 720 &&
                    (int)Math.Round(calculateFrameRate) == 30;
                });

            if(allPropertySets.Count() == 0)
            {
                return false;
            }

            var properties = allPropertySets.FirstOrDefault();
            properties.Properties.Add(ROTATION_KEY, 180);

            IVideoEffectDefinition ved = new MixedRealityCaptureSetting(IsCapturedHologram);

            await _mediaCapture.AddVideoEffectAsync(ved, MediaStreamType.VideoPreview);
            await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, properties);

            return true;
        }

        public async Task<bool> StopVideoModeAsync()
        {
            if(IsStreaming == false)
            {
                return false;
            }

            _mediaFrameReader.FrameArrived -= HandleFrameArrived;
            await _mediaFrameReader.StopAsync();
            _mediaFrameReader.Dispose();
            _mediaFrameReader = null;

            return true;
        }

        public async Task Dispose()
        {
            if(IsStreaming)
            {
                await StopVideoModeAsync();
            }

            _mediaCapture?.Dispose();
        }

        public void CopyFrameToBuffer(byte[] buffer)
        {
            if(buffer == null)
            {
                throw new ArgumentException("buffer is null");
            }

            if(buffer.Length < 4 * bitmap.PixelWidth * bitmap.PixelWidth)
            {
                throw new IndexOutOfRangeException("buffer is not big enough");
            }

            if (bitmap != null)
            {
                bitmap.CopyToBuffer(buffer.AsBuffer());
            }
        }

        private CameraPreviewCapture(MediaFrameSourceInfo mediaFrameSourceInfo)
        {
            _frameSourceInfo = mediaFrameSourceInfo;
        }

        /// <summary>
        /// MediaCaptureのインスタンスを生成する
        /// </summary>
        /// <param name="deviceInformation"></param>
        /// <param name="sourceGroup"></param>
        /// <returns></returns>
        private async Task<bool> CreateMediaCaptureAsync(DeviceInformation deviceInformation, MediaFrameSourceGroup sourceGroup)
        {
            // すでにMediaCaptureを生成済み生成済み
            if(_mediaCapture != null)
            {
                throw new Exception("The MediaCapture object has already been created.");
            }

            _mediaCapture = new MediaCapture();
            // MediaCaptureの設定
            var settings = new MediaCaptureInitializationSettings
            {
                // カメラデバイスの指定
                VideoDeviceId = deviceInformation.Id,
                // 使用するソースグループ
                SourceGroup = sourceGroup,
                // 取得したフレームはCPUメモリに確保
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                // ビデオのみをキャプチャする
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            try
            {
                // 初期化
                await _mediaCapture.InitializeAsync(settings);
            } catch (Exception ex)
            {
                // 初期化失敗
                Debug.WriteLine("MediaCapture initialization failed: " + ex.Message);
                _mediaCapture.Dispose();
                _mediaCapture = null;
                return false;
            }
            // フォーカスはオートに設定
            _mediaCapture.VideoDeviceController.Focus.TrySetAuto(true);
            return true;
        }

        private void HandleFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using(var frame = _mediaFrameReader.TryAcquireLatestFrame())
            {
                if(frame != null)
                {
                    bitmap = frame.VideoMediaFrame.SoftwareBitmap;
                    FrameArrived?.Invoke();
                }
            }
        }

        private SoftwareBitmap bitmap;

        private class MixedRealityCaptureSetting : IVideoEffectDefinition
        {
            public string ActivatableClassId {
                get {
                    return "Windows.Media.MixedRealityCapture.MixedRealityCaptureVideoEffect";
                }
            }

            public IPropertySet Properties {
                get; private set;
            }

            public MixedRealityCaptureSetting(bool IsCapturedHologram)
            {
                Properties = (IPropertySet)new PropertySet();
                Properties.Add("HologramCompositionEnabled", IsCapturedHologram);
                Properties.Add("VideoStabilizationEnabled", true);
                Properties.Add("VideoStabilizationBufferLength", 1);
                Properties.Add("GlobalOpacityCoefficient", 1.0f);
            }
        }
    }
}
