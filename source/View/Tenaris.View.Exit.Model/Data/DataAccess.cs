// -----------------------------------------------------------------------
// <copyright file="DataAccess.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.DbClient;
    using System.Data;
    using Tenaris.Library.Framework;
    using Tenaris.Library.Log;
    using Tamsa.Manager.Exit.Shared;
    using System.Collections.ObjectModel;

    //using Tenaris.Tamsa.HRM.Fat2.Library.LineupColorRules;    
    /// <summary>
    /// Clase para el acceso a la base de datos
    /// </summary>
    public class DataAccess
    {
        #region "Variables"

        /// <summary>
        /// reference to dbclient
        /// </summary>
        DbClient databaseInstance = null;
        ReadOnlyDictionary<int, ItemStatus> statuses;
        ReadOnlyDictionary<int, ILocation> Locations;
        IEnumerable<RejectionCause> RejectionCauses;
        ReadOnlyDictionary<int, string> shifts;     

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
            RejectionCauses = GetRejectionCauses();
            Locations = GetDataLocations();
            shifts = GetShifts();            
        }

        void Initialize()
        {
            // add required storeds

            // NOTA
            // Los nombres de los storeds deben definirse en la clase StoredProcedures

            databaseInstance.AddCommand(StoredProcedures.SpGetAreaComamnd);
            databaseInstance.AddCommand(StoredProcedures.SpGetMachinesComamnd);
            databaseInstance.AddCommand(StoredProcedures.SpGetBundlesbyBatch);
            databaseInstance.AddCommand(StoredProcedures.SpGetStatuses);
            databaseInstance.AddCommand(StoredProcedures.SpGetTrackingsOnBundle);
            databaseInstance.AddCommand(StoredProcedures.FSGetIdUser);
            databaseInstance.AddCommand(StoredProcedures.SpGetAvailableItems);
            databaseInstance.AddCommand(StoredProcedures.SpGetAvailableItemsByProduct, 5000);
            databaseInstance.AddCommand(StoredProcedures.SpDelBundle);
            databaseInstance.AddCommand(StoredProcedures.SpInsbundle);
            databaseInstance.AddCommand(StoredProcedures.SpUpdRejectBundle);
            databaseInstance.AddCommand(StoredProcedures.SpCreateTrackingOnBundle);
            databaseInstance.AddCommand(StoredProcedures.SpUpdTrackingBundle);
            databaseInstance.AddCommand(StoredProcedures.SpDelTrackingBundle);
            databaseInstance.AddCommand(StoredProcedures.SpGetQualityCodes);
            databaseInstance.AddCommand(StoredProcedures.SpGetShifts);
            databaseInstance.AddCommand(StoredProcedures.SpGetHistoricBundles);
            databaseInstance.AddCommand(StoredProcedures.SpGetLocations);
            databaseInstance.AddCommand(StoredProcedures.SpGetBundleData);
            databaseInstance.AddCommand(StoredProcedures.SpGetUserInformation);
            databaseInstance.AddCommand(StoredProcedures.SpGetGroupElaborationState);
            databaseInstance.AddCommand(StoredProcedures.SpGetElaborationStateByGroup);
            databaseInstance.AddCommand(StoredProcedures.SpGetRejectionCodeByElaborationState);
        }

        /// <summary>
        /// Finalize data access
        /// </summary>
        internal void Uninitialize()
        {
            // remove added storeds

            // TO DO
            // Remover los comandos propios de la vista
            databaseInstance.RemoveCommand(StoredProcedures.SpDelBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetAvailableItems);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetAvailableItemsByProduct);
            databaseInstance.RemoveCommand(StoredProcedures.FSGetIdUser);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetTrackingsOnBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetBundlesbyBatch);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetMachinesComamnd);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetAreaComamnd);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetStatuses);
            databaseInstance.RemoveCommand(StoredProcedures.SpInsbundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpUpdRejectBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpCreateTrackingOnBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpUpdTrackingBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpDelTrackingBundle);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetQualityCodes);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetShifts);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetHistoricBundles);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetLocations);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetBundleData);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetUserInformation);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetGroupElaborationState);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetElaborationStateByGroup);
            databaseInstance.RemoveCommand(StoredProcedures.SpGetRejectionCodeByElaborationState);
            // libera la instacia de la base de datos
            databaseInstance.Dispose();
            databaseInstance = null;
        }

        #endregion


        // NOTA
        // Se recomienda que las funciones no regresen un DataTable a menos que sea necesario.
        // Evitar el uso de cursores para no impactar el performace de la base de datos.
        // De preferencia trae toda la informacion requerida de la base de datos y luego
        // procesarla en codigo.
        // Las excepciones se deben dejar pasar para que sean procesadas por quien llamo la funcion.


        // TO DO
        // Adicionar las funciones de acceso a la base de datos
        // Ejemplos
    

        /// <summary>
        /// Obtiene el registro del areaCode indicado
        /// </summary>
        /// <param name="areaCode">area code</param>
        /// <returns>registro del area code solicitado</returns>
        internal AreaRecord GetArea(string areaCode)
        {
            DataTable dtResult;

            // Instancia el comando
            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetAreaComamnd);

            // Preparacion de parametros de entrada
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("@Code", areaCode);

            // Ejecucion del stored
            dtResult = cmd.ExecuteTable(new ReadOnlyDictionary<string, object>(cmdParams));

            // Verificacion de resultado del stored
            if (dtResult.Rows.Count != 1) {
                throw new ApplicationException("Failed to obtain area record from db");
            }

            // Procesamiento de la respuesta
            AreaRecord area = new AreaRecord();
            area.idArea = (int)dtResult.Rows[0]["idArea"];
            area.Code = (string)dtResult.Rows[0]["Code"];
            area.Name = (string)dtResult.Rows[0]["Name"];
            area.Description = DBNull.Value.Equals(dtResult.Rows[0]["Description"]) ? "" : (string)dtResult.Rows[0]["Description"];

            return area;
        }


        /// <summary>
        /// Obtiene la lista de machines perteneciente al area code indicado
        /// </summary>
        /// <param name="areaCode">Nombre del area</param>
        /// <returns>Lista de registros de los machine pertenecientes al area code indicado</returns>
        internal List<MachineRecord> GetMachines(string areaCode)
        {
            DataTable dtResult;
            List<MachineRecord> machineList = new List<MachineRecord>();

            // Instancia el comando
            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetMachinesComamnd);

            // Preparacion de parametros de entrada
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("@AreaCode", areaCode);

            // Ejecucion del stored
            dtResult = cmd.ExecuteTable(new ReadOnlyDictionary<string, object>(cmdParams));

            // Procesamiento de la respuesta
            for (int x = 0; x < dtResult.Rows.Count; x++) {

                MachineRecord machine = new MachineRecord();

                machine.idMachine = (int)dtResult.Rows[x]["idMachine"];
                machine.idArea = (int)dtResult.Rows[x]["idArea"];
                machine.Code = (string)dtResult.Rows[x]["Code"];
                machine.Name = (string)dtResult.Rows[x]["Name"];
                machine.Description = DBNull.Value.Equals(dtResult.Rows[x]["Description"]) ? "" : (string)dtResult.Rows[x]["Description"];

                machineList.Add(machine);

            } // for

            return machineList;
        }

        /// <summary>
        /// Obtiene los atados de un Batch determinado
        /// </summary>
        /// <param name="idBatch"></param>
        /// <returns></returns>
        public List<IBundle> GetBundles(int idBatch, int idCradle)
        {
            DataTable dtResult;
            List<IBundle> bundleList = new List<IBundle>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetBundlesbyBatch);

            // Preparacion de parametros de entrada
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("@idBatch", idBatch);
            cmdParams.Add("@idCradle", idCradle);

            // Ejecucion del stored
            dtResult = cmd.ExecuteTable(new ReadOnlyDictionary<string, object>(cmdParams));

            // Procesamiento de la respuesta
            for (int x = 0; x < dtResult.Rows.Count; x++)
            {
                int isSent = Convert.ToInt32(dtResult.Rows[x]["IsSent"]);
                bool isManual = Convert.ToBoolean(dtResult.Rows[x]["IsManual"]);
                BundleStatus status = (isSent == 1) ? BundleStatus.Sent : BundleStatus.Pending;                                
                Bundle bundle = new Bundle(
                   Convert.ToInt32(dtResult.Rows[x]["idCradle"]),
                   Convert.ToInt32(dtResult.Rows[x]["idBundle"]),
                   Convert.ToInt32(dtResult.Rows[x]["idBatchHistory"]),
                   Convert.ToInt32(dtResult.Rows[x]["idSpecification"]),
                   GetStatus(Convert.ToInt32(dtResult.Rows[x]["idItemStatus"])),
                   BundleState.Closed,
                   status,                   
                   Convert.ToString(dtResult.Rows[x]["Comments"]),isManual
                   );
                   if (!DBNull.Value.Equals(dtResult.Rows[x]["CreationDate"]))
                   {
                    bundle.CreationDate = (DateTimeOffset)dtResult.Rows[x]["CreationDate"];
                   }
                   if (!DBNull.Value.Equals(dtResult.Rows[x]["ShiftDate"]))
                   {
                       bundle.SendingDate = (DateTimeOffset)dtResult.Rows[x]["ShiftDate"];
                   }
                   if (isSent==1)
                   {
                       bundle.Send(Convert.ToInt32(dtResult.Rows[x]["Number"]));
                   }
                   if (!DBNull.Value.Equals(dtResult.Rows[x]["RejectionCause"]))
                   {
                       bundle.RejectionCause = dtResult.Rows[x]["RejectionCause"].ToString();
                   }
                   bundle.Location = ExitModel.Instance.Data.GetLocation(Convert.ToInt32(dtResult.Rows[x]["IdLocation"]));
               
                bundleList.Add(bundle);

            } // for
            return bundleList;            
        }

        public List<IBundle> GetBundles(bool isCheckedDate, DateTime ShiftDate, int ShiftNumber, int OrderNumber)
        {
            DataTable dtResult;
            List<IBundle> bundleList = new List<IBundle>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetHistoricBundles);
            var parameters = new Dictionary<string, object>();
            
            if (isCheckedDate)
                parameters.Add("@ShiftDate", ShiftDate);
            //if (ShiftNumber > 0)
                parameters.Add("@ShiftNumber", ShiftNumber);
            if (OrderNumber > 0)
                parameters.Add("@IdBatch", OrderNumber);

            // Ejecucion del stored
            dtResult = cmd.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
            // Procesamiento de la respuesta
            for (int x = 0; x < dtResult.Rows.Count; x++)
            {
                int isSent = Convert.ToInt32(dtResult.Rows[x]["IsSent"]);
                bool isManual = Convert.ToBoolean(dtResult.Rows[x]["IsManual"]);
                BundleStatus status = (isSent == 3) ? BundleStatus.Sent : BundleStatus.Pending;
                Bundle bundle = new Bundle(
                   Convert.ToInt32(dtResult.Rows[x]["idCradle"]),
                   Convert.ToInt32(dtResult.Rows[x]["idBundle"]),
                   Convert.ToInt32(dtResult.Rows[x]["idBatchHistory"]),
                   Convert.ToInt32(dtResult.Rows[x]["idSpecification"]),
                   GetStatus(Convert.ToInt32(dtResult.Rows[x]["idItemStatus"])),
                   BundleState.Closed,
                   status,
                   Convert.ToString(dtResult.Rows[x]["Comments"]), isManual
                   );
                if (!DBNull.Value.Equals(dtResult.Rows[x]["CreationDate"]))
                {
                    bundle.CreationDate = (DateTimeOffset)dtResult.Rows[x]["CreationDate"];
                }
                if (!DBNull.Value.Equals(dtResult.Rows[x]["ShiftDate"]))
                {
                    bundle.SendingDate = (DateTimeOffset)dtResult.Rows[x]["ShiftDate"];
                }
                //if (isSent == 1)
                //{
                //    bundle.Send(Convert.ToInt32(dtResult.Rows[x]["Number"]));
                //}
                if (isSent == 3 || Convert.ToInt32(dtResult.Rows[x]["Number"]) > 0)
                {
                    bundle.Sent = isSent;
                    bundle.Send(Convert.ToInt32(dtResult.Rows[x]["Number"]));
                }

                
                bundle.Location = ExitModel.Instance.Data.GetLocation(Convert.ToInt32(dtResult.Rows[x]["IdLocation"]));
                bundleList.Add(bundle);

            } // for

            return bundleList;            

        }

        /// <summary>
        /// Get Bundle Data
        /// </summary>
        /// <param name="idBundle"></param>
        /// <returns></returns>
        public Bundle GetBundleData(int idBundle)
        {
            Bundle bundle = null;
            DataTable dtBundle;
            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetBundleData);

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




        public IEnumerable<RejectionCause> GetRejectionCauses(ItemStatus itemStatus)
        {
            int idItemStatus;
            idItemStatus = GetIdStatus(itemStatus);
            return RejectionCauses.Where(rc => rc.IdItemStatus == idItemStatus);
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
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetTrackingsOnBundle);
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
                    foreach (DataColumn column in dtItems.Columns)
                    {
                        if (!itemData.ContainsKey(column.ColumnName))
                        {
                            itemData.Add(column.ColumnName, dtItems.Rows[x][column.ColumnName]);
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

         public List<IItem> GetAvailableItems(int idBatchHistory, int idCradle)
         {
             DataTable dtItems;
             var items = new List<IItem>();
             try
             {
                 Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetAvailableItems);
                 var parameters = new Dictionary<string, object>();
                 parameters.Add("@idBatchHistory", idBatchHistory);
                 parameters.Add("@idCradle", idCradle);  
                 dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                 for (int x = 0; x < dtItems.Rows.Count; x++)
                 {
                     var i = new Item(
                      Convert.ToInt32(dtItems.Rows[x]["IdTracking"]),
                      0,
                      Convert.ToInt32(dtItems.Rows[x]["IdCradle"]),                      
                      Convert.ToInt32(dtItems.Rows[x]["PipeNumber"]),
                      Convert.ToInt32(dtItems.Rows[x]["IdProductionHistory"]),
                      GetStatus(Convert.ToInt32(dtItems.Rows[x]["idItemStatus"])),
                      Convert.ToSingle(dtItems.Rows[x]["PipeLength"]),
                      Convert.ToSingle(dtItems.Rows[x]["PipeWeight"]),
                      (DateTimeOffset)dtItems.Rows[x]["ExitDateTime"],
                      dtItems.Rows[x]["EE"].ToString());

                      Dictionary<string, object> itemData = new Dictionary<string, object>();
                      foreach (DataColumn column in dtItems.Columns)
                      {
                          if (!itemData.ContainsKey(column.ColumnName))
                          {
                              itemData.Add(column.ColumnName, dtItems.Rows[x][column.ColumnName]);
                          }
                      }                    
                    i.ExtraData = itemData;
                    items.Add(i);
                 }
            }
            catch (Exception ex)
            {
                Trace.Message("Error Get Items from IdBatchHistory {0} :{1}", idBatchHistory, ex.Message);
            }

            return items;            
         }


        //
         public List<IItem> GetAvailableItemsByProduct(int order, int heat)
         {
             DataTable dtItems;
             var items = new List<IItem>();
             try
             {
                 Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetAvailableItemsByProduct);
                 var parameters = new Dictionary<string, object>();
                 parameters.Add("@Order", order);
                 parameters.Add("@Heat", heat);
                 dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                 for (int x = 0; x < dtItems.Rows.Count; x++)
                 {
                     var i = new Item(
                      Convert.ToInt32(dtItems.Rows[x]["IdTracking"]),
                      0,
                      Convert.ToInt32(dtItems.Rows[x]["IdCradle"]),
                      Convert.ToInt32(dtItems.Rows[x]["PipeNumber"]),
                      Convert.ToInt32(dtItems.Rows[x]["IdProductionHistory"]),
                      GetStatus(Convert.ToInt32(dtItems.Rows[x]["idItemStatus"])),
                      Convert.ToSingle(dtItems.Rows[x]["PipeLength"]),
                      Convert.ToSingle(dtItems.Rows[x]["PipeWeight"]),
                      (DateTimeOffset)dtItems.Rows[x]["ExitDateTime"],
                      dtItems.Rows[x]["EE"].ToString());

                     Dictionary<string, object> itemData = new Dictionary<string, object>();
                     foreach (DataColumn column in dtItems.Columns)
                     {
                         if (!itemData.ContainsKey(column.ColumnName))
                         {
                             itemData.Add(column.ColumnName, dtItems.Rows[x][column.ColumnName]);
                         }
                     }
                     i.ExtraData = itemData;
                     items.Add(i);
                 }
             }
             catch (Exception ex)
             {
                 Trace.Message(ex.Message);
             }

             return items;
         }
        //

        /// <summary>
        /// Get Logged User Id
        /// </summary>
        /// <param name="Identification"></param>
        /// <returns></returns>
        public int GetIdbyIdentification(string Identification)
        {
            try
            {
                int valueReturn = -1;

                using (var dbCommand = databaseInstance.GetCommand(StoredProcedures.FSGetIdUser))
                {
                    var dictionary = new Dictionary<string, object>() 
                    {
                        {"@Identification", Identification}
                    };

                    ReadOnlyDictionary<string, object> outputParams = null;
                    valueReturn = dbCommand.ExecuteNonQuery(new ReadOnlyDictionary<string, object>(dictionary), out outputParams);
                    if (outputParams != null)
                        valueReturn = Convert.ToInt32(outputParams.ElementAt(0).Value);

                }

                return valueReturn;
            }

            catch (Exception ex)
            {
                Tenaris.Library.Log.Trace.Exception(ex, "{0} Error in DataAccess.GetIdbyIdentification -> ");
                return 0;
            }
        }


        /// <summary>
        /// Get Logged User Id
        /// </summary>
        /// <param name="Identification"></param>
        /// <returns></returns>
        public string GetPipUser(string Identification)
        {
            DataTable dtItems;
            var items = new List<IItem>();

            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetUserInformation);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@Identification", Identification);
                dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                //for (int x = 0; x < dtItems.Rows.Count; x++)
                //{
                string PipIdentification = dtItems.Rows[0]["PipCode"].ToString();
                return PipIdentification;
                //}
            }

            catch (Exception ex)
            {
                Tenaris.Library.Log.Trace.Exception(ex, "{0} Error in DataAccess.GetIdbyIdentification -> ");
                return null;
            }
        }

        /// <summary>
        /// Delete a Folio with their orders
        /// </summary>
        /// <param name="idFolio">
        /// Folio id for deleting
        /// </param>
        /// <returns>
        /// indicates if Folio was deleted : 0 (Not) , id  (Yes)
        /// </returns>
        public int DelBundle(int idBundle)
        {
            //this.VerifyIsInTransaction("execute 'DelFolio' command");
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("idBundle", idBundle);
            using (var cmdDelFolio = this.databaseInstance.GetCommand(StoredProcedures.SpDelBundle))
            {
                var result = cmdDelFolio.ExecuteScalar(new ReadOnlyDictionary<string, object>(cmdParams));
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return -1;
                }
            }
        }


        public int InsBundle(int idCradle, int idBatch, int idItemStatus, int idUser)
        {
            object resultOut = new Object();
            int result = 0;
            Trace.Message("Execute  InsBundle to Cradle {0}", idCradle);
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("idCradle", idCradle);
            cmdParams.Add("idBatchHistory", idBatch);
            cmdParams.Add("idItemStatus", idItemStatus);
            cmdParams.Add("idUser", idUser);
            cmdParams.Add("IsManual", 1);
            cmdParams.Add("IsOpen", 0);            
            Tenaris.Library.Framework.ReadOnlyDictionary<string, object> parametersOut;
            using (var cmdInsItem = this.databaseInstance.GetCommand(StoredProcedures.SpInsbundle))                
            {
                try
                {
                    cmdInsItem.ExecuteNonQuery(new Tenaris.Library.Framework.ReadOnlyDictionary<string, object>(cmdParams), out parametersOut);
                    parametersOut.TryGetValue("id", out resultOut);
                    if (resultOut != null)
                    {
                        result = Convert.ToInt32(resultOut);
                    }
                    return result;

                }
                catch (Exception e)
                {
                    Trace.Error("Error Insert Bundle into idCradle {0} : {1}", idCradle, e.ToString());
                    return 0;
                }
            }
        }


        public int InsItem(int idBundle, int idTracking)
        {
            object resultOut = new Object();
            int result = 0;
            Trace.Message("Execute  InsItem {0} to Bundle {1}", idTracking, idBundle);
            Dictionary<string,object>  cmdParams = new Dictionary<string,object>();
            cmdParams.Add("idBundle", idBundle);
            cmdParams.Add("idTracking", idTracking);         
            Tenaris.Library.Framework.ReadOnlyDictionary<string, object> parametersOut;
            using (var cmdInsItem = this.databaseInstance.GetCommand(StoredProcedures.SpCreateTrackingOnBundle))
            {
                try
                {
                    cmdInsItem.ExecuteNonQuery(new Tenaris.Library.Framework.ReadOnlyDictionary<string, object>(cmdParams), out parametersOut);
                    parametersOut.TryGetValue("IdTrackingBundle", out resultOut);
                    if (resultOut != null)
                    {
                        result = Convert.ToInt32(resultOut);
                    }
                    return result;

                }
                catch (Exception e)
                {
                    Trace.Error("Error Insert Item {0} into idBundle {1} : {2}", idTracking, idBundle, e.ToString());                    
                    return 0;
                }
              }
        }

        public int UpdTrackingBundle(int idBundle, int idTracking, float weight)
        {
            object resultOut = new Object();
            int result = 0;
            Trace.Message("Execute UpdTrackingBundle {0} to Bundle {1}", idTracking, idBundle);
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("idBundleTarget", idBundle);
            cmdParams.Add("idTracking", idTracking);
            cmdParams.Add("Weight", weight);
            Tenaris.Library.Framework.ReadOnlyDictionary<string, object> parametersOut;
            using (var cmdInsItem = this.databaseInstance.GetCommand(StoredProcedures.SpUpdTrackingBundle))
            {
                try
                {
                    cmdInsItem.ExecuteNonQuery(new Tenaris.Library.Framework.ReadOnlyDictionary<string, object>(cmdParams), out parametersOut);
                    parametersOut.TryGetValue("id", out resultOut);
                    if (resultOut != null)
                    {
                        result = Convert.ToInt32(resultOut);
                    }
                    return result;

                }
                catch (Exception e)
                {
                    Trace.Error("Error Update TrackingBundle idTracking {0} into idBundle {1} : {2}", idTracking, idBundle, e.ToString());
                    return 0;
                }
            }
        }


        public bool UpdRejectBundle(int idBundle, string rejectionCode, int idLocation)
        {                   
            Trace.Message("Execute UpdRejectBundle Bundle {0}", idBundle);
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();
            cmdParams.Add("IdBundle", idBundle);
            cmdParams.Add("RejectionCode", rejectionCode);
            cmdParams.Add("IdLocation", idLocation);                      
            using (var cmdInsItem = this.databaseInstance.GetCommand(StoredProcedures.SpUpdRejectBundle))
            {
                try
                {
                    cmdInsItem.ExecuteNonQuery(new Tenaris.Library.Framework.ReadOnlyDictionary<string, object>(cmdParams));
                    return true;

                }
                catch (Exception e)
                {
                    Trace.Error("Error Rejecting Bundle {0} : {1}", idBundle, e.ToString());
                    return false;
                }
            }
        }

        public int DelTrackingBundle(int idTracking)
        {
            object resultOut = new Object();
            int result = 0;
            Trace.Message("Execute DelTrackingBundle {0}", idTracking);
            Dictionary<string, object> cmdParams = new Dictionary<string, object>();            
            cmdParams.Add("idTracking", idTracking);            
            Tenaris.Library.Framework.ReadOnlyDictionary<string, object> parametersOut;
            using (var cmdInsItem = this.databaseInstance.GetCommand(StoredProcedures.SpDelTrackingBundle))
            {
                try
                {
                    cmdInsItem.ExecuteNonQuery(new Tenaris.Library.Framework.ReadOnlyDictionary<string, object>(cmdParams), out parametersOut);
                    parametersOut.TryGetValue("id", out resultOut);
                    if (resultOut != null)
                    {
                        result = Convert.ToInt32(resultOut);
                    }
                    return result;

                }
                catch (Exception e)
                {
                    Trace.Error("Error Executing DelTrackingBundle idTracking {0}  : {1}", idTracking,  e.ToString());
                    return 0;
                }
            }
        }

        public ObservableCollection<GroupElaborationState> GetGroupElaborationState()
        {
            var objList = new ObservableCollection<GroupElaborationState>();

            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetGroupElaborationState);
                //var parameters = new Dictionary<string, object>();
                //parameters.Add("@Order", order);
                //dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                DataTable dtItems = command.ExecuteTable();
                for (int i = 0; i < dtItems.Rows.Count; i++)
                {
                    var group = new GroupElaborationState(
                        Convert.ToInt32(dtItems.Rows[i]["idGroupElaborationState"]),
                        dtItems.Rows[i]["Code"].ToString(),
                        dtItems.Rows[i]["Name"].ToString(),
                        dtItems.Rows[i]["Description"].ToString()
                        );
                    objList.Add(group);
                }
            }
            catch (Exception ex)
            {
                Trace.Message(ex.Message);
            }

            return objList;
        }

        public ObservableCollection<ElaborationState> GetElaborationStateByGroup(int IdGroupElaborationState)
        {
            var objList = new ObservableCollection<ElaborationState>();

            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetElaborationStateByGroup);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdGroupElaborationState", IdGroupElaborationState);
                DataTable dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                for (int i = 0; i < dtItems.Rows.Count; i++)
                {
                    var elaborationState = new ElaborationState(
                        Convert.ToInt32(dtItems.Rows[i]["IdElaborationState"]),
                        dtItems.Rows[i]["Code"].ToString(),
                        dtItems.Rows[i]["Name"].ToString(),
                        dtItems.Rows[i]["Description"].ToString()
                        );
                    objList.Add(elaborationState);
                }

            }
            catch (Exception ex)
            {
                Trace.Message(ex.Message);
            }

            return objList;
        }

        public ObservableCollection<RejectionCode> GetRejections(int idElaborationState)
        {
            var objList = new ObservableCollection<RejectionCode>();

            try
            {
                Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetRejectionCodeByElaborationState);
                var parameters = new Dictionary<string, object>();
                parameters.Add("@IdElaborationState", idElaborationState);
                DataTable dtItems = command.ExecuteTable(new ReadOnlyDictionary<string, object>(parameters));
                for (int i = 0; i < dtItems.Rows.Count; i++)
                {
                    var rejectionCode = new RejectionCode(
                        Convert.ToInt32(dtItems.Rows[i]["IdRejectionCode"]),
                        dtItems.Rows[i]["Code"].ToString(),
                        dtItems.Rows[i]["Name"].ToString(),
                        dtItems.Rows[i]["Description"].ToString()
                        );
                    objList.Add(rejectionCode);
                }
            }
            catch (Exception ex)
            {
                Trace.Message(ex.Message);
            }

            return objList;
        }


        #region Internal Methods

        /// <summary>
        /// Get Status's from DB
        /// </summary>
        /// <returns></returns>
        internal ReadOnlyDictionary<int, ItemStatus> GetStatuses()
        {
            DataTable dtResult;
            Dictionary<int, ItemStatus> status = new Dictionary<int, ItemStatus>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetStatuses);
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

        internal ReadOnlyDictionary<int, ILocation> GetDataLocations()
        {
            DataTable dtResult;
            Dictionary<int, ILocation> locations = new Dictionary<int, ILocation>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetLocations);
            dtResult = cmd.ExecuteTable();
            for (int x = 0; x < dtResult.Rows.Count; x++)
            {
                int idLocation = Convert.ToInt32(dtResult.Rows[x]["IdLocation"]);
                Location location = new Location(idLocation,
                    dtResult.Rows[x]["Code"].ToString(),
                    dtResult.Rows[x]["Location"].ToString(),
                    dtResult.Rows[x]["Description"].ToString(),
                    GetStatus(Convert.ToInt32(dtResult.Rows[x]["IdItemStatus"].ToString())),
                    dtResult.Rows[x]["ProductionType"].ToString(),
                    dtResult.Rows[x]["ElaborationStatus"].ToString()
                    );
                locations.Add(idLocation, location);
            }
            return new ReadOnlyDictionary<int, ILocation>(locations);
        }
            

        /// <summary>
        /// Get Shifts
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<int, string> GetShifts()
        {
            DataTable dtResult;
            Dictionary<int, string> shifts = new Dictionary<int, string>();

            Tenaris.Library.DbClient.IDbCommand cmd = databaseInstance.GetCommand(StoredProcedures.SpGetShifts);
            dtResult = cmd.ExecuteTable();
            for (int x = 0; x < dtResult.Rows.Count; x++)
            {
                int id = Convert.ToInt32(dtResult.Rows[x]["ShiftNumber"]);
                string code = dtResult.Rows[x]["Description"].ToString();
                shifts.Add(id, code);
            }
            return new ReadOnlyDictionary<int, string>(shifts);
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
        /// Get Quality Codes for Rejection Causes
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<RejectionCause> GetRejectionCauses()
        {
             DataTable dtRejectionCause;
             var rejectionCauses = new List<RejectionCause>();
             try
             {
                 Tenaris.Library.DbClient.IDbCommand command = databaseInstance.GetCommand(StoredProcedures.SpGetQualityCodes);
                 dtRejectionCause = command.ExecuteTable();
                 for (int x = 0; x < dtRejectionCause.Rows.Count; x++)
                 {
                     var i = new RejectionCause(
                      Convert.ToInt32(dtRejectionCause.Rows[x]["IdCode"]),
                      dtRejectionCause.Rows[x]["Code"].ToString(),
                      dtRejectionCause.Rows[x]["Name"].ToString(),
                      dtRejectionCause.Rows[x]["Description"].ToString(),
                      Convert.ToInt32(dtRejectionCause.Rows[x]["idItemStatus"]));
                     rejectionCauses.Add(i);
                 }
             }
             catch (Exception ex)
             {
                 Trace.Message("Error Get Rejection Causes from DB : {0}", ex.Message);
             }

             return rejectionCauses;     
        }

        public IEnumerable<ILocation> GetLocations()
        {
            return Locations.Values;
        }

        public ILocation GetLocation(int idLocation)
        {
            ILocation result;
            Locations.TryGetValue(idLocation, out result);
            return result;
        }


        internal ReadOnlyDictionary<int, string> Shifts
        {
            get
            {
                return shifts;
            }
        }
 

       


        #endregion

        
    }
}
