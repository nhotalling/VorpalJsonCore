using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace VorpalJsonCore
{
    public class FileService
    {
        private readonly string _deckPrefix = "deck-";
        private readonly string _deckBackPrefix = "deckback-";
        private readonly string _counterPrefix = "counter-";
        private readonly Dictionary<string, string> _deckBacks = new();

        // estimated using avg browser size of 1366 x 784
        // card image, portrait
        // to the right of the 'roll all' dice icon
        // x: -2850, y: -2110
            
        // below the 'roll all' dice icon
        // x: -3790, y: -1310

        // right edge, row 1
        // x: 6460, y: -2135
        // right edge, row 2
        // x: 6448, y: -1319

        private static readonly double _xStartingPositionDeckRow = -2850; 
        private static readonly double _yStartingPositionDeckRow = -2110;
        private static readonly double _xStartingPositionAssetRow = -3790; 
        private static readonly double _yStartingPositionAssetRow = -1310;
        // 1300 - 1700
        private static readonly double _yOffset = 1700;
        private readonly double _xOffset = 400;
        private readonly double _xRightBoundary = 6450;
        
        // Wrapping logic currently uses top row for decks
        // Second row will be all other images and counter assets and will wrap on the X axis at a given point
        // Position is center of object
        private static readonly Position _deckRowStartingPosition = new (_xStartingPositionDeckRow, _yStartingPositionDeckRow);
        private static readonly Position _assetRowStartingPosition = new(_xStartingPositionAssetRow, _yStartingPositionAssetRow);
        private Position _deckCurrentPosition = _deckRowStartingPosition;
        private Position _assetCurrentPosition = _assetRowStartingPosition;
        //private short _currentAssetNumber = 0;

        // During an early test, Vorpal didn't seem to support folders. Although the deck
        // screen seemed to recognize images in folders, the deck was not functional on the game screen itself.
        // (so images must be in a single folder)
        public List<FileInfo> GetFileInfo(string dir, bool topOnly = true)
        {
            var di = new DirectoryInfo(dir);
            var fileInfos = new List<FileInfo>();
            var scannedImages = new List<MetaData>();
            var imageLibrary = new List<KeyValuePair<string, int>>();

            string[] fileExt = { "*.jpg", "*.gif", "*.png" };

            fileExt.ToList().ForEach(ext => {
                var files = di.GetFiles(ext, topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
                if (files.Any())
                {
                    fileInfos.AddRange(files.ToList());
                }
            });
            return fileInfos;
        }
        
        public AssetCollection ProcessFiles(List<FileInfo> files)
        {
            var assets = new AssetCollection();

            var currentDeck = new Deck();
            var currentDeckName = "";
            var setDeckDimensions = false;
            var imageNumber = 0;
            
            // Gather back images
            files.Where(file => file.Name.StartsWith(_deckBackPrefix))
                .Select(file => file.Name)
                .ToList()
                .ForEach(fileName =>
                {
                    AddDeckBack(fileName);
                    // will be added in main image loop
                    //assets.ImageLibrary.Add(new KeyValuePair<string, short>(fileName, 1));
                });
            
            files
                //.Where(file => !file.Name.StartsWith(_deckBackPrefix))
                .OrderBy(file => file.Name).ToList()
                .ForEach(file => {
                    // reset
                    var assetType = AssetType.Misc;
                    short qty = 1;
                    var counter = 0;
                    //imageNumber++;

                    if (file.Name.StartsWith(_deckPrefix))
                    {
                        assetType = AssetType.DeckItem;
                        var deckName = GetDeckName(file.Name);
                        if (currentDeckName != deckName)
                        {
                            currentDeck = new Deck();
                            //if (deckName == "Deck")
                            //{
                            //    deckName += " " + currentDeck.Id;
                            //}
                            currentDeckName = deckName;
                            currentDeck.Name = deckName;

                            _deckBacks.TryGetValue(deckName, out var deckBackImage);

                            currentDeck.MetaData = Utils.CreateDeckMetaData(_deckCurrentPosition, deckBackImage);
                            assets.DeckList.Add(currentDeck);
                            setDeckDimensions = true;
                        }
                    }
                    else if (file.Name.StartsWith(_counterPrefix))
                    {
                        assetType = AssetType.Counter;
                    }

                    using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var image = Image.FromStream(fileStream, false, false))
                        {
                            if (setDeckDimensions)
                            {
                                setDeckDimensions = false;
                                currentDeck.MetaData.Height = image.Height;
                                currentDeck.MetaData.Width = image.Width;
                                // offset for next deck
                                _deckCurrentPosition.x += image.Width + _xOffset;
                            }

                            var nameSplit = file.Name.Replace(file.Extension, "").Split('_');
                            if (nameSplit.Length > 1)
                            {
                                short num = 0;
                                var hasNumber = short.TryParse(nameSplit.Last(), out num);
                                if (hasNumber && assetType == AssetType.Counter)
                                {
                                    counter = num;
                                }
                                else if (hasNumber)
                                {
                                    qty = num;
                                }
                            }

                            assets.ImageLibrary.Add(new KeyValuePair<string, short>(file.Name, qty));

                            for (var i = 0; i < qty; i++)
                            {
                                Position pos;
                                string backImage = null;
                                if (assetType == AssetType.DeckItem)
                                {
                                    pos = currentDeck.MetaData.Position;
                                    // check for back image in dictionary
                                    var deckName = GetDeckName(file.Name);
                                    _deckBacks.TryGetValue(deckName, out backImage);
                                }
                                else
                                {
                                    imageNumber++;
                                    pos = new Position(_assetCurrentPosition.x, _assetCurrentPosition.y);
                                    // offset asset for next item
                                    _assetCurrentPosition.x += image.Width + _xOffset;
                                    if (_assetCurrentPosition.x > _xRightBoundary)
                                    {
                                        // wrap to new row
                                        _assetCurrentPosition = new Position(_xStartingPositionAssetRow, _assetCurrentPosition.y + image.Height + _yOffset);
                                    }
                                }

                                var newImageMetaData = Utils.CreateImageOrCounterMetaData(file, pos, imageNumber, image.Width, image.Height, backImage);
                                if (assetType == AssetType.Counter)
                                {
                                    newImageMetaData.PropBag = GetPropBagCounter(counter);
                                }
                                if (assetType == AssetType.DeckItem)
                                {
                                    // CardIndex will be created from CardStack
                                    currentDeck.CardStack.Add(newImageMetaData.ImageID);
                                }
                                assets.ScannedImages.Add(newImageMetaData);
                            }

                        }
                    }
                });

            return assets;
        }

        private string GetDeckName(string name)
        {
            // deck-Dracula_filename_numberOfCards.jpg
            var nameSplit = name.Split('_');
            var deckName = nameSplit.First().Replace(_deckPrefix, "");
            return deckName;
        }
        
        private void AddDeckBack(string name)
        {
            // deckback-Dracula_filename.jpg
            var nameSplit = name.Split('_');
            if(nameSplit.Length != 2)
            {
                throw new Exception("Deck back name is invalid.");
            }
            var deckName = nameSplit.First().Replace(_deckBackPrefix, "");

            _deckBacks[deckName] = name;
        }

        private PropBag GetPropBagCounter(int value)
        {
            // counterEnabled and counterValue values are strings
            // "counterEnabled":"true",
            // "counterValue":"12"

            return new PropBag
            {
                CounterEnabled = "true",
                CounterValue = value.ToString()
            };
        }

        public void SaveGameDataFile(string filepath, string contents)
        {
            File.WriteAllText(filepath + @"\gamedata.json", contents);
        }

        // Vorpal did not seem to recognize folder paths for images
        [Obsolete]
        public AssetCollection ProcessFilesInFolders(List<FileInfo> files)
        {
            var assets = new AssetCollection();

            Deck currentDeck = new Deck();
            var currentDeckDirectory = "";
            var setDeckDimensions = false;
            var imageNumber = 0;

            files.OrderBy(file => file.DirectoryName).ToList()
                .ForEach(file => {
                    // reset
                    var assetType = AssetType.Misc;
                    short qty = 1;
                    var counter = 0;
                    //imageNumber++;

                    if (file.Directory != null && file.Directory.Name.StartsWith("deck"))
                    {
                        assetType = AssetType.DeckItem;
                        var deckName = GetDeckNameFromFolder(file.Directory.Name);
                        if (currentDeckDirectory != file.Directory.Name)
                        {
                            currentDeckDirectory = file.Directory.Name;
                            currentDeck = new Deck();
                            if (deckName == "Deck")
                            {
                                deckName += " " + currentDeck.Id;
                            }
                            currentDeck.Name = deckName;
                            // TODO - position offset
                            currentDeck.MetaData = Utils.CreateDeckMetaData(new Position(-1785, -2245));
                            assets.DeckList.Add(currentDeck);
                            setDeckDimensions = true;
                        }
                    }
                    else if (file.Directory != null && file.Directory.Name.StartsWith("counter"))
                    {
                        assetType = AssetType.Counter;
                    }

                    using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var image = Image.FromStream(fileStream, false, false))
                        {
                            if (setDeckDimensions)
                            {
                                setDeckDimensions = false;
                                currentDeck.MetaData.Height = image.Height;
                                currentDeck.MetaData.Width = image.Width;
                            }

                            var nameSplit = file.Name.Replace(file.Extension, "").Split('_');
                            if (nameSplit.Length > 1)
                            {
                                short num = 0;
                                var hasNumber = short.TryParse(nameSplit.Last(), out num);
                                if (hasNumber && assetType == AssetType.Counter)
                                {
                                    counter = num;
                                }
                                else if (hasNumber)
                                {
                                    qty = num;
                                }
                            }

                            // single instance of each image with its qty
                            // TODO - Can Vorpal handle folders?
                            var folderName = "";
                            if (assetType != AssetType.Misc)
                            {
                                folderName = file.Directory.Name + "/";
                            }
                            assets.ImageLibrary.Add(new KeyValuePair<string, short>(folderName + file.Name, qty));

                            for (short i = 0; i < qty; i++)
                            {
                                var pos = new Position(_assetRowStartingPosition.x, _assetRowStartingPosition.y);
                
                                var newImageMetaData = Utils.CreateImageOrCounterMetaData(file, pos, imageNumber, image.Width, image.Height);
                                if (assetType == AssetType.Counter)
                                {
                                    newImageMetaData.PropBag = GetPropBagCounter(counter);
                                }
                                if (assetType == AssetType.DeckItem)
                                {
                                    currentDeck.CardStack.Add(newImageMetaData.ImageID);
                                }
                                assets.ScannedImages.Add(newImageMetaData);
                            }

                        }
                    }
                });

            return assets;
        }

        private string GetDeckNameFromFolder(string name)
        {
            var nameSplit = name.Split('_');
            return nameSplit.Length > 1 ? nameSplit.Last() : "Deck";
        }
    }
}
