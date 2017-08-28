using CameraPreview;
using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPreviewTest : MonoBehaviour {
    // DLLで作ったクラス
    private CameraPreviewCapture  _cameraPreviewCapture;
    // カメラプレビューをレンダリングするためのテクスチャ
    private Texture2D texture;

    [SerializeField]
    private List<RawImage> rawImageList;

    public Texture2D previewTexture {
        get { return texture; }
    }
    
    private byte[] _latestImageBytes;

    private void Awake()
    {
        texture = new Texture2D(896, 504, TextureFormat.BGRA32, false);

        foreach(var rawImage in rawImageList)
        {
            rawImage.texture = texture;
        }
        // ファクトリメソッドをコール
        CameraPreviewCapture.CreateAync(CameraPreviewCapture_OnCreated);
    }

    /// <summary>
    /// ファクトリメソッドを呼び出したときのコールバック
    /// カメラプレビューを開始する
    /// </summary>
    /// <param name="captureObject"></param>
    private async void CameraPreviewCapture_OnCreated(CameraPreviewCapture captureObject)
    {
        if(captureObject == null)
        {
            throw new Exception("Failed to create CameraPreviewCapture instance");
        }
        _cameraPreviewCapture = captureObject;
        // 新しいフレームを取得したときのイベントハンドラを設定
        _cameraPreviewCapture.OnFrameArrived += CameraPreviewCapture_OnFrameArrived;
        // カメラプレビューの開始
        var result = await _cameraPreviewCapture.StartVideoModeAsync(false);

        if(!result)
        {
            throw new Exception("Failed to start camera preview");
        }
    }

    /// <summary>
    /// 新しいフレームを取得したときのイベントハンドラ
    /// フレームのバイナリデータを取得しテクスチャに変換する
    /// </summary>
    /// <param name="frameLength"></param>
    private void CameraPreviewCapture_OnFrameArrived(int frameLength)
    {
        
        if (_latestImageBytes == null || _latestImageBytes.Length < frameLength)
        {
            _latestImageBytes = new byte[frameLength];
        }

        _cameraPreviewCapture.CopyFrameToBuffer(_latestImageBytes);
        

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            texture.LoadRawTextureData(_latestImageBytes);
            texture.Apply();
        }, false);
    }
}
