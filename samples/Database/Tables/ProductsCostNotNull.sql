DROP TABLE IF EXISTS ProductsCostNotNull;

CREATE TABLE ProductsCostNotNull (
	ProductId int NOT NULL PRIMARY KEY,
	Name varchar(100) NULL,
	Cost int NOT NULL
)