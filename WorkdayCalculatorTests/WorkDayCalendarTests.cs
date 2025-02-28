using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using WorkdayCalculator;

namespace WorkdayCalculatorTests;

public class WorkDayCalendarTests
{
    [Theory]
    [InlineData("24-05-2004 15:07", 0.25, "25-05-2004 09:07")]
    [InlineData("24-05-2004 15:07", 0, "24-05-2004 15:07")]
    [InlineData("24-05-2004 04:00", 0.5, "24-05-2004 12:00")]
    [InlineData("24-05-2004 18:05", -5.5, "14-05-2004 12:00")]
    [InlineData("24-05-2004 19:03", 44.723656, "27-07-2004 13:47")]
    [InlineData("24-05-2004 18:03", -6.7470217, "13-05-2004 10:02")]
    [InlineData("24-05-2004 08:03", 12.782709, "10-06-2004 14:18")]
    [InlineData("24-05-2004 07:03", 8.276628, "04-06-2004 10:12")]
    [InlineData("24-05-2004 07:03", 8.5, "04-06-2004 12:00")]
    public void GetWorkdayIncrement(string startDateString, decimal increment, string expectedResultString)
    {
        var sut = Sut();

        sut.SetWorkdayStartAndStop(8, 0, 16, 0);
        sut.SetRecurringHoliday(5, 17);
        sut.SetHoliday(new DateTime(2004, 5, 27));

        var startDate = DateTime.Parse(startDateString);

        var result = sut.GetWorkdayIncrement(startDate, increment);

        var expectedResult = DateTime.Parse(expectedResultString);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(13, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 32, false)]
    [InlineData(1, 31, true)]
    [InlineData(2, 29, true)]
    [InlineData(3, 31, true)]
    [InlineData(4, 30, true)]
    public void SetRecurringHoliday_InputValidity(int month, int day, bool expectsToBeValid)
    {
        var sut = Sut();

        var call = () => sut.SetRecurringHoliday(month, day);

        if (expectsToBeValid)
        {
            call.Should().NotThrow();
        }
        else
        {
            call.Should().Throw<ValidationException>();
        }
    }

    [Theory]
    [InlineData(-1, 2, 2, 2, false)]
    [InlineData(24, 2, 2, 2, false)]
    [InlineData(0, -1, 2, 2, false)]
    [InlineData(0, 60, 2, 2, false)]
    [InlineData(0, 0, -1, 2, false)]
    [InlineData(0, 0, 24, 2, false)]
    [InlineData(0, 0, 1, -1, false)]
    [InlineData(0, 0, 1, 60, false)]
    [InlineData(8, 0, 7, 0, false)]
    [InlineData(8, 0, 7, 59, false)]
    [InlineData(8, 0, 8, 0, false)]
    [InlineData(8, 0, 8, 1, true)]
    [InlineData(8, 0, 9, 0, true)]
    [InlineData(8, 0, 16, 0, true)]
    public void SetWorkdayStartAndStop_InputValidity(int startHours, int startMinutes, int stopHours, int stopMinutes,
        bool expectsToBeValid)
    {
        var sut = Sut();

        var call = () => sut.SetWorkdayStartAndStop(startHours, startMinutes, stopHours, stopMinutes);

        if (expectsToBeValid)
        {
            call.Should().NotThrow();
        }
        else
        {
            call.Should().Throw<ValidationException>();
        }
    }

    private WorkdayCalendar Sut() => new WorkdayCalendar();
}