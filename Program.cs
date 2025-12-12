// Jaden Olvera, CS-1400, Final Project: Chem Assistant

using System.Diagnostics;
using System.Text;
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
    // This line will read the file and parse it into a list of Element objects defined in Element.cs
    // Using '!' because null values have been accounted for
    pTableList = JsonSerializer.Deserialize<List<Element>>(File.ReadAllText(pTableFilePath))!;
}
// If it doesn't, don't even try to do anything else
else
    return;

// ---- Assertions ----
// Everything we need to run these has been loaded by this point, so we test them here
// The pTable List retrieved from the file should have 118 Element objects
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
Debug.Assert(findMolarMass(assertNaNO3, pTableList).ToString("F4") == "84.9947");
Debug.Assert(findMolarMass(assertHCN, pTableList).ToString("F4") == "27.0253");
// ----         ----

// Check for the save file and create it if it doesn't already exist
const string saveFilePath = "savedMolecules.csv";
if (File.Exists(saveFilePath) == false)
    File.Create(saveFilePath);

// Load all saved Molecules from the file into a list of strings
List<string[]> savedMolecules = [];
foreach(string line in File.ReadAllLines(saveFilePath))
{
    string[] lineArray = line.Split(',');
    savedMolecules.Add(lineArray);
}

// Create a prompt string and an array to give to the menu method later
string mainMenuPrompt = "What can ChemAssistant help with?";
string[] mainMenuArray = [  "1. Saved Molecules",
                            "2. Molar Mass Calculator",
                            "3. Element Look Up",
                            "4. Exit"
                        ];

bool exiting = false;
while (exiting == false)
{
    string userInput;
    List<(Element element, int atomCount)> molecule;
    // Handles the returned integer from the menu system
    switch (selectMenu(mainMenuArray, mainMenuPrompt, out int optionHighlighted))
    {
        // Saved Molecules Manager
        case 1:
            savedMoleculeMenu(ref savedMolecules);
            break;

        // Molar Mass Calculator
        case 2:
            // Collect molecule from user input
            userInput = userInputHandler("What molecule do you want the molar mass of?");

            // Parse userInput to a list of elements and atom counts
            molecule = moleculeParser(userInput, in pTableList);

            // Print out elements and counts and their masses
            foreach (var (element, atomCount) in molecule)
            {
                Console.WriteLine($"Element: {element.Name, -10} Atomic Mass: {element.AtomicMass, -15} x{atomCount, -3}");
            }

            // Parse the atomic mass property and total it up
            double molarMass = findMolarMass(in molecule, in pTableList);

            Console.WriteLine($"Molar Mass: {molarMass:F4}");
            Console.WriteLine("\nDo you want to save this molecule?\n[Y]es or [N]o");

            // If the user wants to save this molecule, we check if it's already in the file first
            if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    bool alreadySaved = false;
                    foreach (string[] line in savedMolecules)
                    {
                        if (line[0] == userInput)
                            alreadySaved = true;
                    }
                    if(alreadySaved == true)
                    {
                        Console.WriteLine("Molecule already saved!\nPress any key to continue.");
                    }
                    else
                    {
                        savedMolecules.Add([userInput, $"{molarMass:F4}"]);
                        saveMoleculeFile(saveFilePath, in savedMolecules);
                        Console.WriteLine("Molecule saved!\nPress any key to continue.");
                    }
                }
            Console.ReadKey(true);
            break;

        // Element Look Up
        case 3:
            userInput = userInputHandler("What elements do you want information on?");
            molecule = moleculeParser(userInput, in pTableList);
            foreach (var (element, atomCount) in molecule)
            {
                drawHeader();
                Console.WriteLine(userInput);
                Console.WriteLine($"\n{element.Name} ({element.Symbol})\n"
                    + $"Atomic Mass: {element.AtomicMass + " g·mol⁻¹",-25}"
                    + $"Electron Configuration: {element.ElectronConfig}\n"
                    + $"Group Block: {element.GroupBlock,-25}"
                    + $"Electronegativity: {element.Electronegativity.ToString() + " (Pauling)"}\n"
                    + $"Boiling Point: {element.BoilingPoint.ToString() + " K",-23}"
                    + $"Density: {element.Density.ToString() + " g·cm⁻³"}\n"
                    + $"Melting Point: {element.MeltingPoint} K"
                    );
                if (element != molecule[^1].element)
                    Console.WriteLine("\nPress any key to see the next element's info!");
                else
                    Console.WriteLine("\nPress any key to return to the main menu!");
                Console.ReadKey(true);
            }
            break;

        // Exit!
        case 4:
            Console.Clear();
            Console.WriteLine("Goodbye!");
            exiting = true;
            break;

        // Pressing the D or the left and right arrows keys on the main menu will just refresh the main menu
        // This is to account for those keys returning an integer that's used for the save menu
        case 280:
        case 281:
        case 282:
            break;
        default:
            exiting = true;
            break;
    }
}



// ---- Methods ----

