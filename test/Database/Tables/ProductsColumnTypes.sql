DROP TABLE IF EXISTS ProductsColumnTypes;

CREATE TABLE ProductsColumnTypes (
    ProductId int NOT NULL PRIMARY KEY,
    BigIntType bigint,
    BitType bit,
    DecimalType decimal(18,4),
    NumericType numeric(18,4),
    SmallIntType smallint,
    TinyIntType tinyint,
    FloatType float,
    RealType real,
    DateType date,
    DatetimeType datetime,
    TimeType time,
    CharType char(4),
    VarcharType varchar(100),
    NcharType nchar(4),
    NvarcharType nvarchar(100),
    BinaryType binary(4),
    VarbinaryType varbinary(100)
);
