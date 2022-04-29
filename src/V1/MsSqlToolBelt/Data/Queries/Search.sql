-- DECLARE @search NVARCHAR(50) = '%Author%';

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

-- Add the tables
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

-- Return the result
SELECT
    r.Id,
    r.[Name],
    r.[Definition],
    r.[Type],
    o.create_date AS CreationDateTime,
    o.modify_date AS ModifiedDateTime
FROM
    @result r

    INNER JOIN sys.objects AS o
    ON o.object_id = r.Id
ORDER BY
    r.[Type];

-- Table column information
DECLARE @tableResult TABLE
(
    [Table] SYSNAME NOT NULL,
    [Column] SYSNAME NOT NULL,
    [DataType] NVARCHAR(128) NULL,
    ColumnPosition INT NULL,
    Nullable VARCHAR(3) NULL,
    [MaxLength] INT NOT NULL,
    [Precision] INT NOT NULL,
    DecimalPlaceValue INT NOT NULL,
    DefaultValue NVARCHAR(250) NULL,
    IsReplicated BIT NOT NULL DEFAULT (0)
);

INSERT INTO @tableResult
(
    [Table],
    [Column],
    DataType,
    ColumnPosition,
    Nullable,
    [MaxLength],
    [Precision],
    DecimalPlaceValue,
    DefaultValue,
    IsReplicated
)
SELECT DISTINCT
    c.TABLE_NAME AS [Table],
    c.COLUMN_NAME AS [Column],
    c.DATA_TYPE AS DataType,
    c.ORDINAL_POSITION AS ColumnPosition,
    c.IS_NULLABLE AS Nullable,
    COALESCE(c.CHARACTER_MAXIMUM_LENGTH, 0) AS [MaxLength],
    COALESCE(c.NUMERIC_PRECISION, c.DATETIME_PRECISION, 0) AS [Precision],
    COALESCE(c.NUMERIC_SCALE, 0) AS DecimalPlaceValue,
    '',
    0
FROM
    INFORMATION_SCHEMA.COLUMNS AS c

    INNER JOIN @result AS r
    ON r.[Name] = c.TABLE_NAME
    AND r.[Type] = 'Table'

    LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
    ON tc.TABLE_NAME = c.TABLE_NAME;

-- Replication / default information
UPDATE
    tr
SET
    tr.IsReplicated = c.is_replicated,
    tr.DefaultValue = REPLACE(REPLACE(ISNULL(OBJECT_DEFINITION(c.default_object_id), 'NULL'), '(', ''), ')', '') -- Remove the brackets
FROM
    @tableResult AS tr

    INNER JOIN sys.columns AS c
    ON c.object_id = OBJECT_ID(tr.[Table])
    AND c.[name] = tr.[Column];

SELECT
    [Table],
    [Column],
    DataType,
    ColumnPosition,
    Nullable,
    [MaxLength],
    [Precision],
    DecimalPlaceValue,
    ISNULL(DefaultValue, 'NULL') AS DefaultValue,
    IsReplicated
FROM
    @tableResult;

-- Primary key information
DECLARE @pkValues TABLE
(
    [Database] SYSNAME NOT NULL,
    [Schema] SYSNAME NOT NULL,
    [Table] SYSNAME NOT NULL,
    [Column] SYSNAME NOT NULL,
    [KeySeq] SMALLINT NOT NULL,
    [PkName] SYSNAME NOT NULL
);

-- Cursor to get the information of all pk keys
DECLARE @tableName SYSNAME;

DECLARE TableCursor CURSOR FOR
SELECT
    r.[Name]
FROM
    @result AS r;

OPEN TableCursor;

FETCH NEXT FROM TableCursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO @pkValues
    EXEC sp_pkeys @table_name = @tableName;
    FETCH NEXT FROM TableCursor INTO @tableName;
END;

-- House keeping
CLOSE TableCursor;
DEALLOCATE TableCursor;

SELECT
    [Table],
    [Column]
FROM
    @pkValues;

-- Index
SELECT
    i.[name] AS [Name],
    r.[Name] AS [Table],
    COL_NAME(ic.object_id, ic.column_id) AS [Column]
FROM
    sys.indexes AS i

    INNER JOIN sys.index_columns AS ic
    ON ic.object_id = i.object_id
    AND ic.index_id = i.index_id

    INNER JOIN @result AS r
    ON OBJECT_ID(r.[Name]) = i.object_id
    AND r.[Type] = 'Table';