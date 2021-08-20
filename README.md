# VorpalJsonCore
Cross platform app for turning images into a Vorpal Board `gamedata.json` file

## Steps:
- Gather game images. One option is pulling images from Tabletop Simulator.
  - Subscribe to game in Tabletop Simulator Workshop
  - Use npm package `ttsbackup` to pull down images from selected TTS game
  - Use image splitter to divide deck image into individual cards
- Rename images by type (image, deck, counter, etc.) (see Naming Conventions)
- Download Vorpal Json Core app (may require .NET Core)
- TODO - .NET CORE INSTRUCTIONS
- Run `VorpalConsole.exe` .NET Core console app to generate `gamedata.json` file
- Sync files to your phone. One option for Android:  
  - Create a folder in Google Drive to hold the game images and `gamedata.json`
  - Sync files to phone using Drive Sync

## Gather Images

TODO - 

## Prepare Files

### Naming conventions:

 - An image with N copies: `{image-name}_{N}.{jpg|png|gif}`  
Example: `meeple-red_2.jpg`
 - An image that belongs in a deck with N copies in the deck: `deck-{deck-name}_{image-name}_{N}.{jpg|png|gif}`  
Example: `deck-Vivian (Blue)_move-1_1`
 - Image to be used as a counter: `counter-{imagename}_{startingValue}.{jpg|png|gif}`

## Resources

[Node package 'ttsbackup'](https://www.npmjs.com/package/ttsbackup/v/0.0.7) - Extract files from Tabletop Simulator games 
Autosync (aka Drive Sync Pro) for Google Drive: https://play.google.com/store/apps/de...  
Image splitter: https://www.imgonline.com.ua/eng/cut-...  
VorpalJson: http://bit.ly/VorpalJson  

Vorpal Board: https://www.vorpalboard.com/  
Restoration Games - Unmatched - Cobble & Fog: https://restorationgames.com/cobble-a...  
[Tabletop Simulator Workshop](https://steamcommunity.com/workshop/browse/?appid=286160)

