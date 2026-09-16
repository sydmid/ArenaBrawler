using System;
using System.Numerics;
using Raylib_cs;
using Game.Shared.Models;
using Game.Client.Player;
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

            Console.WriteLine("[Game.Client] PlayerController & ModeSwitcher ready. Entering update loop.");

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();

                PlayerInputPayload inputPayload = controller.PollAndProcessInput(deltaTime, arenaBounds);

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

                Raylib.DrawText("ARENA BRAWLER - PHASE 2 (COMBAT MECHANICS)", 30, 25, 18, Color.Gold);
                Raylib.DrawFPS(screenWidth - 90, 25);

                controller.Render();

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
