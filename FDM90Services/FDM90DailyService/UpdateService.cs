using FDM90.Handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace FDM90DailyService
{
    public partial class UpdateService : ServiceBase
    {
        Timer dailyTimer;
        Timer setupTimer;
        ICampaignHandler _campaignHandler;
        private IFacebookHandler _facebookHandler;
        private ITwitterHandler _twitterHandler;
        private List<IMediaHandler> _mediaHandlers = new List<IMediaHandler>();

        public UpdateService():this (new CampaignHandler(), new FacebookHandler(), new TwitterHandler())
        {

        }

        public UpdateService(ICampaignHandler campaignHandler, IFacebookHandler facebookHandler, ITwitterHandler twitterHandler)
        {
            _campaignHandler = campaignHandler;
            _facebookHandler = facebookHandler;
            _twitterHandler = twitterHandler;
            _mediaHandlers.AddRange(new IMediaHandler[] { _facebookHandler, _twitterHandler });

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // set up midnight timer from now 
            var midnightTonight = DateTime.Today.AddDays(1);
            var differenceInMilliseconds = (midnightTonight - DateTime.Now).TotalMilliseconds;
            setupTimer = new Timer(differenceInMilliseconds);
            setupTimer.Elapsed += new ElapsedEventHandler(SetupTimer);
            setupTimer.Enabled = true;
            setupTimer.Start();
        }

        protected override void OnStop()
        {
            dailyTimer.Stop();
        }

        public void SetupTimer(object sender, ElapsedEventArgs e)
        {
            setupTimer.Enabled = false;
            setupTimer.Stop();

            DailyUpdates(new object(), new EventArgs() as ElapsedEventArgs);
            // start 24 hour timer
            dailyTimer = new Timer(TimeSpan.FromHours(24).TotalMilliseconds);
            dailyTimer.Elapsed += new ElapsedEventHandler(DailyUpdates);
            dailyTimer.Enabled = true;
            dailyTimer.Start();
        }

        public void RunDailyCampaignUpdate()
        {
            _campaignHandler.DailyUpdate();
        }

        public void RunDailyMediaUpdate()
        {
            foreach(IMediaHandler mediaHandler in _mediaHandlers)
            {
                mediaHandler.DailyUpdate();
            }
        }

        public void DailyUpdates(object sender, ElapsedEventArgs e)
        {
            RunDailyCampaignUpdate();
            RunDailyMediaUpdate();
        }
    }
}
