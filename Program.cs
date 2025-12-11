// Jaden Olvera, CS-1400, Final Project: Chem Assistant

// Store paths as variables we can edit easily later if need be
using System.Diagnostics;

string retrieveURL = "placeholder";
string pTableFile = "ptable.csv";

// Check if we have a ptable, try to get it if we don't!
// Await will let this finish before we continue the program
await fileGrab(retrieveURL, pTableFile);

List<List<string>> pTableList;

if (File.Exists(pTableFile))
{
    pTableList = File.ReadAllLines(pTableFile).Select(line => line.Split(',').ToList()).ToList();
}
else
{
    pTableList = new List<List<string>>();
}

// The List retrieved from the file should be 118 elements long with a header line
Debug.Assert(pTableList.Count == 119);

// -------------------------------- Methods --------------------------------

// Checks if we already have a ptable.csv to use, if we don't, retrieves it
static async Task fileGrab(string retrieveURL, string outputFilePath)
{
    if (File.Exists("ptable.csv") == false)
    {
        Console.Clear();
        Console.WriteLine("We couldn't find \"ptable.csv\", and we need it!");
        Console.WriteLine("Do you want to download it from the github?");
        Console.WriteLine("Press a key to select: [Y]es / [N]o");
        bool downloadPermission = false;
        ConsoleKey userInput = Console.ReadKey(true).Key;
        if (userInput == ConsoleKey.Y)
            downloadPermission = true;
        else
            downloadPermission = false;
        if (downloadPermission == true)
        {
            try
            {
                // "using" makes sure that this HttpClient is cleaned up after
                // Apparently calls http.Dispose() automatically
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

                // Checks the code we get back, if we get a bad one, we don't want to try writing the file
                HttpResponseMessage resp = await http.GetAsync(retrieveURL);
                resp.EnsureSuccessStatusCode();

                // Await lets us pause the program until we finish this part, but doesn't "block the thread"
                await using var stream = await resp.Content.ReadAsStreamAsync();
                await using var file = File.Create(outputFilePath);
                await stream.CopyToAsync(file);
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine(hre);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

// Main Menu that returns a value to indicate what the requested function is
static int mainMenu(List<List<string>> dataList, int userIndex)
{
    string[] mainMenuArray = [  "1. Check Balance", 
                                "2. Withdraw", 
                                "3. Deposit", 
                                "4. Display Last 5 Transactions", 
                                "5. Quick Withdraw $40", 
                                "6. Quick Withdraw $100",
                                "7. End Current Session"
                            ];
    int firstOptionIndex = 2;
    // Return selected option, offset by 1 to account for length of array
    return optionSelector(mainMenuArray, firstOptionIndex) - 1;
}

// Displays an array to the user and lets the user control which one is highlighted
// Requires a value to indicate where the first selectable option is in the string array
// Up and Down arrows and Enter to select, returns selected option
static int optionSelector(string[] menuArray, int firstOptionIndex)
{
    int optionHighlighted = firstOptionIndex;
    while (true)
    {
        // drawHeader();
        for (int index = 0; index < menuArray.Length; index++)
        {
            // If this index is our current selection, highlight it
            if (optionHighlighted == index)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            Console.WriteLine(menuArray[index]);
            
            // If we highlighted, reset console coloring for next one
            if (optionHighlighted == index)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        ConsoleKey userInput = Console.ReadKey(true).Key;
        if (userInput == ConsoleKey.UpArrow && optionHighlighted > firstOptionIndex)
            optionHighlighted--;
        else if (userInput == ConsoleKey.DownArrow && optionHighlighted <= menuArray.Length - firstOptionIndex)
            optionHighlighted++;
        else if (userInput == ConsoleKey.Enter)
            return optionHighlighted;
        else
            Console.Beep();
    }
}