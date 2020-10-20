using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            server.AddWebSocketService<Chatroom>("/ChatroomB");

            server.Start();
            while (true) { }
        }
    }


    public class Chatroom : WebSocketBehavior
    {
        byte[] world;
        // 1 0 0 0 0 1
        // 0 0 0 0 1 0
        //  0 0 0 0 1

        List<Player> players;


        // first string is Uri (meaning ws://localhost:8080/ChatroomA or B or wotever
        // Dictionary<string, string> is <user.ID, user.name>
       static Dictionary<string, Dictionary<string, string>> users = new Dictionary<string, Dictionary<string, string>>();
        protected override void OnOpen()
        {
            //Console.WriteLine(Context.RequestUri);
            Send(world);
            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {

            // user.ID, user.name
            Dictionary<string, string> chatroom = null;
            // if there is chatroom with users just set it to this
            // i need this only for multiple chatrooms
            if (!users.ContainsKey(Context.RequestUri.ToString()))
            {
                users[Context.RequestUri.ToString()] = new Dictionary<string, string>();
            }
            // else create new
            chatroom = users[Context.RequestUri.ToString()];
            
            // Context is contained within WebSocketBehavior class and .RequestUri checks full Uri, e.g.:ws://localhost:8080/ChatroomA
            Console.WriteLine(Context.RequestUri);

            // MINI PROTOCOL
            // if e.Data starts with "#name:" then set name
            if (e.Data.StartsWith("#name:"))
            {
                chatroom[ID] = e.Data.Substring(6);
            }
            else if(e.Data.StartsWith("#msg:"))
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
            }
            base.OnMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Dictionary<string, string> chatroom = null;
            // check if chatroom exists
            if (!users.ContainsKey(Context.RequestUri.ToString()))
            {

                users[Context.RequestUri.ToString()] = new Dictionary<string, string>();
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

    public class User
    {
        public string ID;
        public string name;
    }

    public class Player
    {
        string ID;
        int x;
        int y;
    }
}