// Checks if we already have a pTable.json to use, if we don't, ask for permission to retrieve it
static async Task fileGrab(string pTableFilePath)
{
    // Store path as a variable we can edit easily later if need be
    // Can be constant because this won't change during execution
    const string githubURL = "https://raw.githubusercontent.com/JOSnow985/ChemAssistant/refs/heads/main/pTable.json";

    if (File.Exists("pTable.json") == false)
    {
        string downloadMenuPrompt = "We couldn't find \"pTable.json\", and we need it!\nDo you want to download it? (Nothing will be done without permission!)";
        string[] downloadMenuArray = [  "1. Do NOT download anything",
                                        "2. Download the file from this project's github"
                                    ];

        // Switch that maps returned user selection to download permission int
        int downloadPermission;
        downloadPermission = selectMenu(in downloadMenuArray, in downloadMenuPrompt, out int optionHighlighted) switch
        {
            1 => 1,
            2 => 2,
            _ => 0,
        };
        // Only runs if the user selection was 2. If it was 1, program won't connect to the internet
        if (downloadPermission == 2)
        {
            // "using" makes sure that this HttpClient is cleaned up after using Dispose()
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            try
            {
                // Downloads the json file from the project's github repo
                string jsonFile = await http.GetStringAsync(githubURL);
                await File.WriteAllTextAsync("pTable.json", jsonFile);
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
static int selectMenu(in string[] menuArray, in string prompt, out int optionHighlighted)
{
    optionHighlighted = 0;
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
        // These three returns are for the save manager system
        // The values are high enough that they should never be accidentally returned
        else if (userInput == ConsoleKey.LeftArrow)
        {
            return 280;
        }
        else if (userInput == ConsoleKey.RightArrow)
        {
            return 281;
        }
        else if (userInput == ConsoleKey.D)
        {
            return 282;
        }
        // Makes a beep if the key pressed wasn't valid
        else
            Console.Beep();
    }

    // Return selected option, offset by 1 to account for zero indexing the array
    return optionHighlighted;
}

// Menu system for displaying a selection of saved molecules, list passed as a ref so we can edit it in the method
static void savedMoleculeMenu(ref List<string[]> savedMolecules)
{
    // A minimum and maximum index value so we only show so many saved molecules at a time
    int numberSaved = savedMolecules.Count;
    if (numberSaved == 0)
        return;
    int viewRangeMin = 0;
    int viewRangeMax = 8;
    // If there's not enough to fill every "slot", we set the max index to one less than the count, because it's zero indexed
    if (numberSaved < 8)
        viewRangeMax = numberSaved - 1;
    // Page counter to be used as a multiplier
    int currentPage = 1;
    bool exiting = false;
    string savedMoleculePrompt = "Here are the molecules you've saved!\nControls: <-: Previous Page or Exit   ->: Next Page   D: Delete";
    do
    {   // Checks to ensure Max and Min stay within a valid range
        if (currentPage * 8 < numberSaved)
            viewRangeMax = currentPage * 8;
        else
            viewRangeMax = numberSaved - 1;

        if (viewRangeMax - 8 < 0)
            viewRangeMin = 0;
        else
            viewRangeMin = viewRangeMax - 8;

        // Create a list to collect the strings we generate so it can later be passed to selectMenu
        List<string> createSavedMenu = [];

        // Using the minimum index as a start point, we grab lines up to our maximum
        for (int index = viewRangeMin; index <= viewRangeMax; index++)
        {
            createSavedMenu.Add($"{index + 1 + ".", -3} Molecule: {savedMolecules[index][0], -13} Molar Mass: {savedMolecules[index][1]}");
        }
        string[] savedMoleculeMenu = createSavedMenu.ToArray();
        
        // Presents the actual menu of our selected lines from the file
        switch (selectMenu(savedMoleculeMenu, savedMoleculePrompt, out int optionHighlighted))
        {
            // Deleting molecules should be deliberate so we ignore returned number keys
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                break;

            // The left arrow key will reduce the current page by 1 or kick us back to the main menu
            case 280:
                if (currentPage > 1)
                {
                    currentPage--;
                }
                else
                {
                    exiting = true;
                }
                break;

            // The right arrow key increases the current page by 1 or does nothing
            case 281:
                if (viewRangeMax + 1 < numberSaved)
                {
                    currentPage++;
                }
                break;

            // The D key begins the deletion process and presents a confirmation prompt
            case 282:
                Console.WriteLine("\nAre you sure you want to delete this saved molecule?");
                Console.WriteLine("[Y]es or [N]o");
                ConsoleKey response = Console.ReadKey().Key;
                // The text says N will say No but any input will intentionally avoid deleting the molecule
                if (response == ConsoleKey.Y)
                {
                    Console.WriteLine($"Removed {savedMolecules[viewRangeMin + optionHighlighted][0]}!");
                    savedMolecules.RemoveAt(viewRangeMin + optionHighlighted);
                    saveMoleculeFile(saveFilePath, savedMolecules);
                    // Updated number saved so we don't go out of range on accident
                    numberSaved = savedMolecules.Count;
                }
                break;

            default:
                exiting = true;
                break;

        }
    } while (exiting == false);
}

// Takes a list of string arrays and builds strings to save line by line into a csv
static void saveMoleculeFile(string filepath, in List<string[]> listToSave)
{
    List<string> saveList = [];
    foreach (string[] line in listToSave)
    {
        StringBuilder lineBuilder = new();
        foreach (string field in line)
            lineBuilder.Append(field + ",");
        saveList.Add(lineBuilder.ToString().TrimEnd(','));
    }
    string[] dataArray = saveList.ToArray();
    File.WriteAllLines(filepath, dataArray);
}

// Method that will print a prompt and take a user input to provide to other methods
static string userInputHandler(string prompt)
{
    while (true)
    {
    drawHeader();
    Console.WriteLine(prompt + "\n");
    // User input could be null or otherwise bad, so we need to validate
    string? userInput = Console.ReadLine();
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