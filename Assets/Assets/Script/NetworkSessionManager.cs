using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Public Variables
    public static NetworkSessionManager Instance { get; private set; }
    public int selectedTeam = 1;

    public void SelectTeam(int team)
    {
        selectedTeam = team;
        StartGame(GameMode.Client);
    }

    #endregion

    #region Private Variables
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = null;
    private NetworkRunner _networkRunner;

    public List<PlayerRef> _joinedPlayers = new();
    public IReadOnlyList<PlayerRef> JoinedPlayers => _joinedPlayers;

    public event Action<PlayerRef> OnPlayerJoinedEvent;
    public event Action<PlayerRef> OnPlayerLeftEvent;
    #endregion

    async void StartGame(GameMode game)
    {
        // Safety: Prevent multiple runners on one object
        if (_networkRunner != null && (_networkRunner.IsRunning || _networkRunner.IsStarting)) return;

        if (_networkRunner == null)
            _networkRunner = gameObject.GetComponent<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
            
        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        
        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = game, // correctly use the passed parameter
            SessionName = "MyUniqueRoom_123456", // Keep unique to avoid collisions (Code 104)
            Scene = scene,
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    #region Unity Callbacks
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Correctly start as Server if in a standalone server build
        #if SERVER
        StartGame(GameMode.Server);
        #endif
    }

    public void StartButton()
    {
        // Correctly start as Client when button is clicked
        StartGame(GameMode.Client);
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


    #region
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
    #endregion
}