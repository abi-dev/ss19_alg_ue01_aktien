using System;

namespace ue01_aktien
{
    [Serializable]
    public class Hashtable<T> where T: struct
    {
        private readonly int _size;

        private T?[] _items;
        
        private T? _nullItem = new T();

        public T?[] Items
        {
            get => _items;
            set => _items = value;
        }        


        [NonSerialized]
        private Func<string, T, bool> _cmpItemToKey;
        
        public Hashtable(int size, Func<string, T, bool> cmpItemToKey)
        {
            _size = size;
            _items = new T?[size];
            _cmpItemToKey = cmpItemToKey;
        }

        public void Add(ref T item, string key)
        {
            int initialIndex = 0, currentIndex = 0, i = 1;
            string msg = "";

            try
            {
                initialIndex = Hash(key);
                currentIndex = initialIndex;

                while (_items[currentIndex] != null)
                {
                    if (i >= _size)
                    {
                        msg = "Hashtabelle voll.";
                        throw new FormatException(msg);
                    }

                    Console.WriteLine("Kollision am Index: {0}!", currentIndex);
                    currentIndex = (initialIndex + i * i) % _size;
                    i++;
                }

                _items[currentIndex] = item;

                Console.WriteLine(currentIndex);
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public ref T? Search(string key)
        {
            int initialIndex = 0, currentIndex = 0;
            initialIndex = Hash(key);
            currentIndex = initialIndex;

            for (int i=1; i < _size; i++)
            {
                if (_cmpItemToKey(key, _items[currentIndex] ?? default(T)))
                    return ref _items[currentIndex];

                currentIndex = (initialIndex + i * i) % _size;
            }

            return ref _nullItem;
        }

        public void Clear()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = null;
            }
        }

        public int Hash(string key)
        {
            int hashCode = 0, a = 127;

            for(int i=0; i < key.Length; i++)
            {
                hashCode = (key[i] + a * hashCode) % _size;
            }
            
            return hashCode;
        }
    }

    [Serializable]
    public class Share
    {
        public string Name;
        public string Id; // =WKN
        public string Abbr;
        public SharePrice[] SharePrices;
    }

    [Serializable]
    public class SharePrice
    {
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public int Volume;
        public float AdjClose;

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