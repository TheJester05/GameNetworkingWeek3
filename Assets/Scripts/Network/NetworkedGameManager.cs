using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

namespace Network
{
    public class NetworkedGameManager : NetworkBehaviour
    {
        #region Public Variables
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private TextMeshProUGUI _timerCountText;
        #endregion
        
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
        
        private const int maxPlayers = 2;
        private const int timerBeforeStart = 3;
        private bool hasGameStarted = false;
        #region Networked Properties
        [Networked] public TickTimer RoundStartTimer { get; set; }
        #endregion

        public override void Spawned()
        {
            base.Spawned();
            NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
            NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            
            NetworkSessionManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
            NetworkSessionManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (RoundStartTimer.Expired(Object.Runner))
            {
                RoundStartTimer = default;
                OnGameStarted();
            }
        }

        public override void Render()
        {
            _playerCountText.text = 
                $"Players: {Object.Runner.ActivePlayers.Count()}/{maxPlayers}";
            
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
            if (NetworkSessionManager.Instance.JoinedPlayers.Count 
                >= maxPlayers)
            {
                //start game count down and then spawn.
                RoundStartTimer = TickTimer.CreateFromSeconds(
                    Object.Runner,
                    timerBeforeStart);
            }
            Debug.Log($"Player {player.PlayerId} Joined");
        }
        
        private void OnPlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            if (!_spawnedCharacters.TryGetValue(player, 
                    out NetworkObject networkObject)) return;
            Object.Runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }

        private void OnGameStarted()
        {
            Debug.Log($"Game Started");
            if (hasGameStarted) return; 
            hasGameStarted = true;
            foreach (var playerSpawn 
                     in NetworkSessionManager.Instance.JoinedPlayers)
            {
                var networkObject = Object.Runner.Spawn(playerPrefab, 
                    Vector3.zero, Quaternion.identity, playerSpawn);
                _spawnedCharacters.Add(playerSpawn, networkObject);
            }
        }
    }
}