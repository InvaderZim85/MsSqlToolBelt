/*
    Author:        A. Pouwels
    Changed by:    A. Pouwels
    Creation date: 2020-06-17
    Change date:   2021-01-27
    Description:   Loads all available table types
*/

-- Load all available table types
SELECT 
    tt.type_table_object_id AS Id,
    tt.[name]
FROM
    sys.table_types AS tt
WHERE
    tt.is_user_defined = 1;

-- Load the columns of the table types
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
    c.is_nullable AS Nullable,
    c.column_id AS ColumnId
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

-- Load the key columns
SELECT 
    tt.type_table_object_id AS [Key],
    ic.column_id AS [Value]
FROM 
    sys.table_types AS tt
    
    INNER JOIN sys.key_constraints AS kc
    ON kc.parent_object_id = tt.type_table_object_id
    
    INNER JOIN sys.indexes AS i
    ON i.object_id = kc.parent_object_id
        AND i.index_id = kc.unique_index_id
    
    INNER JOIN sys.index_columns AS ic
    ON ic.object_id = kc.parent_object_id
    
    INNER JOIN sys.columns AS c
    ON c.object_id = ic.object_id
    AND c.column_id = ic.column_id
WHERE   
    tt.is_user_defined = 1;