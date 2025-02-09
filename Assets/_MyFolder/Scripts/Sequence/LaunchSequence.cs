using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

public class LaunchSequence : ISequence
{
    private CancellationTokenSource _cts;
    
    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();
        
        UniTask.Action(async () =>
        {
            // 最初はスキャンボタンだけ表示。
            monoBehaviourContainer.MicButton.gameObject.SetActive(false);
            monoBehaviourContainer.MicImage.gameObject.SetActive(false);
            monoBehaviourContainer.ScanButton.gameObject.SetActive(true);
            monoBehaviourContainer.ProgressCircleCanvasGroup.SetAlpha(0);
            
            // ボタン押下待機。
            await monoBehaviourContainer.ScanButton
                .OnClickAsObservable()
                .FirstAsync(cancellationToken: _cts.Token)
                .AsUniTask();
            
            sequenceHandler.ChangeSequence(AppSequence.Scan);
        })();
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
