using System;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Search;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides helper functions for the interaction with the database stuff
/// </summary>
internal static class DataHelper
{
    /// <summary>
    /// Gets the name of the object type
    /// </summary>
    /// <param name="type">The object type</param>
    /// <returns>The name</returns>
    public static string GetTypeName(string type)
    {
        // Clean the value first...
        type = type.Trim().ToUpper();

        return type switch
        {
            "AF" => "Aggregate function",
            "C" => "Check constraint",
            "D" => "Default",
            "EC" => "Edge constraint",
            "F" => "Foreign key constraint",
            "FN" => "Functions",
            "FS" => "CLR scalar functions",
            "FT" => "CLR table valued function",
            "IF" => "Inline table valued function",
            "IT" => "Internal table",
            "P" => "Procedure",
            "PC" => "CLR stored procedure",
            "PG" => "Plan guid",
            "PK" => "Primary key",
            "R" => "Rule",
            "RF" => "Replication filter procedure",
            "S" => "System base table",
            "SN" => "Synonym",
            "SO" => "Sequence object",
            "SQ" => "Service queue",
            "TA" => "CLR DML trigger",
            "TF" => "Table valued function",
            "TR" => "Trigger",
            "TT" => "Table type",
            "U" => "Table (user defined)",
            "UQ" => "Unique constraint",
            "V" => "View",
            "X" => "Extended stored procedure",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Converts the last run date and last run time into a normal date time. Fallback is <see cref="DateTime.MinValue"/>
    /// </summary>
    /// <param name="lastRunDate">The last run date (format = yyyymmdd)</param>
    /// <param name="lastRunTime">The last run time (format = hhmmss)</param>
    /// <returns>The converted date / time. If one of the values is "wrong", <see cref="DateTime.MinValue"/> will be returned</returns>
    public static DateTime ConvertToDateTime(int lastRunDate, int lastRunTime)
    {
        var tmpLastRunDate = lastRunDate.ToString();
        var tmpLastRunTime = lastRunTime.ToString();

        if (tmpLastRunDate.Length != 8 || tmpLastRunTime.Length != 6)
            return DateTime.MinValue;

        var (dateFirst, dateMiddle, dateLast) = GetDateValues(tmpLastRunDate);
        var (timeFirst, timeMiddle, timeLast) = GetTimeValues(tmpLastRunTime);

        return new DateTime(dateFirst, dateMiddle, dateLast, timeFirst, timeMiddle,
            timeLast);
    }

    /// <summary>
    /// Converts the last run duration into a time span
    /// </summary>
    /// <param name="lastRunDuration">The last run time (format = hhmmss)</param>
    /// <returns>The time span</returns>
    public static TimeSpan ConvertToTimeSpan(int lastRunDuration)
    {
        var tmpDuration = lastRunDuration.ToString().PadLeft(6, '0');
        var (first, middle, last) = GetTimeValues(tmpDuration);

        return new TimeSpan(first, middle, last);
    }

    /// <summary>
    /// Extracts the value of last run date
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The single values</returns>
    private static (int First, int Middle, int Last) GetDateValues(string value)
    {
        var first = value[..4]; // First four positions
        var middle = value.Substring(4, 2); // Position 5 and 6
        var last = value.Substring(6, 2); // Last two positions

        return (first.ToInt(), middle.ToInt(), last.ToInt());
    }

    /// <summary>
    /// Extracts the value of the last run time
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The single values</returns>
    private static (int First, int Middle, int Last) GetTimeValues(string value)
    {
        var first = value[..2]; // First four positions
        var middle = value.Substring(2, 2); // Position 5 and 6
        var last = value.Substring(4, 2); // Last two positions

        return (first.ToInt(), middle.ToInt(), last.ToInt());
    }

    /// <summary>
    /// Gets the information value based on the id and the type
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="type">The desired type</param>
    /// <returns>The info</returns>
    public static string GetJobInfo(this int id, JobStepInfoType type)
    {
        return type switch
        {
            JobStepInfoType.SuccessAction => id switch
            {
                1 => "Quit with success (default)",
                2 => "Quit with failure",
                3 => "Go to next step",
                4 => $"Go to the next defined step (see {nameof(JobStepEntry.SuccessNextStepId)})",
                _ => "Unknown"
            },
            JobStepInfoType.FailAction => id switch
            {
                1 => "Quit with success",
                2 => "Quit with failure (default)",
                3 => "Go to next step",
                4 => $"Go to the next defined step (see {nameof(JobStepEntry.SuccessNextStepId)})",
                _ => "Unknown"
            },
            JobStepInfoType.LastRun => id switch
            {
                0 => "Failed",
                1 => "Succeeded",
                2 => "Retry",
                3 => "Canceled",
                _ => "Unknown"
            },
            _ => "Unknown"
        };
    }
}