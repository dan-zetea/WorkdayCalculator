using WorkdayCalculator.Domain;

namespace WorkdayCalculator.MovingDateCursorStrategy;

public class MovingForward : MovingDateCursorStrategyBase
{
    private readonly Workday _workday;

    internal MovingForward(ISet<DateTime> holidays, ISet<RecurringHoliday> recurringHolidays, Workday workday) : base(
        holidays, recurringHolidays, workday)
    {
        _workday = workday;
    }

    public override DateCursor ProcessIncrement(DateCursor dateCursor)
    {
        var (dateTime, incrementInMinutes) = dateCursor;
        var workdayRemainingMinutes = (_workday.Stop - dateTime.TimeOfDay).TotalMinutes;

        if (workdayRemainingMinutes >= incrementInMinutes)
        {
            dateTime = dateTime.AddMinutes(incrementInMinutes);
            incrementInMinutes = 0;
        }
        else
        {
            // TODO: check for better alternative to AddTicks(1)
            dateTime = dateTime.AddMinutes(workdayRemainingMinutes).AddTicks(1);
            incrementInMinutes -= workdayRemainingMinutes;
        }

        return new DateCursor(dateTime, incrementInMinutes);
    }

    public override DateTime MoveToWorkingDay(DateTime dateTime)
    {
        if (!IsWithinWorkingHours(dateTime))
        {
            dateTime = MoveToNextWorkingHoursWindow(dateTime);
        }

        while (!IsWorkingDay(dateTime))
        {
            // when moving to next day, we don't want to keep the initial start date time
            dateTime = dateTime.Date.AddDays(1).Add(_workday.Start);
        }

        return dateTime;
    }

    public override DateTime RoundToMinutes(DateTime dateTime)
    {
        var minutesFraction = dateTime.Ticks % TimeSpan.FromMinutes(1).Ticks;
        return minutesFraction > 0 ? dateTime.AddTicks(-minutesFraction) : dateTime;
    }

    private DateTime MoveToNextWorkingHoursWindow(DateTime dateTime)
    {
        if (dateTime.TimeOfDay < _workday.Start)
        {
            return dateTime.Date.Add(_workday.Start);
        }

        if (dateTime.TimeOfDay > _workday.Stop)
        {
            return dateTime.Date.AddDays(1).Add(_workday.Start);
        }

        return dateTime;
    }
}