using System;
using System.Linq;
using UnityEngine;

public static class LevelDefinitions
{
    public const float GridWorldY = 9.6f;
    public const float TilemapLocalY = 14.5f;
    public const float TileWorldYOffset = GridWorldY + TilemapLocalY;
    public const int GroundCellY = 3;

    public struct MovingSpikeDef
    {
        public Vector2 position;
        public Vector2 pointA;
        public Vector2 pointB;
        public float speed;
    }

    public struct LevelDef
    {
        public string name;
        public string[] map;
        public int originX;
        public Vector2 player;
        public Vector2 finish;
        public Vector2[] stars;
        public Vector2? pushable;
        public Vector2[] staticSpikes;
        public MovingSpikeDef[] movingSpikes;
    }

    public static LevelDef Get(int levelNumber)
    {
        return levelNumber switch
        {
            3 => Level3(),
            4 => Level4(),
            5 => Level5(),
            6 => Level6(),
            _ => throw new ArgumentOutOfRangeException(nameof(levelNumber))
        };
    }

    static string[] Pad(int width, params string[] rows)
    {
        return rows.Select(row =>
        {
            if (row.Length >= width)
                return row.Substring(0, width);
            return row.PadRight(width, ' ');
        }).ToArray();
    }

    // Level 3 baseline (reference for progression)
    static LevelDef Level3()
    {
        const int w = 84;
        return new LevelDef
        {
            name = "Rolling Hills",
            originX = 0,
            map = Pad(w,
                "                                                                                    ",
                "                          *                                                         ",
                "                    ##############                                                  ",
                "              ######################                                                ",
                "        ######################                                                      ",
                "  ######################                                                            ",
                "##################              ##################              ##################    ",
                "################    ########    ########    ########    ########################    ",
                "##############          ##              ##              ##              *           ",
                "##############    ########    ########    ########    ########    ########         ",
                "####################################################################################"
            ),
            player = new Vector2(4f, 28f),
            finish = new Vector2(76f, 28f),
            stars = new[]
            {
                new Vector2(12f, 36f),
                new Vector2(42f, 40f),
                new Vector2(68f, 44f),
            },
            pushable = new Vector2(28f, 36f),
            staticSpikes = Array.Empty<Vector2>(),
            movingSpikes = new[]
            {
                new MovingSpikeDef
                {
                    position = new Vector2(34f, 28f),
                    pointA = new Vector2(32f, 28f),
                    pointB = new Vector2(40f, 28f),
                    speed = 8f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(52f, 28f),
                    pointA = new Vector2(50f, 28f),
                    pointB = new Vector2(58f, 28f),
                    speed = 9f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(62f, 32f),
                    pointA = new Vector2(60f, 32f),
                    pointB = new Vector2(66f, 32f),
                    speed = 10f,
                },
            },
        };
    }

    // Level 4: L2-style horizontal run + L3 platform hops, wider pits
    static LevelDef Level4()
    {
        const int w = 130;
        return new LevelDef
        {
            name = "Spike Alley",
            originX = 0,
            map = Pad(w,
                "                                                                                                                                  ",
                "                                                              *                                                                   ",
                "                                                        ##################                                                        ",
                "                                                  ##########################                                                      ",
                "                                            ##########################                                                            ",
                "                                      ##########################                                                                  ",
                "                                ##########################                                                                        ",
                "                          ##########################                                                                              ",
                "                    ##########################        ##########################                                                  ",
                "              ##########################                      ##########################                                        ",
                "        ##########################                                      ##########################                              ",
                "  ##########################                                                  ##########################                        ",
                "##################    ##########    ##########    ##########    ##########    ##########    ##########    ##########              ",
                "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##          ",
                "##########          ##          ##          ##          ##          ##          ##          ##          ##          *            ",
                "##########    ########    ########    ########    ########    ########    ########    ########    ########    ########            ",
                "#######          ##########          ##########          ##########          ##########          ##########          ##########  ",
                "##################################################################################################################################"
            ),
            player = new Vector2(4f, 28f),
            finish = new Vector2(122f, 28f),
            stars = new[]
            {
                new Vector2(22f, 36f),
                new Vector2(58f, 44f),
                new Vector2(108f, 40f),
            },
            pushable = new Vector2(44f, 36f),
            staticSpikes = new[]
            {
                new Vector2(18f, 28f),
                new Vector2(72f, 28f),
                new Vector2(98f, 28f),
            },
            movingSpikes = new[]
            {
                new MovingSpikeDef
                {
                    position = new Vector2(32f, 28f),
                    pointA = new Vector2(30f, 28f),
                    pointB = new Vector2(38f, 28f),
                    speed = 10f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(52f, 28f),
                    pointA = new Vector2(48f, 32f),
                    pointB = new Vector2(56f, 32f),
                    speed = 11f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(82f, 28f),
                    pointA = new Vector2(78f, 28f),
                    pointB = new Vector2(86f, 28f),
                    speed = 12f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(105f, 28f),
                    pointA = new Vector2(102f, 28f),
                    pointB = new Vector2(110f, 28f),
                    speed = 11f,
                },
            },
        };
    }

