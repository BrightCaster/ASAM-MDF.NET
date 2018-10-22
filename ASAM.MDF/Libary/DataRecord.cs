namespace Mdf
{
    using System;
    using System.Collections;

    using ASAM.MDF.Libary;
    using ASAM.MDF.Libary.Types;

    public struct DataRecord
    {
        public DataRecord(ChannelGroupBlock channelGroup, byte[] data)
        {
            ChannelGroup = channelGroup;
            Data = data;
        }

        public ChannelGroupBlock ChannelGroup { get; private set; }
        public byte[] Data { get; private set; }

        public object GetValue(ChannelBlock channel)
        {
            if (ChannelGroup.Channels.Contains(channel) == false)
                throw new ArgumentException("channel");

            var byteOffset = channel.BitOffset / 8;

            // TODO: BigEndian byte order not supported yet.
            switch (channel.SignalType)
            {
                case SignalType.UInt:
                    {
                        var result = 0u;
                        for (int i = 0; i < channel.NumberOfBits; i++)
                            result |= (uint)((GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i);

                        return result;
                    }

                case SignalType.Int:
                    {
                        var result = 0;
                        for (int i = 0; i < channel.NumberOfBits; i++)
                            result |= (GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i;

                        return result;
                    }

                case SignalType.IEEE754Float:
                    if (channel.NumberOfBits == 32)
                        return BitConverter.ToSingle(Data, byteOffset);
                    if (channel.NumberOfBits == 64)
                        return (float)BitConverter.ToDouble(Data, byteOffset);

                    break;

                case SignalType.IEEE754Double:
                    if (channel.NumberOfBits == 32)
                        return (double)BitConverter.ToSingle(Data, byteOffset);
                    if (channel.NumberOfBits == 64)
                        return BitConverter.ToDouble(Data, byteOffset);

                    break;

                case SignalType.String:
                    return ChannelGroup.Mdf.IDBlock.Encoding.GetString(Data, byteOffset, channel.NumberOfBits / 8);
            }

            return null;
        }

        private static bool GetBit(byte[] array, int bit)
        {
            var bytePosition = bit / 8;
            var bitOffset = bit % 8;

            return (array[bytePosition] & (1 << bitOffset)) != 0;
        }
    }
}
