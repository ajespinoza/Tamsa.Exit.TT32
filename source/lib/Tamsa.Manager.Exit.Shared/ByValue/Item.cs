// -----------------------------------------------------------------------
// <copyright file="Item.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class Item : IItem
    {
        #region Constructor

        public Item(int id)
        {
            this.Id = id;
        }

        public Item(int id, int idCradle, int number, int idBatch, ItemStatus status)
        {
            this.Id = id;            
            this.IdCradle = idCradle;
            this.Number = number;
            this.IdBatch = idBatch;
            this.Status = status;
            this.IsSelected = false;
        }

        public Item(int id, int idBundle, int idCradle, int number, int idBatch, ItemStatus status, float length, float weight, DateTimeOffset exitTime, string ee)
        {
            this.Id = id;
            this.IdBundle = idBundle;
            this.IdCradle = idCradle;
            this.Number = number;
            this.IdBatch = idBatch;
            this.Status = status;
            this.Length = length;
            this.Weight = weight;           
            this.ExitTime = exitTime;
            this.EE = ee;
            this.IsSelected = false;
        }
        #endregion

        public int Id
        {
            get;
            set;
        }

        public int IdBundle
        {
            get;
            set;
        }

        public int IdCradle
        {
            get;
            set;
        }

        public int Number
        {
            get;
            set;
        }

        public int IdBatch
        {
            get;
            set;
        }

        public float Length
        {
            get;
            set;
        }

        public string EE
        {
            get;
            set;
        }

        public float Weight
        {
            get;
            set;
        }

        public ItemStatus Status
        {
            get;
            set;
        }

        public DateTimeOffset ExitTime
        {
            get;
            set;
        }

        public Dictionary<string, object> ExtraData
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get;
            set;
        }

        public override bool Equals(object x)
        {
            Item objItem = (Item)x;
            bool result = false;
            if (this.IdBatch == objItem.IdBatch && this.Status == objItem.Status)
                result = true;
            foreach (KeyValuePair<string,object> extra in this.ExtraData)
            {
                if (objItem.ExtraData.ContainsKey(extra.Key))
                {
                    result = result && this.ExtraData[extra.Key].Equals(objItem.ExtraData[extra.Key]);
                }
            }
            return result;
        }
    
    }
}
