using System;

namespace Sonovate.CodeTest.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetStartDateTime()
        {
            return DateTime.Now.AddMonths(-1);
        }

        public DateTime GetCurrentDateTime()
        {
            return  DateTime.Now;
        }
    }
}
