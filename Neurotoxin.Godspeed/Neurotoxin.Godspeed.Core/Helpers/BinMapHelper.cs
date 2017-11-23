using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Helpers
{
    public static class BinMapHelper
    {
        public static void ModelCompare(BinaryModelBase wr, BinaryModelBase ok, string indent = "")
        {
            var type = wr.GetType();
            var modelType = typeof(BinaryModelBase);
            var wrKeys = wr.BinMap.Keys;
            var okKeys = ok.BinMap.Keys;

            for (var i = 0; i < wrKeys.Length; i++)
            {
                var wrEntry = wr.BinMap.Get(wrKeys[i]).Item2;
                var key = wrKeys[i];
                if (okKeys.Contains(key))
                {
                    var okEntry = ok.BinMap.Get(key).Item2;
                    if (wrEntry.Length != okEntry.Length)
                    {
                        Debug.WriteLine(
                            "{8}[0x{0,8:X8}][{7,4}] Different content length. A[{3},{5}]: {1}; B[{4},{6}]: {2}",
                            wrKeys[i], wrEntry.Length, okEntry.Length, wrEntry.PropertyName,
                            okEntry.PropertyName, wrEntry.ClassName, okEntry.ClassName, wrEntry.BlockNum, indent);
                    }
                    else
                    {
                        var wrData = wr.Binary.ReadBytes(wrKeys[i], wrEntry.Length.Value);
                        var okData = ok.Binary.ReadBytes(wrKeys[i], wrEntry.Length.Value);
                        IStructuralEquatable eqa1 = wrData;
                        if (!eqa1.Equals(okData, StructuralComparisons.StructuralEqualityComparer))
                        {
                            int? blockNum;
                            string wrValue;
                            string okValue;
                            if (wr is HashEntry)
                            {
                                var wrhe = (HashEntry)wr;
                                var okhe = (HashEntry)ok;
                                switch (wrEntry.PropertyName)
                                {
                                    case "BlockHash":
                                        wrValue = wrhe.BlockHash.ToHex();
                                        okValue = okhe.BlockHash.ToHex();
                                        break;
                                    case "Status":
                                        wrValue = wrhe.Status.ToString();
                                        okValue = okhe.Status.ToString();
                                        break;
                                    case "NextBlock":
                                        wrValue = wrhe.NextBlock.ToString(CultureInfo.InvariantCulture);
                                        okValue = okhe.NextBlock.ToString(CultureInfo.InvariantCulture);
                                        break;
                                    default:
                                        throw new NotSupportedException(wrEntry.PropertyName);
                                }
                                blockNum = wrhe.Block;
                            }
                            else
                            {
                                blockNum = wrEntry.BlockNum;
                                wrValue = wrEntry.ClassName;
                                okValue = okEntry.ClassName;
                            }

                            Debug.WriteLine("{6}[0x{0,8:X8}][{2,4}] Different data: {1} ({3}) vs {4} ({5})", wrKeys[i],
                                            wrEntry.PropertyName, blockNum, wrValue, okEntry.PropertyName, okValue,
                                            indent);
                            var pi = type.GetProperty(wrEntry.PropertyName);
                            switch (wrEntry.PropertyName)
                            {
                                case "TopTable":
                                    {
                                        var wrTable = ((StfsPackage)wr).TopTable;
                                        var okTable = ((StfsPackage)ok).TopTable;
                                        if (wrTable.EntryCount != okTable.EntryCount)
                                            Debug.WriteLine("  -- Different entry count --");
                                        else
                                        {
                                            for (var j = 0; j < wrTable.EntryCount; j++)
                                            {
                                                ModelCompare(wrTable.Entries[j], okTable.Entries[j], "  ");
                                            }
                                            Debug.WriteLine("{0} {1}", wrTable.AllocatedBlockCount, okTable.AllocatedBlockCount);
                                        }
                                    }
                                    break;
                                case "Table":
                                    {
                                        var wrTable = ((StfsPackage)wr).TopTable.Tables[int.Parse(wrEntry.ClassName)];
                                        var okTable = ((StfsPackage)ok).TopTable.Tables[int.Parse(wrEntry.ClassName)];
                                        if (wrTable.EntryCount != okTable.EntryCount)
                                            Debug.WriteLine("  -- Different entry count --");
                                        else
                                        {
                                            for (var j = 0; j < wrTable.EntryCount; j++)
                                                ModelCompare(wrTable.Entries[j], okTable.Entries[j], "  ");
                                        }
                                    }
                                    break;
                                case "FileTable":
                                    {
                                        for (var j = 0; j < 64; j++)
                                        {
                                            var addr = key + j * 0x40;
                                            var wrfe = ModelFactory.GetModel<FileEntry>(wr.Binary, addr);
                                            var okfe = ModelFactory.GetModel<FileEntry>(ok.Binary, addr);
                                            ModelCompare(wrfe, okfe, "  ");
                                        }
                                    }
                                    break;
                                default:
                                    if (pi != null && modelType.IsAssignableFrom(pi.PropertyType))
                                    {
                                        var wrProperty = pi.GetValue(wr, null) as BinaryModelBase;
                                        var okProperty = pi.GetValue(ok, null) as BinaryModelBase;
                                        ModelCompare(wrProperty, okProperty, indent + "  ");
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("{4}[0x{0,8:X8}][{2,4}] Missing data. {1} ({3})", wrKeys[i], wrEntry.PropertyName,
                                    wrEntry.BlockNum, wrEntry.ClassName, indent);
                }
            }
        }
    }
}
