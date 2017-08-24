﻿using FDM90.Handlers;
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

        public UpdateService():this (new CampaignHandler())
        {

        }

        public UpdateService(ICampaignHandler campaignHandler)
        {
            _campaignHandler = campaignHandler;
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

            // start 24 hour timer
            dailyTimer = new Timer(TimeSpan.FromHours(24).TotalMilliseconds);
            dailyTimer.Elapsed += new ElapsedEventHandler(RunDailyUpdate);
            dailyTimer.Enabled = true;
            dailyTimer.Start();
        }

        public void RunDailyUpdate(object sender, ElapsedEventArgs e)
        {
            _campaignHandler.DailyUpdate();
        }
    }
}
