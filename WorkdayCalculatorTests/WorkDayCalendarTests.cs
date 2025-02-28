using FluentAssertions;
using WorkdayCalculator;

namespace WorkdayCalculatorTests;

public class WorkDayCalendarTests
{
    [Theory]
    [InlineData("24-05-2004 15:07", 0.25, "25-05-2004 09:07")]
    [InlineData("24-05-2004 04:00", 0.5, "24-05-2004 12:00")]
    [InlineData("24-05-2004 18:05", -5.5, "14-05-2004 12:00")]
    [InlineData("24-05-2004 19:03", 44.723656, "27-07-2004 13:47")]
    [InlineData("24-05-2004 18:03", -6.7470217, "13-05-2004 10:02")]
    [InlineData("24-05-2004 08:03", 12.782709, "10-06-2004 14:18")]
    [InlineData("24-05-2004 07:03", 8.276628, "04-06-2004 10:12")]
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

    private WorkdayCalendar Sut() => new WorkdayCalendar();
}