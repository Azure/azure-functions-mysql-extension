DROP TABLE IF EXISTS ProductsWithUnsupportedColumnTypes;

CREATE TABLE ProductsWithUnsupportedColumnTypes (
    ProductId int NOT NULL PRIMARY KEY,
    Name nvarchar(100) NULL,
    Cost int NULL,
    Point point,
    MultiPoint multipoint,
    LineString linestring,
    MultiLineString multilinestring,
    Polygon polygon,
    MultiPolygon multipolygon,
    Geometry geometry,
    GeometryCollection geometrycollection
);