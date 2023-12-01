using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using System.Linq;
using ExitGames.Client.Photon;
using Quantum.Demo;

public enum GameState
{
    DISCONNECTED,
    CONNECTING,
    ERROR,
    JOINING,
    CREATING,
    WAITING_FOR_PLAYERS,
    CONNECTED
}

public class MultiplayerManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    #region Singleton

    public static MultiplayerManager Singleton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        GameObject instance = new GameObject("MultiplayerManager");
        Singleton = instance.AddComponent<MultiplayerManager>();
        DontDestroyOnLoad(instance);
    }

    #endregion

    public ClientIdProvider.Type idProvider = ClientIdProvider.Type.NewGuid;

    // Room Settings
    private GameState _state = GameState.DISCONNECTED;
    private byte _maxPlayer = 4;
    private byte _requiredPlayerCount = 1;

    private AssetGuid _selectedMapGuid;
    private List<AssetGuid> _mapGuids;
    private LoadBalancingClient _localBalancingClient;
    private RuntimeConfigContainer runtimeConfigContainer;

    public event Action Matchmaking, GameStarted, GameOver;
    public GameState State
    {
        get => _state;
        private set
        {
            _state = value;
            Debug.Log($"Multiplayer State: {_state}");
        }
    }
    public LoadBalancingClient LocalBalancingClient => _localBalancingClient;

    private void Awake()
    {
        PhotonServerSettings serverSettings = PhotonServerSettings.Instance;

        _localBalancingClient = new LoadBalancingClient();
        _localBalancingClient.ConnectionCallbackTargets.Add(this);
        _localBalancingClient.MatchMakingCallbackTargets.Add(this);
        _localBalancingClient.AppId = serverSettings.AppSettings.AppIdRealtime;
        _localBalancingClient.AppVersion = serverSettings.AppSettings.AppVersion;
        _localBalancingClient.ConnectToRegionMaster(serverSettings.AppSettings.FixedRegion);

        MapAsset[] maps = Resources.LoadAll<MapAsset>(QuantumEditorSettings.Instance.DatabasePathInResources);
        _mapGuids = maps.Select(m => m.AssetObject.Guid).ToList();
    }

    private void Update()
    {
        if (State == GameState.DISCONNECTED)
            return;

        if (State == GameState.CONNECTED)
        {
            GameOverCheck();
            return;
        }

        _localBalancingClient?.Service();
        if (_localBalancingClient != null && _localBalancingClient.InRoom)
        {
            var hasStarted = _localBalancingClient.CurrentRoom.CustomProperties.TryGetValue("START", out var start) && (bool)start;

            if (_localBalancingClient.LocalPlayer.IsMasterClient)
                Master(hasStarted);
            if (_selectedMapGuid.IsValid && hasStarted)
                JoinOrCreateRoom();
        }

        void Master(bool inLobby)
        {
            Hashtable ht = new Hashtable();

            if (!inLobby && _localBalancingClient.CurrentRoom.PlayerCount >= _requiredPlayerCount)
            {
                ht.Add("START", true);
                ht.Add("OVER", false);
            }

            if (ht.Count > 0)
                _localBalancingClient.CurrentRoom.SetCustomProperties(ht);
        }

        void JoinOrCreateRoom()
        {
            State = GameState.CONNECTED;

            RuntimeConfig config = runtimeConfigContainer != null ? RuntimeConfig.FromByteArray(RuntimeConfig.ToByteArray(runtimeConfigContainer.Config)) : new RuntimeConfig();
            config.Map.Id = _selectedMapGuid;
            config.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            string clientID = ClientIdProvider.CreateClientId(idProvider, _localBalancingClient);

            QuantumRunner.StartGame(clientID, GetStartParameters(config));
            GameStarted?.Invoke();
        }

        QuantumRunner.StartParameters GetStartParameters(RuntimeConfig config) => new QuantumRunner.StartParameters()
        {
            RuntimeConfig = config,
            DeterministicConfig = DeterministicSessionConfigAsset.Instance.Config,
            GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer,
            PlayerCount = _localBalancingClient.CurrentRoom.MaxPlayers,
            LocalPlayerCount = 1,
            NetworkClient = _localBalancingClient
        };

        void GameOverCheck()
        {
            if (_localBalancingClient != null && _localBalancingClient.InRoom)
            {
                if (_localBalancingClient.CurrentRoom.CustomProperties.TryGetValue("OVER", out var over) && (bool)over || UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                    Disconnect();
            }
        }
    }

    public void StartMatchMaking(RuntimeConfigContainer container, int mapIndex)
    {
        runtimeConfigContainer = container;
        _selectedMapGuid = _mapGuids[mapIndex];
        State = GameState.CONNECTING;
        Matchmaking?.Invoke();
    }

    public void Disconnect()
    {
        if (State != GameState.CONNECTED || _localBalancingClient == null || !_localBalancingClient.InRoom)
            return;

        State = GameState.DISCONNECTED;
        GameOver?.Invoke();
        QuantumRunner.ShutdownAll();
    }

    #region Connection Callbacks

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
        State = GameState.JOINING;
        _localBalancingClient.OpJoinRandomRoom(new OpJoinRandomRoomParams { MatchingType = MatchmakingMode.FillRoom });
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.ServerTimeout)
            _localBalancingClient.ConnectToRegionMaster(PhotonServerSettings.Instance.AppSettings.FixedRegion);
        else
        {
            GameOver?.Invoke();
            State = GameState.DISCONNECTED;
        }

        Debug.Log(cause);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    #endregion

    #region Matchmacking Callback

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("Friend List Update");
    }

    public void OnCreatedRoom()
    {
        Debug.Log("Created Room");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create Room Failed");
    }

    public void OnJoinedRoom()
    {
        Debug.Log("On Joined Room");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join Room Failed");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        State = GameState.CREATING;

        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = _maxPlayer,
            Plugins = new string[] { "QuantumPlugin" }
        };

        _localBalancingClient.OpCreateRoom(new EnterRoomParams() { RoomOptions = roomOptions });

        Debug.Log($"CREATE ROOM WITH {_maxPlayer} MAX PLAYER");
    }

    public void OnLeftRoom()
    {
        Debug.Log("Left Room");
    }

    #endregion


}
