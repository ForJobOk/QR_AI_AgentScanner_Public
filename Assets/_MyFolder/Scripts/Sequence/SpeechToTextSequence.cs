using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Android;

public class SpeechToTextSequence : ISequence
{
    private AndroidJavaClass _nativeRecognizer;
    private AndroidJavaObject _activity;
    private bool _isRunningSpeechRecognizer;
    private CancellationTokenSource _cts;
    
    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();
        
        // ボタンは非表示。
        monoBehaviourContainer.MicButton.gameObject.SetActive(false);
        monoBehaviourContainer.ScanButton.gameObject.SetActive(false);
        
        // マイクアイコンを表示。
        monoBehaviourContainer.MicImage.gameObject.SetActive(true);
        
        UniTask.Action(async () =>
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Permission.RequestUserPermission(Permission.Microphone);
                }
            
                var receiver = monoBehaviourContainer.SpeechToTextCallbackReceiver;
                receiver.StartSpeechRecognizer();
                
                // 認識を待機。
                var result = await receiver.SpeechResultObservable.FirstAsync(_cts.Token).AsUniTask();
                
                // 失敗時の処理。
                if (string.IsNullOrEmpty(result))
                {
                    await FailedSpeechRecognitionAsync(monoBehaviourContainer);
                    
                    //　シーケンス遷移。
                    sequenceHandler.ChangeSequence(AppSequence.Idle);
                    return;
                }
                
                StaticData.UserMessage = result;
                
                // シーケンス遷移。
                sequenceHandler.ChangeSequence(AppSequence.Request);
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: _cts.Token);
                
                // シーケンス遷移。
                sequenceHandler.ChangeSequence(AppSequence.Request);
            }
        })();
    }
    
    private async UniTask FailedSpeechRecognitionAsync(MonoBehaviourContainer monoBehaviourContainer)
    {
        monoBehaviourContainer.MicButton.gameObject.SetActive(false);
        var progressCircleCanvasGroup = monoBehaviourContainer.ProgressCircleCanvasGroup;
        await progressCircleCanvasGroup.FadeAsync(0.5f, 0f, 1.0f,_cts.Token);
        monoBehaviourContainer.ProgressCircleText.text = "音声認識に失敗しました。";
        await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: _cts.Token);
        await progressCircleCanvasGroup.FadeAsync(0.5f, 1.0f, 0f, _cts.Token);
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        // マイクアイコンを非表示。
        monoBehaviourContainer.MicImage.gameObject.SetActive(false);
    }
}
