using System.ComponentModel.DataAnnotations;
using WorkdayCalculator.Domain;
using WorkdayCalculator.MovingDateCursorStrategy;

namespace WorkdayCalculator;

public class WorkdayCalendar : IWorkdayCalendar
{
    private readonly ISet<DateTime> _holidays = new HashSet<DateTime>();
    private readonly ISet<RecurringHoliday> _recurringHolidays = new HashSet<RecurringHoliday>();
    private Workday _workday;


    public void SetHoliday(DateTime date)
    {
        _holidays.Add(date);
    }

    public void SetRecurringHoliday(int month, int day)
    {
        _recurringHolidays.Add(new RecurringHoliday(month, day));
    }

    public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
    {
        _workday =
            new Workday(new TimeOnly(startHours, startMinutes).ToTimeSpan(),
                new TimeOnly(stopHours, stopMinutes).ToTimeSpan());
    }

    public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
    {
        if (_workday == null)
        {
            throw new ValidationException($"Workday not set. {nameof(SetWorkdayStartAndStop)} needs to be called first");
        }

        IMovingDateCursorStrategy movingDateCursorStrategy =
            incrementInWorkdays > 0
                ? new MovingForward(_holidays, _recurringHolidays, _workday)
                : new MovingBackwards(_holidays, _recurringHolidays, _workday);

        var workWindowInMinutes = (_workday.Stop - _workday.Start).TotalMinutes;

        var incrementInMinutes = workWindowInMinutes * (double)incrementInWorkdays;

        var dateCursor = startDate;

        // TODO: maybe can find a better condition
        while (incrementInMinutes != 0)
        {
            dateCursor = movingDateCursorStrategy.MoveToWorkingDay(dateCursor);
            (dateCursor, incrementInMinutes) =
                movingDateCursorStrategy.ProcessIncrement(new DateCursor(dateCursor, incrementInMinutes));
        }

        return movingDateCursorStrategy.RoundToMinutes(dateCursor);
    }
}