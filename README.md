# deepcrawler
A c# console based Application to crawl and/or scrape websites in the clear- or soon darknet
Darkweb is yet not implemented
# usage
To run DeepSpider, simply open up the Terminal, navigate to the folder in which DeepSpider.exe is located, and run it.
#Parameters
There are different ways to run the Software. Using -i as a parameter will give you the option to crawl and scrape clearnet websites.
Putting a number behind the -i parameter specifies the amount of random adresses to generate.
Use parameter -u followed by a number to set the Minimum URL length used for the generation of random adresses.
Use parameter -m followed by a number to set the Maximum URL length for the generated adresses
Use parameter -c followed by a combination of 'l' for letters, 'n' for numbers and 's' for dashes
Use parameter -l followed by a filepath to a .txt file to scan all websites from the given list
In this file, all URLs must be seperated with a ';'.
Adding the parameter -h or -? or not providing any parameter at all prints out a help page similar to this.
If you start the Program with "show c", the license will be printed

