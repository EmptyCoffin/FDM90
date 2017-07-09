using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models.Helpers
{
    public static class DateHelper
    {

        public static DateTime[] GetDates(DateTime startDate, DateTime endDate, bool includeCurrentWeek = true)
        {
            List<DateTime> dateList = new List<DateTime>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date < DateTime.Now.AddDays(includeCurrentWeek ? 0 : -7).Date)
                    dateList.Add(date);
            }
            return dateList.ToArray();
        }
    }
}