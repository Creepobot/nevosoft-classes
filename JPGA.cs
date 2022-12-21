using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Nevosoft
{
    /// <summary>
    /// One of the texture formats. Has "_a_" at the beginning of the filename and ".jpg" extension<br/>
    /// Fun fact: The abbreviation JPGA was not invented by the developers themselves, but by me. Originally it meant JPG with TGA, but later I was told that JPG-Alpha sounds better
    /// </summary>
    public class JPGA
    {
        /// <summary>
        /// Texture image
        /// </summary>
        public Bitmap Source;

        /// <summary>
        /// Create object of class
        /// </summary>
        public JPGA() { }
        /// <summary>
        /// Open texture from file
        /// </summary>
        public JPGA(string filename) => Load(filename);
        /// <summary>
        /// Open texture from <see cref="Stream"/>
        /// </summary>
        public JPGA(Stream stream) => Load(stream);
        /// <summary>
        /// Open texture from <see cref="Bitmap"/>
        /// </summary>
        public JPGA(Bitmap bitmap) => Source = bitmap;

        /// <summary>
        /// Create object of class from file
        /// </summary>
        public static JPGA FromFile(string filename) =>
            new JPGA(filename);
        /// <summary>
        /// Create object of class from <see cref="Stream"/>
        /// </summary>
        public static JPGA FromStream(Stream stream) =>
            new JPGA(stream);
        /// <summary>
        /// Create object of class from <see cref="Bitmap"/>
        /// </summary>
        public static JPGA FromBitmap(Bitmap bitmap) =>
            new JPGA(bitmap);

        /// <summary>
        /// Save texture to file
        /// </summary>
        public void Save(string filename)
        {
            using (FileStream fs = new FileStream(filename,
                FileMode.Create, FileAccess.Write, FileShare.None))
                Save(fs);
        }

        /// <summary>
        /// Save texture to <see cref="Stream"/>
        /// </summary>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!(stream.CanWrite && stream.CanSeek))
                throw new FileLoadException("Stream writing, reading or seeking is not avaiable!");
            stream.Seek(0, SeekOrigin.Begin);
            int l = 0;
            using (Stream image = new MemoryStream())
            {
                var encoder = ImageCodecInfo.GetImageEncoders()
                    .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encParams = new EncoderParameters(3);
                encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                encParams.Param[1] = new EncoderParameter(Encoder.Compression,
                    (long)EncoderValue.CompressionNone);
                encParams.Param[2] = new EncoderParameter(Encoder.ColorDepth, 32L);
                Source.Save(image, encoder, encParams);
                image.Seek(0, SeekOrigin.Begin);
                byte[] FFD9 = new byte[] { 0xFF, 0xD9 };
                while (true)
                {
                    var latestbyte = image.ReadByte();
                    if (latestbyte == -1)
                    {
                        stream.Seek(-1, SeekOrigin.End);
                        stream.WriteByte(0xD9);
                        break;
                    }
                    if (latestbyte == FFD9[l])
                    {
                        l++;
                        if (l == FFD9.Length)
                            latestbyte = 216;
                    }
                    else
                        l = 0;
                    stream.WriteByte((byte)latestbyte);
                }
            }

            byte[] alpha = new byte[1000000];
            l = 0;
            for (int i = 0; i < Source.Height; i++)
            {
                for (int j = 0; j < Source.Width; j++)
                {
                    alpha[l] = Source.GetPixel(j, i).A;
                    l++;
                    if (l >= alpha.Length)
                    {
                        stream.Write(alpha, 0, alpha.Length);
                        Array.Clear(alpha, 0, alpha.Length);
                        l = 0;
                    }
                }
            }
            stream.Write(alpha, 0, l);
        }

        private void Load(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File \"" + filename + "\" not found!");
            using (FileStream FS = new FileStream(filename, FileMode.Open))
                Load(FS);
        }

        private void Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!(stream.CanRead && stream.CanSeek))
                throw new FileLoadException("Stream reading or seeking is not avaiable!");
            Bitmap refb = (Bitmap)Image.FromStream(stream);
            Source = refb.Clone(new Rectangle(0, 0, refb.Width, refb.Height), PixelFormat.Format32bppArgb);
            stream.Seek(0, SeekOrigin.Begin);
            int l = 0;
            byte[] buffer = new byte[1000000];
            byte[] FFD9 = new byte[] { 0xFF, 0xD9 };
            stream.Read(buffer, 0, 1000000);
            while (FindBytes(buffer, FFD9) == -1)
            {
                if (stream.Position < stream.Length)
                {
                    Array.Clear(buffer, 0, 999999);
                    buffer[0] = buffer[999999];
                    stream.Read(buffer, 1, 999999);
                    l++;
                }
                else { break; }
            }
            stream.Position = (999999 * l) + FindBytes(buffer, FFD9) + 2;

            if (stream.Length - stream.Position != Source.Height * Source.Width)
                throw new FileLoadException("File is not a JPGA texture");

            for (int i = 0; i < Source.Height; i++)
            {
                for (int j = 0; j < Source.Width; j++)
                {
                    Color p = Source.GetPixel(j, i);
                    Source.SetPixel(j, i, Color.FromArgb(stream.ReadByte(), p.R, p.G, p.B));
                }
            }
        }

        private static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                    matchIndex = src[i] == find[0] ? 1 : 0;
            }
            return index;
        }
    }
}