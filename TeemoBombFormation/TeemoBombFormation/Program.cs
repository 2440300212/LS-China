using System;
using System.IO;
using System.Resources;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace TeemoBombFormation
{
    class Program
    {
        public static Menu Config;
        public static Vector2 P1, P2, P3, P4, P5;
        static Spell R = new Spell(SpellSlot.R, 300);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (P1.X != 0)
            {
                Utility.DrawCircle(P1.To3D(), 100, Color.Red);
            }
            if (P2.X != 0)
            {
                Utility.DrawCircle(P2.To3D(), 100, Color.Red);
            }
            if (P3.X != 0)
            {
                Utility.DrawCircle(P3.To3D(), 100, Color.Red);
            }
            if (P4.X != 0)
            {
                Utility.DrawCircle(P4.To3D(), 100, Color.Red);
            }
            if (P5.X != 0)
            {
                Utility.DrawCircle(P5.To3D(), 100, Color.Red);
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Teemo Bomb Formation", "BombF", true);

            Config.AddItem(
                new MenuItem("BombFormation", "Bomb Formation").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press, false)));
            Config.AddItem(
                new MenuItem("ResetFormation", "Reset Formation").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false)));
            Config.AddToMainMenu();
            Reset();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast +=Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.BaseSkinName == "Teemo" && sender.IsMe)
            {
                if (args.End.To2D() == P1)
                {
                    Reset(P1);   
                }
                else
                {                        
                    if (args.End.To2D() == P2)
                    {
                        Reset(P2);
                    }
                    else
                    {
                        if (args.End.To2D() == P3)
                        {
                            Reset(P3);
                        }
                        else
                        {
                            if (args.End.To2D() == P4)
                            {
                                Reset(P4);
                            }
                            else
                            {
                                if (args.End.To2D() == P5)
                                {
                                    Reset(P5);
                                }
                            }
                        }
                    }

                }
            }           
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("BombFormation").GetValue<KeyBind>().Active)
            {
                Pentagon();
                if (R.IsReady())
                {
                    
               
                if (P1.X != 0)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, P1.To3D());
                }
                else
                {
                    if (P2.X != 0)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, P2.To3D());
                    }
                    else
                    {
                        if (P3.X != 0)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, P3.To3D());
                        }
                        else
                        {
                            if (P4.X != 0)
                            {
                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, P4.To3D());
                            }
                            else
                            {
                                if (P5.X != 0)
                                {
                                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, P5.To3D());
                                }
                            }
                        }
                    }
                }
                }
            }
            if (Config.Item("ResetFormation").GetValue<KeyBind>().Active)
            {
                Reset();
            }
        }

        private static void Reset(Vector2 pos)
        {
            if (pos == P1)
            {
                P1.X = 0;
                P1.Y = 0;
            }
            if (pos == P2)
            {
                P2.X = 0;
                P2.Y = 0;
            }
            if (pos == P3)
            {
                P3.X = 0;
                P3.Y = 0;
            }
            if (pos == P4)
            {
                P4.X = 0;
                P4.Y = 0;
            }
            if (pos == P5)
            {
                P5.X = 0;
                P5.Y = 0;
            }
        }

        private static void Reset()
        {
                P1.X = 0;
                P1.Y = 0;
                P2.X = 0;
                P2.Y = 0;
                P3.X = 0;
                P3.Y = 0;
                P4.X = 0;
                P4.Y = 0;
                P5.X = 0;
                P5.Y = 0;
        }
        private static void Pentagon()
        {
            if (P1.X == P2.X && P2.X == P3.X && P3.X == P4.X && P4.X == P5.X)
            {
                P1.X = (float)(Game.CursorPos.X + 400 * Math.Cos(1 * 2 * Math.PI / 5));
                P1.Y = (float)(Game.CursorPos.Y + 400 * Math.Sin(1 * 2 * Math.PI / 5) - 100);
                P2.X = (float)(Game.CursorPos.X + 400 * Math.Cos(2 * 2 * Math.PI / 5));
                P2.Y = (float)(Game.CursorPos.Y + 400 * Math.Sin(2 * 2 * Math.PI / 5) - 100);
                P3.X = (float)(Game.CursorPos.X + 400 * Math.Cos(3 * 2 * Math.PI / 5));
                P3.Y = (float)(Game.CursorPos.Y + 400 * Math.Sin(3 * 2 * Math.PI / 5) - 100);
                P4.X = (float)(Game.CursorPos.X + 400 * Math.Cos(4 * 2 * Math.PI / 5));
                P4.Y = (float)(Game.CursorPos.Y + 400 * Math.Sin(4 * 2 * Math.PI / 5) - 100);
                P5.X = (float)(Game.CursorPos.X + 400 * Math.Cos(5 * 2 * Math.PI / 5));
                P5.Y = (float)(Game.CursorPos.Y + 400 * Math.Sin(5 * 2 * Math.PI / 5) - 100);
            }
            else
            {
                if (R.IsReady() && P1.X != 0 && ObjectManager.Player.Distance(P1) < 400)
                {
                    R.Cast(P1);
                }
                if (R.IsReady() && P2.X != 0 && ObjectManager.Player.Distance(P2) < 400)
                {
                    R.Cast(P2);
                }
                if (R.IsReady() && P3.X != 0 && ObjectManager.Player.Distance(P3) < 400)
                {
                    R.Cast(P3);
                }
                if (R.IsReady() && P4.X != 0 && ObjectManager.Player.Distance(P4) < 400)
                {
                    R.Cast(P4);
                }
                if (R.IsReady() && P5.X != 0 && ObjectManager.Player.Distance(P5) < 400)
                {
                    R.Cast(P5);
                }
            }
            

        }

    }
}
