using Sandbox;

namespace PlatesGame;

public static class GameConfig
{
	[ConVar.Replicated( "min_players" )] 
	public static int MinimumPlayers { get; set; } = 2;

	[ConVar.Replicated( "time_between_rounds" )]
	public static float TimeBetweenRounds { get; set; } = 10f;

	[ConVar.Replicated( "time_winner_screen" )]
	public static float WinnerScreenTime { get; set; } = 20f;

	[ConVar.Replicated( "player_default_gravity" )]
	public static float DefaultGravity { get; set; } = 800f;

}

