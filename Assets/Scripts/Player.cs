using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    private GameManager _gameMgr;
    public float movementSpeed = 1.0f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();
            _gameMgr.RequestNewPlayerColorServerRPC();
        }
    }

    public void Start()
    {
        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;
    }

    public void OnPlayerColorChanged(Color previous, Color current)
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
