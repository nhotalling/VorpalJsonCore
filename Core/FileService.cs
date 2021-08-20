using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace VorpalJsonCore
{
    public class FileService
    {
        private readonly string _deckPrefix = "deck-";
        private readonly string _counterPrefix = "counter-";

        private static readonly double _xStartingPosition = -525; 
        private static readonly double _yStartingPosition = -3960;
        // 1300 - 1700
        private static readonly double _yOffset = 1700;
        private readonly int _xMargin = 400;
        
        // Position is center of object
        private readonly Position _deckRowPosition = new (_xStartingPosition, _yStartingPosition);
        private readonly Position _assetRowPosition = new(_xStartingPosition, _yStartingPosition + _yOffset);

        // Vorpal doesn't seem to support folders. Although the deck
        // screen seemed to recognize images in folders, the deck was not
        // functional on the game screen itself.
        public List<FileInfo> GetFileInfo(string dir, bool topOnly = true)
        {
            var di = new DirectoryInfo(dir);
            var fileInfos = new List<FileInfo>();
            var scannedImages = new List<MetaData>();
            List<KeyValuePair<string, int>> imageLibrary;
            imageLibrary = new List<KeyValuePair<string, int>>();

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

            Deck currentDeck = new Deck();
            var currentDeckName = "";
            var setDeckDimensions = false;
            var imageNumber = 0;
            
            // TODO - WRAP FILES so they don't stretch forever across the canvas

            files.OrderBy(file => file.Name).ToList()
                .ForEach(file => {
                    // reset
                    var assetType = AssetType.Misc;
                    short qty = 1;
                    var counter = 0;
                    imageNumber++;

                    if (file.Name.StartsWith(_deckPrefix))
                    {
                        assetType = AssetType.Deck;
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
                            currentDeck.MetaData = Utils.CreateDeckMetaData(_deckRowPosition);
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
                                // offset Row1 for next deck
                                _deckRowPosition.x += image.Width + _xMargin;
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
                                var pos = new Position(1, 1);
                                if (assetType == AssetType.Deck)
                                {
                                    pos = currentDeck.MetaData.Position;
                                }
                                else if (assetType == AssetType.Misc)
                                {
                                    pos = new Position(Row2Position.x, Row2Position.y);
                                    // offset Row2 for next item
                                    Row2Position.x += image.Width + _xMargin;
                                }
                                else if (assetType == AssetType.Counter)
                                {
                                    pos = new Position(Row3Position.x, Row3Position.y);
                                    // offset Row2 for next item
                                    Row3Position.x += image.Width + _xMargin;
                                }

                                var newImageMetaData = Utils.CreateImageOrCounterMetaData(file, pos, imageNumber, image.Width, image.Height);
                                if (assetType == AssetType.Counter)
                                {
                                    newImageMetaData.PropBag = GetPropBagCounter(counter);
                                }
                                if (assetType == AssetType.Deck)
                                {
                                    // currentDeck.CardIndex.Add(newImageMetaData.ImageID);
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
            var nameSplit = name.Split('_');
            var deckName = nameSplit.First().Replace(_deckPrefix, "");
            return deckName;
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

        public void SaveFile(string filepath, string contents)
        {
            File.WriteAllText(filepath + @"\gamedata.json", contents);
        }

        // -------------------------------------------------------------------------------
        // ORIGINAL FOLDER CONCEPT

        // Vorpal did not seem to recognize folder paths for images
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
                    imageNumber++;

                    if (file.Directory != null && file.Directory.Name.StartsWith("deck"))
                    {
                        assetType = AssetType.Deck;
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
                                Position pos = new Position(Row2Position.x, Row2Position.y);

                                var newImageMetaData = Utils.CreateImageOrCounterMetaData(file, pos, imageNumber, image.Width, image.Height);
                                if (assetType == AssetType.Counter)
                                {
                                    newImageMetaData.PropBag = GetPropBagCounter(counter);
                                }
                                if (assetType == AssetType.Deck)
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
