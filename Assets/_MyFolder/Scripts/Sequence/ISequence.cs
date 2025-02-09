
public interface ISequence 
{
    void OnEnter(SequenceHandler sequenceHandler, MonoBehaviourContainer monoBehaviourContainer);
    void OnExit(MonoBehaviourContainer monoBehaviourContainer);
}