﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace Blueprint41.Core
{
    /// <summary>
    /// Dictionary with fast read and not too bad write performance. Use if you have many reads and few writes or updates, eg. caching.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AtomicDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private int readerCount = 0;
        private int awaitingWrite = 0;
        private object syncLock = new object();

        private readonly IDictionary<TKey, TValue> dictionary;

        public AtomicDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }
        public AtomicDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(comparer);
        }
        public AtomicDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
        }
        public AtomicDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public void Read(Action<IDictionary<TKey, TValue>> logic)
        {
            if (awaitingWrite != 0)
            {
                lock (syncLock)
                    ReadInternal();
            }
            else
            {
                ReadInternal();
            }

            void ReadInternal()
            {
                Interlocked.Increment(ref readerCount);
                try
                {
                    logic.Invoke(dictionary);
                }
                finally
                {
                    Interlocked.Decrement(ref readerCount);
                }
            }
        }
        public T Read<T>(Func<IDictionary<TKey, TValue>, T> logic)
        {
            if (awaitingWrite != 0)
            {
                lock (syncLock)
                    return ReadInternal();
            }
            else
            {
                return ReadInternal();
            }

            T ReadInternal()
            {
                Interlocked.Increment(ref readerCount);
                try
                {
                    return logic.Invoke(dictionary);
                }
                finally
                {
                    Interlocked.Decrement(ref readerCount);
                }
            }
        }
        public void Write(Action<IDictionary<TKey, TValue>> logic)
        {
            lock (syncLock)
            {
                Interlocked.Increment(ref awaitingWrite);
                try
                {
                    while (readerCount != 0)
                        Thread.Yield();

                    logic.Invoke(dictionary);
                }
                finally
                {
                    Interlocked.Decrement(ref awaitingWrite);
                }
            }
        }
        public T Write<T>(Func<IDictionary<TKey, TValue>, T> logic)
        {
            lock (syncLock)
            {
                Interlocked.Increment(ref awaitingWrite);
                try
                {
                    while (readerCount != 0)
                        Thread.Yield();

                    return logic.Invoke(dictionary);
                }
                finally
                {
                    Interlocked.Decrement(ref awaitingWrite);
                }
            }
        }
        public void ReadOptionalWrite(ReadLogic readLogic, Action<IDictionary<TKey, TValue>> writeLogic)
        {
            bool executeWrite = false;
            Read(readDict => readLogic.Invoke(readDict, out executeWrite));
            if (executeWrite)
            {
                Write(delegate (IDictionary<TKey, TValue> writeDict)
                {
                    readLogic.Invoke(writeDict, out executeWrite);
                    if (executeWrite)
                        writeLogic.Invoke(writeDict);
                });
            }
        }
        public T ReadOptionalWrite<T>(ReadLogic<T> readLogic, Func<IDictionary<TKey, TValue>, T> writeLogic)
        {
            bool executeWrite = false;
            T result = Read(readDict => readLogic.Invoke(readDict, out executeWrite));
            if (executeWrite)
            {
                Write(delegate (IDictionary<TKey, TValue> writeDict)
                {
                    result = readLogic.Invoke(writeDict, out executeWrite);
                    if (executeWrite)
                        result = writeLogic.Invoke(writeDict);
                });
            }
            return result;
        }
        public delegate void ReadLogic(IDictionary<TKey, TValue> dictionary, out bool executeWrite);
        public delegate T ReadLogic<T>(IDictionary<TKey, TValue> dictionary, out bool executeWrite);

        public TValue this[TKey key]
        {
            get => Read(dict => dict[key]);
            set => Write(dict => dict[key] = value);
        }

        public bool IsReadOnly => false;
        public int Count => Read(dict => dict.Count);
        public bool Contains(KeyValuePair<TKey, TValue> item) => Read(dict => dict.Contains(item));
        public bool ContainsKey(TKey key) => Read(dict => dict.ContainsKey(key));
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Read(dict => dict.CopyTo(array, arrayIndex));
        public void Add(TKey key, TValue value) => Write(dict => dict.Add(key, value));
        public void Add(KeyValuePair<TKey, TValue> item) => Write(dict => dict.Add(item));
        public void Add(IEnumerable<KeyValuePair<TKey, TValue>> items) => Write(dict => items.ForEach(item => AddIfNotExists(dict, item)));
        public bool Remove(TKey key) => Write(dict => dict.Remove(key));
        public bool Remove(KeyValuePair<TKey, TValue> item) => Write(dict => dict.Remove(item));
        public void Remove(IEnumerable<TKey> keys) => Write(dict => keys.ForEach(key => dict.Remove(key)));
        public void Clear() => Write(dict => dict.Clear());

        public ICollection<TKey> Keys => Read(dict => dict.Keys);
        public ICollection<TValue> Values => Read(dict => dict.Values);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Read(dict => dict.GetEnumerator());
        IEnumerator IEnumerable.GetEnumerator() => Read(dict => dict.GetEnumerator());

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool retval = false;

            value = Read(dict =>
            {
                retval = dict.TryGetValue(key, out TValue v);
                return v;
            });

            return retval;
        }
        public TValue TryGetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return ReadOptionalWrite(delegate (IDictionary<TKey, TValue> dictRead, out bool executeWrite)
            {
                executeWrite = !dictRead.TryGetValue(key, out TValue value);
                return value;

            }, delegate (IDictionary<TKey, TValue> dictWrite)
            {
                TValue value = valueFactory.Invoke(key);
                dictWrite.Add(key, value);
                return value;
            });
        }
        public TValue TryGetOrAdd(TKey key, Func<TKey, IEnumerable<KeyValuePair<TKey, TValue>>> valueFactory)
        {
            return ReadOptionalWrite(delegate (IDictionary<TKey, TValue> dictRead, out bool executeWrite)
            {
                executeWrite = !dictRead.TryGetValue(key, out TValue value);
                return value;

            }, delegate (IDictionary<TKey, TValue> dictWrite)
            {
                IEnumerable<KeyValuePair<TKey, TValue>> values = valueFactory.Invoke(key);
                values.ForEach(item => dictWrite.Add(item.Key, item.Value));
                return dictWrite[key]; 
            });
        }

        private void AddIfNotExists(IDictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvp)
        {
            if (!dict.ContainsKey(kvp.Key))
                dict.Add(kvp.Key, kvp.Value);
        }
    }
}
