namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class ChannelCollection : IList<ChannelBlock>
    {
        private List<ChannelBlock> items = new List<ChannelBlock>();

        public ChannelCollection(Mdf mdf)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");
        }

        public Mdf Mdf { get; private set; }
        public int Count
        {
            get { return items.Count; }
        }
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public ChannelBlock this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal void Read(ChannelBlock cnBlock)
        {
            items = Common.BuildBlockList(null, cnBlock);
        }
        internal void Write(byte[] array, ref int index)
        {
            ChannelBlock prev = null;
            int prevIndex = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var block = items[i];
                
                if (prev != null)
                    prev.WriteNextChannelLink(array, index, prevIndex);

                prev = block;
                prevIndex = index;

                block.Write(array, ref index);
                block.WriteChannelConversion(array, ref index, prevIndex);
            }
        }

        public int IndexOf(ChannelBlock item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, ChannelBlock item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public void Add(ChannelBlock item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(ChannelBlock item)
        {
            return items.Contains(item);
        }
        public void CopyTo(ChannelBlock[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public bool Remove(ChannelBlock item)
        {
            return items.Remove(item);
        }

        public IEnumerator<ChannelBlock> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public ChannelCollection Clone(Mdf mdf)
        {
            var Cc = MemberwiseClone() as ChannelCollection;
            var list = new List<ChannelBlock>();

            foreach (var item in items) 
            {
                list.Add(item.Clone(mdf) as ChannelBlock);
            }
            Cc.items = list;
            Cc.Mdf = mdf;
            return Cc;
        }
    }
}
