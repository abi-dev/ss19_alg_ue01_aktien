using System;

namespace ue01_aktien
{
    [Serializable]
    public class Hashtable<T> where T : class, new()
    {
        [NonSerialized] private readonly Func<string, T, bool> _cmpItemToKey;

        private readonly int _size;

        private T[] _items;

        private T _nullItem; // if we need to return a ref to null, is there a cleaner way?

        public Hashtable(int size, Func<string, T, bool> cmpItemToKey)
        {
            _size = size;
            _items = new T[size];
            _cmpItemToKey = cmpItemToKey;
        }

        public T[] Items
        {
            get => _items;
            set => _items = value;
        }

        public void Add(ref T item, string key)
        {
            int initialIndex, currentIndex, i = 1;
            string msg;

            try
            {
                initialIndex = Hash(key); // get the original index according to the hash function
                currentIndex = initialIndex;

                while (_items[currentIndex] != null) // search until we find an empty bucket
                {
                    if (i >= _size) // no empty buckets to find
                    {
                        msg = "Hashtabelle voll.";
                        throw new FormatException(msg);
                    }

                    Console.WriteLine("Kollision am Index: {0}!", currentIndex);
                    currentIndex = (initialIndex + i * i) % _size; // get next index to look at
                    i++;
                }

                _items[currentIndex] = item; // found an empty bucket, insert item

                Console.WriteLine(currentIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (ex is FormatException)
                    Console.WriteLine(ex.Message);
            }
        }

        public ref T Search(string key)
        {
            int initialIndex, currentIndex;
            initialIndex = Hash(key);
            currentIndex = initialIndex;

            if (_items[initialIndex] == null) // initial bucket is empty -> nothing is saved using this key
                return ref _nullItem;

            for (var i = 1; i < _size; i++)
            {
                if (_cmpItemToKey(key, _items[currentIndex])) // check if the desired item is at this key
                    return ref _items[currentIndex];

                currentIndex = (initialIndex + i * i) % _size; // get next index to look at
            }

            return ref _nullItem; // nothing found
        }

        public int Delete(string key)
        {
            int initialIndex, currentIndex, recentIndex, deletedIndex;
            initialIndex = Hash(key);
            currentIndex = initialIndex;
            recentIndex = initialIndex;

            if (_items[initialIndex] == null)
                return -1; // no item with this key in table

            for (var i = 1; i < _size; i++)
            {
                if (_cmpItemToKey(key, _items[currentIndex]))
                {
                    // found item we want to delete:
                    deletedIndex = currentIndex;
                    _items[currentIndex] = null; // delete item
                    // look for next possible key:
                    recentIndex = currentIndex; // recentIndex is now where we deleted our entry

                    // go to next index:
                    currentIndex = (initialIndex + i * i) % _size;

                    if (_items[currentIndex] != null)
                    {
                        // there were item after our bucket due to collisions, need to move forward

                        while (currentIndex <= _size && _items[currentIndex] != null) // move each item forward
                        {
                            _items[recentIndex] = _items[currentIndex]; //1
                            recentIndex = currentIndex; //2
                            currentIndex = (initialIndex + i * i) % _size; //3
                            i++;
                        }

                        _items[recentIndex] = null; // delete the last item (has already been moved)

                        return deletedIndex;
                    }

                    // item found, no items need to be moved
                    return deletedIndex;
                }

                currentIndex = (initialIndex + i * i) % _size;
            }

            return -2;
        }

        public void Clear()
        {
            for (var i = 0; i < _items.Length; i++) _items[i] = null;
        }

        public int Hash(string key)
        {
            int hashCode = 0, a = 127;

            for (var i = 0; i < key.Length; i++)
                // weigh previous letter codes with a and add the new one, stay within table size
                hashCode = (key[i] + a * hashCode) % _size;

            return hashCode;
        }
    }

    [Serializable]
    public class Share
    {
        public string Abbr;
        public string Id; // =WKN
        public string Name;
        public SharePrice[] SharePrices;
    }

    [Serializable]
    public class SharePrice
    {
        public float AdjClose;
        public float Close;
        public DateTime Date;
        public float High;
        public float Low;
        public float Open;
        public int Volume;

        public SharePrice(DateTime date, float open, float high, float low, float close, int volume, float adjClose)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            AdjClose = adjClose;
        }
    }
}