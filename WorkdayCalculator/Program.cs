using WorkdayCalculator;

IWorkdayCalendar calendar = new WorkdayCalendar();
calendar.SetWorkdayStartAndStop(8, 0, 16, 0);
calendar.SetRecurringHoliday(5, 17);
calendar.SetHoliday(new DateTime(2004, 5, 27));

string format = "dd-MM-yyyy HH:mm";
var start = new DateTime(2004, 5, 24, 18, 3, 0);
decimal increment = -6.7470217m;

var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

Console.WriteLine(
    start.ToString(format) +
    " with an addition of " +
    increment +
    " work days is " +
    incrementedDate.ToString(format));