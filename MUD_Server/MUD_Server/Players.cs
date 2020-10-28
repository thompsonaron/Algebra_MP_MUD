using System;
using System.Collections.Generic;
namespace MUD_Server
{
    [Serializable]
    public class Player
    {
        public string ID;
        public string nick;
        public int position;
    }

    [Serializable]
    public class Players
    {
        public List<Player> players = new List<Player>();
    }

    [Serializable]
    public class World
    {
        // 0 = border, 1 = free pos, 2 = hole, 3 = player
        public int[] ground = new int[]{
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 1, 1, 2, 1, 1, 1, 1, 1, 0,
        0, 1, 1, 2, 2, 1, 1, 1, 1, 0,
        0, 1, 1, 1, 1, 1, 2, 1, 1, 0,
        0, 1, 1, 1, 1, 1, 1, 1, 1, 0,
        0, 1, 1, 2, 1, 1, 1, 1, 1, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        };
    }

}
