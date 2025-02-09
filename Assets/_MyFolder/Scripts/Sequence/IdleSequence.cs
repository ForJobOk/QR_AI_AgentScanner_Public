using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

public class IdleSequence : ISequence
{
    private CancellationTokenSource _cts;

    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();

        StaticData.UserMessage = string.Empty;
        monoBehaviourContainer.ScanButton.gameObject.SetActive(true);
        monoBehaviourContainer.MicButton.gameObject.SetActive(true);

        UniTask.Action(async () =>
        {
            var scanButtonTask = monoBehaviourContainer.ScanButton
                .OnClickAsObservable()
                .FirstAsync(cancellationToken:_cts.Token)
                .AsUniTask();

            var micButtonTask = monoBehaviourContainer.MicButton
                .OnClickAsObservable()
                .FirstAsync(cancellationToken:_cts.Token)
                .AsUniTask();

            var result = await UniTask.WhenAny(scanButtonTask, micButtonTask);
            sequenceHandler.ChangeSequence(result.winArgumentIndex == 0 ? AppSequence.Scan : AppSequence.SpeechToText);
        })();
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}