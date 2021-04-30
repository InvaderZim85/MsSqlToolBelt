DECLARE @result TABLE
(
    [Id] INT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Definition] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(50) NOT NULL
);

INSERT INTO @result
SELECT
    m.object_id,
    OBJECT_NAME(m.object_id) AS [Name],
    m.[definition] AS [Definition],
    CASE o.[type]
        WHEN 'AF' THEN 'Aggregate function'
        WHEN 'C' THEN 'Check constraint'
        WHEN 'D' THEN 'Default'
        WHEN 'F' THEN 'Foreign key constraint'
        WHEN 'FN' THEN 'Function'
        WHEN 'FS' THEN 'CLR scalar function'
        WHEN 'FT' THEN 'CLR table valued function'
        WHEN 'IF' THEN 'Inline table valued function'
        WHEN 'IT' THEN 'Internal table'
        WHEN 'P' THEN 'Procedure'
        WHEN 'PC' THEN 'CLR stored procedure'
        WHEN 'PG' THEN 'Plan guid'
        WHEN 'PK' THEN 'Primary key'
        WHEN 'R' THEN 'Rule'
        WHEN 'RF' THEN 'Replication filter procedure'
        WHEN 'S' THEN 'System base table'
        WHEN 'SN' THEN 'Synonym'
        WHEN 'SO' THEN 'Sequence object'
        WHEN 'U' THEN 'Table (user defined)'
        WHEN 'V' THEN 'View'
        WHEN 'EC' THEN 'Edge constraint'
        WHEN 'SQ' THEN 'Service queue'
        WHEN 'TA' THEN 'CLR DML Trigger'
        WHEN 'TF' THEN 'Table valued function'
        WHEN 'TR' THEN 'Trigger'
        WHEN 'TT' THEN 'Table type'
        WHEN 'UQ' THEN 'Unique constraint'
        WHEN 'X' THEN 'Extended stored procedure'
        ELSE o.[type]
    END AS [Type]
FROM
    sys.sql_modules m

    INNER JOIN sys.objects AS o
    ON o.object_id = m.object_id
WHERE
    m.[definition] LIKE @search

UNION

SELECT DISTINCT
    t.object_id,
    t.[name],
    '',
    'Table'
FROM
    sys.tables t

    INNER JOIN sys.columns c
    ON c.object_id = t.object_id
WHERE
    t.[Name] LIKE @search
    OR c.[name] LIKE @search;

SELECT
    r.Id,
    r.[Name],
    r.[Definition],
    r.[Type]
FROM
    @result r
WHERE
    r.[Name] NOT LIKE 'syncobj%'
    AND r.[Name] NOT LIKE '%_CT'
    AND r.[Name] NOT LIKE 'SYNC%'
ORDER BY
    r.[Type];

SELECT DISTINCT
    c.TABLE_NAME AS [Table],
    c.COLUMN_NAME AS [Column],
    c.DATA_TYPE AS [DataType],
    c.ORDINAL_POSITION AS ColumnPosition,
    c.IS_NULLABLE AS Nullable,
    COALESCE(c.CHARACTER_MAXIMUM_LENGTH, 0) AS [MaxLength],
    COALESCE(c.NUMERIC_PRECISION, c.DATETIME_PRECISION, 0) AS [Precision],
    COALESCE(c.NUMERIC_SCALE, 0) AS DecimalPlaceValue
FROM
    INFORMATION_SCHEMA.COLUMNS AS c

    INNER JOIN @result AS r
    ON r.[Name] = c.TABLE_NAME
    AND r.[Type] = 'Table'

    LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
    ON tc.TABLE_NAME = c.TABLE_NAME;

SELECT DISTINCT
    tc.TABLE_NAME AS [Table],
    ccu.COLUMN_NAME AS [Column]
FROM
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc

    INNER JOIN @result AS r
    ON r.[Name] = tc.TABLE_NAME
    AND r.[Type] = 'Table'

    LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu
    ON ccu.TABLE_NAME = tc.TABLE_NAME;