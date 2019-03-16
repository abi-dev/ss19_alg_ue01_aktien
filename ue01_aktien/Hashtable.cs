using System;

namespace ue01_aktien
{
    public class Hashtable
    {
        private readonly int _size;

        private readonly Share[] _shares;
        
        public Hashtable(int size)
        {
            _size = size;
            _shares = new Share[size];
        }

        public void Add(ref Share share, string key)
        {
            int hashtableIndex = 0;

            hashtableIndex = Hash(key);
            _shares[hashtableIndex] = share;
            
            Console.WriteLine(hashtableIndex);
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