using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _textBox;
    [SerializeField] private TextMeshProUGUI _chatBox;
    [SerializeField] private TextMeshProUGUI _playersList;
    public void UpdateChatBox(string nickname, string text)
    {
        _chatBox.text += "\n" + nickname + ": " + text;
    }

    public void UpdatePlayersList(Player[] players)
    {
        _playersList.text = "";
        foreach (var p in players)
        {
            _playersList.text += p.NickName + "\n";
        }
    }

    public string GetTextFromBox()
    {
        return _textBox.text;
    }

    public void SendTextToServer()
    {
        Server.instance.RequestSendText(_textBox.text);
        _textBox.text = "";
    }
}
