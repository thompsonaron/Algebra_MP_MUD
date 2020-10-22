using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            while (true) { }
        }
    }


    public class Chatroom : WebSocketBehavior
    {
        //byte[] world;
        //// 1 0 0 0 0 1
        //// 0 0 0 0 1 0
        ////  0 0 0 0 1

        static World wrld = new World();
        
        Serializator s = new Serializator();

        static List<Player> players = new List<Player>();


        // first string is Uri (meaning ws://localhost:8080/ChatroomA or B or wotever
        // Dictionary<string, string> is <user.ID, user.name>
        //static Dictionary<string, Dictionary<string, string>> users = new Dictionary<string, Dictionary<string, string>>();
        static Dictionary<string, Dictionary<string, Player>> users = new Dictionary<string, Dictionary<string, Player>>();
        protected override void OnOpen()
        {
            //Console.WriteLine(Context.RequestUri);
        Console.WriteLine(wrld.ground[19]);

            byte[] vs = new byte[] { 1 };
            
            // TODO
            Send(s.serialize(wrld));
           // Send(vs);
            Console.WriteLine("Sent world");

            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {

            // user.ID, user.name
            Dictionary<string, Player> chatroom = null;
            // if there is chatroom with users just set it to this
            // i need this only for multiple chatrooms
            if (!users.ContainsKey(Context.RequestUri.ToString()))
            {
                users[Context.RequestUri.ToString()] = new Dictionary<string, Player>();
            }
            // else create new
            chatroom = users[Context.RequestUri.ToString()];

            // Context is contained within WebSocketBehavior class and .RequestUri checks full Uri, e.g.:ws://localhost:8080/ChatroomA
            Console.WriteLine(Context.RequestUri);

            // MINI PROTOCOL
            // if e.Data starts with "#name:" then set name
            if (e.Data.StartsWith("#name:"))
            {
                Player p = new Player();
                p.nick = e.Data.Substring(6);
                p.ID = ID;
                //void PositionPlayer(string iD)
                //{
                //    var random = new Random();
                //    bool searchingFreeSpot = true;
                //    while (searchingFreeSpot)
                //    {
                //        int rndPos = random.Next(10, 89);
                //        if (world.ground[rndPos] == 1)
                //        {
                //            searchingFreeSpot = false;
                //            world.ground[rndPos] = 2;
                //            p.position = rndPos;
                //        }
                //    }
                //}
                //PositionPlayer(ID);

                chatroom[ID] = p;
                players.Add(p);

                //Sessions.SendTo(players, ID);
                Sessions.Broadcast(s.serialize(wrld));

            }
            else if (e.Data.StartsWith("#msg:"))
            {
                // else 
                // if e.Data starts with "#msg:" then send message to everybody

                var msg = e.Data.Substring(5);
                Console.WriteLine(msg);
                // sending message to all clients
                foreach (var user in chatroom)
                {
                    // user.Key is ID
                    Sessions.SendTo(msg, user.Key);
                };
            }
            else if (e.Data.StartsWith("#plyMove:"))
            {
                string move = e.Data.Substring(9);
                char W = move[0];
                char A = move[1];
                char S = move[2];
                char D = move[3];

                if (W == '1')
                {
                    // he moved up

                }
                else if (A == '1')
                {
                    // moved left

                }
                else if (S == '1')
                {
                    // moved down

                }
                else if (D == '1')
                {
                    // moved right
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

            base.OnClose(e);
        }
    }
}
