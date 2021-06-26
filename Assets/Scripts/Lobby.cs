using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class Lobby : MonoBehaviourPunCallbacks
{
   public GameObject mainScreen;
   public GameObject connectedScreen;
   public TMP_InputField createField;
   public TMP_InputField joinField;
   public TextMeshProUGUI welcomeText;
   public string levelToLoad;
   public TextMeshProUGUI errorText;
   public string playerName;
   public void CreateRoom()
   {
      if (string.IsNullOrEmpty(createField.text))
      {
         SetError("Failed to create room. \nPlease introduce a room name.");
      }
      else
      {
         RoomOptions roomOptions = new RoomOptions();
         roomOptions.MaxPlayers = 2;
         PhotonNetwork.CreateRoom(createField.text, roomOptions);
      }
   }

   public void JoinRoom()
   {
      if (string.IsNullOrEmpty(joinField.text))
      {
         SetError("Failed to join room. \nPlease introduce a room name.");
      }
      else PhotonNetwork.JoinRoom(joinField.text);
   }

   public override void OnCreatedRoom()
   {
      Debug.Log("Created Room");
   }
   public override void OnJoinedRoom()
   {
      PhotonNetwork.NickName = playerName;
      PhotonNetwork.LoadLevel(levelToLoad);
   }
   public override void OnCreateRoomFailed(short returnCode, string message)
   {
      SetError("Failed to create room '" + createField.text + "'. \nError: " + returnCode + " \n" + message);
   }
   public override void OnJoinRoomFailed(short returnCode, string message)
   {
       SetError("Failed to join room '" + joinField.text + "'. \nError: " + returnCode + " \n" + message);
   }

   void SetError(string text)
   {
      errorText.text = text;
   }
   
   public void SetWelcomeText(string nickname)
   {
      playerName = nickname;
      welcomeText.text = "Welcome " + nickname;
   }

   public void Disconnect()
   {
      PhotonNetwork.Disconnect();
   }


   public override void OnDisconnected(DisconnectCause cause)
   {
      connectedScreen.SetActive(false);
      mainScreen.SetActive(true);
   }
   
}
