using System.Collections.Generic;
using System.Linq;

namespace PlatesGame.util;

public static class StringFormatter
{
	public static string FormatPlayerNames(List<string> playerNames)
	{
		if (playerNames.Count == 0)
		{
			return string.Empty;
		}

		switch (playerNames.Count)
		{
			case 1:
				return playerNames[0];
			case 2:
				return $"{playerNames[0]} and {playerNames[1]}";
			default:
				var formattedNames = string.Join(", ", playerNames.Take(playerNames.Count - 1));
				return $"{formattedNames}, and {playerNames.Last()}";
		}
	}
}
