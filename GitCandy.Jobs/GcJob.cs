using GitCandy.Base;
using GitCandy.Log;
using GitCandy.Schedules;
using System;

namespace GitCandy.Jobs
{
    public class GcJob : IJob
    {
        public GcJob()
        {
            Logger.Info("Init GcJob");
        }

        public void Execute(JobContext jobContext)
        {
            if (jobContext.ExecutionTimes == 0)
                return;

            Logger.Info("TotalMemory {0}, Generation0 {1}, Generation1 {2}, Generation2 {3}",
                FileHelper.GetSizeString(GC.GetTotalMemory(true)), GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            GC.Collect();
            GC.WaitForFullGCComplete();
            Logger.Info("TotalMemory {0}, Generation0 {1}, Generation1 {2}, Generation2 {3}",
                FileHelper.GetSizeString(GC.GetTotalMemory(true)), GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }

        public TimeSpan GetNextInterval(JobContext jobContext)
        {
            return TimeSpan.FromSeconds(24 * 3600 + 1) - DateTime.Now.TimeOfDay;
        }

        public TimeSpan Due
        {
            get { return TimeSpan.FromSeconds(1); }
        }
    }
}
