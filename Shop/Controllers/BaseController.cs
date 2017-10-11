using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Shop.Global;
using System.Data.Entity.Core.Objects;
using Shop.Models;

namespace Shop.Controllers
{
    public class BaseController : Controller
    {
        public static SQLRepository repository;
        public BaseController()
        {
            repository = new SQLRepository();
        }

        public void SetOrderesCount()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.OrdersCount = repository.GetOrdersByUser(User.Identity.Name, OrderState.InCart.ToString()).Sum(o => o.price);
            }
            else if (Session["OrderList"] != null)
                ViewBag.OrdersCount = ((List<Order>)Session["OrderList"]).Sum(o => o.price);
            else ViewBag.OrdersCount = 0;
        }
    }
}