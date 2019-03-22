using System;

namespace ue01_aktien
{
    class Program
    {
        private static readonly MenuManager MenuManager = new MenuManager();
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            MenuManager.InitMenu();
        }
    }
}