using UnityEngine;
using Quantum;

public class CustomCallbacks : QuantumCallbacks 
{
    public RuntimePlayer playerData;

    public override void OnGameStart(Quantum.QuantumGame game) 
    {
        // paused on Start means waiting for Snapshot
        if (game.Session.IsPaused) return;

        foreach (int localPlayer in game.GetLocalPlayers())
        {
            Debug.Log("CustomCallbacks - sending player: " + localPlayer);
            game.SendPlayerData(localPlayer, playerData);
        }
    }

    public override void OnGameResync(Quantum.QuantumGame game)
    {
        Debug.Log("Detected Resync. Verified tick: " + game.Frames.Verified.Number);
    }
}

