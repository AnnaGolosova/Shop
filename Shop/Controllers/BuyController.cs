using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Shop.Models;
using Shop.Global;

namespace Shop.Controllers
{
    public class BuyController : BaseController
    {
        public ActionResult InBag(int itemId = 0, int priceId = 0)
        {
            //SQLRepository repository = new SQLRepository();
            Item item = repository.GetItem(itemId);
            Price price = repository.GetPrice(priceId);
            if (item == null || price == null)
                return RedirectToAction("Index", "Home");
            Order model = new Order()
            {
                Item = item,
                itemId = item.id,
                price = price.price,
                description = item.description,
                itemImage = item.image,
                itemTitle = item.title,
                partNumber = item.partNumber,
                state = (int)OrderState.InCart,
                supplierAddress = price.Supplier.address,
                supplierCaption = price.Supplier.caption,
                supplierTitle = price.Supplier.title,
                Delivery = repository.GetDelivery(1),
                date = DateTime.Now
            };
            if (User.Identity.IsAuthenticated)
            {
                model.userId = repository.GetUserId(User.Identity.Name);
                repository.AddOrder(model);
            }
            else
            {
                List<Order> OrderList = new List<Order>();
                if (Session["OrderList"] != null)
                {
                    OrderList = (List<Order>)Session["OrderList"];
                }
                model.id = OrderList.Count == 0 ? 0 : OrderList.Max(o => o.id) + 1;
                if (!OrderList.Any(o => o.itemId == model.Item.id
                && String.Compare(o.userId, model.userId) == 0
                && String.Compare(o.supplierTitle, model.supplierTitle) == 0
                && model.state == (int)OrderState.InCart))
                    OrderList.Add(model);

                Session["OrderList"] = OrderList;
            }
            return RedirectToAction("Item", "Home", new { id = itemId });
        }

        [AllowAnonymous]
        public ActionResult Cart()
        {
            SetOrderesCount();
            ViewBag.isCartView = true;
            List<Order> orders = new List<Order>();
            //SQLRepository repository = new SQLRepository();
            if (User.Identity.IsAuthenticated)
            {
                orders = repository.GetOrdersByUser(User.Identity.Name, OrderState.InCart.ToString());
            }
            else
            {
                if (Session["OrderList"] != null)
                {
                    orders = (List<Order>)Session["OrderList"];
                }
            }
            ViewBag.Orders = orders;
            return View();
        }

        public ActionResult Checkout(List<bool> Checked)
        {
            List<Order> orders = new List<Order>();
            if (User.Identity.IsAuthenticated)
            {
                orders = repository.GetOrdersByUser(User.Identity.Name, OrderState.InCart.ToString());
            }
            else
            {
                if (Session["OrderList"] != null)
                {
                    orders = (List<Order>)Session["OrderList"];
                }
            }
            int j = 0;
            List<Order> CheckoutedOrders = new List<Order>();
            foreach (Order order in orders)
            {
                if (Checked[j])
                {
                    //repository.SetOrderState(order.id, (int)OrderState.OnOrder, count[i++]);
                    //order.state = (int)OrderState.OnOrder;
                    //order.count = count[i++];
                    CheckoutedOrders.Add(order);
                    j = j + 2;
                } else
                {
                    j++;
                }
            }
            //ViewBag.Deliveries = repository.GetAllDeliverues().Select(d => new SelectListItem { Text = d.title, Value = d.id.ToString()});
            ViewBag.Deliveries = repository.GetAllDeliverues();

            List<ItemWithInfo> items = new List<ItemWithInfo>();
            foreach (Order o in orders)
                items.Add(repository.GetItemWithInfo(o.Item.id));
            ViewBag.Items = items;

            return View(CheckoutedOrders);
        }

        public ActionResult OnOrder(List<int> orderId, List<String> deliveries, List<int> count, String name = "name", String email = "email")
        {
            if (User.Identity.IsAuthenticated)
            {
                int i = 0;
                foreach (int id in orderId)
                {
                    repository.SetOrderDelivery(id, int.Parse(deliveries[i]));
                    repository.SetOrderState(id, (int)OrderState.OnOrder, count[i++]);
                }
            }
            else
            {
                List<Order> orders = null;
                if(Session["OrderList"] != null)
                {
                    int i = 0;
                    orders = (List<Order>)Session["OrderList"];
                    Guid g;
                    g = Guid.NewGuid();
                    g = Guid.NewGuid();
                    g = Guid.NewGuid();
                    g = Guid.NewGuid();

                    AspNetUsers user = new AspNetUsers()
                    {
                        Id = g.ToString(),
                        Email = email,
                        UserName = name,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        AccessFailedCount = 0,
                        LockoutEnabled = true,
                        TwoFactorEnabled = false
                    };

                    List<string> indexes = repository.GetUserIndexes();

                    foreach(Order order in orders)
                    {
                        if (orderId.Any(oId => oId == orders[i].id))
                        {
                            AspNetUsers addedUser = repository.SerUserForOrder(user);
                            order.userId = repository.GetUserId(addedUser.Email);
                            repository.AddOrder(order);
                            repository.SetOrderState(order.id, (int)OrderState.OnOrder, count[i]);
                        }
                        i++;
                    }
                    
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public ActionResult DeleteOrder(int orderId = 0)
        {
            if(User.Identity.IsAuthenticated)
            {
                repository.DeleteOrder(orderId);
            }
            else
            {
                List<Order> orders = null;
                if (Session["OrderList"] != null)
                {
                    orders = (List<Order>)Session["OrderList"];
                    Order order = orders.Where(o => o.id == orderId).FirstOrDefault();
                    if (order != null)
                        orders.Remove(order);
                    Session["OrderList"] = orders;
                }
            }

            return RedirectToAction("Cart");
        }

        [Authorize]
        [HttpGet]
        public ActionResult OnOrder()
        {
            ViewBag.Title = "Оформленные заказы";
            SetOrderesCount();
            ViewBag.isCartView = true;
            ViewBag.Orders = repository.GetOrdersByUser(User.Identity.Name, OrderState.OnOrder.ToString());
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult History()
        {
            ViewBag.Title = "История";
            SetOrderesCount();
            ViewBag.isCartView = true;
            ViewBag.Orders = repository.GetOrdersByUser(User.Identity.Name, OrderState.Delivered.ToString());
            return View();
        }
    }
}