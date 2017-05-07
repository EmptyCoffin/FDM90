using FDM90.Models;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FDM90.Handlers
{
    public class GoalHandler : IGoalHandler
    {
        private IRepository<Goals> _goalRepo;
        private IReadMultipleSpecific<Goals> _goalReadMultipleRepo;

        public GoalHandler() : this(new GoalRepository())
        {

        }

        public GoalHandler(IRepository<Goals> goalRepo)
        {
            _goalRepo = goalRepo;
            _goalReadMultipleRepo = (IReadMultipleSpecific<Goals>)goalRepo;
        }

        public void CreateGoal(Guid userId, string name, DateTime weekStart, DateTime weekEnd, string targets)
        {
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;
            Goals newGoal = new Goals() {
                UserId = userId,
                GoalName = name,
                Targets = targets
            };

            newGoal.WeekStart = calendar.GetWeekOfYear(weekStart, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            newGoal.WeekEnd = calendar.GetWeekOfYear(weekEnd, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            _goalRepo.Create(newGoal);
        }
    }
}