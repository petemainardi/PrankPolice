using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using ThatNamespace;
using PrankPolice;

public class FirstPersonMovement : NetworkBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody _rigidbody;
    private RigidbodyConstraints _defaultConstraints;
    private Pausable _pausable;
    private Animator _anim;

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();
    private Vector2 _targetDir = Vector2.zero;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _defaultConstraints = _rigidbody.constraints;
        _pausable = FindFirstObjectByType<Pausable>();
        _anim = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Destroy(GameObject.FindGameObjectWithTag("MainCamera"));
        GetComponentInChildren<SkinnedMeshRenderer>().renderingLayerMask = 0;
    }

    private void Update()
    {
        if (!IsOwner || _pausable.IsPaused) return;

        IsRunning = canRun && Input.GetKey(runningKey);
        _targetDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            if (!_pausable.IsPaused)
            {
                _rigidbody.constraints = _defaultConstraints;

                float targetMovingSpeed = IsRunning ? runSpeed : speed;
                if (speedOverrides.Count > 0)
                    targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();

                Vector2 targetVelocity = _targetDir.normalized * targetMovingSpeed;
                _rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, _rigidbody.velocity.y, targetVelocity.y);
            }
            else
            {
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.constraints = _rigidbody.constraints | RigidbodyConstraints.FreezeRotationY;
            }

            _anim.SetFloat("Speed", _rigidbody.velocity.magnitude);
        }
    }
}