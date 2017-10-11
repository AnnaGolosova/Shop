using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Models
{
    public class DetailsUsing
    {
        public int detailsTypeId;
        public bool use;
    }

    public class DetailsValueUse
    {
        public DetailsValue value;
        public bool inUse;
    }

    public class DetailsTypeWithValues
    {
        public DetailsType type;
        public List<DetailsValueUse> values;
    }

    public class HundredCheckBoxModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Dictionary<string, bool> checkedValues = new Dictionary<string, bool>();
            foreach (var i in controllerContext.HttpContext.Request.Form.AllKeys.Where(p => p.Contains("partner")))
                checkedValues.Add(i, true);
            return checkedValues;
        }
    }
}