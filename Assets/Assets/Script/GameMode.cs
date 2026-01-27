using UnityEngine;
using Fusion;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    
    [SerializeField] private MeshRenderer m_MeshRenderer;
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    #region Fusion Callbacks

    //Initialization Logic (New Start/Awake)
    public override void Spawned()
    {

        if (HasInputAuthority) //client
        {
            Transform cameraSpot = transform.Find("Camera");
            if (cameraSpot != null)
            {
                Camera.main.transform.SetParent(cameraSpot);
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.identity;
            }

            var manager = FindAnyObjectByType<NetworkManager>();
            RPC_SetPlayerName(manager.playerName);
            RPC_SetPlayerColor(manager.playerColor);
        }

        if (HasStateAuthority) //server
        {
            NetworkedPosition += new Vector3(0, 1f, 0);
            transform.position = NetworkedPosition;
        }
    }

    //OnDestroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    //Update
    

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (GetInput(out NetworkInputData input))
        {
            if (input.InputVector != Vector3.zero)
            {
                transform.position += input.InputVector * moveSpeed * Runner.DeltaTime;
 
            }

            NetworkedPosition = transform.position;
        }
    }


    //Happens after FixedUpdateNetwork, for non server objects
    public override void Render()
    {
        this.transform.position = NetworkedPosition;
        if(m_MeshRenderer != null && m_MeshRenderer.material.color != PlayerColor)
        {
            m_MeshRenderer.material.color = PlayerColor;
        }

        if (nameText != null)
        {
            nameText.text = PlayerName.ToString();
            nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - Camera.main.transform.position);
        }

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
    private void RPC_SetPlayerName(string name)
    {
        if (HasStateAuthority)
        {
            this.PlayerName = name;
        }
            
    }
    #endregion


    #region UnityCallbacks
    //private void Update()
    //{
    //    if (!HasInputAuthority) return;
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        var randColor = Random.ColorHSV();
    //        RPC_SetPlayerColor(randColor);
    //    }
    //}
    #endregion
}