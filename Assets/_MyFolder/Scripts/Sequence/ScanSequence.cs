using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NRKernal;
using R3;
using UnityEngine;
using ZXing;

public class ScanSequence : ISequence
{
    private NRRGBCamTexture _rbgCameraTextureSource;
    private CancellationTokenSource _cts;

    private const float ScanInterval = 1.0f;

    private readonly BarcodeReader _barcodeReader = new();
    private readonly Subject<string> _scanResultSubject = new();

    public ScanSequence()
    {
        // フリーズの原因になるのでつけっぱなしにしておく。
        if (Application.platform == RuntimePlatform.Android)
        {
            UniTask.Action(async () =>
            {
                // 早すぎるとダメそうなので適当に待つ。
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                // カメラの起動。
                _rbgCameraTextureSource = new NRRGBCamTexture();
                _rbgCameraTextureSource.Play();
            })();
        }
    }

    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();
        monoBehaviourContainer.ScanButton.gameObject.SetActive(false);
        monoBehaviourContainer.MicButton.gameObject.SetActive(false);

        UniTask.Action(async () =>
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                var result = await ScanAsync(monoBehaviourContainer, _cts.Token);
                StaticData.ContentCode = result;
            }
            else
            {
                // プログレスバーの表示。Editor上では何もせずに次に進める。
                monoBehaviourContainer.ProgressCircleText.text = "QRコードを読み取ってください";
                await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 0f, 1.0f, _cts.Token);
                await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: _cts.Token);
                await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 1.0f, 0f, _cts.Token);

                // デバッグ用データを削除。
                StaticData.ContentCode = "354375";
            }

            // 次のシーケンスへ遷移。
            sequenceHandler.ChangeSequence(AppSequence.Request);
        })();
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTask<string> ScanAsync(MonoBehaviourContainer monoBehaviourContainer, CancellationToken ct)
    {
        // プログレスバーの表示。
        monoBehaviourContainer.ProgressCircleText.text = "QRコードを読み取ってください";
        await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 0f, 1.0f, ct);

        // 指定した頻度でQRコードを読み取る。
        Observable.Interval(TimeSpan.FromSeconds(ScanInterval))
            .Subscribe(_ => Scan())
            .AddTo(_cts.Token);

        // 読み取りを待機。
        var scanResult = await _scanResultSubject.FirstAsync(cancellationToken: ct).AsUniTask();

        // プログレスバーの非表示。
        await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 1.0f, 0f, ct);

        return scanResult;
    }

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
            var scan = _barcodeReader.Decode(
                cameraTexture.GetPixels32(),
                cameraTexture.width,
                cameraTexture.height);

            if (scan == null)
            {
                Debug.Log("Not found QR code.");
                return;
            }

            Debug.Log($"QR : {scan.Text}");

            // 発火。
            _scanResultSubject.OnNext(scan.Text);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}