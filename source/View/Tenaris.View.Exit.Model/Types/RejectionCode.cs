using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.View.Exit.Model
{
    public  class RejectionCode
    {
        #region Constructor

        public RejectionCode()
        { }

        public RejectionCode(int IdRejectionCode, string Code, string Name, string Description)
        {
            this.IdRejectionCode = IdRejectionCode;
            this.Code = Code;
            this.Name = Name;
            this.Description = Description;
        }

        #endregion

        #region Properties

        public int IdRejectionCode { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }


        #endregion
    }
}