    // Level 5: L1-style high pushable star + L2 length, tighter vertical puzzle
    static LevelDef Level5()
    {
        const int w = 165;
        return new LevelDef
        {
            name = "Heavy Lifting",
            originX = 0,
            map = Pad(w,
                "                                                                                                                                                                     ",
                "                                                                                *                                                                                    ",
                "                                                                          ##################                                                                       ",
                "                                                                    ##########################                                                                     ",
                "                                                              ##########################                                                                           ",
                "                                                        ##########################                                                                                 ",
                "                                                  ##########################                                                                                       ",
                "                                            ##########################                                                                                             ",
                "                                      ##########################                                                                                                   ",
                "                                ##########################                                                                                                         ",
                "                          ##########################        ##########################                                                                               ",
                "                    ##########################                      ##########################                                                                     ",
                "              ##########################                                    ##########################                                                               ",
                "        ##########################                                                ##########################                                                     ",
                "  ##########################                                                            ##########################                                               ",
                "##################              ##################              ##################              ##################              ##################                ",
                "################    ########    ########    ########    ########    ########    ########    ########    ########    ########    ################                ",
                "##############          ##              ##              ##              ##              ##              ##              ##              *                       ",
                "##############    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########                         ",
                "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##                ",
                "########          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##                    ",
                "######          ##########          ##########          ##########          ##########          ##########          ##########          ##########              ",
                "#################################################################################################################################################################"
            ),
            player = new Vector2(4f, 28f),
            finish = new Vector2(156f, 28f),
            stars = new[]
            {
                new Vector2(28f, 38f),
                new Vector2(72f, 50f),
                new Vector2(130f, 42f),
            },
            pushable = new Vector2(58f, 40f),
            staticSpikes = new[]
            {
                new Vector2(24f, 28f),
                new Vector2(48f, 28f),
                new Vector2(88f, 28f),
                new Vector2(115f, 28f),
            },
            movingSpikes = new[]
            {
                new MovingSpikeDef
                {
                    position = new Vector2(38f, 28f),
                    pointA = new Vector2(35f, 28f),
                    pointB = new Vector2(43f, 28f),
                    speed = 11f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(62f, 28f),
                    pointA = new Vector2(58f, 32f),
                    pointB = new Vector2(66f, 32f),
                    speed = 12f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(95f, 28f),
                    pointA = new Vector2(92f, 28f),
                    pointB = new Vector2(100f, 28f),
                    speed = 12f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(118f, 28f),
                    pointA = new Vector2(114f, 28f),
                    pointB = new Vector2(122f, 28f),
                    speed = 13f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(140f, 32f),
                    pointA = new Vector2(136f, 32f),
                    pointB = new Vector2(144f, 32f),
                    speed = 12f,
                },
            },
        };
    }

    // Level 6: L2-scale finale (~200 units), all mechanics combined
    static LevelDef Level6()
    {
        const int w = 205;
        return new LevelDef
        {
            name = "Grand Finale",
            originX = 0,
            map = Pad(w,
                "                                                                                                                                                                                                             ",
                "                                                      *                                                     *                                                     *                                      ",
                "                                                ##################                                   ##################                                   ##################                            ",
                "                                          ##########################                             ##########################                             ##########################                      ",
                "                                    ##########################                           ##########################                           ##########################                                ",
                "                              ##########################                         ##########################                         ##########################                                          ",
                "                        ##########################                       ##########################                       ##########################                                                    ",
                "                  ##########################                     ##########################                     ##########################                                                              ",
                "            ##########################                   ##########################                   ##########################                                                                    ",
                "      ##########################                 ##########################                 ##########################                                                                                  ",
                "##########################             ##########################             ##########################             ##########################                                                    ",
                "################    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########              ",
                "##############          ##              ##              ##              ##              ##              ##              ##              ##              ##              ##              ##            ",
                "##############    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########                  ",
                "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##            ",
                "########          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##        ",
                "######    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########            ",
                "####          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ",
                "#####################################################################################################################################################################################################"
            ),
            player = new Vector2(4f, 28f),
            finish = new Vector2(196f, 28f),
            stars = new[]
            {
                new Vector2(32f, 44f),
                new Vector2(98f, 52f),
                new Vector2(168f, 46f),
            },
            pushable = new Vector2(62f, 40f),
            staticSpikes = new[]
            {
                new Vector2(20f, 28f),
                new Vector2(45f, 28f),
                new Vector2(78f, 28f),
                new Vector2(112f, 28f),
                new Vector2(148f, 28f),
                new Vector2(178f, 28f),
            },
            movingSpikes = new[]
            {
                new MovingSpikeDef
                {
                    position = new Vector2(30f, 28f),
                    pointA = new Vector2(27f, 28f),
                    pointB = new Vector2(35f, 28f),
                    speed = 12f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(55f, 28f),
                    pointA = new Vector2(52f, 32f),
                    pointB = new Vector2(60f, 32f),
                    speed = 13f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(85f, 28f),
                    pointA = new Vector2(82f, 28f),
                    pointB = new Vector2(90f, 28f),
                    speed = 13f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(105f, 28f),
                    pointA = new Vector2(102f, 28f),
                    pointB = new Vector2(110f, 28f),
                    speed = 14f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(130f, 32f),
                    pointA = new Vector2(126f, 32f),
                    pointB = new Vector2(136f, 32f),
                    speed = 13f,
                },
                new MovingSpikeDef
                {
                    position = new Vector2(160f, 28f),
                    pointA = new Vector2(156f, 28f),
                    pointB = new Vector2(166f, 28f),
                    speed = 14f,
                },
            },
        };
    }

    public static Vector3Int CellFromMap(int originX, int row, int col, int totalRows)
    {
        int cellY = GroundCellY + (totalRows - 1 - row);
        return new Vector3Int(originX + col, cellY, 0);
    }

    public static Vector3 WorldFromCell(Vector3Int cell)
    {
        return new Vector3(cell.x + 0.5f, TileWorldYOffset + cell.y + 0.5f, 0f);
    }
}
