using System;
using Fusion;
using Network;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkPlayer : NetworkBehaviour
{
    private static readonly int Jump = Animator.StringToHash("Jump");
    [SerializeField] private Renderer _meshRenderer;
    
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
            Transform cameraSpot = transform.Find("Camera");
            if (cameraSpot != null)
            {
                Camera.main.transform.SetParent(cameraSpot);
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.identity;
            }
        }

        if (HasStateAuthority) // server
        {
            PlayerColor = Random.ColorHSV();

            AnimatorData = new NetworkAnimatorData()
            {
                Horizontal = 0,
                Vertical = 0,
                Jump = false,
                Crouch = false
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

        float moveSpeed = 5f; // Base speed
        if (input.CrouchInput) moveSpeed = 2.5f;
        // User requested no sprint

        Vector3 moveDirection = new Vector3(input.InputVector.x, 0, input.InputVector.y).normalized;
        
        if (moveDirection.magnitude > 0.1f)
        {
            this.transform.position += moveDirection * moveSpeed * Runner.DeltaTime;
        }

        // Sync Rotation: face the direction of movement
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Runner.DeltaTime * 10f);
        }

        if (input.JumpInput)
            _animator.SetTrigger("Jump");

        _animator.SetFloat("Horizontal", input.InputVector.x);
        _animator.SetFloat("Vertical", input.InputVector.y);
        _animator.SetBool("Crouch", input.CrouchInput);
        
        NetworkedPosition = this.transform.position;
        AnimatorData = new NetworkAnimatorData()
        {
            Horizontal = input.InputVector.x,
            Vertical = input.InputVector.y,
            Jump = input.JumpInput,
            Crouch = input.CrouchInput
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

        _animator.SetFloat("Horizontal", AnimatorData.Horizontal);
        _animator.SetFloat("Vertical", AnimatorData.Vertical);
        _animator.SetBool("Crouch", AnimatorData.Crouch);

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
