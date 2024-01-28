using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
	public class Despawner : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================


        // ========================================================================================
        // Methods
        // ========================================================================================
        private void OnTriggerExit(Collider other)
        {
            Destroy(other.gameObject);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}