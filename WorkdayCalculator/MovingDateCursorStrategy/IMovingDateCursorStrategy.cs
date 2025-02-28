using WorkdayCalculator.Domain;

namespace WorkdayCalculator.MovingDateCursorStrategy;

public interface IMovingDateCursorStrategy
{
    public DateCursor ProcessIncrement(DateCursor dateCursor);

    public DateTime MoveToWorkingDay(DateTime dateTime);

    public DateTime RoundToMinutes(DateTime dateTime);
}