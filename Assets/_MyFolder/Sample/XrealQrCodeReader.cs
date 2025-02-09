using System;
using NRKernal;
using R3;
using UnityEngine;
using ZXing;

/// <summary>
/// QRコードを読み取るクラス。
/// </summary>
public class XrealQrCodeReader : MonoBehaviour
{
    [SerializeField] private float _scanInterval = 1.0f;

    private readonly BarcodeReader _barcodeReader = new();
    private readonly ReactiveProperty<string> _scannedTextReactiveProperty = new();
    private NRRGBCamTexture _rbgCameraTextureSource;

    private bool _isReady;

    private void Start()
    {
        if(Application.isEditor)
        {
            Debug.LogWarning("This script is only for Android.");
            return;
        }

        _rbgCameraTextureSource = new NRRGBCamTexture();
        _rbgCameraTextureSource.Play();

        // 指定した頻度でQRコードを読み取る。
        Observable.Interval(TimeSpan.FromSeconds(_scanInterval))
            .Subscribe(_=> Scan())
            .AddTo(this);
    }

    public Observable<string> OnTextScanned => _scannedTextReactiveProperty.AsObservable();

    private void Scan()
    {
        var cameraTexture = _rbgCameraTextureSource.GetTexture();
        if (cameraTexture == null)
        {
            Debug.LogWarning("No camera texture received.");
            return;
        }

        try
        {
            OnScan(cameraTexture);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnScan(Texture2D cameraTexture)
    {
        var scan = _barcodeReader.Decode(
            cameraTexture.GetPixels32(),
            cameraTexture.width,
            cameraTexture.height);

        if (scan == null)
        {
            Debug.Log("No QR code found.");
            return;
        }

        Debug.Log($"QR : {scan.Text}");

        _scannedTextReactiveProperty.Value = scan.Text;
    }
}