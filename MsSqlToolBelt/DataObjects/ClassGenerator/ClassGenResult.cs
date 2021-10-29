namespace MsSqlToolBelt.DataObjects.ClassGenerator
{
    /// <summary>
    /// Represents the class generator result
    /// </summary>
    internal sealed class ClassGenResult
    {
        /// <summary>
        /// Gets the C# code
        /// </summary>
        public string Code { get; }
        
        /// <summary>
        /// Gets the sql statement
        /// </summary>
        public string Sql { get; }

        /// <summary>
        /// Gets the code for the ef key
        /// </summary>
        public string CodeEfKey { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ClassGenResult"/>
        /// </summary>
        /// <param name="code">The c# code</param>
        /// <param name="sql">The sql statement</param>
        /// <param name="codeEfKey">The c# code for the ef key</param>
        public ClassGenResult(string code, string sql, string codeEfKey)
        {
            Code = code;
            Sql = sql;
            CodeEfKey = codeEfKey;
        }
    }
}
