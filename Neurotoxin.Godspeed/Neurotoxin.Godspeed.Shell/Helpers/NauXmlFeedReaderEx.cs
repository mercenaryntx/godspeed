using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.FeedReaders;
using NAppUpdate.Framework.Tasks;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    //HACK: NAppUpdater extension
    public class NauXmlFeedReaderEx : IUpdateFeedReader
    {
        private Dictionary<string, Type> _updateConditions;
        private Dictionary<string, Type> _updateTasks;

        public string LatestVersionOnServer { get; private set; }

        public IList<IUpdateTask> Read(string feed)
        {
            // Lazy-load the Condition and Task objects contained in this assembly, unless some have already
            // been loaded (by a previous lazy-loading in a call to Read, or by an explicit loading)
            if (_updateTasks == null)
            {
                _updateConditions = new Dictionary<string, Type>();
                _updateTasks = new Dictionary<string, Type>();
                FindTasksAndConditionsInAssembly(typeof(IUpdateFeedReader).Assembly, _updateTasks, _updateConditions);
            }

            var ret = new List<IUpdateTask>();

            var doc = new XmlDocument();
            doc.LoadXml(feed);

            // Support for different feed versions
            var root = doc.SelectSingleNode(@"/Feed[version=""1.0""] | /Feed") ?? doc;

            var baseUrl = root.Attributes["BaseUrl"];
            if (baseUrl != null && !string.IsNullOrEmpty(baseUrl.Value))
            {
                var type = typeof(UpdateManager);
                var pi = type.GetProperty("BaseUrl", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(UpdateManager.Instance, baseUrl.Value, null);
            }

            var version = root.Attributes["Version"];
            if (version != null) LatestVersionOnServer = version.Value;

            // Temporary collection of attributes, used to aggregate them all with their values
            // to reduce Reflection calls
            var attributes = new Dictionary<string, string>();

            var nl = root.SelectNodes("./Tasks/*");
            if (nl == null) return new List<IUpdateTask>(); // TODO: wrong format, probably should throw exception
            foreach (XmlNode node in nl)
            {
                // Find the requested task type and create a new instance of it
                if (!_updateTasks.ContainsKey(node.Name))
                    continue;

                var task = (IUpdateTask)Activator.CreateInstance(_updateTasks[node.Name]);

                // Store all other task attributes, to be used by the task object later
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if ("type".Equals(att.Name))
                            continue;

                        attributes.Add(att.Name, att.Value);
                    }
                    if (attributes.Count > 0)
                    {
                        SetNauAttributes(task, attributes);
                        attributes.Clear();
                    }
                    // TODO: Check to see if all required task fields have been set
                }

                if (node.HasChildNodes)
                {
                    if (node["Description"] != null)
                        task.Description = node["Description"].InnerText;

                    // Read update conditions
                    if (node["Conditions"] != null)
                    {
                        var conditionObject = ReadCondition(node["Conditions"]);
                        if (conditionObject != null)
                        {
                            var boolCond = conditionObject as BooleanCondition;
                            if (boolCond != null)
                                task.UpdateConditions = boolCond;
                            else
                            {
                                if (task.UpdateConditions == null) task.UpdateConditions = new BooleanCondition();
                                task.UpdateConditions.AddCondition(conditionObject);
                            }
                        }
                    }
                }

                ret.Add(task);
            }
            return ret;
        }

        private IUpdateCondition ReadCondition(XmlNode cnd)
        {
            IUpdateCondition conditionObject = null;
            if (cnd.ChildNodes.Count > 0 || "GroupCondition".Equals(cnd.Name))
            {
                var bc = new BooleanCondition();
                foreach (XmlNode child in cnd.ChildNodes)
                {
                    var childCondition = ReadCondition(child);
                    if (childCondition != null)
                        bc.AddCondition(childCondition, BooleanCondition.ConditionTypeFromString(child.Attributes != null && child.Attributes["type"] != null ? child.Attributes["type"].Value : null));
                }
                if (bc.ChildConditionsCount > 0)
                    conditionObject = bc.Degrade();
            }
            else if (_updateConditions.ContainsKey(cnd.Name))
            {
                conditionObject = (IUpdateCondition)Activator.CreateInstance(_updateConditions[cnd.Name]);

                if (cnd.Attributes != null)
                {
                    var dict = cnd.Attributes.Cast<XmlAttribute>().Where(att => !"type".Equals(att.Name)).ToDictionary(att => att.Name, att => att.Value);

                    // Store all other attributes, to be used by the condition object later
                    if (dict.Count > 0) SetNauAttributes(conditionObject, dict);
                }
            }
            return conditionObject;
        }

        private static void FindTasksAndConditionsInAssembly(Assembly assembly, Dictionary<string, Type> updateTasks, Dictionary<string, Type> updateConditions)
        {
            foreach (var t in assembly.GetTypes())
            {
                if (typeof(IUpdateTask).IsAssignableFrom(t))
                {
                    updateTasks.Add(t.Name, t);
                    var tasksAliases = (UpdateTaskAliasAttribute[])t.GetCustomAttributes(typeof(UpdateTaskAliasAttribute), false);
                    foreach (var alias in tasksAliases)
                    {
                        updateTasks.Add(alias.Alias, t);
                    }
                }
                else if (typeof(IUpdateCondition).IsAssignableFrom(t))
                {
                    updateConditions.Add(t.Name, t);
                    var tasksAliases = (UpdateConditionAliasAttribute[])t.GetCustomAttributes(typeof(UpdateConditionAliasAttribute), false);
                    foreach (var alias in tasksAliases)
                    {
                        updateConditions.Add(alias.Alias, t);
                    }
                }
            }
        }

        private static void SetNauAttributes(INauFieldsHolder fieldsHolder, Dictionary<string, string> attributes)
        {
            // Load public non-static properties
            var propertyInfos = fieldsHolder.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var attValue = string.Empty;
            foreach (var pi in propertyInfos)
            {
                var atts = pi.GetCustomAttributes(typeof(NauFieldAttribute), false);
                if (atts.Length != 1) continue; // NauFieldAttribute doesn't allow multiples

                var nfa = (NauFieldAttribute)atts[0];

                // Get the attribute value, process it, and set the object's property with that value
                if (!attributes.TryGetValue(nfa.Alias, out attValue)) continue;
                if (pi.PropertyType == typeof(String))
                {
                    pi.SetValue(fieldsHolder, attValue, null);
                }
                else if (pi.PropertyType == typeof(DateTime))
                {
                    var dt = DateTime.MaxValue;
                    var filetime = long.MaxValue;
                    if (DateTime.TryParse(attValue, out dt))
                        pi.SetValue(fieldsHolder, dt, null);
                    else if (long.TryParse(attValue, out filetime))
                    {
                        try
                        {
                            // use local time, not UTC
                            dt = DateTime.FromFileTime(filetime);
                            pi.SetValue(fieldsHolder, dt, null);
                        }
                        catch { }
                    }
                }
                // TODO: type: Uri
                else if (pi.PropertyType.IsEnum)
                {
                    var eObj = Enum.Parse(pi.PropertyType, attValue);
                    if (eObj != null)
                        pi.SetValue(fieldsHolder, eObj, null);
                }
                else
                {
                    var mi = pi.PropertyType.GetMethod("Parse", new Type[] { typeof(String) });
                    if (mi == null) continue;
                    var o = mi.Invoke(null, new object[] { attValue });

                    if (o != null)
                        pi.SetValue(fieldsHolder, o, null);
                }
            }
        }

    }
}