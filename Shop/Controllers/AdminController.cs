using Shop.Global;
using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;

namespace Shop.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Orders()
        {
            ViewBag.Title = "Админ панель";
            ViewBag.Orders = repository.GetOrders();

            ViewBag.isRedactorView = true;
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult Orders(List<int> list)
        {
            foreach(int i in list)
            {
                repository.SetOrderState(i, (int)OrderState.Delivered);
            }
            return RedirectToAction("Orders");
        }

        public ActionResult Search(string value)
        {
            ViewBag.Title = "Админ панель";
            ViewBag.SearchParams = value;
            ViewBag.isRedactorView = true;
            ViewBag.Orders = repository.SearchOrders(value);
            return View("Orders");
        }

        public ActionResult ClearSearchParams()
        {
            ViewBag.SearchParams = null;
            return RedirectToAction("Orders", (int)OrderState.OnOrder);
        }

        [HttpGet]
        public ActionResult Items()
        {
            ViewBag.Title = "Админ панель";
            ViewBag.isRedactorView = true;
            return View(repository.GetItemWithInfo());
        }

        [HttpGet]
        public ActionResult Categories()
        {


            ViewBag.Title = "Список категорий";
            ViewBag.isRedactorView = true;
            return View(repository.GetCategories());
        }

        public ActionResult DeleteCategory(int id = 0)
        {
            repository.DeleteCategory(id);

            return RedirectToAction("Categories");
        }

        [HttpGet]
        public ActionResult EditCategory(int id = 0)
        {
            ViewBag.Categories = repository.GetCategories();
            ViewBag.Parents = repository.GetPatentCategories(id);
            ViewBag.Children = repository.GetChildrenCategories(id);

            ViewBag.Title = "Редактор категории";
            ViewBag.isRedactorView = true;
            return View(repository.GetCategory(id));
        }

        [HttpPost]
        public ActionResult EditCategory(Category category, List<int> Parents, List<int> Children)
        {
            repository.SetCategoryCahges(category, Parents, Children);

            return RedirectToAction("Categories");
        }

        [HttpGet]
        public ActionResult CreateCategory()
        {
            ViewBag.Categories = repository.GetCategories();
            ViewBag.Parents = new List<Category>();
            ViewBag.Children = new List<Category>();

            ViewBag.Title = "Создание категории";
            ViewBag.isRedactorView = true;
            return View(new Category()
            {
                title = ""
            });
        }

        [HttpPost]
        public ActionResult CreateCategory(Category category, List<int> Parents, List<int> Children)
        {
            repository.AddCategory(category);
            repository.SetCategoryCahges(category, Parents, Children);

            return RedirectToAction("categories");

        }

        public ActionResult DeleteSelectedCategories(List<int> list)
        {
            foreach (int id in list)
                repository.DeleteCategory(id);
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public ActionResult Suppliers()
        {

            ViewBag.Title = "Админ панель";
            ViewBag.isRedactorView = true;
            return View();
        }

        [HttpGet]
        public ActionResult Prices()
        {

            ViewBag.Title = "Админ панель";
            ViewBag.isRedactorView = true;
            return View();
        }

        [HttpGet]
        public ActionResult Deliveries()
        {

            ViewBag.Title = "Админ панель";
            ViewBag.isRedactorView = true;
            return View();
        }

        [HttpGet]
        public ActionResult Details()
        {

            ViewBag.Title = "Админ панель";
            ViewBag.isRedactorView = true;
            return View();
        }

        [HttpGet]
        public ActionResult EditItem(int id = 0)
        {
            Item item = repository.GetItem(id);
            if (item == null)
                RedirectToAction("Items");

            ViewBag.Title = "Админ панель";
            ViewBag.Categories = repository.GetCategories();

            ViewBag.isRedactorView = true;
            return View(item);
        }

        [HttpPost]
        public ActionResult EditItem(Item item, HttpPostedFileBase imageFile)
        {
            if(imageFile != null)
            {
                string fileName = System.IO.Path.GetFileName(imageFile.FileName);
                imageFile.SaveAs(Server.MapPath("~/Files/" + fileName));
                item.image = fileName;
            }
            item.categoryId = item.Category.id;
            repository.SetItemChanges(item);
            
            return RedirectToAction("Items");
        }

        [HttpGet] 
        public ActionResult DeleteItem(int id = 0)
        {
            repository.DeleteItem(id);
            
            return RedirectToAction("Items");
        }

        [HttpGet]
        public ActionResult CreateItem()
        {
            ViewBag.Title = "Создание товара";
            ViewBag.Categories = repository.GetCategories();

            ViewBag.isRedactorView = true;
            return View(new Item(){
                description = "",
                title = "",
                image = ""
            });
        }

        [HttpPost]
        public ActionResult CreateItem(Item item, HttpPostedFileBase imageFile)
        {
            string fileName = System.IO.Path.GetFileName(imageFile.FileName);
            imageFile.SaveAs(Server.MapPath("~/Files/" + fileName));
            item.image = fileName;
            item.categoryId = item.Category.id;
            item.Category = null;
            repository.AddItem(item);
            
            return RedirectToAction("Items");
        }

        [HttpPost]
        public ActionResult DeleteRangeItems(List<int> list)
        {
            foreach(int id in list)
            {
                repository.DeleteItem(id);
            }
            return RedirectToAction("Items");
        }

        [HttpGet]
        public ActionResult LoadCatalogOfItems()
        {
            ViewBag.Title = "Создание категории";
            ViewBag.isRedactorView = true;
            return View();
        }

        [HttpPost]
        public ActionResult LoadCatalogOfItems(HttpPostedFileBase file)
        {
            List<Item> list = new List<Item>();

            Excel.Application newApp = new Excel.Application();
            Excel.Workbook xlsBook = null;
            try
            {
                file.SaveAs(Server.MapPath("~/Files/" + file.FileName));
                xlsBook = newApp.Workbooks.Open(Server.MapPath("~/Files/" + file.FileName));
                Excel._Worksheet xlsSheet = xlsBook.Sheets[1];
                Excel.Range xlRange = xlsSheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                for(int i = 1; i <= rowCount; i++)
                {
                    string partNumber = xlRange.Cells.Item[i][1].Value;
                    string cathegoryId = xlRange.Cells.Item[i][2].Value;
                    string title = xlRange.Cells.Item[i][3].Value;
                    string description = xlRange.Cells.Item[i][4].Value;
                    string image = xlRange.Cells.Item[i][5].Value;

                    if (String.Compare(partNumber, "") == 0)
                        break;


                }
                string s = xlRange.Cells.Item[1][1].Value;

                /*
            Артикул
            Код Категории
            Название
            Описание
            Изображение
                  */
            }
            finally
            {
                if(xlsBook != null)
                    xlsBook.Close();
                newApp.Quit();
            }
            ViewBag.Title = "Создание категории";
            ViewBag.isRedactorView = true;
            return View();
        }
    }
}
