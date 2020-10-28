﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    Players players = new Players();

    private bool worldLoaded = false;
    private World world;
    Serializator s = new Serializator();

    public GameObject hole;
    public GameObject player;
    public GameObject ground;
    public GameObject wall;
    private List<GameObject> playerObjects = new List<GameObject>();

    List<MessageEventArgs> serverMessages = new List<MessageEventArgs>();

    private void Update()
    {
        lock (serverMessages)
        {
            foreach (var msg in serverMessages)
            {
                // if is data treat it as world
                if (msg.IsBinary)
                {
                    if (!worldLoaded)
                    {
                        world = s.DeserializeWorld(msg.RawData);
                        worldLoaded = true;
                        BuildWorld();
                    }
                    else
                    {
                        //Player c = new Player();
                        //c = s.DeserializePlayer(msg.RawData);
                        //Debug.Log(c.nick);

                        players = s.DeserializePlayers(msg.RawData);

                        foreach (var item in players.players)
                        {
                            Debug.Log(item.nick);
                        }

                        // Position players
                        PositionPlayers();
                    }
                }
                // this is probably for messaging
                if (msg.IsText)
                {
                    messages.Add(msg.Data);
                }

                //Debug.Log(e.Data);
                //textMessage.text += e.Data;
                foreach (var message in messages)
                {
                    textMessage.text += message;
                }
            }
            messages.Clear();
            serverMessages.Clear();
        }

        //If the input field is focused, change its color to green.
        //if (messagingField.isFocused == true)
        //{
            
        //}
        //else
        //{

        //}

        //if (Input.GetKeyDown(KeyCode.W))
        //{

        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{

        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{

        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{

        //}
    }

    private void PositionPlayers()
    {
        // TODO remove all player objects then add them back in when instantiating
        if (playerObjects.Count > 0)
        {
            foreach (var item in playerObjects)
            {
                Destroy(item);
            }
        }
        foreach (var item in players.players)
        {
            int posX = item.position % 10;
            int posZ = item.position / 10;
            playerObjects.Add(Instantiate(player, new Vector3(posX, 1f, posZ), Quaternion.identity));
        }
    }

    public void JoinChatroom(string chatroomPath)
    {
        client = new WebSocket("ws://localhost:8080/" + chatroomPath);
        client.OnOpen += Client_OnOpen;
        client.OnMessage += Client_OnMessage;
        client.Connect();

        foreach (var button in inputs) { button.SetActive(false); }
        foreach (var button in messagingObj) { button.SetActive(true); }

    }

    private void Client_OnMessage(object sender, MessageEventArgs e)
    {
        lock (serverMessages)
        {
            serverMessages.Add(e);
        }
    }

    private void BuildWorld()
    {
        int counter = 0;
        int zPos = 0;
        for (int i = 0; i < world.ground.Length; i++)
        {
            switch (world.ground[i])
            {
                case 0:
                    Instantiate(wall, new Vector3(counter, 0.5f, zPos), Quaternion.identity);
                    break;
                case 1:
                    Instantiate(ground, new Vector3(counter, 0f, zPos), Quaternion.identity);
                    break;
                case 2:
                    Instantiate(hole, new Vector3(counter, 0f, zPos), Quaternion.identity);
                    break;
                default:
                    break;
            }
            counter++;
            if (counter == 10)
            {
                zPos++;
                counter = 0;
            }
        }
    }

    private void Client_OnOpen(object sender, EventArgs e)
    {
        client.Send("#name:" + inputName.text);
    }

    private void OnApplicationQuit()
    {
        client.Close();
    }

    public void SendText()
    {
        client.Send("#msg:" + messagingField.text);
        messagingField.text = string.Empty;

        //// TODO project stuff
        //client.Send("#plyMove:" + messagingField.text);
        //// 1. Send the position
        //// 2. event of WASD
        //// 2. #plyMove: 1000 W was up
        //// 2. #plyMove: 2000 W was down
        //// 2. #plyMove: 0100 A was up
        //// 2. #plyMove: 0200 A was up
        //// 3. send player position serialized ()
    }

    bool moveReady = true;
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (!worldLoaded || !moveReady)
            {
                return;
            }

            moveReady = false;

            // Sending movement
            switch (e.keyCode)
            {
                case KeyCode.W:
                    client.Send("#plyMove:W");
                    break;
                case KeyCode.S:
                    client.Send("#plyMove:S");
                    break;
                case KeyCode.A:
                    client.Send("#plyMove:A");
                    break;
                case KeyCode.D:
                    client.Send("#plyMove:D");
                    break;
                default:
                    break;
            }
            StartCoroutine(KeyCooldown());

            IEnumerator KeyCooldown()
            {
                yield return new WaitForSeconds(0.1f);
                moveReady = true;
            }
        }
    }
}
