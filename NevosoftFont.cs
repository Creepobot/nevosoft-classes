using System;
using System.IO;

namespace Nevosoft
{
    /// <summary>
    /// Glyph data from <see cref="FontV2"/>
    /// </summary>
    public struct GlyphData
    {
        /// <summary>
        /// Glyph width in pixels
        /// </summary>
        public ushort GlyphWidth;
        /// <summary>
        /// Symbol that denotes this glyph
        /// </summary>
        public char Glyph;
    }

    /// <summary>
    /// Font class from Nevosoft games after 2007. Has the ".dat" extension<br/>
    /// In addition should have an image with characters, but there is no method to create it
    /// </summary>
    public class FontV2
    {
        /// <summary>
        /// Width of additional texture
        /// </summary>
        public ushort TextureWidth;
        /// <summary>
        /// Height of additional texture
        /// </summary>
        public ushort TextureHeight;
        /// <summary>
        /// Number of glyphs in each row
        /// </summary>
        public uint GlyphsPerRow;
        /// <summary>
        /// Array of <see cref="GlyphData"/> in the class (maximum 1024)
        /// </summary>
        /// <returns>
        /// <see cref="GlyphData"/> at the given index
        /// </returns>
        public GlyphData[] Glyphs = new GlyphData[1024];

        /// <summary>
        /// Create object of class
        /// </summary>
        public FontV2() { }
        /// <summary>
        /// Open font from file
        /// </summary>
        public FontV2(string filename)
        {
            Load(filename);
        }
        /// <summary>
        /// Open font from stream
        /// </summary>
        public FontV2(Stream stream)
        {
            Load(stream);
        }

        /// <summary>
        /// Create object of class from the given file
        /// </summary>
        public FontV2 FromFile(string filename)
        {
            return new FontV2(filename);
        }
        /// <summary>
        /// Create object of class from the given stream
        /// </summary>
        public FontV2 FromStream(Stream stream)
        {
            return new FontV2(stream);
        }

        /// <summary>
        /// Save font to file
        /// </summary>
        public bool Save(string filename)
        {
            try
            {
                bool Result = false;
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    Result = Save(fs);
                return Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load font from file
        /// </summary>
        public bool Load(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File \"" + filename + "\" not found!");
            try
            {
                using (FileStream FS = new FileStream(filename, FileMode.Open))
                    return Load(FS);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load font from stream
        /// </summary>
        public bool Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!(stream.CanRead && stream.CanSeek))
                throw new FileLoadException("Stream reading or seeking is not avaiable!");
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    TextureWidth = br.ReadUInt16();
                    TextureHeight = br.ReadUInt16();
                    GlyphsPerRow = br.ReadUInt32();
                    uint glyphsPerColumn = GlyphsPerRow;
                    if (TextureWidth > TextureHeight)
                        glyphsPerColumn /= 2;
                    uint glyphCount = GlyphsPerRow * glyphsPerColumn;
                    for (uint i = 0; i < glyphCount; i++)
                    {
                        Glyphs[i].GlyphWidth = br.ReadUInt16();
                        Glyphs[i].Glyph = (char)br.ReadUInt16();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save font to stream
        /// </summary>
        public bool Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!(stream.CanWrite && stream.CanSeek))
                throw new FileLoadException("Stream writing or seeking is not avaiable!");
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(TextureWidth);
                    bw.Write(TextureHeight);
                    bw.Write(GlyphsPerRow);
                    foreach (GlyphData i in Glyphs)
                    {
                        bw.Write(i.GlyphWidth);
                        bw.Write((ushort)i.Glyph);
                    }
                    bw.Seek(1023 * 4 + 8, SeekOrigin.Begin);
                    bw.Write(0);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}