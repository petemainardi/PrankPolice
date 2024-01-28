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
    [RequireComponent(typeof(Rigidbody))]
	public class Linkable : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        private Rigidbody _rb;
        private Collider _collider;
        private Transform _linkedTo;

        private bool _defaultGravity;
        private RigidbodyConstraints _defaultConstraints;

        public Rigidbody Rigidbody => _rb;
        // ========================================================================================
        // Mono
        // ========================================================================================
		void Awake()
		{
            _collider = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _defaultGravity = _rb.useGravity;
            _defaultConstraints = _rb.constraints;
		}
        // ----------------------------------------------------------------------------------------
		//void Start()
		//{
			
		//}
        // ----------------------------------------------------------------------------------------
		void Update()
        {
            if (_linkedTo == null) return;

            this.transform.position = _linkedTo.transform.position;
            this.transform.rotation = _linkedTo.transform.rotation;
        }
        // ========================================================================================
        // Methods
        // ========================================================================================
		public bool Link(Transform transformToLink)
        {
            if (_linkedTo != null) return false;

            _linkedTo = transformToLink;
            _collider.enabled = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;
            this.transform.position = _linkedTo.transform.position;
            this.transform.rotation = _linkedTo.transform.rotation;
            return true;
        }
        public void Unlink()
        {
            _linkedTo = null;
            _collider.enabled = true;
            _rb.useGravity = _defaultGravity;
            _rb.constraints = _defaultConstraints;
        }
        // ========================================================================================
	}
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}