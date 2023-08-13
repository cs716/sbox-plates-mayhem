namespace PlatesGame.Event.Events;

public class SafeEvent : BaseEvent
{
	public override EventManager.EventType EventType => EventManager.EventType.RoundEvent;
	public override double EventWeight => 0;
	public override bool IsSecret => false;

	public override void OnEnter()
	{
		base.OnEnter();
		Name = "Safe Event";
		Description = "A safe event where nothing happens";
		ShortName = "Safe Event";
	}
}
