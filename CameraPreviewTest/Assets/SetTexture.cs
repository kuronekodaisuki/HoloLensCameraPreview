using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTexture : MonoBehaviour {
    public RawImage rawImage;
    public CameraPreviewTest test;

    private void Start()
    {
        test.OnTextureGenerated += (sender, args) =>
        {
            rawImage.texture = test.previewTexture;
        };
    }
}
