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

    [Header("Network Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName{ get; set; }
    [Networked] public int Team { get; set; }

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

            NetworkedPosition = this.transform.position;
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
