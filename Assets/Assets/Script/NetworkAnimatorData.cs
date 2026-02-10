using Fusion;

namespace Network
{
    public struct NetworkAnimatorData : INetworkStruct
    {
        public float Horizontal;
        public float Vertical;
        public NetworkBool Jump;
        public NetworkBool Crouch;
    }
}