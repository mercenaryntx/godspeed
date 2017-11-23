using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;

namespace Neurotoxin.Godspeed.Core.Win32
{
    public class RegisteredFileTypes
    {
        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static unsafe extern int DestroyIcon(IntPtr hIcon);

        private readonly Dictionary<string, string> _iconsInfo;

        private static RegisteredFileTypes _instance;
        public static RegisteredFileTypes Instance
        {
            get { return _instance ?? (_instance = new RegisteredFileTypes()); }
        }

        public Icon GetIcon(string fileType)
        {
            return !_iconsInfo.ContainsKey(fileType) ? null : ExtractIconFromFile(_iconsInfo[fileType], false);
        }

        private RegisteredFileTypes()
        {
            // Create a registry key object to represent the HKEY_CLASSES_ROOT registry section
            var rkRoot = Registry.ClassesRoot;

            //Gets all sub keys' names.
            var keyNames = rkRoot.GetSubKeyNames();
            _iconsInfo = new Dictionary<string, string>();

            //Find the file icon.
            foreach (var keyName in keyNames)
            {
                if (String.IsNullOrEmpty(keyName)) continue;
                var indexOfPoint = keyName.IndexOf(".", System.StringComparison.Ordinal);

                //If this key is not a file exttension(eg, .zip), skip it.
                if (indexOfPoint != 0) continue;

                var rkFileType = rkRoot.OpenSubKey(keyName);
                if (rkFileType == null) continue;

                //Gets the default value of this key that contains the information of file type.
                var defaultValue = rkFileType.GetValue("");
                if (defaultValue == null) continue;

                //Go to the key that specifies the default icon associates with this file type.
                var defaultIcon = defaultValue + "\\DefaultIcon";
                var rkFileIcon = rkRoot.OpenSubKey(defaultIcon);
                if (rkFileIcon != null)
                {
                    //Get the file contains the icon and the index of the icon in that file.
                    var value = rkFileIcon.GetValue("");
                    if (value != null)
                    {
                        //Clear all unecessary " sign in the string to avoid error.
                        var fileParam = value.ToString().Replace("\"", "");
                        _iconsInfo.Add(keyName, fileParam);
                    }
                    rkFileIcon.Close();
                }
                rkFileType.Close();
            }
            rkRoot.Close();
        }

        private static Icon ExtractIconFromFile(string param, bool isLarge)
        {
            unsafe
            {
                var hDummy = new[] {IntPtr.Zero};
                var hIconEx = new[] {IntPtr.Zero};

                try
                {
                    string file;
                    int iconIndex;
                    var commaIndex = param.IndexOf(",", StringComparison.Ordinal);
                    //if fileAndParam is some thing likes that: "C:\\Program Files\\NetMeeting\\conf.exe,1".
                    if (commaIndex > 0)
                    {
                        file = param.Substring(0, commaIndex);
                        iconIndex = Int32.Parse(param.Substring(commaIndex + 1));
                    }
                    else 
                    {
                        file = param;
                        iconIndex = 0;
                    }

                    var readIconCount = isLarge
                                            ? ExtractIconEx(file, iconIndex, hIconEx, hDummy, 1)
                                            : ExtractIconEx(file, iconIndex, hDummy, hIconEx, 1);

                    if (readIconCount <= 0 || hIconEx[0] == IntPtr.Zero)
                        return null;
                    else
                    {
                        // Get first icon.
                        var extractedIcon = (Icon) Icon.FromHandle(hIconEx[0]).Clone();
                        return extractedIcon;
                    }
                }
                catch (Exception exc)
                {
                    // Extract icon error.
                    throw new ApplicationException("Could not extract icon", exc);
                }
                finally
                {
                    // Release resources.
                    foreach (var ptr in hIconEx.Where(ptr => ptr != IntPtr.Zero))
                        DestroyIcon(ptr);

                    foreach (var ptr in hDummy.Where(ptr => ptr != IntPtr.Zero))
                        DestroyIcon(ptr);
                }
            }
        }
    }
}