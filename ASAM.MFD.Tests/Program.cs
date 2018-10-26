namespace ASAM.MFD.Tests
{
    using System;
    using System.IO;
    using System.Text;

    using ASAM.MDF.Libary;
    using ASAM.MDF.Libary.Types;

    public class Program
    {
        public static void Main(string[] args)
        {
            var filename1 = "..\\..\\ASAP2_Demo_V161_2.00.dat";
            var filename2 = "..\\..\\AVL_2042.dat";
            var filename3 = "..\\..\\measure05.dat";
            var filename4 = "..\\..\\test_write.dat";

            var mdf = new Mdf();

            mdf.IDBlock.ByteOrder = ByteOrder.LittleEndian;
            mdf.IDBlock.FloatingPointFormat = FloatingPointFormat.IEEE754;
            mdf.IDBlock.Version = 330;
            mdf.IDBlock.FormatIdentifier = mdf.IDBlock.Version.ToString();
            mdf.IDBlock.CodePage = (ushort)Encoding.UTF8.CodePage;
            mdf.IDBlock.Reserved1 = "we";
            mdf.IDBlock.Reserved2 = "ID block reserved2";

            mdf.HDBlock.Date = "26:10:2018";
            mdf.HDBlock.Time = "11:53:22";
            mdf.HDBlock.Author = "Me";
            mdf.HDBlock.Organization = "My organization";
            mdf.HDBlock.Project = "Mdf write";
            mdf.HDBlock.Subject = "testing writing";
            mdf.HDBlock.TimeStamp = 1201278007000000000;
            mdf.HDBlock.UTCTimeOffset = 3;
            mdf.HDBlock.TimeQuality = TimeQuality.External;
            mdf.HDBlock.TimerIdentification = "local";
            mdf.HDBlock.FileComment = TextBlock.Create(mdf, "aksdjashdjashdajshdqweqnwebasdajkshd");
            mdf.HDBlock.ProgramBlock = ProgramBlock.Create(mdf, new byte[] { 1, 2, 3, 4, 5 });

            mdf.DataGroups.Add(DataGroupBlock.Create(mdf));
            mdf.DataGroups.Add(DataGroupBlock.Create(mdf));
            mdf.DataGroups.Add(DataGroupBlock.Create(mdf));

            mdf.DataGroups[0].Reserved = 1;
            mdf.DataGroups[1].Reserved = 2;
            mdf.DataGroups[2].Reserved = 3;

            var bytes = mdf.GetBytes();

            File.WriteAllBytes(filename4, bytes);

            using (var stream = new FileStream(filename4, FileMode.Open))
            {
                mdf = new Mdf(stream);

                for (int i = 0; i < mdf.HDBlock.DataGroupsCount; i++)
                {
                    var dataGroup = mdf.DataGroups[i];

                    for (int k = 0; k < dataGroup.NumChannelGroups; k++)
                    {
                        var chGroup = dataGroup.ChannelGroups[k];
                        var records = dataGroup.Records;

                        Console.WriteLine(chGroup.Comment + " (RS: " + chGroup.RecordSize + ", RN: " + chGroup.NumRecords + ")");

                        for (int j = 0; j < chGroup.NumChannels; j++)
                        {
                            var ch = chGroup.Channels[j];
                            Console.WriteLine(
                                "\t" + ch.SignalName + "(" + ch.BitOffset / 8 + " + " + ch.AdditionalByteOffset + ")" + "\t"
                                + ch.SignalType);

                            Console.Write("\tValues: ");
                            for (int r = 0; r < records.Length; r++)
                            {
                                Console.Write(records[r].GetValue(ch) + "; ");
                            }

                            Console.WriteLine();
                        }
                    }

                    Console.WriteLine();
                }
            }

            Console.ReadKey();
        }
    }
}
