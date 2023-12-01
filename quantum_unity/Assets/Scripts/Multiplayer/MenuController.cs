using Quantum.Demo;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public int mapIndex;
    public RuntimeConfigContainer runtimeContainer;
    public GameObject menuUI;
    public UnityEngine.UI.Button joinButton;
    public TextMeshProUGUI mainText;

    private MultiplayerManager _multiplayerManager;

    private void Awake()
    {
        _multiplayerManager = MultiplayerManager.Singleton;
    }

    private void OnEnable()
    {
        _multiplayerManager.Matchmaking += DuringMatchmaking;
        _multiplayerManager.GameStarted += GameStarted;
        _multiplayerManager.GameOver += GameOver;
    }

    private void OnDisable()
    {
        _multiplayerManager.Matchmaking -= DuringMatchmaking;
        _multiplayerManager.GameStarted -= GameStarted;
        _multiplayerManager.GameOver -= GameOver;
    }

    public void Join() => 
        _multiplayerManager.StartMatchMaking(runtimeContainer, mapIndex);

    private void DuringMatchmaking()
    {
        mainText.text = "Joining...";
        joinButton.interactable = false;
    }

    private void GameStarted()
    {
        mainText.text = "";
        menuUI.SetActive(false);
    }

    private void GameOver()
    {
        mainText.text = "";
        menuUI.SetActive(true);
        joinButton.interactable = true;
    }
}
