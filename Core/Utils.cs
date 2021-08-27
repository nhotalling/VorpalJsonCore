using System;
using System.IO;

namespace VorpalJsonCore
{
    public static class Utils
    {
        public static MetaData CreateImageOrCounterMetaData(FileInfo src, Position pos, int imageNumber, int w, int h, string backImagePath = null)
        {
            return new MetaData
            {
                Position = pos,
                Scale = 1.75,
                Rotation = 0,
                OwnerID = null,
                OwnerColor = null,
                Show = true,
                ShowColor = null,
                ImageNumber = imageNumber,
                ImageID = Guid.NewGuid().ToString(),
                FilePath = src.Name,
                BackPath = backImagePath,
                Width = w,
                Height = h,
                FileType = src.Extension.Replace(".", "").ToUpper(),
                Locked = false,
                //PropBag = new PropBag()
            };
        }

        public static MetaData CreateDeckMetaData(Position pos, string backImagePath = null)
        {
            return new MetaData
            {
                Position = pos,
                Scale = 1.75,
                Rotation = 0,
                OwnerID = null,
                OwnerColor = null,
                Show = false,
                ShowColor = null,
                ImageNumber = 0,
                ImageID = null,
                FilePath = null,
                BackPath = backImagePath,
                // width and height will be set via first card in deck
                Width = 0,
                Height = 0,
                FileType = null,
                Locked = false,
                //PropBag = new PropBag()
            };
        }
    }
}
