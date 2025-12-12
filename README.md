# ChemAssistant
A quick reference chemistry assistant program, my CS-1400 Final Project submission.

## End Goal
My vision for this program is a tool that would remove (or at least reduce) the tedium in certain aspects of chemistry. A tool that could calculate molar masses, tell you which bonds in a molecule are polar or ionic and if the molecule is polar or ionic overall, even formal charges and oxidation states could be something it could handle for a chemist. There's plenty of features that could fall into this scope but in my experience, these are the ones that I found tedious and thought we be a good fit for the project.

## Why Am I Interested?
During this semester, I immediately found totaling molar masses tedious. I thought that task could have been easily automated, but I didn't see that feature on Kalzium, a periodic table program that's part of KDE, or any other periodic table program I tried. I got better at totaling molar masses, memorizing some element's numbers, but it remained one of the more tedious parts of learning chemistry for me. When I started thinking about my final project, that came to mind instantly. It's something that is useful to me so it's interesting!

## What was attempted and what was accomplished?
I focused on my initial desire for a molar mass calculator first and foremost. This was the functionality that inspired the project and I felt it should be the main goal. I also wanted to involve an API call and a saving function so you could store molecules you reference a lot. A third feature I decided upon was displaying information about an element, things like their electron configuration or density. If I already had a file with information about the elements in it, why not use that? 

The program as it is now reads from a json file with elemental data, or acquires the json file if it doesn't find it. An Element object class is constructed using the json properties I found useful and worth displaying, loaded into memory as a List of Elements. 

The saving and loading concept is accomplished using a "Saved Molecules" csv that is read and modified in memory, and displayed to the user in a "scrolling" (it's more paged) list. The user can delete molecules from this menu. 

The Molar Mass Calculator is accomplished using a user-provided string, parsed using a Regex pattern, with matches being compared to the pTable Element list and finally totaling the atomic masses. If the molar mass is above zero here, the user is able to save the molecule they provided with the calculated molar mass to the Saved Molecules csv. If it already exists in the csv, the user is informed and the molecule is not saved.

The "Element Look Up" feature is accomplished using the same user input processing method as the Molar Mass Calculator, but instead of performing a sum, it pulls the properties of the parsed Elements and displays them in an info table format. Each element is displayed left to right, one at a time, duplicates aren't shown twice.

The API call function was more or less a failure. The documentation of the API I was intending to use lead me to believe that the project was hosted at the developer's website but I either made errors in trying to call it or it isn't actually running anymore. The original intention was to check for the pTable.json file and prompt the user to get it if it wasn't found. Three options were presented, an option to deny permission to connect to the internet, an option to pull the file from this project's github repo, or to call neelpatel05's API to retrieve it. The functionality of asking the user for permission to download the file remains in the code, but the only option for file retrieval is the project's repo. 

## What was learned
I learned a lot, both from research and discovery during the coding process. I hadn't used in/out/ref on my methods before and I probably didn't use them to their fullest potential here, but they enabled me to pull some cool things off that I wouldn't have been able to otherwise. I now have a basic understanding of working with .json files and how to construct an object from one, and the basics of a class. Objects having properties with typing already accounted for was something that was very convenient and could have been useful on prior projects. I had a passing familiarity with Regex but this project contains my first successful use of it, although the pattern remains limited in scope. It also lead me to discover resources on learning more about and improving my patterns, so while I haven't learned or mastered it (yet!), I did learn of some paths that look great for learning it.

## Acknowledgements
Despite not being able to call neelpatel05's API, I did retrieve the json from their project and without it, I wouldn't have had anything to display or at least I would have had to use a file that wasn't as cleanly laid out as the one they had.
https://github.com/neelpatel05/periodic-table-api