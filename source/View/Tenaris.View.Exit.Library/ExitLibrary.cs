// -----------------------------------------------------------------------
// <copyright file="ExitLibrary.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------


using Tamsa.Manager.Exit.Shared;
using Tenaris.Library.Log;

namespace Tenaris.View.Exit.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using Tenaris.Tamsa.HRM.Fat2.Library.LineupColorRules;
    using Tenaris.Library.Shared;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExitLibrary
    {

        public static ObservableCollection<Record> Bundles = null;

        private static List<Property> PropertyList = null;
        private static List<Property> BundleList = null;

        private  ObservableCollection<Record> NewRecords;

        private static XDocument BundleGrid;


        private static readonly object Sync = new object();
        private static ExitLibrary instance;

        public static ExitLibrary Instance
        {
            get
            {
                lock (Sync)
                {
                    if (instance == null)
                    {
                        instance = ExitLibrary.instance ?? (instance = new ExitLibrary());
                    }
                    return instance;
                }
            }
        }

        public ExitLibrary()
        {
            BundleGrid = XDocument.Load("Template/BundleGrid.xml"); 
        }

        public void ClearRecord()
        {
            NewRecords = new ObservableCollection<Record>();
        }

        public ObservableCollection<Record> FillData(ICradle cradles, List<IBundle> bundles)
        {
            try
            {
                Bundles = new ObservableCollection<Record>();
                PropertyList = new List<Property>();
                BundleList = new List<Property>();

                

                bundles.ForEach(x => GetData(cradles,x));

             
            }
            catch (Exception ex)
            {
                PropertyList = null;
                Trace.Error(ex.Message);
            }

            PropertyList = null;
            GC.Collect();
            return NewRecords; 

        }


         private void GetData (ICradle cradles,IBundle b)           
         {
             int idBundle = 0;
             int idCradle = 0;
             foreach (MemberInfo mi in b.GetType().GetMembers())
             {
                 if (mi.MemberType == MemberTypes.Property)
                 {
                     PropertyInfo pi = mi as PropertyInfo;
                     if (pi != null)
                     {
                         //Key
                         if (pi.Name == "IdBundle")
                         {
                             idBundle = Convert.ToInt32((pi.GetValue(b, null)));
                         }

                         if (pi.Name == "IdCradle")
                         {
                             idCradle = Convert.ToInt32((pi.GetValue(b, null)));
                             ICradle cradle = cradles;//.FirstOrDefault(c => c.Id == idCradle);
                             if (cradle != null)
                             {
                                 Type CradleType = typeof(ICradle);
                                 foreach (PropertyInfo c in CradleType.GetProperties())
                                 {
                                     if (c.GetValue(cradle,null) != null)
                                        CheckData(BundleGrid, c.Name, (c.GetValue(cradle, null).ToString() != null) ? c.GetValue(cradle, null) : 0,"Cradle");                                     
                                 }
                             }

                         }
                         if (pi.Name == "ExtraData")
                         {
                             if (b.ExtraData != null)                                
                             foreach (KeyValuePair<string, object> data in b.ExtraData)
                             {
                                 CheckData(BundleGrid, data.Key, data.Value, "Bundle");
                             }
                         }

                         
                         //Agrega al observable todas las propiedades del OrderProgram
                     

                     }
                     CheckData(BundleGrid, pi.Name, (pi.GetValue(b, null) != null) ? pi.GetValue(b, null) : 0, "Bundle");
                   
                 }

               
             }
             NewRecords.Add(new Record(BundleList));
             Bundles.Add(new Record(BundleList));

             BundleList = new List<Property>();
             
         
         }

         public List<Property> BundleEmpty(List<Property> lstEmpty, int capacity)
         {
             lstEmpty = new List<Property>();

             for (int i = 0; i < capacity; i++)
             {
                 lstEmpty.Add(new Property("", "", BaseColors.BackGroundHeatEmpty, BaseColors.ForeGroundBlack, true));
             }

             return lstEmpty;
         }


         private void CheckData(XDocument xDocument, string Code, object Value, string Component)
         {

             try
             {

                 var bGrid =
                             from c in xDocument.Descendants("item")
                             where null != c.Elements("Header").Attributes("Code")
                             .FirstOrDefault(t => t.Value == Code)
                             select new
                             {
                                 Code = c.Element("Header").Attribute("Code").Value,
                                 HeaderGrid = c.Element("Header").Attribute("HeaderGrid").Value,
                                 Binding = c.Element("Header").Attribute("Binding").Value,
                                 Component = c.Element("Header").Attribute("Component").Value,
                                 Visible = c.Element("Header").Attribute("Visible").Value,                              

                             };

                 foreach (var item in bGrid)
                 {
                  
                         if (item.Component == Component)
                         {
                             BundleList.Add(new Property((item.HeaderGrid == "") ? item.Code : item.HeaderGrid, Value, BaseColors.ItemColorEmpty, BaseColors.ForeGroundBlack, Convert.ToBoolean(item.Visible)));
                             break;
                         }
                  
                 }

               

             }
             catch (Exception ex)
             {
                 Trace.Error(ex.Message);
             }
         }
      

    }
}
