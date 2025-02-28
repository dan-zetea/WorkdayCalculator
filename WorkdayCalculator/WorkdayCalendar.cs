namespace WorkdayCalculator;

public class WorkdayCalendar : IWorkdayCalendar
{
    private readonly ISet<DateTime> _holidays = new HashSet<DateTime>();
    private readonly ISet<RecurringHoliday> _recurringHolidays = new HashSet<RecurringHoliday>();
    private Workday _workday;
    private DateCursorDirection _dateCursorDirection;


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
        _dateCursorDirection = incrementInWorkdays < 0 ? DateCursorDirection.Backwards : DateCursorDirection.Forward;

        var workWindowInMinutes = (_workday.Stop - _workday.Start).TotalMinutes;

        var incrementInMinutes = workWindowInMinutes * (double)incrementInWorkdays;

        var dateCursor = startDate;

        // TODO: maybe can find a better condition
        while (incrementInMinutes != 0)
        {
            dateCursor = MoveToWorkingDay(dateCursor);

            if (incrementInMinutes > 0)
            {
                var workdayRemainingMinutes = (_workday.Stop - dateCursor.TimeOfDay).TotalMinutes; 

                if (workdayRemainingMinutes >= incrementInMinutes)
                {
                    dateCursor = dateCursor.AddMinutes(incrementInMinutes);
                    incrementInMinutes = 0;
                }
                else
                {
                    // TODO: check for better alternative to AddTicks(1)
                    dateCursor = dateCursor.AddMinutes(workdayRemainingMinutes).AddTicks(1);
                    incrementInMinutes -= workdayRemainingMinutes;
                }
            }

            if (incrementInMinutes < 0)
            {
                var workdayRemainingMinutes = (dateCursor.TimeOfDay - _workday.Start).TotalMinutes;

                if (workdayRemainingMinutes >= Math.Abs(incrementInMinutes))
                {
                    dateCursor = dateCursor.AddMinutes(incrementInMinutes);
                    incrementInMinutes = 0;
                }
                else
                {
                    // TODO: check for better alternative to AddTicks(-1)
                    dateCursor = dateCursor.AddMinutes(-workdayRemainingMinutes).AddTicks(-1);
                    incrementInMinutes += workdayRemainingMinutes;
                }
            }
        }

        return RoundToMinutes(dateCursor);
    }

    private DateTime RoundToMinutes(DateTime dateTime)
    {
        var minutesFraction = dateTime.Ticks % TimeSpan.FromMinutes(1).Ticks;

        if (minutesFraction > 0)
        {
            return _dateCursorDirection switch
            {
                DateCursorDirection.Forward => dateTime.AddTicks(- minutesFraction),
                DateCursorDirection.Backwards => dateTime.AddTicks(TimeSpan.FromMinutes(1).Ticks - minutesFraction),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return dateTime;
    }

    private DateTime MoveToWorkingDay(DateTime dateTime)
    {
        var dateCursorIncrement = _dateCursorDirection switch
        {
            DateCursorDirection.Forward => 1,
            DateCursorDirection.Backwards => -1,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!IsWithinWorkingHours(dateTime))
        {
            dateTime = MoveToNextWorkingHoursWindow(dateTime);
        }

        while (!IsWorkingDay(dateTime))
        {
            // when moving to next day, we don't want to keep the initial start date time
            dateTime = dateTime.Date.AddDays(dateCursorIncrement).Add(GetWorkingDayInitialCursor());
        }

        return dateTime;
    }

    private DateTime MoveToNextWorkingHoursWindow(DateTime dateTime)
    {
        if (dateTime.TimeOfDay < _workday.Start)
        {
            return _dateCursorDirection switch
            {
                DateCursorDirection.Forward => dateTime.Date.Add(_workday.Start),
                DateCursorDirection.Backwards => dateTime.Date.AddDays(-1).Add(_workday.Stop),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        if (dateTime.TimeOfDay > _workday.Stop)
        {
            return _dateCursorDirection switch
            {
                DateCursorDirection.Forward => dateTime.Date.AddDays(1).Add(_workday.Start),
                DateCursorDirection.Backwards => dateTime.Date.Add(_workday.Stop),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return dateTime;
    }

    private bool IsWorkingDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && 
               date.DayOfWeek != DayOfWeek.Sunday && 
               !_holidays.Contains(date) &&
               !_recurringHolidays.Any(h=>h.Day == date.Day && h.Month == date.Month);
    }

    private bool IsWithinWorkingHours(DateTime date)
    {
        return date.TimeOfDay >= _workday.Start && date.TimeOfDay <= _workday.Stop;
    }

    private TimeSpan GetWorkingDayInitialCursor() => _dateCursorDirection switch
    {
        DateCursorDirection.Forward => _workday.Start,
        DateCursorDirection.Backwards => _workday.Stop,
        _ => throw new ArgumentOutOfRangeException()
    };

    private record RecurringHoliday(int Month, int Day);

    private record Workday(TimeSpan Start, TimeSpan Stop);

    private enum DateCursorDirection
    {
        Forward,
        Backwards
    }
}