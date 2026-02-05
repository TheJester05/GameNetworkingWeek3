using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TextMeshProUGUI playerNameTxt;
    [SerializeField] private Transform cameraPos;

    private static readonly int Speed = Animator.StringToHash(name: "Speed");
    private static readonly int Jump = Animator.StringToHash(name: "Jump");
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    [SerializeField] private Animator _animator;

    [Header("Network Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName{ get; set; }
    [Networked] public int Team { get; set; }
    [Networked] public NetworkAnimatorData NetworkAnimatorData { get; set; }

    private Vector3 lastKnownPosition;
    [SerializeField] private float lerpSpeed = 3f;

    [Header("Network Manager")]
    public NetworkSessionManager networkManager;

    #region Fusion Callbacks
    //relevant to the network, do it in spawned (initialization)
    public override void Spawned()
    {
        if (HasInputAuthority) //client
        {
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.SetParent(cameraPos.transform);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;

            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkSessionManager>();
            RPC_SetPlayerCustoms(Color.white, "Player " + UnityEngine.Random.Range(0, 100), networkManager.selectedTeam);
        }

        if (HasStateAuthority) //server
        {
        }
    }

    //On destroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    //update function
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if(GetInput(out NetworkInputData input))
        {
            this.transform.position += 
                new Vector3(input.InputVector.normalized.x, input.InputVector.normalized.y) 
                * Runner.DeltaTime;

            if (input.JumpInput)
                _animator.SetTrigger(name: Jump.ToString());

            _animator.SetFloat(id:Speed, input.SprintInput ? 1f : 0f);
            

            NetworkedPosition = this.transform.position;

            AnimatorData = new NetworkAnimatorData()
            {
                Speed = input.SprintInput ? 1f : 0f,
                Jump = input.JumpInput,
            };
        }
    }

    //happens after fixedupdatenetwork, for nonserver objects
    public override void Render()
    {
        this.transform.position = NetworkedPosition;
        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }

        if (playerNameTxt != null)
            playerNameTxt.text = PlayerName.ToString();
    }

    private void LateUpdate()
    {
        this.transform.position = Vector3.Lerp(lastKnownPosition, NetworkedPosition,Runner.DeltaTime * lerpSpeed);
        lastKnownPosition = NetworkedPosition;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerCustoms(Color color, string name, int team)
    {
        PlayerColor = color;
        PlayerName = name;
        Team = team;

        // Teleport to team spawn point on the server
        if (NetworkGameManager.Instance != null)
        {
            transform.position = NetworkGameManager.Instance.GetSpawnPoint(team);
            NetworkedPosition = transform.position;
        }
    }

    #endregion

    #region Unity Callbacks

    #endregion
}
