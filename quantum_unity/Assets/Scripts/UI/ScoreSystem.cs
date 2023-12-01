using Quantum;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ScoreSystem : MonoBehaviour
    {
        public string preTextAppend = "Score: ";
        public TextMeshProUGUI score;

        public static event Action OnWin;

        private void Update()
        {
            if (!IsValid())
                return;

            var game = QuantumRunner.Default.Game;
            int[] localPlayer = game.GetLocalPlayers();
            var frame = QuantumRunner.Default.Game.Frames.Predicted;
            var filter = frame.Filter<PlayerLink>();

            while (filter.Next(out _, out var playerLink))
            {
                if (playerLink.Player != localPlayer[0])
                    continue;

                score.text = $"{preTextAppend}{playerLink.score}";
                CheckScore(playerLink.score);
            }
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
    }
}
