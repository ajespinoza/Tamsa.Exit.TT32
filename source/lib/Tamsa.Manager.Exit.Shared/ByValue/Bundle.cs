// -----------------------------------------------------------------------
// <copyright file="Bundle.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the bundle class.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
 
    /// <summary>
    /// Bundle class.
    /// </summary>
    [Serializable]
    public class Bundle : IBundle
    {

        #region Constructor

        /// <summary>
        ///  Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        /// <param name="idBundle">
        /// Id bundle.
        /// </param>
        /// <param name="idBatch">
        /// Id batch.
        /// </param>
        /// <param name="idItemStatus">
        /// Id item status.
        /// </param>
        /// <param name="pieces">
        /// Pieces.
        /// </param>
        /// <param name="weight">
        /// Peso.
        /// </param>
        /// <param name="state">
        /// Estado.
        /// </param>
        public Bundle(int idCradle,int idBundle, int idBatch, int idSpecification, ItemStatus itemStatus, BundleState state, BundleStatus status, string comments, bool isManual)
        {
            this.IdCradle = idCradle;
            this.IdBundle = idBundle;
            this.IdBatch = idBatch;
            this.IdSpecification = idSpecification;                                    
            this.ItemStatus = itemStatus;
            this.Status = status;
            this.State = state;              
            this.BundleNumber = 0;
            this.Pieces = 0;
            this.Weight = 0;
            this.Comments = comments;
            this.IsManual = isManual;
            this.Items = new List<IItem>();
            this.CreationDate = DateTimeOffset.Now;
        }


        //
        public Bundle(int idCradle, int idBundle, int idBatch, int idSpecification, ItemStatus itemStatus, BundleState state, BundleStatus status, string comments, bool isManual, string itLocation, int sent)
        {
            this.IdCradle = idCradle;
            this.IdBundle = idBundle;
            this.IdBatch = idBatch;
            this.IdSpecification = idSpecification;
            this.ItemStatus = itemStatus;
            this.Status = status;
            this.State = state;
            this.BundleNumber = 0;
            this.Pieces = 0;
            this.Weight = 0;
            this.Comments = comments;
            this.IsManual = isManual;
            this.ITLocation = itLocation;
            this.Items = new List<IItem>();
            this.CreationDate = DateTimeOffset.Now;
            this.Sent = sent;
        }
        //

        public Bundle(int idCradle, int idBatch, int idSpecification, bool isManual, BundleState state)
        {
            this.IdCradle = idCradle;
            this.IdBatch = idBatch;
            this.IdSpecification = idSpecification;
            this.IsManual = isManual;
            this.Status = BundleStatus.InProcess;
            this.State = state;       
            this.Items = new List<IItem>();
            this.CreationDate = DateTimeOffset.Now;
        }
     
        #endregion
       

        #region Properties
        /// <summary>
        /// Id bundle.
        /// </summary>
        public int IdBundle { get;  private set; }

        /// <summary>
        /// Id Cradle
        /// </summary>
        public int IdCradle { get; private set; }

        /// <summary>
        /// Id batch.
        /// </summary>
        public int IdBatch { get; private set; }

        /// <summary>
        ///  Id Specification
        /// </summary>
        public int IdSpecification { get; private set; }
      
        /// <summary>
        /// Pieces.
        /// </summary>
        public int Pieces { get; set; }

        /// <summary>
        /// Calculate weight for bundle.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Creation Date
        /// </summary>
        public DateTimeOffset CreationDate  { get; set; }

        /// <summary>
        /// Sending Date
        /// </summary>
        public DateTimeOffset SendingDate  { get;   set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IItem> Items  { get; private set; }
       
        /// <summary>
        /// Bundle state {Open, Closed}.
        /// </summary>
        public BundleState State { get; private set; }

        /// <summary>
        /// Bundle Number
        /// </summary>
        public int BundleNumber { get; private set; }



        
        public BundleStatus Status { get; set; }

        public string Comments { get; set; }

        public string ITLocation { get; set; }

        public ILocation Location { get; set; }

        public bool IsManual { get; set; }        
        
        public string RejectionCause {get; set;}

        public int Sent{ get; set; }

        public ItemStatus ItemStatus
        {
            get;
            set;
        }

        public Dictionary<string, object> ExtraData
        {
            get;
            set;
        }

        

        #endregion

        #region Methods

        /// <summary>
        /// Add tracking to bundle.
        /// </summary>
        /// <param name="tracking">
        /// Tracking object.
        /// </param>
        public void AddItem(IItem item)
        {
            List<IItem> items = new List<IItem>();
            items = Items.ToList();
            items.Add(item);
            Items = items;
            this.Pieces = items.Count;
            this.Weight = items.Sum(i => i.Weight);
        }

        public void RemoveItem(IItem item)
        {
            List<IItem> items = new List<IItem>();
            items = Items.ToList();
            if (items.Remove(item))
            {
                this.Pieces = items.Count;
                this.Weight = items.Sum(i => i.Weight);
            }
            Items = items;
        }

        public void AddItems(List<IItem> nitems)
        {
            List<IItem> items = this.Items.ToList();
            items.AddRange(nitems);            
            this.Pieces = items.Count;
            this.Weight = items.Sum(i => i.Weight);
            Items = items;
        }

        public void RemoveItems()
        {
            List<IItem> items = this.Items.ToList();
            items.Clear();
            Items = items;
            this.Pieces = 0;
            this.Weight = 0;
        }
        
        

        /// <summary>
        /// Close bundle.
        /// </summary>
        public void Close()
        {
             this.State = BundleState.Closed;
             this.Status = BundleStatus.Pending;
        }

        public void Send(int number)
        {
            if (number > 0)
            {
                this.BundleNumber = number;
                this.Status = BundleStatus.Sent;
            }
            else
            {
                this.Status = BundleStatus.ErrorSending;
            }
        }

        public void Open()
        {
            if (this.Status == BundleStatus.InProcess)
            {
                this.State = BundleState.Open;
            }
        }


        #endregion











      
    }
}
