using System;
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
    public delegate void CaptureObjectCreatedCallback(CameraPreviewCapture createdObject);
    public delegate void FrameArrivedCallback(int frameLength);

    public sealed class CameraPreviewCapture
    {
        public event FrameArrivedCallback OnFrameArrived;

        private MediaFrameSourceInfo _frameSourceInfo;
        private MediaCapture _mediaCapture;
        private MediaFrameReader _frameReader;

        private SoftwareBitmap _bitmap;

        public bool IsStreaming {
            get {
                return _frameReader != null;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="frameSourceInfo"></param>
        private CameraPreviewCapture(MediaFrameSourceInfo frameSourceInfo)
        {
            _frameSourceInfo = frameSourceInfo;
        }

        /// <summary>
        /// CameraPreviewCaptureのファクトリメソッド
        /// </summary>
        /// <param name="onCreatedCallback"></param>
        public static async Task CreateAync(CaptureObjectCreatedCallback onCreatedCallback)
        {
            // カメラプレビューが可能なRGBカメラを含むMediaFrameSourceGroupの一覧を取得する
            var allFrameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();                                              //Returns IReadOnlyList<MediaFrameSourceGroup>
            var candidateFrameSourceGroups = allFrameSourceGroups.Where(group =>
                group.SourceInfos.Any(sourceInfo => 
                    sourceInfo.MediaStreamType == MediaStreamType.VideoPreview && 
                    sourceInfo.SourceKind == MediaFrameSourceKind.Color
                )
            );
            // 取得した一覧から先頭のMediaFrameSourceGroupを取得する
            var selectedFrameSourceGroup = candidateFrameSourceGroups.FirstOrDefault();
            if (selectedFrameSourceGroup == null)
            {
                onCreatedCallback?.Invoke(null);
                return;
            }
            // カメラプレビューが可能なRGBカメラのMediaFrameSourceInfoを取得
            var selectedFrameSourceInfo = selectedFrameSourceGroup.SourceInfos
                .Where(sourceInfo => 
                    sourceInfo.SourceKind == MediaFrameSourceKind.Color && 
                    sourceInfo.MediaStreamType == MediaStreamType.VideoPreview)
                .FirstOrDefault();
            if (selectedFrameSourceInfo == null)
            {
                onCreatedCallback?.Invoke(null);
                return;
            }
            // MediaFrameSourceのDeviceInformationを取得する
            var deviceInformation = selectedFrameSourceInfo.DeviceInformation;

            // devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);   //Returns DeviceCollection
            // deviceInformation = devices.FirstOrDefault();                               //Returns a single DeviceInformation

            if (deviceInformation == null)
            {
                onCreatedCallback?.Invoke(null);
                return;
            }
            // インスタンス化
            var videoCapture = new CameraPreviewCapture(selectedFrameSourceInfo);
            // MediaCaptureのインスタンスを作成
            var result = await videoCapture.CreateMediaCaptureAsync(selectedFrameSourceGroup, deviceInformation);

            if (result)
            {
                // インスタンスをコールバックに渡す
                onCreatedCallback?.Invoke(videoCapture);
            } else
            {
                onCreatedCallback?.Invoke(null);
            }
        }

        /// <summary>
        /// MediaCaptureインスタンスの作成
        /// </summary>
        /// <param name="frameSourceGroup"></param>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        private async Task<bool> CreateMediaCaptureAsync(MediaFrameSourceGroup frameSourceGroup, DeviceInformation deviceInfo)
        {
            if (_mediaCapture != null)
            {
                return false;
            }

            _mediaCapture = new MediaCapture();
            // 設定オブジェクトの作成、MediaFrameSourceGroupを指定している
            // 取得した画像(フレーム)をバイナリで取得するため、
            // フレームをSoftwareBitmapとして取得できるようMemoryPreferenceを設定する
            var settings = new MediaCaptureInitializationSettings()
            {
                VideoDeviceId = deviceInfo.Id,
                SourceGroup = frameSourceGroup,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };
            // MediaCaptureの初期化
            try
            {
                await _mediaCapture.InitializeAsync(settings);
                _mediaCapture.VideoDeviceController.Focus.TrySetAuto(true);
                return true;
            } catch(Exception)
            {
                // 何かしらの例外が発生
                _mediaCapture.Dispose();
                _mediaCapture = null;
                return false;
            }
        }

        /// <summary>
        /// カメラプレビューを開始する
        /// </summary>
        /// <param name="IsCapturedHologram"></param>
        public async Task<bool> StartVideoModeAsync(bool IsCapturedHologram)
        {
            // MediaFrameSourceを取得する
            // MediaFrameSourceはMediaFrameSourceGroupから直接取得することはできず
            // MediaCapture経由で取得する必要がある
            var mediaFrameSource = _mediaCapture.FrameSources[_frameSourceInfo.Id];

            if (mediaFrameSource == null)
            {
                return false;
            }
            // Unityのテクスチャに変換できるフォーマットを指定
            var pixelFormat= MediaEncodingSubtypes.Bgra8;
            // MediaFrameReaderの作成
            _frameReader = await _mediaCapture.CreateFrameReaderAsync(mediaFrameSource, pixelFormat);
            // フレームを取得したときのイベントハンドラ
            _frameReader.FrameArrived += HandleFrameArrived;
            // フレームの取得を開始する
            var result = await _frameReader.StartAsync();
            // デバイスがサポートするビデオフォーマットの一覧を取得する
            // ここではHoloLensがサポートする896x504 30fpsに絞って取得している
            var allPropertySets = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview)
                .Select(x => x as VideoEncodingProperties)
                .Where(x =>
                {
                    if (x == null) return false;
                    if (x.FrameRate.Denominator == 0) return false;

                    double frameRate = (double)x.FrameRate.Numerator / (double)x.FrameRate.Denominator;

                    return x.Width == 896 && x.Height == 504 && (int)Math.Round(frameRate) == 30;
                });
            // 取得したフォーマット情報を使ってキャプチャするフレームの解像度とFPSを設定する
            VideoEncodingProperties properties = allPropertySets.FirstOrDefault();
            await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, properties);
            // Mixed Reality Captureの設定
            IVideoEffectDefinition ved = new MixedRealityCaptureSetting(IsCapturedHologram, false, 0, IsCapturedHologram ? 0.9f : 0.0f);
            await _mediaCapture.AddVideoEffectAsync(ved, MediaStreamType.VideoPreview);

            return true;
        }

        /// <summary>
        /// SoftwareBitmapとして保持しているフレームを引数のバイナリバッファに渡す
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyFrameToBuffer(byte[] buffer)
        {
            if(buffer == null)
            {
                throw new ArgumentException("buffer is null");
            }

            if(buffer.Length < 4 * _bitmap.PixelWidth * _bitmap.PixelHeight)
            {
                throw new IndexOutOfRangeException("buffer is not big enough");
            }

            if(_bitmap != null)
            {
                _bitmap.CopyToBuffer(buffer.AsBuffer());
                _bitmap.Dispose();
            }
        }

        /// <summary>
        /// カメラプレビューを停止する
        /// </summary>
        /// <param name="onVideoModeStoppedCallback"></param>
        public async Task<bool> StopVideoModeAsync()
        {
            if (IsStreaming == false)
            {
                // すでにプレビューを停止している場合は何もしない
                return false;
            }

            _frameReader.FrameArrived -= HandleFrameArrived;
            await _frameReader.StopAsync();
            _frameReader.Dispose();
            _frameReader = null;
            return true;
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public async Task Dispose()
        {
            if (IsStreaming)
            {
                await StopVideoModeAsync();
            }
            _mediaCapture?.Dispose();
        }

        /// <summary>
        /// 新しいフレームを取得したときのハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            // プラグイン外からイベントハンドラが設定されていない場合は何もしない
            if (OnFrameArrived == null)
            {
                return;
            }
            // 最新のフレームを取得
            using (var frame = _frameReader.TryAcquireLatestFrame())
            {
                if (frame != null)
                {
                    // SoftwareBitmapとして保持し、サブスクライバにはBitmapのサイズを通知
                    _bitmap = frame.VideoMediaFrame.SoftwareBitmap; 
                    OnFrameArrived?.Invoke(4 * _bitmap.PixelHeight * _bitmap.PixelWidth);
                }
            }
        }

    }

    public class MixedRealityCaptureSetting : IVideoEffectDefinition
    {
        public string ActivatableClassId {
            get {
                return "Windows.Media.MixedRealityCapture.MixedRealityCaptureVideoEffect";
            }
        }

        public IPropertySet Properties {
            get; private set;
        }

        public MixedRealityCaptureSetting(bool HologramCompositionEnabled, bool VideoStabilizationEnabled, int VideoStabilizationBufferLength, float GlobalOpacityCoefficient)
        {
            Properties = (IPropertySet)new PropertySet();
            Properties.Add("HologramCompositionEnabled", HologramCompositionEnabled);           // Hologramをキャプチャするかどうか
            Properties.Add("VideoStabilizationEnabled", VideoStabilizationEnabled);             // 手ブレ補正を行うかどうか
            Properties.Add("VideoStabilizationBufferLength", VideoStabilizationBufferLength);   // 手ブレ補正に使用するバッファ
            Properties.Add("GlobalOpacityCoefficient", GlobalOpacityCoefficient);               // キャプチャしたHologramの透過度
        }
    }
}