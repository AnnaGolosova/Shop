using Shop.Models;
using Shop.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.AspNet.Identity;
using System.IO;

namespace Shop.Controllers
{
    public class HomeController : BaseController
    {

        public class Relation
        {
            public Category child;
            public Category parent;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Index()
        {
            SetOrderesCount();
            ViewBag.Items = repository.GetTopItems(repository.GetDetails());
            ViewBag.mianCategories = repository.GetMainCategories();
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Item(int id = 0)
        {
            ItemWithInfo curItem;
            curItem = repository.GetItemWithInfo(id);
            if (curItem != null)
            {
                repository.SetVisit(User.Identity.IsAuthenticated ? repository.GetUserId(User.Identity.Name) : null, id);
            }
            SetOrderesCount();
            return View(curItem);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Catalog(int categoryId = 0)
        {
            SetOrderesCount();
            List<Category> rout = new List<Category>();
            if (Session["NavRout"] != null)
            {
                rout = (List<Category>)Session["NavRout"];
            }
            else
            {
                rout.Clear();
                rout.Add(new Category() { title = "Каталог", id = 0 });
            }

            List<DetailsTypeWithValues> Filters = repository.GetDetails();
            if (Session["Filters"] != null)
                Filters = (List<DetailsTypeWithValues>)Session["Filters"];
            else
            {
                Session["Filters"] = Filters;
            }
            if (repository.GetCategory(categoryId) != null)
                rout.Add(repository.GetCategory(categoryId));
            if (categoryId == 0)
            {
                rout.Add(new Category() { title = "Каталог", id = 0 });
                ViewBag.Categories = repository.GetMainCategories();
            } else
                ViewBag.Categories = repository.GetCategories(categoryId);
            Session["NavRout"] = repository.CheckRout(rout);
            ViewBag.Items = repository.GetItemsByCategory(categoryId, Filters);
            ViewBag.Details = Filters;
            SetOrderesCount();
            string[] model = new string[] {"ff", "dfdf"};

            return View(model);
        }

        public ActionResult About()
        {
            SetOrderesCount();
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            SetOrderesCount();
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult SetFilters(string[] checkedValues)
        {
            List<DetailsTypeWithValues> Filters = (List<DetailsTypeWithValues>)Session["Filters"];
            foreach (var v in Filters)
                foreach (var vv in v.values)
                    vv.inUse = false; 
            foreach(string s in checkedValues)
            {
                Filters.All(f => f.values.Where(v => v.value.id == int.Parse(s)).All(fl => fl.inUse = true));
            }
            Session["Filters"] = Filters;
            return RedirectToAction("Catalog", "Home");
        }

        [HttpGet]
        public FileResult PrintExcel()
        {
            Excel.Application newApp = new Excel.Application();
            Excel.Worksheet currSheet;
            //string filePath = "~/Files/ExcelReport" + (User.Identity.IsAuthenticated ? User.Identity.Name : "Guest") + DateTime.Now.ToString() + ".xls";
            string filePath = Server.MapPath( "/Files") + (User.Identity.IsAuthenticated ? User.Identity.Name : "Guest") 
                + DateTime.Now.Ticks.ToString() + ".xls";
                newApp.Workbooks.Add(Type.Missing);
                currSheet = (Excel.Worksheet)newApp.Worksheets[1];
                int i = 1;
                List<ItemWithInfo> items= (List<ItemWithInfo>)Session["Items"];
                currSheet.Cells[1][1] = "Артикул";
                currSheet.Cells[2][1] = "Название";
                currSheet.Cells[3][1] = "Описание";
                currSheet.Cells[4][1] = "Категория";
                currSheet.Cells[5][1] = "Поставщик";
                currSheet.Cells[6][i++] = "Цена";
                foreach (ItemWithInfo item in items)
                {
                    currSheet.Cells[1][i] = item.Item.partNumber;
                    currSheet.Cells[2][i] = item.Item.title;
                    currSheet.Cells[3][i] = item.Item.description;
                    currSheet.Cells[4][i] = item.Item.Category.title;
                    foreach(Price price in item.Price)
                    {
                        currSheet.Cells[5][i] = price.Supplier.title;
                        currSheet.Cells[6][i++] = price.price;
                    }
                    
                }
                currSheet.SaveAs(filePath, Excel.XlSaveAsAccessMode.xlNoChange);
                newApp.Workbooks[1].Close();
                newApp.Quit();
            string file_type = "application/excel";
            string file_name ="Price list.xls";
            return File(filePath, file_type, file_name);
        }
    }
}