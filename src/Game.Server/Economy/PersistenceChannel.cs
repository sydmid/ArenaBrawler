using System.Threading.Channels;

namespace Game.Server.Economy
{
    public class PersistenceChannel
    {
        private readonly Channel<PersistenceItem> _channel;

        public PersistenceChannel()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateUnbounded<PersistenceItem>(options);
        }

        public ChannelReader<PersistenceItem> Reader => _channel.Reader;

        public bool TryWrite(in PersistenceItem item)
        {
            return _channel.Writer.TryWrite(item);
        }

        public void Complete()
        {
            _channel.Writer.Complete();
        }
    }
}
