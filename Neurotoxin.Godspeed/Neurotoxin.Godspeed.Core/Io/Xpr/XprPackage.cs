using System;
using System.Drawing;
using System.IO;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Xpr
{
    public class XprPackage : BinaryModelBase
    {
        [BinaryData(4, "ascii")]
        public virtual string Magic { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int FileSize { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int HeaderSize { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int TextureCommon { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int TextureData { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int TextureLock { get; set; }

        [BinaryData]
        public virtual byte TextureMisc1 { get; set; }

        [BinaryData(1)]
        public virtual XprFormat TextureFormat { get; set; }

        [BinaryData(1)]
        public virtual byte TextureRes1 { get; set; }

        [BinaryData(1)]
        public virtual byte TextureRes2 { get; set; }

        public byte[] Image
        {
            get { return Binary.ReadBytes(HeaderSize, FileSize - HeaderSize); }
        }

        public int Width
        {
            get { return (int) Math.Pow(2.0, TextureRes2); }
        }

        public int Height
        {
            get { return (int)Math.Pow(2.0, TextureRes2); }
        }

        public bool IsValid
        {
            get { return Magic == "XPR0"; }
        }

        public XprPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public Image DecompressImage()
        {
            switch (TextureFormat)
            {
                case XprFormat.Dxt1:
                    return DecompressDxt1();
                case XprFormat.Argb:
                    return DecompressArgb();
                default:
                    return new Bitmap(Width, Height);
            }
        }

        private Image DecompressDxt1()
        {
            var image = Image;
            var width = Width;
            var height = Height;
            var img = new Bitmap(width, height);

            var num = (width + 3) / 4;
            var num2 = (height + 3) / 4;
            long sourceIndex = 0;
            for (var i = 0; i < num2; i += 1)
            {
                for (var j = 0; j < num; j += 1)
                {
                    var destinationArray = new byte[8];
                    Array.Copy(image, sourceIndex, destinationArray, 0, 8);
                    sourceIndex += 8;
                    DecompressBlockDxt1(j * 4, i * 4, width, destinationArray, img);
                }
            }
            return img;
        }

        private void DecompressBlockDxt1(int x, int y, int width, byte[] image, Bitmap img)
        {
            var num = BitConverter.ToUInt16(image, 0);
            var num2 = BitConverter.ToUInt16(image, 2);
            var num3 = (ulong)(((num >> 11) * 0xff) + 0x10);
            var r = (byte)(((num3 / 0x20) + num3) / 0x20);
            num3 = (ulong)((((num & 0x7e0) >> 5) * 0xff) + 0x20);
            var g = (byte)(((num3 / 0x40) + num3) / 0x40);
            num3 = (ulong)(((num & 0x1f) * 0xff) + 0x10);
            var b = (byte)(((num3 / 0x20) + num3) / 0x20);
            num3 = (ulong)(((num2 >> 11) * 0xff) + 0x10);
            var num7 = (byte)(((num3 / 0x20) + num3) / 0x20);
            num3 = (ulong)((((num2 & 0x7e0) >> 5) * 0xff) + 0x20);
            var num8 = (byte)(((num3 / 0x40) + num3) / 0x40);
            num3 = (ulong)(((num2 & 0x1f) * 0xff) + 0x10);
            var num9 = (byte)(((num3 / 0x20) + num3) / 0x20);
            ulong num10 = BitConverter.ToUInt32(image, 4);

            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    var color = new Color();
                    var num13 = (byte)((num10 >> (2 * ((4 * i) + j))) & 3);
                    if (num > num2)
                    {
                        switch (num13)
                        {
                            case 0:
                                color = Color.FromArgb(r, g, b);
                                break;

                            case 1:
                                color = Color.FromArgb(num7, num8, num9);
                                break;

                            case 2:
                                color = Color.FromArgb((byte)(((2 * r) + num7) / 3), (byte)(((2 * g) + num8) / 3), (byte)(((2 * b) + num9) / 3));
                                break;

                            case 3:
                                color = Color.FromArgb((byte)((r + (2 * num7)) / 3), (byte)((g + (2 * num8)) / 3), (byte)((b + (2 * num9)) / 3));
                                break;
                        }
                    }
                    else
                    {
                        switch (num13)
                        {
                            case 0:
                                color = Color.FromArgb(r, g, b);
                                break;

                            case 1:
                                color = Color.FromArgb(num7, num8, num9);
                                break;

                            case 2:
                                color = Color.FromArgb((byte)((r + num7) / 2), (byte)((g + num8) / 2), (byte)((b + num9) / 2));
                                break;

                            case 3:
                                color = Color.FromArgb(0, 0, 0);
                                break;
                        }
                    }
                    if ((x + j) < width)
                    {
                        img.SetPixel(x + j, y + i, color);
                    }
                }
            }

        }

        private Image DecompressArgb()
        {
            var image = Image;
            var width = Width;
            var height = Height;
            var img = new Bitmap(width, height);
            var index = 0;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var color = Color.FromArgb(image[index + 3], image[index + 2], image[index + 1], image[index]);
                    img.SetPixel(j, i, color);
                    index += 4;
                }
            }
            return img;
        }
    }
}