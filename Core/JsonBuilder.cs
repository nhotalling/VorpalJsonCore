using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace VorpalJsonCore
{
    public static class VorpalJsonBuilder
    {
        public static string BuildJson(AssetCollection assets, string vorpalProjectName)
        {
            // todo - storage path by device type
            var fullStoragePath = Constants.DeviceStoragePath + vorpalProjectName;

            var root = new Root
            {
                StorageAbsolutePath = fullStoragePath,
                Name = vorpalProjectName
            };

            var date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffzz':00'");
            root.Date = date;

            var maxCardNumber = assets.ScannedImages.Max(img => img.ImageNumber);
            root.NextCardNumber = maxCardNumber + 1;

            root.ScannedImageMetaData =
                assets.ScannedImages.ToDictionary(imgMetaData => imgMetaData.ImageID, imgMetaData => imgMetaData);

            root.ImageLibrary = new Dictionary<string, short>();
            // todo - get rid of kvp
            assets.ImageLibrary.ForEach(kvp => {
                root.ImageLibrary.Add(kvp.Key, kvp.Value);
            });

            root.Decks = assets.DeckList.ToDictionary(deck => deck.Id, deck => deck);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(root, options);
        }
    }
}
