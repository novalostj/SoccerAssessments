using Quantum;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Assets.Scripts.UI
{
    public class ScoreSystem : MonoBehaviour
    {
        public List<TextMeshProUGUI> texts;
        public static event Action OnWin;

        private void Update()
        {
            if (!IsValid())
                return;

            var game = QuantumRunner.Default.Game;
            int[] localPlayer = game.GetLocalPlayers();
            var frame = QuantumRunner.Default.Game.Frames.Predicted;
            var filter = frame.Filter<PlayerLink>();

            List<PlayerLink> playerLinks = new List<PlayerLink>();

            while (filter.Next(out _, out var playerLink))
            {
                playerLinks.Add(playerLink);
                if (playerLink.Player != localPlayer[0])
                    continue;

                CheckScore(playerLink.score);
            }

            SetScoreBoardDisplay(playerLinks);
        }

        private void CheckScore(int score)
        {
            if (score < 3)
                return;

            OnWin?.Invoke();
            enabled = false;
        }

        private bool IsValid()
        {
            if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null)
                return false;

            return QuantumRunner.Default.Game.Frames.Predicted != null;
        }


        private void SetScoreBoardDisplay(List<PlayerLink> playerLinks)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                Color color = playerLinks.Count > i
                    ? playerLinks[i].GetPlayerColor()
                    : Color.white;

                texts[i].text = playerLinks.Count > i ? $"{playerLinks[i].Player._index}: {playerLinks[i].score}" : "";
                texts[i].color = color;
            }
        }
    }
}
