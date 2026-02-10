using UnityEngine;
using Fusion;
using System.Collections.Generic;

using System.Linq;
using TMPro;

namespace Network
{
public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private TextMeshProUGUI _timerCountText;

    private Dictionary <PlayerRef, NetworkObject> _spawnedCharacters = new();

    private NetworkSessionManager _networkSessionManager;

    private int maxPlayers = 2;
    private int timerBeforeStart = 3;

    [Networked] public TickTimer RoundStartTimer { get; set; }
    [Networked] public NetworkBool GameHasStarted { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _networkSessionManager = GetComponent<NetworkSessionManager>();

    }

    public override void Spawned()
    {
        base.Spawned();
        NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
            NetworkSessionManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (RoundStartTimer.Expired(Object.Runner))
        {
            if (!GameHasStarted)
            {
                GameHasStarted = true;
                RoundStartTimer = default;
                OnGameStarted();
            }
        }
    }

    public override void Render()
    {
        base.Render();
        _playerCountText.text = $"Players: {Object.Runner.ActivePlayers.Count()}/{maxPlayers}";

        if (RoundStartTimer.IsRunning)
        {
            _timerCountText.text = RoundStartTimer.RemainingTime(Object.Runner).ToString();
        }
        else
        {
            _timerCountText.text = "";
        }
    }

    private void OnPlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (NetworkSessionManager.Instance.JoinedPlayers.Count >= maxPlayers)
        {
            //start game count down and then spawn.
            OnGameStarted();
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
        foreach (var player in _networkSessionManager.JoinedPlayers)
            {
                var networkObject = Object.Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
                _spawnedCharacters.Add(player, networkObject);
            }
    }
}
}