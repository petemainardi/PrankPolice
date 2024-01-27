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

    public class Pausable : MonoBehaviour
    {
        public event Action<bool> PausedChanged;
        private bool _paused;
        public bool IsPaused
        {
            get => _paused;
            set
            {
                if (_paused != value)
                {
                    _paused = value;
                    PausedChanged?.Invoke(_paused);
                }
            }

        }
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}