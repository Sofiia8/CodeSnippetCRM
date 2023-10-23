using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Website.Data;
using Website.Models;

namespace Website.Controllers
{
    /// <summary>
    /// Основной контроллер, отвечающий за заявки клиентов, в его конструкторе через внедрение зависмостей получаем репозиторий
    /// заявок и репозиторий настроек. В репозитории настроек хранятся наименования кнопок и полей для лендинговой страницы сайта
    /// для клиентов. Все эти настройки можно изменять в администраторской версии сайта.
    /// Можно получить заявки для определенного интервала времени, можно создать новую заявку, можно изменить статус заявки
    /// </summary>
    public class RequestController : Controller
    {
        private readonly IRepository<ApplicationView> _repositoryApplications;
        private readonly IRepositorySettings _repositorySettings;
        private static Dictionary<int, string> applicationStatus = new Dictionary<int, string>()
        {
            { 1, "Получена" },
            { 2, "В работе" },
            { 3, "Выполнена" },
            { 4, "Отклонена" },
            { 5, "Отменена" }
        };
        public RequestController(IRepository<ApplicationView> repositoryApplications, IRepositorySettings repositorySettings)
        {
            _repositoryApplications = repositoryApplications;
            _repositorySettings = repositorySettings;
        }

        // GET: RequestController
        public async Task<IActionResult> ApplicationsAdmin()
        {
            return RedirectToAction("GetItemsFromToDates", "Request", new
            {
                dateTimeFrom = DateTime.Today.AddDays(-1),
                dateTimeTo = DateTime.Today
            });
        }

        // GET: RequestController/GetItemsFromToDates
        [HttpGet]
        public async Task<IActionResult> GetItemsFromToDates(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            string? jwt = Request.Cookies["jwt"];
            if (jwt == null) return Redirect("/Auth/Login");

            var allItems = await _repositoryApplications.GetItems(jwt);
            if (allItems == null)
            {
                return RedirectToAction("NotSuccess", "Auth", new { errors = "Сервис не доступен." });
            }
            ViewBag.countAll = allItems.Count();

            ViewBag.dateFrom = dateTimeFrom.ToString("yyyy-MM-dd");
            ViewBag.dateTo = dateTimeTo.ToString("yyyy-MM-dd");
            var answer = await _repositoryApplications.GetItemsFromToDates(dateTimeFrom, dateTimeTo, jwt);
            if (answer != null)
            {
                ViewBag.count = answer.Count();
                ViewBag.dictionary = applicationStatus;
                return View("ApplicationsAdmin", answer);
            }
            else
                return RedirectToAction("NotSuccess", "Auth", new { errors = "Сервис не доступен" });

        }

        // GET: RequestController/Create
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                var settings = await _repositorySettings.GetMainSettings();
                if (settings == null)
                {
                    return RedirectToAction("NotSuccess", "Auth", new { errors = "Нет соединения с сервером" });
                }
                var appSetView = new AppSettingView();
                var fields = typeof(AppSettingView).GetProperties();

                foreach (var field in fields)
                {
                    field?.SetValue(appSetView, settings[field.Name]);
                }
                return View("StartPageForm", appSetView);
            }
            else
                return RedirectToAction("ApplicationsAdmin");
        }

        // POST: RequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string name, string email, string description)
        {
            var result = await _repositoryApplications.PostNewItem(
                new ApplicationView
                {
                    Datetime = DateTime.Now,
                    Name = name,
                    Email = email,
                    Description = description,
                });
            if (result != HttpStatusCode.Created && result != HttpStatusCode.OK)
            {
                return RedirectToAction("NotSuccess", "Auth", new { errors = "Не удалось отправить заявку" });
            }
            return Redirect("~/");
        }

        // POST: RequestController/EditStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditStatus(int id, string status, DateTime dateFrom, DateTime dateTo)
        {
            string? jwt = Request.Cookies["jwt"];
            if (jwt == null) return Redirect("/Auth/Login");

            var result = await _repositoryApplications.EditRecord(id, status, jwt);
            if (result != HttpStatusCode.OK)
            {
                return RedirectToAction("NotSuccess", "Auth", new { errors = "Не удалось изменить запись, ошибка" });
            }
            return RedirectToAction("GetItemsFromToDates", "Request", new
            {
                dateTimeFrom = dateFrom,
                dateTimeTo = dateTo
            });
        }
    }
}
