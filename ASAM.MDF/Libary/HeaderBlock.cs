namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
    using System.Text;

    using ASAM.MDF.Libary.Types;

    public class HeaderBlock : Block
    {
        private string author;
        private string date;
        private string organization;
        private string project;
        private string subject;
        private string time;
        private string timerIdentification;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderBlock" /> class.
        /// </summary>
        /// <param name="mdf">The MDF.</param>
        /// <exception cref="System.FormatException"></exception>
        private HeaderBlock(Mdf mdf) : base(mdf)
        {
        }
        
        /// <summary>
        /// Gets the file comment.
        /// </summary>
        /// <value>
        /// The file comment.
        /// </value>
        public TextBlock FileComment { get; set; }

        /// <summary>
        /// Gets the program block.
        /// </summary>
        /// <value>
        /// The program block.
        /// </value>
        public ProgramBlock ProgramBlock { get; set; }

        /// <summary>
        /// Number of data groups (redundant information)
        /// </summary>
        /// <value>
        /// The data groups count.
        /// </value>
        public ushort DataGroupsCount { get; private set; }

        /// <summary>
        /// Gets the date at which the recording was started in "DD:MM:YYYY" format
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public string Date
        {
            get { return date; }
            set { SetStringValue(ref date, value, 10); }
        }

        /// <summary>
        /// Gets the time at which the recording was started in "HH:MM:SS" format 
        /// for locally displayed time, i.e. considering the daylight saving time (DST)
        /// Note: all time stamps in a time channel are only relative to the start 
        /// time of the measurement (see remark on time channel type in CNBLOCK)
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public string Time
        {
            get { return time; }
            set { SetStringValue(ref time, value, 8); }
        }

        /// <summary>
        /// Gets the authors name.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        public string Author
        {
            get { return author; }
            set { SetStringValue(ref author, value, 32); }
        }

        /// <summary>
        /// Gets the name of the organization or department
        /// </summary>
        /// <value>
        /// The organization or department.
        /// </value>
        public string Organization
        {
            get { return organization; }
            set { SetStringValue(ref organization, value, 32); }
        }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public string Project
        {
            get { return project; }
            set { SetStringValue(ref project, value, 32); }
        }

        /// <summary>
        /// Gets the Subject / Measurement object, e.g. vehicle information
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject
        {
            get { return subject; }
            set { SetStringValue(ref subject, value, 32); }
        }

        /// <summary>
        /// Time stamp at which recording was started in nanoseconds. 
        /// Elapsed time since 00:00:00 01.01.1970 (local time) 
        /// (local time = UTC time + UTC time offset) 
        /// Note: the local time does not contain a daylight saving time (DST) 
        /// offset! Valid since version 3.20. Default value: 0 See <see cref="UTCTimeOffset"/>
        /// </summary>
        /// <value>
        /// The time stamp.
        /// </value>
        [MdfVersion(320, 0)]
        public ulong TimeStamp { get; set; }

        /// <summary>
        /// UTC time offset in hours (= GMT time zone) For example 1 means 
        /// GMT+1 time zone = Central European Time (CET). The value must be
        /// in range [-12, 12], i.e. it can be negative! 
        /// Valid since version 3.20. 
        /// Default value: 0 (= GMT time)
        /// </summary>
        /// <value>
        /// The UTC time offset.
        /// </value>
        [MdfVersion(320, 0)]
        public short UTCTimeOffset { get; set; }

        /// <summary>
        /// Time quality class
        /// 0 = local PC reference time (Default)
        /// 10 = external time source
        /// 16 = external absolute synchronized time
        /// Valid since version 3.20. Default value: 0
        /// </summary>
        /// <value>
        /// The time quality.
        /// </value>
        [MdfVersion(320, 0)]
        public TimeQuality TimeQuality { get; set; }

        /// <summary>
        /// Timer identification (time source), e.g. "Local PC Reference Time" 
        /// or "GPS Reference Time". 
        /// Valid since version 3.20. 
        /// Default value: empty string
        /// </summary>
        /// <value>
        /// The timer identification.
        /// </value>
        [MdfVersion(320, "")]
        public string TimerIdentification
        {
            get { return timerIdentification; }
            set { SetStringValue(ref timerIdentification, value, 32); }
        }

        internal static HeaderBlock Create(Mdf mdf)
        {
            var block = new HeaderBlock(mdf);

            block.Identifier = "HD";
            block.Date = "";
            block.Time = "";
            block.Author = "";
            block.Organization = "";
            block.Project = "";
            block.Subject = "";
            block.TimerIdentification = "";

            return block;
        }
        internal static HeaderBlock Read(Mdf mdf, Stream stream)
        {
            var block = new HeaderBlock(mdf);
            block.Read(stream);

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.DataGroupsCount = BitConverter.ToUInt16(data, 12);
            block.Date = mdf.IDBlock.Encoding.GetString(data, 14, 10);
            block.Time = mdf.IDBlock.Encoding.GetString(data, 24, 8);
            block.Author = mdf.IDBlock.Encoding.GetString(data, 32, 32);
            block.Organization = mdf.IDBlock.Encoding.GetString(data, 64, 32);
            block.Project = mdf.IDBlock.Encoding.GetString(data, 96, 32);
            block.Subject = mdf.IDBlock.Encoding.GetString(data, 128, 32);

            if (mdf.IDBlock.Version >= 320)
            {
                block.TimeStamp = BitConverter.ToUInt64(data, 160);
                block.UTCTimeOffset = BitConverter.ToInt16(data, 168);
                block.TimeQuality = (TimeQuality)BitConverter.ToUInt16(data, 170);
                block.TimerIdentification = mdf.IDBlock.Encoding.GetString(data, 172, 32);
            }
            else
            {
                block.TimeStamp = 0;
                block.UTCTimeOffset = 0;
                block.TimeQuality = 0;
                block.TimerIdentification = "";
            }

            var ptrFirstDataGroup = BitConverter.ToUInt32(data, 0);
            var ptrTextBlock = BitConverter.ToUInt32(data, 4);
            var ptrProgramBlock = BitConverter.ToUInt32(data, 8);

            // Check if ptrTextBlock is null
            if (ptrTextBlock != 0)
            {
                stream.Position = ptrTextBlock;
                block.FileComment = TextBlock.Read(mdf, stream);
            }

            // Check if ptrProgramBlock is null
            if (ptrProgramBlock != 0)
            {
                stream.Position = ptrProgramBlock;
                block.ProgramBlock = ProgramBlock.Read(mdf, stream);
            }

            // Check if ptrFirstDataGroup is null
            if (ptrFirstDataGroup != 0)
            {
                mdf.DataGroups.Read(DataGroupBlock.Read(mdf, stream, ptrFirstDataGroup));
            }

            return block;
        }

        internal override ushort GetSize()
        {
            if (Mdf.IDBlock.Version >= 320)
                return 208;

            return 164;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var baseIndex = index;
            var bytesDataGroupsCount = BitConverter.GetBytes(Mdf.DataGroups.Count);
            var bytesDate = Mdf.IDBlock.Encoding.GetBytes(Date);
            var bytesTime = Mdf.IDBlock.Encoding.GetBytes(Time);
            var bytesAuthor = Mdf.IDBlock.Encoding.GetBytes(Author);
            var bytesOrganization = Mdf.IDBlock.Encoding.GetBytes(Organization);
            var bytesProject = Mdf.IDBlock.Encoding.GetBytes(Project);
            var bytesSubject = Mdf.IDBlock.Encoding.GetBytes(Subject);
            var bytesTimestamp = BitConverter.GetBytes(TimeStamp);
            var bytesUtcTimeOffset = BitConverter.GetBytes(UTCTimeOffset);
            var bytesTimeQuality = BitConverter.GetBytes((ushort)TimeQuality);
            var bytesTimeIdentification = Mdf.IDBlock.Encoding.GetBytes(TimerIdentification);

            Array.Copy(bytesDataGroupsCount, 0, array, index + 16, bytesDataGroupsCount.Length);
            Array.Copy(bytesDate, 0, array, index + 18, bytesDate.Length);
            Array.Copy(bytesTime, 0, array, index + 28, bytesTime.Length);
            Array.Copy(bytesAuthor, 0, array, index + 36, bytesAuthor.Length);
            Array.Copy(bytesOrganization, 0, array, index + 68, bytesOrganization.Length);
            Array.Copy(bytesProject, 0, array, index + 100, bytesProject.Length);
            Array.Copy(bytesSubject, 0, array, index + 132, bytesSubject.Length);

            index += 164;

            if (Mdf.IDBlock.Version >= 320)
            {
                Array.Copy(bytesTimestamp, 0, array, index, bytesTimestamp.Length);
                Array.Copy(bytesUtcTimeOffset, 0, array, index + 8, bytesUtcTimeOffset.Length);
                Array.Copy(bytesTimeQuality, 0, array, index + 10, bytesTimeQuality.Length);
                Array.Copy(bytesTimeIdentification, 0, array, index + 12, bytesTimeIdentification.Length);

                index += 44;
            }
        }
        internal void WriteFirstDataGroupLink(byte[] array, int index, int baseIndex)
        {
            var bytesFirstDataGroupLink = BitConverter.GetBytes(index);

            Array.Copy(bytesFirstDataGroupLink, 0, array, baseIndex + 4, bytesFirstDataGroupLink.Length);
        }
        internal void WriteFileComment(byte[] array, ref int index, int baseIndex)
        {
            if (FileComment == null) return;

            var bytesFileCommentLink = BitConverter.GetBytes(index);

            Array.Copy(bytesFileCommentLink, 0, array, baseIndex + 8, bytesFileCommentLink.Length);

            FileComment.Write(array, ref index);
        }
        internal void WriteProgramBlock(byte[] array, ref int index, int baseIndex)
        {
            if (ProgramBlock == null) return;

            var bytesProgramLink = BitConverter.GetBytes(index);

            Array.Copy(bytesProgramLink, 0, array, baseIndex + 12, bytesProgramLink.Length);

            ProgramBlock.Write(array, ref index);
        }
    }
}
