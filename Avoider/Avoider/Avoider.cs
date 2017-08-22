﻿using System.Linq;

using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using System.Drawing;
using Aimtec.SDK.Util.Cache;
using System;
using System.Collections.Generic;
using Aimtec.SDK.Orbwalking;

namespace Avoider
{
    internal class Avoider
    {
        public static Menu Menu = new Menu("Avoider", "Avoider", true);
        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public static List<GameObject> trapsList = new List<GameObject>();

        public Avoider()
        {
            Menu.Add(new MenuKeyBind("Key", "Auto Avoid", Aimtec.SDK.Util.KeyCode.N, KeybindType.Toggle));

            var Draw = new Menu("Draw", "Drawings");
            {
                Draw.Add(new MenuBool("Object", "Traps"));
                Draw.Add(new MenuBool("Pathing", "Pathing"));
            }
            Menu.Add(Draw);
            Menu.Attach();
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += OnGameObjectCreated;
            GameObject.OnDestroy += OnGameObjectDestroyed;
        }

        private void OnGameObjectCreated(GameObject sender)
        {

            if (!sender.IsAlly)
               return;

            if (sender.Name == "Caitlyn_Base_W_Indicator_SizeRing.troy")
            {
                trapsList.Add(sender);
            }

            if (sender.Name == "Jinx_Base_E_Mine_Ready_Green.troy")
            {
                trapsList.Add(sender);
            }

            if (sender.Name == "Nidalee_Base_W_TC_Green.troy")
            {
                trapsList.Add(sender);
            }
        }

        private void OnGameObjectDestroyed(GameObject sender)
        {
            if (!sender.IsAlly)
                return;

            if (sender.Name == "Caitlyn_Base_W_Indicator_SizeRing.troy")
            {
                trapsList.Remove(sender);
            }

            if (sender.Name == "Jinx_Base_E_Mine_Ready_Green.troy")
            {
                trapsList.Remove(sender);
            }

            if (sender.Name == "Nidalee_Base_W_TC_Green.troy")
            {
                trapsList.Remove(sender);
            }
        }

        private void Game_OnUpdate()
        {
            if (Player.IsDead || !Menu["Key"].Enabled)
                return;

            /*foreach (var obj in GameObjects.AllGameObjects.Where(obj => obj.Team == Player.Team && obj.IsValid))
            {
                if (Player.Distance(obj) > 1000)
                    continue;

                //supports caitlyn trap, teemo trap, nidalee trap. WILL NOT AVOID JINX TRAP!
                if (obj.Name.ToLower().Contains("cupcake trap") || obj.Name.ToLower().Contains("noxious trap"))
                {
                    if (Player.Distance(obj) < 200)
                        Avoid(obj.Position, 200);

                    if (Menu["Object"].Enabled)
                        Render.Circle(obj.Position, 100, 30, Color.Red);
                }
            }*/


            for (var i = 0; i < trapsList.Count; i++)
            {
                if (Player.Distance(trapsList[i]) > 1000)
                    continue;

                if (Menu["Object"].Enabled)
                 Render.Circle(trapsList[i].Position, 65, 30, Color.Red);



                if (Player.Distance(trapsList[i]) < 200 && Player.Distance(trapsList[i]) > 75 && !Player.HasBuffOfType(BuffType.Snare))
                {
                    Orbwalker.Implementation.AttackingEnabled = false;
                    //caitlyn trap
                    if (trapsList[i].Name == "Caitlyn_Base_W_Indicator_SizeRing.troy")
                    {
                        Avoid(trapsList[i].Position, 200, trapsList[i]);

                    }

                    //jinx trap
                    if (trapsList[i].Name == "Jinx_Base_E_Mine_Ready_Green.troy")
                    {
                        Avoid(trapsList[i].Position, 200, trapsList[i]);
                    }

                }
                else
                {
                    Orbwalker.Implementation.AttackingEnabled = true;
                }
            }
        }

        public static List<Vector3> Pathing(float radius, Vector3 position, GameObject trap)
        {
            List<Vector3> points = new List<Vector3>();
            for (var i = 1; i <= 360; i++)
            {
                var angle = i * Math.PI / 360; //angle = i * 2 * Math.PI / 360;

                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z + radius * (float)Math.Sin(angle));

                if (trap.Name == "Jinx_Base_E_Mine_Ready_Green.troy")
                {
                    //point = Player.ServerPosition + (Player.ServerPosition - 100).Normalized() * Math.Min((Player.BoundingRadius + 50) * 1.25f, position.Z + radius * (float)Math.Cos(angle));
                    //point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z + radius * (float)Math.Sin(angle));
                }

                points.Add(point);
            }
            return points;
        }

        public static Vector3 Perpendicular(Vector3 v)
        {
            return new Vector3(-v.Z, v.Y, v.X);
        }

        private static void Avoid(Vector3 position, float range, GameObject trap)
        {
            //var trapPositions = trapsList.Select(x => x.ServerPosition).ToArray();

            var nextPoints = Pathing(100, Player.Position, trap);

            var getPoint = nextPoints.Where(x => x.Distance(position) > range).OrderBy(y => y.Distance(Game.CursorPos)).FirstOrDefault();

            if (getPoint != null)
            {
                if (Menu["Pathing"].Enabled)
                    Render.Circle(getPoint, 30, 30, Color.LightBlue);

                Orbwalker.Implementation.Move(getPoint);
            }
        }
    }
}