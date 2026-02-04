using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    #region Public Variables
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private TextMeshProUGUI _timerCountText;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _team1Spawns;
    [SerializeField] private Transform[] _team2Spawns;

    public static NetworkGameManager Instance { get; private set; }
    #endregion

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    private NetworkSessionManager _networkSessionManager;

    private int maxPlayers = 2;
    private int timerBeforeStart = 3;
    private bool GameHasStarted = false;

    #region Networked Properties
    [Networked] public TickTimer RoundStartTimer { get; set; }
    #endregion

    public override void Spawned()
    {
        base.Spawned();
        NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
    }

    //On destroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkSessionManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
        NetworkSessionManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
    }

    public override void FixedUpdateNetwork()
    {
        _playerCountText.text = $"Players: {Object.Runner.ActivePlayers.Count()}/{maxPlayers}";

        if (RoundStartTimer.IsRunning)
        {
            _timerCountText.text = RoundStartTimer.RemainingTime(Object.Runner).ToString();
        }
        else
        {
            _timerCountText.text = "";
        }

        if (!GameHasStarted && RoundStartTimer.Expired(Object.Runner))
        {
            GameHasStarted = true;
            OnGameStarted();
        }
    }

    private void OnPlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (NetworkSessionManager.Instance.JoinedPlayers.Count >= maxPlayers)
        {
            //start game count down nad then spawn
            RoundStartTimer = TickTimer.CreateFromSeconds(Object.Runner, timerBeforeStart);
        }
        Debug.Log($"Player {player.PlayerId} Joined");
    }

    private void OnPlayerLeft(PlayerRef player) 
    {
        if (!HasStateAuthority) return;
        if (!_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) return;

        Object.Runner.Despawn(networkObject);
        _spawnedCharacters.Remove(player);
    }

    private void OnGameStarted()
    {
        Debug.Log($"Game Started");
        foreach (var playerSpawn in NetworkSessionManager.Instance.JoinedPlayers)
        {
            var networkObject = Object.Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerSpawn);
            _spawnedCharacters.Add(playerSpawn, networkObject);
        }
    }

    public Vector3 GetSpawnPoint(int team)
    {
        Transform[] spawns = team == 1 ? _team1Spawns : _team2Spawns;
        if (spawns == null || spawns.Length == 0) return Vector3.zero;
        return spawns[UnityEngine.Random.Range(0, spawns.Length)].position;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}

