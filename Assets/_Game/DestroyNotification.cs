using System;
using UnityEngine;

namespace PrankPolice
{
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
    /**
     *  Send an event to notify when the GameObject is being destroyed.
     */
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================

    [DisallowMultipleComponent]
	public class DestroyNotification : MonoBehaviour
	{
        public event Action<GameObject> Destroyed;
        private void OnDestroy()
        {
            Destroyed?.Invoke(this.gameObject);
        }
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}