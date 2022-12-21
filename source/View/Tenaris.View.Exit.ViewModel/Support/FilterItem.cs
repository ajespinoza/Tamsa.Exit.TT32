using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tenaris.View.Exit.ViewModel.Support
{
    public class FilterItem
    {
        #region "Variables"

        private string displayValue;
        private object itemValue;

        #endregion

        #region "Properties"

        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                if (displayValue != value)
                {
                    displayValue = value;
                } // if
            }
        }

        public object ItemValue
        {
            get { return itemValue; }
            set
            {
                if (itemValue != value)
                {
                    itemValue = value;
                } // if
            }
        }

        #endregion


        public FilterItem(string text, object value)
        {
            displayValue = text;
            itemValue = value;
        }
    }
}
