//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shop.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DetailsLink
    {
        public int id { get; set; }
        public int detailsValueId { get; set; }
        public int itemId { get; set; }
    
        public virtual DetailsValue DetailsValue { get; set; }
        public virtual Item Item { get; set; }
    }
}
