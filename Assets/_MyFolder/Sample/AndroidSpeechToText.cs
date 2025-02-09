using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class AndroidSpeechToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _speechStatusText;
    [SerializeField] private TextMeshProUGUI _speechResultText;
    [SerializeField] private Button _micButton;

    private AndroidJavaClass _nativeRecognizer;
    private AndroidJavaObject _activity;
    private bool _isRunningSpeechRecognizer;
    private float _startTime;

    private const float Timeout = 10.0f;

    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        _micButton.OnClickAsObservable()
            .Where(_ => Application.platform == RuntimePlatform.Android)
            .Where(_ => !_isRunningSpeechRecognizer)
            .Subscribe(_ => StartSpeechRecognizer())
            .AddTo(this);
    }

    private void Update()
    {
        if (Time.time - _startTime > Timeout && _isRunningSpeechRecognizer)
        {
            _micButton.image.color = Color.white;
            _isRunningSpeechRecognizer = false;
            _speechStatusText.text = "";
        }
    }

    private void StartSpeechRecognizer()
    {
        _startTime = Time.time;
        _isRunningSpeechRecognizer = true;
        _micButton.image.color = Color.red;
        _speechResultText.text = "";

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
            _speechStatusText.text = "認識中...";
        }

        // ユーザーが話すのを終了した際のコールバック。
        if (messages[0] == "onEndOfSpeech")
        {
            _speechStatusText.text = "";
            _micButton.image.color = Color.white;
            _isRunningSpeechRecognizer = false;
        }

        // エラーが発生した際のコールバック。タイムアウトの場合も含む。
        if (messages[0] == "onError")
        {
            _speechStatusText.text = "";
            _micButton.image.color = Color.white;
            _isRunningSpeechRecognizer = false;
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
            _speechStatusText.text = "";
            _speechResultText.text = msg;
        }
    }
}