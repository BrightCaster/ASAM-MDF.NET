namespace ASAM.MDF.Libary
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
        public byte[] Data { get; set; }

        public object GetValue(ChannelBlock channel)
        {
            if (ChannelGroup.Channels.Contains(channel) == false)
                throw new ArgumentException("channel");

            var byteOffset = channel.ByteOffset != 0 ? (int)channel.ByteOffset : channel.BitOffset / 8; //
            var value = (object)null;

            // TODO: BigEndian byte order not supported yet.
            switch (channel.SignalType)
            {
                case SignalType.UInt:
                    {
                        var result = 0u;
                        if (channel.BitLength != 0)
                            for (int i = 0; i < channel.BitLength; i++)
                                result |= (uint)((GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i);
                        else
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (uint)((GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i);

                        value = result;
                        break;
                    }

                case SignalType.Int:
                    {
                        var result = 0;
                        if (channel.BitLength != 0)
                            for (int i = 0; i < channel.BitLength; i++)
                                result |= (GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i;
                        else
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i;

                        var maxValue = channel.BitLength != 0 ? Math.Pow(2, channel.BitLength) / 2 : Math.Pow(2, channel.NumberOfBits) / 2;
                        if (result > maxValue)
                            result -= (int)(maxValue * 2);

                        value = result;
                        break;
                    }

                case SignalType.IEEE754Float:
                    if (channel.BitLength != 0)
                    {
                        if (channel.BitLength == 32)
                            value = BitConverter.ToSingle(Data, byteOffset);
                        if (channel.BitLength == 64)
                            value = (float)BitConverter.ToDouble(Data, byteOffset);
                    }
                    else
                    {
                        if (channel.NumberOfBits == 32)
                            value = BitConverter.ToSingle(Data, byteOffset);
                        if (channel.NumberOfBits == 64)
                            value = (float)BitConverter.ToDouble(Data, byteOffset);
                    }
                    break;

                case SignalType.IEEE754Double:
                    if (channel.BitLength != 0)
                    {
                        if (channel.BitLength == 32)
                            value = (double)BitConverter.ToSingle(Data, byteOffset);
                        if (channel.BitLength == 64)
                            value = BitConverter.ToDouble(Data, byteOffset);
                    }
                    else
                    {
                        if (channel.NumberOfBits == 32)
                            value = (double)BitConverter.ToSingle(Data, byteOffset);
                        if (channel.NumberOfBits == 64)
                            value = BitConverter.ToDouble(Data, byteOffset);
                    }
                    break;

                case SignalType.String:
                    if (channel.BitLength != 0)
                        value = ChannelGroup.Mdf.IDBlock.Encoding.GetString(Data, byteOffset, (int)channel.BitLength / 8);
                    else
                        value = ChannelGroup.Mdf.IDBlock.Encoding.GetString(Data, byteOffset, channel.NumberOfBits / 8);
                    break;

            }

            if (channel.ChannelConversion != null && value != null)
                return channel.ChannelConversion.AdditionalConversionData.GetPhyValue(Convert.ToDouble(value));

            return value;
        }

        private static bool GetBit(byte[] array, int bit)
        {
            var bytePosition = bit / 8;
            var bitOffset = bit % 8;

            return (array[bytePosition] & (1 << bitOffset)) != 0;
        }
    }
}
