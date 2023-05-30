namespace Quartz
{


    [Obsolete("Please use QuartzJobScheduler", true)]
    [AttributeUsage(AttributeTargets.Class)]
    public class SilkierQuartzAttribute : QuartzJobSchedulerAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class QuartzJobSchedulerAttribute : Attribute
    {
        public QuartzJobSchedulerAttribute()
        {

        }
        public QuartzJobSchedulerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, string _identity, string _desciption) : this(days, hours, minutes, seconds, milliseconds, 0, _identity, _desciption)
        {
        }

        public QuartzJobSchedulerAttribute(double hours, double minutes, double seconds, string _identity, string _desciption) : this(0, hours, minutes, seconds, 0, 0, _identity, _desciption)
        {
        }

        public QuartzJobSchedulerAttribute(double minutes, double seconds, string _identity, string _desciption) : this(0, 0, minutes, seconds, 0, 0, _identity, _desciption)
        {
        }
        public QuartzJobSchedulerAttribute(double seconds, string _identity, string _desciption) : this(0, 0, 0, seconds, 0, 0, _identity, _desciption)
        {
        }


        public QuartzJobSchedulerAttribute(double days, double hours, double minutes, double seconds, double milliseconds) : this(days, hours, minutes, seconds, milliseconds, 0, null, null)
        {
        }

        public QuartzJobSchedulerAttribute(double hours, double minutes, double seconds) : this(0, hours, minutes, seconds, 0, 0, null, null)
        {
        }

        public QuartzJobSchedulerAttribute(double minutes, double seconds) : this(0, 0, minutes, seconds, 0, 0, null, null)
        {
        }
        public QuartzJobSchedulerAttribute(double seconds) : this(0, 0, 0, seconds, 0, 0, null, null)
        {
        }
        public QuartzJobSchedulerAttribute(double minutes, double seconds, bool start_at, double start_at_minutes, double start_at_seconds) : this(0, 0, minutes, seconds, 0, 0, null, null)
        {
            if (start_at)
            {
                StartAt = DateTimeOffset.Now.AddMinutes(start_at_minutes).AddSeconds(start_at_seconds);
            }
        }
        public QuartzJobSchedulerAttribute(double seconds,bool start_at , double start_at_seconds) : this(0, 0, 0, seconds, 0, 0, null, null)
        {
            if (start_at)
            {
                StartAt = DateTimeOffset.Now.AddSeconds(start_at_seconds);
            }
        }

        public QuartzJobSchedulerAttribute(bool Manual) : this(0, 0, 0, 0, 0, 0, null, null)
        {
            this.Manual = true;
        }
   

        public QuartzJobSchedulerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, long ticks, string? _identity, string? _desciption)
        {

            WithInterval = TimeSpan.FromTicks(ticks + (long)(days * TimeSpan.TicksPerDay
                                             + hours * TimeSpan.TicksPerHour
                                             + minutes * TimeSpan.TicksPerMinute
                                             + seconds * TimeSpan.TicksPerSecond
                                             + milliseconds + TimeSpan.TicksPerMillisecond));
            Identity= _identity;
            Desciption= _desciption;
        }
        public string? Desciption { get; set; } = null;
        public string? Identity { get; set; } = null;
        internal TimeSpan WithInterval { get; set; }
        internal DateTimeOffset StartAt { get; set; } = DateTimeOffset.MinValue;
        public int RepeatCount { get; set; } = 0;
        public string? TriggerName { get; set; } = null;
        public string? TriggerGroup { get; set; } = null;
        public string? TriggerDescription { get; set; } =null;
        public int Priority { get; set; } = 0;
        public bool Manual { get; set; } = false;
    }


  
}