using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;
using PagedList;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Globalization;
using System.IO;

namespace RssNewsWebApp.Controllers
{
    public class HomeController : Controller
    {
        Model1 context = new Model1();
        [HttpGet]
        public ActionResult Index(int? page, string src, string sortstyle)
        {
           
            string xmlText, FilePath = "C://Windows//Temp//rss1.xml";
            List<News> lst = new List<News>();
            List<News> lst1 = new List<News>(); //пустой список дня начального представления
            int CurPage = (page ?? 0);
            if (CurPage == 0)
            {
                
                string HabrUrl = "http://habrahabr.ru/rss/";
                string InterfaxUrl = "http://www.interfax.by/news/feed";
                int rez1 = 0, rez2 = 0;

                //Файл на ресурсе интерфакс записан с пустым символом в начале, удаляем символ из файла, потом читаем файл
                #region interfax 
                WebRequest request = WebRequest.Create(InterfaxUrl);
                try
                {
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        xmlText = reader.ReadToEnd();
                    }
                    using (StreamWriter writer = new StreamWriter(FilePath))
                    {
                        writer.Write(xmlText.Remove(0, 1));
                    }
                }
                catch (WebException)
                {

                    return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "The internet resource is not available");
                }

                #endregion   

                //Читаем новые новости и сохраняем в БД
                //Список для Хабрахабр
                List<News> habr = new List<News>();
                //Список для Интерфакс
                List<News> ifax = new List<News>();
                ReadWriteMethods.ReadRssNews(HabrUrl, 1, ref habr);
                ReadWriteMethods.ReadRssNews(FilePath, 2, ref ifax);
                rez1 = ReadWriteMethods.SaveRssNews(context, habr);
                rez2 = ReadWriteMethods.SaveRssNews(context, ifax);
                if (rez1 == -1 && rez2 == -1)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Data not are not written to the database");
                }
                //Создаем список для дропбокса
                string[] s = { "Все", "Хабрахабр", "Интерфакс" };
                ViewBag.Source = new SelectList(s);
                lst1.Clear();
                return View(lst1.ToPagedList(1, 10));
            }
            else if (src == "Хабрахабр")
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.SortStyle = "date";
                    ViewBag.CurrentSource = src;
                    ViewBag.Source = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 1
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();

                    return View(lst.ToPagedList(CurPage, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurrentSource = src;
                    ViewBag.SortStyle = "src1";
                    ViewBag.Source = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 1
                             select a);
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();
                    return View(lst.ToPagedList(CurPage, 10));
                }
            }
            else if (src == "Интерфакс")
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurrentSource = src;
                    ViewBag.SortStyle = "date";
                    ViewBag.Source = new SelectList(s);
                    
                    var l = (from a in context.NewsItems
                             where a.SourceId == 2
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();
                    
                    return View(lst.ToPagedList(CurPage, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurrentSource = src;
                    ViewBag.SortStyle = "src1";
                    ViewBag.Source = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 2
                             select a);
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();
                    
                    return View(lst.ToPagedList(CurPage, 10));
                }
            }
            else
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurrentSource = src;
                    ViewBag.SortStyle = "date";
                    ViewBag.Source = new SelectList(s);
                    
                    var l = (from a in context.NewsItems
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();
                    
                    return View(lst.ToPagedList(CurPage, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { src, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurrentSource = src;
                    ViewBag.SortStyle = "src1";
                    ViewBag.Source = new SelectList(s);
                    
                    var l = from a in context.NewsItems
                            select a;
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();
                    
                    return View(lst.ToPagedList(CurPage, 10));
                }
            }
        }

        [HttpPost]
        public ActionResult Index(string Source, int? page, string sortstyle)
        {
            string src; //передаем в GET значение текущего источника
            ViewBag.SortStyle = sortstyle;
            ViewBag.CurrentSource = Source;
            //номер страницы
            int PageNumber = (page ?? 1);

            if (Source != null)
            {
                if (Source == "Все")
                {
                    //Создаем список для дропбокса
                    string[] s = { "Все", "Хабрахабр", "Интерфакс" };
                    ViewBag.Source = new SelectList(s);
                    ViewBag.CurrentSource = "Все";
                    src = "Все";
                    page = 1;
                    return RedirectToAction("Index", new { page, src, sortstyle });
                }
                else if (Source == "Хабрахабр")
                {
                    //Создаем список для дропбокса
                    string[] s = { "Хабрахабр", "Все", "Интерфакс" };
                    ViewBag.Source = new SelectList(s);
                    ViewBag.CurrentSource = "Хабрахабр";
                    src = "Хабрахабр";
                    page = 1;
                    return RedirectToAction("Index", new { page, src, sortstyle });
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.Source = new SelectList(s);
                    ViewBag.CurrentSource = "Интерфакс";
                    src = "Интерфакс";
                    page = 1;
                    return RedirectToAction("Index", new { page, src, sortstyle });
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Something was wrong with your request");
            }
        }

        [HttpGet]
        public ActionResult AjaxView(int? p, string src, string sortstyle)
        {
            string sourc=src;            
            List<News> lst = new List<News>();
            int page = (p ?? 0);
            if (page == 0)
            {
                var lst1 = new List<News>();
                //Создаем список для дропбокса
                string[] s = { "Все", "Хабрахабр", "Интерфакс" };
                
                ViewBag.sourc = new SelectList(s);
                lst.Clear();
                return View(lst.ToPagedList(1,10));
               // return PartialView("_PartialAjaxView", all.ToPagedList(page, 10));
            }
            //else
            //{
            //    var entity = context.NewsItems.Where(x => x.RSS_source.Name == sourc).ToList();
            //    if (entity.Count != 0)
            //        return PartialView("_PartialAjaxView", entity.ToPagedList(page, 10));
            //    else
            //    {
            //        var all = context.NewsItems.ToList();
            //        return PartialView("_PartialAjaxView", all.ToPagedList(page, 10));
            //    }
            //}
            else if (sourc == "Хабрахабр")
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.Sorting = "date";
                    ViewBag.CurSource = sourc;
                    ViewBag.sourc = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 1
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();

                   
                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurSource = sourc;
                    ViewBag.Sorting = "src2";
                    ViewBag.sourc = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 1
                             select a);
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();
                   
                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
            }
            else if (sourc == "Интерфакс")
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurSource = sourc;
                    ViewBag.Sorting = "date";
                    ViewBag.sourc = new SelectList(s);
                    
                    var l = (from a in context.NewsItems
                             where a.SourceId == 2
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();

                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurSource = sourc;
                    ViewBag.Sorting = "src2";
                    ViewBag.sourc = new SelectList(s);
                    var l = (from a in context.NewsItems
                             where a.SourceId == 2
                             select a);
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();
 
                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
            }
            else
            {
                if (sortstyle == "date")
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurSource = sourc;
                    ViewBag.Sorting = "date";
                    ViewBag.sourc = new SelectList(s);
                    
                    var l = (from a in context.NewsItems
                             select a);
                    l = l.OrderByDescending(x => x.PubDate);
                    lst = l.ToList();
                    
                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { sourc, "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.CurSource = sourc;
                    ViewBag.Sorting = "src2";
                    ViewBag.sourc = new SelectList(s);

                    var l = from a in context.NewsItems
                            select a;
                    l = l.OrderBy(x => x.RSS_source.Name);
                    lst = l.ToList();

                    return PartialView("_PartialAjaxView", lst.ToPagedList(page, 10));
                }
            }



        }
        [HttpPost]
        public ActionResult AjaxView(string sourc, int? p, string sortstyle)
        {
            
            string src; //передаем в GET значение текущего источника
            ViewBag.Sorting = sortstyle;
           
            //номер страницы
            int PageNumber = (p ?? 1);

            if (sourc != null)
            {
                if (sourc == "Все")
                {
                    //Создаем список для дропбокса
                    string[] s = { "Все", "Хабрахабр", "Интерфакс" };
                    ViewBag.sourc = new SelectList(s);
                    ViewBag.CurSource = "Все";
                    src = "Все";
                    p = 1;
                    return RedirectToAction("AjaxView", new { p, src, sortstyle });
                }
                else if (sourc == "Хабрахабр")
                {
                    //Создаем список для дропбокса
                    string[] s = { "Хабрахабр", "Все", "Интерфакс" };
                    ViewBag.sourc = new SelectList(s);
                    ViewBag.CurSource = "Хабрахабр";
                    src = "Хабрахабр";
                    p = 1;
                    return RedirectToAction("AjaxView", new { p, src, sortstyle });
                }
                else
                {
                    //Создаем список для дропбокса
                    string[] s = { "Интерфакс", "Хабрахабр", "Все" };
                    ViewBag.sourc = new SelectList(s);
                    ViewBag.CurSource = "Интерфакс";
                    src = "Интерфакс";
                    p = 1;
                    return RedirectToAction("AjaxView", new { p, src, sortstyle });
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Something was wrong with your request");
            }
        }
    }
}