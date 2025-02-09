using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TextToSpeechSequence : ISequence
{
    private CancellationTokenSource _cts;
    private bool _isReady;
    private bool _isSpeaking;
    
    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();
        
        UniTask.Action(async () =>
        {
            var message = StaticData.ResponseMessage;
            if (Application.platform == RuntimePlatform.Android)
            {
                await SpeechAsync(message, _cts.Token);
            }
            else
            {
                Debug.Log($"音声読み上げをEditor上でスキップします。読み上げ音声： {message}");
            }
            
            // 次のシーケンスへ遷移。
            sequenceHandler.ChangeSequence(AppSequence.Idle);
        })();
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTask SpeechAsync(string message, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(message)) return;

        // AndroidのTextToSpeechクラスを取得。
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var textToSpeech = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TextToSpeechInitListener(this));

        // 初期化が完了するまで待機。
        await UniTask.WaitUntil(() => _isReady, cancellationToken: ct);
        
        // メッセージを話す。
        var paramsMap = new AndroidJavaObject("android.os.Bundle");
        textToSpeech.Call<int>("speak", message, 0, paramsMap, "utteranceId");
        
        // 読み上げが完了するまで待機。
        await UniTask.WaitUntil(() => !_isSpeaking, cancellationToken: ct);
    }

    private class TextToSpeechInitListener : AndroidJavaProxy
    {
        private readonly TextToSpeechSequence _textToSpeechSequence;

        public TextToSpeechInitListener(TextToSpeechSequence ttsInstance) : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            _textToSpeechSequence = ttsInstance;
        }

        public void onInit(int status)
        {
            if (status == 0) // SUCCESS
            {
                _textToSpeechSequence._isReady = true;
            }
        }
    }
}
