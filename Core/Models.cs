using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace VorpalJsonCore
{
    public class AssetCollection
    {
        public List<KeyValuePair<string, short>> ImageLibrary { get; set; } = new();
        public List<Deck> DeckList { get; set; } = new();
        public List<MetaData> ScannedImages { get; set; } = new();
    }

    public static class Constants
    {
        // Legacy Android path
        //public static string DeviceStoragePath => "/storage/emulated/0/Vorpal Board/CardCache/";
        public static string AndroidDeviceStoragePath => "/storage/emulated/0/Android/data/com.Vorpal/files/Documents/games/";
    }

    /// <summary>
    /// For images and decks
    /// </summary>
    public class MetaData
    {
        public Position Position { get; set; }
        public double Scale { get; set; }
        public int Rotation { get; set; }
        public string OwnerID { get; set; }
        public string OwnerColor { get; set; }
        public bool Show { get; set; }
        public string ShowColor { get; set; }
        /// <summary>
        /// 0 for deck
        /// </summary>
        public int ImageNumber { get; set; }
        /// <summary>
        /// Used in cardIndex, etc.
        /// Unique for each card, even if they have the same FilePath
        /// Null for deck
        /// </summary>
        public string ImageID { get; set; }
        /// <summary>
        /// Null for deck
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Image name for deck's back image
        /// </summary>
        public string BackPath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// PNG | JPG | etc. Null for deck
        /// </summary>
        public string FileType { get; set; }
        public bool Locked { get; set; }

        /// <summary>
        /// Empty object unless counter
        /// </summary>
        public object PropBag { get; set; } = new();
        /// <summary>
        /// Indicates if the image still needs to be uploaded to the cloud from the device
        /// </summary>
        public bool CloudFile { get; set; } = false;
        /// <summary>
        /// False = drag and drop (uploaded via browser)
        /// </summary>
        public bool PhoneToCloud { get; set; } = true;
    }

    public class PropBag {
        // these are intentionally strings to match model although we could probably set up serialization rules
        [JsonPropertyName("counterEnabled")]
        public string CounterEnabled { get; set; }
        
        [JsonPropertyName("counterValue")]
        public string CounterValue { get; set; }
    }

    public class Root
    {
        public string StorageAbsolutePath { get; set; }
        //public string Id => Guid.NewGuid().ToString();
        public string Name { get; set; }
        // "2020-08-12T19:26:58.756746-05:00"
        public string Date { get; set; }
        public Dictionary<string, MetaData> ScannedImageMetaData { get; set; }
        public object StreamImageMetaData { get; set; } // null
        public object VideoMetaData { get; set; } // null
        // next highest imageNumber
        public int NextCardNumber { get; set; }
        // cards not in scanned card list - leave as empty array
        public List<int> CardStragglerNumbers { get; set; } = new();

        public object Players { get; set; } = new();
        public Dictionary<string, short> ImageLibrary { get; set; }

        public Dictionary<string, Deck> Decks { get; set; }
        
        /// <summary>
        /// 0 normal, 1 set
        /// </summary>
        public short Type { get; set; } = 0;
    }

    public class Position
    {
        public double x { get; set; }
        public double y { get; set; }

        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /*
   "Players":{
      "2b8bd481-2542-4452-9b4a-ff51b8a528e8":{
         "name":"Nostalgic Minsky",
         "color":"#9c27b0",
         "playerId":"2b8bd481-2542-4452-9b4a-ff51b8a528e8"
      },
      "96832a2b-2af2-4087-bf79-232165aba0ef":{
         "name":"Sad Buck",
         "color":"#ffeb3b",
         "playerId":"96832a2b-2af2-4087-bf79-232165aba0ef"
      },
      "playerGuid" : { PlayerDetails }
      }
    */

    public class PlayerDetails
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string PlayerId { get; set; }
    }

    //    public class Players
    //    {
    //        public 2b8bd481254244529b4aFf51b8a528e8 2b8bd481-2542-4452-9b4a-ff51b8a528e8 { get; set; }
    //    public 96832a2b2af24087Bf79232165aba0ef 96832a2b-2af2-4087-bf79-232165aba0ef { get; set; }
    //public Eae6bb63Aa7848a78a04F1e76ab0b398 eae6bb63-aa78-48a7-8a04-f1e76ab0b398 { get; set; } 
    //    }

    public class Deck
    {
        [JsonPropertyName("id")]
        public string Id { get; } = Guid.NewGuid().ToString();
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("metaData")]
        public MetaData MetaData { get; set; } = new();
        /// <summary>
        /// list of kvp (cardId, emptyString), not an array like the others
        /// </summary>
        [JsonPropertyName("cardIndex")]
        public Dictionary<string, string> CardIndex
        {
            get
            {
                return CardStack.ToDictionary(card => card, x => string.Empty);
            }
        }
        [JsonPropertyName("cardStack")]
        public List<string> CardStack { get; set; } = new();
        [JsonPropertyName("discardStack")]
        public List<string> DiscardStack { get; set; } = new();
    }

    public enum AssetType
    {
        Counter,
        DeckItem,
        Misc,
    }

}
