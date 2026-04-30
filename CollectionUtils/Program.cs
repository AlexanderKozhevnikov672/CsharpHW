public static class CollectionUtils
{
    public static List<T> Distinct<T>(List<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var seen = new HashSet<T>();
        var result = new List<T>();
        foreach (var item in source)
        {
            if (seen.Add(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
        List<TValue> source,
        Func<TValue, TKey> keySelector
    ) where TKey : notnull
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var dict = new Dictionary<TKey, List<TValue>>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                dict[key] = list;
            }

            list.Add(item);
        }

        return dict;
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second,
        Func<TValue, TValue, TValue> conflictResolver
    ) where TKey : notnull
    {
        if (first == null) 
        {
            throw new ArgumentNullException(nameof(first));
        }
        if (second == null)
        {
            throw new ArgumentNullException(nameof(second));
        }
        if (conflictResolver == null)
        {
            throw new ArgumentNullException(nameof(conflictResolver));
        }

        var result = new Dictionary<TKey, TValue>(first);
        foreach (var item in second)
        {
            if (result.TryGetValue(item.Key, out var existingValue))
            {
                result[item.Key] = conflictResolver(existingValue, item.Value);
            }
            else
            {
                result[item.Key] = item.Value;
            }
        }

        return result;
    }

    public static T MaxBy<T, TKey>(
        List<T> source,
        Func<T, TKey> selector
    ) where TKey : IComparable<TKey>
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        if (source.Count == 0)
        {
            throw new InvalidOperationException("Collection must not be empty.");
        }

        T maxItem = source[0];
        TKey maxKey = selector(maxItem);
        for (int i = 1; i < source.Count; i++)
        {
            T currentItem = source[i];
            TKey currentKey = selector(currentItem);
            if (currentKey.CompareTo(maxKey) > 0)
            {
                maxItem = currentItem;
                maxKey = currentKey;
            }
        }

        return maxItem;
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString() => $"[Product] Id={Id}, Name={Name}, Price={Price:C}";
}

class Program
{
    static void Main()
    {
        List<int> numbers = new List<int> { 1, 2, 2, 3, 1, 4, 5, 4 };
        Console.WriteLine("Numbers: " + string.Join(", ", numbers));
        var distinctNumbers = CollectionUtils.Distinct(numbers);
        Console.WriteLine("Distinct numbers: " + string.Join(", ", distinctNumbers));

        List<string> strings = new List<string> { "a", "b", "a", "c", "b", "d" };
        Console.WriteLine("\nStrings: " + string.Join(", ", strings));
        var distinctStrings = CollectionUtils.Distinct(strings);
        Console.WriteLine("Distinct strings: " + string.Join(", ", distinctStrings));

        List<string> words = new List<string> { "cat", "dog", "mouse", "elephant", "bird", "ant" };
        Console.WriteLine("\nWords: " + string.Join(", ", words));
        var groupedByLength = CollectionUtils.GroupBy(words, w => w.Length);
        Console.WriteLine("Words grouped by length:");
        foreach (var item in groupedByLength)
        {
            Console.WriteLine($"Length {item.Key}: {string.Join(", ", item.Value)}");
        }

        var dict1 = new Dictionary<string, int> { { "apple", 2 }, { "banana", 3 }, { "cherry", 1 } };
        Console.WriteLine("\nDict1:");
        foreach (var item in dict1)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
        var dict2 = new Dictionary<string, int> { { "banana", 2 }, { "melon", 5 }, { "apple", 1 } };
        Console.WriteLine("Dict2:");
        foreach (var item in dict2)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
        var merged = CollectionUtils.Merge(dict1, dict2, (v1, v2) => v1 + v2);
        Console.WriteLine("Merged with value sum resolver:");
        foreach (var item in merged)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 1500m },
            new Product { Id = 2, Name = "Mouse", Price = 25m },
            new Product { Id = 3, Name = "Keyboard", Price = 80m }
        };
        Console.WriteLine("\nProducts:");
        foreach (var p in products)
        {
            Console.WriteLine(p);
        }
        var mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
        Console.WriteLine($"Most expensive product: {mostExpensive}");
    }
}
