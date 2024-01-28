using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using PrankPolice;

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

	public class SpawnerByCount : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        public int MaxCount = 1;
        private int _count = 0;

        public float Delay = 1.0f;
        private float _timer = -1.0f;

        [SerializeField] private Transform[] Spawnables;
        [SerializeField] private Transform SpawnLocation;
        // ========================================================================================
        // Mono
        // ========================================================================================
        void Awake()
        {
            _timer = Delay;
        }
        // ----------------------------------------------------------------------------------------
        //void Start()
        //{

        //}
        // ----------------------------------------------------------------------------------------
        void Update()
		{
			if (_count < MaxCount)
            {
                _timer -= Time.deltaTime;
                if (_timer < 0.0f)
                {
                    Spawn();
                }
            }
		}
        // ========================================================================================
        // Methods
        // ========================================================================================
		public void Spawn()
        {
            int index = UnityEngine.Random.Range(0, Spawnables.Length);
            Transform obj = Instantiate(Spawnables[index], SpawnLocation.position, Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();

            if (!obj.TryGetComponent(out DestroyNotification onDestroy))
                onDestroy = obj.gameObject.AddComponent<DestroyNotification>();
            onDestroy.Destroyed += _ => _count--;

            _count++;
            _timer = Delay;
        }
        // ========================================================================================
	}
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}