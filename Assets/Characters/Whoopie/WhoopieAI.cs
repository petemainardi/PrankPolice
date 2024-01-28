using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace PrankPolice
{
    [RequireComponent(typeof(Linkable))]
    public class WhoopieAI : NetworkBehaviour
    {
        private NavMeshAgent _agent;
        private Linkable _linker;

        public float EnemyDistanceRun = 4.0f;

        private Transform[] _players;
        private Transform _target;

        // Start is called before the first frame update
        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _linker = GetComponent<Linkable>();


            _linker.LinkedChanged += linked => { _agent.enabled = false; };
            _linker.LinkedChanged += linked =>
            {
                if (linked)
                    Debug.Log($"Linking {this.name} to {_linker.LinkedObj.name}");
                else
                    Debug.Log($"Unlinking {this.name}");
            };
        }

        public override void OnNetworkSpawn()
        {
            _players = GameObject.FindObjectsByType<FirstPersonMovement>(FindObjectsSortMode.None)
                .Select(x => x.transform)
                .ToArray();
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner || !_agent.enabled) return;

            int target = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < _players.Length; i++)
            {
                float dist = Vector3.Distance(transform.position, _players[i].position);
                if (dist < distance)
                {
                    distance = dist;
                    target = i;
                }
            }

            if (distance < EnemyDistanceRun)
            {
                Vector3 dirToPlayer = transform.position - _players[target].position;

                Vector3 newPos = transform.position + dirToPlayer;

                _agent.SetDestination(newPos);
                _target = null;
            }
            else
            {
                if (_target == null || (_target.position - transform.position).sqrMagnitude < 10)
                {
                    var targets = FindObjectsByType<SpawnerByCount>(0);
                    _target = targets[Random.Range(0, targets.Length)].transform;
                }
                _agent.SetDestination(_target.position);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsOwner && collision.gameObject.tag == "Ground")
                _agent.enabled = true;
        }
    }
}
