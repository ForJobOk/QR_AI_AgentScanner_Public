using R3;
using TMPro;
using UnityEngine;

public class OnScanQrTest : MonoBehaviour
{
    [SerializeField] private XrealQrCodeReader _qrCodeReader;
    [SerializeField] private TextMeshProUGUI _scanQrResultTex;

    private void Start()
    {
        _qrCodeReader.OnTextScanned
            .Subscribe(text => _scanQrResultTex.text = text)
            .AddTo(this);
    }
}
