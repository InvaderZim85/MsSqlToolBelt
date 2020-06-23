﻿/*
    Author:        A. Pouwels
    Changed by:    /
    Creation date: 2020-06-22
    Change date:   /
    Description:   Loads all available tables
    Used by:       MsSqlToolBelt
*/

DECLARE @tables TABLE
(
    [Name] sysname NOT NULL
);

INSERT INTO @tables
SELECT 
    t.TABLE_NAME
FROM
    INFORMATION_SCHEMA.TABLES AS t
WHERE
    t.TABLE_NAME NOT LIKE 'syncobj%'
    AND t.TABLE_NAME NOT LIKE 'cdc_%'
    AND t.TABLE_NAME NOT LIKE 'MSpeer%'
    AND t.TABLE_NAME NOT LIKE 'MSpub%'
    AND t.TABLE_NAME NOT LIKE 'dtproperties'
    AND t.TABLE_NAME NOT LIKE 'sys%'
ORDER BY 
    t.TABLE_NAME;

-- Return the tables
SELECT
    t.[Name]
FROM
    @tables AS t;

SELECT
    c.TABLE_NAME AS [Table],
    c.COLUMN_NAME AS [Column],
    c.DATA_TYPE AS [DataType]
FROM
    INFORMATION_SCHEMA.COLUMNS AS c

    INNER JOIN @tables AS t
    ON t.[Name] = c.TABLE_NAME;