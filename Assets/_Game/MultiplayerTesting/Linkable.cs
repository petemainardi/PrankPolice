using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

#pragma warning disable 0649    // Variable declared but never assigned to

namespace PrankPolice
{
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
    /**
     *  This class does things...
     */
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
    [RequireComponent(typeof(Rigidbody))]
	public class Linkable : NetworkBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        private Rigidbody _rb;
        private Collider _collider;
        private Transform _linkedTo;
        public Transform LinkedObj => _linkedTo;

        private bool _defaultGravity;
        private RigidbodyConstraints _defaultConstraints;
        private float _defaultDrag;

        public Rigidbody Rigidbody => _rb;

        public event Action<bool> LinkedChanged;
        // ========================================================================================
        // Mono
        // ========================================================================================
		void Awake()
		{
            _collider = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _defaultGravity = _rb.useGravity;
            _defaultConstraints = _rb.constraints;
            _defaultDrag = _rb.drag;
		}
        // ----------------------------------------------------------------------------------------
		//void Start()
		//{
			
		//}
        // ----------------------------------------------------------------------------------------
		void Update()
        {
            if (!IsOwner || _linkedTo == null) return;

            this.transform.position = _linkedTo.transform.position;
            this.transform.rotation = _linkedTo.transform.rotation;
        }
        // ========================================================================================
        // Methods
        // ========================================================================================
        [ServerRpc(RequireOwnership = false)]
		public void LinkServerRpc(NetworkBehaviourReference transformToLink)
        {
            if (_linkedTo != null || !transformToLink.TryGet(out ClientNetworkTransform t)) return;

            _linkedTo = t.transform;
            LinkedChanged?.Invoke(true);

            _collider.enabled = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;
            _rb.drag = 0;
            this.transform.position = _linkedTo.transform.position;
            this.transform.rotation = _linkedTo.transform.rotation;
        }
        public void Link(ClientNetworkTransform transformToLink) =>
            LinkServerRpc(new NetworkBehaviourReference(transformToLink));
        // ----------------------------------------------------------------------------------------
        [ServerRpc(RequireOwnership = false)]
        public void UnlinkServerRpc(Vector3 initialVelocity, Vector3 initialRotation)
        {
            _linkedTo = null;
            _collider.enabled = true;
            _rb.useGravity = _defaultGravity;
            _rb.constraints = _defaultConstraints;
            _rb.velocity = initialVelocity;
            _rb.angularVelocity = initialRotation;

            LinkedChanged?.Invoke(false);
        }
        public void Unlink() => UnlinkServerRpc(Vector3.zero, Vector3.zero);
        public void Unlink(Vector3 initialVelocity, Vector3 initialRotation) =>
            UnlinkServerRpc(initialVelocity, initialRotation);
        // ========================================================================================
	}
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}