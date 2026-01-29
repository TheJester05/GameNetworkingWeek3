using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace Network
{
public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private Dictionary <PlayerRef, NetworkObject> _spawnedCharacters = new();

    private NetworkSessionManager _networkSessionManager;

    private int maxPlayers = 2;
    private int timerBeforeStart = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _networkSessionManager = GetComponent<NetworkSessionManager>();

    }

    public override void Spawned()
    {
        base.Spawned();
        _networkSessionManager.OnPlayerJoinedEvent += OnPlayerJoined;
        _networkSessionManager.OnPlayerLeftEvent += OnPlayerLeft;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        _networkSessionManager.OnPlayerJoinedEvent += OnPlayerJoined;
        _networkSessionManager.OnPlayerLeftEvent += OnPlayerLeft;
    }

    private void OnPlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (_networkSessionManager.JoinedPlayers.Count >= maxPlayers)
        {
            //start game count down and then spawn.
            OnGameStarted();
        }
        Debug.Log($"Player {player.PlayerID} Joined");
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