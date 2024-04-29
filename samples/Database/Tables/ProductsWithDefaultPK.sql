DROP TABLE IF EXISTS ProductsWithDefaultPK;

CREATE TABLE ProductsWithDefaultPK (
	ProductId int PRIMARY KEY AUTO_INCREMENT,
	Name varchar(100) NULL,
	Cost int NULL
) AUTO_INCREMENT = 100;