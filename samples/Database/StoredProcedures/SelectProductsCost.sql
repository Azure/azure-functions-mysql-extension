DROP PROCEDURE SelectProductsCost;

DELIMITER //
Create Procedure SelectProductsCost(
cost INT
)
BEGIN
	SELECT * from Products where Products.cost = cost;
END//
DELIMITER ;

