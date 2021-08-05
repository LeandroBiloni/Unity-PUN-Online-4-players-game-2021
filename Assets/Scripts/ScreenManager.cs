using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
public class ScreenManager : MonoBehaviourPunCallbacks
{
    public GameObject waitingScreen;
    public GameObject disconnectScreen;
    public GameObject winScreen;
    public TextMeshProUGUI roomNameText;
    
    public TextMeshProUGUI endText;
    public string levelToLoad;

    private Player _player;

    //Lo ejecuta el local player, notifica al server local para que el server original me desconecte
    public void Disconnect()
    {
        Server.Instance.PlayerLeavesRoom(PhotonNetwork.LocalPlayer);
    }

    public void WaitingScreenState(bool state)
    {
        waitingScreen.SetActive(state);
    }

    public void WinScreen()
    {
        winScreen.SetActive(true);
    }

    public void DisconnectScreen()
    {
        disconnectScreen.SetActive(true);
    }

    public void SetRoomName(string roomName)
    {
        roomNameText.text = "Room name: " + roomName;
    }
}
