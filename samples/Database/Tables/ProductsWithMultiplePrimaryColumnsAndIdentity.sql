DROP TABLE IF EXISTS ProductsWithMultiplePrimaryColumnsAndIdentity;

CREATE TABLE ProductsWithMultiplePrimaryColumnsAndIdentity (
	ProductId int NOT NULL AUTO_INCREMENT,
	ExternalId int NOT NULL,
	Name varchar(100) NULL,
	Cost int NULL,
	PRIMARY KEY (ProductId, ExternalId)
);
