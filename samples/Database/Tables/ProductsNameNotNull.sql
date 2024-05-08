DROP TABLE IF EXISTS ProductsNameNotNull;

CREATE TABLE ProductsNameNotNull (
	ProductId int NOT NULL PRIMARY KEY,
	Name varchar(100) NOT NULL,
	Cost int NULL
)