using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace MacAuth
{
    //  Ported from image2pict1 by Steven Troughton-Smith (c) 2015
    //  https://github.com/steventroughtonsmith/image2pict1
    public class Image2PICT1
    {
        Stream _inputStream;

        public Image2PICT1(Stream inputStream)
        {
            _inputStream = inputStream;
        }

        struct Frame
        {
            public short x;
            public short y;
            public short x2;
            public short y2;
        };

        /* QuickDraw PICT v1.0 Opcodes */
        readonly byte clpRegion = 0x01;
        readonly byte picVersion = 0x11;

        readonly byte BitsRect = 0x90;
        readonly byte EndOfPicture = 0xff;

        byte HI(short num)
        {
            return (byte)(((num) & 0x0000FF00) >> 8);
        }

        byte LO(short num)
        {
            return (byte)((num) & 0x000000FF);
        }

        Frame FrameMake(short x, short y, short x2, short y2)
        {
            Frame f;
            f.x = x;
            f.y = y;
            f.x2 = x2;
            f.y2 = y2;
            return f;
        }

        void WriteWord(BinaryWriter writer, short word)
        {
            writer.Write(HI(word));
            writer.Write(LO(word));
        }

        void WriteFrame(BinaryWriter writer, Frame f)
        {
            // top, left, bottom, right
            WriteWord(writer, f.y);
            WriteWord(writer, f.x);
            WriteWord(writer, f.y2);
            WriteWord(writer, f.x2);
        }

        void WriteByte(BinaryWriter writer, byte bte)
        {
            writer.Write(bte);
        }

        void WritePicVersion(BinaryWriter writer, byte version)
        {
            WriteByte(writer, picVersion);
            WriteByte(writer, version);
        }

        void WriteClipRegion(BinaryWriter writer, Frame f)
        {
            WriteByte(writer, clpRegion);
            WriteWord(writer, 10);
            WriteFrame(writer, f);
        }

        void WriteEndOfPicture(BinaryWriter writer)
        {
            WriteByte(writer, EndOfPicture);
        }

        void WriteHeader(BinaryWriter writer)
        {
            for (int i = 0; i < 512; i++) // 512-byte blank header
            {
                WriteByte(writer, 0);
            }
        }

        public void Write(Stream outputStream) 
        {
            using (var sourceImage = new Bitmap(_inputStream))
            {
                var pictureFrame = FrameMake(0, 0, (short)sourceImage.Width, (short)sourceImage.Height);

                var writer = new BinaryWriter(outputStream);
                var canvasWidth = sourceImage.Width;
                var canvasHeight = sourceImage.Height;

                /* Since we're chunking the image into 32x32 pieces, add padding if image size not divisible */
                canvasWidth = (int)Math.Ceiling((float)canvasWidth / 32.0) * 32;
                canvasHeight = (int)Math.Ceiling((float)canvasHeight / 32.0) * 32;

                WriteHeader(writer);
                WriteWord(writer, 0);

                WriteFrame(writer, pictureFrame);
                WritePicVersion(writer, 1);
                WriteClipRegion(writer, pictureFrame);

                int chunkPxSize = 32;
                int bpp = 1;
                int bpr = bpp * chunkPxSize / 8;

                for (int y = 0; y < canvasHeight; y += 32)
                {
                    for (int x = 0; x < canvasWidth; x += 32)
                    {
                        /* Check if chunk has worthwhile pixels */
                        bool ignoreChunk = true;

                        for (int yy = 0; yy < chunkPxSize; yy++)
                        {
                            for (int xx = 0; xx < chunkPxSize; xx++)
                            {
                                if (x + xx < (int)sourceImage.Width && y + yy < (int)sourceImage.Height)
                                {
                                    var pixel = sourceImage.GetPixel(x + xx, y + yy);

                                    if (pixel.GetBrightness() < 0.98)
                                    {
                                        ignoreChunk = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!ignoreChunk)
                        {
                            WriteByte(writer, BitsRect);
                            WriteWord(writer, (short)bpr); // bpr
                            WriteFrame(writer, FrameMake(0, 0, (short)chunkPxSize, (short)chunkPxSize)); // bounds
                            WriteFrame(writer, FrameMake(0, 0, (short)chunkPxSize, (short)chunkPxSize)); // src
                            WriteFrame(writer, FrameMake((short)x, (short)y, (short)(x + chunkPxSize), (short)(y + chunkPxSize))); // dest
                            WriteWord(writer, 0); // mode=srcCpy

                            for (int yy = 0; yy < chunkPxSize; yy++)
                            {
                                for (int xx = 0; xx < chunkPxSize / 8; xx++)
                                {
                                    byte eightpixels = 0;

                                    for (int bit = 0; bit < 8; bit++)
                                    {
                                        eightpixels = (byte)(eightpixels << 1);

                                        if ((x + (xx * 8) + bit) < (int)sourceImage.Width && y + yy < (int)sourceImage.Height)
                                        {
                                            var pixel = sourceImage.GetPixel(x + (xx * 8) + bit, y + yy);

                                            if (pixel.GetBrightness() < 0.98)
                                            {
                                                eightpixels |= (byte)1;
                                            }
                                        }
                                    }

                                    WriteByte(writer, eightpixels);
                                }
                            }
                        }
                    }
                }

                WriteEndOfPicture(writer);
            }
	    }
    }
}
