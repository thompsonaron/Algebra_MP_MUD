using System.Collections.Generic;
using System.IO;
using System;
using MUD_Server;

public class Serializator
{
    public byte[] serialize(Player player)
    {
        var s = new MemoryStream();
        var bW = new BinaryWriter(s);
        bW.Write(player.ID);
        bW.Write(player.nick);
        bW.Write(player.position);
        return s.ToArray();
    }
    public byte[] serialize(Players players)
    {
        var s = new MemoryStream();
        var bW = new BinaryWriter(s);
        bW.Write(players.players.Count);
        foreach (var item in players.players)
        {
            bW.Write(serialize(item));
        }
        return s.ToArray();
    }
    public byte[] serialize(World world)
    {
        var s = new MemoryStream();
        var bW = new BinaryWriter(s);
        bW.Write(world.ground.Length);
        foreach (var item in world.ground)
        {
            bW.Write(item);
        }
        return s.ToArray();
    }
    public Player DeserializePlayer(byte[] b)
    {
        var s = new MemoryStream(b);
        var bR = new BinaryReader(s);
        var obj = new Player();
        obj.ID = bR.ReadString();
        obj.nick = bR.ReadString();
        obj.position = bR.ReadInt32();
        return obj;
    }
    public Players DeserializePlayers(byte[] b)
    {
        var s = new MemoryStream(b);
        var bR = new BinaryReader(s);
        var obj = new Players();
        obj.players = new List<Player>();
        int playersListSize = bR.ReadInt32();
        for (int i = 0; i < playersListSize; i++)
        {
            obj.players.Add(DeserializePlayer(ref b, ref s, ref bR));
        }
        return obj;
    }
    public World DeserializeWorld(byte[] b)
    {
        var s = new MemoryStream(b);
        var bR = new BinaryReader(s);
        var obj = new World();
        int groundArraySize = bR.ReadInt32();
        obj.ground = new Int32[groundArraySize];
        for (int i = 0; i < groundArraySize; i++)
        {
            obj.ground[i] = bR.ReadInt32();
        }
        return obj;
    }

    private Player DeserializePlayer(ref byte[] b, ref MemoryStream s, ref BinaryReader bR)
    {
        var obj = new Player();
        obj.ID = bR.ReadString();
        obj.nick = bR.ReadString();
        obj.position = bR.ReadInt32();
        return obj;
    }
    private Players DeserializePlayers(ref byte[] b, ref MemoryStream s, ref BinaryReader bR)
    {
        var obj = new Players();
        obj.players = new List<Player>();
        int playersListSize = bR.ReadInt32();
        for (int i = 0; i < playersListSize; i++)
        {
            obj.players.Add(DeserializePlayer(ref b, ref s, ref bR));
        }
        return obj;
    }
    private World DeserializeWorld(ref byte[] b, ref MemoryStream s, ref BinaryReader bR)
    {
        var obj = new World();
        int groundArraySize = bR.ReadInt32();
        obj.ground = new Int32[groundArraySize];
        for (int i = 0; i < groundArraySize; i++)
        {
            obj.ground[i] = bR.ReadInt32();
        }
        return obj;
    }

}
