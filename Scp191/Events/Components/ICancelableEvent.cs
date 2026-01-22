namespace Scp191.Events.Components;

public interface ICancelableEvent
{
    public bool IsAllowed { get; set; }
}