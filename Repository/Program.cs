public interface IEntity
{
    int Id { get; }
}

public class Repository<T> where T : IEntity
{
    private Dictionary<int, T> _storage = new Dictionary<int, T>();

    public void Add(T item)
    {
        if (_storage.ContainsKey(item.Id))
        {
            throw new InvalidOperationException($"Elem with Id {item.Id} already exists.");
        }

        _storage[item.Id] = item;
    }

    public bool Remove(int id)
    {
        return _storage.Remove(id);
    }

    public T? GetById(int id)
    {
        _storage.TryGetValue(id, out T? value);

        return value;
    }

    public IReadOnlyList<T> GetAll()
    {
        return [.. _storage.Values];
    }

    public int Count => _storage.Count;

    public IReadOnlyList<T> Find(Predicate<T> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var result = new List<T>();
        foreach (var item in _storage.Values)
        {
            if (predicate(item))
            {
                result.Add(item);
            }
        }

        return result;
    }
}

public class Product : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString() => $"[Product] Id={Id}, Name={Name}, Price={Price:C}";
}

public class User : IEntity
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;

    public override string ToString() => $"[User] Id={Id}, Login={Login}";
}

class Program
{
    static void Main()
    {
        var productRepo = new Repository<Product>();
        productRepo.Add(new Product { Id = 1, Name = "Laptop", Price = 1500m });
        productRepo.Add(new Product { Id = 2, Name = "Mouse", Price = 25m });
        productRepo.Add(new Product { Id = 3, Name = "Keyboard", Price = 80m });

        Console.WriteLine($"Product number: {productRepo.Count}");

        Console.WriteLine("All products:");
        foreach (var p in productRepo.GetAll())
        {
            Console.WriteLine(p);
        }

        var prodById = productRepo.GetById(2);
        Console.WriteLine($"\nGet product by Id=2: {prodById}");

        Console.WriteLine("\nProducts with price higher than 100:");
        foreach (var p in productRepo.Find(p => p.Price > 100))
        {
            Console.WriteLine(p);
        }

        try
        {
            productRepo.Add(new Product { Id = 1, Name = "Dublicate", Price = 999m });
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"\nError during dublicate adding: {ex.Message}");
        }

        var userRepo = new Repository<User>();
        userRepo.Add(new User { Id = 101, Login = "Alice" });
        userRepo.Add(new User { Id = 102, Login = "Bob" });

        Console.WriteLine($"\nUser number: {userRepo.Count}");

        Console.WriteLine("All users:");
        foreach (var u in userRepo.GetAll())
        {
            Console.WriteLine(u);
        }

        var userById = userRepo.GetById(102);
        Console.WriteLine($"\nGet user by Id=102: {userById}");

        Console.WriteLine("\nUsers with login legth less than 5:");
        foreach (var u in userRepo.Find(u => u.Login.Length < 5))
        {
            Console.WriteLine(u);
        }

        try
        {
            userRepo.Add(new User { Id = 101, Login = "Dublicate"});
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"\nError during dublicate adding: {ex.Message}");
        }
    }
}
