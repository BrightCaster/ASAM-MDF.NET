using System;
using System.Reflection.Metadata.Ecma335;

namespace ASAM.MDF.Libary
{
    /// <summary>
    /// TODO: Complete TriggerBlock
    /// </summary>
    public class TriggerBlock : Block
    {
        public TextBlock Comment { get; private set; }
        public UInt16 Events { get; private set; }

        public TriggerBlock(Mdf mdf)
          : base(mdf)
        {
        }
        public override Block Clone(Mdf mdf)
        {
            var tr = base.Clone(mdf) as TriggerBlock;
            tr.Comment = Comment.Clone(mdf) as TextBlock;

            return tr;
        }
    }
}
