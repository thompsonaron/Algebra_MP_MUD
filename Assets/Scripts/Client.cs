using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using EventArgs = System.EventArgs;

public class Client : MonoBehaviour
{
    public WebSocket client;

    public GameObject[] inputs;
    public GameObject[] messagingObj;
    public InputField inputName;
    public InputField messagingField;
    public Text textMessage;

    List<string> messages = new List<string>();

    public void JoinChatroom(string chatroomPath)
    {
        client = new WebSocket("ws://localhost:8080/" + chatroomPath);
        client.OnOpen += Client_OnOpen;
        client.OnMessage += Client_OnMessage;
        client.Connect();


        foreach (var button in inputs)
        {
            button.SetActive(false);
        }

        foreach (var button in messagingObj)
        {
            button.SetActive(true);
        }

    }

    private void Client_OnMessage(object sender, MessageEventArgs e)
    {
        // if is data treat it as world
        if (e.IsBinary)
        {

        }
        // this is probably for messaging
        if (e.IsText)
        {

        }

        Debug.Log(e.Data);
        messages.Add(e.Data);
        //textMessage.text += e.Data;
    }

    private void Client_OnOpen(object sender, EventArgs e)
    {
        client.Send("#name:" + inputName.text);
    }

    private void Update()
    {
        foreach (var msg in messages)
        {
            textMessage.text += msg;
        }
        messages.Clear();
    }

    private void OnApplicationQuit()
    {
        client.Close();
    }


    public void SendText()
    {
        client.Send("#msg:" + messagingField.text);
        messagingField.text = string.Empty;

        // TODO project stuff
        client.Send("#plyMove:" + messagingField.text);
        // 1. Send the position
        // 2. event of WASD
            // 2. #plyMove: 1000 W was up
            // 2. #plyMove: 2000 W was down
            // 2. #plyMove: 0100 A was up
            // 2. #plyMove: 0200 A was up
        // 3. send player position serialized ()
    }

}
