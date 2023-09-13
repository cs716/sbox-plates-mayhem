using Sandbox;

namespace PlatesGame;

public class TestBot : Bot
{
	[ConCmd.Admin( "bot_custom", Help = "Spawn my custom bot." )]
	internal static void SpawnCustomBot()
	{
		Game.AssertServer();

		// Create an instance of your custom bot.
		_ = new TestBot();
	}

	public override void BuildInput()
	{
		// And here, we'll make the bot walk forward and turn in a wide circle.
		Input.AnalogMove = Vector3.Forward;
		Input.AnalogLook = new Angles( 0, 0, 0 );
		// Finally, we'll call BuildInput on the bot's client's pawn. 
		// Note that Entity.BuildInput is NOT automatically called for the pawns of
		// simulated clients that are driven by bots, so that's why we call it here.
		(Client.Pawn as Entity).BuildInput();
	}
}
