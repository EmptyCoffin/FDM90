using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static IEnumerable<DateTime> GetDatesFromWeekNumber(int weekOfYear)
        {
            try
            {
                DateTime jan1 = new DateTime(DateTime.Now.Year, 1, 1);
                int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
                DateTime firstWeekDay = jan1.AddDays(daysOffset);
                int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1,
                                                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
                {
                    weekOfYear -= 1;
                }

                return GetDates(firstWeekDay.AddDays(weekOfYear * 7), firstWeekDay.AddDays((weekOfYear * 7) + 7), false).ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}