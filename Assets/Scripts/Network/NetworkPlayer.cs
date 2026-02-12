using System;
using Fusion;
using Network;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkPlayer : NetworkBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Jump = Animator.StringToHash("Jump");
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    
    [SerializeField] private Animator _animator;

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }

    [Networked] public NetworkAnimatorData AnimatorData { get; set; }

    #region Interpolation Variables
    private Vector3 _lastKnownPosition;
    [SerializeField]private float _lerpSpeed = 3f;
    #endregion

    #region Fusion Callbacks
    public override void Spawned()
    {
        if (HasInputAuthority) // client
        {
            
        }

        if (HasStateAuthority) // server
        {
            PlayerColor = Random.ColorHSV();

            AnimatorData = new NetworkAnimatorData()
            {
                Speed = 0,
                Jump = false
            };


        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (!GetInput(out NetworkInputData input)) return;

        this.transform.position +=
            new Vector3(input.InputVector.normalized.x,
                0,
                input.InputVector.normalized.y)
            * Runner.DeltaTime;

        if (input.JumpInput)
            _animator.SetTrigger("Jump");

        _animator.SetFloat(Speed, input.SprintInput ? 1f : 0f);
        
        NetworkedPosition = this.transform.position;
        AnimatorData = new NetworkAnimatorData()
        {
            Speed = input.SprintInput ? 1f : 0f,
            Jump = input.JumpInput,
        };
    }

    public override void Render()
    {
        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }
        
        if (AnimatorData.Jump)
            _animator.SetTrigger("Jump");

        _animator.SetFloat(Speed, AnimatorData.Speed);

    }

    public void LateUpdate()
    {
        this.transform.position = Vector3.Lerp(_lastKnownPosition, NetworkedPosition, Runner.DeltaTime * _lerpSpeed);
        _lastKnownPosition = NetworkedPosition;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(Color color)
    {
        if (HasStateAuthority)
        {
            this.PlayerColor = color;
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string color)
    {
        if (HasStateAuthority)
        {
            this.PlayerName = color;
        }
        //example of how to use string
        //this.PlayerName.ToString();
    }

    #endregion
    
    #region Unity Callbacks

    private void Update()
    {
        if(!HasInputAuthority) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var randColor = Random.ColorHSV();
            RPC_SetPlayerColor(randColor);
        }
    }
    
    #endregion
    
}
