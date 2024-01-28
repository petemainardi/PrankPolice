using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThatNamespace;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;

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
        [SerializeField] private GameObject SelectScreen;
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

        public async void StartSingleplayerGame()
        {
            try
            {
                SelectScreen.SetActive(false);

                var lobby = await LobbyService.Instance.CreateLobbyAsync($"{Guid.NewGuid()}'s Lobby", 1);
                Allocation alloc = await RelayService.Instance.CreateAllocationAsync(1);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    alloc.RelayServer.IpV4,
                    (ushort)alloc.RelayServer.Port,
                    alloc.AllocationIdBytes,
                    alloc.Key,
                    alloc.ConnectionData
                    );

                if (NetworkManager.Singleton.StartHost())
                    OnStart();
                else
                    SelectScreen.SetActive(true);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                SelectScreen.SetActive(true);
            }
        }

        private void OnStart()
        {
            _gameStarted = true;
            QuitButton.gameObject.SetActive(false);
            Title.SetActive(false);

            if (NetworkManager.Singleton.IsHost)
                FindObjectsByType<SpawnerByCount>(FindObjectsSortMode.None)
                    .ToList().ForEach(spawner => spawner.enabled = true);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}