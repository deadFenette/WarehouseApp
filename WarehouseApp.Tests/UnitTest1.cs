namespace WarehouseApp.Tests;

public class UnitTest1
{
    [Fact]
    public void Box_Constructor_ShouldCalculateExpiryDate()
    {
        // Arrange
        DateTime productionDate = new DateTime(2023, 1, 1);

        // Act
        Box box = new Box(1, 10, 10, 10, 5, null, productionDate);

        // Assert
        Assert.Equal(productionDate.AddDays(100), box.ExpiryDate);
    }

    [Fact]
    public void Pallet_AddBox_ShouldIncreaseWeightAndVolume()
    {
        // Arrange
        Pallet pallet = new Pallet(1, 100, 100, 100, 50);
        Box box = new Box(1, 10, 10, 10, 5, DateTime.Now);

        // Act
        pallet.AddBox(box);

        // Assert
        Assert.Equal(50 + 5 + 30, pallet.Weight); // вес паллеты + вес коробки + 30кг
        Assert.Equal(pallet.Width * pallet.Height * pallet.Depth + box.Volume, pallet.Volume);
    }

    [Fact]
    public void Pallet_ExpiryDate_ShouldReturnEarliestExpiryDate()
    {
        // Arrange
        Pallet pallet = new Pallet(1, 100, 100, 100, 50);
        Box box1 = new Box(1, 10, 10, 10, 5, new DateTime(2024, 1, 1));
        Box box2 = new Box(2, 10, 10, 10, 5, new DateTime(2023, 5, 1));

        // Act
        pallet.AddBox(box1);
        pallet.AddBox(box2);

        // Assert
        Assert.Equal(new DateTime(2023, 5, 1), pallet.ExpiryDate);
    }
}