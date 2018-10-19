namespace ASAM.MFD.Tests
{
    using System;
    using System.IO;

    using ASAM.MDF.Libary;

    public class Program
    {
        public static void Main(string[] args)
        {
            var filename1 = "..\\..\\ASAP2_Demo_V161_2.00.dat";
            var filename2 = "..\\..\\AVL_2042.dat";
            var filename3 = "..\\..\\measure05.dat";
            using (var stream = new FileStream(filename2, FileMode.Open))
            {
                var mdf = new Mdf(stream);

                for (int i = 0; i < mdf.HDBlock.DataGroupsCount; i++)
                {
                    var dataGroup = mdf.HDBlock.DataGroups[i];

                    Console.WriteLine("Data offset: " + dataGroup.m_ptrDataBlock);

                    for (int k = 0; k < dataGroup.NumChannelGroups; k++)
                    {
                        var chGroup = dataGroup.ChannelGroups[k];
                        var records = dataGroup.Records;

                        Console.WriteLine(chGroup.Comment + " (RS: " + chGroup.RecordSize + ", RN: " + chGroup.NumRecords + ")");

                        for (int j = 0; j < chGroup.NumChannels; j++)
                        {
                            var ch = chGroup.Channels[j];
                            Console.WriteLine("\t" + ch.SignalName + "(" + ch.BitOffset + ")" + "\t" + ch.SignalType);
                        }
                    }

                    Console.WriteLine();
                }
            }

            Console.ReadKey();
        }
    }
}
