using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class Tree<T> : TreeItem<T> where T : class, INamed
    {
        public Tree() : base(string.Empty, GetDefaultContent(), null, null)
        {
        }

        public Tree(string name, T content) : base(name, content, null, null)
        {
            Root = this;
        }
    }

    public class TreeItem<T> : IList<T> where T : class, INamed
    {
        public TreeItem<T> Root { get; protected set; }
        public TreeItem<T> Parent { get; protected set; }
        public string Name { get; protected set; }
        public T Content { get; set; }
        public Dictionary<string, TreeItem<T>> Children { get; private set; }

        protected TreeItem(string name, T content, TreeItem<T> parent, TreeItem<T> root)
        {
            Name = name;
            Content = content;
            Parent = parent;
            Root = root;
            Children = new Dictionary<string, TreeItem<T>>();
        }

        public void Add(T content)
        {
            AddItem(content);
        }

        private TreeItem<T> AddItem(T content)
        {
            var treeItem = new TreeItem<T>(content.Name, content, this, Root);
            Children.Add(content.Name, treeItem);
            return treeItem;
        }

        public IEnumerable<TreeItem<T>> AddRange(IEnumerable<T> contents)
        {
            if (contents == null) throw new ArgumentException();
            var result = new List<TreeItem<T>>();
            foreach (var content in contents)
            {
                result.Add(AddItem(content));
            }
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var child in Children.Values)
            {
                array[arrayIndex++] = child.Content;
            }
        }

        public bool Remove(string key)
        {
            return Children.Remove(key);
        }

        bool ICollection<T>.Remove(T item)
        {
            var treeItem = Children.FirstOrDefault(c => c.Value.Content.Equals(item));
            if (treeItem.Key == null) return false;
            Children.Remove(treeItem.Key);
            return true;
        }

        public int Count
        {
            get { return Children.Count; }
        }

        public bool IsReadOnly { get; set; }

        public void Clear()
        {
            Children.Clear();
        }

        public bool Contains(string key)
        {
            return Children.ContainsKey(key);
        }

        public bool Contains(T item)
        {
            return Children.Any(c => c.Value.Content.Equals(item));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Children.Values.Select(c => c.Content).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Children.Values.Select((c, i) => c.Content.Equals(item) ? i : -1).Max();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            Children.Remove(IndexToKey(index));
        }

        private string IndexToKey(int index)
        {
            if (index < 0 || index > Children.Keys.Count) throw new ArgumentOutOfRangeException();
            var enumerator = Children.Keys.GetEnumerator();
            var i = -1;
            string key;
            do
            {
                enumerator.MoveNext();
                key = enumerator.Current;
                i++;
            }
            while (i < index);
            return key;
        }

        public T this[int index]
        {
            get { return Children[IndexToKey(index)].Content; }
            set { Children[IndexToKey(index)].Content = value; }
        }

        public T this[string key]
        {
            get { return Children[key].Content; }
            set { Children[key].Content = value; }
        }

        public T Find(string path)
        {
            var node = GetNode(path);
            return node != null ? node.Content : null;
        }

        public IList<T> GetChildren(string path)
        {
            var node = GetNode(path);
            return node != null ? node.Children.Select(c => c.Value.Content).ToList() : null;
        }

        public void Insert(string path, T content = null, Func<string, T> factory = null)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("//") && this != Root)
            {
                Root.Insert(path, content, factory);
                return;
            }
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var node = this;
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (node.Contains(part))
                {
                    node = node.Children[part];
                }
                else
                {
                    if (i == parts.Length - 1) //  || content != null
                    {
                        node = node.AddItem(content);
                    }
                    else
                    {
                        var placeHolder = factory != null ? factory.Invoke(part) : GetDefaultContent();
                        placeHolder.Name = part;
                        node = node.AddItem(placeHolder);
                    }
                }
            }
        }

        private TreeItem<T> GetNode(string path)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("//") && this != Root) return Root.GetNode(path);
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var node = this;
            foreach (var part in parts)
            {
                if (!node.Contains(part)) return null;
                node = node.Children[part];
            }
            return node;
        }

        protected static T GetDefaultContent()
        {
            var instance = Activator.CreateInstance<T>();
            instance.Name = string.Empty;
            return instance;
        }

    }
}