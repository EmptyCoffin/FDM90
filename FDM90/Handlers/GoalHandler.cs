using FDM90.Models;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FDM90.Handlers
{
    public class GoalHandler : IGoalHandler
    {
        private IRepository<Goals> _goalRepo;
        private IReadMultipleSpecific<Goals> _goalReadMultipleRepo;
        private IFacebookHandler _facebookHandler;

        public GoalHandler() : this(new GoalRepository(), new FacebookHandler())
        {

        }

        public GoalHandler(IRepository<Goals> goalRepo, IFacebookHandler facebookHandler)
        {
            _goalRepo = goalRepo;
            _goalReadMultipleRepo = (IReadMultipleSpecific<Goals>)goalRepo;
            _facebookHandler = facebookHandler;
        }

        public void CreateGoal(Guid userId, string name, string weekStart, string weekEnd, string targets)
        {
            Goals newGoal = new Goals() {
                UserId = userId,
                GoalName = name,
                StartDate = DateTime.Parse(weekStart),
                EndDate = DateTime.Parse(weekEnd),
                Targets = targets
            };

            //newGoal.StartDate = calendar.GetWeekOfYear(DateTime.Parse(weekStart), dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            //newGoal.WeekEnd = calendar.GetWeekOfYear(DateTime.Parse(weekEnd), dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            _goalRepo.Create(newGoal);
        }

        public void UpdateGoals(Guid userId)
        {
            foreach (Goals goal in GetUserGoals(userId))
            {
                Task.Factory.StartNew(() =>
                {
                    _facebookHandler.GetGoalInfo(userId, goal.StartDate, goal.EndDate);
                });
            }
        }

        public IEnumerable<Goals> GetUserGoals(Guid userId)
        {
            return _goalReadMultipleRepo.ReadMultipleSpecific(userId.ToString());
        }
    }
}