using System;
using System.IO;
using System.Threading;

namespace XeXtractor
{
    public class FileHandler
    {
        public FileHandler()
        {
        }

        public static string GetFileType(byte[] data)
        {
            EndianIo endianIo = new EndianIo(data, EndianType.BigEndian);
            endianIo.Open();
            string str = endianIo.In.ReadAsciiString(4);
            endianIo.Close();
            return str;
        }

        public static void HandleFile(string fileName)
        {
            (new Thread(new ParameterizedThreadStart(FileHandler.HandleFileThreaded))).Start(fileName);
        }

        public static void HandleFile(FileEntry data)
        {
            try
            {
                string fileType = "";
                if ((int)data.Data.Length > 4)
                {
                    fileType = FileHandler.GetFileType(data.Data);
                }
                string str = fileType;
                string str1 = str;
                if (str != null)
                {
                    if (str1 == "XEX2")
                    {
                        FileHandler.HandleXEX2(data.Data, Path.GetFileNameWithoutExtension(data.fileName));
                    }
                    //else if (str1 == "XSTR")
                    //{
                    //    FileHandler.HandleXSTR(data.Data, Path.GetFileNameWithoutExtension(data.fileName));
                    //}
                    //else if (str1 == "XSRC")
                    //{
                    //    FileHandler.HandleXSRC(data.Data);
                    //}
                    //else if (str1 == "XDBF")
                    //{
                    //    FileHandler.HandleXDBF(data.Data);
                    //}
                    //else if (str1 == "XUIZ")
                    //{
                    //    FileHandler.HandleXUIZ(data.Data, data.fileName);
                    //}
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                //Log.getInstance().AddEntry(string.Concat("Error :", exception.Message));
            }
        }

        public static void HandleFileThreaded(object fileName)
        {
            byte[] numArray = File.ReadAllBytes(fileName.ToString());
            FileEntry fileEntry = new FileEntry()
            {
                Data = numArray,
                fileName = Path.GetFileNameWithoutExtension(fileName.ToString())
            };
            FileHandler.HandleFile(fileEntry);
            if (FileHandler.ParseCompleted != null)
            {
                FileHandler.ParseCompleted(null, null);
            }
        }

        private static void HandleXACH(byte[] file)
        {
        }

        //private static void HandleXDBF(byte[] file)
        //{
        //    XDBF xDBF = new XDBF(file);
        //    xDBF.Open();
        //    xDBF.Read();
        //    xDBF.ExtractEntryData();
        //    xDBF.Close();
        //    FileEntry[] files = InnerFileStructure.getInstance().getFiles("XDBF", "Bins");
        //    for (int i = 0; i < (int)files.Length; i++)
        //    {
        //        if (files[i].fileName.ToLower().EndsWith("xitb"))
        //        {
        //            XITB xITB = new XITB(files[i].Data);
        //            xITB.Open();
        //            xITB.Read();
        //            xITB.Close();
        //        }
        //    }
        //    for (int j = 0; j < (int)files.Length; j++)
        //    {
        //        if (files[j].fileName.ToLower().EndsWith("xach"))
        //        {
        //            FileEntry[] fileEntryArray = InnerFileStructure.getInstance().getFiles("XDBF", "Strings");
        //            for (int k = 0; k < (int)fileEntryArray.Length; k++)
        //            {
        //                XSTR xSTR = new XSTR(fileEntryArray[k].Data);
        //                xSTR.Open();
        //                xSTR.Read(true, "");
        //                XACH xACH = new XACH(files[j].Data);
        //                xACH.Open();
        //                xACH.Read(xSTR, fileEntryArray[k].fileName);
        //                xACH.Close();
        //                xSTR.Close();
        //            }
        //        }
        //    }
        //}

        private static void HandleXEX2(byte[] file, string fileName)
        {
            XEX2 xEX2 = new XEX2(file);
            xEX2.Open();
            xEX2.Read();
            xEX2.DecryptBaseFile();
            FileEntry fileEntry = new FileEntry()
            {
                Data = xEX2.getBaseFile(),
                type = "Base File",
                fileName = string.Concat(fileName, ".exe")
            };
            InnerFileStructure.getInstance().AddFileEntry(fileEntry);
            xEX2.ExtractAllRessource();
            xEX2.Close();
        }

        //private static void HandleXSRC(byte[] file)
        //{
        //    XSRC xSRC = new XSRC(file);
        //    xSRC.Open();
        //    xSRC.Read();
        //    xSRC.Close();
        //}

        //private static void HandleXSTR(byte[] file, string sourceName)
        //{
        //    XSTR xSTR = new XSTR(file);
        //    xSTR.Open();
        //    xSTR.Read(false, sourceName);
        //    xSTR.Close();
        //}

        //private static void HandleXUIZ(byte[] file, string filename)
        //{
        //    XUIZ xUIZ = new XUIZ(file, filename);
        //    xUIZ.Open();
        //    xUIZ.Read();
        //    xUIZ.Close();
        //}

        public static event EventHandler ParseCompleted;
    }
}