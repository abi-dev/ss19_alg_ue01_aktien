using System;

namespace ue01_aktien
{
    public class Hashtable
    {
        public readonly int Size;

        private Share[] _shares;
        
        public Hashtable(int size)
        {
            Size = size;
            _shares = new Share[size];
        }

        public void Add(Share share)
        {
            throw new NotImplementedException();
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