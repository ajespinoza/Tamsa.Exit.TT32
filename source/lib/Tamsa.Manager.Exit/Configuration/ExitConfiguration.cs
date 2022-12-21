// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExitConfiguration.cs" company="Tenaris">
//   Tamsa.
// </copyright>
// <summary>
//   Defines the ExitConfiguration class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    using Tenaris.Library.Log;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Xml.Schema;

    public class FieldsElement : ConfigurationElement
    {


        [ConfigurationProperty("code", IsKey = true)]
        public string Code
        {
            get { return this["code"].ToString(); }
            set { this["code"] = value; }
        }

        /// <summary>
        /// Column Name
        /// </summary>
        [ConfigurationProperty("ColumnName", IsRequired = true)]
        public string ColumnName
        {
            get { return this["ColumnName"].ToString(); }
            set { this["ColumnName"] = value; }
        }

    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ConfigurationCollection(typeof(FieldsElement), AddItemName = "Field", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class FieldsCollection : ConfigurationElementCollection
    {

        public FieldsElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as FieldsElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FieldsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FieldsElement)element).Code;
        }
    }


    /// <summary>
    /// Exit configuration class
    /// </summary>
    [Serializable]
    public class ExitConfiguration : ConfigurationSection, IXmlSerializable
    {


        [ConfigurationProperty("BundleFields", IsDefaultCollection = false)]
        public FieldsCollection BundleFields
        {
            get
            {
                return (FieldsCollection)base["BundleFields"];
            }
        }

        [ConfigurationProperty("ProductFields", IsDefaultCollection = false)]
        public FieldsCollection ProductFields
        {
            get
            {
                return (FieldsCollection)base["ProductFields"];
            }
        }

        /// <summary>
        /// Gets or sets database connection string.
        /// </summary>
        [ConfigurationProperty("DatabaseConnectionStringName", IsRequired = true, DefaultValue = "Level2")]
        public string DatabaseConnectionStringName
        {
            get { return (string)base["DatabaseConnectionStringName"]; }
            set { base["DatabaseConnectionStringName"] = value; }
        }

        /// <summary>
        /// Gets or sets the period to remove.
        /// </summary>
        [ConfigurationProperty("PeriodToRemove", DefaultValue = 30)]
        public int PeriodToRemove
        {
            get { return Convert.ToInt32(this["PeriodToRemove"].ToString()); }
            set { base["PeriodToRemove"] = value; }
        }

        /// <summary>
        /// Gets or Sets Pieces by Bundle
        /// </summary>
        [ConfigurationProperty("PiecesByBundle", IsRequired = true, DefaultValue = 10)]
        public int PiecesByBundle
        {
            get { return Convert.ToInt32(this["PiecesByBundle"].ToString()); }
            set { base["PiecesByBundle"] = value; }
        }

        /// <summary>
        /// It indicates if Heat is used for separating Bundles
        /// </summary>
        [ConfigurationProperty("SeparateByHeat", IsRequired = true, DefaultValue = "true")]
        public bool SeparateByHeat
        {
            get { return Convert.ToBoolean(this["SeparateByHeat"]); }
            set { base["SeparateByHeat"] = value; }
        }

        /// <summary>
        /// It indicates if Heat is used for separating Bundles
        /// </summary>
        [ConfigurationProperty("CodeHeat", IsRequired = true, DefaultValue = "HeatNumber")]
        public string CodeHeat
        {
            get { return this["CodeHeat"].ToString() ; }
            set { base["CodeHeat"] = value; }
        }

        /// <summary>
        /// Indicates Url of Web Service IT
        /// </summary>
        [ConfigurationProperty("UrlWebService", IsRequired = false, DefaultValue = "")]
        public string UrlWebService
        {
            get { return this["UrlWebService"].ToString(); }
            set { this["UrlWebService"] = value; }
        }

        /// <summary>
        /// Indicates IT Machines
        /// </summary>
        [ConfigurationProperty("ITMachines", IsRequired = false, DefaultValue = "")]
        public string ITMachines
        {
            get { return this["ITMachines"].ToString(); }
            set { this["ITMachines"] = value; }
        }

        /// <summary>
        /// Indicates IT Machines
        /// </summary>
        [ConfigurationProperty("ITMachineEE", IsRequired = false, DefaultValue = "")]
        public string ITMachineEE
        {
            get { return this["ITMachineEE"].ToString(); }
            set { this["ITMachineEE"] = value; }
        }

        [ConfigurationProperty("IsRequiredEE", IsRequired = false, DefaultValue = "true")]
        public string IsRequiredEE
        {
            get { return this["IsRequiredEE"].ToString(); }
            set { this["IsRequiredEE"] = value; }
        }


        /// <summary>
        /// IXmlSerializable implementation
        /// </summary>
        /// <returns>
        /// Returns null
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// IXmlSerializable implementation
        /// </summary>
        /// <param name="reader">
        /// The reader
        /// </param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.DeserializeElement(reader, false);
        }

        /// <summary>
        /// IXmlSerializable implementation
        /// </summary>
        /// <param name="writer">
        /// The writer
        /// </param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            return;
        }
    }
}
