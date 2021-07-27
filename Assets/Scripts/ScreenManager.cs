using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
public class ScreenManager : MonoBehaviourPun
{
    public GameObject waitingScreen;
    public GameObject disconnectScreen;
    public GameObject winScreen;
    public TextMeshProUGUI roomNameText;
    
    public TextMeshProUGUI endText;
    public string levelToLoad;

    private PhotonView _myView;

    public bool checkCharacters;
    // Start is called before the first frame update
    void Start()
    {
        _myView = GetComponent<PhotonView>();
        checkCharacters = false;
    }

    private void Update()
    {
        // if (checkCharacters && PhotonNetwork.PlayerList.Length < 2)
        // {
        //     EndGame();
        // }
        
    }

    // public void EndGame()
    // {
    //     string win = "";
    //     string lose = "";
    //     
    //     var players = FindObjectsOfType<Character>();
    //
    //     foreach (var character in players)
    //     {
    //         if (character.alive)
    //             win = character.GetPlayerName();
    //         else lose = character.GetPlayerName();
    //     } 
    //     _myView.RPC("ActivateEndGameScreen", RpcTarget.All, win, lose);
    // }
    
    // [PunRPC]
    // public void ActivateEndGameScreen(string win, string lose)
    // {
    //     endScreen.SetActive(true);
    //
    //     if (string.IsNullOrEmpty(lose))
    //         lose = "Disconnected Player";
    //     
    //     endText.text = win +" wins. \n" + lose + " lose.";
    // }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel(levelToLoad);
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
