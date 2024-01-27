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

	public class ConnectionUI : MonoBehaviour
	{		
        // ========================================================================================
        // Methods
        // ========================================================================================
		public void ConnectAsHost()
        {
            NetworkManager.Singleton.StartHost();
            Destroy(this.gameObject);
        }
        // ----------------------------------------------------------------------------------------
        public void ConnectAsClient()
        {
            NetworkManager.Singleton.StartClient();
            Destroy(this.gameObject);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}