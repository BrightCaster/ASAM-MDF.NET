namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
    using System.Linq;

    using ASAM.MDF.Libary.Types;

    public class ChannelBlock : Block, INext<ChannelBlock>, IPrevious<ChannelBlock>, IParent<ChannelGroupBlock>
    {
        public delegate void ChanelHandlerRemovedAddress(ChannelBlock block, byte[] bytes);

        private const int MIN_VERSION_LONG_SIGNAL_NAME = 212;
        private const int MIN_VERSION_DISPLAY_NAME = 300;
        private const int MIN_VERSION_ADDITIONAL_BYTE_OFFSET = 300;

        internal (ulong address, int offset) ptrNextChannelBlock;
        internal (ulong address, int offset) ptrChannelConversionBlock;
        internal (ulong address, int offset) ptrDataBlockSignal;
        internal (ulong address, int offset) ptrUnit;
        internal (ulong address, int offset) ptrTextBlockComment;
        internal (ulong address, int offset) ptrAttachment;
        internal (ulong address, int offset) ptrChannelExtensionBlock;
        internal (ulong address, int offset) ptrChannelDependencyBlock;
        internal (ulong address, int offset) ptrChannelComment;
        internal (ulong address, int offset) ptrLongSignalName;
        internal (ulong address, int offset) ptrDisplayName;
        internal (ulong address, int offset) ptrComponentAddress;
        internal (ulong address, int offset) ptrTextBlockChanelName;

        private string signalName;
        private string signalDescription;
        private byte SyncType;
        private byte DataType;
        private ChannelConversionBlock channelConversion;
        private ChannelBlock next;

        private ChannelBlock(Mdf mdf) : base(mdf)
        { }

        public event ChanelHandlerRemovedAddress ChanelsRemovedAddress;

        public ChannelBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelBlock.address != 0 && ptrNextChannelBlock.address < (ulong)Mdf.Data.Length)
                    next = Read(Mdf, (int)ptrNextChannelBlock.address);

                return next;
            }
        }
        public ChannelBlock Previous { get; set; }
        public ChannelConversionBlock ChannelConversion { get => channelConversion; set => channelConversion = value; }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; private set; }
        public ChannelTypeV3 TypeV3 { get; set; }
        public ChannelTypeV4 TypeV4 { get; set; }

        public string SignalName
        {
            get { return signalName; }
            set { SetStringValue(ref signalName, value, 32); }
        }
        public string SignalDescription
        {
            get { return signalDescription; }
            set { SetStringValue(ref signalDescription, value, 128); }
        }
        public ushort BitOffset { get; set; }
        public uint BitLength { get; private set; }
        public uint ChannelFlags { get; private set; }
        public uint InvalidBitPos { get; private set; }
        public byte Precision { get; private set; }
        public byte Reserved1 { get; private set; }
        public ushort AttachmentCount { get; private set; }
        public double ValRangeMin { get; private set; }
        public double ValRangeMax { get; private set; }
        public double LimitMin { get; private set; }
        public double LimitMax { get; private set; }
        public double LimitMinExt { get; private set; }
        public double LimitMaxExt { get; private set; }
        public ushort NumberOfBits { get; set; }
        public SignalTypeV3 SignalTypeV3 { get; set; }
        public SignalTypeV4 SignalTypeV4 { get; set; }
        public bool ValueRange { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double SampleRate { get; set; }
        public TextBlock LongSignalName { get; private set; }
        public TextBlock DisplayName { get; private set; }
        public uint AdditionalByteOffset { get; set; }
        public TextBlock Unit { get; private set; }
        public ChannelGroupBlock Parent { get; set; }

        public static ChannelBlock Create(Mdf mdf)
        {
            return new ChannelBlock(mdf)
            {
                Identifier = "CN",
                SignalName = "",
                SignalDescription = "",
            };
        }
        public static ChannelBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelBlock(mdf);
            block.next = null;
            block.SourceDepending = null;
            block.Dependency = null;
            block.Comment = null;

            block.Read();
            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            ptrNextChannelBlock = (Mdf.ReadU32().ValidateAddress(Mdf), 4);
            ptrChannelConversionBlock = (Mdf.ReadU32().ValidateAddress(Mdf),ptrNextChannelBlock.offset + 4);
            ptrChannelExtensionBlock = (Mdf.ReadU32().ValidateAddress(Mdf),ptrChannelConversionBlock.offset + 4);
            ptrChannelDependencyBlock = (Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelExtensionBlock.offset + 4);
            ptrChannelComment = (Mdf.ReadU32().ValidateAddress(Mdf),ptrChannelDependencyBlock.offset + 4);
            TypeV3 = (ChannelTypeV3)Mdf.ReadU16();
            SignalName = Mdf.GetString(32);
            SignalDescription = Mdf.GetString(128);
            BitOffset = Mdf.ReadU16();
            NumberOfBits = Mdf.ReadU16();
            SignalTypeV3 = (SignalTypeV3)Mdf.ReadU16();
            ValueRange = Mdf.ReadBoolean();

            if (ValueRange)
            {
                MinValue = Mdf.ReadDouble();
                MaxValue = Mdf.ReadDouble();
            }
            else
            {
                Mdf.UpdatePosition(Mdf.position + 16);
            }
            SampleRate = Mdf.ReadDouble();

            var offset = 2 + 32 + 128 + 2 + 2 + 2 + 2 + 16 + 8;
            if (Mdf.IDBlock.Version >= MIN_VERSION_LONG_SIGNAL_NAME)
            {
                ptrLongSignalName = (Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelComment.offset + offset);
                offset += 8;
            }
            if (Mdf.IDBlock.Version >= MIN_VERSION_DISPLAY_NAME)
                ptrDisplayName = (Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelComment.offset + offset);

            if (Mdf.IDBlock.Version >= MIN_VERSION_ADDITIONAL_BYTE_OFFSET)
                AdditionalByteOffset = Mdf.ReadU16();

            if (ptrTextBlockChanelName.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrTextBlockChanelName.address);

            if (ptrUnit.address != 0)
                Unit = TextBlock.Read(Mdf, (int)ptrUnit.address);

            if (ptrTextBlockComment.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlockComment.address);

            if (ptrLongSignalName.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrLongSignalName.address);

            if (channelConversion == null && ptrChannelConversionBlock.address != 0)
                ChannelConversion = ChannelConversionBlock.Read(Mdf, (int)ptrChannelConversionBlock.address);
            //if (ptrChannelExtensionBlock != 0)
            //{
            //    if (Mdf.IDBlock.Version == 400)

            //    SourceDepending = new ChannelExtensionBlock(Mdf, ptrChannelExtensionBlock);
            //}
        }

        internal override void ReadV4()
        {
            base.ReadV4();

            ptrNextChannelBlock = (Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrComponentAddress = (Mdf.ReadU64().ValidateAddress(Mdf), ptrNextChannelBlock.offset + 8);
            ptrTextBlockChanelName = (Mdf.ReadU64().ValidateAddress(Mdf), ptrComponentAddress.offset + 8);
            ptrChannelExtensionBlock = (Mdf.ReadU64().ValidateAddress(Mdf), ptrTextBlockChanelName.offset + 8);
            ptrChannelConversionBlock = (Mdf.ReadU64().ValidateAddress(Mdf), ptrChannelExtensionBlock.offset + 8);
            ptrDataBlockSignal = (Mdf.ReadU64().ValidateAddress(Mdf), ptrChannelConversionBlock.offset + 8);
            ptrUnit = (Mdf.ReadU64().ValidateAddress(Mdf), ptrDataBlockSignal.offset + 8);
            ptrTextBlockComment = (Mdf.ReadU64().ValidateAddress(Mdf), ptrUnit.offset + 8);
            //ptrAttachment = Mdf.ReadU64();
            //ptrDefaultDGBlock = Mdf.ReadU64();
            //ptrDefaultCGBlock = Mdf.ReadU64();
            //ptrDefaultCurrentChanelBlock = Mdf.ReadU64();
            TypeV4 = (ChannelTypeV4)Mdf.ReadByte();
            SyncType = Mdf.ReadByte();
            SignalTypeV4 = (SignalTypeV4)Mdf.ReadByte();
            BitOffset = Mdf.ReadByte();
            AdditionalByteOffset = Mdf.ReadU32();
            NumberOfBits = (ushort)Mdf.ReadU32();
            ChannelFlags = Mdf.ReadU32();
            InvalidBitPos = Mdf.ReadU32();
            Precision = Mdf.ReadByte();
            Reserved1 = Mdf.ReadByte();
            AttachmentCount = Mdf.ReadU16();
            ValRangeMin = Mdf.ReadDouble();
            ValRangeMax = Mdf.ReadDouble();
            LimitMin = Mdf.ReadDouble();
            LimitMax = Mdf.ReadDouble();
            LimitMinExt = Mdf.ReadDouble();
            LimitMaxExt = Mdf.ReadDouble();

            if (ptrTextBlockChanelName.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrTextBlockChanelName.address);

            if (ptrUnit.address != 0)
                Unit = TextBlock.Read(Mdf, (int)ptrUnit.address);

            if (ptrTextBlockComment.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlockComment.address);

            if (ptrLongSignalName.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrLongSignalName.address);

            if (channelConversion == null && ptrChannelConversionBlock.address != 0)
                ChannelConversion = ChannelConversionBlock.Read(Mdf, (int)ptrChannelConversionBlock.address);
        }
        /// <summary>
        /// Set this address 0 for previous channel. Lost address
        /// </summary>
        /// <returns>Copied modified the entire array of mdf bytes</returns>
        public byte[] Remove(byte[] bytes)
        {
            if (TypeV3 == ChannelTypeV3.Time)
                return bytes;

            var previous = Previous;
            if (previous == null && next != null)// first of list node channels: [X channel]->[1 channel]->[2 channel]->...->[n channel]
            {
                ChanelsRemovedAddress?.Invoke(this, bytes);

                next.Previous = null;

                return bytes;
            }
            else if (previous == null && next == null)
            {
                ChanelsRemovedAddress?.Invoke(this, bytes);

                return bytes;
            }
            var blockPrevAddress = previous.BlockAddress;
            var thisPointer = blockPrevAddress + 4;

            var newbytes = BitConverter.GetBytes((int)ptrNextChannelBlock.address);
            Array.Copy(newbytes, 0, bytes, thisPointer, newbytes.Length);//changing the pointer to this block from the previous block, to the next of this block
            //Array.Copy(new byte[(int)Size - 4 - newbytes.Length], 0, bytes, BlockAddress + 4 + newbytes.Length, (int)Size - 4 - newbytes.Length); //set empty(?) data after address of next block

            previous.ptrNextChannelBlock = ptrNextChannelBlock;
            previous.next = next;

            if (next != null)
                next.Previous = previous;

            ChanelsRemovedAddress?.Invoke(this, bytes);
            
            var removedBytes = bytes.Where((x, index) => index < BlockAddress || BlockAddress + (int)Size < index).ToArray();
            Array.Copy(removedBytes, bytes, removedBytes.Length);
            Array.Resize(ref bytes, removedBytes.Length);
            return bytes;
        }

        public override string ToString()
        {
            return SignalName;
        }

        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            if (channelConversion != null)
                size += channelConversion.GetSizeTotal();

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesChannelType = BitConverter.GetBytes((ushort)TypeV3);
            var bytesSignalName = Mdf.IDBlock.Encoding.GetBytes(SignalName);
            var bytesSignalDesc = Mdf.IDBlock.Encoding.GetBytes(SignalDescription);
            var bytesBitOffset = BitConverter.GetBytes(BitOffset);
            var bytesNumOfBits = BitConverter.GetBytes(NumberOfBits);
            var bytesSignalDataType = BitConverter.GetBytes((ushort)SignalTypeV3);
            var bytesValueRangeValid = BitConverter.GetBytes(ValueRange);
            var bytesMinValue = BitConverter.GetBytes(MinValue);
            var bytesMaxValue = BitConverter.GetBytes(MaxValue);
            var bytesSampleRate = BitConverter.GetBytes(SampleRate);

            Array.Copy(bytesChannelType, 0, array, index + 24, bytesChannelType.Length);
            Array.Copy(bytesSignalName, 0, array, index + 26, bytesSignalName.Length);
            Array.Copy(bytesSignalDesc, 0, array, index + 58, bytesSignalDesc.Length);
            Array.Copy(bytesBitOffset, 0, array, index + 186, bytesBitOffset.Length);
            Array.Copy(bytesNumOfBits, 0, array, index + 188, bytesNumOfBits.Length);
            Array.Copy(bytesSignalDataType, 0, array, index + 190, bytesSignalDataType.Length);
            Array.Copy(bytesValueRangeValid, 0, array, index + 192, bytesValueRangeValid.Length);
            Array.Copy(bytesMinValue, 0, array, index + 194, bytesMinValue.Length);
            Array.Copy(bytesMaxValue, 0, array, index + 202, bytesMaxValue.Length);
            Array.Copy(bytesSampleRate, 0, array, index + 210, bytesSampleRate.Length);

            if (Mdf.IDBlock.Version >= 212)
            {
                // TODO: LongSignalName.
            }

            if (Mdf.IDBlock.Version >= 300)
            {
                // TODO: DisplayName.
                var bytesAdditionalOffset = BitConverter.GetBytes(AdditionalByteOffset);

                Array.Copy(bytesAdditionalOffset, 0, array, index + 226, bytesAdditionalOffset.Length);
            }

            index += GetSize();
        }
        internal void WriteChannelConversion(byte[] array, ref int index, int blockIndex)
        {
            if (channelConversion == null)
                return;

            var bytesConversionIndex = BitConverter.GetBytes(index);

            Array.Copy(bytesConversionIndex, 0, array, blockIndex + 8, bytesConversionIndex.Length);

            ChannelConversion.Write(array, ref index);
        }
        internal void WriteNextChannelLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelLink, 0, array, blockIndex + 4, bytesNextChannelLink.Length);
        }

        internal void ChannelUpdateAddress(int indexDeleted, byte[] bytes, ulong countDeleted)
        {
            if ((int)ptrAttachment.address > indexDeleted)
            {
                ptrAttachment.address -= countDeleted;

                this.CopyAddress(ptrAttachment, bytes);
            }
            if ((int)ptrChannelComment.address > indexDeleted)
            {
                ptrChannelComment.address -= countDeleted;

                this.CopyAddress(ptrChannelComment, bytes);
            }
            if ((int)ptrChannelConversionBlock.address > indexDeleted)
            {
                ptrChannelConversionBlock.address -= countDeleted;

                this.CopyAddress(ptrChannelConversionBlock, bytes);
            }
            if ((int)ptrChannelDependencyBlock.address > indexDeleted)
            {
                ptrChannelDependencyBlock.address -= countDeleted;

                this.CopyAddress(ptrChannelDependencyBlock, bytes);
            }
            if ((int)ptrChannelExtensionBlock.address > indexDeleted)
            {
                ptrChannelExtensionBlock.address -= countDeleted;

                this.CopyAddress(ptrChannelExtensionBlock, bytes);
            }
            if ((int)ptrComponentAddress.address > indexDeleted)
            {
                ptrComponentAddress.address -= countDeleted;

                this.CopyAddress(ptrComponentAddress, bytes);
            }
            if ((int)ptrDataBlockSignal.address > indexDeleted)
            {
                ptrDataBlockSignal.address -= countDeleted;

                this.CopyAddress(ptrDataBlockSignal, bytes);
            }
            if ((int)ptrDisplayName.address > indexDeleted)
            {
                ptrDisplayName.address -= countDeleted;

                this.CopyAddress(ptrDisplayName, bytes);
            }
            if ((int)ptrLongSignalName.address > indexDeleted)
            {
                ptrLongSignalName.address -= countDeleted;

                this.CopyAddress(ptrLongSignalName, bytes);
            }
            if ((int)ptrNextChannelBlock.address > indexDeleted)
            {
                ptrNextChannelBlock.address -= countDeleted;

                this.CopyAddress(ptrNextChannelBlock, bytes);
            }
            if ((int)ptrTextBlockChanelName.address > indexDeleted)
            {
                ptrTextBlockChanelName.address -= countDeleted;

                this.CopyAddress(ptrTextBlockChanelName, bytes);
            }
            if ((int)ptrTextBlockComment.address > indexDeleted)
            {
                ptrTextBlockComment.address -= countDeleted;

                this.CopyAddress(ptrTextBlockComment, bytes);
            }
            if ((int)ptrUnit.address > indexDeleted)
            {
                ptrUnit.address -= countDeleted;

                this.CopyAddress(ptrUnit, bytes);
            }
        }
    }
}
