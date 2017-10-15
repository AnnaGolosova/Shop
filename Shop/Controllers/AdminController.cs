using Shop.Global;
using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Diagnostics;
using System.Collections;

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

            //ViewBag.isRedactorView = true;
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
            Excel.Workbook xlsBook = null;
            Excel.Workbooks xlsBooks = null;
            Excel.Application newApp = null;
            Excel._Worksheet xlsSheet = null;
            Hashtable myHashtable  = null;
            try
            {

                Process[] AllProcesses = Process.GetProcessesByName("excel");
                myHashtable = new Hashtable();
                int iCount = 0;

                foreach (Process ExcelProcess in AllProcesses)
                {
                    myHashtable.Add(ExcelProcess.Id, iCount);
                    iCount = iCount + 1;
                }

                List<Item> list = new List<Item>();
                List<bool> results = new List<bool>();
                List<List<string>> values = new List<List<string>>();

                newApp = new Excel.Application();
                file.SaveAs(Server.MapPath("~/Files/" + file.FileName));
                xlsBooks = newApp.Workbooks;
                xlsBook = xlsBooks.Open(Server.MapPath("~/Files/" + file.FileName));
                xlsSheet = xlsBook.Sheets[1];
                Excel.Range xlRange = xlsSheet.UsedRange;
                int rowCount = xlRange.Rows.Count;
                for(int i = 1; i <= rowCount; i++)
                {
                    values.Add(new List<string>());
                    if (xlRange.Cells.Item[1][i] != null)
                        values[i - 1].Add(xlRange.Cells.Item[1][i].Value.ToString());
                    else values[i - 1].Add(String.Empty);
                    if (xlRange.Cells[2][i].Value != null)
                        values[i - 1].Add(xlRange.Cells[2][i].Value.ToString());
                    else values[i - 1].Add(String.Empty);
                    if (xlRange.Cells[3][i].Value != null)
                        values[i - 1].Add(xlRange.Cells.Item[3][i].Value.ToString());
                    else values[i - 1].Add(String.Empty);
                    if (xlRange.Cells[4][i].Value != null)
                        values[i - 1].Add(xlRange.Cells.Item[4][i].Value.ToString());
                    else values[i - 1].Add(String.Empty);
                    if (xlRange.Cells[5][i].Value != null)
                        values[i - 1].Add(xlRange.Cells.Item[5][i].Value.ToString());
                    else values[i - 1].Add(String.Empty);

                    if (String.Compare(values[i-1][0], "") == 0)
                        break;
                    results.Add(false);
                    Item item = new Item();
                    int pN = 0;
                    if (!int.TryParse(values[i - 1][0], out pN))
                        continue;
                    item.partNumber = pN;

                    int cId = 0;
                    if (int.TryParse(values[i - 1][1], out cId))
                    {
                        if (repository.GetCategory(cId) != null)
                            item.categoryId = cId;
                    }
                    else if (repository.GetCategories().Any(c => String.Compare(c.title, values[i - 1][1]) == 0))
                            item.categoryId = repository.GetCategories()
                                .Where(c => String.Compare(c.title, values[i - 1][1]) == 0).FirstOrDefault().id;
                    else continue;
                    item.title = values[i - 1][2];
                    item.description = values[i - 1][3];
                    item.image = values[i - 1][4];

                    results[results.Count - 1] = true;
                    list.Add(item);
                }

                repository.AddItems(list);
                ViewBag.CorrectFlags = results;

                ViewBag.Cells = values;

                ViewBag.Title = "Создание категории";
                ViewBag.isRedactorView = true;
                return View(values);
            }
            finally
            {
                Process[] AllProcesses = Process.GetProcessesByName("excel");

                // check to kill the right process
                foreach (Process ExcelProcess in AllProcesses)
                {
                    if (myHashtable.ContainsKey(ExcelProcess.Id) == false)
                        ExcelProcess.Kill();
                }

                AllProcesses = null;
            }
        }

        public ActionResult Statistic()
        {



            return View();
        }
    }
}
