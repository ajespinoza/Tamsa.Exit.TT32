// -----------------------------------------------------------------------
// <copyright file="WebServiceITAdapter.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Tenaris.Library.Log;
    using Tamsa.Manager.Exit.Shared;
    using Tamsa.Manager.Exit.SendIT;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WebServiceITAdapter : IDisposable
    {


        private SendITProxy senderIT;

        #region Constructor
        public WebServiceITAdapter()
        {
            senderIT = new SendITProxy(ExitManager.Instance.Config.UrlWebService,ExitManager.Instance.Config.ITMachines, ExitManager.Instance.Config.ITMachineEE);
        }
         
        #endregion

        
        //
        public bool SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, string impresora, string groupElaborationState, string elaborationState, string location, string rejectionCode, out Dictionary<int, string> errors, out int BundleNumber)
        {
            ITBundle atado = new ITBundle();
            string xmlIT=string.Empty;
            errors = new Dictionary<int, string>();
            BundleNumber = 0;
            IBundle bundle = ExitManager.Instance.dataAccess.GetBundleData(idBundle);
            string isRequiredEEOut = ExitManager.Instance.Config.IsRequiredEE;
            if (bundle != null)
            {

                if (bundle.Status == BundleStatus.Sent)
                {
                    errors.Add(-2, "El atado ya ha sido enviado");
                    return false;
                }
                else
                {
                    int HeatNumber = 0;
                    int OrderNumber = 0;
                    int bundleNumber = 0;
                    string EE = "";
                    object value;
                    string sendingString = String.Empty;
                    string responseString = String.Empty;
                    bundle.ExtraData.TryGetValue("OrderNumber", out value);
                    if (value != null)
                    {
                        OrderNumber = Convert.ToInt32(value);
                    }
                    bundle.ExtraData.TryGetValue(ExitManager.Instance.Config.CodeHeat, out value);
                    if (value != null)
                    {
                        HeatNumber = Convert.ToInt32(value);
                    }
                    // Enviar al PIP
                    // si el result = true, obtener el numero de atado asignado           
                    sendingString = sendingString + " - " + OrderNumber.ToString() + HeatNumber.ToString();
                    
                    Trace.Debug("Obteniendo EE");
                    //EE DEL PRIMER TUBO DEL BUNDLE
                    bundle.ExtraData.TryGetValue("EE", out value);
                    if (value != null)
                    {
                        EE = value.ToString();
                        Trace.Debug("EE: {0}", EE);
                    }
                    //
                    if (bundle.Sent == 0)
                    {
                        atado = senderIT.SendBundleMES(PipUser, OrderNumber, HeatNumber, bundle, comments, rutaSEAS, idBundle, impresora, EE, groupElaborationState, elaborationState, location, rejectionCode, isRequiredEEOut, out errors, out xmlIT);
                        //AMMF VERIFICANDO QUE UNA VEZ CREADO EL ATADO YA NO SE MANDE SEAS
                        if(atado.Machine == 1 && atado.idBundle == 0)
                        {
                            atado = senderIT.SendBundleMSEAS(PipUser, OrderNumber, HeatNumber, bundle, comments, rutaSEAS, idBundle, impresora, EE, groupElaborationState, elaborationState, location, rejectionCode, isRequiredEEOut, out errors, out xmlIT);
                        }
                        
                    }
                    else
                    {
                        if(bundle.Sent == 1)
                        {
                            atado = senderIT.SendBundleMSEAS(PipUser, OrderNumber, HeatNumber, bundle, comments, rutaSEAS, idBundle, impresora, EE, groupElaborationState, elaborationState, location, rejectionCode, isRequiredEEOut, out errors, out xmlIT);
                        }
                    }
                    

                    // SE LE PASA EL PIPUSER
                    //bundleNumber = senderIT.SendBundle(PipUser, OrderNumber, HeatNumber, bundle, comments, rutaSEAS, idBundle, impresora, EE, out errors, out xmlIT);
                    

                    //atado = senderIT.SendBundleMES(PipUser, OrderNumber, HeatNumber, bundle, comments, rutaSEAS, idBundle, impresora, EE, out errors, out xmlIT);
                    

                    if (atado.idBundle > 0)
                    {
                        // Actualiza en la BD el resultado del envio
                        BundleNumber = atado.idBundle;
                        
                        ExitManager.Instance.dataAccess.InsSending(idBundle, BundleNumber, idUser, xmlIT, "Ok", 1, atado.Machine, DateTimeOffset.Now);
                        // Actualiza en memoria el estatus del atado
                        bundle.Send(BundleNumber);
                        return true;
                    }
                    else
                    {
                        foreach (KeyValuePair<int, string> error in errors)
                        {
                            responseString = responseString + " Codigo error : " + error.Key + " - Descripcion : " + error.Value;
                        }

                        ExitManager.Instance.dataAccess.InsSending(idBundle, 0, idUser, xmlIT, responseString, 0, atado.Machine, DateTimeOffset.Now);
                        return false;
                    }

                }
            }
            else return false;
        }

        //


        #region IDisposable Members

        public void Dispose()
        {
            Trace.Message("Terminando servicio");
            //if (WebService != null)
            //{
            //    WebService.Dispose();
            //}
        }

         #endregion

    }
}
