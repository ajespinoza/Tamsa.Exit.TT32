using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.View.Exit.Model
{
    public class GroupElaborationState
    {
        #region Constructor

        public GroupElaborationState()
        { }

        public GroupElaborationState(int IdGroupElaborationState, string Code, string Name, string Description)
        {
            this.IdGroupElaborationState = IdGroupElaborationState;
            this.Code = Code;
            this.Name = Name;
            this.Description = Description;
        }

        #endregion

        #region Properties

        public int IdGroupElaborationState { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }


        #endregion
    }
}
