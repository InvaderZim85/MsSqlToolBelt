/*
    Author:        A. Pouwels
    Changed by:    /
    Creation date: 2020-06-17
    Change date:   /
    Description:   Loads all available table types
*/

SELECT 
    tt.type_table_object_id AS Id,
    tt.[name]
FROM
    sys.table_types AS tt
WHERE
    tt.is_user_defined = 1;

SELECT
    tt.type_table_object_id AS TableTypeId,
    c.[name] AS [Name],
    st.[name] AS Datatype,
    CASE
        WHEN st.[name] IN ('numeric', 'decimal') THEN
            '(' + CONVERT(VARCHAR(5), c.[precision]) + ', ' + CONVERT(VARCHAR(5), c.scale) + ')'
        WHEN st.[name] IN
        (
            'char',
            'nchar',
            'varchar',
            'nvarchar'
        ) THEN
            '(' + CONVERT(VARCHAR(5), c.max_length) + ')'
        ELSE
            ''
    END AS Size,
    c.is_nullable AS Nullable
FROM
    sys.table_types AS tt

    INNER JOIN sys.columns AS c
    ON c.object_id = tt.type_table_object_id

    INNER JOIN sys.systypes AS st
    ON st.xusertype = c.system_type_id 
    AND st.uid = 4
WHERE
    tt.is_user_defined = 1
ORDER BY
    tt.[name],
    c.column_id;