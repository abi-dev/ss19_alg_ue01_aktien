using System;
using System.Text;

namespace ue01_aktien
{
    internal class Program
    {
        private static readonly MenuManager MenuManager = new MenuManager();

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            MenuManager.InitMenu();
        }
    }
}