using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Public Variables
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Dropdown colorDropdown;
    
    public string playerName;
    public Color playerColor;
    public event Action<PlayerRef> OnPlayerJoinedEvent;
    public event Action<PlayerRef> OnPlayerLeftEvent;
    #endregion
    #region Private Variables
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private NetworkRunner _networkRunner;
    private bool hasName;
    private bool hasColor;
    private List <PlayerRef> JoinedPlayers => _joinedPlayers;
    
    #endregion

    public async void StartGame(GameMode gameMode)
    {
        _networkRunner = this.gameObject.AddComponent<NetworkRunner>();
        _networkRunner.ProvideInput = true;

        if (_networkRunner == null)
{
    _networkRunner = this.gameObject.AddComponent<NetworkRunner>();
    DontDestroyOnLoad(_networkRunner.gameObject);
}

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if(scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "TestSession",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });//Task<StartGameResult>

    }

    #region Unity Callbacks
    private void Start()
    {

    }
    public void StartButton()
    {
       
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            Debug.LogWarning("Name cannot be empty!");
            return;
        }

        playerName = nameInputField.text;
        hasName = true;

        switch (colorDropdown.value)
        {
            case 0: playerColor = Color.red; break;
            case 1: playerColor = Color.blue; break;
            case 2: playerColor = Color.green; break;
            case 3: playerColor = Color.purple; break;
            case 4: playerColor = Color.orange; break;
            case 5: playerColor = Color.black; break;
            default: playerColor = Color.white; break;
        }
        hasColor = true;

        #if SERVER
        StartGame(GameMode.Host);
        #elif CLIENT
        StartGame(GameMode.Client);
        #endif
    }

    #endregion


    #region Used Fusion Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        data.InputVector = (forward * v + right * h).normalized;

        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
       _joinedPlayers.Add(player);
        OnPlayerJoinedEvent?.Invoke(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        _joinedPlayers.Remove(player);
        OnPlayerLeftEvent?.Invoke(player);
    }
    #endregion

    #region Unsused Fusion Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

 
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }


    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
    #endregion
}