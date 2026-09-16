using System.Runtime.InteropServices;
using Game.Shared.Network;
using Xunit;

namespace Game.Shared.Tests
{
    public class GameFrameHeaderTests
    {
        [Fact]
        public void GameFrameHeader_HasExpectedSizeAndMagic()
        {
            Assert.Equal(8, Marshal.SizeOf<GameFrameHeader>());
            Assert.Equal(8, GameFrameHeader.Size);

            GameFrameHeader validHeader = new(GameProtocolConstants.MsgTypeEntityState, 0, 38, 1);
            Assert.True(validHeader.IsValid);
            Assert.Equal(GameProtocolConstants.ArenaFrameMagic, validHeader.Magic);
            Assert.Equal(GameProtocolConstants.MsgTypeEntityState, validHeader.MsgType);
            Assert.Equal(38, validHeader.PayloadLen);
        }
    }
}
