using System.Collections;
using UnityEngine;

public class AndroidTextToSpeech : MonoBehaviour
{
    private AndroidJavaObject _tts; // TTSインスタンス
    private bool _isTTSReady;

    private string _message = "毎日動画をアップすると費やせる時間と手間は限られ、クオリティを高めるのは難しい。週一にしてクオリティを上げようとすると、たしかに面白い動画も撮れるがその分ハードルも上がる。ペースを落とした人は消え、毎日続ける人がずっと第一線に残る。成果が出なくて辞めてしまう人と、それでもずっとやり続ける人に分かれる。";

    private IEnumerator Start()
    {
        if (RuntimePlatform.Android != Application.platform) yield break;

        // AndroidのTextToSpeechクラスを取得
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        _tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSInitListener(this));

        // TTSの初期化が完了するまで待機
        yield return new WaitUntil(() => _isTTSReady);
        _tts.Call<int>("speak", _message, 0, null, null);
    }

    private class TTSInitListener : AndroidJavaProxy
    {
        private AndroidTextToSpeech androidTTS;

        public TTSInitListener(AndroidTextToSpeech ttsInstance) : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            androidTTS = ttsInstance;
        }

        public void onInit(int status)
        {
            if (status == 0)
            {
                androidTTS._isTTSReady = true;
            }
        }
    }
}
