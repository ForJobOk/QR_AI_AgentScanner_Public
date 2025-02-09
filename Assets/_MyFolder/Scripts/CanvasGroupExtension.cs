using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class CanvasGroupExtension
{
    public static async UniTask FadeAsync(
        this CanvasGroup canvasGroup,
        float duration, 
        float startAlpha, 
        float endAlpha, 
        CancellationToken ct)
    {
        var startTime = Time.time;
        var endTime = startTime + duration;
        while (Time.time < endTime)
        {
            var t = (Time.time - startTime) / duration;
            var alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            canvasGroup.alpha = alpha;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        canvasGroup.alpha = endAlpha;
    }
        
    public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0;
        canvasGroup.interactable = alpha > 0;
    }
}


