// Copyright (c) Valdis Iljuconoks. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DbLocalizationProvider.AdminUI
{
    public class DirtyList<T> : ICollection<T>, IDirtyList
    {
        private readonly List<T> _actualList = new List<T>();

        public DirtyList(params T[] items)
        {
            if(items != null) _actualList.AddRange(items.ToList());
        }

        public IEnumerator<T> GetEnumerator() => _actualList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _actualList.Add(item);
            IsDirty = true;
        }

        public void Clear()
        {
            _actualList.Clear();
            IsDirty = true;
        }

        public bool Contains(T item) => _actualList.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            _actualList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) => _actualList.Remove(item);

        public int Count => _actualList.Count;

        public bool IsReadOnly => false;

        public bool IsDirty { get; private set; }
    }
}
