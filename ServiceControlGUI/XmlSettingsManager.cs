using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml;
using WSR.Core.SettingsMgmt;

namespace WSR.Settings.GUI
{
    /// <summary>
    ///     This class allows to manage xml settings from config files.
    ///     its usefull when you want to modify application settings in runtime
    ///     using another applications i.e. to modify service configuration files with an external app GUI.
    /// </summary>
    public class XmlSettingsManager : ICustomTypeDescriptor
    {
        private readonly string _nameSpace;
        private readonly List<Setting> _settings = new List<Setting>();
        private readonly IniFile commentsIniFile;
        private string _basePath;
        private string _configFilePath;
        private XmlDocument _xmlDocument;

        public XmlSettingsManager(string settingsNameSpace, string configFilePath, string iniFilePath = null)
        {
            _nameSpace = settingsNameSpace;
            _configFilePath = configFilePath;
            _basePath = Path.GetDirectoryName(configFilePath);

            if (!string.IsNullOrEmpty(iniFilePath))
                commentsIniFile = new IniFile(iniFilePath);

            LoadSettings();
        }

        public List<Setting> Settings
        {
            get { return _settings; }
        }


        /// <summary>
        ///     add a parameter to already loaded configuration
        /// </summary>
        /// <param name="setting"></param>
        public void AddSetting(Setting setting)
        {
            Settings.Add(setting);
        }

        private void LoadSettings()
        {
            XmlElement root = GetXmlRoot();


            XmlNodeList settingsList = root.SelectNodes(@"setting");

            if (settingsList != null)
            {
                foreach (XmlNode xmlSetting in settingsList)
                {
                    var element = (XmlElement) xmlSetting;
                    string comment = "";
                    if (commentsIniFile != null)
                    {
                        comment = commentsIniFile.GetString(_nameSpace, element.Attributes["name"].Value, "N/A");
                    }


                    var newSetting = new Setting
                    {
                        Comment = comment,
                        Name = element.Attributes["name"].Value,
                        SerializeAs = element.Attributes["serializeAs"].Value,
                        Value = element.FirstChild.InnerText // <value>text</value>
                    };


                    AddSetting(newSetting);
                }
            }
        }


        public void SaveSettings()
        {
            XmlElement root = GetXmlRoot();

            // doc.InsertBefore(xmlDeclaration, doc.FirstChild);

            if (root == null)
                throw new NullReferenceException("Namespace not found in configuration file");

            foreach (Setting setting in Settings)
            {
                var xmlSetting = root.SelectSingleNode(@"//setting[@name='" + setting.Name + "']") as XmlElement;

                if (xmlSetting != null)
                {
                    xmlSetting.FirstChild.InnerText = setting.Value;
                }
                else
                {
                    xmlSetting = _xmlDocument.CreateElement("setting");
                    XmlAttribute name = _xmlDocument.CreateAttribute("name");
                    name.Value = setting.Name;
                    XmlAttribute serializeAs = _xmlDocument.CreateAttribute("serializeAs");
                    serializeAs.Value = setting.SerializeAs;
                    XmlElement value = _xmlDocument.CreateElement("value");
                    value.InnerText = setting.Value;
                    xmlSetting.Attributes.Append(name);
                    xmlSetting.Attributes.Append(serializeAs);
                    xmlSetting.AppendChild(value);
                    root.AppendChild(xmlSetting);
                }
            }


            _xmlDocument.Save(_configFilePath);
        }


        /// <summary>
        ///     Retourne le noeud qui correspond au namespace
        /// </summary>
        /// <returns></returns>
        private XmlElement GetXmlRoot()
        {
            _xmlDocument = null;
            _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(File.ReadAllText(_configFilePath));

            var root = _xmlDocument.SelectSingleNode("//" + _nameSpace) as XmlElement;

            if (root == null)
                throw new NullReferenceException("Namespace not found in configuration file");

            // on gere le cas de la redirection de fichier de configuration
            foreach (XmlAttribute attribute in root.Attributes)
            {
                if (attribute.Name == "configSource")
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        string path = Path.Combine(_basePath, attribute.Value);
                        _xmlDocument.LoadXml(File.ReadAllText(path));
                        root = _xmlDocument.SelectSingleNode("//" + _nameSpace) as XmlElement;
                        _configFilePath = path;
                        _basePath = Path.GetDirectoryName(_configFilePath);
                    }
                }
            }
            return root;
        }

        #region Implementation of ICustomTypeDescriptor

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            // Create a new collection object PropertyDescriptorCollection
            var pds = new PropertyDescriptorCollection(null);

            // Iterate the list of employees
            foreach (Setting setting in Settings)
            {
                var propDesc = new SettingPropertyDescriptor(setting, _nameSpace);

                pds.Add(propDesc);
            }
            return pds;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }


    /// <summary>
    ///     This is a setting as defined in a config file
    /// </summary>
    /// <example>
    ///     <WSR.Settings.NotificationSettings>
    ///         <setting name="SMTPHost" serializeAs="String">
    ///             <value>localhost</value>
    ///         </setting>
    ///     </WSR.Settings.NotificationSettings>
    /// </example
    public class Setting : ICustomTypeDescriptor
    {
        public string Name { get; set; }

        public string SerializeAs { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public string Category { get; set; }

        #region Implementation of ICustomTypeDescriptor

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            // Create a new collection object PropertyDescriptorCollection
            var pds = new PropertyDescriptorCollection(null);

            // Iterate the list of employees


            var propDesc = new SettingPropertyDescriptor(this, Category);

            pds.Add(propDesc);


            return pds;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }

    /// <summary>
    /// </summary>
    public class SettingPropertyDescriptor : PropertyDescriptor
    {
        private readonly string _category;
        private readonly Setting _setting;

        public SettingPropertyDescriptor(Setting setting, string category)
            : base(setting.Name, null)
        {
            _setting = setting;
            _category = category;
        }

        #region Overrides of PropertyDescriptor

        public override AttributeCollection Attributes
        {
            get
            {
                var attr = new ReadOnlyAttribute(false);
                var isBrowsable = new BrowsableAttribute(true);

                Attribute[] attributeCollection = {attr, isBrowsable};

                return new AttributeCollection(attributeCollection);
            }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override string DisplayName
        {
            get { return _setting.Name; }
        }

        public override string Description
        {
            get { return _setting.Comment; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override string Name
        {
            get { return _setting.Name; }
        }

        public override Type PropertyType
        {
            get
            {
                bool res;
                Boolean.TryParse(_setting.Value, out res);

                if (res)
                    return typeof (bool);
                return typeof (string);
            }
        }

        public override string Category
        {
            get { return _category; }
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return _setting.Value;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override void SetValue(object component, object value)
        {
            switch (value.GetType().ToString())
            {
                case "System.Boolean":
                    _setting.Value = ((bool) value).ToString(CultureInfo.InvariantCulture);
                    break;
                default:
                    _setting.Value = (string) value;
                    break;
            }
        }

        #endregion
    }
}
