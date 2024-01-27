using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine.SceneManagement;

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

	public class LobbyScreen : MonoBehaviour
	{
        // ========================================================================================
        // Fields
        // ========================================================================================
        [SerializeField] private GameObject Panel;

        [SerializeField] private TMP_Text JoinCode;

        [SerializeField] private PlayerPreview Player1;
        [SerializeField] private PlayerPreview Player2;

        [SerializeField] public Button StartGameButton;
        [SerializeField] public Button LeaveButton;

        // ========================================================================================
        // Methods
        // ========================================================================================
        private void Awake()
        {
            StartGameButton.onClick.AddListener(StartGame);
        }
        // ----------------------------------------------------------------------------------------
        public void Show(bool isVisible = true) => Panel.SetActive(isVisible);
        public void UpdateInfo(Lobby lobby)
        {
            if (lobby == null)
            {
                Debug.Log("Lobby was null");
                return;
            }

            JoinCode.text = lobby.LobbyCode;

            string myID = AuthenticationService.Instance.PlayerId;
            bool isHost = myID == lobby.Data["Host"].Value;

            void InitPreview(Player player, PlayerPreview preview)
            {
                bool kickable = isHost && player.Id != myID;
                preview.Init(player.Data["PlayerName"].Value, kickable ? player.Id : "");
                if (kickable)
                    preview.KickButton.onClick.AddListener(() => LobbyService.Instance.RemovePlayerAsync(lobby.Id, player.Id));
            }

            InitPreview(lobby.Players[0], Player1);

            Player2.gameObject.SetActive(lobby.Players.Count > 1);
            if (Player2.gameObject.activeInHierarchy)
                InitPreview(lobby.Players[1], Player1);

            StartGameButton.gameObject.SetActive(isHost);
        }
        // ----------------------------------------------------------------------------------------
        public void StartGame()
        {
            Panel.SetActive(false);
        }
        // ========================================================================================
    }
    // ============================================================================================
    // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // ============================================================================================
}