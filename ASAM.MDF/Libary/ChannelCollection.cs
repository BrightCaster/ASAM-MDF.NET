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
            for (int i = 0; i < items.Count; i++)
                items[i].Write(array, ref index);
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
    }
}
