using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using WebSocketSharp;
using EventArgs = System.EventArgs;

public class Client : MonoBehaviour
{
    private Serializator ser = new Serializator();
    public WebSocket client;
    private List<string> messages = new List<string>();
    private List<MessageEventArgs> serverMessages = new List<MessageEventArgs>();
    private Players players = new Players();
    private List<GameObject> playerObjects = new List<GameObject>();
    private bool moveReady = true;
    private bool worldLoaded = false;
    private bool writingMessage = false;
    private World world;

    [Header("UI Elements")]
    public GameObject[] inputs;
    public GameObject[] messagingObj;
    public InputField inputName;
    public InputField messagingField;
    public Text textMessage;

    [Header("World Objects")]
    public GameObject hole;
    public GameObject player;
    public GameObject ground;
    public GameObject wall;

    private void Client_OnMessage(object sender, MessageEventArgs e)
    {
        lock (serverMessages)
        {
            serverMessages.Add(e);
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

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.keyCode == KeyCode.Return && writingMessage)
            {
                SendText();
                messagingField.ActivateInputField();
            }

            if (!worldLoaded || !moveReady || writingMessage) { return; }

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
                yield return new WaitForSeconds(0.3f);
                moveReady = true;
            }
        }
    }

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
                        world = ser.DeserializeWorld(msg.RawData);
                        worldLoaded = true;
                        BuildWorld();
                    }
                    else
                    {
                        players = ser.DeserializePlayers(msg.RawData);
                        PositionPlayers();
                    }
                }
                // if msg is string
                if (msg.IsText)
                {
                    messages.Add(msg.Data);
                }

                foreach (var message in messages)
                {
                    textMessage.text += message + Environment.NewLine;
                }
            }
            messages.Clear();
            serverMessages.Clear();
        }

        if (messagingField.isFocused == true){writingMessage = true;}
        else{writingMessage = false;}
    }

    public void JoinChatroom(string chatroomPath)
    {
        if (inputName.text.Trim() != string.Empty)
        {
            client = new WebSocket("ws://localhost:8080/" + chatroomPath);
            client.OnOpen += Client_OnOpen;
            client.OnMessage += Client_OnMessage;
            client.Connect();

            foreach (var button in inputs) { button.SetActive(false); }
            foreach (var button in messagingObj) { button.SetActive(true); }
        }

    }

    public void SendText()
    {
        if (messagingField.text.Trim() != string.Empty)
        {
            client.Send("#msg:" + messagingField.text);
            messagingField.text = string.Empty;
        }
    }

    private void PositionPlayers()
    {
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
}