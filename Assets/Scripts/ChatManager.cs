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
    [SerializeField] private List<Color> _colorsList = new List<Color>();
    public void UpdateChatBox(int pos, string nickname, string text)
    {
        var color = ColorUtility.ToHtmlStringRGB(_colorsList[pos]);
        _chatBox.text += "\n" + "<b><color=#"+color+">" + nickname + "</color></b>" + ": " + text;
    }

    public void UpdatePlayersList(Player[] players)
    {
        _playersList.text = "";
        int count = 0;
        foreach (var p in players)
        {
            var color = ColorUtility.ToHtmlStringRGB(_colorsList[count]);
            _playersList.text += "<b><color=#"+color+">" + p.NickName + "</color></b>" + "\n";
            count++;
        }
    }

    public string GetTextFromBox()
    {
        return _textBox.text;
    }

    public void SendTextToServer()
    {
        Server.Instance.RequestSendText(_textBox.text);
        _textBox.text = "";
    }
}
