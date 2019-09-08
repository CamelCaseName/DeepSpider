# deepcrawler
A c# console based Application to crawl and/or scrape websites in the clear- or soon darknet   <br />
Darkweb is not yet implemented  <br />
# usage
To run DeepSpider, simply open up the Terminal, navigate to the folder in which DeepSpider.exe is located, and run it.  <br />
#Parameters
There are different ways to run the Software. Using -i as a parameter will give you the option to crawl and scrape clearnet websites.  <br />
Putting a number behind the -i parameter specifies the amount of random adresses to generate.  <br />
Use parameter -u followed by a number to set the Minimum URL length used for the generation of random adresses.  <br />
Use parameter -m followed by a number to set the Maximum URL length for the generated adresses.  <br />
Use parameter -c followed by a combination of 'l' for letters, 'n' for numbers and 's' for dashes  <br />
Use parameter -l followed by a filepath to a .txt file to scan all websites from the given list  <br />
Hashing can be very ineffective and slow. To stop hashing every find use -n  <br />
In this file, all URLs must be seperated with a ';'.  <br />
Adding the parameter -h or -? or not providing any parameter at all prints out a help page similar to this.  <br />
If you start the Program with "show c", the license will be printed  <br />
'show w' will show warranty information.  <br />