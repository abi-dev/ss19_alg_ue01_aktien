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
            - ADD: add <name> <wkn> <kuerzel>
            - DEL: del <name/kuerzel>
            - IMPORT: Kurswerte für eine Aktie aus einer csv Datei importieren
            - SEARCH: search <name/kuerzel>
            - PLOT: Die Schlusskurse der letzten 30 Tage einer Aktie als ASCII
            Grafik ausgeben.
            - SAVE <filename>: Hashtabelle in eine Datei speichern
            - LOAD <filename>: Hashtabelle aus einer Datei laden
            - QUIT: Programm beenden";

        private Hashtable<Share> nameHashtable = new Hashtable<Share>(2039, (string key, Share share) => key == share.Name);
        private Hashtable<Share> abbrHashtable = new Hashtable<Share>(2039, (string key, Share share) => key == share.Abbr);
        
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
                    case "search":
                        HandleSearch();
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

        private void HandleSearch()
        {
            string msg = "";
            Share? abbrSearchRes = null, nameSearchRes = null;
            Share foundAbbrEntry, foundNameEntry;

            try
            {
                if (_cmd.Cmd != "search")
                {
                    msg = "Ungültiger Aufruf des Handlers für das SEARCH-Kommando.";
                    throw new FormatException(msg);
                } else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }

                abbrSearchRes = abbrHashtable.Search(_cmd.Args[0]);
                if (abbrSearchRes != null)
                {
                    foundAbbrEntry = abbrSearchRes ?? default(Share);
                    Console.WriteLine("Aktie mit dem Kuerzel {0} gefunden!", foundAbbrEntry.Abbr);
                }
                nameSearchRes = nameHashtable.Search(_cmd.Args[0]);
                if (nameSearchRes != null)
                {
                    foundNameEntry = nameSearchRes ?? default(Share);
                    Console.WriteLine("Aktie mit dem Namen {0} gefunden!", foundNameEntry.Name);
                }
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