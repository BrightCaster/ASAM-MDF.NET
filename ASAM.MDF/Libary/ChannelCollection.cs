namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class ChannelCollection : IList<ChannelBlock>
    {
        private ChannelBlock first;
        private List<ChannelBlock> lChannelBlock;

        public ChannelCollection(Mdf mdf, ChannelBlock cnBlock)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            if (cnBlock == null)
                throw new ArgumentNullException("cnBlock");
            Mdf = mdf;

            first = cnBlock;
            lChannelBlock = Common.BuildBlockList(this.lChannelBlock, this.first);
        }

        public Mdf Mdf { get; private set; }
        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public ChannelBlock this[int index]
        {
            get
            {
                return this.lChannelBlock[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int IndexOf(ChannelBlock item)
        {
            throw new NotImplementedException();
        }
        public void Insert(int index, ChannelBlock item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        public void Add(ChannelBlock item)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public bool Contains(ChannelBlock item)
        {
            return lChannelBlock.Contains(item);
        }
        public void CopyTo(ChannelBlock[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public bool Remove(ChannelBlock item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ChannelBlock> GetEnumerator()
        {
            return this.lChannelBlock.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
