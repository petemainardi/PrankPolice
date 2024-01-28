using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using TMPro;

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

	public class InfoScreen : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        [Header("Player Name")]
        [SerializeField] private GameObject NameValidationMessage;
        [SerializeField] private TMP_InputField PlayerName;

        [Header("Lobby")]
        [SerializeField] private Button CreateLobbyButton;
        [SerializeField] private Button JoinLobbyButton;
        [SerializeField] private GameObject LobbyValidationMessage;
        [SerializeField] private TMP_InputField LobbyCode;

        [Header("Navigation")]
        [SerializeField] private GameObject ContentParent;
        [SerializeField] private LobbyScreen LobbyScreen;
        [SerializeField] private Button BackButton;

        [Header("Settings")]
        [SerializeField] private float _heartbeatTime = 15f;
        [SerializeField] private float _pollTime = 1.1f;

        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float _timerHeartbeat;
        private float _timerPoll;

        private int _maxPlayers = 2;

        [Space]
        public UnityEvent<Lobby> LobbyJoined = new UnityEvent<Lobby>();
        public UnityEvent GameStarted = new UnityEvent();

        // ========================================================================================
        // Mono
        // ========================================================================================
		async void Start()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            CreateLobbyButton.onClick.AddListener(() => _ = CreateLobby());
            NameValidationMessage.SetActive(false);

            JoinLobbyButton.onClick.AddListener(JoinLobby);
            LobbyValidationMessage.SetActive(false);

            BackButton.onClick.AddListener(LeaveLobby);

            LobbyScreen.StartGameButton.onClick.AddListener(StartGame);
            LobbyScreen.LeaveButton.onClick.AddListener(LeaveLobby);

            LobbyJoined.AddListener(LobbyScreen.UpdateInfo);
        }
        // ----------------------------------------------------------------------------------------
		void Update()
        {
            // Keep the lobby alive on the service
            if (_hostLobby != null)
            {
                _timerHeartbeat -= Time.deltaTime;
                if (_timerHeartbeat < 0f)
                {
                    _timerHeartbeat = _heartbeatTime;
                    LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                }
            }
            // Update local lobby data
            PollLobbyForUpdate();
        }
        private async void PollLobbyForUpdate()
        {
            try
            {
                if (_joinedLobby != null)
                {
                    _timerPoll -= Time.deltaTime;
                    if (_timerPoll < 0f)
                    {
                        _timerPoll = _pollTime;
                        _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                        if (!_joinedLobby.Players.Any(p => p.Id == AuthenticationService.Instance.PlayerId))
                            _joinedLobby = null;

                        // Start game if relay code exists
                        if (_joinedLobby != null && _joinedLobby.Data["RelayCode"].Value != "0")
                        {
                            // Join relay if not the host (which will have already started the relay)
                            if (_joinedLobby.Data["Host"].Value != AuthenticationService.Instance.PlayerId)
                                await JoinRelay(_joinedLobby.Data["RelayCode"].Value);

                            LobbyScreen.StartGame();
                            _joinedLobby = null;
                            GameStarted.Invoke();
                        }
                        else
                            LobbyJoined.Invoke(_joinedLobby);
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
        // ========================================================================================
        // Utility Methods
        // ========================================================================================
        private Player GetPlayer()
        {
            return new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName.text) }
                }
            );
        }
        private bool ValidatePlayerName()
        {
            bool nameExists = PlayerName.text.Length > 0;
            NameValidationMessage.SetActive(!nameExists);
            return nameExists;
        }
        // ----------------------------------------------------------------------------------------
        private void EnableButtons(bool enabled)
        {
            CreateLobbyButton.interactable = enabled;
            JoinLobbyButton.interactable = enabled;
            BackButton.interactable = enabled;
        }
        // ========================================================================================
        // Lobby Methods
        // ========================================================================================
        public async Task CreateLobby()
        {
            LobbyValidationMessage.SetActive(false);
            if (!ValidatePlayerName()) return;

            try
            {
                EnableButtons(false);

                CreateLobbyOptions options = new CreateLobbyOptions {
                    IsPrivate = true,
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject> {
                        { "Host", new DataObject(DataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId) },
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                    }
                };
                _hostLobby = await LobbyService.Instance.CreateLobbyAsync($"{PlayerName.text}'s Lobby", _maxPlayers, options);
                _joinedLobby = _hostLobby;
                LobbyScreen.Show();
                LobbyJoined.Invoke(_joinedLobby);

                EnableButtons(true);
                ContentParent.SetActive(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void JoinLobby()
        {
            LobbyValidationMessage.SetActive(false);
            if (!ValidatePlayerName()) return;

            bool codeExists = PlayerName.text.Length > 0;
            LobbyValidationMessage.SetActive(!codeExists);
            if (!codeExists) return;

            try
            {
                EnableButtons(false);
                if (_hostLobby != null)
                    await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
                _hostLobby = null;

                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(
                    LobbyCode.text, new JoinLobbyByCodeOptions { Player = GetPlayer() }
                    );
                LobbyScreen.Show();
                LobbyJoined.Invoke(_joinedLobby);

                EnableButtons(true);
                ContentParent.SetActive(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void LeaveLobby()
        {
            if (_joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                _joinedLobby = null;
            }
            if (_hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
                _hostLobby = null;
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void QuickJoinLobby()
        {
            try
            {
                EnableButtons(false);
                _joinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                LobbyJoined.Invoke(_joinedLobby);
                EnableButtons(true);
                ContentParent.SetActive(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ========================================================================================
        // Relay Methods
        // ========================================================================================
        private async Task<string> CreateRelay()
        {
            try
            {
                Allocation alloc = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    alloc.RelayServer.IpV4,
                    (ushort)alloc.RelayServer.Port,
                    alloc.AllocationIdBytes,
                    alloc.Key,
                    alloc.ConnectionData
                    );

                NetworkManager.Singleton.StartHost();
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        private async Task JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    alloc.RelayServer.IpV4,
                    (ushort)alloc.RelayServer.Port,
                    alloc.AllocationIdBytes,
                    alloc.Key,
                    alloc.ConnectionData,
                    alloc.HostConnectionData
                    );

                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ========================================================================================
        // 
        // ========================================================================================
        public async void StartGame()
        {
            try
            {
                string relayCode = await CreateRelay();
                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });
                _joinedLobby = lobby;
                LobbyJoined.Invoke(_joinedLobby);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}