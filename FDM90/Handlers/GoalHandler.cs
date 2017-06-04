using FDM90.Models;
using FDM90.Repository;
using Newtonsoft.Json.Linq;
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
        private ITwitterHandler _twitterHandler;
        private IUserHandler _userHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();

        public GoalHandler() : this(new GoalRepository(), new FacebookHandler(), new TwitterHandler(), new UserHandler())
        {

        }

        public GoalHandler(IRepository<Goals> goalRepo, IFacebookHandler facebookHandler, ITwitterHandler twitterHandler, IUserHandler userHandler)
        {
            _goalRepo = goalRepo;
            _goalReadMultipleRepo = (IReadMultipleSpecific<Goals>)goalRepo;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _userHandler = userHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, twitterHandler });
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

            _goalRepo.Create(newGoal);
            UpdateGoals(userId, newGoal);
        }

        public Task UpdateGoals(Guid userId, Goals newGoal)
        {
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;

            var existingGoals = GetUserGoals(userId);
            var user = _userHandler.GetUser(userId.ToString());
            int firstWeekNumber = calendar.GetWeekOfYear(newGoal.StartDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int lastWeekNumber = calendar.GetWeekOfYear(newGoal.EndDate, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);
            int currentWeekNumber = calendar.GetWeekOfYear(DateTime.Now, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            JObject newProgress = new JObject();
            foreach (Goals goal in existingGoals.Where(x => x.StartDate <= newGoal.StartDate))
            {
                // get weeks for new goal
                JObject progress = JObject.Parse(goal.Progress);
                
                foreach(JProperty media in progress.Properties())
                {
                    JObject newMediaProgress = new JObject();
                    
                    foreach (JProperty week in media.Values())
                    {
                        if (int.Parse(week.Name.Substring(4)) >= firstWeekNumber && int.Parse(week.Name.Substring(4)) <= lastWeekNumber)
                        {
                            newMediaProgress.Add(week.Name, week.Value);
                        }
                    }
                    newProgress.Add(media.Name, newMediaProgress);
                }
            }

            List<Task> tasks = new List<Task>();

            // here when first week only less than current week, but not if we have info
            if (firstWeekNumber < currentWeekNumber && newProgress.First?.Children().Count() != currentWeekNumber - firstWeekNumber)
            {
                foreach (IMediaHandler mediaHandler in _mediaHandlers.Where(x =>
                                             bool.Parse(user.GetType().GetProperties().Where(y => y.Name == x.MediaName).First().GetValue(user).ToString())))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        foreach (JObject newWeek in mediaHandler.GetGoalInfo(userId, 
                                        newGoal.StartDate.AddDays(newProgress.First != null ? newProgress.First.Children().Count() : 0), newGoal.EndDate))
                        {
                            if ((JObject)newProgress[mediaHandler.MediaName] == null)
                            {
                                newProgress.Add(mediaHandler.MediaName, new JObject());
                            }

                            ((JObject)newProgress[mediaHandler.MediaName]).Add(newWeek.Path, newWeek);
                        }
                    }));
                }
            }
            else
            {
                tasks.Add(Task.FromResult<object>(null));
            }

            return Task.Factory.ContinueWhenAll(tasks.ToArray(), taskReturned =>
            {
                newGoal.Progress = newProgress.ToString();
                _goalRepo.Update(newGoal);
            });

        }

        public IEnumerable<Goals> GetUserGoals(Guid userId)
        {
            return _goalReadMultipleRepo.ReadMultipleSpecific(userId.ToString());
        }
    }
}