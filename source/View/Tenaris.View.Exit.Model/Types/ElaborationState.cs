using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.View.Exit.Model
{
    public class ElaborationState
    {
        #region Constructor

        public ElaborationState()
        { }

        public ElaborationState(int IdElaborationState, string Code, string Name, string Description)
        {
            this.IdElaborationState = IdElaborationState;
            this.Code = Code;
            this.Name = Name;
            this.Description = Description;
        }

        #endregion

        #region Properties

        public int IdElaborationState { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }


        #endregion
    }
}
