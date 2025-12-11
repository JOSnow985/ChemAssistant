// Jaden Olvera, CS-1400, Final Project: Chem Assistant

using System.Diagnostics;

// Store paths as variables we can edit easily later if need be
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

// The pTable List retrieved from the file should be 118 elements long with a header line
Debug.Assert(pTableList.Count == 119);

string[] mainMenuArray = [  "1. Molar Mass Calculator",
                                "2. Bookmarks",
                                "3. Element Look Up",
                                "4. Exit"
                        ];

bool exiting = false;
while (exiting == false)
{
    switch (SelectMenu(mainMenuArray))
    {
        case 0:
            Console.WriteLine("0");
            exiting = true;
            break;
        case 1:
            Console.WriteLine("1");
            exiting = true;
            break;
        case 2:
            Console.WriteLine("2");
            exiting = true;
            break;
        case 3:
            Console.WriteLine("3");
            exiting = true;
            break;
        case 4:
            Console.WriteLine("4");
            exiting = true;
            break;
        default:
            Console.WriteLine("d");
            exiting = true;
            break;
            
    }
}






// -------------------------------- Methods --------------------------------

// Checks if we already have a ptable.csv to use, if we don't, ask for permission to retrieve it
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

static void drawHeader()
{
    Console.Clear();
    Console.WriteLine("ChemAssistant");
    Console.WriteLine();
}

// Menu system that returns a value to indicate what the requested function is
static int SelectMenu(string[] menuArray)
{
    int optionHighlighted = 0;
    bool selectionMade = false;

    // Displays an array to the user and lets the user control which one is highlighted
    // Up and Down arrows and Enter to select, returns selected option
    while (selectionMade == false)
    {
        drawHeader();
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
        // Casts the pressed key to an integer value we can use to check if they pressed a number key
        int userInputValue = (int)userInput - 48;

        // Then checks if that number is a possible choice on the menu
        if (userInputValue >= 0 && userInputValue < menuArray.Length)
        {
            optionHighlighted = userInputValue;
            selectionMade = true;
        }
        else if (userInput == ConsoleKey.UpArrow && optionHighlighted > 0)
            optionHighlighted--;
        else if (userInput == ConsoleKey.DownArrow && optionHighlighted < menuArray.Length - 1)
            optionHighlighted++;
        else if (userInput == ConsoleKey.Enter)
        {
            optionHighlighted++;
            selectionMade = true;
        }
        else
            Console.Beep();
    }

    // Return selected option, offset by 1 to account for zero indexing the array
    return optionHighlighted;
}