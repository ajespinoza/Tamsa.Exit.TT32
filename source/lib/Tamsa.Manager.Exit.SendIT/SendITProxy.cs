// -----------------------------------------------------------------------
// <copyright file="SendITProxy.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using Tenaris.Library.Log;    

    using Tamsa.Manager.Exit.Shared;
    using Tamsa.Manager.Exit.SendIT;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SendITProxy
    {
        
        #region Const        

        string SECURITYID = String.Empty;
        byte[] LANGUAGEID = { 4 };
        private const int ORDER_CHARSIZE = 9;
        private const int HEAT_CHARSIZE = 5;
        private const char PADDINGCHAR = '0';

        //ReportType es el tipo de reporte que quieres efectuar. Valores: 
        //P PRODUCCIóN 
        //B BAJADA 
        //F DESCARTE RFL 
        //N DESCARTE NREC 
        //T TRANSFERENCIAS
        //C CORTES 
        //E ESMERILES     
        private const string REPORTTYPE = "P";
        private const string REPORTTYPECUTTING = "C";
        private const string STOCKTYPE = "TU";
        #endregion

        #region Variables

        string ERRORTYPE = null;
        decimal ERRORNUMBER = 0;
        string ERRORDESCRIPTION = null;

        PIPFinishingExitApplication webService;
        PIN149FFINISHINGREPORT Report;
        PIN149FFINISHINGREPORTGENERALINFORMATION GeneralInformation;
        PIN149FFINISHINGREPORTMACHINESPRODUCTION MachineInformation;
        PIN149FFINISHINGREPORTPIPESPRODUCTION PipeInformation;
        PIN149FResponseDETAILEDTRANSACTIONSTATUS Response;
        PIN149FDETAILEDTRANSACTIONSTATUS TransactionStatus;

        private string[] ITMachines;
        private string[] ITMachineEE;

        #endregion

         #region Constructor

        public SendITProxy(string url, string itMachines, string itMachineEE)
        {
            Trace.Message("Conecting to Send Service");
            webService = new PIPFinishingExitApplication(url);
            Report = new PIN149FFINISHINGREPORT();
            this.ITMachines = itMachines.Split(',');
            this.ITMachineEE = itMachineEE.Split(',');
        }

        #endregion

        public int SendBundle(string PipUser, int OrderNumber, int HeatNumber, IBundle bundle, string comments, bool rutaSEAS, int idBundleL2, out Dictionary<int, string> errors, out string xmlIT)
        {
            xmlIT = null;
            int ITMachinesRute;
            Trace.Message("Loading PIN149F.... ");
            Trace.Message("idBundle L2: {0}", idBundleL2);
            errors = new Dictionary<int,string>();
            int result = 0;            
            #region Informacion General
            GeneralInformation = new PIN149FFINISHINGREPORTGENERALINFORMATION();
            
            GeneralInformation.TRANSACTIONNUMBER = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            xmlIT = "<urn:PIN149F><FINISHINGREPORT><GENERALINFORMATION><TRANSACTIONNUMBER>" + GeneralInformation.TRANSACTIONNUMBER + "</TRANSACTIONNUMBER>";

            string materialType;
            string side;
            string RejectedCode = String.Empty;
            object value;

            bundle.ExtraData.TryGetValue("MaterialType", out value);
            if (value != null)
                materialType = value.ToString();
            else
                materialType = "G";
            GeneralInformation.HANDLINGTYPE = materialType; // Detalle o Granel
            xmlIT = xmlIT + "<HANDLINGTYPE>" + GeneralInformation.HANDLINGTYPE + "</HANDLINGTYPE>";
            bundle.ExtraData.TryGetValue("Side", out value);
            if (value != null)
                side = value.ToString();
            else
                side = "A";

            RejectedCode = (bundle.RejectionCause!= null) ? bundle.RejectionCause : "";           

            GeneralInformation.STOCKTYPE = STOCKTYPE;
            if (bundle.ItemStatus == ItemStatus.Good)
                GeneralInformation.REPORTTYPE = REPORTTYPE;
            else
                GeneralInformation.REPORTTYPE = REPORTTYPECUTTING;
            GeneralInformation.PRODUCTIONORDER = OrderNumber.ToString().PadLeft(ORDER_CHARSIZE, PADDINGCHAR);
            GeneralInformation.HEATNUMBER = HeatNumber.ToString().PadLeft(HEAT_CHARSIZE, PADDINGCHAR);
            GeneralInformation.FACTORYLOT = "00000";
            GeneralInformation.LOCATION = "SE305";
            //GeneralInformation.LOCATION = (bundle.Location.Name != String.Empty) ? bundle.Location.Name : null;
            xmlIT = xmlIT + "<STOCKTYPE>" + GeneralInformation.STOCKTYPE + "</STOCKTYPE>" + "<REPORTTYPE>" + GeneralInformation.REPORTTYPE + "</REPORTTYPE>"
                    + "<PRODUCTIONORDER>" + GeneralInformation.PRODUCTIONORDER + "</PRODUCTIONORDER>" + "<HEATNUMBER>" + GeneralInformation.HEATNUMBER + "</HEATNUMBER>"
                    + "<FACTORYLOT>" + GeneralInformation.FACTORYLOT + "</FACTORYLOT>" + "<LOCATION>" + GeneralInformation.LOCATION + "</LOCATION>";
            GeneralInformation.MANUFACTURINGSTATUSIN = (side.Equals("A")) ? "69" : "79";
                //2
            GeneralInformation.MANUFACTURINGSTATUSOUT = (bundle.Location.ElaborationStatus != String.Empty) ? bundle.Location.ElaborationStatus : null;
            GeneralInformation.TOTALPIECES = bundle.Items.Count();
            xmlIT = xmlIT + "<MANUFACTURINGSTATUSIN>" + GeneralInformation.MANUFACTURINGSTATUSIN + "</MANUFACTURINGSTATUSIN>" + "<MANUFACTURINGSTATUSOUT>" + GeneralInformation.MANUFACTURINGSTATUSOUT + "</MANUFACTURINGSTATUSOUT>";
                //1
            GeneralInformation.PRINTTAG = "N";
                //8
            GeneralInformation.PRINTER = null;
                //70
            GeneralInformation.COMMENTS = comments;
                //1
            GeneralInformation.GENERATEDETAIL = "N";
                //1
            GeneralInformation.RENUMBERPIPES = "N";
                //5
            GeneralInformation.NEXTMACHINE = "N";

            xmlIT = xmlIT + "<TOTALPIECES>" + GeneralInformation.TOTALPIECES + "</TOTALPIECES>" + "<PRINTTAG>" + GeneralInformation.PRINTTAG + "</PRINTTAG>" + "<PRINTER>" + GeneralInformation.PRINTER + "</PRINTER>"
                    + "<COMMENTS>" + GeneralInformation.COMMENTS + "</COMMENTS>" + "<GENERATEDETAIL>" + GeneralInformation.GENERATEDETAIL + "</GENERATEDETAIL>"
                    + "<RENUMBERPIPES>" + GeneralInformation.RENUMBERPIPES + "</RENUMBERPIPES>" + "<NEXTMACHINE>" + GeneralInformation.NEXTMACHINE + "</NEXTMACHINE>"
                    + "</GENERALINFORMATION>";
            
            #endregion    
                    
        #region Machine Information

            MachineInformation = new PIN149FFINISHINGREPORTMACHINESPRODUCTION();
            if (rutaSEAS)
            {
                ITMachinesRute = ITMachines.Count();
                MachineInformation.MACHINE = new string[ITMachines.Count()];
            }
            else
            {
                //No tiene ruta por SEAS, solo se reporta una maquina
                ITMachinesRute = 1;
                MachineInformation.MACHINE = new string[1];
                
            }
            

            MachineInformation.PROCCESEDSIDE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.DATE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.HOUR = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.USERID = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.PRODUCTIONTYPE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.REJECTIONCAUSE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
        #endregion

        #region Pipes Production

        PipeInformation = new PIN149FFINISHINGREPORTPIPESPRODUCTION();
        PipeInformation.PIPENUMBER = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
        PipeInformation.FACTORYIDOUT = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
        PipeInformation.REPROCESSINGNUMBER = new decimal[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
        // Recorrer el atado e insertar los tubos.

        xmlIT = xmlIT + "<PIPESPRODUCTION>" + "<PIPENUMBER>";
        string auxFactory = null;
        string auxReprocessing = null;
        string auxMachine = null;
        string auxProccesSide = null;
        string auxDate = null;
        string auxHour = null;
        string auxUser = null;
        string auxProductionType = null;
        string auxRejectionCause = null;

        for (int i=0; i<bundle.Items.Count(); i++)
        {
            xmlIT = xmlIT + "<string>";
            MachineInformation.PROCCESEDSIDE[i] = new string[ITMachinesRute];
            MachineInformation.DATE[i] = new string[ITMachinesRute];
            MachineInformation.HOUR[i] = new string[ITMachinesRute];
            MachineInformation.USERID[i] = new string[ITMachinesRute];
            MachineInformation.PRODUCTIONTYPE[i] = new string[ITMachinesRute];
            MachineInformation.REJECTIONCAUSE[i] = new string[ITMachinesRute];
            auxProccesSide = auxProccesSide + "<string_1>";
            auxDate = auxDate + "<string_1>";
            auxHour = auxHour + "<string_1>";
            auxUser = auxUser + "<string_1>";
            auxProductionType = auxProductionType + "<string_1>";
            auxRejectionCause = auxRejectionCause + "<string_1>";

            Trace.Debug("REPORT PRODUCTION  USER {0} :ITEM  {1}, STATUS {2}", PipUser, bundle.Items.ElementAt(i).Number.ToString(), bundle.ItemStatus.ToString());
            for (int m = 0; m < ITMachinesRute; m++)
            {                
                MachineInformation.MACHINE[m] = ITMachines[m];
                auxMachine = auxMachine + "<string>" + MachineInformation.MACHINE[m] + "</string>";
                MachineInformation.PROCCESEDSIDE[i][m] = "";
                auxProccesSide = auxProccesSide + "<string>" + MachineInformation.PROCCESEDSIDE[i][m] + "</string>";
                MachineInformation.DATE[i][m] = System.DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                auxDate = auxDate + "<string>" + MachineInformation.DATE[i][m] + "</string>";
                MachineInformation.HOUR[i][m] = System.DateTimeOffset.Now.ToString("HHmmssff", CultureInfo.InvariantCulture);
                auxHour = auxHour + "<string>" + MachineInformation.HOUR[i][m] + "</string>";
                MachineInformation.USERID[i][m] = PipUser;
                auxUser = auxUser + "<string>" + MachineInformation.USERID[i][m] + "</string>";
                MachineInformation.PRODUCTIONTYPE[i][m] = "BUE";
                auxProductionType = auxProductionType + "<string>" + MachineInformation.PRODUCTIONTYPE[i][m] + "</string>";
                MachineInformation.REJECTIONCAUSE[i][m] = "";
                auxRejectionCause = auxRejectionCause + "<string>" + MachineInformation.REJECTIONCAUSE[i][m] + "</string>";
                if (RejectedCode != String.Empty && m == ITMachinesRute - 1)
                {                    
                    Trace.Debug("ITEM WITH REJECTION CODE {0]", RejectedCode);
                    MachineInformation.REJECTIONCAUSE[i][m] = RejectedCode;
                    MachineInformation.PRODUCTIONTYPE[i][m] = bundle.Location.ProductionType;
                }
                //Trace.Debug("MACHINE: {0}", MachineInformation.MACHINE[m]);
                //Trace.Debug("PRODUCTIONTYPE: {0}", MachineInformation.PRODUCTIONTYPE[i][m]);
                //Trace.Debug("REJECTIONCAUSE: {0}", MachineInformation.REJECTIONCAUSE[i][m]);
                //Trace.Debug("USERID: {0}", MachineInformation.USERID[i][m]);
                
            }
            auxProccesSide = auxProccesSide + "</string_1>";
            auxDate = auxDate + "</string_1>";
            auxHour = auxHour + "</string_1>";
            auxUser = auxUser + "</string_1>";
            auxProductionType = auxProductionType + "</string_1>";
            auxRejectionCause = auxRejectionCause + "</string_1>";

            PipeInformation.PIPENUMBER[i] = bundle.Items.ElementAt(i).Number.ToString();         
            PipeInformation.FACTORYIDOUT[i] = String.Concat("0/", GeneralInformation.PRODUCTIONORDER, " ", GeneralInformation.HEATNUMBER, " ",PipeInformation.PIPENUMBER[i]);
            PipeInformation.REPROCESSINGNUMBER[i] = 0;
            xmlIT = xmlIT + PipeInformation.PIPENUMBER[i] + "</string>";
            auxFactory = auxFactory + "<string>" + PipeInformation.FACTORYIDOUT[i] + "</string>";
            auxReprocessing = auxReprocessing + "<decimal>" + PipeInformation.REPROCESSINGNUMBER[i] + "</decimal>";
        }
        xmlIT = xmlIT + "</PIPENUMBER>" + "<FACTORYIDOUT>" + auxFactory + "</FACTORYIDOUT>" + "<REPROCESSINGNUMBER>" + auxReprocessing + "</REPROCESSINGNUMBER>"
                + "</PIPESPRODUCTION>" + "<MACHINESPRODUCTION>" + "<MACHINE>" + auxMachine + "</MACHINE>" + "<PROCCESEDSIDE>" + auxProccesSide + "</PROCCESEDSIDE>"
                + "<DATE>" + auxDate + "</DATE>" + "<HOUR>" + auxHour + "</HOUR>" + "<USERID>" + auxUser + "</USERID>" + "<PRODUCTIONTYPE>" + auxProductionType
                + "</PRODUCTIONTYPE>" + "<REJECTIONCAUSE>" + auxRejectionCause + "</REJECTIONCAUSE>" + "</MACHINESPRODUCTION>" + "</FINISHINGREPORT>";

        

        #endregion
        Report.GENERALINFORMATION = GeneralInformation;
        Report.MACHINESPRODUCTION = MachineInformation;
        Report.PIPESPRODUCTION = PipeInformation;

        TransactionStatus = new PIN149FDETAILEDTRANSACTIONSTATUS();

        Trace.Debug("INICIANDO ENVIO");
        Trace.Debug("COMMENTS: {0}", Report.GENERALINFORMATION.COMMENTS);
        Trace.Debug("FACTORYLOT: {0}", Report.GENERALINFORMATION.FACTORYLOT);
        Trace.Debug("GENERATEDETAIL: {0}", Report.GENERALINFORMATION.GENERATEDETAIL);
        Trace.Debug("HANDLINGTYPE: {0}", Report.GENERALINFORMATION.HANDLINGTYPE);
        Trace.Debug("HEATNUMBER: {0}", Report.GENERALINFORMATION.HEATNUMBER);
        Trace.Debug("LOCATION: {0}", Report.GENERALINFORMATION.LOCATION);
        Trace.Debug("MANUFACTURINGSTATUSIN: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSIN);
        Trace.Debug("MANUFACTURINGSTATUSOUT: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSOUT);
        Trace.Debug("NEXTMACHINE: {0}", Report.GENERALINFORMATION.NEXTMACHINE);
        Trace.Debug("PRINTER: {0}", Report.GENERALINFORMATION.PRINTER);
        Trace.Debug("PRINTTAG: {0}", Report.GENERALINFORMATION.PRINTTAG);
        Trace.Debug("PRODUCTIONORDER: {0}", Report.GENERALINFORMATION.PRODUCTIONORDER);
        Trace.Debug("RENUMBERPIPES: {0}", Report.GENERALINFORMATION.RENUMBERPIPES);
        Trace.Debug("REPORTTYPE: {0}", Report.GENERALINFORMATION.REPORTTYPE);
        Trace.Debug("STOCKTYPE: {0}", Report.GENERALINFORMATION.STOCKTYPE);
        Trace.Debug("TOTALPIECES: {0}", Report.GENERALINFORMATION.TOTALPIECES.ToString());
        Trace.Debug("TRANSACTIONNUMBER: {0}", Report.GENERALINFORMATION.TRANSACTIONNUMBER);

            if (rutaSEAS)
            {
                Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
                Trace.Debug("MACHINE[1]: {0}", Report.MACHINESPRODUCTION.MACHINE[1]);
                Trace.Debug("MACHINE[2]: {0}", Report.MACHINESPRODUCTION.MACHINE[2]);
            }
            else
            {
                Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
            }
        
        
        
        for (int i = 0; i < bundle.Items.Count(); i++)
        {
            for (int m = 0; m < ITMachinesRute; m++)
            {
                Trace.Debug("MACHINE: [{0}], PRODUCTIONTYPE: {1}, REJECTIONCAUSE: {2}, USERID: {3}", Report.MACHINESPRODUCTION.MACHINE[m], Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][m], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][m], Report.MACHINESPRODUCTION.USERID[i][m]);
            }
                //Trace.Debug("PRODUCTIONTYPE MACHINE1: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][1], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][1], Report.MACHINESPRODUCTION.USERID[i][1]);
                //Trace.Debug("PRODUCTIONTYPE MACHINE2: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][2], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][2], Report.MACHINESPRODUCTION.USERID[i][2]);
                Trace.Debug("FACTORYIDOUT: {0}, PIPENUMBER: {1}, REPROCESSINGNUMBER: {2}", Report.PIPESPRODUCTION.FACTORYIDOUT[i], Report.PIPESPRODUCTION.PIPENUMBER[i], Report.PIPESPRODUCTION.REPROCESSINGNUMBER[i]);
        }

        ERRORTYPE = "";
        ERRORNUMBER = 0;
        ERRORDESCRIPTION = "";

        Trace.Debug("XML con long. de : {0}" , xmlIT.Length);
        Trace.Debug("Sending bundle to IT...");
        try
        {
            Response = webService.PIN149F(Report, TransactionStatus, SECURITYID, LANGUAGEID, ref ERRORTYPE, ref ERRORNUMBER, ref ERRORDESCRIPTION);
        }
            catch(Exception ex)
        {
            Trace.Error("The bundle was not sent", ex.Message);
        }
         
       
            if (Convert.ToInt32(ERRORNUMBER) > 0)
        {
            errors.Add(Convert.ToInt32(ERRORNUMBER), ERRORDESCRIPTION);
            Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
        }
        if (Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED > 0)
        {
            result = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
            Trace.Debug("Created Bundle {0} PIP Transaction {1}", result, Response.BUNDLEANDTRANSACTIONSCREATED.PIPTRANSACTION.ToString());
        }
        else
            if (Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count() > 0)
            {
                for (int e = 0; e < Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count(); e++)
                {
                    Trace.Error(" Response PIN149F Error Number {0} : {1}", Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER[e], Response.DESCRIPTIVEERROR.DETAILEDERRORDESCRIPTION[e]);
                }
            }
        return result;

                
        }

        //
        public int SendBundle(string PipUser, int OrderNumber, int HeatNumber, IBundle bundle, string comments, bool rutaSEAS, int idBundleL2, string impresora, string EE, out Dictionary<int, string> errors, out string xmlIT)
        {
            xmlIT = null;
            int ITMachinesRute;
            Trace.Message("Loading PIN149F.... ");
            Trace.Message("idBundle L2: {0}", idBundleL2);
            Trace.Debug("Canasta: {0}", bundle.IdCradle);
            errors = new Dictionary<int, string>();
            int result = 0;
            #region Informacion General
            GeneralInformation = new PIN149FFINISHINGREPORTGENERALINFORMATION();

            GeneralInformation.TRANSACTIONNUMBER = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            xmlIT = "<urn:PIN149F><FINISHINGREPORT><GENERALINFORMATION><TRANSACTIONNUMBER>" + GeneralInformation.TRANSACTIONNUMBER + "</TRANSACTIONNUMBER>";

            string materialType;
            string side;
            string RejectedCode = String.Empty;
            object value;

            bundle.ExtraData.TryGetValue("MaterialType", out value);
            if (value != null)
                materialType = value.ToString();
            else
                materialType = "G";
            GeneralInformation.HANDLINGTYPE = materialType; // Detalle o Granel
            xmlIT = xmlIT + "<HANDLINGTYPE>" + GeneralInformation.HANDLINGTYPE + "</HANDLINGTYPE>";
            bundle.ExtraData.TryGetValue("Side", out value);
            if (value != null)
                side = value.ToString();
            else
                side = "A";

            RejectedCode = (bundle.RejectionCause != null) ? bundle.RejectionCause : "";

            GeneralInformation.STOCKTYPE = STOCKTYPE;
            if (bundle.ItemStatus == ItemStatus.Good)
                GeneralInformation.REPORTTYPE = REPORTTYPE;
            else
                GeneralInformation.REPORTTYPE = REPORTTYPECUTTING;
            GeneralInformation.PRODUCTIONORDER = OrderNumber.ToString().PadLeft(ORDER_CHARSIZE, PADDINGCHAR);
            GeneralInformation.HEATNUMBER = HeatNumber.ToString().PadLeft(HEAT_CHARSIZE, PADDINGCHAR);
            GeneralInformation.FACTORYLOT = "00000";

            Trace.Debug("ITLocation: {0}", bundle.ITLocation);
            GeneralInformation.LOCATION = bundle.ITLocation;
                        
            //GeneralInformation.LOCATION = (bundle.Location.Name != String.Empty) ? bundle.Location.Name : null;
            xmlIT = xmlIT + "<STOCKTYPE>" + GeneralInformation.STOCKTYPE + "</STOCKTYPE>" + "<REPORTTYPE>" + GeneralInformation.REPORTTYPE + "</REPORTTYPE>"
                    + "<PRODUCTIONORDER>" + GeneralInformation.PRODUCTIONORDER + "</PRODUCTIONORDER>" + "<HEATNUMBER>" + GeneralInformation.HEATNUMBER + "</HEATNUMBER>"
                    + "<FACTORYLOT>" + GeneralInformation.FACTORYLOT + "</FACTORYLOT>" + "<LOCATION>" + GeneralInformation.LOCATION + "</LOCATION>";

            //CAMBIO DE FER PASANDOLE A IT EL EE
            GeneralInformation.MANUFACTURINGSTATUSIN = EE;
            //GeneralInformation.MANUFACTURINGSTATUSIN = (side.Equals("A")) ? "69" : "79";
            //2
            GeneralInformation.MANUFACTURINGSTATUSOUT = (bundle.Location.ElaborationStatus != String.Empty) ? bundle.Location.ElaborationStatus : null;
            GeneralInformation.TOTALPIECES = bundle.Items.Count();
            xmlIT = xmlIT + "<MANUFACTURINGSTATUSIN>" + GeneralInformation.MANUFACTURINGSTATUSIN + "</MANUFACTURINGSTATUSIN>" + "<MANUFACTURINGSTATUSOUT>" + GeneralInformation.MANUFACTURINGSTATUSOUT + "</MANUFACTURINGSTATUSOUT>";
            //1
            GeneralInformation.PRINTTAG = "N";
            //8
            GeneralInformation.PRINTER = impresora;
            //70
            GeneralInformation.COMMENTS = comments;
            //1
            GeneralInformation.GENERATEDETAIL = "N";
            //1
            GeneralInformation.RENUMBERPIPES = "N";
            //5
            GeneralInformation.NEXTMACHINE = "N";

            xmlIT = xmlIT + "<TOTALPIECES>" + GeneralInformation.TOTALPIECES + "</TOTALPIECES>" + "<PRINTTAG>" + GeneralInformation.PRINTTAG + "</PRINTTAG>" + "<PRINTER>" + GeneralInformation.PRINTER + "</PRINTER>"
                    + "<COMMENTS>" + GeneralInformation.COMMENTS + "</COMMENTS>" + "<GENERATEDETAIL>" + GeneralInformation.GENERATEDETAIL + "</GENERATEDETAIL>"
                    + "<RENUMBERPIPES>" + GeneralInformation.RENUMBERPIPES + "</RENUMBERPIPES>" + "<NEXTMACHINE>" + GeneralInformation.NEXTMACHINE + "</NEXTMACHINE>"
                    + "</GENERALINFORMATION>";

            #endregion

            #region Machine Information

            MachineInformation = new PIN149FFINISHINGREPORTMACHINESPRODUCTION();
            //if (rutaSEAS)
            //{
            //    ITMachinesRute = ITMachines.Count();
            //    MachineInformation.MACHINE = new string[ITMachines.Count()];
            //}
            //else
            //{
                //No tiene ruta por SEAS, solo se reporta una maquina
            ITMachinesRute = ITMachineEE.Count();
                MachineInformation.MACHINE = new string[1];

            //}


            MachineInformation.PROCCESEDSIDE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.DATE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.HOUR = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.USERID = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.PRODUCTIONTYPE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.REJECTIONCAUSE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            #endregion

            #region Pipes Production

            PipeInformation = new PIN149FFINISHINGREPORTPIPESPRODUCTION();
            PipeInformation.PIPENUMBER = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.FACTORYIDOUT = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.REPROCESSINGNUMBER = new decimal[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            // Recorrer el atado e insertar los tubos.

            xmlIT = xmlIT + "<PIPESPRODUCTION>" + "<PIPENUMBER>";
            string auxFactory = null;
            string auxReprocessing = null;
            string auxMachine = null;
            string auxProccesSide = null;
            string auxDate = null;
            string auxHour = null;
            string auxUser = null;
            string auxProductionType = null;
            string auxRejectionCause = null;
            int auxError = 0;

            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                xmlIT = xmlIT + "<string>";
                MachineInformation.PROCCESEDSIDE[i] = new string[ITMachinesRute];
                MachineInformation.DATE[i] = new string[ITMachinesRute];
                MachineInformation.HOUR[i] = new string[ITMachinesRute];
                MachineInformation.USERID[i] = new string[ITMachinesRute];
                MachineInformation.PRODUCTIONTYPE[i] = new string[ITMachinesRute];
                MachineInformation.REJECTIONCAUSE[i] = new string[ITMachinesRute];
                auxProccesSide = auxProccesSide + "<string_1>";
                auxDate = auxDate + "<string_1>";
                auxHour = auxHour + "<string_1>";
                auxUser = auxUser + "<string_1>";
                auxProductionType = auxProductionType + "<string_1>";
                auxRejectionCause = auxRejectionCause + "<string_1>";

                Trace.Debug("REPORT PRODUCTION  USER {0} :ITEM  {1}, STATUS {2}", PipUser, bundle.Items.ElementAt(i).Number.ToString(), bundle.ItemStatus.ToString());
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    MachineInformation.MACHINE[m] = ITMachineEE[m];
                    auxMachine = auxMachine + "<string>" + MachineInformation.MACHINE[m] + "</string>";
                    MachineInformation.PROCCESEDSIDE[i][m] = "";
                    auxProccesSide = auxProccesSide + "<string>" + MachineInformation.PROCCESEDSIDE[i][m] + "</string>";
                    MachineInformation.DATE[i][m] = System.DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    auxDate = auxDate + "<string>" + MachineInformation.DATE[i][m] + "</string>";
                    MachineInformation.HOUR[i][m] = System.DateTimeOffset.Now.ToString("HHmmssff", CultureInfo.InvariantCulture);
                    auxHour = auxHour + "<string>" + MachineInformation.HOUR[i][m] + "</string>";
                    MachineInformation.USERID[i][m] = PipUser;
                    auxUser = auxUser + "<string>" + MachineInformation.USERID[i][m] + "</string>";
                    MachineInformation.PRODUCTIONTYPE[i][m] = "BUE";
                    auxProductionType = auxProductionType + "<string>" + MachineInformation.PRODUCTIONTYPE[i][m] + "</string>";
                    MachineInformation.REJECTIONCAUSE[i][m] = "";
                    auxRejectionCause = auxRejectionCause + "<string>" + MachineInformation.REJECTIONCAUSE[i][m] + "</string>";
                    if (RejectedCode != String.Empty && m == ITMachinesRute - 1)
                    {
                        Trace.Debug("ITEM WITH REJECTION CODE {0]", RejectedCode);
                        MachineInformation.REJECTIONCAUSE[i][m] = RejectedCode;
                        MachineInformation.PRODUCTIONTYPE[i][m] = bundle.Location.ProductionType;
                    }
                    //Trace.Debug("MACHINE: {0}", MachineInformation.MACHINE[m]);
                    //Trace.Debug("PRODUCTIONTYPE: {0}", MachineInformation.PRODUCTIONTYPE[i][m]);
                    //Trace.Debug("REJECTIONCAUSE: {0}", MachineInformation.REJECTIONCAUSE[i][m]);
                    //Trace.Debug("USERID: {0}", MachineInformation.USERID[i][m]);

                }
                auxProccesSide = auxProccesSide + "</string_1>";
                auxDate = auxDate + "</string_1>";
                auxHour = auxHour + "</string_1>";
                auxUser = auxUser + "</string_1>";
                auxProductionType = auxProductionType + "</string_1>";
                auxRejectionCause = auxRejectionCause + "</string_1>";

                PipeInformation.PIPENUMBER[i] = bundle.Items.ElementAt(i).Number.ToString();
                PipeInformation.FACTORYIDOUT[i] = String.Concat("0/", GeneralInformation.PRODUCTIONORDER, " ", GeneralInformation.HEATNUMBER, " ", PipeInformation.PIPENUMBER[i]);
                PipeInformation.REPROCESSINGNUMBER[i] = 0;
                xmlIT = xmlIT + PipeInformation.PIPENUMBER[i] + "</string>";
                auxFactory = auxFactory + "<string>" + PipeInformation.FACTORYIDOUT[i] + "</string>";
                auxReprocessing = auxReprocessing + "<decimal>" + PipeInformation.REPROCESSINGNUMBER[i] + "</decimal>";
            }
            xmlIT = xmlIT + "</PIPENUMBER>" + "<FACTORYIDOUT>" + auxFactory + "</FACTORYIDOUT>" + "<REPROCESSINGNUMBER>" + auxReprocessing + "</REPROCESSINGNUMBER>"
                    + "</PIPESPRODUCTION>" + "<MACHINESPRODUCTION>" + "<MACHINE>" + auxMachine + "</MACHINE>" + "<PROCCESEDSIDE>" + auxProccesSide + "</PROCCESEDSIDE>"
                    + "<DATE>" + auxDate + "</DATE>" + "<HOUR>" + auxHour + "</HOUR>" + "<USERID>" + auxUser + "</USERID>" + "<PRODUCTIONTYPE>" + auxProductionType
                    + "</PRODUCTIONTYPE>" + "<REJECTIONCAUSE>" + auxRejectionCause + "</REJECTIONCAUSE>" + "</MACHINESPRODUCTION>" + "</FINISHINGREPORT>";



            #endregion
            Report.GENERALINFORMATION = GeneralInformation;
            Report.MACHINESPRODUCTION = MachineInformation;
            Report.PIPESPRODUCTION = PipeInformation;

            TransactionStatus = new PIN149FDETAILEDTRANSACTIONSTATUS();

            Trace.Debug("INICIANDO ENVIO");
            Trace.Debug("COMMENTS: {0}", Report.GENERALINFORMATION.COMMENTS);
            Trace.Debug("FACTORYLOT: {0}", Report.GENERALINFORMATION.FACTORYLOT);
            Trace.Debug("GENERATEDETAIL: {0}", Report.GENERALINFORMATION.GENERATEDETAIL);
            Trace.Debug("HANDLINGTYPE: {0}", Report.GENERALINFORMATION.HANDLINGTYPE);
            Trace.Debug("HEATNUMBER: {0}", Report.GENERALINFORMATION.HEATNUMBER);
            Trace.Debug("LOCATION: {0}", Report.GENERALINFORMATION.LOCATION);
            Trace.Debug("MANUFACTURINGSTATUSIN: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSIN);
            Trace.Debug("MANUFACTURINGSTATUSOUT: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSOUT);
            Trace.Debug("NEXTMACHINE: {0}", Report.GENERALINFORMATION.NEXTMACHINE);
            Trace.Debug("PRINTER: {0}", Report.GENERALINFORMATION.PRINTER);
            Trace.Debug("PRINTTAG: {0}", Report.GENERALINFORMATION.PRINTTAG);
            Trace.Debug("PRODUCTIONORDER: {0}", Report.GENERALINFORMATION.PRODUCTIONORDER);
            Trace.Debug("RENUMBERPIPES: {0}", Report.GENERALINFORMATION.RENUMBERPIPES);
            Trace.Debug("REPORTTYPE: {0}", Report.GENERALINFORMATION.REPORTTYPE);
            Trace.Debug("STOCKTYPE: {0}", Report.GENERALINFORMATION.STOCKTYPE);
            Trace.Debug("TOTALPIECES: {0}", Report.GENERALINFORMATION.TOTALPIECES.ToString());
            Trace.Debug("TRANSACTIONNUMBER: {0}", Report.GENERALINFORMATION.TRANSACTIONNUMBER);

            //if (rutaSEAS)
            //{
            //    Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
            //    Trace.Debug("MACHINE[1]: {0}", Report.MACHINESPRODUCTION.MACHINE[1]);
            //    Trace.Debug("MACHINE[2]: {0}", Report.MACHINESPRODUCTION.MACHINE[2]);
            //}
            //else
            //{
                Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
            //}



            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    Trace.Debug("MACHINE: [{0}], PRODUCTIONTYPE: {1}, REJECTIONCAUSE: {2}, USERID: {3}", Report.MACHINESPRODUCTION.MACHINE[m], Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][m], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][m], Report.MACHINESPRODUCTION.USERID[i][m]);
                }
                //Trace.Debug("PRODUCTIONTYPE MACHINE1: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][1], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][1], Report.MACHINESPRODUCTION.USERID[i][1]);
                //Trace.Debug("PRODUCTIONTYPE MACHINE2: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][2], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][2], Report.MACHINESPRODUCTION.USERID[i][2]);
                Trace.Debug("FACTORYIDOUT: {0}, PIPENUMBER: {1}, REPROCESSINGNUMBER: {2}", Report.PIPESPRODUCTION.FACTORYIDOUT[i], Report.PIPESPRODUCTION.PIPENUMBER[i], Report.PIPESPRODUCTION.REPROCESSINGNUMBER[i]);
            }

            ERRORTYPE = "";
            ERRORNUMBER = 0;
            ERRORDESCRIPTION = "";

            Trace.Debug("XML con long. de : {0}", xmlIT.Length);
            Trace.Debug("Sending bundle to IT...");
            try
            {
                Response = webService.PIN149F(Report, TransactionStatus, SECURITYID, LANGUAGEID, ref ERRORTYPE, ref ERRORNUMBER, ref ERRORDESCRIPTION);
            }
            catch (Exception ex)
            {
                Trace.Error("The bundle was not sent", ex.Message);
            }

            Trace.Debug("RutaSeas: {0}", rutaSEAS);
            //Mod sin EE seas
            if (rutaSEAS == true && Convert.ToInt32(ERRORNUMBER) == 0)
            {
                SendBundleSEAS(PipUser, OrderNumber, HeatNumber, bundle, comments, idBundleL2, impresora, out errors, out xmlIT);
                auxError = 1;
            }
            //

            if (Convert.ToInt32(ERRORNUMBER) > 0 && auxError == 0)
            {
                Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
                errors.Add(Convert.ToInt32(ERRORNUMBER), ERRORDESCRIPTION);
                //Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
            }
            if (Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED > 0)
            {
                
                result = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
                Trace.Debug("Created Bundle {0} PIP Transaction {1}", result, Response.BUNDLEANDTRANSACTIONSCREATED.PIPTRANSACTION.ToString());
            }
            else
                if (Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count() > 0)
                {
                    for (int e = 0; e < Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count(); e++)
                    {
                        Trace.Error(" Response PIN149F Error Number {0} : {1}", Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER[e], Response.DESCRIPTIVEERROR.DETAILEDERRORDESCRIPTION[e]);
                    }
                }
            return result;


        }

        //

        public int SendBundleSEAS(string PipUser, int OrderNumber, int HeatNumber, IBundle bundle, string comments, int idBundleL2, string impresora, out Dictionary<int, string> errors, out string xmlIT)
        {
            xmlIT = null;
            int ITMachinesRute;
            Trace.Message("Loading PIN149F.... ");
            Trace.Message("idBundle L2: {0}", idBundleL2);
            errors = new Dictionary<int, string>();
            int result = 0;
            #region Informacion General
            GeneralInformation = new PIN149FFINISHINGREPORTGENERALINFORMATION();

            GeneralInformation.TRANSACTIONNUMBER = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            xmlIT = "<urn:PIN149F><FINISHINGREPORT><GENERALINFORMATION><TRANSACTIONNUMBER>" + GeneralInformation.TRANSACTIONNUMBER + "</TRANSACTIONNUMBER>";

            string materialType;
            string side;
            string RejectedCode = String.Empty;
            object value;

            bundle.ExtraData.TryGetValue("MaterialType", out value);
            if (value != null)
                materialType = value.ToString();
            else
                materialType = "G";
            GeneralInformation.HANDLINGTYPE = materialType; // Detalle o Granel
            xmlIT = xmlIT + "<HANDLINGTYPE>" + GeneralInformation.HANDLINGTYPE + "</HANDLINGTYPE>";
            bundle.ExtraData.TryGetValue("Side", out value);
            if (value != null)
                side = value.ToString();
            else
                side = "A";

            RejectedCode = (bundle.RejectionCause != null) ? bundle.RejectionCause : "";

            GeneralInformation.STOCKTYPE = STOCKTYPE;
            if (bundle.ItemStatus == ItemStatus.Good)
                GeneralInformation.REPORTTYPE = REPORTTYPE;
            else
                GeneralInformation.REPORTTYPE = REPORTTYPECUTTING;
            GeneralInformation.PRODUCTIONORDER = OrderNumber.ToString().PadLeft(ORDER_CHARSIZE, PADDINGCHAR);
            GeneralInformation.HEATNUMBER = HeatNumber.ToString().PadLeft(HEAT_CHARSIZE, PADDINGCHAR);
            GeneralInformation.FACTORYLOT = "00000";

            Trace.Debug("ITLocation: {0}", bundle.ITLocation);
            GeneralInformation.LOCATION = bundle.ITLocation;

            //GeneralInformation.LOCATION = "SE305";
            //GeneralInformation.LOCATION = (bundle.Location.Name != String.Empty) ? bundle.Location.Name : null;
            xmlIT = xmlIT + "<STOCKTYPE>" + GeneralInformation.STOCKTYPE + "</STOCKTYPE>" + "<REPORTTYPE>" + GeneralInformation.REPORTTYPE + "</REPORTTYPE>"
                    + "<PRODUCTIONORDER>" + GeneralInformation.PRODUCTIONORDER + "</PRODUCTIONORDER>" + "<HEATNUMBER>" + GeneralInformation.HEATNUMBER + "</HEATNUMBER>"
                    + "<FACTORYLOT>" + GeneralInformation.FACTORYLOT + "</FACTORYLOT>" + "<LOCATION>" + GeneralInformation.LOCATION + "</LOCATION>";

            //CAMBIO DE FER PASANDOLE A IT EL EE
            GeneralInformation.MANUFACTURINGSTATUSIN = "";
            //GeneralInformation.MANUFACTURINGSTATUSIN = (side.Equals("A")) ? "69" : "79";
            //2
            GeneralInformation.MANUFACTURINGSTATUSOUT = (bundle.Location.ElaborationStatus != String.Empty) ? bundle.Location.ElaborationStatus : null;
            GeneralInformation.TOTALPIECES = bundle.Items.Count();
            xmlIT = xmlIT + "<MANUFACTURINGSTATUSIN>" + GeneralInformation.MANUFACTURINGSTATUSIN + "</MANUFACTURINGSTATUSIN>" + "<MANUFACTURINGSTATUSOUT>" + GeneralInformation.MANUFACTURINGSTATUSOUT + "</MANUFACTURINGSTATUSOUT>";
            //1
            GeneralInformation.PRINTTAG = "S";
            //8
            GeneralInformation.PRINTER = impresora;
            //70
            GeneralInformation.COMMENTS = comments;
            //1
            GeneralInformation.GENERATEDETAIL = "N";
            //1
            GeneralInformation.RENUMBERPIPES = "N";
            //5
            GeneralInformation.NEXTMACHINE = "N";

            xmlIT = xmlIT + "<TOTALPIECES>" + GeneralInformation.TOTALPIECES + "</TOTALPIECES>" + "<PRINTTAG>" + GeneralInformation.PRINTTAG + "</PRINTTAG>" + "<PRINTER>" + GeneralInformation.PRINTER + "</PRINTER>"
                    + "<COMMENTS>" + GeneralInformation.COMMENTS + "</COMMENTS>" + "<GENERATEDETAIL>" + GeneralInformation.GENERATEDETAIL + "</GENERATEDETAIL>"
                    + "<RENUMBERPIPES>" + GeneralInformation.RENUMBERPIPES + "</RENUMBERPIPES>" + "<NEXTMACHINE>" + GeneralInformation.NEXTMACHINE + "</NEXTMACHINE>"
                    + "</GENERALINFORMATION>";

            #endregion

            #region Machine Information

            MachineInformation = new PIN149FFINISHINGREPORTMACHINESPRODUCTION();
            
            
                ITMachinesRute = ITMachines.Count();
                MachineInformation.MACHINE = new string[ITMachines.Count()];
            
            

            MachineInformation.PROCCESEDSIDE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.DATE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.HOUR = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.USERID = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.PRODUCTIONTYPE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.REJECTIONCAUSE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            #endregion

            #region Pipes Production

            PipeInformation = new PIN149FFINISHINGREPORTPIPESPRODUCTION();
            PipeInformation.PIPENUMBER = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.FACTORYIDOUT = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.REPROCESSINGNUMBER = new decimal[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            // Recorrer el atado e insertar los tubos.

            xmlIT = xmlIT + "<PIPESPRODUCTION>" + "<PIPENUMBER>";
            string auxFactory = null;
            string auxReprocessing = null;
            string auxMachine = null;
            string auxProccesSide = null;
            string auxDate = null;
            string auxHour = null;
            string auxUser = null;
            string auxProductionType = null;
            string auxRejectionCause = null;

            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                xmlIT = xmlIT + "<string>";
                MachineInformation.PROCCESEDSIDE[i] = new string[ITMachinesRute];
                MachineInformation.DATE[i] = new string[ITMachinesRute];
                MachineInformation.HOUR[i] = new string[ITMachinesRute];
                MachineInformation.USERID[i] = new string[ITMachinesRute];
                MachineInformation.PRODUCTIONTYPE[i] = new string[ITMachinesRute];
                MachineInformation.REJECTIONCAUSE[i] = new string[ITMachinesRute];
                auxProccesSide = auxProccesSide + "<string_1>";
                auxDate = auxDate + "<string_1>";
                auxHour = auxHour + "<string_1>";
                auxUser = auxUser + "<string_1>";
                auxProductionType = auxProductionType + "<string_1>";
                auxRejectionCause = auxRejectionCause + "<string_1>";

                Trace.Debug("REPORT PRODUCTION  USER {0} :ITEM  {1}, STATUS {2}", PipUser, bundle.Items.ElementAt(i).Number.ToString(), bundle.ItemStatus.ToString());
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    MachineInformation.MACHINE[m] = ITMachines[m];
                    auxMachine = auxMachine + "<string>" + MachineInformation.MACHINE[m] + "</string>";
                    MachineInformation.PROCCESEDSIDE[i][m] = "";
                    auxProccesSide = auxProccesSide + "<string>" + MachineInformation.PROCCESEDSIDE[i][m] + "</string>";
                    MachineInformation.DATE[i][m] = System.DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    auxDate = auxDate + "<string>" + MachineInformation.DATE[i][m] + "</string>";
                    MachineInformation.HOUR[i][m] = System.DateTimeOffset.Now.ToString("HHmmssff", CultureInfo.InvariantCulture);
                    auxHour = auxHour + "<string>" + MachineInformation.HOUR[i][m] + "</string>";
                    MachineInformation.USERID[i][m] = PipUser;
                    auxUser = auxUser + "<string>" + MachineInformation.USERID[i][m] + "</string>";
                    MachineInformation.PRODUCTIONTYPE[i][m] = "BUE";
                    auxProductionType = auxProductionType + "<string>" + MachineInformation.PRODUCTIONTYPE[i][m] + "</string>";
                    MachineInformation.REJECTIONCAUSE[i][m] = "";
                    auxRejectionCause = auxRejectionCause + "<string>" + MachineInformation.REJECTIONCAUSE[i][m] + "</string>";
                    if (RejectedCode != String.Empty && m == ITMachinesRute - 1)
                    {
                        Trace.Debug("ITEM WITH REJECTION CODE {0]", RejectedCode);
                        MachineInformation.REJECTIONCAUSE[i][m] = RejectedCode;
                        MachineInformation.PRODUCTIONTYPE[i][m] = bundle.Location.ProductionType;
                    }
                    //Trace.Debug("MACHINE: {0}", MachineInformation.MACHINE[m]);
                    //Trace.Debug("PRODUCTIONTYPE: {0}", MachineInformation.PRODUCTIONTYPE[i][m]);
                    //Trace.Debug("REJECTIONCAUSE: {0}", MachineInformation.REJECTIONCAUSE[i][m]);
                    //Trace.Debug("USERID: {0}", MachineInformation.USERID[i][m]);

                }
                auxProccesSide = auxProccesSide + "</string_1>";
                auxDate = auxDate + "</string_1>";
                auxHour = auxHour + "</string_1>";
                auxUser = auxUser + "</string_1>";
                auxProductionType = auxProductionType + "</string_1>";
                auxRejectionCause = auxRejectionCause + "</string_1>";

                PipeInformation.PIPENUMBER[i] = bundle.Items.ElementAt(i).Number.ToString();
                PipeInformation.FACTORYIDOUT[i] = String.Concat("0/", GeneralInformation.PRODUCTIONORDER, " ", GeneralInformation.HEATNUMBER, " ", PipeInformation.PIPENUMBER[i]);
                PipeInformation.REPROCESSINGNUMBER[i] = 0;
                xmlIT = xmlIT + PipeInformation.PIPENUMBER[i] + "</string>";
                auxFactory = auxFactory + "<string>" + PipeInformation.FACTORYIDOUT[i] + "</string>";
                auxReprocessing = auxReprocessing + "<decimal>" + PipeInformation.REPROCESSINGNUMBER[i] + "</decimal>";
            }
            xmlIT = xmlIT + "</PIPENUMBER>" + "<FACTORYIDOUT>" + auxFactory + "</FACTORYIDOUT>" + "<REPROCESSINGNUMBER>" + auxReprocessing + "</REPROCESSINGNUMBER>"
                    + "</PIPESPRODUCTION>" + "<MACHINESPRODUCTION>" + "<MACHINE>" + auxMachine + "</MACHINE>" + "<PROCCESEDSIDE>" + auxProccesSide + "</PROCCESEDSIDE>"
                    + "<DATE>" + auxDate + "</DATE>" + "<HOUR>" + auxHour + "</HOUR>" + "<USERID>" + auxUser + "</USERID>" + "<PRODUCTIONTYPE>" + auxProductionType
                    + "</PRODUCTIONTYPE>" + "<REJECTIONCAUSE>" + auxRejectionCause + "</REJECTIONCAUSE>" + "</MACHINESPRODUCTION>" + "</FINISHINGREPORT>";



            #endregion
            Report.GENERALINFORMATION = GeneralInformation;
            Report.MACHINESPRODUCTION = MachineInformation;
            Report.PIPESPRODUCTION = PipeInformation;

            TransactionStatus = new PIN149FDETAILEDTRANSACTIONSTATUS();

            Trace.Debug("INICIANDO ENVIO");
            Trace.Debug("COMMENTS: {0}", Report.GENERALINFORMATION.COMMENTS);
            Trace.Debug("FACTORYLOT: {0}", Report.GENERALINFORMATION.FACTORYLOT);
            Trace.Debug("GENERATEDETAIL: {0}", Report.GENERALINFORMATION.GENERATEDETAIL);
            Trace.Debug("HANDLINGTYPE: {0}", Report.GENERALINFORMATION.HANDLINGTYPE);
            Trace.Debug("HEATNUMBER: {0}", Report.GENERALINFORMATION.HEATNUMBER);
            Trace.Debug("LOCATION: {0}", Report.GENERALINFORMATION.LOCATION);
            Trace.Debug("MANUFACTURINGSTATUSIN: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSIN);
            Trace.Debug("MANUFACTURINGSTATUSOUT: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSOUT);
            Trace.Debug("NEXTMACHINE: {0}", Report.GENERALINFORMATION.NEXTMACHINE);
            Trace.Debug("PRINTER: {0}", Report.GENERALINFORMATION.PRINTER);
            Trace.Debug("PRINTTAG: {0}", Report.GENERALINFORMATION.PRINTTAG);
            Trace.Debug("PRODUCTIONORDER: {0}", Report.GENERALINFORMATION.PRODUCTIONORDER);
            Trace.Debug("RENUMBERPIPES: {0}", Report.GENERALINFORMATION.RENUMBERPIPES);
            Trace.Debug("REPORTTYPE: {0}", Report.GENERALINFORMATION.REPORTTYPE);
            Trace.Debug("STOCKTYPE: {0}", Report.GENERALINFORMATION.STOCKTYPE);
            Trace.Debug("TOTALPIECES: {0}", Report.GENERALINFORMATION.TOTALPIECES.ToString());
            Trace.Debug("TRANSACTIONNUMBER: {0}", Report.GENERALINFORMATION.TRANSACTIONNUMBER);

            
                Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
                Trace.Debug("MACHINE[1]: {0}", Report.MACHINESPRODUCTION.MACHINE[1]);
                //Trace.Debug("MACHINE[2]: {0}", Report.MACHINESPRODUCTION.MACHINE[2]);
            
            



            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    Trace.Debug("MACHINE: [{0}], PRODUCTIONTYPE: {1}, REJECTIONCAUSE: {2}, USERID: {3}", Report.MACHINESPRODUCTION.MACHINE[m], Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][m], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][m], Report.MACHINESPRODUCTION.USERID[i][m]);
                }
                //Trace.Debug("PRODUCTIONTYPE MACHINE1: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][1], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][1], Report.MACHINESPRODUCTION.USERID[i][1]);
                //Trace.Debug("PRODUCTIONTYPE MACHINE2: {0}, REJECTIONCAUSE: {1}, USERID: {3}", Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][2], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][2], Report.MACHINESPRODUCTION.USERID[i][2]);
                Trace.Debug("FACTORYIDOUT: {0}, PIPENUMBER: {1}, REPROCESSINGNUMBER: {2}", Report.PIPESPRODUCTION.FACTORYIDOUT[i], Report.PIPESPRODUCTION.PIPENUMBER[i], Report.PIPESPRODUCTION.REPROCESSINGNUMBER[i]);
            }

            ERRORTYPE = "";
            ERRORNUMBER = 0;
            ERRORDESCRIPTION = "";

            Trace.Debug("XML con long. de : {0}", xmlIT.Length);
            Trace.Debug("Sending bundle to IT...");
            try
            {
                Response = webService.PIN149F(Report, TransactionStatus, SECURITYID, LANGUAGEID, ref ERRORTYPE, ref ERRORNUMBER, ref ERRORDESCRIPTION);
            }
            catch (Exception ex)
            {
                Trace.Error("The bundle was not sent", ex.Message);
            }


            if (Convert.ToInt32(ERRORNUMBER) > 0)
            {
                Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
                errors.Add(Convert.ToInt32(ERRORNUMBER), ERRORDESCRIPTION);
                
            }
            if (Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED > 0)
            {
                result = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
                Trace.Debug("Created Bundle {0} PIP Transaction {1}", result, Response.BUNDLEANDTRANSACTIONSCREATED.PIPTRANSACTION.ToString());
            }
            else
                if (Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count() > 0)
                {
                    for (int e = 0; e < Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count(); e++)
                    {
                        Trace.Error(" Response PIN149F Error Number {0} : {1}", Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER[e], Response.DESCRIPTIVEERROR.DETAILEDERRORDESCRIPTION[e]);
                    }
                }
            return result;
        }

        
    
        //QUITANDO ERRORES
        public ITBundle SendBundleMES(string PipUser, int OrderNumber, int HeatNumber, IBundle bundle, string comments, bool rutaSEAS, int idBundleL2, string impresora, string EE, string groupElaborationState, string elaborationState, string location, string rejectionCode, string isRequiredEEOut, out Dictionary<int, string> errors, out string xmlIT)
        {
            ITBundle atado = new ITBundle();
            xmlIT = null;
            int ITMachinesRute;
            Trace.Message("Loading PIN149F.... ");
            Trace.Message("idBundle L2: {0}", idBundleL2);
            Trace.Debug("Canasta: {0}", bundle.IdCradle);
            errors = new Dictionary<int, string>();
            //int result = 0;
            #region Informacion General
            GeneralInformation = new PIN149FFINISHINGREPORTGENERALINFORMATION();

            GeneralInformation.TRANSACTIONNUMBER = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            xmlIT = "<urn:PIN149F><FINISHINGREPORT><GENERALINFORMATION><TRANSACTIONNUMBER>" + GeneralInformation.TRANSACTIONNUMBER + "</TRANSACTIONNUMBER>";

            string materialType;
            string side;
            string RejectedCode = String.Empty;
            object value;

            bundle.ExtraData.TryGetValue("MaterialType", out value);
            if (value != null)
                materialType = value.ToString();
            else
                materialType = "G";
            GeneralInformation.HANDLINGTYPE = materialType; // Detalle o Granel
            xmlIT = xmlIT + "<HANDLINGTYPE>" + GeneralInformation.HANDLINGTYPE + "</HANDLINGTYPE>";
            bundle.ExtraData.TryGetValue("Side", out value);
            if (value != null)
                side = value.ToString();
            else
                side = "A";

            RejectedCode = (bundle.RejectionCause != null) ? bundle.RejectionCause : "";

            GeneralInformation.STOCKTYPE = STOCKTYPE;
            if (bundle.ItemStatus == ItemStatus.Good)
                GeneralInformation.REPORTTYPE = REPORTTYPE;
            else
                GeneralInformation.REPORTTYPE = REPORTTYPECUTTING;
            GeneralInformation.PRODUCTIONORDER = OrderNumber.ToString().PadLeft(ORDER_CHARSIZE, PADDINGCHAR);
            GeneralInformation.HEATNUMBER = HeatNumber.ToString().PadLeft(HEAT_CHARSIZE, PADDINGCHAR);
            GeneralInformation.FACTORYLOT = "00000";

            Trace.Debug("ITLocation: {0}", bundle.ITLocation);
            if(location == String.Empty)
                GeneralInformation.LOCATION = bundle.ITLocation;
            else
                GeneralInformation.LOCATION = location;

            //GeneralInformation.LOCATION = (bundle.Location.Name != String.Empty) ? bundle.Location.Name : null;
            xmlIT = xmlIT + "<STOCKTYPE>" + GeneralInformation.STOCKTYPE + "</STOCKTYPE>" + "<REPORTTYPE>" + GeneralInformation.REPORTTYPE + "</REPORTTYPE>"
                    + "<PRODUCTIONORDER>" + GeneralInformation.PRODUCTIONORDER + "</PRODUCTIONORDER>" + "<HEATNUMBER>" + GeneralInformation.HEATNUMBER + "</HEATNUMBER>"
                    + "<FACTORYLOT>" + GeneralInformation.FACTORYLOT + "</FACTORYLOT>" + "<LOCATION>" + GeneralInformation.LOCATION + "</LOCATION>";

            //CAMBIO DE FER PASANDOLE A IT EL EE
            GeneralInformation.MANUFACTURINGSTATUSIN = EE;
            //GeneralInformation.MANUFACTURINGSTATUSIN = (side.Equals("A")) ? "69" : "79";
            //2
            //GeneralInformation.MANUFACTURINGSTATUSOUT = (bundle.Location.ElaborationStatus != String.Empty) ? bundle.Location.ElaborationStatus : null;

            if (isRequiredEEOut.ToUpper().Equals("TRUE"))
                GeneralInformation.MANUFACTURINGSTATUSOUT = elaborationState;
            else
                GeneralInformation.MANUFACTURINGSTATUSOUT = "";

            
            GeneralInformation.TOTALPIECES = bundle.Items.Count();
            xmlIT = xmlIT + "<MANUFACTURINGSTATUSIN>" + GeneralInformation.MANUFACTURINGSTATUSIN + "</MANUFACTURINGSTATUSIN>" + "<MANUFACTURINGSTATUSOUT>" + GeneralInformation.MANUFACTURINGSTATUSOUT + "</MANUFACTURINGSTATUSOUT>";
            //1
            GeneralInformation.PRINTTAG = "N";
            //8
            GeneralInformation.PRINTER = impresora;
            //70
            GeneralInformation.COMMENTS = comments;
            //1
            GeneralInformation.GENERATEDETAIL = "N";
            //1
            GeneralInformation.RENUMBERPIPES = "N";
            //5
            GeneralInformation.NEXTMACHINE = "N";

            xmlIT = xmlIT + "<TOTALPIECES>" + GeneralInformation.TOTALPIECES + "</TOTALPIECES>" + "<PRINTTAG>" + GeneralInformation.PRINTTAG + "</PRINTTAG>" + "<PRINTER>" + GeneralInformation.PRINTER + "</PRINTER>"
                    + "<COMMENTS>" + GeneralInformation.COMMENTS + "</COMMENTS>" + "<GENERATEDETAIL>" + GeneralInformation.GENERATEDETAIL + "</GENERATEDETAIL>"
                    + "<RENUMBERPIPES>" + GeneralInformation.RENUMBERPIPES + "</RENUMBERPIPES>" + "<NEXTMACHINE>" + GeneralInformation.NEXTMACHINE + "</NEXTMACHINE>"
                    + "</GENERALINFORMATION>";

            #endregion

            #region Machine Information

            MachineInformation = new PIN149FFINISHINGREPORTMACHINESPRODUCTION();
            ITMachinesRute = ITMachineEE.Count();
            MachineInformation.MACHINE = new string[1];

            MachineInformation.PROCCESEDSIDE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.DATE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.HOUR = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.USERID = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.PRODUCTIONTYPE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.REJECTIONCAUSE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            #endregion

            #region Pipes Production

            PipeInformation = new PIN149FFINISHINGREPORTPIPESPRODUCTION();
            PipeInformation.PIPENUMBER = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.FACTORYIDOUT = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.REPROCESSINGNUMBER = new decimal[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            // Recorrer el atado e insertar los tubos.

            xmlIT = xmlIT + "<PIPESPRODUCTION>" + "<PIPENUMBER>";
            string auxFactory = null;
            string auxReprocessing = null;
            string auxMachine = null;
            string auxProccesSide = null;
            string auxDate = null;
            string auxHour = null;
            string auxUser = null;
            string auxProductionType = null;
            string auxRejectionCause = null;
            int MachineSent = 0;

            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                xmlIT = xmlIT + "<string>";
                MachineInformation.PROCCESEDSIDE[i] = new string[ITMachinesRute];
                MachineInformation.DATE[i] = new string[ITMachinesRute];
                MachineInformation.HOUR[i] = new string[ITMachinesRute];
                MachineInformation.USERID[i] = new string[ITMachinesRute];
                MachineInformation.PRODUCTIONTYPE[i] = new string[ITMachinesRute];
                MachineInformation.REJECTIONCAUSE[i] = new string[ITMachinesRute];
                auxProccesSide = auxProccesSide + "<string_1>";
                auxDate = auxDate + "<string_1>";
                auxHour = auxHour + "<string_1>";
                auxUser = auxUser + "<string_1>";
                auxProductionType = auxProductionType + "<string_1>";
                auxRejectionCause = auxRejectionCause + "<string_1>";

                Trace.Debug("REPORT PRODUCTION  USER {0} :ITEM  {1}, STATUS {2}", PipUser, bundle.Items.ElementAt(i).Number.ToString(), bundle.ItemStatus.ToString());
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    MachineInformation.MACHINE[m] = ITMachineEE[m];
                    auxMachine = auxMachine + "<string>" + MachineInformation.MACHINE[m] + "</string>";
                    MachineInformation.PROCCESEDSIDE[i][m] = "";
                    auxProccesSide = auxProccesSide + "<string>" + MachineInformation.PROCCESEDSIDE[i][m] + "</string>";
                    MachineInformation.DATE[i][m] = System.DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    auxDate = auxDate + "<string>" + MachineInformation.DATE[i][m] + "</string>";
                    MachineInformation.HOUR[i][m] = System.DateTimeOffset.Now.ToString("HHmmssff", CultureInfo.InvariantCulture);
                    auxHour = auxHour + "<string>" + MachineInformation.HOUR[i][m] + "</string>";
                    MachineInformation.USERID[i][m] = PipUser;
                    auxUser = auxUser + "<string>" + MachineInformation.USERID[i][m] + "</string>";
                    MachineInformation.PRODUCTIONTYPE[i][m] = groupElaborationState;
                    auxProductionType = auxProductionType + "<string>" + MachineInformation.PRODUCTIONTYPE[i][m] + "</string>";
                    MachineInformation.REJECTIONCAUSE[i][m] = rejectionCode;
                    auxRejectionCause = auxRejectionCause + "<string>" + MachineInformation.REJECTIONCAUSE[i][m] + "</string>";
                    //if (RejectedCode != String.Empty && m == ITMachinesRute - 1)
                    //{
                    //    Trace.Debug("ITEM WITH REJECTION CODE {0]", RejectedCode);
                    //    MachineInformation.REJECTIONCAUSE[i][m] = RejectedCode;
                    //    MachineInformation.PRODUCTIONTYPE[i][m] = bundle.Location.ProductionType;
                    //}
                    if (rejectionCode != String.Empty && m == ITMachinesRute - 1)
                    {
                        Trace.Debug("ITEM WITH REJECTION CODE {0]", rejectionCode);
                        MachineInformation.REJECTIONCAUSE[i][m] = rejectionCode;
                        MachineInformation.PRODUCTIONTYPE[i][m] = groupElaborationState;
                    }

                }
                auxProccesSide = auxProccesSide + "</string_1>";
                auxDate = auxDate + "</string_1>";
                auxHour = auxHour + "</string_1>";
                auxUser = auxUser + "</string_1>";
                auxProductionType = auxProductionType + "</string_1>";
                auxRejectionCause = auxRejectionCause + "</string_1>";

                PipeInformation.PIPENUMBER[i] = bundle.Items.ElementAt(i).Number.ToString();
                PipeInformation.FACTORYIDOUT[i] = String.Concat("0/", GeneralInformation.PRODUCTIONORDER, " ", GeneralInformation.HEATNUMBER, " ", PipeInformation.PIPENUMBER[i]);
                PipeInformation.REPROCESSINGNUMBER[i] = 0;
                xmlIT = xmlIT + PipeInformation.PIPENUMBER[i] + "</string>";
                auxFactory = auxFactory + "<string>" + PipeInformation.FACTORYIDOUT[i] + "</string>";
                auxReprocessing = auxReprocessing + "<decimal>" + PipeInformation.REPROCESSINGNUMBER[i] + "</decimal>";
            }
            xmlIT = xmlIT + "</PIPENUMBER>" + "<FACTORYIDOUT>" + auxFactory + "</FACTORYIDOUT>" + "<REPROCESSINGNUMBER>" + auxReprocessing + "</REPROCESSINGNUMBER>"
                    + "</PIPESPRODUCTION>" + "<MACHINESPRODUCTION>" + "<MACHINE>" + auxMachine + "</MACHINE>" + "<PROCCESEDSIDE>" + auxProccesSide + "</PROCCESEDSIDE>"
                    + "<DATE>" + auxDate + "</DATE>" + "<HOUR>" + auxHour + "</HOUR>" + "<USERID>" + auxUser + "</USERID>" + "<PRODUCTIONTYPE>" + auxProductionType
                    + "</PRODUCTIONTYPE>" + "<REJECTIONCAUSE>" + auxRejectionCause + "</REJECTIONCAUSE>" + "</MACHINESPRODUCTION>" + "</FINISHINGREPORT>";



            #endregion
            Report.GENERALINFORMATION = GeneralInformation;
            Report.MACHINESPRODUCTION = MachineInformation;
            Report.PIPESPRODUCTION = PipeInformation;

            TransactionStatus = new PIN149FDETAILEDTRANSACTIONSTATUS();

            Trace.Debug("INICIANDO ENVIO");
            Trace.Debug("COMMENTS: {0}", Report.GENERALINFORMATION.COMMENTS);
            Trace.Debug("FACTORYLOT: {0}", Report.GENERALINFORMATION.FACTORYLOT);
            Trace.Debug("GENERATEDETAIL: {0}", Report.GENERALINFORMATION.GENERATEDETAIL);
            Trace.Debug("HANDLINGTYPE: {0}", Report.GENERALINFORMATION.HANDLINGTYPE);
            Trace.Debug("HEATNUMBER: {0}", Report.GENERALINFORMATION.HEATNUMBER);
            Trace.Debug("LOCATION: {0}", Report.GENERALINFORMATION.LOCATION);
            Trace.Debug("MANUFACTURINGSTATUSIN: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSIN);
            Trace.Debug("MANUFACTURINGSTATUSOUT: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSOUT);
            Trace.Debug("NEXTMACHINE: {0}", Report.GENERALINFORMATION.NEXTMACHINE);
            Trace.Debug("PRINTER: {0}", Report.GENERALINFORMATION.PRINTER);
            Trace.Debug("PRINTTAG: {0}", Report.GENERALINFORMATION.PRINTTAG);
            Trace.Debug("PRODUCTIONORDER: {0}", Report.GENERALINFORMATION.PRODUCTIONORDER);
            Trace.Debug("RENUMBERPIPES: {0}", Report.GENERALINFORMATION.RENUMBERPIPES);
            Trace.Debug("REPORTTYPE: {0}", Report.GENERALINFORMATION.REPORTTYPE);
            Trace.Debug("STOCKTYPE: {0}", Report.GENERALINFORMATION.STOCKTYPE);
            Trace.Debug("TOTALPIECES: {0}", Report.GENERALINFORMATION.TOTALPIECES.ToString());
            Trace.Debug("TRANSACTIONNUMBER: {0}", Report.GENERALINFORMATION.TRANSACTIONNUMBER);
            Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);



            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    Trace.Debug("MACHINE: [{0}], PRODUCTIONTYPE: {1}, REJECTIONCAUSE: {2}, USERID: {3}", Report.MACHINESPRODUCTION.MACHINE[m], Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][m], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][m], Report.MACHINESPRODUCTION.USERID[i][m]);
                }
                Trace.Debug("FACTORYIDOUT: {0}, PIPENUMBER: {1}, REPROCESSINGNUMBER: {2}", Report.PIPESPRODUCTION.FACTORYIDOUT[i], Report.PIPESPRODUCTION.PIPENUMBER[i], Report.PIPESPRODUCTION.REPROCESSINGNUMBER[i]);
            }

            ERRORTYPE = "";
            ERRORNUMBER = 0;
            ERRORDESCRIPTION = "";

            Trace.Debug("XML con long. de : {0}", xmlIT.Length);
            Trace.Debug("Sending bundle to IT...");

            try
            {
                Response = webService.PIN149F(Report, TransactionStatus, SECURITYID, LANGUAGEID, ref ERRORTYPE, ref ERRORNUMBER, ref ERRORDESCRIPTION);
                atado.Machine = 1;
            }
            catch (Exception ex)
            {
                Trace.Error("The bundle was not sent", ex.Message);
                atado.Machine = 0;
                atado.idBundle = 0;
            }

            if (Convert.ToInt32(ERRORNUMBER) > 0)
            {
                Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
                MachineSent = 0;
                atado.Machine = 0;
                atado.idBundle = 0;
                errors.Add(Convert.ToInt32(ERRORNUMBER), ERRORDESCRIPTION);
                //Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
            }
            if (Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED > 0)
            {

                //result = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
                atado.idBundle = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
                Trace.Debug("Created Bundle {0} PIP Transaction {1}", atado.idBundle, Response.BUNDLEANDTRANSACTIONSCREATED.PIPTRANSACTION.ToString());
                atado.Machine = 1;
            }
            else
                if (Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count() > 0)
            {
                for (int e = 0; e < Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count(); e++)
                {
                    Trace.Error(" Response PIN149F Error Number {0} : {1}", Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER[e], Response.DESCRIPTIVEERROR.DETAILEDERRORDESCRIPTION[e]);
                    atado.Machine = 0;
                    atado.idBundle = 0;
                }
            }

            //return result;
            return atado;
        }


        public ITBundle SendBundleMSEAS(string PipUser, int OrderNumber, int HeatNumber, IBundle bundle, string comments, bool rutaSEAS, int idBundleL2, string impresora, string EE, string groupElaborationState, string elaborationState, string location, string rejectionCode, string isRequiredEEOut, out Dictionary<int, string> errors, out string xmlIT)
        {
            ITBundle atado = new ITBundle();
            xmlIT = null;
            int ITMachinesRute;
            Trace.Message("Loading PIN149F.... ");
            Trace.Message("idBundle L2: {0}", idBundleL2);
            errors = new Dictionary<int, string>();
            //int result = 0;
            #region Informacion General
            GeneralInformation = new PIN149FFINISHINGREPORTGENERALINFORMATION();

            GeneralInformation.TRANSACTIONNUMBER = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            xmlIT = "<urn:PIN149F><FINISHINGREPORT><GENERALINFORMATION><TRANSACTIONNUMBER>" + GeneralInformation.TRANSACTIONNUMBER + "</TRANSACTIONNUMBER>";

            string materialType;
            string side;
            string RejectedCode = String.Empty;
            object value;

            bundle.ExtraData.TryGetValue("MaterialType", out value);
            if (value != null)
                materialType = value.ToString();
            else
                materialType = "G";
            GeneralInformation.HANDLINGTYPE = materialType; // Detalle o Granel
            xmlIT = xmlIT + "<HANDLINGTYPE>" + GeneralInformation.HANDLINGTYPE + "</HANDLINGTYPE>";
            bundle.ExtraData.TryGetValue("Side", out value);
            if (value != null)
                side = value.ToString();
            else
                side = "A";

            RejectedCode = (bundle.RejectionCause != null) ? bundle.RejectionCause : "";

            GeneralInformation.STOCKTYPE = STOCKTYPE;
            if (bundle.ItemStatus == ItemStatus.Good)
                GeneralInformation.REPORTTYPE = REPORTTYPE;
            else
                GeneralInformation.REPORTTYPE = REPORTTYPECUTTING;
            GeneralInformation.PRODUCTIONORDER = OrderNumber.ToString().PadLeft(ORDER_CHARSIZE, PADDINGCHAR);
            GeneralInformation.HEATNUMBER = HeatNumber.ToString().PadLeft(HEAT_CHARSIZE, PADDINGCHAR);
            GeneralInformation.FACTORYLOT = "00000";

            if (location == String.Empty)
                GeneralInformation.LOCATION = bundle.ITLocation;
            else
                GeneralInformation.LOCATION = location;
            Trace.Debug("ITLocation: {0}", GeneralInformation.LOCATION);


            //GeneralInformation.LOCATION = "SE305";
            //GeneralInformation.LOCATION = (bundle.Location.Name != String.Empty) ? bundle.Location.Name : null;
            xmlIT = xmlIT + "<STOCKTYPE>" + GeneralInformation.STOCKTYPE + "</STOCKTYPE>" + "<REPORTTYPE>" + GeneralInformation.REPORTTYPE + "</REPORTTYPE>"
                    + "<PRODUCTIONORDER>" + GeneralInformation.PRODUCTIONORDER + "</PRODUCTIONORDER>" + "<HEATNUMBER>" + GeneralInformation.HEATNUMBER + "</HEATNUMBER>"
                    + "<FACTORYLOT>" + GeneralInformation.FACTORYLOT + "</FACTORYLOT>" + "<LOCATION>" + GeneralInformation.LOCATION + "</LOCATION>";

            //CAMBIO DE FER PASANDOLE A IT EL EE
            GeneralInformation.MANUFACTURINGSTATUSIN = "";
            //GeneralInformation.MANUFACTURINGSTATUSIN = (side.Equals("A")) ? "69" : "79";
            //2
            //GeneralInformation.MANUFACTURINGSTATUSOUT = (bundle.Location.ElaborationStatus != String.Empty) ? bundle.Location.ElaborationStatus : null;
            if (isRequiredEEOut.ToUpper().Equals("TRUE"))
                GeneralInformation.MANUFACTURINGSTATUSOUT = elaborationState;
            else
                GeneralInformation.MANUFACTURINGSTATUSOUT = "";

            GeneralInformation.TOTALPIECES = bundle.Items.Count();
            xmlIT = xmlIT + "<MANUFACTURINGSTATUSIN>" + GeneralInformation.MANUFACTURINGSTATUSIN + "</MANUFACTURINGSTATUSIN>" + "<MANUFACTURINGSTATUSOUT>" + GeneralInformation.MANUFACTURINGSTATUSOUT + "</MANUFACTURINGSTATUSOUT>";
            //1
            GeneralInformation.PRINTTAG = "S";
            //8
            GeneralInformation.PRINTER = impresora;
            //70
            GeneralInformation.COMMENTS = comments;
            //1
            GeneralInformation.GENERATEDETAIL = "N";
            //1
            GeneralInformation.RENUMBERPIPES = "N";
            //5
            GeneralInformation.NEXTMACHINE = "N";

            xmlIT = xmlIT + "<TOTALPIECES>" + GeneralInformation.TOTALPIECES + "</TOTALPIECES>" + "<PRINTTAG>" + GeneralInformation.PRINTTAG + "</PRINTTAG>" + "<PRINTER>" + GeneralInformation.PRINTER + "</PRINTER>"
                    + "<COMMENTS>" + GeneralInformation.COMMENTS + "</COMMENTS>" + "<GENERATEDETAIL>" + GeneralInformation.GENERATEDETAIL + "</GENERATEDETAIL>"
                    + "<RENUMBERPIPES>" + GeneralInformation.RENUMBERPIPES + "</RENUMBERPIPES>" + "<NEXTMACHINE>" + GeneralInformation.NEXTMACHINE + "</NEXTMACHINE>"
                    + "</GENERALINFORMATION>";

            #endregion

            #region Machine Information

            MachineInformation = new PIN149FFINISHINGREPORTMACHINESPRODUCTION();


            ITMachinesRute = ITMachines.Count();
            MachineInformation.MACHINE = new string[ITMachines.Count()];



            MachineInformation.PROCCESEDSIDE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.DATE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.HOUR = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.USERID = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.PRODUCTIONTYPE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            MachineInformation.REJECTIONCAUSE = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)][];
            #endregion

            #region Pipes Production

            PipeInformation = new PIN149FFINISHINGREPORTPIPESPRODUCTION();
            PipeInformation.PIPENUMBER = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.FACTORYIDOUT = new string[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            PipeInformation.REPROCESSINGNUMBER = new decimal[Convert.ToInt32(GeneralInformation.TOTALPIECES)];
            // Recorrer el atado e insertar los tubos.

            xmlIT = xmlIT + "<PIPESPRODUCTION>" + "<PIPENUMBER>";
            string auxFactory = null;
            string auxReprocessing = null;
            string auxMachine = null;
            string auxProccesSide = null;
            string auxDate = null;
            string auxHour = null;
            string auxUser = null;
            string auxProductionType = null;
            string auxRejectionCause = null;

            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                xmlIT = xmlIT + "<string>";
                MachineInformation.PROCCESEDSIDE[i] = new string[ITMachinesRute];
                MachineInformation.DATE[i] = new string[ITMachinesRute];
                MachineInformation.HOUR[i] = new string[ITMachinesRute];
                MachineInformation.USERID[i] = new string[ITMachinesRute];
                MachineInformation.PRODUCTIONTYPE[i] = new string[ITMachinesRute];
                MachineInformation.REJECTIONCAUSE[i] = new string[ITMachinesRute];
                auxProccesSide = auxProccesSide + "<string_1>";
                auxDate = auxDate + "<string_1>";
                auxHour = auxHour + "<string_1>";
                auxUser = auxUser + "<string_1>";
                auxProductionType = auxProductionType + "<string_1>";
                auxRejectionCause = auxRejectionCause + "<string_1>";

                Trace.Debug("REPORT PRODUCTION  USER {0} :ITEM  {1}, STATUS {2}", PipUser, bundle.Items.ElementAt(i).Number.ToString(), bundle.ItemStatus.ToString());
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    MachineInformation.MACHINE[m] = ITMachines[m];
                    auxMachine = auxMachine + "<string>" + MachineInformation.MACHINE[m] + "</string>";
                    MachineInformation.PROCCESEDSIDE[i][m] = "";
                    auxProccesSide = auxProccesSide + "<string>" + MachineInformation.PROCCESEDSIDE[i][m] + "</string>";
                    MachineInformation.DATE[i][m] = System.DateTimeOffset.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    auxDate = auxDate + "<string>" + MachineInformation.DATE[i][m] + "</string>";
                    MachineInformation.HOUR[i][m] = System.DateTimeOffset.Now.ToString("HHmmssff", CultureInfo.InvariantCulture);
                    auxHour = auxHour + "<string>" + MachineInformation.HOUR[i][m] + "</string>";
                    MachineInformation.USERID[i][m] = PipUser;
                    auxUser = auxUser + "<string>" + MachineInformation.USERID[i][m] + "</string>";
                    MachineInformation.PRODUCTIONTYPE[i][m] = groupElaborationState;
                    auxProductionType = auxProductionType + "<string>" + MachineInformation.PRODUCTIONTYPE[i][m] + "</string>";
                    MachineInformation.REJECTIONCAUSE[i][m] = rejectionCode;
                    auxRejectionCause = auxRejectionCause + "<string>" + MachineInformation.REJECTIONCAUSE[i][m] + "</string>";
                    //if (RejectedCode != String.Empty && m == ITMachinesRute - 1)
                    //{
                    //    Trace.Debug("ITEM WITH REJECTION CODE {0]", RejectedCode);
                    //    MachineInformation.REJECTIONCAUSE[i][m] = RejectedCode;
                    //    MachineInformation.PRODUCTIONTYPE[i][m] = bundle.Location.ProductionType;
                    //}
                    if (rejectionCode != String.Empty && m == ITMachinesRute - 1)
                    {
                        Trace.Debug("ITEM WITH REJECTION CODE {0]", rejectionCode);
                        MachineInformation.REJECTIONCAUSE[i][m] = rejectionCode;
                        MachineInformation.PRODUCTIONTYPE[i][m] = groupElaborationState;
                    }

                }
                auxProccesSide = auxProccesSide + "</string_1>";
                auxDate = auxDate + "</string_1>";
                auxHour = auxHour + "</string_1>";
                auxUser = auxUser + "</string_1>";
                auxProductionType = auxProductionType + "</string_1>";
                auxRejectionCause = auxRejectionCause + "</string_1>";

                PipeInformation.PIPENUMBER[i] = bundle.Items.ElementAt(i).Number.ToString();
                PipeInformation.FACTORYIDOUT[i] = String.Concat("0/", GeneralInformation.PRODUCTIONORDER, " ", GeneralInformation.HEATNUMBER, " ", PipeInformation.PIPENUMBER[i]);
                PipeInformation.REPROCESSINGNUMBER[i] = 0;
                xmlIT = xmlIT + PipeInformation.PIPENUMBER[i] + "</string>";
                auxFactory = auxFactory + "<string>" + PipeInformation.FACTORYIDOUT[i] + "</string>";
                auxReprocessing = auxReprocessing + "<decimal>" + PipeInformation.REPROCESSINGNUMBER[i] + "</decimal>";
            }
            xmlIT = xmlIT + "</PIPENUMBER>" + "<FACTORYIDOUT>" + auxFactory + "</FACTORYIDOUT>" + "<REPROCESSINGNUMBER>" + auxReprocessing + "</REPROCESSINGNUMBER>"
                    + "</PIPESPRODUCTION>" + "<MACHINESPRODUCTION>" + "<MACHINE>" + auxMachine + "</MACHINE>" + "<PROCCESEDSIDE>" + auxProccesSide + "</PROCCESEDSIDE>"
                    + "<DATE>" + auxDate + "</DATE>" + "<HOUR>" + auxHour + "</HOUR>" + "<USERID>" + auxUser + "</USERID>" + "<PRODUCTIONTYPE>" + auxProductionType
                    + "</PRODUCTIONTYPE>" + "<REJECTIONCAUSE>" + auxRejectionCause + "</REJECTIONCAUSE>" + "</MACHINESPRODUCTION>" + "</FINISHINGREPORT>";



            #endregion
            Report.GENERALINFORMATION = GeneralInformation;
            Report.MACHINESPRODUCTION = MachineInformation;
            Report.PIPESPRODUCTION = PipeInformation;

            TransactionStatus = new PIN149FDETAILEDTRANSACTIONSTATUS();

            Trace.Debug("INICIANDO ENVIO");
            Trace.Debug("COMMENTS: {0}", Report.GENERALINFORMATION.COMMENTS);
            Trace.Debug("FACTORYLOT: {0}", Report.GENERALINFORMATION.FACTORYLOT);
            Trace.Debug("GENERATEDETAIL: {0}", Report.GENERALINFORMATION.GENERATEDETAIL);
            Trace.Debug("HANDLINGTYPE: {0}", Report.GENERALINFORMATION.HANDLINGTYPE);
            Trace.Debug("HEATNUMBER: {0}", Report.GENERALINFORMATION.HEATNUMBER);
            Trace.Debug("LOCATION: {0}", Report.GENERALINFORMATION.LOCATION);
            Trace.Debug("MANUFACTURINGSTATUSIN: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSIN);
            Trace.Debug("MANUFACTURINGSTATUSOUT: {0}", Report.GENERALINFORMATION.MANUFACTURINGSTATUSOUT);
            Trace.Debug("NEXTMACHINE: {0}", Report.GENERALINFORMATION.NEXTMACHINE);
            Trace.Debug("PRINTER: {0}", Report.GENERALINFORMATION.PRINTER);
            Trace.Debug("PRINTTAG: {0}", Report.GENERALINFORMATION.PRINTTAG);
            Trace.Debug("PRODUCTIONORDER: {0}", Report.GENERALINFORMATION.PRODUCTIONORDER);
            Trace.Debug("RENUMBERPIPES: {0}", Report.GENERALINFORMATION.RENUMBERPIPES);
            Trace.Debug("REPORTTYPE: {0}", Report.GENERALINFORMATION.REPORTTYPE);
            Trace.Debug("STOCKTYPE: {0}", Report.GENERALINFORMATION.STOCKTYPE);
            Trace.Debug("TOTALPIECES: {0}", Report.GENERALINFORMATION.TOTALPIECES.ToString());
            Trace.Debug("TRANSACTIONNUMBER: {0}", Report.GENERALINFORMATION.TRANSACTIONNUMBER);


            Trace.Debug("MACHINE[0]: {0}", Report.MACHINESPRODUCTION.MACHINE[0]);
            Trace.Debug("MACHINE[1]: {0}", Report.MACHINESPRODUCTION.MACHINE[1]);

            for (int i = 0; i < bundle.Items.Count(); i++)
            {
                for (int m = 0; m < ITMachinesRute; m++)
                {
                    Trace.Debug("MACHINE: [{0}], PRODUCTIONTYPE: {1}, REJECTIONCAUSE: {2}, USERID: {3}", Report.MACHINESPRODUCTION.MACHINE[m], Report.MACHINESPRODUCTION.PRODUCTIONTYPE[i][m], Report.MACHINESPRODUCTION.REJECTIONCAUSE[i][m], Report.MACHINESPRODUCTION.USERID[i][m]);
                }
                Trace.Debug("FACTORYIDOUT: {0}, PIPENUMBER: {1}, REPROCESSINGNUMBER: {2}", Report.PIPESPRODUCTION.FACTORYIDOUT[i], Report.PIPESPRODUCTION.PIPENUMBER[i], Report.PIPESPRODUCTION.REPROCESSINGNUMBER[i]);
            }

            ERRORTYPE = "";
            ERRORNUMBER = 0;
            ERRORDESCRIPTION = "";

            Trace.Debug("XML con long. de : {0}", xmlIT.Length);
            Trace.Debug("Sending bundle to IT...");

            try
            {
                Response = webService.PIN149F(Report, TransactionStatus, SECURITYID, LANGUAGEID, ref ERRORTYPE, ref ERRORNUMBER, ref ERRORDESCRIPTION);
                atado.Machine = 3;
            }
            catch (Exception ex)
            {
                Trace.Error("The bundle was not sent", ex.Message);
                atado.Machine = 1;
            }


            if (Convert.ToInt32(ERRORNUMBER) > 0)
            {
                Trace.Error("ERRORTYPE {0}, ERRORNUMBER {1}: {2} from PIN149F", ERRORTYPE, ERRORNUMBER, ERRORDESCRIPTION);
                errors.Add(Convert.ToInt32(ERRORNUMBER), ERRORDESCRIPTION);
                atado.Machine = 1;

            }
            if (Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED > 0)
            {
                atado.idBundle = Convert.ToInt32(Response.BUNDLEANDTRANSACTIONSCREATED.BUNDLECREATED);
                Trace.Debug("Created Bundle {0} PIP Transaction {1}", atado.idBundle, Response.BUNDLEANDTRANSACTIONSCREATED.PIPTRANSACTION.ToString());
                atado.Machine = 3;
            }
            else
                if (Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count() > 0)
            {
                for (int e = 0; e < Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER.Count(); e++)
                {
                    Trace.Error(" Response PIN149F Error Number {0} : {1}", Response.DESCRIPTIVEERROR.DETAILEDERRORNUMBER[e], Response.DESCRIPTIVEERROR.DETAILEDERRORDESCRIPTION[e]);
                }
            }

            return atado;
        }

    }
}
