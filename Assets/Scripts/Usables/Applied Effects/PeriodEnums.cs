namespace Arcatech.Usables.Effects
{
    public enum PeriodicityKind
    {
        OneShot,
        Repeating
    }
    /// <summary> One-shot timing: when the single application fires. </summary>
    public enum OneShotMoment
    {
        AtStart,
        AtEnd
    }
    /// <summary>
    /// Repeating interval mechanic from the design doc.
    /// Before: tick fires at the START of each interval (after offset).
    /// After:  tick fires at the END of each interval (after offset).
    /// </summary>
    public enum IntervalMode
    {
        Before,
        After
    }
}