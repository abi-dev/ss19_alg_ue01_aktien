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
        private const string WelcomeText = @"
        Willkommen im ALG Aktien Manager!
        © Benjamin Ableidinger, Michael Brunner
        Ein Projekt im Rahmen der LV Algorithmen und Datenstrukturen.";
        private const string MenuText = @"
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
        
        private readonly Hashtable<Share> _nameHashtable = new Hashtable<Share>(Tablesize, (key, share) => key == share.Name);
        private readonly Hashtable<Share> _abbrHashtable = new Hashtable<Share>(Tablesize, (key, share) => key == share.Abbr);
        
        private struct Command
        {
            public string Cmd;
            public string[] Args;
            public bool IsChecked;
        }
        
        private Command _cmd;

        public void InitMenu()
        {
            _cmd.IsChecked = false;
            
            Console.WriteLine(WelcomeText);
            ShowMenu(false);
        }

        private void ShowMenu(bool clrScreen = true)
        {
            string cmdText;

            if (clrScreen)
            {
                Console.Clear();
            }
            else
            {
                WriteSpacer();
            }
            
            Console.WriteLine(MenuText);
            cmdText = Console.ReadLine();
            ParseCmd(cmdText);
        }

        private void ParseCmd(string cmdText)
        {
            string msg;
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
            string msg;

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
                    case "plot":
                        HandlePlot();
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
            string msg;

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
            string msg;
            Share searchRes;

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

                searchRes = _abbrHashtable.Search(_cmd.Args[0]);
                if (searchRes != null)
                {
                    Console.WriteLine("Aktie durch den Kuerzel {0} gefunden!", searchRes.Abbr);
                }
                else
                {
                    searchRes = _nameHashtable.Search(_cmd.Args[0]);
                    if (searchRes  != null)
                    {
                        Console.WriteLine("Aktie durch den Namen {0} gefunden!", searchRes.Name);
                    }
                }

                if (searchRes?.SharePrices != null)
                {
                    Console.WriteLine("Open: {0}", searchRes.SharePrices[0].Open);
                    Console.WriteLine("High: {0}", searchRes.SharePrices[0].High);
                    Console.WriteLine("Low: {0}", searchRes.SharePrices[0].Low);
                    Console.WriteLine("Close: {0}", searchRes.SharePrices[0].Close);
                    Console.WriteLine("Volume: {0}", searchRes.SharePrices[0].Volume);
                    Console.WriteLine("Adj. Close: {0}", searchRes.SharePrices[0].AdjClose);
                } else if (searchRes != null)
                {
                    Console.WriteLine("Keine Kursdaten gefunden.");
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
            string msg;

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
            string msg;
            Hashtable<Share> readHashtable;

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
                        ref Share shareToInsert = ref readHashtable.Items[i];
                        _abbrHashtable.Add(ref shareToInsert, shareToInsert.Abbr); // TODO: Copies share then refs them?
                    }
                        
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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

        private void HandleImport()
        {
            string msg;

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

                Share searchRes = _abbrHashtable.Search(_cmd.Args[0]);

                if (searchRes == null)
                    searchRes = _nameHashtable.Search(_cmd.Args[0]);

                if (searchRes != null)
                {
                    List<SharePrice> data;
                    if (searchRes.SharePrices != null)
                    {
                        data = new List<SharePrice>(searchRes.SharePrices);
                    }   
                    else
                    {
                        data = new List<SharePrice>(30);
                    }
                    TextFieldParser csvParser = new TextFieldParser("./imports/"+_cmd.Args[0]+".csv");
                    csvParser.SetDelimiters(",");

                    csvParser.ReadLine();

                    while (!csvParser.EndOfData)
                    {
                          string[] fields = csvParser.ReadFields();
                          DateTime date = DateTime.Parse(fields[0]);

                          SharePrice priceToInsert = new SharePrice(date, float.Parse(fields[1]),
                              float.Parse(fields[2]), float.Parse(fields[3]), float.Parse(fields[4]),
                              int.Parse(fields[5]), float.Parse(fields[6]));
                          
                          for(int i=0; i < 30; i++)
                          {
                              if (i >= data.Count)
                              {
                                  data.Add(priceToInsert);
                                  break;
                              }

                              if (date > data[i].Date)
                              {
                                  data.Insert(i, priceToInsert);
                                  break;
                              }    
                          }
                    }

                    searchRes.SharePrices = data.Take(30).ToArray(); // TODO: use refernce, return ref from search?
                }
                else
                {
                    Console.WriteLine("Keine Aktie unter diesem Namen gefunden. Vor dem Importieren muss die Aktie anlegt worden sein.");
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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

        private void HandlePlot()
        {
            string msg;
            int x, y, startX = 10, startY = 6;
            float maxValue = 0, minValue = int.MaxValue;

            try
            {
                if (_cmd.Cmd != "plot")
                {
                    msg = "Ungültiger Aufruf des Handlers für das PLOT-Kommando.";
                    throw new FormatException(msg);
                }
                else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }

                Share searchRes = _abbrHashtable.Search(_cmd.Args[0]);

                if (searchRes == null)
                    searchRes = _nameHashtable.Search(_cmd.Args[0]);

                if (searchRes != null)
                {
                    if (searchRes.SharePrices == null)
                    {
                        Console.WriteLine("Keine Kursdaten gefunden.");
                    }
                    else
                    {
                        // get max and min value
                        for (int i = 0; i < searchRes.SharePrices.Length; i++)
                        {
                            if (searchRes.SharePrices[i].Close > maxValue)
                                maxValue = searchRes.SharePrices[i].Close;

                            if (searchRes.SharePrices[i].Close < minValue)
                                minValue = searchRes.SharePrices[i].Close;
                        }
                        
                        Console.Clear();
                        Console.WriteLine("Name der Aktie: {0}({1}), Schlusskurse", searchRes.Name, searchRes.Abbr);
                        Console.WriteLine("Anzahl Werte: {0}", searchRes.SharePrices.Length);
                        Console.Write("Datum erster Wert: {0}", searchRes.SharePrices[0].Date.ToString("d MMM yyyy"));
                        Console.SetCursorPosition(40, Console.CursorTop);
                        Console.Write("Datum letzter Wert: {0}\n", searchRes.SharePrices[searchRes.SharePrices.Length-1].Date.ToString("d MMM yyyy"));
                        Console.Write("Max. Wert: {0}", maxValue);
                        Console.SetCursorPosition(40, Console.CursorTop);
                        Console.Write("Min. Wert: {0}\n", minValue);
                        
                        WriteSpacer();

                        x = startX;
                        y = startY;
                        Console.SetCursorPosition(x, y);
                        while(y <= startY + 10)
                        {
                            WriteAt(x, y, '│');
                            y++;
                        }
                        WriteAt(x, y, '└');
                        x++;
                        while (x <= startX + 61)
                        {
                            WriteAt(x, y, '─');
                            x++;
                        }

                        x = startX + 61;
                        y = startY + 10;
                        for (int i = 0; i < searchRes.SharePrices.Length; i++)
                        {
                            x = startX + 61 - i * 2;
                            y = startY + 10;
                            int value = (int)(((searchRes.SharePrices[i].Close - minValue) / (maxValue - minValue)) * 10);
                            int endY = y - value;

                            while (y >= endY)
                            {
                                WriteAt(x, y, '█');
                                y--;
                            }
                        }
                        
                        Console.SetCursorPosition(1, 10);
                        Console.Write("Schlussk.");
                        
                        Console.SetCursorPosition(36, 18);
                        Console.Write("Datum");
                        
                        Console.Write("\n\n");
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
                }
                else
                {
                    Console.WriteLine(ex);
                }
                
                Console.WriteLine("Zurück zum Menü mit irgendeiner Taste...");
                Console.ReadKey();
                ShowMenu();
            }
        }

        private void WriteSpacer()
        {
            for (int i=0; i < Console.BufferWidth; i++)
            {
                Console.Write("━");
            }
            Console.WriteLine();
        }

        private void WriteAt(int x, int y, char c)
        {
            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(c);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}