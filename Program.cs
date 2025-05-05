
using System;
            using System.Collections.Generic;
            using System.Globalization;
            using System.IO;
            using System.Linq;

namespace CovidDataAnalyzer
    {
        public class CovidData
        {
            public DateTime Meldedatum { get; set; }
            public string Altersgruppe { get; set; }
            public int Bevoelkerung { get; set; }
            public int FaelleGesamt { get; set; }
            public int FaelleNeu { get; set; }
            public int Faelle7Tage { get; set; }
            public double Inzidenz7Tage { get; set; }
        }

        class Program
        {
            private static List<CovidData> _data = new List<CovidData>();

            static void Main(string[] args)
            {
                LoadData();
                ShowMainMenu();
            }

            static void LoadData()
            {
                try
                {
                    using var reader = new StreamReader("C:\\Users\\maxik\\Downloads\\COVID-19-Faelle_7-Tage-Inzidenz_Deutschland.csv");
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        if (values.Length >= 7)
                        {
                            _data.Add(new CovidData
                            {
                                Meldedatum = DateTime.ParseExact(
                                    values[0],
                                    "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture
                                ),
                                Altersgruppe = values[1],
                                Bevoelkerung = int.Parse(values[2]),
                                FaelleGesamt = int.Parse(values[3]),
                                FaelleNeu = int.Parse(values[4]),
                                Faelle7Tage = int.Parse(values[5]),
                                Inzidenz7Tage = double.Parse(values[6], CultureInfo.InvariantCulture)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Lesen der Datei: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            static void ShowMainMenu()
            {
                while (true)
                {
                    Console.WriteLine("1 - Gesamte Inhalte tabellarisch ausgeben");
                    Console.WriteLine("2 - Durchschnittswerte pro Monat tabellarisch ausgeben");
                    Console.WriteLine("3 - Tage mit den jeweils höchsten Kennzahlen ausgeben");
                    Console.Write("> ");

                    if (!int.TryParse(Console.ReadLine(), out var choice))
                    {
                        Console.WriteLine("Ungültige Eingabe!");
                        continue;
                    }

                    string result = choice switch
                    {
                        1 => GenerateFullTable(),
                        2 => GenerateMonthlyAverage(),
                        3 => GenerateMaxIncidences(),
                        _ => "Ungültige Option!"
                    };

                    Console.WriteLine(result);

                    Console.Write("\nAusgabe als Textdatei speichern? (J/N) ");
                    var save = Console.ReadLine().Trim().ToUpper() == "J";

                    if (save)
                    {
                        SaveResult(result);
                    }
                }
            }

            static string GenerateFullTable()
            {
                var table = $"{"Datum",-12} {"Altersgruppe",-12} {"Inzidenz",10}\n";
                table += new string('-', 37) + "\n";

                foreach (var entry in _data)
                {
                    table += $"{entry.Meldedatum:dd.MM.yyyy} {entry.Altersgruppe,-12} {entry.Inzidenz7Tage,10:N2}\n";
                }
                return table;
            }

            static string GenerateMonthlyAverage()
            {
                var monthlyData = _data
                    .GroupBy(d => new { d.Meldedatum.Year, d.Meldedatum.Month })
                    .Select(g => new
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Average = g.Average(d => d.Inzidenz7Tage)
                    })
                    .OrderBy(x => x.Month);

                var table = $"{"Monat",-10} {"Durchschnitts-Inzidenz",20}\n";
                table += new string('-', 30) + "\n";

                foreach (var month in monthlyData)
                {
                    table += $"{month.Month}{month.Average,20:N2}\n";
                }
                return table;
            }

            static string GenerateMaxIncidences()
            {
                var maxIncidence = _data.Max(d => d.Inzidenz7Tage);
                var maxDays = _data.Where(d => d.Inzidenz7Tage == maxIncidence);

                var table = $"{"Datum",-12} {"Altersgruppe",-12} {"Höchste Inzidenz",15}\n";
                table += new string('-', 40) + "\n";

                foreach (var day in maxDays)
                {
                    table += $"{day.Meldedatum:dd.MM.yyyy} {day.Altersgruppe,-12} {day.Inzidenz7Tage,15:N2}\n";
                }
                return table;
            }

            static void SaveResult(string content)
            {
                var fileName = $"covid_analysis_{DateTime.Now:yyyyMMddHHmmss}.txt";
                try
                {
                    File.WriteAllText($"C:\\Users\\maxik\\source\\repos\\Gesundheits_ding\\{fileName}", content);
                    Console.WriteLine($"Datei '{fileName}' erfolgreich gespeichert!\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Speichern fehlgeschlagen: {ex.Message}\n");
                }
            }
        }
    }