using Quantum;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtils
{
	public static Dictionary<PlayerLink, Color> playerColors = new Dictionary<PlayerLink, Color>();

	public static Color GetPlayerColor(this PlayerLink player)
    {
		if (playerColors.ContainsKey(player))
			return playerColors[player];
		else
		{
			Color color = new Color32(
				(byte)player.colorR,
				(byte)player.colorG,
				(byte)player.colorB,
				255);

			playerColors.Add(player, color);
			return color;
		}
	}
}