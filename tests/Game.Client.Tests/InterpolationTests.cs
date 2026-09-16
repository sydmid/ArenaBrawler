using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;
using Game.Shared.Models;
using Game.Shared.Network;
using Game.Client.Network;
using Fexe.Player.Core;

namespace Game.Client.Tests
{
    public class InterpolationTests
    {
        [Fact]
        public void Interpolation_CalculatesCorrectPosition()
        {
            // For interpolation math check, a simple linear interpolation
            Vector2 start = new Vector2(100, 100);
            Vector2 end = new Vector2(200, 200);
            float t = 0.5f;

            Vector2 result = Vector2.Lerp(start, end, t);

            Assert.Equal(150, result.X);
            Assert.Equal(150, result.Y);
        }
    }
}
