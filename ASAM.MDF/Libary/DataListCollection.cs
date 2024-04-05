using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASAM.MDF.Libary
{
    public class DataListCollection : IList<DataList>
    {
        private List<DataList> items = new List<DataList>();
        public DataListCollection(Mdf mdf, DataGroupBlock dataGroupBlock)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Mdf = mdf;
            DataGroupBlock = dataGroupBlock;
        }

        public Mdf Mdf { get; private set; }
        public DataGroupBlock DataGroupBlock { get; internal set; }
        public int Count
        {
            get { return items.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public DataList this[int index]
        {
            get { return this.items[index]; }
            set { throw new NotImplementedException(); }
        }

        internal void Read(DataList datalistBlock)
        {
            items = Common.BuildBlockList(null, datalistBlock);
        }
        

        public int IndexOf(DataList item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, DataList item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public void Add(DataList item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(DataList item)
        {
            return items.Contains(item);
        }
        public void CopyTo(DataList[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public bool Remove(DataList item)
        {
            return items.Remove(item);
        }

        public IEnumerator<DataList> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public DataListCollection Clone(Mdf mdf)
        {
            var dlc = MemberwiseClone() as DataListCollection;
            var list = new List<DataList>();
            foreach ( DataList item in items )
            {
                list.Add(item.Clone(mdf) as DataList);
            }
            dlc.items = list;
            dlc.Mdf = mdf;
            //dlc.DataGroupBlock = DataGroupBlock?.Clone(mdf) as DataGroupBlock;

            return dlc;
        }
    }
}