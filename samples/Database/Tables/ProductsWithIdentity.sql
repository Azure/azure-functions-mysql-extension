DROP TABLE IF EXISTS ProductsWithIdentity;

CREATE TABLE ProductsWithIdentity (
	ProductId int PRIMARY KEY AUTO_INCREMENT,
	Name varchar(100) NULL,
	Cost int NULL
);
