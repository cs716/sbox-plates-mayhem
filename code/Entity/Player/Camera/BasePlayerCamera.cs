using Sandbox;

namespace PlatesGame;

public class BasePlayerCamera : EntityComponent<PlatesPlayer>, ISingletonComponent
{
	public virtual void Update() { }
	public virtual void BuildInput() { }
}
