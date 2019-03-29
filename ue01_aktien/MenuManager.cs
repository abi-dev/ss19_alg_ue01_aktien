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
        
        private const int Tablesize = 2039;    // => <50% fill factor, 509*4+3=2039, prime
        
        // two hashtable store references to shares, using the share name and abbr as keys, constructor gets compare func
        private readonly Hashtable<Share> _nameHashtable = new Hashtable<Share>(Tablesize, (key, share) => key == share.Name);
        private readonly Hashtable<Share> _abbrHashtable = new Hashtable<Share>(Tablesize, (key, share) => key == share.Abbr);
        
        private struct Command
        {
            public string Cmd;
            public string[] Args;
            public bool IsChecked; // has the command been parsed?
        }
        
        private Command _cmd; // stores the current command

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
            cmdText = Console.ReadLine(); // read command
            ParseCmd(cmdText);
        }

        private void ParseCmd(string cmdText)
        {
            string msg;
            string[] splitCmdText;
            
            try
            {
                // regex matches everything that has a word of a-z followed by parameters which may use a-z and digits,
                // seperated by whitespaces
                var match = Regex.Match(cmdText, @"^[a-zA-Z]*(\s[a-zA-Z1-9]*)*");
            
                if (!match.Success)
                {
                    msg = "Syntax Error.";
                    throw new FormatException(msg);
                }

                splitCmdText = cmdText.Split(" ");
                _cmd.Cmd = splitCmdText[0].Trim().ToLower(); // no case sensitivity
                _cmd.Args = splitCmdText.Skip(1).ToArray();  // skip first word, which is the command itself
                _cmd.IsChecked = true;    // command has been parsed

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
                if (!_cmd.IsChecked) // command has not been parsed
                {
                    msg = "Es wurde noch kein valides Kommando erfasst.";
                    throw new FormatException(msg);
                }

                switch (_cmd.Cmd) // call the command handler
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
                    case "del":
                        HandleDelete();
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

                // search for the share we want to insert to prevent duplicates
                Share searchRes = _abbrHashtable.Search(_cmd.Args[2]) ?? _nameHashtable.Search(_cmd.Args[0]);

                if (searchRes != null) // share already exists
                {
                    msg = "Es gibt bereits eine Aktie mit diesem Kürzel oder Namen!";
                    throw new FormatException(msg);
                }
                
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

                // search for abbreviation first, then name
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

                // print newest share if there is data for it
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
        
        private void HandleDelete()
        {
            string msg;
            int searchRes;

            try
            {
                if (_cmd.Cmd != "del")
                {
                    msg = "Ungültiger Aufruf des Handlers für das DELETE-Kommando.";
                    throw new FormatException(msg);
                } else if (_cmd.Args.Length != 1)
                {
                    msg = "Ungültige Anzahl an Parametern.";
                    throw new FormatException(msg);
                }
                
                // get res so the share can be deleted from both hashtables
                Share res = _abbrHashtable.Search(_cmd.Args[0]) ?? _nameHashtable.Search(_cmd.Args[0]);

                try
                {
                    // delete from both hashtables
                    searchRes = _nameHashtable.Delete(res.Name); // TODO: return value on first call is not used...
                    searchRes = _abbrHashtable.Delete(res.Abbr);

                    if (searchRes > 0)
                    {
                        Console.WriteLine("Aktie {0} ({1}) erfolgreich gefunden und gelöscht.", res.Name, res.Abbr);
                    }

                    else
                    {
                        if (searchRes == -1)
                        {
                            Console.WriteLine("Hashtable ist leer.");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Aktie {0} konnte nicht gefunden werden. Vergewissern Sie sich, dass die Aktie vorhanden ist und Sie sich nicht verschrieben haben.",
                                _cmd.Args[0]);
                        }
                    }
                }

                catch
                {
                    Console.WriteLine(
                        "Aktie {0} konnte nicht gefunden werden. Vergewissern Sie sich, dass die Aktie vorhanden ist und Sie sich nicht verschrieben haben.", _cmd.Args[0]);
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

                // open file stream and save serialized hashtable object in it (uses name hashtable)
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(System.IO.Directory.GetCurrentDirectory() + "/saves/" + _cmd.Args[0] + ".bin", FileMode.Create, FileAccess.Write,
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
                
                // open file stream and read saved hashtable (nameHashTable)
                IFormatter formatter = new BinaryFormatter();  
                Stream stream = new FileStream(System.IO.Directory.GetCurrentDirectory() + "/saves/"+_cmd.Args[0]+".bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                readHashtable = (Hashtable<Share>) formatter.Deserialize(stream); 
                stream.Close();

                _nameHashtable.Items = readHashtable.Items;
                // clear abbr HT
                _abbrHashtable.Clear();
                
                // reindex abbr HT
                for (int i = 0; i < Tablesize; i++)
                {
                    if (readHashtable.Items[i] != null)
                    {
                        ref Share shareToInsert = ref readHashtable.Items[i];
                        _abbrHashtable.Add(ref shareToInsert, shareToInsert.Abbr);
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

                Share searchRes = _abbrHashtable.Search(_cmd.Args[0]) ?? _nameHashtable.Search(_cmd.Args[0]);

                if (searchRes != null) // found a share for the file name
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
                    
                    // start parsing the file, name must be the same as share name
                    TextFieldParser csvParser = new TextFieldParser(
                        System.IO.Directory.GetCurrentDirectory() + "/imports/"+_cmd.Args[0]+".csv");
                    csvParser.SetDelimiters(",");

                    csvParser.ReadLine(); // do not use column names, skip to data

                    while (!csvParser.EndOfData)
                    {
                          string[] fields = csvParser.ReadFields();
                          DateTime date = DateTime.Parse(fields[0]); // parse date column

                          // save data from current line
                          SharePrice priceToInsert = new SharePrice(date, float.Parse(fields[1]),
                              float.Parse(fields[2]), float.Parse(fields[3]), float.Parse(fields[4]),
                              int.Parse(fields[6]), float.Parse(fields[5]));
                          
                          for(int i=0; i < 30; i++)
                          {
                              if (i >= data.Count) // append if there are not 30 values already
                              {
                                  data.Add(priceToInsert);
                                  break;
                              }

                              if (date > data[i].Date) // insert if date is newer then the saved data
                              {
                                  if(i > 0)
                                      if (date == data[i - 1].Date) // do not insert if we already have data for that day
                                          break;
                                    
                                  data.Insert(i, priceToInsert);
                                  break;
                              }    
                          }
                    }

                    searchRes.SharePrices = data.Take(30).ToArray(); // save the first 30 values to our data structure
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

                Share searchRes = _abbrHashtable.Search(_cmd.Args[0]) ?? _nameHashtable.Search(_cmd.Args[0]);

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
                        
                        // write some data that our plot uses, to give the user context
                        Console.Clear();
                        Console.WriteLine("Name der Aktie: {0}({1}), Schlusskurse", searchRes.Name, searchRes.Abbr);
                        Console.WriteLine("Anzahl Werte: {0}", searchRes.SharePrices.Length);
                        Console.Write("Datum erster Wert: {0}", searchRes.SharePrices[searchRes.SharePrices.Length - 1].Date.ToString("d MMM yyyy"));
                        Console.SetCursorPosition(40, Console.CursorTop);
                        Console.Write("Datum letzter Wert: {0}\n", searchRes.SharePrices[0].Date.ToString("d MMM yyyy"));
                        Console.Write("Max. Wert: {0}", maxValue);
                        Console.SetCursorPosition(40, Console.CursorTop);
                        Console.Write("Min. Wert: {0}\n", minValue);
                        
                        WriteSpacer();

                        // print axes
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

                        // fill graph with data, starting with the newest data on the right side
                        x = startX + 61;
                        y = startY + 10;
                        for (int i = 0; i < searchRes.SharePrices.Length; i++)
                        {
                            x = startX + 61 - i * 2;
                            y = startY + 10;
                            // print bar according to value, normalized to the min and max values
                            int value = (int)(((searchRes.SharePrices[i].Close - minValue) / (maxValue - minValue)) * 10);
                            int endY = y - value;

                            while (y >= endY)
                            {
                                WriteAt(x, y, '█');
                                y--;
                            }
                        }
                        
                        // print axis labels
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