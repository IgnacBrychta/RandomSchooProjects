using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Net;
using System.Diagnostics;

namespace Worldle_MyTake_Console
{
    internal static class Program
    {
        public const int maximalniPocetPokusu = 6;
        public const int fixniDelkaSlov = 5;
        public static int vyhry = 0;
        public static int prohry = 0;
        public static int pocetZadanychNesmyslu = 0;
        static readonly SlovaDatabaze slovaDatabaze = new SlovaDatabaze("wordList.txt", fixniDelkaSlov);
        private static WebClient client = new WebClient();
        static void Main(string[] args)
        {
            while (true)
            {
                Mainloop();
            }
        }
        private static void Mainloop()
        {
            string nahodneSlovo = slovaDatabaze.NahodneSlovo;
            int pocetPokusu = 0;
            string[] tipyHrace = new string[maximalniPocetPokusu];
            bool slovoUhodnuto = false;
            do
            {
                string tipHrace = ZiskatSlovo().ToUpper();
                Console.Clear();
                tipyHrace[pocetPokusu] = tipHrace;
                for (int i = 0; i < pocetPokusu + 1; i++)
                {
                    Console.Write((i+1) + ". ");
                    ZobrazitSpravnaPismenaTipu(tipyHrace[i], nahodneSlovo);
                    Console.WriteLine();
                }
                pocetPokusu++;
                if (tipHrace == nahodneSlovo)
                {
                    slovoUhodnuto = true;
                    break; 
                }
            } while (pocetPokusu != maximalniPocetPokusu);
            if (slovoUhodnuto)
            {
                vyhry++;
                Console.WriteLine("\nGratulujeme!");
            } else
            {
                prohry++;
                Console.WriteLine($"\nCo už.\nSprávné slovo: {nahodneSlovo}");
            }
            Console.WriteLine($"Úspěšná uhodnutí: {vyhry} | Neuhodnutí: {prohry} | Zmotaná slova: {pocetZadanychNesmyslu}");
            Console.ReadKey();
            Console.Clear();
        }
        public static bool JeVlozeneSlovoRealne(string slovo)
        {
            string data = client.DownloadString($"https://prirucka.ujc.cas.cz/?slovo={slovo}");
            FileStream fileStream = new FileStream("xml_data", FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(data);
            streamWriter.Close();
            fileStream.Close();
            bool nenalezeno = data.Contains("nebyl nalezen");
            bool pretizeni = data.Contains("is still evaluated.");
            if (pretizeni)
            {
                Console.WriteLine("Došlo k přetížení, nutno zajít na web UJC a kliknout na \"Nechci čekat na odpověď\".");
                try
                {
                    Process.Start("chrome", "https://prirucka.ujc.cas.cz/"); // beru v potaz, že existuje jen a pouze chrome, ač ho já sám nemám lol
                }
                catch (Exception) { }
                return true;
            }
            return nenalezeno;
        }
        private static void ZobrazitSpravnaPismenaTipu(string tip, string slovo)
        {
            for (int i = 0; i < fixniDelkaSlov; i++)
            {
                if (tip[i] == slovo[i])
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                }
                else if (slovo.Contains(tip[i]))
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    
                }
                    Console.Write(tip[i]);
                    Console.BackgroundColor = ConsoleColor.Black;
            }
        }
        private static string ZiskatSlovo()
        {
            
            string slovo = string.Empty;
            while (true)
            {
                Console.WriteLine($"\nVložte slovo na {fixniDelkaSlov} písmen.");
                slovo = Console.ReadLine();

                if (slovo.Length == fixniDelkaSlov)
                {
                    if (JeVlozeneSlovoRealne(slovo))
                    {
                        Console.WriteLine($"Vloženo nereálné slovo. Co jedeš?");
                        pocetZadanychNesmyslu++;
                    }
                    else
                    {
                        break;
                    }
                } else
                {
                    Console.WriteLine($"Vložené slovo nemá {fixniDelkaSlov} písmen.");
                }
            }
            return slovo;
        }
    } 
    internal class SlovaDatabaze
    {
        private Random random = new Random();
        public string NahodneSlovo { get { return nactenaSlova[random.Next(nactenaSlova.Length)]; } private set { } }
        public string[] nactenaSlova;
        public int fixniDelkaSlov;
        public SlovaDatabaze(string seznamSlov, int fixniDelkaSlov)
        {
            this.fixniDelkaSlov = fixniDelkaSlov;
            NacistSlova(seznamSlov);
        }
        private void NacistSlova(string seznamSlov)
        {
            string[] workingDir = Environment.CurrentDirectory.Split('\\');
            workingDir = workingDir.Take(workingDir.Length - 2).ToArray();
            string fileLocation = string.Join("\\", workingDir);
            FileStream fileStream = new FileStream(fileLocation + "\\" + seznamSlov, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            nactenaSlova = streamReader.ReadToEnd().ToUpper().Split(" | ".ToCharArray());
            nactenaSlova = nactenaSlova.Where(x => x.Length == fixniDelkaSlov).ToArray();
            streamReader.Close();
            fileStream.Close();

        }
    }
}
