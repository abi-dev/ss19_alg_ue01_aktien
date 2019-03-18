using System;

namespace ue01_aktien
{
    public class Hashtable<T> where T: struct
    {
        private readonly int _size;

        private readonly T?[] _items;

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

        public T? Search(string key)
        {
            int initialIndex = 0, currentIndex = 0;
            initialIndex = Hash(key);
            currentIndex = initialIndex;

            for (int i=1; i < _size; i++)
            {
                if (_cmpItemToKey(key, _items[currentIndex] ?? default(T)))
                    return _items[currentIndex];

                currentIndex = (initialIndex + i * i) % _size;
            }

            return null;
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

    public struct Share
    {
        public string Name;
        public string Id; // =WKN
        public string Abbr;
        public SharePrice[] SharePrices;
    }

    public struct SharePrice
    {
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public int Volume;
        public float AdjClose;
    }
}