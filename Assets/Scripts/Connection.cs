using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Connection : MonoBehaviourPunCallbacks
{
    public GameObject mainScreen;
    public GameObject connectedScreen;
    public TMP_InputField nicknameField;
    public TextMeshProUGUI errorText;
    public void Connect()
    {
        if (string.IsNullOrWhiteSpace(nicknameField.text))
        {
            ErrorText("Please set a nickname.");
            return;
        }
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     errorText.text = "Connection failed: " + cause.ToString();
    // }

    void ErrorText(string text)
    {
        errorText.text = "Error: " + text;
    }

    public override void OnJoinedLobby()
    {
        mainScreen.SetActive(false);
        connectedScreen.SetActive(true);
        var lobby = FindObjectOfType<Lobby>();
        lobby.SetWelcomeText(nicknameField.text);
    }
}
