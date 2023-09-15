using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace PlatesGame;

public static class Stat
{
	public static string Wins = "wins";
	public static string Deaths = "deaths";
	public static string EventsSurvived = "events";
}

public static class Players
{
	public static IEnumerable<PlatesPlayer> GetLiving()
	{
		return Entity.All.OfType<PlatesPlayer>().Where( p => p.LifeState == LifeState.Alive );
	}
	
}
