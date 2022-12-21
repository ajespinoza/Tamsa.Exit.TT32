// -----------------------------------------------------------------------
// <copyright file="SpecificationAdapter.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamsa.Service.Specification.Client;
using Tamsa.Service.Specification.Library;
using Tenaris.Service.Specification.Shared;

namespace Tenaris.View.Exit.Model
{
    using Tenaris.Library.Framework;
    using Tenaris.Library.Log;
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SpecificationAdapter : IDisposable
    {

        #region "Variables"

        /// <summary>
        /// reference to dbclient
        /// </summary>
        SpecificationProxy specificationClient = null;


        #endregion
        #region Constructor
        public SpecificationAdapter(string IPService, int _port)
        {           
            specificationClient = new SpecificationProxy(IPService, _port);
            specificationClient.SpecificationChanged += new EventHandler<SpecificationChangedEventArgs>(specificationClient_SpecificationChanged);
            Initialize();
        }
     

        /// <summary>
        /// Init data access
        /// </summary>
        private void Initialize()
        {

        }


        /// <summary>
        /// Finalize data access
        /// </summary>
        private void Uninitialize()
        {

            specificationClient.Dispose();
            specificationClient = null;
        }

        #endregion

        public void specificationClient_SpecificationChanged(object sender, SpecificationChangedEventArgs e)
        {
            Trace.Message("Update Specification {0}", e.IdSpecification);            
        }

        #region IDisposable
        public void Dispose()
        {
            Uninitialize();
        }
        #endregion


        public Dictionary<string, object> GetProductionUnit(int idSpecification)
        {
            return specificationClient.GetSpecificationUnit(idSpecification);
        }

        public ISpecification GetSpecification(int idSpecification)
        {
            return specificationClient.GetSpecification(idSpecification, null, null, null);
        }

        public Dictionary<string, object> UpdSpecification(int idSpecification, Dictionary<string, Dictionary<string, object>> componentFieldData, int idUser, bool isManual)
        {
            return specificationClient.UpdSpecification(idSpecification, componentFieldData, idUser, isManual);
        }

        public int InsSpecification(Dictionary<string, object> productionUnitValues, Dictionary<string, Dictionary<string, object>> componentFieldData, int idUser, bool isManual)
        {
            return specificationClient.InsSpecification(productionUnitValues, componentFieldData, idUser, isManual);
        }

      



    }
}
