namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class DataGroupCollection : IList<DataGroupBlock>
    {
        private List<DataGroupBlock> items = new List<DataGroupBlock>();

        public DataGroupCollection(Mdf mdf)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Mdf = mdf;
        }

        public Mdf Mdf { get; private set; }
        
        internal void Read(DataGroupBlock block)
        {
            items = Common.BuildBlockList(null, block);
        }
        internal void Write(byte[] array, ref int index)
        {
            DataGroupBlock prev = null;
            int prevIndex = 0;
            for (int i = 0; i < Count; i++)
            {
                var block = items[i];

                if (prev != null)
                    prev.WriteNextBlockLink(array, index, prevIndex);

                prev = block;
                prevIndex = index;

                block.Write(array, ref index);
            }
        }

        // IList.
        public int IndexOf(DataGroupBlock item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, DataGroupBlock item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public DataGroupBlock this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }
        public void Add(DataGroupBlock item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(DataGroupBlock item)
        {
            return items.Contains(item);
        }
        public void CopyTo(DataGroupBlock[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get { return items.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(DataGroupBlock item)
        {
            return items.Remove(item);
        }
        public IEnumerator<DataGroupBlock> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
