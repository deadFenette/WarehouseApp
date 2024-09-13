// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public abstract class BaseItem
{
    public int ID { get; }
    public double Width { get; }
    public double Height { get; }
    public double Depth { get; }
    public virtual double Weight { get; }

    public virtual double Volume
    {
        get { return Width * Height * Depth; }
    }

    protected BaseItem(int id, double width, double height, double depth, double weight)
    {
        ID = id;
        Width = width > 0 ? width : throw new ArgumentException("Ширина должна быть больше нуля.");
        Height = height > 0 ? height : throw new ArgumentException("Высота должна быть больше нуля.");
        Depth = depth > 0 ? depth : throw new ArgumentException("Глубина должна быть больше нуля.");
        Weight = weight > 0 ? weight : throw new ArgumentException("Вес должен быть больше нуля.");
    }
}

public class Box : BaseItem
{
    public DateTime? ExpiryDate { get; }
    public DateTime? ProductionDate { get; }

    public Box(int id, double width, double height, double depth, double weight, DateTime? expiryDate = null, DateTime? productionDate = null)
        : base(id, width, height, depth, weight)
    {
        ExpiryDate = expiryDate ?? (productionDate.HasValue ? productionDate.Value.AddDays(100) : null);
        ProductionDate = productionDate;

        if (ProductionDate.HasValue && ExpiryDate.HasValue && ProductionDate > ExpiryDate)
        {
            throw new ArgumentException("Дата производства не может быть позже даты истечения срока годности.");
        }
    }
}

public class Pallet : BaseItem
{
    public List<Box> Boxes { get; } = new List<Box>();

    public override double Weight => base.Weight + Boxes.Sum(box => box.Weight) + 30; // Вес паллеты: вес всех коробок + 30кг

    public override double Volume => base.Volume + Boxes.Sum(box => box.Volume); // Объем паллеты: объем самой паллеты + объем коробок

    public DateTime? ExpiryDate => Boxes.Count == 0 ? null : Boxes.Min(box => box.ExpiryDate); // Срок годности паллеты - минимальный срок коробок

    public Pallet(int id, double width, double height, double depth, double weight) : base(id, width, height, depth, weight)
    {
    }

    public void AddBox(Box box)
    {
        if (box == null) throw new ArgumentNullException(nameof(box));
        if (box.Width <= Width && box.Depth <= Depth)
        {
            Boxes.Add(box);
        }
        else
        {
            throw new ArgumentException("Коробка не помещается на паллету по размерам.");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var pallets = new List<Pallet>();

        Console.WriteLine("Введите количество паллет для создания:");
        int palletCount = ReadIntFromConsole();

        for (int i = 0; i < palletCount; i++)
        {
            pallets.Add(CreatePalletFromInput(i + 1));
        }

        Console.WriteLine("\nВсе паллеты созданы:");
        foreach (var pallet in pallets)
        {
            Console.WriteLine($"Паллет ID: {pallet.ID}, Объем: {pallet.Volume}, Вес: {pallet.Weight}, Срок годности: {pallet.ExpiryDate?.ToString("yyyy-MM-dd") ?? "Нет срока"}");
        }

        DisplaySortedPallets(pallets);
    }

    // Метод для вывода отсортированных данных
    static void DisplaySortedPallets(List<Pallet> pallets)
    {
        Console.WriteLine("\nПаллеты, отсортированные по сроку годности и весу:");

        var groupedAndSortedPallets = pallets
            .Where(p => p.ExpiryDate.HasValue) // Только паллеты с указанным сроком годности
            .GroupBy(p => p.ExpiryDate)
            .OrderBy(g => g.Key) // Сортировка по сроку годности (по возрастанию)
            .SelectMany(g => g.OrderBy(p => p.Weight)); // Сортировка паллет внутри группы по весу

        foreach (var pallet in groupedAndSortedPallets)
        {
            Console.WriteLine($"Паллет ID: {pallet.ID}, Срок годности: {pallet.ExpiryDate?.ToString("yyyy-MM-dd")}, Вес: {pallet.Weight}");
        }

        // 3 паллеты с коробками с наибольшим сроком годности, отсортированные по объему
        var topPalletsByExpiryDate = pallets
            .Where(p => p.ExpiryDate.HasValue)
            .OrderByDescending(p => p.ExpiryDate)
            .Take(3)
            .OrderBy(p => p.Volume);

        Console.WriteLine("\nТоп 3 паллеты с коробками с наибольшим сроком годности, отсортированные по объему:");
        foreach (var pallet in topPalletsByExpiryDate)
        {
            Console.WriteLine($"Паллет ID: {pallet.ID}, Срок годности: {pallet.ExpiryDate?.ToString("yyyy-MM-dd")}, Объем: {pallet.Volume}");
        }
    }

    // Метод создания паллеты
    static Pallet CreatePalletFromInput(int id)
    {
        Console.WriteLine($"\nСоздание паллеты с ID {id}");

        double width = GetValidDoubleInput("Введите ширину паллеты:");
        double height = GetValidDoubleInput("Введите высоту паллеты:");
        double depth = GetValidDoubleInput("Введите глубину паллеты:");
        double weight = GetValidDoubleInput("Введите вес паллеты (без коробок):");

        var pallet = new Pallet(id, width, height, depth, weight);

        Console.WriteLine("Введите количество коробок на паллете:");
        int boxCount = ReadIntFromConsole();

        for (int i = 0; i < boxCount; i++)
        {
            pallet.AddBox(CreateBoxFromInput(i + 1));
        }

        return pallet;
    }

    // Метод создания коробки
    static Box CreateBoxFromInput(int id)
    {
        Console.WriteLine($"\nСоздание коробки с ID {id}");

        double width = GetValidDoubleInput("Введите ширину коробки:");
        double height = GetValidDoubleInput("Введите высоту коробки:");
        double depth = GetValidDoubleInput("Введите глубину коробки:");
        double weight = GetValidDoubleInput("Введите вес коробки:");

        Console.WriteLine("Введите срок годности (гггг-мм-дд) или оставьте пустым:");
        string? expiryDateInput = Console.ReadLine();
        DateTime? expiryDate = string.IsNullOrEmpty(expiryDateInput) ? (DateTime?)null : ParseDate(expiryDateInput);

        Console.WriteLine("Введите дату производства (гггг-мм-дд) или оставьте пустым:");
        string? productionDateInput = Console.ReadLine();
        DateTime? productionDate = string.IsNullOrEmpty(productionDateInput) ? (DateTime?)null : ParseDate(productionDateInput);

        return new Box(id, width, height, depth, weight, expiryDate, productionDate);
    }

    // Универсальный метод ввода вещественных чисел с проверкой
    static double GetValidDoubleInput(string message)
    {
        double result;
        while (true)
        {
            Console.WriteLine(message);
            if (double.TryParse(Console.ReadLine(), out result) && result > 0)
            {
                return result;
            }
            Console.WriteLine("Введите корректное число больше 0.");
        }
    }

    // Метод ввода целых чисел с проверкой
    static int ReadIntFromConsole()
    {
        int result;
        while (!int.TryParse(Console.ReadLine(), out result) || result <= 0)
        {
            Console.WriteLine("Некорректный ввод. Попробуйте снова:");
        }
        return result;
    }

    // Метод парсинга даты
    static DateTime? ParseDate(string input)
    {
        if (DateTime.TryParse(input, out DateTime result))
        {
            return result;
        }
        Console.WriteLine("Неверный формат даты. Оставляем значение пустым.");
        return null;
    }
}
