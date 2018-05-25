using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SheepReaper.Neural.MNISTReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("\nBegin\n");

                var ImageCollection = new IdxDataSet();
                ImageCollection.Load(@"C:\temp\t10k-images.idx3-ubyte", @"C:\temp\t10k-labels.idx1-ubyte");

                foreach (var image in ImageCollection)
                {
                    Console.WriteLine(image.ToAscii());
                }

                Console.WriteLine("\nEnd\n");
                Console.ReadLine();
            }finally {}
            
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    throw ex;
            //    Console.ReadLine();
            //}
        }
    }

    internal class IdxDataSet : List<CharacterMap>
    {
        public string PathToDecompressedImageFile { get; set; }
        public string PathToDecompressedLabelFile { get; set; }
        public CharacterMapDimensions ImageDimensions { get; private set; } = new CharacterMapDimensions();

        public void Load(string pathToDecompressedImageFile, string pathToDecompressedLabelFile)
        {
            var fsLabels = new FileStream(PathToDecompressedLabelFile = pathToDecompressedLabelFile, FileMode.Open);
            var fsImages = new FileStream(PathToDecompressedImageFile = pathToDecompressedImageFile, FileMode.Open);
            var binLabels = new BinaryReader(fsLabels);
            var binImages = new BinaryReader(fsImages);
            binImages.ReadInt32();
            var numImages = binImages.ReadBigInt32();
            var numRows = ImageDimensions.Rows = binImages.ReadBigInt32();
            var numCols = ImageDimensions.Cols = binImages.ReadBigInt32();
            binLabels.ReadInt32();
            binLabels.ReadInt32();

            var pixels = Utils.EmptyByteArray(numRows, numCols);

            for (var imageIndex = 0; imageIndex < numImages; imageIndex++)
            {
                for (var rowIndex = 0; rowIndex < numRows; rowIndex++) 
                    pixels[rowIndex] = binImages.ReadBytes(numCols);

                Add(new CharacterMap(pixels, binLabels.ReadByte()));
            }

            fsImages.Close();
            fsLabels.Close();
            binImages.Close();
            binLabels.Close();

            ImageDimensions = new CharacterMapDimensions(numRows, numCols);
        }

        public void Stream()
        {
        }
    }

    internal class Utils
    {
        public static byte[][] EmptyByteArray(int numRows, int numCols)
        {
            var newByteArray = new byte[numRows][];
            for (var rowIndex = 0; rowIndex < numRows; rowIndex++) newByteArray[rowIndex] = new byte[numCols];

            return newByteArray;
        }
    }

    internal class CharacterMap
    {
        private byte[][] _pixels;

        public CharacterMap(byte[][] pixels)
        {
            _pixels = pixels;

            Dimensions = new CharacterMapDimensions(pixels.Length, pixels[0].Length);
        }

        public CharacterMap(byte[][] pixels, byte label) : this(pixels)
        {
            Label = label;
        }

        public CharacterMap(int numRows, int numCols) : this(Utils.EmptyByteArray(numRows, numCols))
        {
        }

        public byte Label { get; set; }
        public CharacterMapDimensions Dimensions { get; }

        public byte[][] Pixels
        {
            get => _pixels;
            set
            {
                _pixels = value;
                Dimensions.Rows = value.Length;
                Dimensions.Cols = value[0].Length;
            }
        }

        public override string ToString()
        {
            return ToAscii();
        }

        public string ToAscii(bool includeLabel = false)
        {
            var outputString = "";
            foreach (var rowElement in Pixels)
            {
                foreach (var pixel in rowElement)
                    switch (pixel)
                    {
                        case 0:
                            outputString += " "; // white
                            break;
                        case 255:
                            outputString += "O"; // black
                            break;
                        default:
                            outputString += "."; // gray
                            break;
                    }

                outputString += "\n";
            }

            outputString += includeLabel ? Label.ToString() : "";

            return outputString;
        }
    }

    internal class CharacterMapDimensions
    {
        private readonly int[] _dimensions;

        public CharacterMapDimensions()
        {
            _dimensions = new int[2];
        }

        public CharacterMapDimensions(int rows, int cols)
        {
            _dimensions = new[] {rows, cols};
        }

        public int Rows
        {
            get => _dimensions[0];
            set => _dimensions[0] = value;
        }

        public int Cols
        {
            get => _dimensions[1];
            set => _dimensions[1] = value;
        }
    }

    internal static class Extensions
    {
        public static int ReadBigInt32(this BinaryReader br)
        {
            var bytes = br.ReadBytes(sizeof(int));
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}