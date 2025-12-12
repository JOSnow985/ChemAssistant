// Jaden Olvera, CS-1400, Final Project: Chem Assistant

using System.Diagnostics;
using System.Text.RegularExpressions;

// Store paths as variables we can edit easily later if need be
string retrieveURL = "https://raw.githubusercontent.com/JOSnow985/ChemAssistant/refs/heads/main/ptable.csv";
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
    pTableList = [];
}



// ---- Asserts ----
// The pTable List retrieved from the file should be 118 elements long with a header line
Debug.Assert(pTableList.Count == 119);
// HCN should be parsed into a list with three entries, any entry should have an atom count of 1
var assertHCN = StringParser("HCN");
Debug.Assert(assertHCN.Count == 3);
Debug.Assert(assertHCN[1].atomCount == 1);
// NaNO3 should be parsed into a list with three entries, and the third entry should have an atom count of 3
var assertNaNO3 = StringParser("NaNO3");
Debug.Assert(assertNaNO3.Count == 3);
Debug.Assert(assertNaNO3[2].atomCount == 3);
// NaNO3 and HCN should calculate to these molar masses
// Debug.Assert(findMolarMass(assertNaNO3, pTableList).ToString("F4") == "84.9947");
// Debug.Assert(findMolarMass(assertHCN, pTableList).ToString("F4") == "27.0253");
// ----         ----



string[] mainMenuArray = [  "1. Bookmarks",
                            "2. Molar Mass Calculator",
                            "3. Element Look Up",
                            "4. Exit"
                        ];

bool exiting = false;
while (exiting == false)
{
    // Handles the returned integer from the menu system
    switch (SelectMenu(mainMenuArray))
    {
        case 1:
            Console.WriteLine("1");
            exiting = true;
            break;
        case 2:
            double molarMass = findMolarMass(in pTableList);
            Console.WriteLine($"Molar Mass: {molarMass:F4}");
            Console.WriteLine("\nPress any key to return to the main menu!");
            Console.ReadKey(true);
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



// ---- Methods ----

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
                HttpResponseMessage response = await http.GetAsync(retrieveURL);
                response.EnsureSuccessStatusCode();

                // Await lets us pause the program until we finish this part, but doesn't "block the thread"
                await using var stream = await response.Content.ReadAsStreamAsync();
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
    // Keeps looping until a selection has been made
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
        if (userInputValue >= 1 && userInputValue <= menuArray.Length)
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
        // Makes a beep if the key pressed wasn't valid
        else
            Console.Beep();
    }

    // Return selected option, offset by 1 to account for zero indexing the array
    return optionHighlighted;
}

static string userInputHandler(string prompt)
{
    while (true)
    {
    drawHeader();
    Console.WriteLine(prompt);
    string userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.WriteLine("Sorry but your input didn't look right.\nPlease try again with something like this: KMnO4\n");
        Console.WriteLine("Press any key to continue!");
        Console.ReadKey(true);
    }
    else
        return userInput;
    }
}

// Uses Regex to take a string and parse it to a list of elements and numbers of atoms
static List<(string elementSymbol, int atomCount)> StringParser(string inputString)
{
    List<(string, int)> formulaList = [];

    // Makes two "groups" that we'll use to make our tuple, needs at least one letter for the element's symbol
    // The *'s indicate those components are optional, the element doesn't need a lowercase letter and a digit
    string regexPattern = @"([A-Z][a-z]*)(\d*)";
    foreach (Match pulledElement in Regex.Matches(inputString, regexPattern))
    {
        // The "Match" returns two groups, the symbol string and a string containing the number of atoms
        string elementSymbol = pulledElement.Groups[1].Value;
        string atomCountString = pulledElement.Groups[2].Value;

        // If there isn't an atom count in the second string, it's just a single atom.
        int atomCountInt;
        if (Int32.TryParse(atomCountString, out int atomParse) == true)
            atomCountInt = atomParse;
        else
            atomCountInt = 1;
        formulaList.Add((elementSymbol, atomCountInt));
    }
    return formulaList;
}

// Searches through the passed list to retrieve the information of the passed string's element symbol
static List<string> RetrieveInfo(in string elementSymbol, in List<List<string>> listToSearch)
{
    foreach (List<string> element in listToSearch)
    {
        if (elementSymbol == element[1].Trim())
            return element;
    }
    // If we get through the entire table without an element to return, just give them Neon
    return listToSearch[10];
}

// Uses info from RetrieveInfo to grab the molar masses from the data file
static double findMolarMass(in List<List<string>> pTable)
{
    // Collect molecule from user input
    string userInput = userInputHandler("What molecule to do you want the molar mass of?");
    // Parse userInput to a list of elements and atom counts
    var atomList = StringParser(userInput);

    double massTotal = 0;
    // Iterate over the list of atoms passed, these are what we're finding the masses of and adding up
    foreach ((string elementSymbol, int atomCount) in atomList)
    {
        string elementMassString = RetrieveInfo(elementSymbol, pTable)[3];
        // Because the molar mass isn't a pure number in the data file, we need to trim it
        string trimmedMassString = "0";
        for (int charIndex = 0; charIndex < elementMassString.Length; charIndex++)
        {
            if (elementMassString[charIndex] == '(')
            {
                trimmedMassString = elementMassString.Substring(0, charIndex);
                charIndex = elementMassString.Length;
            }
        }
        // Finally try parsing, if it works we multiply the resulting double by the number of atoms present
        if (double.TryParse(trimmedMassString, out double parsedMass))
            massTotal += parsedMass * atomCount;
    }
    return massTotal;
}