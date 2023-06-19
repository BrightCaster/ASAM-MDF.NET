using System;
using System.Text;

namespace ASAM.MDF.Libary
{
    using ASAM.MDF.Libary.Types;

    public class ChannelExtensionBlock : Block
    {
        public ExtensionType Type { get; private set; }
        public DimBlockSupplement DimBlockSupplement { get; private set; }
        public VectorCanBlockSupplement VectorCanBlockSupplement { get; private set; }

        public ChannelExtensionBlock(Mdf mdf, ulong position)
          : base(mdf)
        {
            mdf.UpdatePosition(position);

          Type = (ExtensionType)mdf.ReadU16();

          if (Type == ExtensionType.DIM)
          {
            DimBlockSupplement = new DimBlockSupplement(mdf);
          }
          else if (Type == ExtensionType.VectorCAN)
          {
            VectorCanBlockSupplement = new VectorCanBlockSupplement(mdf);
          }
        }
    }
}
