using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RequestSequence : ISequence
{
    private const string API_URL = "エンドポイント";
    private CancellationTokenSource _cts;
    
    public void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts = new CancellationTokenSource();
        
        UniTask.Action(async () =>
        {
            // プログレスバーの表示。
            monoBehaviourContainer.ProgressCircleText.text = "AIを呼び出し中...";
            await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 0f, 1.0f, _cts.Token);
            
            var contentCode = StaticData.ContentCode;
            var userMessage = StaticData.UserMessage;
            var response = await SendPostRequestAsync(contentCode, userMessage, _cts.Token);
            StaticData.ResponseMessage = response;
            
            // プログレスバーの非表示。
            await monoBehaviourContainer.ProgressCircleCanvasGroup.FadeAsync(0.5f, 1.0f, 0f, _cts.Token);
            
            // 次のシーケンスへ遷移。
            sequenceHandler.ChangeSequence(AppSequence.TextToSpeech);
        })();
    }

    public void OnExit(MonoBehaviourContainer monoBehaviourContainer)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
    
    /// <summary>
    /// Chat API にリクエストを送信
    /// </summary>
    private async UniTask<string> SendPostRequestAsync(
        string contentCode,
        string userMessage,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(contentCode))
        {
            return null;
        }
        
        // ユーザーメッセージが空の場合はデフォルトメッセージを設定。スキャン後はここを通る。
        if (string.IsNullOrEmpty(userMessage))
        {
            userMessage = "こんにちは";
        }

        var response = await SendPostRequestInternalAsync(contentCode, userMessage, ct);
        return response;
    }

    /// <summary>
    /// POSTリクエストを送信
    /// </summary>
    private async UniTask<string> SendPostRequestInternalAsync(
        string contentCode,
        string userMessage,
        CancellationToken ct)
    {
        var requestUrl = $"{API_URL}{contentCode}";
        Debug.Log(requestUrl);

        using var request = new UnityWebRequest(requestUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        var requestData = new ChatRequestData { message = userMessage };
        var jsonData = JsonUtility.ToJson(requestData);
        var bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;

        // リクエスト送信（非同期）
        await request.SendWebRequest().WithCancellation(ct);

        // エラーハンドリング
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"エラー: {request.error}");
            return "エラーが発生しました。";
        }

        // JSON パースしてレスポンスを取得
        var responseData = JsonUtility.FromJson<ChatResponseData>(request.downloadHandler.text);
        Debug.Log(responseData.response);
        return responseData.response;
    }

    [Serializable]
    private class ChatRequestData
    {
        public string message;
    }

    [Serializable]
    private class ChatResponseData
    {
        public string response;
    }
}
