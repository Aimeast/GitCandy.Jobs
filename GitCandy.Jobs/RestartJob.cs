using GitCandy.Base;
using GitCandy.Log;
using GitCandy.Schedules;
using System;
using System.Web;

namespace GitCandy.Jobs
{
    public class RestartJob : IJob
    {
        public RestartJob()
        {
            Logger.Info("Init GcJob");
        }

        public void Execute(JobContext jobContext)
        {
            if (jobContext.ExecutionTimes == 0)
                return;

            Logger.Info("Restart site");

            HttpRuntime.UnloadAppDomain();
        }

        public TimeSpan GetNextInterval(JobContext jobContext)
        {
            // 4:00 AM
            return TimeSpan.FromHours(22 + 4) - DateTime.Now.TimeOfDay;
        }

        public TimeSpan Due
        {
            get { return TimeSpan.FromSeconds(1); }
        }
    }
}
