using FDM90.Handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SchedulerService
{
    public partial class SchedulerCheck : ServiceBase
    {
        Timer quarterlyTimer;
        Timer setupTimer;
        private ISchedulerHandler _schedulerHandler;

        public SchedulerCheck():this(new SchedulerHandler())
        {

        }

        public SchedulerCheck(ISchedulerHandler schedulerHandler)
        {
            _schedulerHandler = schedulerHandler;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            var quarterOfHour = Math.Round(DateTime.Now.Minute / 15.0);
            var differenceInMilliseconds = TimeSpan.FromMinutes((quarterOfHour == 0 ? 60 : 15 * quarterOfHour) - DateTime.Now.Minute).TotalMilliseconds;
            setupTimer = new Timer(differenceInMilliseconds);
            setupTimer.Elapsed += new ElapsedEventHandler(SetupTimer);
            setupTimer.Enabled = true;
            setupTimer.Start();
        }

        public void SetupTimer(object sender, ElapsedEventArgs e)
        {
            setupTimer.Enabled = false;
            setupTimer.Stop();
            // start 15 minute timer
            quarterlyTimer = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
            quarterlyTimer.Elapsed += new ElapsedEventHandler(RunQuarterlyUpdate);
            quarterlyTimer.Enabled = true;
            quarterlyTimer.Start();
        }

        public void RunQuarterlyUpdate(object sender, ElapsedEventArgs e)
        {
            Debugger.Launch();
            _schedulerHandler.SchedulerPostsForTime(DateTime.Now);
        }

        protected override void OnStop()
        {
            quarterlyTimer.Stop();
        }
    }
}
