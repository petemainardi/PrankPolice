using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThatNamespace;

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

	public class MainMenu : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        [SerializeField] private GameObject Title;

        [SerializeField] private Button QuitButton;
        [SerializeField] private InfoScreen InfoScreen;
        private bool _gameStarted = false;

        private Pausable _pausable;
        // ========================================================================================
        // Methods
        // ========================================================================================
        private void Awake()
        {
            _pausable = GetComponent<Pausable>();
            _pausable.PausedChanged += paused => QuitButton.gameObject.SetActive(paused);
        }
        void Start ()
        {
            InfoScreen.GameStarted.AddListener(OnStart);
        }

        private void Update()
        {
            if (!_gameStarted) return;

            if (Input.GetButtonDown("Pause"))
                _pausable.IsPaused = !_pausable.IsPaused;
        }

        public static void Quit() => Application.Quit();

        public void StartSingleplayerGame()
        {

        }

        private void OnStart()
        {
            _gameStarted = true;
            QuitButton.gameObject.SetActive(false);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}