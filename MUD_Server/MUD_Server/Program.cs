using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MUD_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer server = new WebSocketServer(8080);

            server.AddWebSocketService<Chatroom>("/ChatroomA");
            // server.AddWebSocketService<Chatroom>("/ChatroomB");

            server.Start();
            Console.WriteLine("Server running");
            while (true) { }
        }
    }


    public class Chatroom : WebSocketBehavior
    {
        Serializator ser = new Serializator();
        static World wrld = new World();
        static Players players = new Players();

        // first string is Uri (meaning ws://localhost:8080/ChatroomA or B or wotever
        static Dictionary<string, Dictionary<string, Player>> users = new Dictionary<string, Dictionary<string, Player>>();
        protected override void OnOpen()
        {
            // Sending world
            Send(ser.serialize(wrld));
            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            // user.ID, user.name
            Dictionary<string, Player> chatroom = null;
            if (players.players == null)
            {
                players.players = new List<Player>();
            }
            // if there is chatroom with users just set it to this
            // i need this only for multiple chatrooms
            if (!users.ContainsKey(Context.RequestUri.ToString()))
            {
                users[Context.RequestUri.ToString()] = new Dictionary<string, Player>();
            }
            // else create new
            chatroom = users[Context.RequestUri.ToString()];

            // Context is contained within WebSocketBehavior class and .RequestUri checks full Uri, e.g.:ws://localhost:8080/ChatroomA
            // Console.WriteLine(Context.RequestUri);

            // MINI PROTOCOL
            // if e.Data starts with "#name:" then set name
            if (e.Data.StartsWith("#name:"))
            {
                Player p = new Player();
                p.nick = e.Data.Substring(6);
                p.ID = ID;
                void PositionPlayer(string iD)
                {
                    var random = new Random();
                    bool searchingFreeSpot = true;
                    while (searchingFreeSpot)
                    {
                        int rndPos = random.Next(10, wrld.ground.Length - 12);

                        // if position is on edges -> skip
                        if (rndPos % 10 == 0 || (rndPos - 9) % 10 == 0)
                        {
                            continue;
                        }

                        p.position = rndPos;
                        searchingFreeSpot = false;
                        foreach (var item in players.players)
                        {
                            if (rndPos == item.position)
                            {
                                searchingFreeSpot = true;
                            }
                        }
                    }
                }

                PositionPlayer(ID);
                chatroom[ID] = p;
                players.players.Add(p);

                Sessions.Broadcast(ser.serialize(players));
            }
            else if (e.Data.StartsWith("#msg:"))
            {
                Player tempPlayer = new Player();
                foreach (var p in players.players)
                {
                    if (ID == p.ID)
                    {
                        tempPlayer = p;
                        break;
                    }
                }
                var msg = tempPlayer.nick + ": " + e.Data.Substring(5);
                Console.WriteLine(msg);
                // sending message to all clients
                foreach (var user in chatroom)
                {
                    //user.Key is ID
                   Sessions.SendTo(msg, user.Key);
                };
            }
            else if (e.Data.StartsWith("#plyMove:"))
            {
                string move = e.Data.Substring(9);

                Player tempPlayer = new Player();
                foreach (var p in players.players)
                {
                    if (ID == p.ID)
                    {
                        tempPlayer = p;
                        break;
                    }
                }

                if (move == "W")
                {
                    if (wrld.ground[tempPlayer.position + 10] == 2)
                    {
                        return;
                    }
                    // he moved up
                    if (tempPlayer.position + 10 < wrld.ground.Length - 10)
                    {
                        tempPlayer.position += 10;
                        Sessions.Broadcast(ser.serialize(players));
                    }

                }
                else if (move == "S")
                {
                    if (wrld.ground[tempPlayer.position - 10] == 2)
                    {
                        return;
                    }
                    // he moved down
                    if (tempPlayer.position - 10 > 10)
                    {
                        tempPlayer.position -= 10;
                        Sessions.Broadcast(ser.serialize(players));
                    }
                }
                else if (move == "A")
                {
                    if (wrld.ground[tempPlayer.position - 1] == 2)
                    {
                        return;
                    }
                    // he moved left
                    if ((tempPlayer.position - 1) % 10 != 0)
                    {
                        tempPlayer.position--;
                        Sessions.Broadcast(ser.serialize(players));
                    }
                }
                else if (move == "D")
                {
                    if (wrld.ground[tempPlayer.position + 1] == 2)
                    {
                        return;
                    }
                    // he moved right
                    if ((tempPlayer.position + 1 - 9) % 10 != 0)
                    {
                        tempPlayer.position++;
                        Sessions.Broadcast(ser.serialize(players));
                    }
                }
            }
            base.OnMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Dictionary<string, Player> chatroom = null;
            // check if chatroom exists
            if (!users.ContainsKey(Context.RequestUri.ToString()))
            {
                users[Context.RequestUri.ToString()] = new Dictionary<string, Player>();
            }
            // else create new
            chatroom = users[Context.RequestUri.ToString()];
            // remove user if exists upon close
            if (chatroom.ContainsKey(ID))
            {
                chatroom.Remove(ID);
            }

            foreach (var item in players.players)
            {
                if (item.ID == ID)
                {
                    players.players.Remove(item);
                    Sessions.Broadcast(ser.serialize(players));
                    break;
                }
            }

            base.OnClose(e);
        }
    }
}
