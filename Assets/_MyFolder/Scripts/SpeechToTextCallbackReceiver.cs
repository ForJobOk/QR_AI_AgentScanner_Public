using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 非MonoBehaviourにしたいが、Android側からコールバックを受け取るためにMonoBehaviourにしている。
/// </summary>
public class SpeechToTextCallbackReceiver : MonoBehaviour
{
    [SerializeField] Image _micButtonImage;

    public Observable<string> SpeechResultObservable => _speechResultSubject.AsObservable();

    private readonly Subject<string> _speechResultSubject = new();

    private AndroidJavaClass _nativeRecognizer;
    private AndroidJavaObject _activity;
    private bool _isRunningSpeechRecognizer;
    private string _result;

    public void StartSpeechRecognizer()
    {
        _result = "";
        _isRunningSpeechRecognizer = true;

        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            _nativeRecognizer = new AndroidJavaClass("com.ud.speech.NativeSpeechRecognizer");
            _nativeRecognizer.CallStatic(
                "StartRecognizer",
                activity,
                gameObject.name,
                nameof(CallbackMethod));
        }));
    }

    private void CallbackMethod(string message)
    {
        var messages = message.Split('\n');

        // 認識した音量変化のコールバック。
        if (messages[0] == "onRmsChanged" && _isRunningSpeechRecognizer)
        {
            Debug.Log("認識中...");
            _micButtonImage.color = Color.red;
        }

        // ユーザーが話すのを終了した際のコールバック。
        if (messages[0] == "onEndOfSpeech")
        {
            Debug.Log("認識終了");
            _isRunningSpeechRecognizer = false;
            _micButtonImage.color = Color.white;

            // 音声認識の失敗/成功、どちらの場合もこのコールバックが呼ばれる。
            // 加えて、成功時にはonResultsの呼び出し前に呼ばれるため、失敗の判定が難しい。
            // よって、遅れて実行し、onResultsが確実に終わっている状態で結果を通知する。
            // これにより、失敗時は空文字が発火するようになる。
            ResultOnNextAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // エラーが発生した際のコールバック。タイムアウトの場合も含む。
        if (messages[0] == "onError")
        {
            Debug.Log("エラー");
            _isRunningSpeechRecognizer = false;
            _micButtonImage.color = Color.white;
            _speechResultSubject.OnNext("");
        }

        // 認識結果の準備が完了したコールバック。
        if (messages[0] == "onResults")
        {
            var msg = "";
            for (var i = 1; i < messages.Length; i++)
            {
                msg += messages[i] + "\n";
            }

            Debug.Log(msg);

            _micButtonImage.color = Color.white;
            _result = msg;
        }
    }

    private async UniTask ResultOnNextAsync(CancellationToken ct)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: ct);
        _speechResultSubject.OnNext(_result);
    }
}