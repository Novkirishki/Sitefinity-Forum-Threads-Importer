using Quartz;
using Quartz.Impl;
using System;

namespace ForumThreadsImporter
{
    class Program
    {

        static void Main(string[] args)
        {
            CreateScheduler();
            Console.ReadLine();
        }

        private static async void CreateScheduler()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<ImportJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .StartNow()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(9, 30))
                  )
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
