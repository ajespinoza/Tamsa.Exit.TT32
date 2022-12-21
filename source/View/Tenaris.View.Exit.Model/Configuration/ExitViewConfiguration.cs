// -----------------------------------------------------------------------
// <copyright file="TemplateViewConfiguration.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    using System.Xml.Serialization;

    /// <summary>
    /// Configuracion de la vista
    /// 
    /// En esta clase ponemos las diversas variables que necesitamos para
    /// configurar el comportamiento de la vista
    /// </summary>
    public class ExitViewConfiguration : ConfigurationSection, IXmlSerializable
    {

        // variable para configurar el idioma
        [ConfigurationProperty("Language", IsRequired = true)]
        public string Language
        {
            get { return this["Language"].ToString(); }
            set { this["Language"] = value; }
        }

        // variable para el desktop manager
        [ConfigurationProperty("DskMgr", DefaultValue = false)]
        public bool DskMgr
        {
            get { return System.Convert.ToBoolean(this["DskMgr"]); }
            set { this["DskMgr"] = value; }
        }

        [ConfigurationProperty("GridHeight", IsRequired = true)]
        public int GridHeight
        {
            get { return Convert.ToInt32(this["GridHeight"]); }
            set { this["GridHeight"] = value; }
        }

        /// <summary>
        /// Indicates the path to Dynamic Resources.
        /// </summary>
        [ConfigurationProperty("DynamicResourcesPath", DefaultValue = "Resources/Theme.xaml")]
        public string DynamicResourcesPath
        {
            get { return this["DynamicResourcesPath"].ToString(); }
            set { this["DynamicResourcesPath"] = value; }
        }

        [ConfigurationProperty("DBConnection", IsRequired = true)]
        public string DBConnection
        {
            get { return this["DBConnection"].ToString(); }
            set { this["DBConnection"] = value; }
        }

        [ConfigurationProperty("AreaCode", IsRequired = true)]
        public string AreaCode
        {
            get { return this["AreaCode"].ToString(); }
            set { this["AreaCode"] = value; }
        }

        [ConfigurationProperty("ITPrintMachines", IsRequired = true)]
        public string ITPrintMachines
        {
            get { return this["ITPrintMachines"].ToString(); }
            set { this["ITPrintMachines"] = value; }
        }

        [ConfigurationProperty("IsActiveSinSeas", IsRequired = true)]
        public string IsActiveSinSeas
        {
            get { return this["IsActiveSinSeas"].ToString(); }
            set { this["IsActiveSinSeas"] = value; }
        }

        [ConfigurationProperty("SpecificationServer", IsRequired = true)]
        public string SpecificationServer
        {
            get { return this["SpecificationServer"].ToString(); }
            set { this["SpecificationServer"] = value; }
        }

        [ConfigurationProperty("SpecificationPort", IsRequired = true)]
        public int SpecificationPort
        {
            get { return Convert.ToInt32(this["SpecificationPort"]); }
            set { this["SpecificationPort"] = value; }
        }
        


        [ConfigurationProperty("ExitManagerSection", IsRequired = true)]
        public string ExitManagerSection
        {
            get { return this["ExitManagerSection"].ToString(); }
            set { this["ExitManagerSection"] = value; }
        }

        [ConfigurationProperty("DefaultGroupEECode", IsRequired = true)]
        public string DefaultGroupEECode
        {
            get { return this["DefaultGroupEECode"].ToString(); }
            set { this["DefaultGroupEECode"] = value; }
        }
        [ConfigurationProperty("DefaultEECode", IsRequired = true)]
        public string DefaultEECode
        {
            get { return this["DefaultEECode"].ToString(); }
            set { this["DefaultEECode"] = value; }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.DeserializeElement(reader, false);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            return;
        }
    }
}
