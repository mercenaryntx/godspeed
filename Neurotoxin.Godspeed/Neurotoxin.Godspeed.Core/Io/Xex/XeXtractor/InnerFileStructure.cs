using System;
using System.Collections;

namespace XeXtractor
{
    public class InnerFileStructure
    {
        private ArrayList files = new ArrayList();

        private static InnerFileStructure m_instance;

        private InnerFileStructure()
        {
        }

        public void AddFileEntry(FileEntry entr)
        {
            this.files.Add(entr);
            FileHandler.HandleFile(entr);
            if (this.FileAdded != null)
            {
                this.FileAdded(this, null);
            }
        }

        public void Clear()
        {
            this.files.Clear();
            if (this.FileAdded != null)
            {
                this.FileAdded(this, null);
            }
        }

        public FileEntry[] getFiles()
        {
            return (FileEntry[])this.files.ToArray(typeof(FileEntry));
        }

        public FileEntry[] getFiles(string type, string folder)
        {
            ArrayList arrayLists = new ArrayList();
            for (int i = 0; i < this.files.Count; i++)
            {
                FileEntry item = (FileEntry)this.files[i];
                if (item.type == type && item.folder == folder)
                {
                    arrayLists.Add(item);
                }
            }
            return (FileEntry[])arrayLists.ToArray(typeof(FileEntry));
        }

        public FileEntry[] getFiles(string type, string folder, long id, bool exactFolder)
        {
            ArrayList arrayLists = new ArrayList();
            for (int i = 0; i < this.files.Count; i++)
            {
                FileEntry item = (FileEntry)this.files[i];
                if (item.type == type && item.id == id)
                {
                    bool flag = false;
                    if (exactFolder)
                    {
                        if (item.folder == folder)
                        {
                            flag = true;
                        }
                    }
                    else if (item.folder.StartsWith(folder))
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        arrayLists.Add(item);
                    }
                }
            }
            return (FileEntry[])arrayLists.ToArray(typeof(FileEntry));
        }

        public static InnerFileStructure getInstance()
        {
            if (InnerFileStructure.m_instance == null)
            {
                InnerFileStructure.m_instance = new InnerFileStructure();
            }
            return InnerFileStructure.m_instance;
        }

        public event EventHandler FileAdded;
    }
}