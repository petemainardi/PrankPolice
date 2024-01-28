using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using PrankPolice;
using ThatNamespace;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;

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

	public class Thrower : NetworkBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        private Pausable _pausable;

        [SerializeField] private Transform Camera;
        [SerializeField] private Transform Hand;
        [SerializeField] private Transform AimTarget;
        [SerializeField] private float MinFocusAimHeight = 6;
        [SerializeField] private float ThrowSpeed = 12;
        [SerializeField] private float FocusVariance = 2;

        [SerializeField] private float GrabDistance = 4;
        private Linkable _throwItem;

        [field: SerializeField] public float TimeToMaxThrow { get; private set; } = 2.2f;
        [field: SerializeField] public float ThrowThreshold { get; private set; } = 0.35f;
        private float _throwTimer = 0;
        public float ThrowForcePercentage => _throwTimer / TimeToMaxThrow;
        public event Action<float> ThrowForceChanged;

        // ========================================================================================
        // Mono
        // ========================================================================================
		void Awake()
		{
		}
        // ----------------------------------------------------------------------------------------
		void Start()
		{
			_pausable = FindFirstObjectByType<Pausable>();
		}
        // ----------------------------------------------------------------------------------------
		void Update()
		{
            if (_pausable.IsPaused) return;

            // Drop held item
            if (Input.GetButton("Fire2") && _throwItem != null)
            {
                _throwItem.Unlink();
                _throwItem = null;
            }
            // Hold throw button to focus fire
            if (Input.GetButton("Fire1") && _throwItem != null && _throwTimer < TimeToMaxThrow)
            {
                _throwTimer += Time.deltaTime;
                _throwTimer = Math.Min(_throwTimer, TimeToMaxThrow);
                ThrowForceChanged?.Invoke(_throwTimer / TimeToMaxThrow);
            }
            // Release throw button to launch held item
            if (Input.GetButtonUp("Fire1"))
            {
                if (_throwItem != null && _throwTimer > ThrowThreshold)
                    Throw(_throwTimer / TimeToMaxThrow);
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(Camera.transform.position, Camera.transform.forward, GrabDistance);
                    //RaycastHit picked = hits.FirstOrDefault(hit => hit.transform.tag == "Throwable");
                    RaycastHit picked = hits.FirstOrDefault(hit => hit.transform.GetComponent<Linkable>());
                    if (picked.rigidbody != null)
                        PickUp(picked.transform.GetComponent<Linkable>());
                }

                _throwTimer = 0;
                ThrowForceChanged?.Invoke(0);
            }
		}
        // ========================================================================================
        // Methods
        // ========================================================================================
		public void PickUp(Linkable throwable)
        {
            if (throwable == null) return;

            if (_throwItem != null)
            {
                //_throwItem.transform.SetParent(null);
                //_throwItem.GetComponent<Collider>().enabled = true;
                _throwItem.Unlink();
            }
            _throwItem = throwable;
            //_throwItem.GetComponent<Collider>().enabled = false;
            //_throwItem.velocity = Vector3.zero;
            //_throwItem.angularVelocity = Vector3.zero;
            //_throwItem.transform.SetParent(Hand);
            //_throwItem.transform.localPosition = Vector3.zero;
            //_throwItem.transform.localRotation = Quaternion.identity;
            _throwItem.Link(Hand);
        }

        public void Throw(float focusPercent)
        {
            if (_throwItem == null) return;

            //_throwItem.transform.SetParent(null);
            //_throwItem.GetComponent<Collider>().enabled = true;
            _throwItem.Unlink();

            float speed = ThrowSpeed + (focusPercent * ThrowSpeed);

            Vector2 noiseDir = UnityEngine.Random.insideUnitCircle * FocusVariance * (1 - focusPercent);
            Vector3 aimDir = AimTarget.position
                + AimTarget.right * noiseDir.x
                + AimTarget.up * noiseDir.y
                + AimTarget.up * (MinFocusAimHeight - AimTarget.localPosition.y) * (1 - focusPercent)
                ;
            aimDir = (aimDir - Hand.position).normalized;

            _throwItem.Rigidbody.velocity = aimDir * speed;
            _throwItem = null;
        }
        // ========================================================================================
	}
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}