using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Lobby.MultiTankManagement
{
    public class MultiTankManager : IDisposable
    {
        List<PlayerInput> players = new();

        public void Start()
        {
            WorldTankSpawnerView.Instance.PlayerJoined += OnPlayerJoined;
            WorldTankSpawnerView.Instance.PlayerLeft += OnPlayerLeft;
        }
        public void OnPlayerJoined(PlayerInput playerInput)
        {
            players.Add(playerInput);
        }
        public void OnPlayerLeft(PlayerInput playerInput)
        {
            players.Remove(playerInput);
        }
        public bool IsGameReady()
        {
            if (players.Count < 2)
                return false;

            foreach (PlayerInput player in players)
            {
                if (player.gameObject.GetComponent<SpriteRenderer>().color != Color.green)
                    return false;
            }
            return true;
        }
        public void SetPlayerReady()
        {
            foreach (PlayerInput player in players)
            {
                if (player.actions["Join"].IsPressed())
                {
                    if (player.gameObject.GetComponent<SpriteRenderer>().color == Color.white)
                        player.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                }
            }
        }
        void IDisposable.Dispose()
        {
            WorldTankSpawnerView.Instance.PlayerJoined -= OnPlayerJoined;
            WorldTankSpawnerView.Instance.PlayerLeft -= OnPlayerLeft;
        }
    }
}
