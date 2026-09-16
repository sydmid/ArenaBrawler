using System;
using System.Numerics;
using Raylib_cs;
using Game.Shared.Models;
using Fexe.Player.Core;

namespace Game.Client.Player
{
    public class PlayerController
    {
        public Vector2 Position { get; set; }
        private readonly MeleeCombatSystem _melee;
        private readonly RangedCombatSystem _ranged;
        private readonly PlayerCombatModeSwitcher _switcher;
        private ulong _playerId = 1;
        private uint _inputSequence = 0;

        public PlayerController(Vector2 spawnPos, MeleeCombatSystem melee, RangedCombatSystem ranged, PlayerCombatModeSwitcher switcher)
        {
            Position = spawnPos;
            _melee = melee;
            _ranged = ranged;
            _switcher = switcher;
        }

        public PlayerInputPayload PollAndProcessInput(float deltaTime, Rectangle bounds)
        {
            _inputSequence++;

            float dirX = 0;
            float dirY = 0;

            if (Raylib.IsKeyDown(KeyboardKey.W)) dirY -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S)) dirY += 1;
            if (Raylib.IsKeyDown(KeyboardKey.A)) dirX -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D)) dirX += 1;

            if (dirX != 0 || dirY != 0)
            {
                var length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                dirX /= length;
                dirY /= length;
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            float aimAngle = (float)Math.Atan2(mousePos.Y - Position.Y, mousePos.X - Position.X);

            bool attackRequested = Raylib.IsMouseButtonPressed(MouseButton.Left);
            bool combatToggleRequested = Raylib.IsKeyPressed(KeyboardKey.Q);

            if (combatToggleRequested)
            {
                _switcher.ToggleMode();
            }

            return new PlayerInputPayload(
                _playerId,
                _inputSequence,
                dirX,
                dirY,
                aimAngle,
                attackRequested,
                combatToggleRequested,
                (ulong)(DateTime.UtcNow.Ticks / 10)
            );
        }

        public void Render()
        {
            Raylib.DrawCircleV(Position, 20.0f, Color.Blue);

            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.DrawLineV(Position, mousePos, Color.DarkGray);
        }
    }
}
