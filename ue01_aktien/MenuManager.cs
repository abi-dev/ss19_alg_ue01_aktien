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
            1. ADD: Eine Aktie mit Namen, WKN und Kürzel hinzufügen.
            2. DEL: Aktie löschen.
            3. IMPORT: Kurswerte für eine Aktie aus einer csv Datei importieren
            4. SEARCH: Eine Aktie in der Hashtabelle suchen (Eingabe von Namen
            oder Kürzel) und den aktuellsten Kurseintrag mit
            (Date,Open,High,Low,Close,Volume,Adj Close) ausgeben.
            5. PLOT: Die Schlusskurse der letzten 30 Tage einer Aktie als ASCII
            Grafik ausgeben.
            6. SAVE <filename>: Hashtabelle in eine Datei speichern
            7. LOAD <filename>: Hashtabelle aus einer Datei laden
            8. QUIT: Programm beenden";

        private Hashtable nameHashtable = new Hashtable(2039);
        private Hashtable abbrHashtable = new Hashtable(2039);
        
        private struct Command
        {
            public string Cmd;
            public string[] Args;
            public bool IsChecked;
        }
        
        private Command _cmd;
        
        public MenuManager()
        {
            
        }

        public void InitMenu()
        {
            _cmd.IsChecked = false;
            
            Console.WriteLine(Welcome);
            ShowMenu();
        }

        private void ShowMenu()
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
                    msg = "Syntax Error.";
                    throw new FormatException(msg);
                }

                splitCmdText = cmdText.Split(" ");
                _cmd.Cmd = splitCmdText[0].Trim().ToLower();
                _cmd.Args = splitCmdText.Skip(1).ToArray();
                _cmd.IsChecked = true;

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
                if (!_cmd.IsChecked)
                {
                    msg = "Es wurde noch kein valides Kommando erfasst.";
                    throw new FormatException(msg);
                }

                switch (_cmd.Cmd)
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
                if (_cmd.Cmd != "add")
                {
                    msg = "Ungültiger Aufruf des Handlers für das ADD-Kommando.";
                    throw new FormatException(msg);
                } else if (_cmd.Args.Length != 3)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }
                
                Share shareToInsert = new Share();
                shareToInsert.Name = _cmd.Args[0];
                shareToInsert.Id = _cmd.Args[1];
                shareToInsert.Abbr = _cmd.Args[2];
                
                nameHashtable.Add(ref shareToInsert, shareToInsert.Name);
                abbrHashtable.Add(ref shareToInsert, shareToInsert.Abbr);
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            
        }
    }
}