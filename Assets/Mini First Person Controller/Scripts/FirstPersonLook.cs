using UnityEngine;
using Unity.Netcode;
using ThatNamespace;
using PrankPolice;
using System.Linq;

public class FirstPersonLook : NetworkBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    private Pausable _pausable;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            foreach (Component c in gameObject.GetComponents(typeof(Component)).Reverse())
            {
                if (c == this || c.GetType().IsAssignableFrom(typeof(Transform)))
                    continue;
                Destroy(c);
            }
            return;
        }


        Cursor.lockState = CursorLockMode.Locked;
    }

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        _pausable = FindFirstObjectByType<Pausable>();
        _pausable.PausedChanged +=
            paused => Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void Update()
    {
        if (_pausable.IsPaused) return;

        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
