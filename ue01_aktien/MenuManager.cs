using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

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
            - IMPORT: import <filename>
            - SEARCH: search <name/kuerzel>
            - PLOT: Die Schlusskurse der letzten 30 Tage einer Aktie als ASCII
            Grafik ausgeben.
            - SAVE: save <filename>
            - LOAD: load <filename>
            - QUIT: Programm beenden";
        
        private const int Tablesize = 2039;
        
        private readonly Hashtable<Share> _nameHashtable = new Hashtable<Share>(Tablesize, (string key, Share share) => key == share.Name);
        private readonly Hashtable<Share> _abbrHashtable = new Hashtable<Share>(Tablesize, (string key, Share share) => key == share.Abbr);
        
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
                    case "save":
                        HandleSave();
                        break;
                    case "load":
                        HandleLoad();
                        break;
                    case "import":
                        HandleImport();
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
                
                _nameHashtable.Add(ref shareToInsert, shareToInsert.Name);
                _abbrHashtable.Add(ref shareToInsert, shareToInsert.Abbr);
                
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

                abbrSearchRes = _abbrHashtable.Search(_cmd.Args[0]);
                if (abbrSearchRes != null)
                {
                    foundAbbrEntry = abbrSearchRes ?? default(Share);
                    Console.WriteLine("Aktie mit dem Kuerzel {0} gefunden!", foundAbbrEntry.Abbr);
                }
                nameSearchRes = _nameHashtable.Search(_cmd.Args[0]);
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

        private void HandleSave()
        {
            string msg = "";

            try
            {
                if (_cmd.Cmd != "save")
                {
                    msg = "Ungültiger Aufruf des Handlers für das SAVE-Kommando.";
                    throw new FormatException(msg);
                }
                else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("./saves/" + _cmd.Args[0] + ".bin", FileMode.Create, FileAccess.Write,
                    FileShare.None);
                formatter.Serialize(stream, _nameHashtable);
                stream.Close();

                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (Exception ex)
            {
                if (ex is FormatException)
                {
                    Console.WriteLine(ex.Message);
                } else if (ex is FileNotFoundException)
                {
                    Console.WriteLine("Datei nicht gefunden.");
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
        }
        
        private void HandleLoad()
        {
            string msg = "";
            Hashtable<Share> readHashtable;
            Share shareToInsert;

            try
            {
                if (_cmd.Cmd != "load")
                {
                    msg = "Ungültiger Aufruf des Handlers für das LOAD-Kommando.";
                    throw new FormatException(msg);
                } else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }
                
                IFormatter formatter = new BinaryFormatter();  
                Stream stream = new FileStream( "./saves/"+_cmd.Args[0]+".bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                readHashtable = (Hashtable<Share>) formatter.Deserialize(stream); 
                stream.Close();

                _nameHashtable.Items = readHashtable.Items;
                _abbrHashtable.Clear();

                for (int i = 0; i < Tablesize; i++)
                {
                    if (readHashtable.Items[i] != null)
                    {
                        shareToInsert = readHashtable.Items[i] ?? default(Share);
                        _abbrHashtable.Add(ref shareToInsert, shareToInsert.Abbr); // TODO: Copies share then refs them?
                        // TODO: seperate folder for saved files?
                    }
                        
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (Exception ex)
            {
                if (ex is FormatException)
                {
                    Console.WriteLine(ex.Message);
                } else if (ex is FileNotFoundException)
                {
                    Console.WriteLine("Datei nicht gefunden.");
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
        }

        public void HandleImport()
        {
            string msg = "";
            Share? searchRes = null;

            try
            {
                if (_cmd.Cmd != "import")
                {
                    msg = "Ungültiger Aufruf des Handlers für das IMPORT-Kommando.";
                    throw new FormatException(msg);
                } else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }

                searchRes = _abbrHashtable.Search(_cmd.Args[0]);
                searchRes = _nameHashtable.Search(_cmd.Args[0]);

                if (searchRes != null)
                {
                    Share share = searchRes ?? default(Share);
                    
                    // SharePrice[] data = share.SharePrices;
                    List<SharePrice> data = new List<SharePrice>(share.SharePrices);
                    TextFieldParser csvParser = new TextFieldParser("./imports/"+_cmd.Args[0]+".csv");
                    csvParser.SetDelimiters(";");

                    csvParser.ReadLine();

                    while (!csvParser.EndOfData)
                    {
                          string[] fields = csvParser.ReadFields();
                          DateTime date = DateTime.Parse(fields[0]);
                          
                          for(int i=0; i < 30; i++)
                          {   
                              if (date < data[i].Date)
                                    data.Insert(i, new SharePrice(date, float.Parse(fields[1]), float.Parse(fields[2]), float.Parse(fields[3]), float.Parse(fields[4]), int.Parse(fields[5]), float.Parse(fields[6])));
                          }
                    }

                    share.SharePrices = data.Take(30).ToArray(); // TODO: use refernce, return ref from search?
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (Exception ex)
            {
                if (ex is FormatException)
                {
                    Console.WriteLine(ex.Message);
                } else if (ex is FileNotFoundException)
                {
                    Console.WriteLine("Datei nicht gefunden.");
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
        }
    }
}