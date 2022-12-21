// -----------------------------------------------------------------------
// <copyright file="ProductData.cs" company="Techint">
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
    /// Product Data class.
    /// </summary>
    [Serializable]
    public class ProductData : IProductData
    {

        #region Constructor

        public ProductData(int idBatch, int idSpecification)
        {
            this.IdBatch = idBatch;
            this.IdSpecification = idSpecification;
            this.Data = new Dictionary<string, object>();
        }

        #endregion

        public int IdBatch
        {
            get;
            set;
        }

        public int IdSpecification
        {
            get;
            set;
        }

        public Dictionary<string, object> Data
        {
            get;
            set;
        }

        public override bool Equals(object x)
        {
            ProductData objProduct = (ProductData)x;
            bool result = false;
            if (this.IdBatch == objProduct.IdBatch && this.IdSpecification == objProduct.IdSpecification)
                result = true;
            foreach (KeyValuePair<string, object> extra in this.Data)
            {
                if (objProduct.Data.ContainsKey(extra.Key))
                {
                    result = result && this.Data[extra.Key].Equals(objProduct.Data[extra.Key]);
                }
            }
            return result;
        }

        public override string ToString()
        {
            string result;
            result = String.Format(" Product Data : IdBatch {0} , IdSpecification {1}  Extra Data ", IdBatch, IdSpecification);
            foreach (KeyValuePair<string, object> extra in this.Data)
            {
                result = result + " " + extra.Key + " - " + extra.Value.ToString();
            }
            return result;
        }
             
    }
}
