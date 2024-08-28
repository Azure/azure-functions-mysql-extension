DROP PROCEDURE IF EXISTS SelectProductsCost;

Create Procedure SelectProductsCost(cost INT)
BEGIN
	SELECT * from Products where Products.cost = cost;
END

