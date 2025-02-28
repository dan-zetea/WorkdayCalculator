using WorkdayCalculator.Domain;

namespace WorkdayCalculator.MovingDateCursorStrategy;

public class MovingForward : MovingDateCursorStrategyBase
{
    private readonly Workday _workday;

    public MovingForward(ISet<DateTime> holidays, ISet<RecurringHoliday> recurringHolidays, Workday workday) : base(
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

    protected override DateTime MoveToNextDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).Add(_workday.Start);
    }

    protected override DateTime MoveToNextWorkingHoursWindow(DateTime dateTime)
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

    protected override DateTime RoundToMinutes(DateTime dateTime, long minutesFraction)
    {
        return dateTime.AddTicks(-minutesFraction);
    }
}