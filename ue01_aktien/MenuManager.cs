using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ue01_aktien
{
    public class MenuManager
    {
        private const string Welcome = @"
        Willkommen im ALG Aktien Manager!
        © Benjamin Ableidinger, Michael Brunner
        Ein Projekt im Rahmen der LV Algorithmen und Datenstrukturen.";
        private const string Menu = @"
        -----------------------------------------------------------------------------
        Menü:
            1. ADD: Eine Aktie mit Namen, WKN und Kürzel wird hinzugefügt.
            2. DEL: Aktie wird gelöscht.
            3. IMPORT: Kurswerte für eine Aktie werden aus einer csv Datei importiert
            4. SEARCH: Eine Aktie wird in der Hashtabelle gesucht (Eingabe von Namen
            oder Kürzel) und der aktuellste Kurseintrag
            (Date,Open,High,Low,Close,Volume,Adj Close) wird ausgegeben.
            5. PLOT: Die Schlusskurse der letzten 30 Tage einer Aktie werden als ASCII
            Grafik ausgegeben, Format ist frei wählbar.
            6. SAVE <filename>: Programm speichert die Hashtabelle in eine Datei ab
            7. LOAD <filename>: Programm lädt die Hashtabelle aus einer Datei
            8. QUIT: Programm wird beendet";

        private struct Command
        {
            public string cmd;
            public string[] args;
            public bool isChecked;
        }
        
        private Command _cmd;
        
        public MenuManager()
        {
            
        }

        public void InitMenu()
        {
            _cmd.isChecked = false;
            
            Console.WriteLine(Welcome);
            ShowMenu();
        }

        public void ShowMenu()
        {
            string cmdText = "";
            
            Console.WriteLine(Menu);
            cmdText = Console.ReadLine();
            ParseCmd(cmdText);
        }

        private void ParseCmd(string cmdText)
        {
            string msg = "";
            string[] splitCmdText;
            
            try
            {
                var match = Regex.Match(cmdText, @"^[a-zA-Z]*(\s[a-zA-Z]*)*");
            
                if (!match.Success)
                {
                    msg = "\nSyntax Error.";
                    throw new FormatException(msg);
                }

                splitCmdText = cmdText.Split(" ");
                _cmd.cmd = splitCmdText[0].Trim().ToLower();
                _cmd.args = splitCmdText.Skip(1).ToArray();
                _cmd.isChecked = true;

                HandleCmd();
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
        }

        private void HandleCmd()
        {
            string msg = "";

            try
            {
                if (!_cmd.isChecked)
                {
                    msg = "Es wurde noch kein valides Kommando erfasst.";
                    throw new FormatException(msg);
                }

                switch (_cmd.cmd)
                {
                    case "add":
                        HandleAdd();
                        break;
                    case "quit":
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Kommando nicht gefunden.");
                        Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                        Console.ReadKey();
                        ShowMenu();
                        break;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            
        }

        private void HandleAdd()
        {
            string msg = "";

            try
            {
                if (_cmd.cmd != "add")
                {
                    msg = "Ungültiger Aufruf des Handlers für das ADD-Kommando.";
                    throw new FormatException(msg);
                }
                
                throw new NotImplementedException();
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}