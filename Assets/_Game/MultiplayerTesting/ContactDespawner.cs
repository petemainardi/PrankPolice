using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    [RequireComponent(typeof(Collider))]
	public class ContactDespawner : NetworkBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        private Collider _collider;
        public event Action<Vector3, Vector3> DestroyedObject;

        // ========================================================================================
        // Methods
        // ========================================================================================
        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Vector3 contactPos = _collider.ClosestPointOnBounds(other.transform.position);
            Destroy(other.gameObject);
            DestroyedClientRpc(contactPos, (this.transform.position - contactPos).normalized);
        }

        [ClientRpc]
        public void DestroyedClientRpc(Vector3 pos, Vector3 dir)
        {
            DestroyedObject?.Invoke(pos, dir);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}