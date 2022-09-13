using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);

    public float movementSpeed = 1.0f;

    private Color[] playerColors = new Color[]
    {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.grey,
        Color.cyan
    };
    private int colorIndex = 0;

    public void Start()
    {
        ApplyPlayerColor();
    }

    public void ApplyPlayerColor()
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    Vector3 CalcMovement()
    {
        Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveVect *= movementSpeed;
        return moveVect;
    }

    [ServerRpc]
    void RequestPositionForMovementServerRPC(Vector3 movement)
    {
        Position.Value += movement;

        float planeSize = 5f;
        Vector3 newPosition = Position.Value + movement;
        newPosition.x = Mathf.Clamp(newPosition.x, planeSize * -1, planeSize);
        newPosition.y = Mathf.Clamp(newPosition.y, planeSize * -1, planeSize);
    }

    [ServerRpc]
    public void RequestNewPlayerColorServerRPC(ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        Color newColor = playerColors[colorIndex];
        colorIndex += 1;
        if(colorIndex > playerColors.Length - 1)
        {
            colorIndex = 0;
        }

        PlayerColor.Value = newColor;
    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector3 move = CalcMovement();
            if(move.magnitude > 0)
            {
                RequestPositionForMovementServerRPC(move);
            }
            else
            {
                transform.position = Position.Value;
            }
        }
    }
}
