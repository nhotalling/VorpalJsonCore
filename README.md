# VorpalJsonCore
Cross platform app for turning images into a Vorpal Board `gamedata.json` file.
Currently only supports creating gamedata for Android.

## Steps:
- Gather game images. One option is pulling images from Tabletop Simulator.
  - Subscribe to game in Tabletop Simulator Workshop
  - Use npm package `ttsbackup` to pull down images from selected TTS game
  - Use image splitter to divide deck image into individual cards
- Rename images by type (image, deck, counter, etc.) (see Naming Conventions)
- Install .NET 5 if your system does not have it (see Resources)
- Download Vorpal Json Core app
- Run `VorpalJsonConsole.exe` .NET Core console app to generate `gamedata.json` file
  - Enter the Vorpal Board game name when prompted (spaces are okay). This will be the name of the folder
  you create later to hold the game files.
  - Enter the folder path on your computer where the prepared images are.
- Manually add files to your phone or use a program to sync files to your phone.
  (see Android/Google Drive sync example below)

## Prepare Files

### Naming conventions:

 - An image with N copies: `{image-name}_{N}.{jpg|png|gif}`  
Example: `meeple-red_2.jpg`
 - An image that belongs in a deck with N copies in the deck: `deck-{deck-name}_{image-name}_{N}.{jpg|png|gif}`  
Example: `deck-Vivian (Blue)_move-1_1`
 - Image to be used as a counter: `counter-{imagename}_{startingValue}.{jpg|png|gif}`

## Android/Google Drive Sync Example
This example follows the steps needed to sync from Google Drive to an Android device using DriveSync Pro 
(also called Autosync - I personally use the paid version).

- Set up folder on Google Drive to hold all Vorpal Board game folders and files, e.g. `DriveSyncFiles`.
- On DriveSync Pro/Autosync, set up folder sync that connects your Google Drive `DriveSyncFiles`
  folder to `[Internal storage]/Android/data/com.Vorpal/files/Documents/games`. As you're clicking through 
  directories, you'll have to stop and grant permission to access the `data` folder. For settings, 
  I use Two-Way sync, uncheck all options, and enable folder pair.
- After preparing your image files and generating the gamedata.json file, run `VorpalJsonConsole.exe`
  to generate the `gamedata.json` file. (The Vorpal Board game name you enter on this step will be used in the next step.)
- On Google Drive, create a folder inside of the `DriveSyncFiles` folder that matches the Vorpal Board game name.
  (If you entered "My Game" as the game name, create a folder called "My Game")
- Copy all of the prepared images and the newly generated `gamedata.json` file to your new Google Drive folder.
- Sync the files to your phone by clicking the sync button in DriveSync Pro.
- Open the Vorpal Board app and your new game should appear in the list.

## Resources

- .NET 5: https://dotnet.microsoft.com/download/dotnet/5.0
- VorpalJsonCore console app (compiled version): http://bit.ly/VorpalJson
- Slightly outdated Vorpal Json overview video: https://www.youtube.com/watch?v=DlpihhO-BEI
- Node package 'ttsbackup' - Extract files from Tabletop Simulator games: https://www.npmjs.com/package/ttsbackup/v/0.0.7 
- Autosync (aka DriveSync Pro) for Google Drive: https://play.google.com/store/apps/details?id=com.ttxapps.drivesync 
- Image splitter: https://www.imgonline.com.ua/eng/cut-photo-into-pieces-result.php  
- Vorpal Board: https://www.vorpalboard.com/  
- Restoration Games - Unmatched - Cobble & Fog: https://restorationgames.com/cobble-and-fog
- Tabletop Simulator Workshop: https://steamcommunity.com/workshop/browse/?appid=286160

