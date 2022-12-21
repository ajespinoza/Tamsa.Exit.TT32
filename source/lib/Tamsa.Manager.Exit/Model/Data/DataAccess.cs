// -----------------------------------------------------------------------
// <copyright file="DataAccess.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the DataAccess class.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.DbClient;
    using Tenaris.Library.Log;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Library.Framework;
    using System.Data;

    /// <summary>
    /// Gets or sets to data access class.
    /// </summary>
    public class DataAccess
    {
        #region "Variables"

        /// <summary>
        /// reference to dbclient
        /// </summary>
        DbClient databaseInstance = null;        
        ReadOnlyDictionary<int, ItemStatus> statuses;
        List<string> BundleFields;
        List<string> ProductFields;

        #endregion

        #region Constructor


        /// <summary>
        /// Inicializa el data access
        /// Por estandard debemos inicializar la data access ya con la instancia propia dbClient
        /// </summary>
        /// <param name="db">instacia dbClient a usar </param>
        internal DataAccess(DbClient db)
        {
            databaseInstance = db;
            Initialize();
            statuses = GetStatuses();
            BundleFields = GetBundleFields();
            ProductFields = GetProductFields();
        }

        
        #endregion

        
        #region Base methods

        void Initialize()
        {
            databaseInstance.AddCommand(StoredProcedure.SpGetCradles);
            databaseInstance.AddCommand(StoredProcedure.SpGetLastItem);
            databaseInstance.AddCommand(StoredProcedure.SpCreateBundlebyTracking);
            databaseInstance.AddCommand(StoredProcedure.SpGetOpenBundle);      
   
            databaseInstance.AddCommand(StoredProcedure.SpGetTrackingsOnBundle);            
            databaseInstance.AddCommand(StoredProcedure.SpCreateTrackingOnBundle);
            databaseInstance.AddCommand(StoredProcedure.SpUpdCloseBundle);
            databaseInstance.AddCommand(StoredProcedure.SpUpdateCradle);
            databaseInstance.AddCommand(StoredProcedure.SpGetStatuses);
            databaseInstance.AddCommand(StoredProcedure.SpGetTrackingData);
            databaseInstance.AddCommand(StoredProcedure.SpGetBundleData);
            databaseInstance.AddCommand(StoredProcedure.SpInsSending);
            databaseInstance.AddCommand(StoredProcedure.SpGetLocation);
            databaseInstance.AddCommand(StoredProcedure.SpUpdMaxPieces);
            databaseInstance.AddCommand(StoredProcedure.SpGetMaxPieces);


        }

        void Uninitialize()
        {
            databaseInstance.RemoveCommand(StoredProcedure.SpGetLocation);
            databaseInstance.RemoveCommand(StoredProcedure.SpInsSending);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetBundleData);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetStatuses);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetTrackingData);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetCradles);           
            databaseInstance.RemoveCommand(StoredProcedure.SpGetOpenBundle);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetTrackingsOnBundle);          
            databaseInstance.RemoveCommand(StoredProcedure.SpCreateTrackingOnBundle);
            databaseInstance.RemoveCommand(StoredProcedure.SpUpdCloseBundle);
            databaseInstance.RemoveCommand(StoredProcedure.SpCreateBundlebyTracking);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetLastItem);
            databaseInstance.RemoveCommand(StoredProcedure.SpUpdateCradle);
            databaseInstance.RemoveCommand(StoredProcedure.SpUpdMaxPieces);
            databaseInstance.RemoveCommand(StoredProcedure.SpGetMaxPieces);
        }

      


        #endregion

        #region Methods

        /// <summary>
        /// Gets cradles for area.
        /// </summary>
        /// <param name="idArea">
        /// Id cradle.
        /// </param>
        /// <returns>
        /// IEnumerable cradles.
        /// </returns>
        public IEnumerable<Cradle> GetCradles(int idArea, bool IsManual)
        {
            int idCradle;
            var cradlesList = new List<Cradle>();
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetCradles);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@idArea", idArea);

                using (var reader = command.ExecuteReader(new ReadOnlyDictionary<string, object>(parameters)))
                {
                    while (reader.Read())
                    {
                        idCradle = Convert.ToInt32(reader["Id"]);
                        var objCradle = new Cradle(
                            idCradle,
                            Convert.ToString(reader["Code"]),
                            Convert.ToString(reader["Name"]),
                            Convert.ToString(reader["Description"]),
                            Convert.ToInt32(reader["IdZone"]),
                            Convert.ToString(reader["TrkZoneCode"]),
                            Convert.ToSingle(reader["MaximumWeight"]),
                            Convert.ToInt32(reader["Mode"]),
                            Convert.ToInt32(reader["State"]));
                        GetBundle(ref objCradle, IsManual);
                        cradlesList.Add(objCradle);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.Message("Error:{0}", ex.Message);
            }
            return cradlesList;
        }

        
        public Bundle GetBundleData(int idBundle)
        {
            Bundle bundle = null;
            DataTable dtBundle;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetBundleData);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdBundle", idBundle);
                dtBundle = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));            
                for (int x = 0; x < dtBundle.Rows.Count; x++)            
                    {
                        bool isOpened = Convert.ToBoolean(dtBundle.Rows[x]["IsOpen"]);
                        int isSent = Convert.ToInt32(dtBundle.Rows[x]["IsSent"]);
                        bool isManual = Convert.ToBoolean(dtBundle.Rows[x]["IsManual"]);
                        BundleState state = (isOpened) ? BundleState.Open : BundleState.Closed;
                        BundleStatus status = (isSent == 3) ? BundleStatus.Sent : BundleStatus.Pending;
                        bundle = new Bundle(Convert.ToInt32(dtBundle.Rows[x]["IdCradle"]),
                         idBundle,                         
                         Convert.ToInt32(dtBundle.Rows[x]["IdBatchHistory"]),
                         Convert.ToInt32(dtBundle.Rows[x]["IdSpecification"]),
                         GetStatus(Convert.ToInt32(dtBundle.Rows[x]["IdItemStatus"])),
                         state,
                         status,
                         dtBundle.Rows[x]["Comments"].ToString(), isManual, dtBundle.Rows[x]["ITLocation"].ToString(), isSent);
                         if (!DBNull.Value.Equals(dtBundle.Rows[x]["CreationDate"]))
                         {
                            bundle.CreationDate = (DateTimeOffset)dtBundle.Rows[x]["CreationDate"];
                         }
                         if (!DBNull.Value.Equals(dtBundle.Rows[x]["RejectionCause"]))
                         {
                             bundle.RejectionCause = dtBundle.Rows[x]["RejectionCause"].ToString();
                         }                         
                         bundle.Location = GetLocation(Convert.ToInt32(dtBundle.Rows[x]["IdLocation"]));                                                  
                         if (bundle != null) // Existe un batch Activo y un Bundle en la canasta abierto
                         {
                             Dictionary<string, object> bundleData = new Dictionary<string, object>();        
                             if (BundleFields.Count > 0)
                             {
                                 foreach (string columnName in BundleFields)
                                 {
                                     if (dtBundle.Columns.Contains(columnName))
                                     {
                                         bundleData.Add(columnName, dtBundle.Rows[x][columnName]);
                                     }
                                 }

                             }                                                                             
                             // Get trackings on bundle.                            
                             List<IItem> items = GetTrackingsOnBundle(Convert.ToInt32(dtBundle.Rows[x]["IdCradle"]), bundle.IdBundle);
                             if (ExitManager.Instance.Config.SeparateByHeat)
                             {                                 
                                 if (items != null)
                                 {
                                     IItem it = items.FirstOrDefault();
                                     foreach (KeyValuePair<string, object> data in it.ExtraData)
                                     {
                                         if (!bundleData.ContainsKey(data.Key))
                                         {
                                             bundleData.Add(data.Key, data.Value);
                                         }
                                     }
                                     foreach (var itm in items)
                                     {                                         
                                         bundle.AddItem(itm);                                        
                                     }                                                                 
                                 }
                             }
                             bundle.ExtraData = bundleData;
                         }                                               
                    }
                  //  reader.Close();
                    if (bundle != null)
                        return bundle;
                    else
                        return null;
                //}
            } // try
            catch (Exception ex)
            {
                Trace.Message("Error:{0}", ex.Message);
            }
            return null;
        }

       

       

        /// <summary>
        /// Create tracking on bundle.
        /// </summary>
        /// <param name="idBundle">
        /// Id bundle.
        /// </param>
        /// <param name="idTracking">
        /// Id tracking.
        /// </param>
        /// <param name="weight">
        /// Weight.
        /// </param>
        /// <returns>
        /// Result.
        /// </returns>
        public bool CreateTrackingBundle(int idBundle, int idTracking, float weight)
        {
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpCreateTrackingOnBundle);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdBundle", idBundle);
                parameters.Add("@IdTracking", idTracking);
                parameters.Add("@Weight", weight);

                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters));

                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool UpdMaxPieces(int idCradle, int? maxPieces)
        {
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpUpdMaxPieces);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@idCradle", idCradle);
                parameters.Add("@MaxPieces", maxPieces);

                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int GetMaxPieces(int idCradle)
        {
            int maxPieces = 0;
            try
            {
                DataTable Record;
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetMaxPieces);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdCradle", idCradle);
                Record = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                for (int x = 0; x < Record.Rows.Count; x++)
                {
                    maxPieces = Convert.ToInt32(Record.Rows[x]["MaximumPieces"]);
                }

                //EN CASO DE QUE LLEGUE 0 SE ASIGNA POR VALOR MINIMO 10 PIEZAS A LA CANASTA
                maxPieces = (maxPieces > 0) ? maxPieces : 10;
                return maxPieces;
            }
            catch (Exception ex)
            {
                return maxPieces = 20;
                Trace.Exception(ex, "Exception GetMaxPieces {0}: {1}", idCradle, ex.ToString());
            }
        }


        /// <summary>
        /// Update bundle (Close)
        /// </summary>
        /// <param name="idBundle">
        /// Id bundle.
        /// </param>
        /// <param name="isOpen">
        /// IsOpen.
        /// </param>
        /// <returns></returns>
        public bool  CloseBundle(int idBundle)
        {
            object resultOut = new Object();
            int result = 0;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpUpdCloseBundle);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdBundle", idBundle);

                Tenaris.Library.Framework.ReadOnlyDictionary<string, object> parametersOut;
                
                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters),out parametersOut);
                parametersOut.TryGetValue("id", out resultOut);
                if (resultOut != null)
                {
                    result = Convert.ToInt32(resultOut);
                }
                if (result > 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update cradle.
        /// </summary>
        /// <param name="idCradle">
        /// Id cradle.
        /// </param>
        /// <param name="mode">
        /// Operation mode.
        /// </param>
        /// <param name="state">
        /// State.
        /// </param>
        /// <returns>
        /// Result
        /// </returns>
        public bool UpdateCradle(int idCradle, int mode, int state)
        {
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpUpdateCradle);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdCradle", idCradle);
                parameters.Add("@Mode", mode);
                parameters.Add("@State", state);

                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters));

                return true;
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Exception Update Cradle {0}: {1}", idCradle, ex.ToString());
                return false;
            }
        }

       
        /// <summary>
        /// Get Item Data
        /// </summary>
        /// <param name="idTracking"></param>
        /// <param name="Length"></param>
        /// <param name="Weight"></param>
        /// <param name="ExitTime"></param>
        public Item GetItemData(int idCradle,int idTracking, ref IProductData productData)
        {
            DataTable dtItems;           
            Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetTrackingData);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@IdTracking", idTracking);
            dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
            for (int x = 0; x < dtItems.Rows.Count; x++)
            {
                bool isManual = Convert.ToBoolean(dtItems.Rows[x]["IsManual"]);
                if (!isManual)
                {
                    int idSpecification = Convert.ToInt32(dtItems.Rows[x]["IdSpecification"]);
                    int idBatch = Convert.ToInt32(dtItems.Rows[x]["IdProductionHistory"]);
                    productData = new ProductData(idBatch, idSpecification);
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    Item item = new Item(idTracking, idCradle, Convert.ToInt32(dtItems.Rows[x]["PipeNumber"]),
                        idBatch, GetStatus(Convert.ToInt32(dtItems.Rows[x]["idItemStatus"])));
                    item.Length = Convert.ToSingle(dtItems.Rows[x]["PipeLength"]);
                    item.Weight = Convert.ToSingle(dtItems.Rows[x]["PipeWeight"]);                                                
                    item.ExitTime = System.DateTimeOffset.Now;
                    item.ExtraData =  new Dictionary<string, object>();
                    if (BundleFields.Count > 0)
                    {
                        foreach (string columnName in BundleFields)
                        {
                            if (dtItems.Columns.Contains(columnName))
                            {
                                item.ExtraData.Add(columnName, dtItems.Rows[x][columnName]);                              
                            }
                        }
                       
                    }
                    if (ProductFields.Count > 0)
                    {
                        foreach (string columnName in ProductFields)
                        {
                            if (dtItems.Columns.Contains(columnName))
                            {                             
                                data.Add(columnName, dtItems.Rows[x][columnName]);
                            }
                        }
                    }
                    productData.Data = data;
                    return item;
                }                
            }
            return null;            
        }

        /// <summary>
        /// Insert into 
        /// </summary>
        /// <param name="idBundle"></param>
        /// <param name="idUser"></param>
        /// <param name="shiftDate"></param>
        /// <param name="shiftNumber"></param>
        /// <param name="sendingString"></param>
        /// <param name="responseString"></param>
        /// <returns></returns>
        public bool InsSending(int idBundle, int bundleNumber, int idUser, string sendingString, string responseString, int isSent, int idMachine, DateTimeOffset responseTime)
        {
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpInsSending);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdBundle", idBundle);
                parameters.Add("@BundleNumber", bundleNumber);
                parameters.Add("@IdUser", idUser);              
                parameters.Add("@IsSent", isSent);
                parameters.Add("@SendingString", sendingString);
                parameters.Add("@ResponseString", responseString);
                parameters.Add("@ResponseDateTime", responseTime);
                parameters.Add("@IdMachine", idMachine);
                parameters.Add("@Id", ParameterDirection.Output);
                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters));
                try
                {
                    idBundle = this.GetParameterValue<int>(command.DbParameters, "@Id");                    

                }
                catch (Exception ex)
                {
                    Trace.Message("Exception: Mismatch recording group to database.", ex);
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Error Update Send Status to Bundle Id {0}: {1}", idBundle, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get Location Data of a Bundle
        /// </summary>
        /// <param name="idLocation"></param>
        /// <returns></returns>
        public ILocation GetLocation(int idLocation)
        {
            DataTable dtLocation;           
            Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetLocation);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@IdLocation", idLocation);
            dtLocation = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
            for (int x = 0; x < dtLocation.Rows.Count; x++)
            {               
                Location location = new Location(idLocation,
                    dtLocation.Rows[x]["Code"].ToString(),
                    dtLocation.Rows[x]["Location"].ToString(),
                    dtLocation.Rows[x]["Description"].ToString(),
                    GetStatus(Convert.ToInt32(dtLocation.Rows[x]["IdItemStatus"])),
                    dtLocation.Rows[x]["ProductionType"].ToString(),
                    dtLocation.Rows[x]["ElaborationStatus"].ToString()
                    );
                return location;
            }
            return null;
        }
      
        #endregion

        #region Private Methods

        /// <summary>
        /// Gets parameter.
        /// </summary>
        /// <typeparam name="TParameter">
        /// TParameter.
        /// </typeparam>
        /// <param name="parameters">
        /// Parameters
        /// </param>
        /// <param name="parameterName">
        /// Parameter name.
        /// </param>
        /// <returns></returns>
        private TParameter GetParameterValue<TParameter>(ReadOnlyDictionary<string, DbParameter> parameters, string parameterName)
        {
            try
            {
                try
                {
                    return (TParameter)Convert.ChangeType(parameters[parameterName].Value, typeof(TParameter));
                }
                catch
                {
                    return (TParameter)parameters[parameterName].Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error getting parameter '{0}' value: {1}", parameterName, ex.Message));
            }
        }

        /// <summary>
        /// Gets trackings on bundle.
        /// </summary>
        /// <param name="idBundle">
        /// Id bundle.
        /// </param>
        /// <returns>
        /// Tracking list on bundle.
        /// </returns>
        public List<IItem> GetTrackingsOnBundle(int idCradle,int idBundle)
        {
            DataTable dtItems;
            var items = new List<IItem>();
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetTrackingsOnBundle);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdBundle", idBundle);
                dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                for (int x = 0; x < dtItems.Rows.Count; x++)
                {                
                        var i = new Item(
                            Convert.ToInt32(dtItems.Rows[x]["IdTracking"]),
                            idBundle,
                            idCradle,
                            Convert.ToInt32(dtItems.Rows[x]["PipeNumber"]),
                            Convert.ToInt32(dtItems.Rows[x]["IdProductionHistory"]),
                            GetStatus(Convert.ToInt32(dtItems.Rows[x]["idItemStatus"])),
                            Convert.ToSingle(dtItems.Rows[x]["PipeLength"]),
                            Convert.ToSingle(dtItems.Rows[x]["PipeWeight"]),
                            (DateTimeOffset)dtItems.Rows[x]["ExitDateTime"],
                            dtItems.Rows[x]["EE"].ToString());
                            Dictionary<string, object> itemData = new Dictionary<string, object>();                          
                            if (BundleFields.Count > 0)
                            {
                                foreach (string columnName in BundleFields)
                                {
                                    if (dtItems.Columns.Contains(columnName))
                                    {
                                        itemData.Add(columnName, dtItems.Rows[x][columnName]);
                                    }
                                }

                            }
                            i.ExtraData = itemData;
                            items.Add(i);
                    }                    
            }
            
            catch (Exception ex)
            {
                Trace.Message("Error Get Tracking of Bundle {0} :{1}", idBundle, ex.Message);
            }

            return items;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Get Fields from Config
        /// </summary>
        /// <returns></returns>
        internal List<string> GetBundleFields()
        {
            List<string> fields = new List<string>();
            try
            {
                foreach (FieldsElement element in ExitManager.Instance.Config.BundleFields)
                {
                    fields.Add(element.ColumnName);
                }
                return fields;

            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
                return fields;
            }
        }

        internal List<string> GetProductFields()
        {
            List<string> fields = new List<string>();
            try
            {
                foreach (FieldsElement element in ExitManager.Instance.Config.ProductFields)
                {
                    fields.Add(element.ColumnName);
                }
                return fields;

            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
                return fields;
            }
        }

        /// <summary>
        /// Get Status's from DB
        /// </summary>
        /// <returns></returns>
        internal ReadOnlyDictionary<int, ItemStatus> GetStatuses()
        {
            DataTable dtResult;
            Dictionary<int, ItemStatus> status = new Dictionary<int, ItemStatus>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedure.SpGetStatuses);
            dtResult = cmd.ExecuteTable();
            for (int x = 0; x < dtResult.Rows.Count; x++)
            {
                int id = (int)dtResult.Rows[x]["IdItemStatus"];
                string code = (string)dtResult.Rows[x]["Code"];
                switch (code)
                {
                    case "Good":
                        status.Add(id, ItemStatus.Good);
                        break;
                    case "Rejected":
                        status.Add(id, ItemStatus.Rejected);
                        break;
                    case "Warned":
                        status.Add(id, ItemStatus.Warned);
                        break;
                    case "Pending":
                        status.Add(id, ItemStatus.Pending);
                        break;
                    case "Discarded":
                        status.Add(id, ItemStatus.Discarded);
                        break;
                    case "Hold":
                        status.Add(id, ItemStatus.Hold);
                        break;
                    case "Canceled":
                        status.Add(id, ItemStatus.Canceled);
                        break;
                    case "Reworked":
                        status.Add(id, ItemStatus.Reworked);
                        break;
                }


            }
            return new ReadOnlyDictionary<int, ItemStatus>(status);
        }

        /// <summary>
        /// Convert Id Status to Status
        /// </summary>
        /// <param name="idStatus"></param>
        /// <returns></returns>
        internal ItemStatus GetStatus(int idStatus)
        {
            ItemStatus result;
            if (statuses.TryGetValue(idStatus, out result))
                return result;
            else
                return ItemStatus.Good;
        }

        /// <summary>
        /// Convert Status to Id
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        internal int GetIdStatus(ItemStatus st)
        {
            int result = statuses.First(s => s.Value == st).Key;
            if (result > 0)
                return result;
            else
                return 0;
        }

        /// <summary>
        /// Get Current Bundle of a Cradle (if it exists)
        /// </summary>
        /// <param name="cradle">
        /// Cradle Id
        /// </param>
        internal void GetBundle(ref Cradle cradle, bool isManual)
        {
            try
            {
                Bundle bundle;
                if (cradle.Mode == CradleMode.Automatic)
                {
                    IItem lastItem = null;
                    IProductData productData = null;
                    lastItem = GetLastItem(cradle.Id, ref productData);
                    //if(isManual)
                    //    bundle = GetOpenBundle(cradle.Id);
                    bundle = GetOpenBundle(cradle.Id);
                    if (bundle != null) // Existe un batch Activo y un Bundle en la canasta abierto
                    {
                        Trace.Message("Initializing Cradle {0} with Mode {1} ", cradle.Id, cradle.Mode.ToString());
                        // Get trackings on bundle.                            
                        List<IItem> items = GetTrackingsOnBundle(cradle.Id, bundle.IdBundle);
                        IItem firstItem = items.FirstOrDefault();
                        if (firstItem != null)
                        {
                            if (lastItem != null)
                            {
                                if (!lastItem.Equals(firstItem))
                                {
                                    if (isManual == false)
                                    {
                                        //Cerrar en BD el Atado Abierto y Crear uno con el nuevo Tracking
                                        if (CloseBundle(bundle.IdBundle))
                                            bundle.Close();
                                        bundle = CreateBundlebyTracking(cradle.Id, lastItem.Id);
                                        Trace.Debug("Insertando atado en canasta: {0}, IdTracking: {1}", cradle.Id, lastItem.Id);
                                        bundle.AddItem(lastItem);
                                    }
                                    else
                                    {
                                        Trace.Debug("Actualizacion de la vista");
                                    }
                                }
                                else
                                {
                                    bundle.AddItems(items);
                                }
                                cradle.PreviousItem = lastItem;
                                cradle.ProductData = productData;
                                Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                bundleData = lastItem.ExtraData;
                                bundle.ExtraData = bundleData;                                
                            }
                        }                      
                    }
                    else // Si no existe ningún atado 
                    {
                        if (lastItem != null)
                        {
                            if (isManual == false)
                            {
                                bundle = CreateBundlebyTracking(cradle.Id, lastItem.Id);
                                Trace.Debug("Insertando atado en canasta: {0}, IdTracking: {1} en GetBundle", cradle.Id, lastItem.Id);
                                bundle.AddItem(lastItem);
                                cradle.PreviousItem = lastItem;
                                cradle.ProductData = productData;
                                Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                bundleData = lastItem.ExtraData;
                                bundle.ExtraData = bundleData;
                            }
                            else
                            { // CCD
                                cradle.PreviousItem = lastItem;
                                cradle.ProductData = productData;
                                //Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                //bundleData = lastItem.ExtraData;
                                //bundle.ExtraData = bundleData;
                                cradle.MaximumPieces = ExitManager.Instance.dataAccess.GetMaxPieces(cradle.Id);
                                Trace.Debug("Actualización de la Vista con lastItem diferente de nulo");
                            }
                        }
                    }
                    if (bundle != null && cradle.ProductData != null)
                    {
                        Trace.Message("Initializing Cradle {0} with Bundle {1} and idBatch {2}", cradle.Id, bundle.IdBundle, cradle.ProductData.IdBatch);
                        if (bundle.Weight > 0)
                        {
                            cradle.MaximumPieces = ExitManager.Instance.dataAccess.GetMaxPieces(cradle.Id);
                        }
                        else
                            cradle.MaximumPieces = ExitManager.Instance.dataAccess.GetMaxPieces(cradle.Id);
                        cradle.CurrentBundle = bundle;
                    }
                } // En Modo Manual debe buscar los valores del Producto Actual en la Zona asociada a la Canasta.
                else
                {
                    IItem lastItem = null;
                    IProductData productData = null;
                    lastItem = GetLastItem(cradle.Id, ref productData);
                    if (lastItem != null) // Existe un batch Activo y un Bundle en la canasta abierto
                    {
                        if (productData != null)
                        {
                            Trace.Message("Initializing Cradle {0} with Mode {1} and idBatch {2} ", cradle.Id, cradle.Mode.ToString(), productData.IdBatch);
                            cradle.ProductData = productData;
                        }
                        cradle.MaximumPieces = ExitManager.Instance.dataAccess.GetMaxPieces(cradle.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Exception Get Current Bundle of Cradle Id {0} : {1}", cradle.Id, ex.ToString());
            }

        }


        /// <summary>
        /// Get Last Item into Asociated Zone of Cradle
        /// </summary>
        /// <param name="idCradle">
        /// Cradle Id
        /// </param>
        /// <param name="ProductData">
        /// Product Data of Item
        /// </param>
        /// <returns>Last Item</returns>
        internal IItem GetLastItem(int idCradle, ref IProductData ProductData)
        {
            IItem result = null;
            ProductData = null;
            DataTable dtItem;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetLastItem);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdCradle", idCradle);
                dtItem = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                for (int x = 0; x < dtItem.Rows.Count; x++)
                {
                    int idTracking = Convert.ToInt32(dtItem.Rows[x]["IdTracking"]);
                    result = new Item(idTracking, idCradle, Convert.ToInt32(dtItem.Rows[x]["TraceabilityNumber"]),
                             Convert.ToInt32(dtItem.Rows[x]["IdProductionHistory"]),
                               GetStatus(Convert.ToInt32(dtItem.Rows[x]["IdItemStatus"])));
                    if (!DBNull.Value.Equals(dtItem.Rows[x]["ExitDateTime"]))
                        result.ExitTime = (DateTimeOffset)dtItem.Rows[x]["ExitDateTime"];
                    ProductData = new ProductData(Convert.ToInt32(dtItem.Rows[x]["IdProductionHistory"]), Convert.ToInt32(dtItem.Rows[x]["IdSpecification"]));
                    Dictionary<string, object> Data = new Dictionary<string, object>();
                    result.ExtraData = new Dictionary<string, object>();
                    if (BundleFields.Count > 0)
                    {
                        foreach (string columnName in BundleFields)
                        {
                            if (dtItem.Columns.Contains(columnName))
                            {                               
                                result.ExtraData.Add(columnName, dtItem.Rows[x][columnName]);
                            }
                        }
                    }
                    if (ProductFields.Count > 0)
                    {
                        foreach (string columnName in ProductFields)
                        {
                            if (dtItem.Columns.Contains(columnName))
                            {
                                Data.Add(columnName, dtItem.Rows[x][columnName]);                             
                            }
                        }
                    }
                    ProductData.Data = Data;
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.Message("Error Get Last Item from Cradle Id :{0}", ex.Message);
                return null;
            }

        }

        /// <summary>
        /// Get Opened Bundle of a Cradle
        /// </summary>
        /// <param name="idCradle">
        /// Cradle Id
        /// </param>
        /// <returns></returns>
        internal Bundle GetOpenBundle(int idCradle)
        {
            Bundle bundle = null;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpGetOpenBundle);

                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdCradle", idCradle);
                using (var reader = command.ExecuteReader(new ReadOnlyDictionary<string, object>(parameters)))
                {
                    while (reader.Read())
                    {
                        bundle = new Bundle(idCradle,
                         Convert.ToInt32(reader["IdBundle"]),
                         Convert.ToInt32(reader["IdBatchHistory"]),
                         Convert.ToInt32(reader["IdSpecification"]),
                         GetStatus(Convert.ToInt32(reader["IdItemStatus"])),
                         BundleState.Open,
                         BundleStatus.InProcess,
                         reader["Comments"].ToString(), false);
                        if (!DBNull.Value.Equals(reader["CreationDate"]))
                        {
                            bundle.CreationDate = (DateTimeOffset)reader["CreationDate"];
                        }
                        bundle.Location = GetLocation(Convert.ToInt32(reader["IdLocation"]));
                    }
                    reader.Close();
                    if (bundle != null)
                        return bundle;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Trace.Message("Error:{0}", ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Create Bundle from a Tracking
        /// </summary>
        /// <param name="idCradle">
        /// Cradle Id
        /// </param>
        /// <param name="idTracking">
        /// Tracking Id
        /// </param>
        /// <returns></returns>
        internal Bundle CreateBundlebyTracking(int idCradle, int idTracking)
        {
            Bundle bundle = null;
            int idBundle = 0;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedure.SpCreateBundlebyTracking);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdCradle", idCradle);
                parameters.Add("@IdTracking", idTracking);
                parameters.Add("@Id", ParameterDirection.Output);

                command.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(parameters));
                try
                {
                    idBundle = this.GetParameterValue<int>(command.DbParameters, "@Id");                    
                }
                catch (Exception ex)
                {
                    Trace.Message("Exception: Mismatch recording group to database.", ex);
                }
                if (idBundle > 0)
                {
                    bundle = GetOpenBundle(idCradle);
                    return bundle;
                }
                    else
                    return null;              
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Exception Creating Bundle for Cradle Id {0} by IdTracking {1} : {2}", idCradle, idTracking, ex.ToString());
                return null;
            }
            
        }


        #endregion
    }
}



