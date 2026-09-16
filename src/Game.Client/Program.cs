using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using Game.Shared.Models;
using Game.Shared.Network;
using Game.Client.Player;
using Game.Client.Network;
using Fexe.Player.Core;

namespace Game.Client
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const int screenWidth = 1280;
            const int screenHeight = 720;
            const string windowTitle = "Arena Brawler - Real-Time Combat Arena (60Hz)";

            Raylib.InitWindow(screenWidth, screenHeight, windowTitle);
            Raylib.SetTargetFPS(60);

            Console.WriteLine("[Game.Client] Raylib initialized successfully.");

            Rectangle arenaBounds = new(60, 60, screenWidth - 120, screenHeight - 120);

            var melee = new MeleeCombatSystem();
            var ranged = new RangedCombatSystem();

            var switcher = new PlayerCombatModeSwitcher(melee, ranged, CombatMode.Ranged);
            switcher.OnModeChanged += (newMode) =>
            {
                Console.WriteLine($"[Game.Client] Combat mode transitioned to: {newMode}");
            };

            Vector2 spawnPos = new(screenWidth / 2.0f, screenHeight / 2.0f);
            var controller = new PlayerController(spawnPos, melee, ranged, switcher);

            // Connect client
            using var client = new GameClient("127.0.0.1", 9050, 1);
            client.Start();

            Console.WriteLine("[Game.Client] PlayerController & ModeSwitcher ready. Entering update loop.");

            var remoteEntities = new Dictionary<ulong, EntityState>();

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();

                PlayerInputPayload inputPayload = controller.PollAndProcessInput(deltaTime, arenaBounds);
                client.SendInput(inputPayload);

                if (client.TryGetLatestSnapshot(out var snapshot))
                {
                    foreach (var state in snapshot.states)
                    {
                        if (state.EntityId == client.LocalPlayerId)
                        {
                            // Basic client prediction / server reconciliation can happen here
                            // For simplicity, just snap to server state
                            controller.Position = new Vector2(state.PositionX, state.PositionY);
                        }
                        else
                        {
                            remoteEntities[state.EntityId] = state;
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(20, 24, 32, 255));

                for (int x = (int)arenaBounds.X; x <= (int)(arenaBounds.X + arenaBounds.Width); x += 60)
                {
                    Raylib.DrawLine(x, (int)arenaBounds.Y, x, (int)(arenaBounds.Y + arenaBounds.Height), new Color(32, 38, 50, 255));
                }
                for (int y = (int)arenaBounds.Y; y <= (int)(arenaBounds.Y + arenaBounds.Height); y += 60)
                {
                    Raylib.DrawLine((int)arenaBounds.X, y, (int)(arenaBounds.X + arenaBounds.Width), y, new Color(32, 38, 50, 255));
                }

                Raylib.DrawRectangleLinesEx(arenaBounds, 3.0f, new Color(65, 80, 100, 255));

                Raylib.DrawText("ARENA BRAWLER - PHASE 3 (NETWORKED LOOP)", 30, 25, 18, Color.Gold);
                Raylib.DrawFPS(screenWidth - 90, 25);

                controller.Render();

                foreach (var remoteEntity in remoteEntities.Values)
                {
                    Raylib.DrawCircleV(new Vector2(remoteEntity.PositionX, remoteEntity.PositionY), 20.0f, Color.Red);
                }

                switcher.RenderHUD(screenWidth, screenHeight);

                string weaponStatus = switcher.IsMelee
                    ? $"Status: {(melee.IsAttacking ? "SWINGING" : "READY")} | Range: {melee.MeleeRange}px"
                    : $"Status: READY | Active Bullets: {ranged.ActiveBulletCount}";
                Raylib.DrawText(weaponStatus, 380, screenHeight - 45, 13, Color.LightGray);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
            Console.WriteLine("[Game.Client] Window closed cleanly.");
        }
    }
}
