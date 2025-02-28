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
        if (month < 1 || month > 12)
        {
            throw new ValidationException($"{month} is invalid value for {nameof(month)}");
        }

        // we need 29 Feb to be valid, so we should use a leap year to validate
        bool isDayValid = day >= 1 && day <= DateTime.DaysInMonth(2024, month);
        if (!isDayValid)
        {
            throw new ValidationException($"{day} is invalid value for month {month}");
        }

        _recurringHolidays.Add(new RecurringHoliday(month, day));
    }

    public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
    {
        ValidateHoursInputValid(startHours);
        ValidateHoursInputValid(stopHours);
        ValidateMinutesInputValid(startMinutes);
        ValidateMinutesInputValid(stopMinutes);

        var startTime = new TimeOnly(startHours, startMinutes);
        var stopTime = new TimeOnly(stopHours, stopMinutes);

        if (startTime >= stopTime)
        {
            throw new ValidationException($"{startTime} - {stopTime} is not a valid workday interval");
        }

        _workday = new Workday(startTime.ToTimeSpan(), stopTime.ToTimeSpan());
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

        while (incrementInMinutes != 0)
        {
            dateCursor = movingDateCursorStrategy.MoveToWorkingDay(dateCursor);
            (dateCursor, incrementInMinutes) =
                movingDateCursorStrategy.ProcessIncrement(new DateCursor(dateCursor, incrementInMinutes));
        }

        return movingDateCursorStrategy.RoundToMinutes(dateCursor);
    }

    private void ValidateHoursInputValid(int hours)
    {
        var isValid = hours >= 0 && hours <= 23;

        if (!isValid)
        {
            throw new ValidationException($"{hours} is not a valid hours value");
        }
    }

    private void ValidateMinutesInputValid(int minutes)
    {
        var isValid = minutes >= 0 && minutes <= 59;

        if (!isValid)
        {
            throw new ValidationException($"{minutes} is not a valid minutes value");
        }
    }
}