using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class CheckoutHelper
    {
        public bool Checked;
        public int count;
    }

    public class CheckoutList
    {
        public List<CheckoutHelper> list;
    }
}