using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace RssNewsWebApp
{
    public static class ReadWriteMethods
    {
        //Метод чтения Rss новостей из всех источников
        #region ReadRss
        public static void ReadRssNews(string url, int k, ref List<News> lst)
        {
            lst.Clear(); //Очищаем список, чтобы не суммировались записи при повторном вызове 1 пункта менюю
            SyndicationFeed feed = new SyndicationFeed();
            try
            {
                using (XmlReader reader = XmlReader.Create(url)) //Создаем экземпляр
                {
                    feed = SyndicationFeed.Load(reader); //загружаем rss feed
                }
            }
            catch (Exception ex)
            {

                if (ex is WebException)
                {
                    throw new HttpException("The server is not reached");
                }
                else if (ex is XmlException)
                {
                    throw new XmlException("The error reading the file");
                }
                else
                {
                    throw new Exception("Something was wrong with reading data from source Habrahabr");
                }
            }

            foreach (SyndicationItem item in feed.Items)
            {
                string LinkUrl = null;
                string title = item.Title.Text;
                string description = item.Summary.Text;
                // Убираем не нужные теги из содержания
                description = Regex.Replace(description, @"<.+?>", String.Empty);
                description = Regex.Replace(description, @"Читать.дальше..", String.Empty);
                // Декодируем HTML сущности
                description = WebUtility.HtmlDecode(description);
                DateTime date = item.PublishDate.UtcDateTime;

                foreach (var link in item.Links)
                {
                    LinkUrl = link.Uri.ToString();
                }
                //Создаем экземпляр rss новости
                News i = new News
                {
                    SourceId = k,
                    Description = description,
                    PubDate = date,
                    URL = LinkUrl,
                    Title = title
                };
                lst.Add(i); //Добавляем в список
            }                       
        }
        #endregion

        //Метод сохранения свежих новостей в базе
        #region SaveRss
        public static int SaveRssNews(Model1 context, List<News> l)
        {           
            if (l.Count == 0)
            {
                return -1;
            }
            else
            {
                foreach (News item in l) //Поиск в бд соответсвующих записей по первичным ключам заголовка и даты
                {                  
                    News element = context.NewsItems.Find(item.PubDate, item.SourceId, item.Title);
                    if (element is null)
                    {
                        context.NewsItems.Add(item); //Если записей в бд не найдено, то добавляем новую запись в бд                      
                    }
                }
                context.SaveChanges();
                return 1;
            }
        }
        #endregion
    }
}