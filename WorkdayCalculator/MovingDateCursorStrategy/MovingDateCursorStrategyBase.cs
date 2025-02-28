using WorkdayCalculator.Domain;

namespace WorkdayCalculator.MovingDateCursorStrategy;

public abstract class MovingDateCursorStrategyBase : IMovingDateCursorStrategy
{
    private readonly ISet<DateTime> _holidays;
    private readonly ISet<RecurringHoliday> _recurringHolidays;
    private readonly Workday _workday;

    protected MovingDateCursorStrategyBase(ISet<DateTime> holidays, ISet<RecurringHoliday> recurringHolidays,
        Workday workday)
    {
        _holidays = holidays;
        _recurringHolidays = recurringHolidays;
        _workday = workday;
    }

    public abstract DateCursor ProcessIncrement(DateCursor dateCursor);

    public DateTime MoveToWorkingDay(DateTime dateTime)
    {
        if (!IsWithinWorkingHours(dateTime))
        {
            dateTime = MoveToNextWorkingHoursWindow(dateTime);
        }

        while (!IsWorkingDay(dateTime))
        {
            dateTime = MoveToNextDay(dateTime);
        }

        return dateTime;
    }

    public DateTime RoundToMinutes(DateTime dateTime)
    {
        var minutesFraction = dateTime.Ticks % TimeSpan.FromMinutes(1).Ticks;
        return minutesFraction > 0 ? RoundToMinutes(dateTime, minutesFraction) : dateTime;
    }

    protected abstract DateTime RoundToMinutes(DateTime dateTime, long minutesFraction);

    protected abstract DateTime MoveToNextDay(DateTime dateTime);

    protected abstract DateTime MoveToNextWorkingHoursWindow(DateTime dateTime);

    protected bool IsWithinWorkingHours(DateTime date)
    {
        return date.TimeOfDay >= _workday.Start && date.TimeOfDay <= _workday.Stop;
    }

    protected bool IsWorkingDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday &&
               date.DayOfWeek != DayOfWeek.Sunday &&
               !_holidays.Contains(date.Date) &&
               !_recurringHolidays.Any(h => h.Day == date.Day && h.Month == date.Month);
    }
}