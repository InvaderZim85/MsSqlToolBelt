namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different job step info types
/// </summary>
internal enum JobStepInfoType
{
    /// <summary>
    /// The success action of the job
    /// </summary>
    SuccessAction,

    /// <summary>
    /// The fail action of the job
    /// </summary>
    FailAction,

    /// <summary>
    /// The last run outcome of the job
    /// </summary>
    LastRun
}