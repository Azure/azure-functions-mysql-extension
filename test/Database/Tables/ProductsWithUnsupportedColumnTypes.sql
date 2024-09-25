DROP TABLE IF EXISTS ProductsWithUnsupportedColumnTypes;

CREATE TABLE ProductsWithUnsupportedColumnTypes (
    ProductId int NOT NULL PRIMARY KEY,
    Name nvarchar(100) NULL,
    Cost int NULL,
    Geometry geometry NULL,
    GeometryCollection geometrycollection NULL
);