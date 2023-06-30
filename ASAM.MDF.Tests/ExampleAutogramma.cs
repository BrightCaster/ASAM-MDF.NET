using ASAM.MDF.Libary;
using ASAM.MDF.Libary.Types;
using System;
using System.Collections.Generic;

namespace ASAM.MDF.Tests
{
    public static class ExampleAutogramma
    {
        private static readonly List<ChannelTag> channels = new List<ChannelTag>();
        public static void Start(Mdf mdf, string channelNames)
        {
            for (int i = 0; i < mdf.DataGroups.Count; i++)
            {
                var dataGroup = mdf.DataGroups[i];

                for (int k = 0; k < dataGroup.ChannelGroups.Count; k++)
                {
                    var chGroup = dataGroup.ChannelGroups[k];
                    if (chGroup.CycleCount == 0)
                        continue;

                    var timeChannel = GetTimeChannel(chGroup);

                    // Add parameters channels.
                    for (int j = 0; j < chGroup.Channels.Count; j++)
                    {
                        var ch = chGroup.Channels[j];
                        if (ch.TypeV4 != ChannelTypeV4.Data)
                            continue;

                        var chName = (ch.LongSignalName != null ? ch.LongSignalName.Text : ch.SignalName)
                            .Replace("\\CCP:1", "")
                            .Replace("\0", "");

                        if (channelNames != null && !channelNames.Contains(chName))
                            continue;

                        var channelTag = new ChannelTag(dataGroup, ch, chName, timeChannel);

                        channels.Add(channelTag);
                    }
                }
            }
            ReadValues();
        }
        static ChannelBlock GetTimeChannel(ChannelGroupBlock group)
        {
            if (group == null) return null;
            if (group.Mdf.IDBlock.Version >= 400)
            {
                for (int j = 0; j < group.Channels.Count; j++)
                {
                    var ch = group.Channels[j];
                    if (ch.TypeV4 != ChannelTypeV4.Time)
                        continue;

                    return ch;
                }
            }
            else
            {

                for (int j = 0; j < group.NumChannels; j++)
                {
                    var ch = group.Channels[j];
                    if (ch.TypeV3 != ChannelTypeV3.Time)
                        continue;

                    return ch;
                }
            }

            return null;
        }
        static void ReadValues()
        {
            foreach (var channelTag in channels)
            {
                //try
                //{
                var records = channelTag.DataGroup.Records;
                var pageSize = records.Length;

                for (int k = 0; k < pageSize; k++)
                {
                    var rec = records[k];
                    var val = rec.GetValue(channelTag.Channel);
                    if (val == null)
                        continue;

                    // Add time based categories.
                    if (channelTag.Time == null)
                        continue;

                    var valDouble = Convert.ToDouble(val);

                    channelTag.values.Add(valDouble);
                }
                //}
                //catch (Exception ex)
                //{
                //     Logger.Add("MdfReader", "Can't read " + channelTag, LogTypes.Warning);
                // }
            }
        }

        private class ChannelTag
        {
            private bool conversionCached;
            private string conversionString;

            internal List<double> values = new List<double>();

            public ChannelTag(DataGroupBlock dataGroup, ChannelBlock channel, string channelName, ChannelBlock time)
            {
                DataGroup = dataGroup;
                Channel = channel;
                Name = channelName;
                Time = time;
            }

            public ChannelBlock Channel { get; private set; }
            public string ConversionString
            {
                get
                {
                    if (conversionCached)
                        return conversionString;

                    var conversion = Channel.ChannelConversion;
                    if (conversion != null && conversion.PhysicalUnit != null)
                    {
                        conversionString = conversion.PhysicalUnit.Replace("\0", "");
                        if (conversionString == "1")
                            conversionString = "";
                    }

                    conversionCached = true;

                    return conversionString;
                }
            }
            public DataGroupBlock DataGroup { get; private set; }
            public string Name { get; set; }
            public ChannelBlock Time { get; private set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
