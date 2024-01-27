using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

#pragma warning disable 0649    // Variable declared but never assigned to

namespace ThatNamespace
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

	public class LobbyTest : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        [SerializeField] private int _maxPlayers = 2;
        [SerializeField] private bool _isPrivate = true;
        [SerializeField] private string _playerName = "Player 1";
        [SerializeField] private string _lobbyName = "Lobby 1";

        [SerializeField] private float _heartbeatTime = 15f;
        [SerializeField] private float _pollTime = 1.1f;

        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float _timerHeartbeat;
        private float _timerPoll;

        // ========================================================================================
        // Mono
        // ========================================================================================
		async void Start()
		{
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        // ----------------------------------------------------------------------------------------
        private void Update()
        {
            // Keep the lobby alive on the service
            if (_hostLobby != null)
            {
                _timerHeartbeat -= Time.deltaTime;
                if (_timerHeartbeat  < 0f)
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
            if (_joinedLobby != null)
            {
                _timerPoll -= Time.deltaTime;
                if (_timerPoll < 0f)
                {
                    _timerPoll = _pollTime;
                    _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                }
            }
        }
        // ========================================================================================
        // Methods
        // ========================================================================================
        private async void CreateLobby()
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions {
                    IsPrivate = _isPrivate,
                    Player = GetPlayer()
                };
                _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, _maxPlayers);
                _joinedLobby = _hostLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void ListLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions {
                    Count = 25,
                    Filters = new List<QueryFilter> {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    },
                    Order = new List<QueryOrder> {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    }
                };
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
                foreach (Lobby lobby in queryResponse.Results)
                {

                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }

        }
        // ----------------------------------------------------------------------------------------
        private async void JoinLobbies()
        {
            try
            {
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
                _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(
                    lobbyCode, new JoinLobbyByCodeOptions { Player = GetPlayer() });
                PrintPlayers(_joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void QuickJoinLobby(string lobbyCode)
        {
            try
            {
                await Lobbies.Instance.QuickJoinLobbyAsync();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void LeaveLobby(string lobbyCode)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void KickPlayer(string playerId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void MigrateLobbyHost(string playerId)
        {
            try
            {
                Player newHost = null;
                if (_hostLobby.Players.Count > 1)
                    newHost = _hostLobby.Players[1];
                else
                    return;

                _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = newHost.Id
                });
                _joinedLobby = _hostLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private async void DeleteLobby(string playerId)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        // ----------------------------------------------------------------------------------------
        private Player GetPlayer()
        {
            return new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                }
            );
        }
        private void PrintPlayers(Lobby lobby)
        {
            foreach (var player in lobby.Players)
            {
                Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
            }
        }
        private async void UpdatePlayerName(string newName)
        {
            try
            {
                _playerName = newName;
                await LobbyService.Instance.UpdatePlayerAsync(
                    _joinedLobby.Id,
                    AuthenticationService.Instance.PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data = new Dictionary<string, PlayerDataObject> {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                        }
                    });
            }
            catch (LobbyServiceException e)
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