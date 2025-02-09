using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonoBehaviourContainer : MonoBehaviour
{
    [SerializeField] private SpeechToTextCallbackReceiver _speechToTextCallbackReceiver;
    [SerializeField] private Image _micImage;
    [SerializeField] private Button _micButton;
    [SerializeField] private Button _scanButton;
    [SerializeField] private CanvasGroup _progressCircleCanvasGroup;
    [SerializeField] private TextMeshProUGUI _progressCircleText;
    
    public SpeechToTextCallbackReceiver SpeechToTextCallbackReceiver => _speechToTextCallbackReceiver;
    public Button ScanButton => _scanButton;
    public Button MicButton => _micButton;
    public Image MicImage => _micImage;
    public CanvasGroup ProgressCircleCanvasGroup => _progressCircleCanvasGroup;
    public TextMeshProUGUI ProgressCircleText => _progressCircleText;
}
