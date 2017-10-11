using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Models
{
    public class ItemWithInfo
    {
        public Item Item { get; set; }
        public List<Price> Price { get; set; }
        public List<DetailsLink> DetailsLink { get; set; }
        public List<Order> Order { get; set; }
    }
}