using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DesktopWpfLib.Data
{
    /// <summary>
    /// Интерфейс, описывающий методы, необходимые для работы контроллера Заявок (RequestController)
    /// </summary>
    public interface IRepository<T>
    {
        Task<ObservableCollection<T>> GetItems(string jwt);
        Task<ObservableCollection<T>> GetItemsFromToDates(DateTime dateTimeFrom, DateTime dateTimeTo, string jwt);
        Task<HttpStatusCode> PostNewItem(T application);
        Task<HttpStatusCode> EditRecord(int id, string status, string jwt);
    }
}
