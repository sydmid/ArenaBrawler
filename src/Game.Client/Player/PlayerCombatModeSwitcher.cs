using System;
using Raylib_cs;
using Game.Shared.Models;

namespace Fexe.Player.Core
{
    public class PlayerCombatModeSwitcher
    {
        private readonly object _melee;
        private readonly object _ranged;
        public CombatMode CurrentMode { get; private set; }

        public event Action<CombatMode>? OnModeChanged;

        public bool IsMelee => CurrentMode == CombatMode.Melee;

        public PlayerCombatModeSwitcher(object melee, object ranged, CombatMode initialMode)
        {
            _melee = melee;
            _ranged = ranged;
            CurrentMode = initialMode;
        }

        public void ToggleMode()
        {
            CurrentMode = CurrentMode == CombatMode.Melee ? CombatMode.Ranged : CombatMode.Melee;
            OnModeChanged?.Invoke(CurrentMode);
        }

        public void RenderHUD(int screenWidth, int screenHeight)
        {
            Raylib.DrawText($"Current Mode: {CurrentMode}", 380, screenHeight - 65, 13, Color.LightGray);
        }
    }
}
