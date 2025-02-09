using System.Collections.Generic;
using R3;
using UnityEngine;

public class SequenceHandler : MonoBehaviour
{
    [SerializeField] MonoBehaviourContainer _monoBehaviourContainer;
    
    private readonly Dictionary<AppSequence,ISequence> _sequenceDictionary = new ();
    private readonly Subject<AppSequence> _sequenceSubject = new();
    
    private AppSequence _currentSequence;
    
    private void Awake()
    {
        _sequenceDictionary.Add(AppSequence.Launch, new LaunchSequence());
        _sequenceDictionary.Add(AppSequence.Scan, new ScanSequence());
        _sequenceDictionary.Add(AppSequence.Request, new RequestSequence());
        _sequenceDictionary.Add(AppSequence.TextToSpeech, new TextToSpeechSequence());
        _sequenceDictionary.Add(AppSequence.Idle, new IdleSequence());
        _sequenceDictionary.Add(AppSequence.SpeechToText, new SpeechToTextSequence());

        _sequenceSubject
            .Subscribe(appSequence =>
            {
                _sequenceDictionary[_currentSequence].OnExit(_monoBehaviourContainer);
                _sequenceDictionary[appSequence].OnEnter(this, _monoBehaviourContainer);
                _currentSequence = appSequence;
            }).AddTo(this);
        
        // エントリーポイント
        _sequenceSubject.OnNext(AppSequence.Launch);
    }
    
    public void ChangeSequence(AppSequence appSequence)
    {
        _sequenceSubject.OnNext(appSequence);
        Debug.Log($"ChangeSequence: {appSequence}");
    }
}
