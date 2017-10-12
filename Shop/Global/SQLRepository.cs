using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shop.Models;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace Shop.Global
{
    public class SQLRepository
    {
        private static Entity db;
        private static UserEntity Userdb;

        public SQLRepository()
        {
            db = new Entity();
            Userdb = new UserEntity();
        }

        public List<AspNetRoles> GetUserRoles(string username)
        {
            IEnumerable<AspNetUsers> user;
            user = Userdb.AspNetUsers.Where(x => string.Compare(x.Email, username, false) == 0);
            return user.FirstOrDefault().AspNetRoles.ToList();
        }

        public AspNetUsers GetUser(string login)
        {
            IEnumerable<AspNetUsers> user;
            user = Userdb.AspNetUsers.Where(x => string.Compare(x.Email, login, false) == 0).AsNoTracking();
            return user.FirstOrDefault();
        }

        public ItemWithInfo GetItemWithInfo(int id)
        {
            var curItem = db.Item.Where(x => x.id == id).Select(x => new ItemWithInfo
            {
                Item = x,
                Price = db.Price.Where(p => p.partNumber == x.partNumber).ToList(),
                DetailsLink = db.DetailsLink.Where(d => d.itemId == x.id).ToList(),
                Order = db.Order.Where(o => o.itemId == x.id).ToList()
            });
            return curItem.AsNoTracking().FirstOrDefault();
        }

        public List<ItemWithInfo> GetItemWithInfo()
        {
            List<ItemWithInfo> Item = db.Item.Select(x => new ItemWithInfo
            {
                Item = x,
                Price = db.Price.Where(p => p.partNumber == x.partNumber).ToList(),
                DetailsLink = db.DetailsLink.Where(d => d.itemId == x.id).ToList(),
                Order = db.Order.Where(o => o.itemId == x.id).ToList()
            }).AsNoTracking().ToList();
            return Item;
        }

        public List<Category> GetMainCategories()
        {
            return db.Category.Where(c => !db.CategoriesLink.Any(cl => cl.childId == c.id)).ToList();
        }

        public List<Category> GetCategories(int id = 0)
        {
            if (id == 0)
                return db.Category.AsNoTracking().ToList();
            if(db.CategoriesLink.Any(cl => cl.Parent.id == id))
                return db.CategoriesLink.Where(cl => cl.Parent.id == id).Select(cl => cl.Child).AsNoTracking().ToList();
            return null;
        }

        public List<Order> GetOrdersByUser(string userName, string fieldName)
        {
            if (!Userdb.AspNetUsers.Any(u => string.Compare(u.Email, userName) == 0))
            {
                return null;
            }
            string userId = Userdb.AspNetUsers.Where(u => string.Compare(u.Email, userName) == 0).AsNoTracking().FirstOrDefault().Id;
            foreach (var state in Enum.GetValues(typeof(OrderState)))
            {
                if (((OrderState)state).ToString() == fieldName)
                {
                    return db.Order.Where(o => string.Compare(o.userId, userId) == 0)
                        .Where(o => o.state == (int)((OrderState)state))
                        .OrderBy(o => o.date)
                        .AsNoTracking().ToList();
                }
            }
            return db.Order.Where(o => string.Compare(o.userId, userId) == 0).AsNoTracking().ToList();
        }

        public bool ExistOdrers(string userName)
        {
            if (Userdb.AspNetUsers.Any(u =>string.Compare(u.Email, userName) == 0))
            {
                string userId = Userdb.AspNetUsers.Where(u => string.Compare(u.Email, userName) == 0).AsNoTracking().FirstOrDefault().Id;
                if (db.Order.Any(o => string.Compare(o.userId, userId)==0))
                {
                    return (db.Order.Where(o => string.Compare(o.userId, userId) == 0)
                            .Where(o => o.state == (int)OrderState.InCart).Count() > 0);
                }
            }
            return false;
        }

        public string GetUserId(string name)
        {
            if (Userdb.AspNetUsers.Any(u => string.Compare(u.Email, name) == 0))
            {
                return Userdb.AspNetUsers.Where(u => string.Compare(u.Email, name) == 0).Select(u => u.Id).AsNoTracking().FirstOrDefault();
            }
            return null;
        }

        public void SetOrderDelivery(int id, int deliveryId)
        {
            db.Order.Where(o => o.id == id).AsNoTracking().FirstOrDefault().deliveryId = deliveryId;
            db.Entry(db.Order.Where(o => o.id == id).FirstOrDefault()).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Item GetItem(int itemId)
        {
            if (db.Item.Any(o => o.id == itemId))
            {
                return db.Item.Where(i => i.id == itemId).AsNoTracking().FirstOrDefault();
            }
            return null;
        }

        public Price GetPrice(int priceId)
        {
            if (db.Price.Any(o => o.id == priceId))
            {
                return db.Price.Where(i => i.id == priceId).AsNoTracking().FirstOrDefault();
            }
            return null;
        }

        public List<Delivery> GetAllDeliverues()
        {
            return db.Delivery.AsNoTracking().ToList();
        }

        public Delivery GetDelivery(int id)
        {
            if (db.Delivery.Any(o => o.id == id))
            {
                return db.Delivery.Where(i => i.id == id).AsNoTracking().FirstOrDefault();
            }
            return null;
        }

        public void AddOrder(Order order)
        {
            //try
            {
                if (!(db.Order.Any(o => (o.itemId == order.Item.id)
                && (String.Compare(o.userId, order.userId) == 0)
                && (String.Compare(o.supplierTitle, order.supplierTitle) == 0)
                && (o.state == (int)OrderState.InCart))))
                {
                    db.Entry(order).State = EntityState.Added;
                    db.Order.Add(order);
                    db.SaveChanges();
                }
            }
            //catch (Exception ex)
            {
                //throw new SQLException();
            }
        }

        public void AddOrder(List<Order> Order)
        {
            db.Order.AddRange(Order);
            db.SaveChanges();
        }

        public float GetSum(string name)
        {
            if (db.Order
                .Any(o => string.Compare(o.userId, Userdb.AspNetUsers.Where(u => string.Compare(u.UserName, name) == 0).AsNoTracking().FirstOrDefault().Id) == 0
                && o.state == (int)OrderState.InCart))
            {
                return (float)db.Order
                .Where(o => string.Compare(o.userId, Userdb.AspNetUsers.Where(u => string.Compare(u.UserName, name) == 0).AsNoTracking().FirstOrDefault().Id) == 0 && o.state == (int)OrderState.InCart)
                .Sum(o => o.price);
            }
            return 0F;
        }

        public List<Visit> GetVisits()
        {
            return db.Visit.AsNoTracking().ToList();
        }

        public void SetVisit(string userid, int itemid)
        {
            Visit visit = new Visit()
            {
                userId = userid,
                itemId = itemid,
                date = DateTime.Now
            };
            db.Entry(visit).State = EntityState.Added;
            db.SaveChanges();
        }

        public List<ItemWithInfo> GetTopItems(List<DetailsTypeWithValues> Filters, int count = 20)
        {

            IEnumerable<ItemWithInfo> Items = db.Item.Select(x => new ItemWithInfo
            {
                Item = x,
                Price = db.Price.Where(p => p.partNumber == x.partNumber).ToList(),
                DetailsLink = db.DetailsLink.Where(d => d.itemId == x.id).ToList(),
            }).OrderByDescending(x => db.Visit.Where(v => v.itemId == x.Item.id).Count()).Take(count);
            return FilteredItems(Items.ToList(), Filters);
        }

        public Category GetCategory(int id)
        {
            return db.Category.Where(c => c.id == id).AsNoTracking().FirstOrDefault();
        }

        public List<ItemWithInfo> GetItemsByCategory(int categoryId, List<DetailsTypeWithValues> Filters, List<ItemWithInfo> currentList = null)
        {
            if (categoryId == 0)
                return GetTopItems(Filters);
            if (currentList == null)
                currentList = new List<ItemWithInfo>();
            currentList.AddRange(db.Item
                .Where(i => i.categoryId == categoryId)
                .Select(x => new ItemWithInfo
                {
                    Item = x,
                    Price = db.Price.Where(p => p.partNumber == x.partNumber).ToList(),
                    DetailsLink = db.DetailsLink.Where(d => d.itemId == x.id).ToList(),
                    Order = db.Order.Where(o => o.itemId == x.id).ToList()
                }).AsNoTracking().ToList());
            foreach (Category categoty in db.CategoriesLink.Where(cl => cl.parentId == categoryId).Select(cl => cl.Child))
            {
                GetItemsByCategory(categoty.id, Filters, currentList);
            }
            
            return FilteredItems(currentList, Filters);
        }

        public bool isParent(int parentId, int childId)
        {
            return db.CategoriesLink.Any(cl => cl.childId == childId && cl.parentId == parentId);
        }

        public List<Category> CheckRout(List<Category> list)
        {
            for (int i = list.Count() - 1; i >= 2; i--)
            {
                int chId = list[i].id;
                int prId = list[i - 1].id;
                if (db.CategoriesLink.Any(cl => cl.childId == chId && cl.parentId == prId))
                {
                }
                else
                {
                    list.Remove(list[i - 1]);
                }
            }
            if (list.Count() == 2 && list[0].id == list[1].id)
                list.Remove(list[1]);
            return list;
        }

        public List<DetailsTypeWithValues> GetDetails()
        {
            var res = db.DetailsType.Select(d => new DetailsTypeWithValues
            {
                type = d,
                values = db.DetailsValue.Where(dv => dv.detailsTypeId == d.id).Select( dv =>
                    new DetailsValueUse
                    {
                        inUse = false,
                        value = dv
                    }
                ).ToList()
            }).AsNoTracking().ToList();
            return res;
        }

        public List<ItemWithInfo> FilteredItems(List<ItemWithInfo> list, List<DetailsTypeWithValues> filters)
        {
            if (filters != null)
            {
                if(filters.All(f => f.values.All(v => v.inUse == false)))
                {
                    filters.All(f => f.values.All(v => v.inUse = true));
                }
                return list
                .Where(i => i.DetailsLink.All(dl => filters.Any(dv => dv.values.Any(v => v.value.id == dl.detailsValueId && v.inUse))))
                .ToList();
            }
            return list;
        }

        public void SetOrderState(int orderId, int state, int count = 0)
        {
            if(db.Order.Any(o => o.id == orderId))
            {
                Order o = db.Order.Where(or => or.id == orderId).AsNoTracking().FirstOrDefault();
                o.state = state;
                db.Entry(o).State = EntityState.Modified;
                if (state == (int)OrderState.OnOrder)
                {
                    o.count += count;
                    db.Price
                        .Where(p => p.partNumber == db.Order.Where(or => or.id == orderId).AsNoTracking().FirstOrDefault().partNumber)
                        .AsNoTracking().FirstOrDefault()
                        .count -= count;
                    db.Entry(db.Price
                        .Where(p => p.partNumber == db.Order.Where(or => or.id == orderId).AsNoTracking().FirstOrDefault().partNumber)
                        .AsNoTracking().FirstOrDefault()).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        public void DeleteOrder(int orderId)
        {
            Order order = null;
            if ((order = db.Order.Where(o => o.id == orderId).AsNoTracking().FirstOrDefault()) != null)
            {
                db.Order.Remove(order);
                db.Entry(order).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public Order GetOrder(int id)
        {
            return db.Order.Where(o => o.id == id).AsNoTracking().FirstOrDefault();
        }

        public void AddOrders(List<Order> list)
        {
            db.Order.AddRange(list);
            db.Entry(list).State = EntityState.Added;
            db.SaveChanges();
        }

        public AspNetUsers SerUserForOrder(AspNetUsers user, int userRoleId = (int)UserRoles.none)
        {
            //try
            {
                AspNetUsers us;
                user.AspNetRoles.Add(GetUserRole(userRoleId));
                if ((us = Userdb.AspNetUsers.Where(u => String.Compare(u.Email, user.Email) == 0
                             || String.Compare(u.UserName, user.UserName) == 0).AsNoTracking().FirstOrDefault()) == null)
                {
                    Userdb.AspNetUsers.Add(user);
                    Userdb.Entry(user).State = EntityState.Added;
                    Userdb.SaveChanges();
                }
                return us == null ? user : us;
            } //catch (Exception ex)
            {
                //throw null;
            }
        }

        public AspNetRoles GetUserRole(int roleCode = (int)UserRoles.none)
        {
            return Userdb.AspNetRoles.Where(r => r.Id == roleCode.ToString()).AsNoTracking().FirstOrDefault();
        }

        public List<string> GetUserIndexes()
        {
            return Userdb.AspNetUsers.Select(u => u.Id).AsNoTracking().ToList();
        }

        public List<Order> GetOrders()
        {
            return db.Order.Where(o => o.state == (int)OrderState.OnOrder).AsNoTracking().ToList();
        }

        public List<Order> SearchOrders(string value, int orderState = -1)
        {
            List<Order> or =  db.Order.AsNoTracking().Where(o => (o.Item.title.Contains(value)) || (o.Item.description.Contains(value))).ToList();
            if(orderState != -1)
            {
                or = or.Where(o => o.state == orderState).ToList();
            }
            return or;
        }

        public void SetItemChanges(Item item)
        {
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void DeleteItem(int id)
        {
            Item item = db.Item.Where(i => i.id == id).FirstOrDefault();
            if(item != null)
            {
                db.Item.Remove(item);
                db.Entry(item).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public void AddItem(Item item)
        {
            db.Item.Add(item);
            db.Entry(item).State = EntityState.Added;
            db.SaveChanges();

            // errors!
        }

        public void AddItems(List<Item>items)
        {
            foreach (Item i in items)
                AddItem(i);
        } 

        public void DeleteCategory(int id)
        {

            Category category = db.Category.Where(i => i.id == id).FirstOrDefault();
            if (category != null)
            {
                db.Category.Remove(category);
                db.Entry(category).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public void SetCategoryCahges(Category category, List<int> parents, List<int> children)
        {
            if (parents == null)
                parents = new List<int>();
            if (children == null)
                children = new List<int>();
            List<CategoriesLink> oldParenrs = db.CategoriesLink.Where(cl => cl.childId == category.id).Where(cl => !parents.Any(p => p == cl.parentId)).ToList();

            List<CategoriesLink> oldChildren = db.CategoriesLink.Where(cl => cl.parentId == category.id).Where(cl => !children.Any(p => p == cl.childId)).ToList();

            foreach (CategoriesLink cl in oldChildren)
            {
                db.CategoriesLink.Remove(cl);
                db.Entry(cl).State = EntityState.Deleted;
            }

            foreach (CategoriesLink cl in oldParenrs)
            {
                db.CategoriesLink.Remove(cl);
                db.Entry(cl).State = EntityState.Deleted;
            }

            foreach (int parentId in parents)
            {
                if (!db.CategoriesLink.Any(cl => cl.childId == category.id && cl.parentId == parentId))
                {
                    db.CategoriesLink.Add(new CategoriesLink()
                    {
                        childId = category.id,
                        parentId = parentId
                    });
                }
            }

            foreach (int childId in children)
            {
                if (!db.CategoriesLink.Any(cl => cl.childId == childId && cl.parentId == category.id))
                {
                    db.CategoriesLink.Add(new CategoriesLink()
                    {
                        childId = childId,
                        parentId = category.id
                    });
                }
            }

            db.Entry(category).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void AddCategory(Category category)
        {
            db.Category.Add(category);
            db.Entry(category).State = EntityState.Added;
            db.SaveChanges();
        }

        public List<Category> GetPatentCategories(int id)
        {
            return db.Category.AsNoTracking().Where(c => db.CategoriesLink.Any(cl => cl.childId == id && cl.parentId == c.id)).ToList();
        }

        public List<Category> GetChildrenCategories(int id)
        {
            return db.Category.AsNoTracking().Where(c => db.CategoriesLink.Any(cl => cl.parentId == id && cl.childId == c.id)).ToList();
        }
    }
}