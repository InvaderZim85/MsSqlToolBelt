/*
    Author:        A. Pouwels
    Changed by:    A. Pouwels
    Creation date: 2020-06-22
    Change date:   2021-01-27
    Description:   Loads all available tables
    Used by:       MsSqlToolBelt
*/

DECLARE @tables TABLE
(
    [Name] sysname NOT NULL,
    [Schema] sysname NOT NULL
);

INSERT INTO @tables
SELECT 
    t.TABLE_NAME,
    t.TABLE_SCHEMA
FROM
    INFORMATION_SCHEMA.TABLES AS t
ORDER BY 
    t.TABLE_NAME;

-- Return the tables
SELECT
    t.[Name],
    t.[Schema]
FROM
    @tables AS t;

SELECT
    c.TABLE_NAME AS [Table],
    c.COLUMN_NAME AS [Column],
    c.DATA_TYPE AS [DataType],
    c.ORDINAL_POSITION AS [ColumnPosition]
FROM
    INFORMATION_SCHEMA.COLUMNS AS c

    INNER JOIN @tables AS t
    ON t.[Name] = c.TABLE_NAME;