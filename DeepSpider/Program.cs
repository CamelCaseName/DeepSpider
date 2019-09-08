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
using MySqlX.XDevAPI.CRUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
        //uses hashing yes or no
        private static bool hashing = true;
        //true when a list is used to read urls
        private static bool isList = false;
        //true when a list is used to export the results
        private static bool resList = false;
        //bool to check wether a site has already been visited and thus just needs updating
        private static bool update;
        //saves the complexity to use
        private static char[] complexityArr = new char[4];
        //a char  with the current parameter
        private static char itemarg;
        //chars to split strings with
        private static char[] splitChar = new char[3];
        //an array of which chars to use in a url
        private static int[] urlCharArr;
        //amount of urls
        private static int amount;
        //some status vars
        private static int count = 0;
        private static int counter = 0;
        private static int iteration =0 ;
        private static int listPos = 0;
        //minimal and maximal url lengths
        private static int urlLengthMax;
        private static int urlLengthMin;
        //a list of chars with every possible char you could find in a url
        private static List<char> urlChars = new List<char>();
        //the connectiong string for mysql, without any data
        private static string connection_string = "";
        //some configurations
        private static string config = "";
        private static string[] configArray;
        //complexity as a string
        private static string complexity = "";
        //path to the list of urls to scrape
        private static string location = "";
        //params for the program when started again from itself
        private static string parameters = "";
        //path to the file in which the results are saved
        private static string resultPath = "";
        //the website title, obviously
        private static string title = "";
        //the top level domain to use
        private static string tld = "";
        //placeholder and url
        private static string url = "";
        //an array of urls read from the list
        private static string[] urlArray;
        //rng
        private static Random randomNr = new Random();
        //regex to check filepaths
        private static Regex regEx = new Regex("([a - zA - Z]:(\\w +) *\\[a-zA-Z0_9]+)?.txt$");
        //stopwatch
        private static Stopwatch watchy = new Stopwatch();
        //a span to use as a timeout for the connections
        private static TimeSpan span;
        //tools to interact with a txt file
        private static TextReader textReader;
        private static TextWriter writer;

        //main method
        private static void Main(string[] args)
        {
            //loads the config file and spits out all values
            textReader = new StreamReader("..\\..\\Resources\\config.txt");
            splitChar[0] = '%';
            splitChar[1] = '\n';
            splitChar[2] = '\r';
            //splits the file at every %
            configArray = textReader.ReadToEnd().Split(splitChar[0]);
            Console.WriteLine("reading config.txt...");
            foreach (var s in configArray)
            {
                splitChar[0] = '=';
                //reset the config string
                config = "";
                //true, if the beginning of a line doesnt start with #. sorts out the first comments
                if (!(s.ToCharArray()[0] == '#'))
                {
                    //iterates through the string and removes every char after a # to get rid of between variable comments
                    foreach (var c in s.ToCharArray())
                    {
                        if (c != '#')
                        {
                            config += c;
                        }
                        else break;
                    }
                    //checks the variable name for every item in the cleaned list

                    //sets the string to connect to  the db with and prints out the result
                    if (config.Contains("connectionstring"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        config = config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (config.IndexOf('\0') > 0)
                        {
                            connection_string = config.Remove(config.IndexOf('\0'), config.Length - config.IndexOf('\0'));
                        }
                        else connection_string = config;
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("Connection string read: " + connection_string);
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }
                    //sets the minimum url length and prints out the result
                    else if (config.Contains("urlLengthMin"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        urlLengthMin = int.Parse(config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0'));
                        Console.WriteLine("Minimum URL length read: " + urlLengthMin);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    //sets the maximum url length and prints out the result
                    else if (config.Contains("urlLengthMax"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        urlLengthMax = int.Parse(config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0'));
                        Console.WriteLine("Maximum URL length read: " + urlLengthMax);
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                    }
                    //sets the desired url complexity for the random url generation and prints out the result
                    else if (config.Contains("complexity"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        config = config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (config.IndexOf('\0') > 0)
                        {
                            complexity = config.Remove(config.IndexOf('\0'), config.Length - config.IndexOf('\0'));
                        }
                        else complexity = config;
                        Console.WriteLine("Complexity read: " + complexity);
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                    }
                    //sets the top level domain to use and prints out the result
                    else if (config.Contains("tld"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        config = config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (config.IndexOf('\0') > 0)
                        {
                            tld = config.Remove(config.IndexOf('\0'), config.Length - config.IndexOf('\0'));
                        }
                        else tld = config;
                        Console.WriteLine("Top level domain read: " + tld);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    //sets the filepath to a list of urls to conect to and prints out the result
                    else if (config.Contains("location"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        config = config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (config.IndexOf('\0') > 0)
                        {
                            location = config.Remove(config.IndexOf('\0'), config.Length - config.IndexOf('\0'));
                        }
                        else location = config;
                        Console.WriteLine("Filepath to URLs.txt read: " + location);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    //sets the filepath to an output file to write results to and prints out the result
                    else if (config.Contains("resultPath"))
                    {
                        //cleans the value from \n and \r and the variable name + =
                        config = config.Remove(0, config.IndexOf('=') + 1).Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (config.IndexOf('\0') > 0)
                        {
                            resultPath = config.Remove(config.IndexOf('\0'), config.Length - config.IndexOf('\0'));
                        }
                        else resultPath = config;
                        Console.WriteLine("Filepath to result.txt read: " + resultPath);
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                }
            }
            Console.WriteLine("All configurations read and loaded!");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var arg in args)
            {
                parameters += arg;
            }
            span = TimeSpan.FromSeconds(10);
            UserInteraction(parameters);
            Console.ReadLine();
        }

        private static void UserInteraction(string parameters)
        {
            //user interaction       
            if (parameters.Length > 0)
            {
                //splits all the parameters to use in the switch case
                splitChar[0] = '-';
                string[] paramarray = parameters.Split(splitChar[0]);
                splitChar[0] = '<';
                foreach (string item in paramarray)
                {
                    //shows the license
                    if (item.Contains("show c"))
                    {
                        textReader = new StreamReader("..\\..\\Resources\\LICENSE");
                        Console.Clear();
                        Console.Write(textReader.ReadToEnd());
                        Console.ReadLine();
                    }
                    //shows the warranty
                    else if (item.Contains("show w"))
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("This Software is distributed under GNU GENERAL PUBLIC LICENSE Version 3.0 and AS IS.");
                        Console.WriteLine("This means that there may be bugs in this Software or it may be unstable.");
                        Console.WriteLine("The original author is thereby not responsible for any money or time lost due to imperfections in this software.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Use Parameter 'show c' to print out the license to the console.");
                        UserInteraction(Console.ReadLine());
                    }
                    //iterates through all parameters and does what they are supposed to do
                    else if (item.Length > 0)
                    {
                        try{
                            itemarg = item.ToCharArray()[0];
                        }
                        //exception thrown when no correct parameters are given
                        catch(FormatException)
                        {
                            Console.WriteLine("Please give me valid arguments...");
                            Console.WriteLine("Enter all arguments and confirm with ENTER");
                            UserInteraction(Console.ReadLine());
                        }
                        
                        switch (itemarg) //all cases: i u m c l f n j (s d o) h ?
                        {
                            //searches the clearnet with random adresses
                            case 'i':
                                urlArray = new string[1];
                                isList = false;
                                Console.Clear();
                                darknet = false;
                                if (item.Length > 1)
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
                                if (item.Length > 1)
                                {
                                    //reads and cleans the value
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
                                if (item.Length > 1)
                                {
                                    //reads and cleans the value
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
                                if (item.Length > 1 && item.Length <7)
                                {
                                    //reads and cleans the value
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
                                else
                                {
                                    Console.Clear();
                                    Console.WriteLine("Please enter the desired URL complexity");
                                    Console.WriteLine("add 'l' for letters, 'n' for numbers and 's' for special characters");
                                    complexity = Console.ReadLine();
                                }
                                break;

                            //scan all websites from a specified list 
                            case 'l':
                                if (item.Length > 1)
                                {
                                //chechks the filepath with the regex
                                    if (regEx.IsMatch(item.Remove(0, 1)))
                                    {
                                        location = item.Remove(0, 1);
                                        isList = true;
                                    }
                                }
                                else if(location.Length > 1){
                                    isList = true;
                                }
                                Console.Clear();
                                Console.WriteLine("Crawling the Internet...");
                                Console.WriteLine("Scanning all adresses from the list");
                                
                                //sets all settings as needed
                                darknet = false;
                                connect = true;
                                listPos = 0;
                                if(isList){
                                    try{
                                        textReader = new StreamReader(location);
                                    }
                                    catch(Exception)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Please check your filepath");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine("Enter all arguments and confirm with ENTER");
                                        UserInteraction(Console.ReadLine());                                       
                                    }
                                    splitChar[0] = ';';
                                    url = textReader.ReadToEnd();
                                    urlArray = url.Split(splitChar[0]);
                                    splitChar[0] = '<';
                                    listPos = 0;
                                    amount = urlArray.Length;
                                }
                                else //ERRORS n stuff
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Please check your filepath");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Enter all arguments and confirm with ENTER");
                                    UserInteraction(Console.ReadLine());
                                }
                                break;
                            
                            //saving to a local file, not a db
                            case 'f':
                                if (item.Length > 1)
                                {
                                    // checks the path against the regex
                                    if (regEx.IsMatch(item.Remove(0, 1)))
                                    {
                                        resultPath = item.Remove(0, 1);
                                        resList = true;
                                    }
                                }
                                else if(resultPath.Length > 1){
                                    resList = true;
                                }

                                //creates the writer instance
                                writer = new StreamWriter(resultPath);
                                break;

                            //sets wether the entrys will be hashed or not
                            case 'n':
                                hashing = false;
                                break;

                            //brute force all urls given the complexity and length
                            case 'j':
                                //sets all settings as needed
                                urlArray = new string[1];
                                isList = false;
                                iteration = -1;
                                darknet = false;
                                Console.Clear();
                                Console.WriteLine("Brute force is cool");
                                if (item.Length > 1)
                                {
                                    //reads and cleans the value
                                    if (int.TryParse(item.Remove(0, 1), out amount))
                                    {
                                        urlCharArr = new int[urlLengthMax+1];
                                    }
                                    else
                                    {
                                        Console.WriteLine("The amount specified in config.txt will be used, so most likely 0");
                                    }
                                    for (int i = 0; i < urlCharArr.Length; i++)
                                    {
                                        //prepares the url array
                                        urlCharArr[i] = -1;
                                    }
                                    connect = true;
                                }
                                break;

                                /*
                            case 'd':
                                urlArray = new string[1];
                                isList = false;
                                iteration = -1;
                                darknet = false;
                                amount = (int)Math.Pow(26,5);
                                Console.Clear();
                                Console.WriteLine("Debug mode");
                                urlCharArr = new int[urlLengthMax+1];
                                for (int i = 0; i < urlCharArr.Length; i++)
                                {
                                    urlCharArr[i] = -1;
                                }
                                watchy.Start();
                                for (int i = 0; i < amount; i++)
                                {
                                    url = IterateURL(darknet);
                                }
                                watchy.Stop();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(watchy.ElapsedMilliseconds);
                                Console.ForegroundColor = ConsoleColor.White;
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

                                //help, cant u read
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
                
                //true when trying to connect to a website
                if (connect)
                {
                //informs the user about the current state of the app
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("This is the spidey spider^^");
                    Console.ForegroundColor = ConsoleColor.White;
                    if (!darknet)
                    {
                        Console.WriteLine("Crawling the Web...");
                        if(isList)
                        {
                            Console.WriteLine("Amount of adresses in the list: " + amount);
                        }
                        else
                        {
                            Console.WriteLine("Amount: " + amount + ", MaxLength: " + urlLengthMax + ", MinLength: " + urlLengthMin + ", URL Comlexity: " + complexity);
                        }                      
                    }
                    //executes the right query if there are still urls left to connect to
                    if(amount > 0){
                        if(isList && listPos <= urlArray.Length)
                        {
                            Connect(darknet, "dummy");
                        }
                        else if(listPos < urlArray.Length && iteration == 0)
                        {
                            Connect(darknet, RandomURL(darknet));
                        }
                        //bruteforces all urls
                        else if(iteration == -1)
                        {
                            Connect(darknet, IterateURL(darknet));
                        }
                    }
                    else
                    {
                        //errors n stuff
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("the amount is set to 0");
                        Console.ForegroundColor = ConsoleColor.White;
                        UserInteraction(Console.ReadLine());
                    }
                }
            }
            else
            {
                //basically the same like when starting the program
                Console.WriteLine("please enter all parameters and confirm with ENTER");
                UserInteraction(Console.ReadLine());
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

                //reads all urls from a list
                if(isList)
                {
                    if(listPos < urlArray.Length)
                    {
                        //splits the string and saves the urls
                        url = urlArray[listPos];
                        url = url.Replace(splitChar[1], '\0').Replace(splitChar[2], '\0');
                        if (url.IndexOf('\0') > 0)
                        {
                            url = url.Remove(url.IndexOf('\0'), url.Length - url.IndexOf('\0'));
                        }
                        listPos++;
                    }
                    else
                    {
                        EndDisplay();
                    }
                }
                //printing stuff
                Console.WriteLine("Connecting to: " + url + "     | Sites Left: " + amount);

                //trying to get the website, with timeout and other settings
                client.Timeout = span;
                try { answer = await client.GetAsync(url); }
                catch (Exception ex)
                {
                //catch all the different exceptions
                    if (ex is InvalidOperationException || ex is HttpRequestException || ex is UriFormatException)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("URL can't be resolved!");
                        Console.ForegroundColor = ConsoleColor.White;
                        badR = true;

                    }
                }

                //gets executed if everything went right
                if (!badR && answer.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Connected!");
                    Console.ForegroundColor = ConsoleColor.White;

                    //reading the website as a string
                    string content = "";
                    try { content = await answer.Content.ReadAsStringAsync(); }
                    catch (Exception exce)
                    {
                        if (exce is ObjectDisposedException || exce is NullReferenceException || exce is UriFormatException)
                        {
                            badR = true;
                        }
                    }

                    //writes the results to a db or a file, and calls the corresponding methods again
                    if (!badR)
                    {
                        if (WriteToDB(content))
                        {
                            if (amount == 0)
                            {
                                EndDisplay();
                            }
                            else
                            {
                                ConnectAgain();
                            }
                        }
                        else
                        {
                            ConnectAgain();
                            if (amount == 0)
                            {
                                EndDisplay();
                            }
                        }
                    }
                    else
                    {
                        if (amount > 0)
                        {
                            ConnectAgain();
                        }
                        else if (amount == 0)
                        {
                            EndDisplay();
                        }
                    }
                }
                else
                {
                    if (amount > 0)
                    {
                        ConnectAgain();
                    }
                    else if (amount == 0)
                    {
                        EndDisplay();
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

                //extracting the title via the html tags
                string[] contentArray = content.Split(splitChar[0]);
                foreach (var item in contentArray)
                {
                    if (item.Contains("title>") && counter == 0)
                    {
                        counter++;
                        title = item.Substring(6);
                    }
                }

                //if a title is found
                if(counter == 1)
                {
                    Console.WriteLine("Title extracted: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(title + "\n");
                }

                //if no title is found, the url is used instead
                else
                {
                    title = url;
                    Console.WriteLine("No Title found, using URL: ");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(title + "\n");
                }
                Console.ForegroundColor = ConsoleColor.White;

                //saving to a db
                if(!resList)
                {
                    Console.WriteLine("Connecting to DataBase");
                
                    //pushing the title and other information to a db
                    using (MySqlConnection conn = new MySqlConnection())
                    {
                        conn.ConnectionString = connection_string;
                        MySqlCommand command;

                        //checks on duplicats and updates the entry in the db correspondingly               
                        try{
                            conn.Open();
                        }
                        //exceptions, as always
                        catch(MySqlException m){
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There is a problem with MySql: " + m);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Go check your settings or server");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("You may now end this program with ENTER");
                            Console.ReadLine();
                            Environment.Exit(1);
                        }

                        //selecting all occurances of the current url
                        command = new MySqlCommand("SELECT * FROM sites WHERE url = @url", conn);
                        command.Parameters.Add(new MySqlParameter("url", url));

                        //checking for duplicates 
                        if (command.ExecuteReader().Read())
                        {                       
                            update = true;
                        }
                    
                        //killing all subprocesses related to the query
                        command.Dispose();

                        //setting up the querys
                        if (update)
                        {
                            command.CommandText = "UPDATE sites SET title = @title, time = @time, content = @content, sha128 = @hash WHERE url = @url";
                            command.Parameters.Add(new MySqlParameter("title", title));
                            command.Parameters.Add(new MySqlParameter("time", DateTime.Now));
                            command.Parameters.Add(new MySqlParameter("content", content));
                            command.Parameters.Add(new MySqlParameter("hash", Hash(content)));
                        }
                        else
                        {
                            command.CommandText = "INSERT INTO sites (title, id, time, url, content, sha128) VALUES (@title, '', @time, @url, @content, @hash)";
                            command.Parameters.Add(new MySqlParameter("title", title));
                            command.Parameters.Add(new MySqlParameter("time", DateTime.Now));
                            command.Parameters.Add(new MySqlParameter("content", content));
                            command.Parameters.Add(new MySqlParameter("hash", Hash(content)));
                        }

                        //running the update query
                        if(update)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Updating the entry in the DataBase");
                            try{
                                //running the query
                                if(command.ExecuteNonQuery() > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                    Console.WriteLine("Updated!");
                                }
                            }
                            //exception, cuz dirty and ez
                            catch(Exception)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Couldn't update for some reason");
                            }                      
                            Console.ForegroundColor = ConsoleColor.White;
                            update = false;
                        }

                        //running the adding query
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("Adding the entry to the DataBase");
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            try
                            {
                                Console.WriteLine("Added to the DB - affected rows: " + command.ExecuteNonQuery());
                            }
                            //another dirty exception catch
                            catch(Exception)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Couldn't save to the db");
                            }                      
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        //cleaning up
                        conn.Close();
                    }
                }

                //writing to a file
                else
                {
                    Console.WriteLine("Writing to the specified output file.");

                    writer.Write("<title>"+title+"</title>"+"<time>"+DateTime.Now+"</time>"+"<sitecontent>"+content+"</sitecontent>"+"<hash>"+Hash(content)+"</hash>");       
                }

                //informing the user about the current state
                if (title.Length > 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Entry scraped successfully");
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                }
                //errors, what else is there
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Something died while scraping!");
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
            }
            //more errors
            else 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something died while scraping!");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
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
                complexityArr = complexity.ToCharArray();
                int urlLength = randomNr.Next(urlLengthMin, urlLengthMax);

                //creates a random url given the complexity and length

                foreach (var item in complexityArr)
                {
                    //checks for every type of char to add
                    switch(item){

                            //adds all chars to a list to use in url generation
                        case 'l':
                        foreach(var c in "abcdefghijklmnopqrstuvwxyz".ToCharArray())
                        {
                            urlChars.Add(c);
                        }
                            break;

                            //adds all numbers to the list of all chars to use
                        case 'n':
                        for(int i = 0; i<10;i++)
                        {
                            urlChars.Add(i.ToString().ToCharArray()[0]);
                        }
                            break;
                        
                            //adds all special chars to the list
                        case 's':
                        foreach(var c in "-~_".ToCharArray())
                        {
                            urlChars.Add(c);
                        }
                            break;
                    }
                }

                //add a random char from the list for the given length
                for (int i = 0; i < urlLength; i++)
                {
                    url += urlChars.ElementAt(randomNr.Next(0,urlChars.Count()));
                }

                //adds he rest of url stuff, such as tld or protocoll
                url = "http://www." + url + tld;
                return url;
            }
        }

        //generates a new address to scrape, but iteratively
        private static string IterateURL(bool darknet)
        {
            if (darknet)
            {
                //generate a new .onion address
                return "";
            }
            else if(iteration < urlCharArr.Length)
            {
                //generate a new clearnet address
                url = "";
                complexityArr = complexity.ToCharArray();

                //creates a random url given the complexity and length
                if(iteration == -1) 
                {
                    foreach (var item in complexityArr)
                    {
                        switch(item){

                            //adds all chars to a list to use in url generation
                            case 'l':
                            foreach(var c in "abcdefghijklmnopqrstuvwxyz".ToCharArray())
                            {
                                urlChars.Add(c);
                            }
                                break;

                                 //adds all numbers to the list of all chars to use
                            case 'n':
                            for(int i = 0; i<10;i++)
                            {
                                urlChars.Add(i.ToString().ToCharArray()[0]);
                            }
                                break;

                                //adds all special chars to the list
                            case 's':
                            foreach(var c in "-~_".ToCharArray())
                            {
                                urlChars.Add(c);
                            }
                                break;
                        }
                    }

                    //adjusting the value for further use
                    iteration = 1;
                }

                //test params -j100-n-cn-u0-m3

                if(urlCharArr[iteration-1] < urlChars.Count()-1)
                {
                    urlCharArr[iteration-1]++;
                }
                else
                {   
                    //for every place in the url a value is calculated, kind of like a numbering scheme
                    //auto increments and carries

                    //iterates through the url backwards
                    for (int k = urlCharArr.Length - 1; k >= 0 ; k--)
                    {   
                        //if the value is greater than the max
                        if(urlCharArr[k] >= urlChars.Count()-1)
                        {
                            if(k>0)
                            {
                                //resets the current char and raises the value of the previous by one
                                if(urlCharArr[k - 1] < urlChars.Count()-1)
                                {
                                    urlCharArr[k] = 0;
                                    urlCharArr[k - 1]++;
                                }
                            }

                            count = 0;

                            //counts the amount of maximum chars currently avalable
                            foreach (var item in urlCharArr)
                            {
                                if(item >= urlChars.Count()-1)
                                {
                                    count++;
                                }
                            }

                            //adds one more char to the url and esets all when the current maximum is reached
                            if(count == iteration)
                            {
                                for(int h = 0;h<iteration + 1;h++)
                                {
                                    urlCharArr[h] = 0;
                                }
                                iteration++;
                            }
                        }
                    }
                }

                //translating the array into a string according to the complexity
                foreach (var item in urlCharArr)
                {
                    if(item > -1){
                        url += urlChars.ElementAt(item);
                    }
                }

                //adding the rest like the tld
                url = "http://www." + url + tld;
                return url;
            }else return "";            
        }

        //generates a sha1 hash for the given string
        private static string Hash(string input)
        {
            if(hashing)
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

                    //returning the hash
                    return sb.ToString();
                }
            }
            else
            {
                return "";
            }
        }
        
        //prints out help
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
            Console.WriteLine("use parameter -f to write to a local file");
            Console.WriteLine("use parameter -n to not hash the websites");
            Console.WriteLine("use parameter -o to scan all .onion sites from a given list");
            Console.WriteLine("using parameter -h or -? or nothing takes you here");
            Console.WriteLine("[show c]opyright information");
            Console.WriteLine("[show w]arranty information");
            Console.WriteLine("You may now restart the program");
        }

        //ends the porgram
        private static void EndDisplay(){
            if(resList)
            {
                writer.Close();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("We did it! Woohoo!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Wanna search for more?");
            Console.WriteLine("To do that, you sadly have to restart the program.");
            Console.WriteLine("I haven't found a way yet to get around some pesky ReadKey() problems");
            Console.WriteLine("ENTER will close the program");
        }

        //a method to reconnect
        private static void ConnectAgain()
        {
            if(isList && listPos < urlArray.Length)
            {
                Connect(darknet, "dummy");
            }
            else if(listPos < urlArray.Length && iteration == 0)
            {
                Connect(darknet, RandomURL(darknet));
            }
            else if(iteration > 0)
            {
                Connect(darknet, IterateURL(darknet));
            }
        }
    }
}