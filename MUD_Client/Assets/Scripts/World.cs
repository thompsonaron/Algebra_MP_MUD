using System;

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