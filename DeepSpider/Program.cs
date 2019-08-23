/*
 *     Copyright (C) 2019  Leonhard Seidel
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DeepSpider
{
    internal class Spider
    {
        //settings and variables
        private static bool badR;
        private static bool connect;
        private static bool darknet = false;
        private static bool update;
        private static char itemarg;
        private static char[] splitChar = new char[1];
        private static int amount;
        private static int counter = 0;
        private static int urlLengthMax = 10;
        private static int urlLengthMin = 6;
        private static string location;
        private static string parameters;
        private static string title = "";
        private static string tld = ".com";
        private static string url = "https://www.reddit.com";
        private static readonly string connection_string = "Server=localhost;Database=mathebot_deepcrawler;Uid=root;Pwd=;";
        private static Random randomNr = new Random();
        private static Regex regEx = new Regex("([a - zA - Z]:(\\w +) *\\[a-zA-Z0_9]+)?.txt$");
        private static TimeSpan span;
        private static TextReader textReader;

        //add 's' for special chars, 'n' for numbers and 'l' for letters
        private static string complexity = "l";

        //main method
        private static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                parameters += arg;
            }
            span = TimeSpan.FromSeconds(10);
            UserInteraction();
            Console.ReadLine();
        }

        private static void UserInteraction()
        {
            //user interaction            
            splitChar[0] = '-';
            string[] paramarray = parameters.Split(splitChar[0]);
            splitChar[0] = '<';
            foreach (string item in paramarray)
            {
                if(item.Contains("show c"))
                {
                    textReader = new StreamReader("LICENSE.txt");
                    Console.Clear();
                    Console.Write(textReader.ReadToEnd());
                }
                if (item.Length > 0)
                {
                    itemarg = item.ToCharArray()[0];
                    switch (itemarg)
                    {
                        //searches the clearnet with random adresses
                        case 'i':
                            darknet = false;
                            if (item.Length > 2)
                            {
                                if (int.TryParse(item.Remove(0, 1), out amount))
                                {
                                    connect = true;
                                }
                            }
                            //if no number was given, the user has 4 tries to enter a valid number or else the program exits
                            else
                            {
                                Console.WriteLine("Enter the amount of random adresses to search");
                                try
                                {
                                    amount = int.Parse(Console.ReadLine());
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Please enter a number!");
                                    try
                                    {
                                        amount = int.Parse(Console.ReadLine());
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Please enter a number!!");
                                        try
                                        {
                                            amount = int.Parse(Console.ReadLine());
                                        }
                                        catch (FormatException)
                                        {
                                            Console.WriteLine("Please enter a number!!!");
                                            try
                                            {
                                                amount = int.Parse(Console.ReadLine());
                                            }
                                            catch (FormatException)
                                            {
                                                Console.WriteLine("I'm giving up!");
                                                Thread.Sleep(1500);
                                                Environment.Exit(1);
                                            }
                                        }
                                    }
                                }
                                connect = true;
                            }
                            break;

                        //set the minimum url length
                        case 'u':
                            if (item.Length > 2)
                            {
                                if (!int.TryParse(item.Remove(0, 1), out urlLengthMin))
                                {
                                    Console.Clear();
                                    Console.WriteLine("Enter the minimum URL length");
                                    urlLengthMin = int.Parse(Console.ReadLine());
                                }
                            }
                            break;

                        //set the maximum url length
                        case 'm':
                            if (item.Length > 2)
                            {
                                if (!int.TryParse(item.Remove(0, 1), out urlLengthMax))
                                {
                                    Console.Clear();
                                    Console.WriteLine("Enter the maximum URL Length");
                                    urlLengthMax = int.Parse(Console.ReadLine());
                                }
                            }
                            break;

                        //specify the complexity
                        case 'c':
                            if (item.Length > 1)
                            {
                                if (!item.Remove(0, 1).Contains("l") && !item.Remove(0, 1).Contains("n") && !item.Remove(0, 1).Contains("s"))
                                {
                                    Console.Clear();
                                    Console.WriteLine("Enter the desired URL complexity");
                                    Console.WriteLine("add 'l' for letters, 'n' for numbers and 's' for special characters");
                                    complexity = Console.ReadLine();
                                }
                                else
                                {
                                    complexity = item.Remove(0, 1);
                                }
                            }
                            break;

                        //scan all websites from a specified list -> -p
                        case 'l':
                            if (item.Length > 1)
                            {
                                if (regEx.IsMatch(item.Remove(0, 1)))
                                {
                                    location = item.Remove(0, 1);
                                    textReader = new StreamReader(location);
                                }
                            }
                            Console.Clear();
                            Console.WriteLine("Crawling the Internet...");
                            Console.WriteLine("Scanning all adresses from the list");
                            darknet = false;
                            connect = true;
                            break;
                        /*
                        case 'p':
                           
                            break;

                        
                        case 's':
                            Console.Clear();
                            Console.WriteLine("Settings:");
                            break;
                            
                        case 'd':
                            Console.Clear();
                            Console.WriteLine("Crawling the Darknet...       Spooky");
                            Console.WriteLine("Enter the amount of random adresses to search");
                            darknet = true;
                            amount = int.Parse(Console.ReadLine());
                            Connect(darknet, RandomURL(darknet));
                            break;

                        case 'o':
                            Console.Clear();
                            Console.WriteLine("Crawling the Darknet...       Spooky");
                            Console.WriteLine("Scanning all adresses from the list");
                            darknet = true;
                            Connect(darknet, RandomURL(darknet));
                            break; */

                        case 'h':
                            Help();
                            break;

                        case '?':
                            Help();
                            break;

                        default:
                            Help();
                            break;
                    }
                }
            }

            if (connect)
            {
                Console.Clear();
                Console.WriteLine("This is the spidey spider^^");
                if (!darknet)
                {
                    Console.WriteLine("Crawling the Web...");
                    Console.WriteLine("Amount: " + amount + ", MaxLength: " + urlLengthMax + ", MinLength: " + urlLengthMin + ", URL Comlexity: " + complexity);
                    using (MySqlConnection conn = new MySqlConnection())
                    {
                        conn.ConnectionString = connection_string;
                        conn.Open();

                        MySqlCommand command = new MySqlCommand("SELECT url FROM sites WHERE url = @url", conn);
                        command.Parameters.Add(new MySqlParameter("url", url));

                        if (command.ExecuteNonQuery() >= 1)
                        {
                            update = true;
                        }

                    }
                }
                Connect(darknet, RandomURL(darknet));
            }
        }

        //establishes an internet connection and tor proxy
        private static async void Connect(bool darknet, string urlp)
        {
            amount--;
            badR = false;
            url = urlp;
            if (!darknet)
            {
                //create a new httpclient to connect to the internet
                HttpClient client = new HttpClient();
                HttpResponseMessage answer = new HttpResponseMessage();

                Console.WriteLine("Connecting to: " + url + "     | Sites Left: " + amount);

                //trying to get the website
                client.Timeout = span;
                try { answer = await client.GetAsync(url); }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException || ex is HttpRequestException)
                    {
                        Console.WriteLine("URL can't be resolved!");
                        badR = true;

                    }
                }
                if (!badR && answer.IsSuccessStatusCode)
                {
                    Console.WriteLine("Connected!");

                    //reading the website as a string
                    string content = "";
                    try { content = await answer.Content.ReadAsStringAsync(); }
                    catch (Exception exce)
                    {
                        if (exce is ObjectDisposedException || exce is NullReferenceException)
                        {
                            badR = true;
                        }
                    }
                    if (!badR)
                    {
                        if (WriteToDB(content))
                        {
                            if (amount == 0)
                            {
                                Console.WriteLine("We did it! Woohoo!");
                                Console.WriteLine("Wanna search for more?");
                                Console.WriteLine("To do that, you sadly have to restart the program.");
                                Console.WriteLine("I haven't found a way yet to get around some pesky ReadKey() problems");
                            }
                            else
                            {
                                Connect(darknet, RandomURL(darknet));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Something died while scraping!");
                            Connect(darknet, RandomURL(darknet));
                        }
                    }
                    else
                    {
                        if (amount > 0)
                        {
                            Connect(darknet, RandomURL(darknet));
                        }
                        else if (amount == 0)
                        {
                            Console.WriteLine("We did it...");
                            Console.WriteLine("Wanna search for more?");
                            Console.WriteLine("To do that, you sadly have to restart the program.");
                            Console.WriteLine("I haven't found a way yet to get around some pesky ReadKey() problems");
                        }
                    }
                }
                else
                {
                    if (amount > 0)
                    {
                        Connect(darknet, RandomURL(darknet));
                    }
                    else if (amount == 0)
                    {
                        Console.WriteLine("We did it...");
                        Console.WriteLine("Wanna search for more?");
                        Console.WriteLine("To do that, you sadly have to restart the program.");
                        Console.WriteLine("I haven't found a way yet to get around some pesky ReadKey() problems");
                    }
                }
            }
        }

        //adds the entry to the db while extracting some information
        private static bool WriteToDB(string content)
        {
            if (content.Length > 0)
            {
                counter = 0;
                //splitting the website and extracting title information
                Console.WriteLine("Extracting title information...");

                string[] contentArray = content.Split(splitChar[0]);
                foreach (var item in contentArray)
                {
                    if (item.Contains("title>") && counter == 0)
                    {
                        counter++;
                        title = item.Substring(6);
                    }
                }

                Console.WriteLine("Title extracted: " + title);
                Console.WriteLine("Connecting to DataBase");

                //pushing the title and other information to a db
                using (MySqlConnection conn = new MySqlConnection())
                {
                    conn.ConnectionString = connection_string;
                    conn.Open();
                    MySqlCommand command = new MySqlCommand();

                    if (update)
                    {
                        command = new MySqlCommand("UPDATE sites SET title = @title, time = @time, content = @content, sha128 = @hash", conn);
                        command.Parameters.Add(new MySqlParameter("title", title));
                        command.Parameters.Add(new MySqlParameter("time", DateTime.Now));
                        command.Parameters.Add(new MySqlParameter("content", content));
                        command.Parameters.Add(new MySqlParameter("hash", Hash(content)));
                    }
                    else
                    {
                        command = new MySqlCommand("INSERT INTO sites (title, id, time, url, content, sha128) VALUES (@title, '', @time, @url, @content, @hash)", conn);
                        command.Parameters.Add(new MySqlParameter("title", title));
                        command.Parameters.Add(new MySqlParameter("time", DateTime.Now));
                        command.Parameters.Add(new MySqlParameter("url", url));
                        command.Parameters.Add(new MySqlParameter("content", content));
                        command.Parameters.Add(new MySqlParameter("hash", Hash(content)));
                    }

                    Console.WriteLine("Adding the entry to the DataBase");
                    Console.WriteLine("Added to the DB - affected rows: " + command.ExecuteNonQuery());
                }

                if (counter == 1)
                {
                    Console.WriteLine("Entry scraped successfully");
                    return true;
                }
                else return false;
            }
            else return false;
        }

        //generates a new address to scrape
        private static string RandomURL(bool darknet)
        {
            if (darknet)
            {
                //generate a new .onion address
                return "";
            }
            else
            {
                //generate a new clearnet address
                url = "";
                int urlLength = randomNr.Next(urlLengthMin, urlLengthMax);
                //Console.WriteLine("The Url will be " + urlLength + " charachters long.");

                //creates a random url given the complexity and maximum length

                //letters
                if (complexity.Contains("l"))
                {
                    //letters and numbers
                    if (complexity.Contains("n"))
                    {
                        //letters, numbers, and special charachters: _ -
                        if (complexity.Contains("s"))
                        {
                            for (int i = 0; i < urlLength; i++)
                            {

                                int x = randomNr.Next(0, 37);
                                if (x <= 9)
                                {
                                    url += x;
                                }
                                else if (x >= 10 && x <= 35)
                                {
                                    x += 87;
                                    char s = (char)x;
                                    url += s;
                                }
                                else if (x == 36)
                                {
                                    url += "-";
                                }
                            }
                        }

                        //only letters and numbers
                        else
                        {
                            for (int i = 0; i < urlLength; i++)
                            {

                                int x = randomNr.Next(0, 36);
                                if (x <= 9)
                                {
                                    url += x;
                                }
                                else if (x >= 10 && x <= 35)
                                {
                                    x += 87;
                                    char s = (char)x;
                                    url += s;
                                }
                            }
                        }
                    }

                    //letters and special charachters: _ -
                    else
                    {
                        if (complexity.Contains("s"))
                        {
                            for (int i = 0; i < urlLength; i++)
                            {

                                int x = randomNr.Next(0, 26);
                                if (x >= 0 && x <= 25)
                                {
                                    x += 97;
                                    char s = (char)x;
                                    url += s;
                                }
                                else if (x == 26)
                                {
                                    url += "-";
                                }
                            }
                        }

                        //only letters
                        else
                        {
                            for (int i = 0; i < urlLength; i++)
                            {
                                int x = randomNr.Next(0, 26);
                                if (x >= 0 && x <= 25)
                                {
                                    x += 97;
                                    char s = (char)x;
                                    url += s;
                                }
                            }
                        }
                    }
                }

                //numbers
                else if (complexity.Contains("n"))
                {
                    //numbers and special chararcters
                    if (complexity.Contains("s"))
                    {
                        for (int i = 0; i < urlLength; i++)
                        {
                            int x = randomNr.Next(0, 11);
                            if (x <= 9)
                            {
                                url += x;
                            }
                            else if (x == 10)
                            {
                                url += "-";
                            }
                        }
                    }

                    //numbers only
                    else
                    {
                        for (int i = 0; i < urlLength; i++)
                        {
                            int x = randomNr.Next(0, 10);
                            if (x <= 9)
                            {
                                url += x;
                            }
                        }
                    }
                }

                //special chars only
                else
                {
                    for (int i = 0; i < urlLength; i++)
                    {
                        url += "-";
                    }
                }

                url = "http://www." + url + tld;
                return url;
            }
        }

        //generates a sha1 hash for the given string
        private static string Hash(string input)
        {
            Console.WriteLine("Hashing the Website for change detection");

            using (SHA1Managed sha1 = new SHA1Managed())
            {
                //hash generation
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                //stitching the hash to a nice string
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static void Help()
        {
            Console.Clear();
            Console.WriteLine("use parameter -i followed by a number to scan random web addresses. The number represents the amount of adresses to scan");
            Console.WriteLine("use parameter -d to scan random .onion addresses");
            Console.WriteLine("use parameter -u followed by a number to set the Minimum URL length");
            Console.WriteLine("use parameter -m followed by a number to set the Maximum URL length");
            Console.WriteLine("use parameter -c followed by a combination of 'l' for letters, 'n' for numbers and 's' for special characters");
            Console.WriteLine("use parameter -l followed by a filepath to a .txt file to scan all websites from the given list");
            Console.WriteLine("In this file, all URLs must be seperated with a ';'.");
            Console.WriteLine("use parameter -o to scan all .onion sites from a given list");
            Console.WriteLine("using parameter -h or -? or nothing takes you here");
            Console.WriteLine("You may now restart the program");
        }
    }
}