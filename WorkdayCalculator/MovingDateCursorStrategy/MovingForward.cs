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
            dateTime = dateTime.AddMinutes(workdayRemainingMinutes);
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

        if (dateTime.TimeOfDay >= _workday.Stop)
        {
            return dateTime.Date.AddDays(1).Add(_workday.Start);
        }

        return dateTime;
    }

    protected override bool IsWithinWorkingHours(DateTime date)
    {
        return date.TimeOfDay >= _workday.Start && date.TimeOfDay < _workday.Stop;
    }

    protected override DateTime RoundToMinutes(DateTime dateTime, long minutesFraction)
    {
        return dateTime.AddTicks(-minutesFraction);
    }
}