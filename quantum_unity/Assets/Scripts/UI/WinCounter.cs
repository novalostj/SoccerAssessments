using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class WinCounter : MonoBehaviour
    {
        public string preTextAppend = "Win: ";
        public TextMeshProUGUI win;

        private int winCount = 0;
        private const string KEY = "WIN_COUNT";

        private void OnEnable()
        {
            ScoreSystem.OnWin += IncreaseWin;
            winCount = PlayerPrefs.GetInt(KEY, 0); 
            RefreshText();
        }

        private void OnDisable()
        {
            ScoreSystem.OnWin -= IncreaseWin;
            PlayerPrefs.SetInt(KEY, winCount);
        }

        private void IncreaseWin()
        {
            winCount++;
            Hashtable ht = new Hashtable
            {
                { "OVER", true }
            };
            MultiplayerManager.Singleton.LocalBalancingClient.CurrentRoom.SetCustomProperties(ht);

            PlayerPrefs.SetInt(KEY, winCount);
            RefreshText();
        }

        private void RefreshText() => win.text = $"{preTextAppend}{winCount}";

#if UNITY_EDITOR

        [Header("EDITOR")]
        public bool resetWinScore;

        private void OnValidate()
        {
            if (resetWinScore)
                PlayerPrefs.DeleteKey(KEY);
            resetWinScore = false;
        }


#endif
    }
}
