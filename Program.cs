// Jaden Olvera, CS-1400, Final Project: Chem Assistant

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

// Check if we have a pTable, try to get it if we don't!
// Await will let this finish before we continue the program
const string pTableFilePath = "pTable.json";
await fileGrab(pTableFilePath);

List<Element> pTableList;

// If the File Exists, read into into a list we can use
if (File.Exists(pTableFilePath))
{
    // If the file exists then the properties we're getting from the file for our objects should not be null, so ! added
    pTableList = JsonSerializer.Deserialize<List<Element>>(File.ReadAllText(pTableFilePath))!;
}
else
{
    return;
}

// ---- Asserts ----
// The pTable List retrieved from the file should have 118 Elements
Debug.Assert(pTableList.Count == 118);
// HCN should be parsed into a list with three entries, any entry should have an atom count of 1
var assertHCN = moleculeParser("HCN", in pTableList);
Debug.Assert(assertHCN.Count == 3);
Debug.Assert(assertHCN[1].atomCount == 1);
// NaNO3 should be parsed into a list with three entries, and the third entry should have an atom count of 3
var assertNaNO3 = moleculeParser("NaNO3", pTableList);
Debug.Assert(assertNaNO3.Count == 3);
Debug.Assert(assertNaNO3[2].atomCount == 3);
// NaNO3 and HCN should calculate to these molar masses
// Debug.Assert(findMolarMass(assertNaNO3, pTableList).ToString("F4") == "84.9947");
// Debug.Assert(findMolarMass(assertHCN, pTableList).ToString("F4") == "27.0253");
// ----         ----


string mainMenuPrompt = "What can ChemAssistant help with?";
string[] mainMenuArray = [  "1. Bookmarks",
                            "2. Molar Mass Calculator",
                            "3. Element Look Up",
                            "4. Exit"
                        ];

bool exiting = false;
while (exiting == false)
{
    // Handles the returned integer from the menu system
    switch (selectMenu(mainMenuArray, mainMenuPrompt))
    {
        case 1:
            Console.WriteLine("1");
            exiting = true;
            break;
        case 2:
            // Collect molecule from user input
            string userInput = userInputHandler("What molecule do you want the molar mass of?");
            // Parse userInput to a list of elements and atom counts
            var molecule = moleculeParser(userInput, in pTableList);
            foreach(var atoms in molecule)
                {
                    Console.WriteLine($"Element: {atoms.element.Name} Count: {atoms.atomCount} Atomic Mass: {atoms.element.AtomicMass}");
                }
            double molarMass = findMolarMass(in molecule, in pTableList);
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

// Checks if we already have a pTable.json to use, if we don't, ask for permission to retrieve it
static async Task fileGrab(string pTableFilePath)
{
    // Store paths as variables we can edit easily later if need be
    // Can be constant because this won't change during execution
    const string githubURL = "https://raw.githubusercontent.com/JOSnow985/ChemAssistant/refs/heads/main/pTable.json";
    const string neelAPI = "https://neelpatel05.pythonanywhere.com/";

    if (File.Exists("pTable.json") == false)
    {
        string downloadMenuPrompt = "We couldn't find \"pTable.json\", and we need it!\nDo you want to download it? (Nothing will be done without permission!)";
        string[] downloadMenuArray = [  "1. Do NOT download anything",
                                        "2. Download the file from this project's github",
                                        "3. Call the API at neelpatel05's website"
                                    ];
        int downloadPermission;
        // Switch that maps returned user selection to download permission int
        downloadPermission = selectMenu(in downloadMenuArray, in downloadMenuPrompt) switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 0,
        };
        // Only runs if the user selection was 2 or 3. If it was 1, program won't connect to the internet
        if (downloadPermission > 1)
        {
            // "using" makes sure that this HttpClient is cleaned up after using Dispose()
            // Creates a single client to then be used for either download type the user chose
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            try
            {
                // Downloads the json file from the project's github repo
                if (downloadPermission == 2)
                {
                    string jsonFile = await http.GetStringAsync(githubURL);
                    await File.WriteAllTextAsync("pTable.json", jsonFile);
                }
                // Calls neelpatel05's api to get the json of the periodic table
                else if (downloadPermission == 3)
                {
                    // Gets a string with the data in it from the URL
                    string jsonFile = await http.GetStringAsync(neelAPI);

                    // Writes that text out to the json file
                    await File.WriteAllTextAsync("pTable.json", jsonFile);
                }
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine($"Network Error: {hre}");
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
static int selectMenu(in string[] menuArray, in string prompt)
{
    int optionHighlighted = 0;
    bool selectionMade = false;

    // Displays an array to the user and lets the user control which one is highlighted
    // Up and Down arrows and Enter to select, returns selected option
    // Keeps looping until a selection has been made
    while (selectionMade == false)
    {
        drawHeader();
        Console.WriteLine(prompt);
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
    Console.WriteLine(prompt + "\n");
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
static List<(Element element, int atomCount)> moleculeParser(string inputString, in List<Element> pTable)
{
    List<(Element element, int atomCount)> molecule = [];

    // Makes two "groups" that we'll use to make our tuple, needs at least one letter for the element's symbol
    // The *'s indicate those components are optional, the element doesn't need a lowercase letter and a digit
    string regexPattern = @"([A-Z][a-z]*)(\d*)";
    foreach (Match pulledElement in Regex.Matches(inputString, regexPattern))
    {
        // The "Match" returns two groups, the symbol string and a string containing the number of atoms
        string inputSymbol = pulledElement.Groups[1].Value;
        string inputAtomCount = pulledElement.Groups[2].Value;

        // If there isn't an atom count in the second string, it's just a single atom.
        int atomCount;
        if (Int32.TryParse(inputAtomCount, out int atomParse) == true)
            atomCount = atomParse;
        else
            atomCount = 1;

        // Linearly searches the periodic table for the user provided elemental symbol
        foreach (Element elementInTable in pTable)
        {
            // When we find it, either add it to our tuple list or increase the atom count
            if (inputSymbol == elementInTable.Symbol)
            {
                // Finds the index of the tuple containing the Element if there is one
                int indexOfElement = molecule.FindIndex(tuple => tuple.element.Equals(elementInTable));

                // If we have a valid returned index, replace the value of that index with an updated tuple
                if (indexOfElement != -1)
                    molecule[indexOfElement] = (molecule[indexOfElement].element, molecule[indexOfElement].atomCount + atomCount);
                else
                    molecule.Add((elementInTable, atomCount));
            }
        }
    }
    return molecule;
}

// Uses info from RetrieveInfo to grab the molar masses from the data file
static double findMolarMass(in List<(Element element, int atomCount)> molecule, in List<Element> pTable)
{
    double massTotal = 0;
    // Iterate over the molecule passed, these are what we're finding the masses of and adding up
    foreach (var(element, atomCount) in molecule)
    {
        string elementMassString = element.AtomicMass;
        // Because the molar mass has a standard deviation in the string, we need to trim it before parsing it
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