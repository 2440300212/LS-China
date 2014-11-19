using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace KurisuRiven
{
    internal class KurisuLib
    {      
        private static bool active;
        public static bool canexport = true;

        private const int minrange = 100;
        private const int rotatemultiplier = 15;
        private static Vector3 startpoint, endpoint, directionpoint;

        private static readonly Spell wings = new Spell(SpellSlot.Q, 280f);
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        public KurisuLib()
        {

            if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.SummonersRift))
                Coordinates();
            if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.Unknown))
                NSRCoordinates();

            Game.OnGameUpdate += Game_OnGameUpdate;           
        }

        public static readonly List<Coordinate> jumpList = new List<Coordinate>();

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (KurisuRiven.config.Item("exportjump").GetValue<KeyBind>().Active && canexport)
            {
                using (var file = new System.IO.StreamWriter(@"C:\Rivenjumps.txt"))
                {
                   file.WriteLine("jumpList.Add(new Coordinate(new Vector3(" + player.Position.X + "f," + player.Position.Y + "f," + player.Position.Z + "f),");
                   file.WriteLine("new Vector3(" + Game.CursorPos.X + "f," + Game.CursorPos.Y + "f," + Game.CursorPos.Z + "f)));");
                   file.Close();
                }

                Game.PrintChat("Debug: Position exported!");
                canexport = false;
            }
            else
            {
                canexport = true;
            }

            if (!active && KurisuRiven.config.Item("jumpkey").GetValue<KeyBind>().Active && KurisuRiven.cleavecount == 2)
            {
                var playeradjacentpos = (float) (minrange + 1);
                for (int i = 0; i < jumpList.Count; i++)
                {
                    Coordinate pos = jumpList[i];
                    if (player.Distance(pos.pointA) < playeradjacentpos || player.Distance(pos.pointB) < playeradjacentpos)
                    {
                        active = true;
                        if (player.Distance(pos.pointA) < player.Distance(pos.pointB))
                        {
                            playeradjacentpos = player.Distance(pos.pointA);
                            startpoint = pos.pointA;
                            endpoint = pos.pointB;
                        }
                        else
                        {
                            playeradjacentpos = player.Distance(pos.pointB);
                            startpoint = pos.pointB;
                            endpoint = pos.pointA;
                        }
                    }
                }
                if (active)
                {
                    directionpoint.X = startpoint.X - endpoint.X;
                    directionpoint.Y = startpoint.Y - endpoint.Y;

                    player.IssueOrder(GameObjectOrder.HoldPosition, player.ServerPosition);
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startpoint.X, startpoint.Y)).Send();
                    Utility.DelayAction.Add(Game.Ping + 70, Direction1);
                }
            }         

        }


        private void Direction1()
        {
            player.IssueOrder(GameObjectOrder.HoldPosition, player.ServerPosition);
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startpoint.X + directionpoint.X/rotatemultiplier,
                startpoint.Y + directionpoint.Y/rotatemultiplier)).Send();

            Utility.DelayAction.Add(Game.Ping + 17, Direction2);
        }

        private void Direction2()
        {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startpoint.X, startpoint.Y)).Send();
            Utility.DelayAction.Add(Game.Ping + 23, Walljump);
        }

        public static void Walljump()
        {
            wings.Cast(endpoint, true);
            player.IssueOrder(GameObjectOrder.HoldPosition, player.ServerPosition);
            Utility.DelayAction.Add(1000, () => active = false);
        }


        // new coordinates from new map by me :)
        private static void NSRCoordinates()
        {
            jumpList.Add(new Coordinate(new Vector3(6696f, 8774f, -71.2406f),
            new Vector3(6933.048f, 8941.319f, 52.87134f)));
            jumpList.Add(new Coordinate(new Vector3(6698f, 8984f, 49.94371f),
            new Vector3(6462.293f, 8778.244f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(6830f, 8564f, -71.2406f),
            new Vector3(7180.054f, 8743.792f, 52.87256f)));
            jumpList.Add(new Coordinate(new Vector3(7096f, 8366f, -70.89347f),
            new Vector3(7354.568f, 8580.59f, 52.87244f)));
            jumpList.Add(new Coordinate(new Vector3(7634f, 6178f, 52.4513f),
            new Vector3(8115.281f, 6314.993f, -71.21191f)));
            jumpList.Add(new Coordinate(new Vector3(7542f, 6226f, 52.4513f),
            new Vector3(7800.893f, 6505.175f, -34.67603f)));
            jumpList.Add(new Coordinate(new Vector3(5096f, 11744f, 56.80341f),
            new Vector3(5224.156f, 12203.83f, 56.47681f)));
            jumpList.Add(new Coordinate(new Vector3(4776f, 11816f, 56.67248f),
            new Vector3(4843.92f, 12275.33f, 56.47705f)));
            jumpList.Add(new Coordinate(new Vector3(5498f, 11778f, 56.8484f),
            new Vector3(5627.228f, 12230.11f, 55.75854f)));
            jumpList.Add(new Coordinate(new Vector3(6351.196f, 11996.04f, 56.4768f),
            new Vector3(6265.39f, 11543.65f, 56.6145f)));
            jumpList.Add(new Coordinate(new Vector3(6374f, 11678f, 55.30333f),
            new Vector3(6343.67f, 12142.45f, 56.47705f)));
            jumpList.Add(new Coordinate(new Vector3(6598f, 11996f, 56.4768f),
            new Vector3(6689.555f, 11571.94f, 53.82983f)));
            jumpList.Add(new Coordinate(new Vector3(6652f, 11778f, 53.82969f),
            new Vector3(6593.762f, 12196.2f, 56.47681f)));
            jumpList.Add(new Coordinate(new Vector3(5574f, 12106f, 56.45419f),
            new Vector3(5548.412f, 11644.23f, 56.84839f)));
            jumpList.Add(new Coordinate(new Vector3(6696f, 11334f, 53.76143f),
            new Vector3(7009.122f, 11047.42f, 56.05029f)));
            jumpList.Add(new Coordinate(new Vector3(6974f, 10178f, 53.17089f),
            new Vector3(6948.026f, 10633.8f, 55.99829f)));
            jumpList.Add(new Coordinate(new Vector3(8210f, 3562f, 52.44194f),
            new Vector3(7961.107f, 3854.945f, 53.72083f)));
            jumpList.Add(new Coordinate(new Vector3(7974f, 3834f, 53.72089f),
            new Vector3(8194.845f, 3528.548f, 52.14685f)));
            jumpList.Add(new Coordinate(new Vector3(7572f, 8956f, 52.8726f),
            new Vector3(7790.026f, 9295.767f, 52.45325f)));
            jumpList.Add(new Coordinate(new Vector3(7372f, 9106f, 52.8726f),
            new Vector3(7708.071f, 9405.216f, 52.39282f)));
            jumpList.Add(new Coordinate(new Vector3(7018f, 10584f, 55.99712f),
            new Vector3(7070.215f, 10143.04f, 52.79089f)));
            jumpList.Add(new Coordinate(new Vector3(6224f, 12778f, 54.0266f),
            new Vector3(6175.879f, 13218.46f, 52.83826f)));
            jumpList.Add(new Coordinate(new Vector3(6359.42f, 13198.41f, 52.8381f),
            new Vector3(6519.4f, 12739.75f, 55.48572f)));
            jumpList.Add(new Coordinate(new Vector3(6982f, 11122f, 55.99969f),
            new Vector3(6572.649f, 11412.77f, 55.05566f)));
            jumpList.Add(new Coordinate(new Vector3(7750f, 10706f, 50.76583f),
            new Vector3(7365.367f, 10769.14f, 56.38928f)));
            jumpList.Add(new Coordinate(new Vector3(7394f, 10750f, 56.1977f),
            new Vector3(7761.647f, 10581.55f, 50.73694f)));
            jumpList.Add(new Coordinate(new Vector3(8194f, 11064f, 50.62045f),
            new Vector3(8604.129f, 11278.68f, 51.7439f)));
            jumpList.Add(new Coordinate(new Vector3(8574.423f, 11208.04f, 51.4786f),
            new Vector3(8130.391f, 11013.55f, 50.59875f)));
            jumpList.Add(new Coordinate(new Vector3(8094f, 9642f, 52.07279f),
            new Vector3(8479.859f, 9675.006f, 50.38391f)));
            jumpList.Add(new Coordinate(new Vector3(8375.989f, 9806.928f, 50.38285f),
            new Vector3(7974.133f, 9827.299f, 51.17188f)));
            jumpList.Add(new Coordinate(new Vector3(8646f, 9584f, 50.38403f),
            new Vector3(8654.872f, 9225.769f, 53.09827f)));
            jumpList.Add(new Coordinate(new Vector3(8670f, 9272f, 52.77951f),
            new Vector3(8490.986f, 9651.499f, 50.38428f)));
            jumpList.Add(new Coordinate(new Vector3(9400f, 12550f, 52.41484f),
            new Vector3(8931.289f, 12490.32f, 56.47681f)));
            jumpList.Add(new Coordinate(new Vector3(8998f, 12612f, 56.4768f),
            new Vector3(9476.425f, 12634.55f, 52.43921f)));
            jumpList.Add(new Coordinate(new Vector3(7122f, 12812f, 56.4768f),
            new Vector3(7200.856f, 13268.59f, 52.83826f)));
            jumpList.Add(new Coordinate(new Vector3(9772f, 12674f, 52.30659f),
            new Vector3(10198.09f, 12691.25f, 91.42993f)));
            jumpList.Add(new Coordinate(new Vector3(10140f, 12720f, 91.42984f),
            new Vector3(9682.941f, 12717.68f, 52.36975f)));
            jumpList.Add(new Coordinate(new Vector3(9844f, 12312f, 52.3063f),
            new Vector3(10244.92f, 12435.43f, 91.42981f)));
            jumpList.Add(new Coordinate(new Vector3(10104f, 12390f, 91.42984f),
            new Vector3(9662.812f, 12270.61f, 52.30627f)));
            jumpList.Add(new Coordinate(new Vector3(9946f, 11968f, 52.3063f),
            new Vector3(10319.01f, 12046.75f, 91.43005f)));
            jumpList.Add(new Coordinate(new Vector3(10200f, 11994f, 91.42978f),
            new Vector3(9760.642f, 11839.77f, 52.30627f)));
            jumpList.Add(new Coordinate(new Vector3(9996f, 11672f, 52.3063f),
            new Vector3(10351.38f, 11851.09f, 91.42957f)));
            jumpList.Add(new Coordinate(new Vector3(10322.26f, 11656.51f, 91.42979f),
            new Vector3(9941.854f, 11452.55f, 52.3064f)));
            jumpList.Add(new Coordinate(new Vector3(10890f, 12376f, 91.42981f),
            new Vector3(10894.18f, 12809.14f, 91.43018f)));
            jumpList.Add(new Coordinate(new Vector3(10888f, 12684f, 91.42981f),
            new Vector3(10887.33f, 12221.07f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(12294f, 10990f, 91.42981f),
            new Vector3(12645.71f, 10780.09f, 91.42993f)));
            jumpList.Add(new Coordinate(new Vector3(12476f, 10806.76f, 91.42981f),
            new Vector3(12294.53f, 11196.42f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(11494f, 10484f, 91.4298f),
            new Vector3(11384.13f, 10058.49f, 52.30688f)));
            jumpList.Add(new Coordinate(new Vector3(11428f, 10182f, 52.3063f),
            new Vector3(11524.18f, 10633.09f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(11865.53f, 10351f, 91.42981f),
            new Vector3(11722.06f, 9893.707f, 52.3064f)));
            jumpList.Add(new Coordinate(new Vector3(11776f, 10078f, 52.3063f),
            new Vector3(11908.76f, 10501.42f, 91.42944f)));
            jumpList.Add(new Coordinate(new Vector3(12230f, 10236f, 91.42981f),
            new Vector3(12192.78f, 9794.665f, 52.3064f)));
            jumpList.Add(new Coordinate(new Vector3(12126f, 9928f, 52.3063f),
            new Vector3(12222.6f, 10359.59f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(12568f, 10234f, 91.42981f),
            new Vector3(12515.01f, 9799.781f, 52.30615f)));
            jumpList.Add(new Coordinate(new Vector3(12524f, 9928f, 52.3063f),
            new Vector3(12565.06f, 10381.16f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(11268f, 7484f, 52.20285f),
            new Vector3(11212.31f, 7042.612f, 51.72363f)));
            jumpList.Add(new Coordinate(new Vector3(11422f, 7208f, 51.72648f),
            new Vector3(11520.11f, 7675.308f, 52.21631f)));
            jumpList.Add(new Coordinate(new Vector3(11552f, 7488f, 52.20296f),
            new Vector3(11446.06f, 7050.877f, 51.72339f)));
            jumpList.Add(new Coordinate(new Vector3(10726f, 7170f, 51.7226f),
            new Vector3(10299.64f, 7346.098f, 51.88916f)));
            jumpList.Add(new Coordinate(new Vector3(10394f, 7348f, 51.79094f),
            new Vector3(10804.31f, 7112.198f, 51.72241f)));
            jumpList.Add(new Coordinate(new Vector3(11256f, 11290f, 91.42986f),
            new Vector3(10945.58f, 10998.41f, 91.42981f)));
            jumpList.Add(new Coordinate(new Vector3(11044f, 11114f, 92.07098f),
            new Vector3(11341.25f, 11371.78f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(12941.35f, 10193.76f, 91.42978f),
            new Vector3(12858.71f, 9749.06f, 52.26074f)));
            jumpList.Add(new Coordinate(new Vector3(12822f, 9878f, 48.18407f),
            new Vector3(12855.1f, 10319.91f, 91.42993f)));
            jumpList.Add(new Coordinate(new Vector3(13636f, 10420f, 91.42979f),
            new Vector3(13621.92f, 10859.09f, 91.42993f)));
            jumpList.Add(new Coordinate(new Vector3(13668.66f, 10712.31f, 91.42981f),
            new Vector3(13624.57f, 10254.54f, 91.42969f)));
            jumpList.Add(new Coordinate(new Vector3(10610f, 13710f, 94.01597f),
            new Vector3(10211.83f, 13685.04f, 98.49829f)));
            jumpList.Add(new Coordinate(new Vector3(10344f, 13652f, 96.73087f),
            new Vector3(10689.69f, 13672.11f, 91.54248f)));
            jumpList.Add(new Coordinate(new Vector3(8873.54f, 10412.11f, 50.52374f),
            new Vector3(9075.934f, 9979.541f, 48.40625f)));
            jumpList.Add(new Coordinate(new Vector3(8194f, 10702f, 49.78528f),
            new Vector3(8674.658f, 10671.49f, 50.52466f)));
            jumpList.Add(new Coordinate(new Vector3(8550f, 10652f, 50.52413f),
            new Vector3(8074.135f, 10599.5f, 49.72729f)));
            jumpList.Add(new Coordinate(new Vector3(11776f, 8858f, 50.30732f),
            new Vector3(11408.74f, 8556.481f, 60.1958f)));
            jumpList.Add(new Coordinate(new Vector3(11744f, 8520f, 57.36847f),
            new Vector3(12054.6f, 8837.893f, 50.4707f)));
            jumpList.Add(new Coordinate(new Vector3(11744f, 8310f, 55.47791f),
            new Vector3(12135.24f, 8399.932f, 52.31201f)));
            jumpList.Add(new Coordinate(new Vector3(12076.03f, 8258.561f, 52.3117f),
            new Vector3(11633.12f, 8114.488f, 53.89453f)));
            jumpList.Add(new Coordinate(new Vector3(11024f, 8134f, 62.62095f),
            new Vector3(11155.47f, 7710.521f, 52.20581f)));
            jumpList.Add(new Coordinate(new Vector3(11100f, 7776f, 52.20348f),
            new Vector3(11015.81f, 8214.057f, 62.448f)));
            jumpList.Add(new Coordinate(new Vector3(10776f, 8408f, 63.09288f),
            new Vector3(10311.45f, 8501.852f, 63.427f)));
            jumpList.Add(new Coordinate(new Vector3(10484f, 8584f, 64.92075f),
            new Vector3(10891.32f, 8359.844f, 62.68604f)));
            jumpList.Add(new Coordinate(new Vector3(10918f, 7494f, 52.20335f),
            new Vector3(10936.67f, 7034.37f, 51.72266f)));
            jumpList.Add(new Coordinate(new Vector3(10972f, 7208f, 51.72375f),
            new Vector3(11016.62f, 7641.816f, 52.20361f)));
            jumpList.Add(new Coordinate(new Vector3(5874f, 9456f, -71.2406f),
            new Vector3(5947.653f, 9891.044f, 53.1123f)));
            jumpList.Add(new Coordinate(new Vector3(5922f, 9884f, 52.9804f),
            new Vector3(5828.5f, 9479.167f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(4032f, 2230f, 95.74808f),
            new Vector3(4105.018f, 2666.675f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(4948f, 8386f, 25.79336f),
            new Vector3(4933.921f, 7908.062f, 52.02856f)));
            jumpList.Add(new Coordinate(new Vector3(3864f, 7420f, 51.61989f),
            new Vector3(3830.952f, 7881.322f, 51.96851f)));
            jumpList.Add(new Coordinate(new Vector3(3598f, 7416f, 51.89053f),
            new Vector3(3642.549f, 7868.615f, 53.93628f)));
            jumpList.Add(new Coordinate(new Vector3(4146f, 7736f, 50.63115f),
            new Vector3(4528.448f, 7545.06f, 51.10376f)));
            jumpList.Add(new Coordinate(new Vector3(3296f, 1140f, 95.74805f),
            new Vector3(3666.198f, 1427.618f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(4148f, 1260f, 95.74805f),
            new Vector3(4478.822f, 1263.775f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(4024f, 2534f, 95.74808f),
            new Vector3(3994.615f, 2128.424f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(3098f, 3078f, 93.37599f),
            new Vector3(3414.418f, 3408.074f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(2084f, 4664f, 95.74805f),
            new Vector3(2091.404f, 5181.122f, 53.39844f)));
            jumpList.Add(new Coordinate(new Vector3(3302f, 3404f, 95.74803f),
            new Vector3(3143.135f, 3002.351f, 95.67896f)));
            jumpList.Add(new Coordinate(new Vector3(3546f, 3582f, 95.74805f),
            new Vector3(3849.985f, 3909.208f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(3758f, 3788f, 95.74806f),
            new Vector3(3461.857f, 3466.945f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(3652f, 1240f, 94.38851f),
            new Vector3(3213.636f, 1229.118f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(2508f, 3932f, 95.74786f),
            new Vector3(2264.065f, 4246.885f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(2364f, 4144f, 95.74799f),
            new Vector3(2575.738f, 3820.933f, 95.7478f)));
            jumpList.Add(new Coordinate(new Vector3(2334f, 4724f, 95.74805f),
            new Vector3(2421.255f, 5186.035f, 52.89868f)));
            jumpList.Add(new Coordinate(new Vector3(2624f, 4658f, 95.74805f),
            new Vector3(2675.792f, 5112.661f, 53.17065f)));
            jumpList.Add(new Coordinate(new Vector3(2904f, 4630f, 95.74805f),
            new Vector3(3029.563f, 5032.221f, 53.52295f)));
            jumpList.Add(new Coordinate(new Vector3(3140f, 4524f, 95.74805f),
            new Vector3(3344.618f, 4941.21f, 54.14868f)));
            jumpList.Add(new Coordinate(new Vector3(3274f, 4834f, 54.14972f),
            new Vector3(3171.109f, 4389.878f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(2960f, 4892f, 53.73007f),
            new Vector3(2843.646f, 4462.927f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(2660f, 5010f, 53.07819f),
            new Vector3(2619.296f, 4545.331f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(2374f, 5058f, 52.56479f),
            new Vector3(2331.146f, 4545.331f, 95.74854f)));
            jumpList.Add(new Coordinate(new Vector3(2074f, 5034f, 52.54222f),
            new Vector3(2064.493f, 4532.074f, 95.74854f)));
            jumpList.Add(new Coordinate(new Vector3(4742f, 2048f, 95.74805f),
            new Vector3(5134.207f, 2065.609f, 52.13721f)));
            jumpList.Add(new Coordinate(new Vector3(4678f, 2192f, 95.74805f),
            new Vector3(5126.953f, 2271.042f, 51.52222f)));
            jumpList.Add(new Coordinate(new Vector3(5000f, 2188f, 51.87373f),
            new Vector3(4493.176f, 2145.012f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(5050f, 2002f, 52.27983f),
            new Vector3(4623.25f, 1819.071f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(5000f, 2552f, 51.26714f),
            new Vector3(4535.013f, 2430.725f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(4696f, 2472f, 95.74805f),
            new Vector3(5150.116f, 2643.843f, 51.25903f)));
            jumpList.Add(new Coordinate(new Vector3(4952f, 2898f, 51.08503f),
            new Vector3(4488.702f, 2719.693f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(4698f, 2772f, 95.74805f),
            new Vector3(5119.878f, 2965.672f, 51.09546f)));
            jumpList.Add(new Coordinate(new Vector3(1174f, 3380f, 95.68596f),
            new Vector3(1151.922f, 3890.267f, 95.74829f)));
            jumpList.Add(new Coordinate(new Vector3(1178f, 3794f, 95.66292f),
            new Vector3(1167.029f, 3327.453f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(1164f, 4128f, 95.74802f),
            new Vector3(1155.062f, 4555.145f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(1174f, 4434f, 95.74805f),
            new Vector3(1162.369f, 3998.082f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(3012f, 6054f, 57.04691f),
            new Vector3(3339.819f, 6344.271f, 52.29932f)));
            jumpList.Add(new Coordinate(new Vector3(3160f, 6340f, 51.89334f),
            new Vector3(2844.186f, 5981.917f, 57.04443f)));
            jumpList.Add(new Coordinate(new Vector3(2798f, 6658f, 57.01933f),
            new Vector3(3249.252f, 6631.022f, 51.69019f)));
            jumpList.Add(new Coordinate(new Vector3(3052f, 6598f, 51.30767f),
            new Vector3(2611.247f, 6490.441f, 57.01782f)));
            jumpList.Add(new Coordinate(new Vector3(3592f, 7692f, 51.80717f),
            new Vector3(3515.772f, 7248.599f, 51.77148f)));
            jumpList.Add(new Coordinate(new Vector3(3840f, 7684f, 51.87151f),
            new Vector3(3840.06f, 7292.919f, 51.09668f)));
            jumpList.Add(new Coordinate(new Vector3(3682f, 7034f, 49.89319f),
            new Vector3(3727.482f, 6578.841f, 52.46167f)));
            jumpList.Add(new Coordinate(new Vector3(3642f, 6724f, 52.45851f),
            new Vector3(3636.869f, 7112.767f, 50.98193f)));
            jumpList.Add(new Coordinate(new Vector3(3344f, 7428f, 51.89142f),
            new Vector3(3330.87f, 7847.645f, 52.22314f)));
            jumpList.Add(new Coordinate(new Vector3(3324f, 7684f, 51.46717f),
            new Vector3(3279.267f, 7264.259f, 51.89063f)));
            jumpList.Add(new Coordinate(new Vector3(4462f, 7526f, 50.83388f),
            new Vector3(4041.504f, 7715.907f, 51.75513f)));
            jumpList.Add(new Coordinate(new Vector3(2244f, 7776f, 50.41083f),
            new Vector3(2533.404f, 8146.746f, 51.82056f)));
            jumpList.Add(new Coordinate(new Vector3(2516f, 8084f, 51.82605f),
            new Vector3(2246.906f, 7663.188f, 50.40967f)));
            jumpList.Add(new Coordinate(new Vector3(1896f, 7974f, 51.16259f),
            new Vector3(2168.052f, 8336.56f, 51.7771f)));
            jumpList.Add(new Coordinate(new Vector3(2256f, 8184f, 51.78577f),
            new Vector3(1958.272f, 7851.851f, 50.64185f)));
            jumpList.Add(new Coordinate(new Vector3(1698f, 8554f, 52.8381f),
            new Vector3(2136.605f, 8631.22f, 51.77734f)));
            jumpList.Add(new Coordinate(new Vector3(6324f, 3558f, 49.66662f),
            new Vector3(6719.724f, 3853.052f, 48.59741f)));
            jumpList.Add(new Coordinate(new Vector3(6654f, 3842f, 48.52352f),
            new Vector3(6249.627f, 3601.047f, 49.82275f)));
            jumpList.Add(new Coordinate(new Vector3(6274f, 4308f, 48.87802f),
            new Vector3(6708.84f, 4205.648f, 48.52539f)));
            jumpList.Add(new Coordinate(new Vector3(6650f, 4286f, 48.52583f),
            new Vector3(6210.893f, 4448.303f, 48.53638f)));
            jumpList.Add(new Coordinate(new Vector3(5958f, 4632f, 48.53367f),
            new Vector3(5638.711f, 4969.978f, 48.81689f)));
            jumpList.Add(new Coordinate(new Vector3(5748f, 4942f, 48.31442f),
            new Vector3(6085.081f, 4624.43f, 48.53369f)));
            jumpList.Add(new Coordinate(new Vector3(6180f, 5326f, 48.52729f),
            new Vector3(6042.292f, 5622.549f, 51.7804f)));
            jumpList.Add(new Coordinate(new Vector3(6130f, 5584f, 51.7751f),
            new Vector3(6215.104f, 5177.999f, 48.52795f)));
            jumpList.Add(new Coordinate(new Vector3(6444f, 5176f, 48.52723f),
            new Vector3(6892.5f, 5190.152f, 48.52698f)));
            jumpList.Add(new Coordinate(new Vector3(6752f, 5102f, 48.527f),
            new Vector3(6281.794f, 5080.849f, 48.5282f)));
            jumpList.Add(new Coordinate(new Vector3(7198f, 5542f, 55.9738f),
            new Vector3(7521.103f, 5873.835f, 52.59497f)));
            jumpList.Add(new Coordinate(new Vector3(7324f, 5908f, 52.50191f),
            new Vector3(7108.314f, 5540.416f, 56.42932f)));
            jumpList.Add(new Coordinate(new Vector3(8568f, 3236f, 54.356f),
            new Vector3(8527.594f, 2740.154f, 50.6095f)));
            jumpList.Add(new Coordinate(new Vector3(8254f, 2880f, 51.13f),
            new Vector3(8156.763f, 3340.199f, 51.56714f)));
            jumpList.Add(new Coordinate(new Vector3(8476f, 2888f, 51.13f),
            new Vector3(8493.787f, 3355.073f, 54.08716f)));
            jumpList.Add(new Coordinate(new Vector3(9318f, 3136f, 55.41505f),
            new Vector3(9241.075f, 2710.113f, 49.22266f)));
            jumpList.Add(new Coordinate(new Vector3(9424f, 2832f, 49.22298f),
            new Vector3(9472.017f, 3274.394f, 54.85278f)));
            jumpList.Add(new Coordinate(new Vector3(10204f, 2184f, 49.2229f),
            new Vector3(10182.22f, 1718.163f, 50.06506f)));
            jumpList.Add(new Coordinate(new Vector3(9772f, 3036f, 63.28237f),
            new Vector3(9691.301f, 2617.71f, 49.2229f)));
            jumpList.Add(new Coordinate(new Vector3(9924f, 2732f, 49.22295f),
            new Vector3(9915.359f, 3167.894f, 55.28052f)));
            jumpList.Add(new Coordinate(new Vector3(9004f, 1988f, 64.91981f),
            new Vector3(8720.87f, 1611.468f, 49.45776f)));
            jumpList.Add(new Coordinate(new Vector3(8792f, 1732f, 49.4511f),
            new Vector3(9017.521f, 2151.304f, 54.97693f)));
            jumpList.Add(new Coordinate(new Vector3(8522f, 1730f, 49.4503f),
            new Vector3(8498.881f, 2160.272f, 51.12988f)));
            jumpList.Add(new Coordinate(new Vector3(8364f, 2036f, 51.13f),
            new Vector3(8331.094f, 1575.509f, 49.45837f)));
            jumpList.Add(new Coordinate(new Vector3(7574f, 1732f, 49.44834f),
            new Vector3(7661.29f, 2154.325f, 51.15857f)));
            jumpList.Add(new Coordinate(new Vector3(7672f, 2108f, 51.1551f),
            new Vector3(7637.703f, 1644.864f, 49.44763f)));
            jumpList.Add(new Coordinate(new Vector3(8822f, 5058f, 51.75765f),
            new Vector3(8871.964f, 5619.657f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(8928f, 5434f, -71.2406f),
            new Vector3(8770.28f, 4940.507f, 51.90063f)));
            jumpList.Add(new Coordinate(new Vector3(9090f, 4758f, 51.56561f),
            new Vector3(9508.204f, 4649.149f, -71.24023f)));
            jumpList.Add(new Coordinate(new Vector3(9370f, 4682f, -71.2406f),
            new Vector3(8946.168f, 4803.055f, 51.78931f)));
            jumpList.Add(new Coordinate(new Vector3(9044f, 4406f, 52.701f),
            new Vector3(9466.732f, 4418.132f, -71.23999f)));
            jumpList.Add(new Coordinate(new Vector3(9302f, 4456f, -71.2406f),
            new Vector3(8844.863f, 4401.378f, 53.06079f)));
            jumpList.Add(new Coordinate(new Vector3(9486f, 4134f, -68.90685f),
            new Vector3(9167.848f, 3772.066f, 55.40771f)));
            jumpList.Add(new Coordinate(new Vector3(9154f, 3916f, 54.35764f),
            new Vector3(9588.933f, 4202.336f, -71.24023f)));
            jumpList.Add(new Coordinate(new Vector3(9704f, 3886f, -71.2406f),
            new Vector3(9524.099f, 3456.417f, 60.53613f)));
            jumpList.Add(new Coordinate(new Vector3(9522f, 3558f, 64.14748f),
            new Vector3(9767.617f, 4060.979f, -71.24097f)));
            jumpList.Add(new Coordinate(new Vector3(9912f, 3834f, -71.2406f),
            new Vector3(9675.586f, 3375.487f, 52.74512f)));
            jumpList.Add(new Coordinate(new Vector3(10650f, 4506f, -72.7732f),
            new Vector3(10260.96f, 4422.892f, -71.24072f)));
            jumpList.Add(new Coordinate(new Vector3(10488f, 4356f, -71.2406f),
            new Vector3(10924.26f, 4368.185f, -71.22656f)));
            jumpList.Add(new Coordinate(new Vector3(9724f, 5184f, -72.09716f),
            new Vector3(9741.12f, 4738.446f, -71.24097f)));
            jumpList.Add(new Coordinate(new Vector3(9766f, 4928f, -71.2406f),
            new Vector3(9731.139f, 5331.521f, -68.29907f)));
            jumpList.Add(new Coordinate(new Vector3(7976f, 5930f, 50.13454f),
            new Vector3(7850.892f, 6385.171f, -60.94141f)));
            jumpList.Add(new Coordinate(new Vector3(8002f, 6168f, -71.67461f),
            new Vector3(7561.733f, 6066.61f, 52.45313f)));
            jumpList.Add(new Coordinate(new Vector3(8078f, 6138f, -71.2406f),
            new Vector3(7976.015f, 5761.078f, 51.7417f)));
            jumpList.Add(new Coordinate(new Vector3(7372f, 6382f, 52.4513f),
            new Vector3(7682.28f, 6651.342f, 46.63013f)));
            jumpList.Add(new Coordinate(new Vector3(7944f, 5966f, 50.25438f),
            new Vector3(8413.663f, 6142.276f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(4578f, 5848f, 51.72738f),
            new Vector3(4661.317f, 5510.839f, 50.23816f)));
            jumpList.Add(new Coordinate(new Vector3(4712f, 5630f, 50.2312f),
            new Vector3(4441.707f, 5890.851f, 52.42834f)));
            jumpList.Add(new Coordinate(new Vector3(4042f, 6420f, 52.46645f),
            new Vector3(4407.121f, 6251.123f, 51.33948f)));
            jumpList.Add(new Coordinate(new Vector3(4810f, 3288f, 50.86842f),
            new Vector3(4346.738f, 3092.491f, 95.74805f)));
            jumpList.Add(new Coordinate(new Vector3(4524f, 3258f, 95.74808f),
            new Vector3(4858.832f, 3542.613f, 50.75488f)));
            jumpList.Add(new Coordinate(new Vector3(4336f, 6278f, 51.34194f),
            new Vector3(3927.237f, 6465.769f, 52.46436f)));
            jumpList.Add(new Coordinate(new Vector3(5850f, 2462f, 52.13899f),
            new Vector3(5379.39f, 2412.754f, 51.24487f)));
            jumpList.Add(new Coordinate(new Vector3(5444f, 2326f, 51.1855f),
            new Vector3(5894.803f, 2372.421f, 52.1394f)));
            jumpList.Add(new Coordinate(new Vector3(8162f, 3136f, 51.5508f),
            new Vector3(8279.901f, 2703.895f, 51.12988f)));
            jumpList.Add(new Coordinate(new Vector3(2002f, 8654f, 51.77731f),
            new Vector3(1546.777f, 8653.186f, 52.83862f)));
            jumpList.Add(new Coordinate(new Vector3(2870f, 10322f, 54.3255f),
            new Vector3(3129.455f, 10764.55f, -67.69873f)));
            jumpList.Add(new Coordinate(new Vector3(3106f, 10584f, -70.85432f),
            new Vector3(2784.672f, 10134.64f, 54.32544f)));
            jumpList.Add(new Coordinate(new Vector3(3304f, 10136f, -64.47968f),
            new Vector3(2940.083f, 9778.102f, 52.79517f)));
            jumpList.Add(new Coordinate(new Vector3(3128f, 9976f, 53.15556f),
            new Vector3(3452.468f, 10388.89f, -66.53906f)));
            jumpList.Add(new Coordinate(new Vector3(3246f, 9614f, 50.8065f),
            new Vector3(3711.912f, 9774.602f, -68.27612f)));
            jumpList.Add(new Coordinate(new Vector3(3500f, 9744f, -59.57598f),
            new Vector3(3043.061f, 9559.309f, 51.10278f)));
            jumpList.Add(new Coordinate(new Vector3(4112f, 8022f, 50.71408f),
            new Vector3(4482.876f, 8252.238f, 48.91431f)));
            jumpList.Add(new Coordinate(new Vector3(4416.438f, 8110.586f, 48.79285f),
            new Vector3(3986.606f, 7906.659f, 51.28076f)));
            jumpList.Add(new Coordinate(new Vector3(2596f, 9328f, 51.77343f),
            new Vector3(2984.94f, 9366.622f, 50.72021f)));
            jumpList.Add(new Coordinate(new Vector3(2802f, 9392f, 50.85761f),
            new Vector3(2379.708f, 9141.052f, 51.77612f)));
            jumpList.Add(new Coordinate(new Vector3(2202f, 9216f, 51.77626f),
            new Vector3(1791.216f, 9519.231f, 52.83813f)));
            jumpList.Add(new Coordinate(new Vector3(1844f, 9384f, 52.8381f),
            new Vector3(2188.243f, 9065.282f, 51.77637f)));
            jumpList.Add(new Coordinate(new Vector3(10418f, 1782f, 49.35614f),
            new Vector3(10381.67f, 2239.321f, 49.2229f)));
            jumpList.Add(new Coordinate(new Vector3(5488f, 10350f, -71.18363f),
            new Vector3(5886.312f, 10259.44f, 54.63257f)));
            jumpList.Add(new Coordinate(new Vector3(5800f, 10358f, 54.52341f),
            new Vector3(5425.606f, 10435.02f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(5756f, 10678f, 55.75924f),
            new Vector3(5349.564f, 10593.63f, -71.24072f)));
            jumpList.Add(new Coordinate(new Vector3(5496f, 10640f, -71.2406f),
            new Vector3(5803.17f, 10843.28f, 55.97473f)));
            jumpList.Add(new Coordinate(new Vector3(5348f, 10822f, -71.2406f),
            new Vector3(5631.257f, 11058.52f, 56.8324f)));
            jumpList.Add(new Coordinate(new Vector3(5578f, 11046f, 56.8512f),
            new Vector3(5270.925f, 10779.2f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(5134f, 10978f, -71.23817f),
            new Vector3(5300.562f, 11291.07f, 56.82715f)));
            jumpList.Add(new Coordinate(new Vector3(10700f, 6948f, 51.7226f),
            new Vector3(10270.1f, 6769.453f, 51.99146f)));
            jumpList.Add(new Coordinate(new Vector3(10394f, 6822f, 51.97513f),
            new Vector3(10832.54f, 6996.207f, 51.72241f)));
            jumpList.Add(new Coordinate(new Vector3(12794f, 6256f, 51.66834f),
            new Vector3(13234.44f, 6234.645f, 55.27124f)));
            jumpList.Add(new Coordinate(new Vector3(13178.31f, 6107.714f, 55.41211f),
            new Vector3(12688.38f, 6129.937f, 54.74561f)));
            jumpList.Add(new Coordinate(new Vector3(12734f, 6580f, 51.66419f),
            new Vector3(13014.84f, 6909.749f, 52.10498f)));
            jumpList.Add(new Coordinate(new Vector3(12925.02f, 6908.355f, 51.85595f),
            new Vector3(12586.91f, 6516.82f, 51.72314f)));
            jumpList.Add(new Coordinate(new Vector3(11344f, 5304f, -56.59019f),
            new Vector3(11707.61f, 5211.494f, 52.29712f)));
            jumpList.Add(new Coordinate(new Vector3(12744f, 5910f, 51.59283f),
            new Vector3(13180.69f, 5833.892f, 55.12866f)));
            jumpList.Add(new Coordinate(new Vector3(11744f, 4644f, -71.2406f),
            new Vector3(12140.57f, 4757.023f, 51.72949f)));
            jumpList.Add(new Coordinate(new Vector3(11978.53f, 4708.429f, 51.7294f),
            new Vector3(11508.46f, 4636.023f, -71.24048f)));
            jumpList.Add(new Coordinate(new Vector3(11994f, 5696f, 52.02821f),
            new Vector3(12447.71f, 5707.316f, 53.01465f)));
            jumpList.Add(new Coordinate(new Vector3(12300f, 5820f, 58.94732f),
            new Vector3(11874.41f, 5715.601f, 51.16064f)));
            jumpList.Add(new Coordinate(new Vector3(11570f, 5224f, 51.75803f),
            new Vector3(11224.61f, 5177.344f, -70.22119f)));
            jumpList.Add(new Coordinate(new Vector3(12050f, 4540f, 51.86011f),
            new Vector3(11763.1f, 4227.405f, -71.24072f)));
            jumpList.Add(new Coordinate(new Vector3(11894f, 4348f, -71.2406f),
            new Vector3(12161.24f, 4637.81f, 51.72925f)));

        }
        private static void Coordinates()
        {

            jumpList.Add(new Coordinate(new Vector3(7873f,
                8735f,
                55.44274f), new Vector3(7545f, 9063f, 55.6065f)));
            jumpList.Add(new Coordinate(new Vector3(6393.7299804688f,
                -63.87451171875f,
                8341.7451171875f), new Vector3
                    (6612.1625976563f,
                        56.018413543701f,
                        8574.7412109375f
                    )
                ));

            jumpList.Add(new Coordinate(new Vector3(7041.7885742188f,
                8810.1787109375f,
                0f), new Vector3
                    (7296.0341796875f,
                        9056.4638671875f,
                        55.610824584961f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4546.0258789063f,
                2548.966796875f,
                54.257415771484f), new Vector3
                    (4185.0786132813f,
                        2526.5520019531f,
                        109.35539245605f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2805.4074707031f,
                6140.130859375f,
                55.182941436768f), new Vector3
                    (2614.3215332031f,
                        5816.9438476563f,
                        60.193073272705f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6696.486328125f,
                5377.4013671875f,
                61.310482025146f), new Vector3
                    (6868.6918945313f,
                        5698.1455078125f,
                        55.616455078125f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1677.9854736328f,
                8319.9345703125f,
                54.923847198486f), new Vector3
                    (1270.2786865234f,
                        8286.544921875f,
                        50.334892272949f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2809.3254394531f,
                10178.6328125f,
                -58.759708404541f), new Vector3
                    (2553.8962402344f,
                        9974.4677734375f,
                        53.364395141602f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5102.642578125f,
                10322.375976563f,
                -62.845260620117f), new Vector3
                    (5483f,
                        10427,
                        54.5009765625f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6000.2373046875f,
                11763.544921875f,
                39.544124603271f), new Vector3
                    (6056.666015625f,
                        11388.752929688f,
                        54.385917663574f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1742.34375f,
                7647.1557617188f,
                53.561042785645f), new Vector3
                    (1884.5321044922f,
                        7995.1459960938f,
                        54.930736541748f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3319.087890625f,
                7472.4760742188f,
                55.027889251709f), new Vector3
                    (3388.0522460938f,
                        7101.2568359375f,
                        54.486026763916f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3989.9423828125f,
                7929.3422851563f,
                51.94282913208f), new Vector3
                    (3671.623046875f,
                        7723.146484375f,
                        53.906265258789f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4936.8452148438f,
                10547.737304688f,
                -63.064865112305f), new Vector3
                    (5156.7397460938f,
                        10853.216796875f,
                        52.951190948486f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5028.1235351563f,
                10115.602539063f,
                -63.082695007324f), new Vector3
                    (5423f,
                        10127,
                        55.15357208252f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6035.4819335938f,
                10973.666015625f,
                53.918266296387f), new Vector3
                    (6385.4013671875f,
                        10827.455078125f,
                        54.63500213623f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4747.0625f,
                11866.421875f,
                41.584358215332f), new Vector3
                    (4743.23046875f,
                        11505.842773438f,
                        51.196254730225f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6749.4487304688f,
                12980.83984375f,
                44.903495788574f), new Vector3
                    (6701.4965820313f,
                        12610.278320313f,
                        52.563804626465f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3114.1865234375f,
                9420.5078125f,
                -42.718975067139f), new Vector3
                    (2757f,
                        9255,
                        53.77322769165f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2786.8354492188f,
                9547.8935546875f,
                53.645294189453f), new Vector3
                    (3002.0930175781f,
                        9854.39453125f,
                        -53.198081970215f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3803.9470214844f,
                7197.9018554688f,
                53.730079650879f), new Vector3
                    (3664.1088867188f,
                        7543.572265625f,
                        54.18229675293f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2340.0886230469f,
                6387.072265625f,
                60.165466308594f), new Vector3
                    (2695.6096191406f,
                        6374.0634765625f,
                        54.339839935303f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3249.791015625f,
                6446.986328125f,
                55.605854034424f), new Vector3
                    (3157.4558105469f,
                        6791.4458007813f,
                        54.080295562744f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3823.6242675781f,
                5923.9130859375f,
                55.420352935791f), new Vector3
                    (3584.2561035156f,
                        6215.4931640625f,
                        55.6123046875f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5796.4809570313f,
                5060.4116210938f,
                51.673671722412f), new Vector3
                    (5730.3081054688f,
                        5430.1635742188f,
                        54.921173095703f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6007.3481445313f,
                4985.3803710938f,
                51.673641204834f), new Vector3
                    (6388.783203125f,
                        4987,
                        51.673400878906f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7040.9892578125f,
                3964.6728515625f,
                57.192108154297f), new Vector3
                    (6668.0073242188f,
                        3993.609375f,
                        51.671356201172f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7763.541015625f,
                3294.3481445313f,
                54.872283935547f), new Vector3
                    (7629.421875f,
                        3648.0581054688f,
                        56.908012390137f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4705.830078125f,
                9440.6572265625f,
                -62.586814880371f), new Vector3
                    (4779.9809570313f,
                        9809.9091796875f,
                        -63.09009552002f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4056.7907714844f,
                10216.12109375f,
                -63.152275085449f), new Vector3
                    (3680.1550292969f,
                        10182.296875f,
                        -63.701038360596f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4470.0883789063f,
                12000.479492188f,
                41.59789276123f), new Vector3
                    (4232.9799804688f,
                        11706.015625f,
                        49.295585632324f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5415.5708007813f,
                12640.216796875f,
                40.682685852051f), new Vector3
                    (5564.4409179688f,
                        12985.860351563f,
                        41.373748779297f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6053.779296875f,
                12567.381835938f,
                40.587882995605f), new Vector3
                    (6045.4555664063f,
                        12942.313476563f,
                        41.211364746094f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4454.66015625f,
                8057.1313476563f,
                42.799690246582f), new Vector3
                    (4577.8681640625f,
                        7699.3686523438f,
                        53.31339263916f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7754.7700195313f,
                10449.736328125f,
                52.890430450439f), new Vector3
                    (8096.2885742188f,
                        10288.80078125f,
                        53.66955947876f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7625.3139648438f,
                9465.7001953125f,
                55.008113861084f), new Vector3
                    (7995.986328125f,
                        9398.1982421875f,
                        53.530490875244f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9767f,
                8839f,
                53.044532775879f), new Vector3
                    (9653.1220703125f,
                        9174.7626953125f,
                        53.697280883789f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10775.653320313f,
                7612.6943359375f,
                55.35241317749f), new Vector3
                    (10665.490234375f,
                        7956.310546875f,
                        65.222145080566f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10398.484375f,
                8257.8642578125f,
                66.200691223145f), new Vector3
                    (10176.104492188f,
                        8544.984375f,
                        64.849853515625f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11198.071289063f,
                8440.4638671875f,
                67.641044616699f), new Vector3
                    (11531.436523438f,
                        8611.0087890625f,
                        53.454048156738f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11686.700195313f,
                8055.9624023438f,
                55.458232879639f), new Vector3
                    (11314.19140625f,
                        8005.4946289063f,
                        58.438243865967f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10707.119140625f,
                7335.1752929688f,
                55.350387573242f), new Vector3
                    (10693f,
                        6943,
                        54.870254516602f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10395.380859375f,
                6938.5009765625f,
                54.869094848633f), new Vector3
                    (10454.955078125f,
                        7316.7041015625f,
                        55.308219909668f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10358.5859375f,
                6677.1704101563f,
                54.86909866333f), new Vector3
                    (10070.067382813f,
                        6434.0815429688f,
                        55.294486999512f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11161.98828125f,
                5070.447265625f,
                53.730766296387f), new Vector3
                    (10783f,
                        4965,
                        -63.57177734375f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11167.081054688f,
                4613.9829101563f,
                -62.898971557617f), new Vector3
                    (11501f,
                        4823,
                        54.571090698242f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11743.823242188f,
                4387.4672851563f,
                52.005855560303f), new Vector3
                    (11379f,
                        4239,
                        -61.565242767334f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10388.120117188f,
                4267.1796875f,
                -63.61775970459f), new Vector3
                    (10033.036132813f,
                        4147.1669921875f,
                        -60.332069396973f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8964.7607421875f,
                4214.3833007813f,
                -63.284225463867f), new Vector3
                    (8569f,
                        4241,
                        55.544258117676f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5554.8657226563f,
                4346.75390625f,
                51.680099487305f), new Vector3
                    (5414.0634765625f,
                        4695.6860351563f,
                        51.611679077148f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7311.3393554688f,
                10553.6015625f,
                54.153884887695f), new Vector3
                    (6938.5209960938f,
                        10535.8515625f,
                        54.441242218018f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7669.353515625f,
                5960.5717773438f,
                -64.488967895508f), new Vector3
                    (7441.2182617188f,
                        5761.8989257813f,
                        54.347793579102f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7949.65625f,
                2647.0490722656f,
                54.276401519775f), new Vector3
                    (7863.0063476563f,
                        3013.7814941406f,
                        55.178623199463f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8698.263671875f,
                3783.1169433594f,
                57.178703308105f), new Vector3
                    (9041f,
                        3975,
                        -63.242683410645f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9063f,
                3401f,
                68.192077636719f), new Vector3
                    (9275.0751953125f,
                        3712.8935546875f,
                        -63.257461547852f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12064.340820313f,
                6424.11328125f,
                54.830627441406f), new Vector3
                    (12267.9375f,
                        6742.9453125f,
                        54.83561706543f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12797.838867188f,
                5814.9653320313f,
                58.281986236572f), new Vector3
                    (12422.740234375f,
                        5860.931640625f,
                        54.815074920654f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11913.165039063f,
                5373.34375f,
                54.050819396973f), new Vector3
                    (11569.1953125f,
                        5211.7143554688f,
                        57.787326812744f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9237.3603515625f,
                2522.8937988281f,
                67.796775817871f), new Vector3
                    (9344.2041015625f,
                        2884.958984375f,
                        65.500213623047f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7324.2783203125f,
                1461.2199707031f,
                52.594970703125f), new Vector3
                    (7357.3852539063f,
                        1837.4309082031f,
                        54.282878875732f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6478.0454101563f,
                8342.501953125f,
                -64.045028686523f), new Vector3
                    (6751f,
                        8633,
                        56.019004821777f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6447f,
                8663f,
                56.018882751465f), new Vector3
                    (6413f,
                        8289,
                        62.786361694336f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6195.8334960938f,
                8559.810546875f,
                -65.304061889648f), new Vector3
                    (6327f,
                        56.517200469971f,
                        8913
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7095f,
                8763f,
                56.018997192383f), new Vector3
                    (7337f,
                        55.616943359375f,
                        9047
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7269f,
                9055f,
                55.611968994141f), new Vector3
                    (7027f,
                        8767,
                        56.018997192383f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5407f,
                10095f,
                55.045528411865f), new Vector3
                    (5033f,
                        10119,
                        63.082427978516f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5047f,
                10113f,
                -63.08129119873f), new Vector3
                    (5423f,
                        10109,
                        55.007797241211f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4747f,
                9463f,
                -62.445854187012f), new Vector3
                    (4743f,
                        9837,
                        -63.093593597412f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4769f,
                9677f,
                -63.086654663086f), new Vector3
                    (4775f,
                        9301,
                        -63.474864959717f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6731f,
                8089f,
                -64.655540466309f), new Vector3
                    (7095f,
                        8171,
                        56.051624298096f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7629.0434570313f,
                9462.6982421875f,
                55.042400360107f), new Vector3
                    (8019f,
                        9467,
                        53.530429840088f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7994.2685546875f,
                9477.142578125f,
                53.530174255371f), new Vector3
                    (7601f,
                        9441,
                        55.379856109619f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6147f,
                11063f,
                54.117427825928f), new Vector3
                    (6421f,
                        10805,
                        54.63500213623f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5952.1977539063f,
                11382.287109375f,
                54.240119934082f), new Vector3
                    (5889f,
                        11773,
                        39.546829223633f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6003.1401367188f,
                11827.516601563f,
                39.562377929688f), new Vector3
                    (6239f,
                        11479,
                        54.632926940918f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3947f,
                8013f,
                51.929698944092f), new Vector3
                    (3647f,
                        7789,
                        54.027297973633f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1597f,
                8463f,
                54.923656463623f), new Vector3
                    (1223f,
                        8455,
                        50.640468597412f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1247f,
                8413f,
                50.737510681152f), new Vector3
                    (1623f,
                        8387,
                        54.923782348633f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2440.49609375f,
                10038.1796875f,
                53.364398956299f), new Vector3
                    (2827f,
                        10205,
                        -64.97053527832f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2797f,
                10213f,
                -65.165946960449f), new Vector3
                    (2457f,
                        10055,
                        53.364398956299f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2797f,
                9563f,
                53.640556335449f), new Vector3
                    (3167f,
                        9625,
                        -63.810096740723f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3121.9699707031f,
                9574.16015625f,
                -63.448329925537f), new Vector3
                    (2755f,
                        9409,
                        53.722351074219f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3447f,
                7463f,
                55.021110534668f), new Vector3
                    (3581f,
                        7113,
                        54.248985290527f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3527f,
                7151f,
                54.452239990234f), new Vector3
                    (3372.861328125f,
                        7507.2211914063f,
                        55.13143157959f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2789f,
                6085f,
                55.241321563721f), new Vector3
                    (2445f,
                        5941,
                        60.189605712891f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2573f,
                5915f,
                60.192783355713f), new Vector3
                    (2911f,
                        6081,
                        55.503971099854f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3005f,
                5797f,
                55.631782531738f), new Vector3
                    (2715f,
                        5561,
                        60.190528869629f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2697f,
                5615f,
                60.190807342529f), new Vector3
                    (2943f,
                        5901,
                        55.629695892334f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3894.1960449219f,
                7192.3720703125f,
                53.4684715271f), new Vector3
                    (3641f,
                        7495,
                        54.714691162109f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3397f,
                6515f,
                55.605663299561f), new Vector3
                    (3363f,
                        6889,
                        53.412925720215f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3347f,
                6865f,
                53.312397003174f), new Vector3
                    (3343f,
                        6491,
                        55.605716705322f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3705f,
                7829f,
                53.67945098877f), new Vector3
                    (4009f,
                        8049,
                        51.996047973633f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7581f,
                5983f,
                -65.361351013184f), new Vector3
                    (7417f,
                        5647,
                        54.716590881348f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7495f,
                5753f,
                53.744125366211f), new Vector3
                    (7731f,
                        6045,
                        -64.48851776123f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7345f,
                6165f,
                -52.344753265381f), new Vector3
                    (7249f,
                        5803,
                        55.641929626465f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7665.0073242188f,
                5645.7431640625f,
                54.999004364014f), new Vector3
                    (7997f,
                        5861,
                        -62.778995513916f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7995f,
                5715f,
                -61.163398742676f), new Vector3
                    (7709f,
                        5473,
                        56.321662902832f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8653f,
                4441f,
                55.073780059814f), new Vector3
                    (9027f,
                        4425,
                        -61.594711303711f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8931f,
                4375f,
                -62.612571716309f), new Vector3
                    (8557f,
                        4401,
                        55.506855010986f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8645f,
                4115f,
                55.960289001465f), new Vector3
                    (9005f,
                        4215,
                        -63.280235290527f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8948.08203125f,
                4116.5078125f,
                -63.252712249756f), new Vector3
                    (8605f,
                        3953,
                        56.22159576416f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9345f,
                2815f,
                67.37971496582f), new Vector3
                    (9375f,
                        2443,
                        67.509948730469f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9355f,
                2537f,
                67.649841308594f), new Vector3
                    (9293f,
                        2909,
                        63.953853607178f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8027f,
                3029f,
                56.071315765381f), new Vector3
                    (8071f,
                        2657,
                        54.276405334473f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7995.0229492188f,
                2664.0703125f,
                54.276401519775f), new Vector3
                    (7985f,
                        3041,
                        55.659393310547f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5785f,
                5445f,
                54.918552398682f), new Vector3
                    (5899f,
                        5089,
                        51.673694610596f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5847f,
                5065f,
                51.673683166504f), new Vector3
                    (5683f,
                        5403,
                        54.923862457275f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6047f,
                4865f,
                51.67359161377f), new Vector3
                    (6409f,
                        4765,
                        51.673400878906f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6347f,
                4765f,
                51.673400878906f), new Vector3
                    (5983f,
                        4851,
                        51.673580169678f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6995f,
                5615f,
                55.738128662109f), new Vector3
                    (6701f,
                        5383,
                        61.461639404297f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6697f,
                5369f,
                61.083110809326f), new Vector3
                    (6889f,
                        5693,
                        55.628131866455f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11245f,
                4515f,
                52.104347229004f), new Vector3
                    (11585f,
                        4671,
                        52.104347229004f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11491.91015625f,
                4629.763671875f,
                52.506042480469f), new Vector3
                    (11143f,
                        4493,
                        -63.063579559326f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11395f,
                4315f,
                -62.597496032715f), new Vector3
                    (11579f,
                        4643,
                        51.962089538574f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11245f,
                4915f,
                53.017200469971f), new Vector3
                    (10869f,
                        4907,
                        -63.132637023926f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10923.66015625f,
                4853.9931640625f,
                -63.288948059082f), new Vector3
                    (11295f,
                        4913,
                        53.402942657471f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10595f,
                6965f,
                54.870422363281f), new Vector3
                    (10351f,
                        7249,
                        55.198459625244f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10415f,
                7277f,
                55.269580841064f), new Vector3
                    (10609f,
                        6957,
                        54.870502471924f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12395f,
                6115f,
                54.809947967529f), new Vector3
                    (12759f,
                        6201,
                        57.640727996826f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12745f,
                6265f,
                57.225738525391f), new Vector3
                    (12413f,
                        6089,
                        54.803039550781f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12645f,
                4615f,
                53.343021392822f), new Vector3
                    (12349f,
                        4849,
                        56.222766876221f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12395f,
                4765f,
                52.525123596191f), new Vector3
                    (12681f,
                        4525,
                        53.853294372559f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11918.497070313f,
                5471f,
                57.399909973145f), new Vector3
                    (11535f,
                        5471,
                        54.801097869873f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11593f,
                5501f,
                54.610706329346f), new Vector3
                    (11967f,
                        5477,
                        56.541202545166f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11140.984375f,
                8432.9384765625f,
                65.858421325684f), new Vector3
                    (11487f,
                        8625,
                        53.453464508057f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11420.7578125f,
                8608.6923828125f,
                53.453437805176f), new Vector3
                    (11107f,
                        8403,
                        65.090522766113f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11352.48046875f,
                8007.10546875f,
                57.916156768799f), new Vector3
                    (11701f,
                        8165,
                        55.458843231201f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11631f,
                8133f,
                55.45885848999f), new Vector3
                    (11287f,
                        7979,
                        58.037368774414f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10545f,
                7913f,
                65.745803833008f), new Vector3
                    (55.338600158691f,
                        10555f,
                        7537
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10795f,
                7613f,
                55.354972839355f), new Vector3
                    (10547f,
                        7893,
                        65.771072387695f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10729f,
                7307f,
                55.352409362793f), new Vector3
                    (10785f,
                        6937,
                        54.87170791626f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10745f,
                6965f,
                54.871494293213f), new Vector3
                    (10647f,
                        7327,
                        55.350120544434f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10099f,
                8443f,
                66.309921264648f), new Vector3
                    (10419f,
                        8249,
                        66.106910705566f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9203f,
                3309f,
                63.777507781982f), new Vector3
                    (9359f,
                        3651,
                        -63.260040283203f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9327f,
                3675f,
                -63.258842468262f), new Vector3
                    (9185f,
                        3329,
                        65.192367553711f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10045f,
                6465f,
                55.140678405762f), new Vector3
                    (10353f,
                        6679,
                        54.869094848633f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(10441f,
                8315.2333984375f,
                65.793014526367f), new Vector3
                    (10133f,
                        8529,
                        64.52165222168f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8323f,
                9137f,
                54.89501953125f), new Vector3
                    (8207f,
                        9493,
                        53.530456542969f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8295f,
                9363f,
                53.530418395996f), new Vector3
                    (8359f,
                        8993,
                        54.895038604736f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8495f,
                9763f,
                52.768348693848f), new Vector3
                    (8401f,
                        10125,
                        53.643203735352f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8419f,
                9997f,
                53.59920501709f), new Vector3
                    (8695f,
                        9743,
                        51.417175292969f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7145f,
                5965f,
                55.597702026367f), new Vector3
                    (7413f,
                        6229,
                        -66.513969421387f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6947f,
                8213f,
                56.01900100708f), new Vector3
                    (6621f,
                        8029,
                        -62.816535949707f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6397f,
                10813f,
                54.634998321533f), new Vector3
                    (6121f,
                        11065,
                        54.092365264893f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6247f,
                11513f,
                54.6325340271f), new Vector3
                    (6053f,
                        11833,
                        39.563938140869f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4627f,
                11897f,
                41.618049621582f), new Vector3
                    (4541f,
                        11531,
                        51.561706542969f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5179f,
                10839f,
                53.036727905273f), new Vector3
                    (4881f,
                        10611,
                        -63.11701965332f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4897f,
                10613f,
                -63.125648498535f), new Vector3
                    (5177f,
                        10863,
                        52.773872375488f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11367f,
                9751f,
                50.348838806152f), new Vector3
                    (11479f,
                        10107,
                        106.51720428467f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(11489f,
                10093f,
                106.53769683838f), new Vector3
                    (11403f,
                        9727,
                        50.349449157715f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12175f,
                9991f,
                106.80973052979f), new Vector3
                    (12143f,
                        9617,
                        50.354927062988f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(12155f,
                9623f,
                50.354919433594f), new Vector3
                    (12123f,
                        9995,
                        106.81489562988f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9397f,
                12037f,
                52.484146118164f), new Vector3
                    (9769f,
                        12077,
                        106.21959686279f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9745f,
                12063f,
                106.2202835083f), new Vector3
                    (9373f,
                        12003,
                        52.484580993652f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9345f,
                12813f,
                52.689178466797f), new Vector3
                    (9719f,
                        12805,
                        106.20919799805f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4171f,
                2839f,
                109.72004699707f), new Vector3
                    (4489f,
                        3041,
                        54.030017852783f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4473f,
                3009f,
                54.04020690918f), new Vector3
                    (4115f,
                        2901,
                        110.06342315674f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2669f,
                4281f,
                105.9382019043f), new Vector3
                    (2759f,
                        4647,
                        57.061370849609f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2761f,
                4653f,
                57.062965393066f), new Vector3
                    (2681f,
                        4287,
                        106.2310256958f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1623f,
                4487f,
                108.56233215332f), new Vector3
                    (1573f,
                        4859,
                        56.13228225708f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1573f,
                4845f,
                56.048126220703f), new Vector3
                    (1589f,
                        4471,
                        108.56234741211f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2355.4450683594f,
                6366.453125f,
                60.167724609375f), new Vector3
                    (2731f,
                        6355,
                        54.617771148682f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2669f,
                6363f,
                54.488224029541f), new Vector3
                    (2295f,
                        6371,
                        60.163955688477f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2068.5336914063f,
                8898.5322265625f,
                54.921718597412f), new Vector3
                    (2457f,
                        8967,
                        53.765918731689f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2447f,
                8913f,
                53.763805389404f), new Vector3
                    (2099f,
                        8775,
                        54.922241210938f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1589f,
                9661f,
                49.631057739258f), new Vector3
                    (1297f,
                        9895,
                        38.928337097168f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(1347f,
                9813f,
                39.538192749023f), new Vector3
                    (1609f,
                        9543,
                        50.499561309814f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3997f,
                10213f,
                -63.152000427246f), new Vector3
                    (3627f,
                        10159,
                        -64.785446166992f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(3709f,
                10171f,
                -63.07014465332f), new Vector3
                    (4085f,
                        10175,
                        -63.139434814453f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(9695f,
                12813f,
                106.20919799805f), new Vector3
                    (9353f,
                        12965,
                        95.629013061523f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5647f,
                9563f,
                55.136940002441f), new Vector3
                    (5647f,
                        9187,
                        -65.224411010742f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(5895f,
                3389f,
                52.799312591553f), new Vector3
                    (6339f,
                        3633,
                        51.669734954834f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(6225f,
                3605f,
                51.669948577881f), new Vector3
                    (5793f,
                        3389,
                        53.080261230469f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8201f,
                1893f,
                54.276405334473f), new Vector3
                    (8333f,
                        1407,
                        52.60326385498f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8185f,
                1489f,
                52.59805679321f), new Vector3
                    (8015f,
                        1923,
                        54.276405334473f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2351f,
                4743f,
                56.366249084473f), new Vector3
                    (2355f,
                        4239,
                        107.71157836914f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(2293f,
                4389f,
                109.00361633301f), new Vector3
                    (2187f,
                        4883,
                        56.207984924316f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4271f,
                2065f,
                108.56426239014f), new Vector3
                    (4775f,
                        2033,
                        54.37939453125f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(4675f,
                2013f,
                54.971534729004f), new Vector3
                    (4173f,
                        1959,
                        108.41383361816f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(7769f,
                10925f,
                53.940235137939f), new Vector3
                    (8257f,
                        11049,
                        49.935401916504f
                    )
                ));
            jumpList.Add(new Coordinate(new Vector3(8123f,
                11051f,
                49.935398101807f), new Vector3
                    (7689f,
                        10831,
                        53.834579467773f
                    )
                ));

        }
    }

    internal struct Coordinate
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public Coordinate(Vector3 p1, Vector3 p2)
        {
            pointA = p1;
            pointB = p2;
        }
    }
}