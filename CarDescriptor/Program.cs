public interface ICar
{
    string GetDescription();
}

public abstract class ACar : ICar
{
    public abstract string GetBrandName();
    public abstract string GetEngineDescription();
    public abstract string GetTransmissionDescription();
    public abstract string GetInteriorDescription();

    public virtual string GetDescription()
    {
        return @$"{GetBrandName()} car description: 
{GetEngineDescription()}
{GetTransmissionDescription()}
{GetInteriorDescription()}";
    }
}

public interface IElectricEngine
{
    string GetEnginePower();
    string GetEngineEfficiency();
    string GetEngineType();

    string GetElectricEngineDescription()
    {
        return @$"Electric engine description:
- Power: {GetEnginePower()}
- Efficiency: {GetEngineEfficiency()}
- Type: {GetEngineType()}";
    }
}

public interface ICombustionEngine
{
    string GetEnginePower();
    string GetEngineConsumption();
    string GetEngineEcoClass();

    string GetCombustionEngineDescription()
    {
        return @$"Combustion engine description:
- Power: {GetEnginePower()}
- Consumption: {GetEngineConsumption()}
- Eco class: {GetEngineEcoClass()}";
    }
}

public interface IAutomaticTransmission
{
    string GetTransmissionType();
    int GetTransmissionGearsCount();
    string GetTransmissionConverterType();

    string GetAutomaticTransmissionDescription()
    {
        return @$"Automatic transmission description:
- Type: {GetTransmissionType()}
- Gears: {GetTransmissionGearsCount()}
- Converter type: {GetTransmissionConverterType()}";
    }
}

public interface IManualTransmission
{
    string GetTransmissionType();
    int GetTransmissionGearsCount();
    string GetTransmissionClutchType();

    string GetManualTransmissionDescription()
    {
        return @$"Manual transmission description:
- Type: {GetTransmissionType()}
- Gears: {GetTransmissionGearsCount()}
- Clutch type: {GetTransmissionClutchType()}";
    }
}

public interface ICarInterior
{
    int GetSeatCount();
    string GetUpholsteryMaterial();
    string GetInfotainmentSystem();

    string GetCarInteriorDescription()
    {
        return @$"Interior description:
- Seats: {GetSeatCount()}
- Upholstery: {GetUpholsteryMaterial()}
- Infotainment: {GetInfotainmentSystem()}";
    }
}

public class TeslaCar : ACar, IElectricEngine, IAutomaticTransmission, ICarInterior
{
    public string GetEnginePower() => "450 hp";
    public string GetEngineEfficiency() => "90%";
    public string GetEngineType() => "synchronous electric motor";

    public string GetTransmissionType() => "single-speed reduction gear";
    public int GetTransmissionGearsCount() => 1;
    public string GetTransmissionConverterType() => "not applicable";

    public int GetSeatCount() => 5;
    public string GetUpholsteryMaterial() => "vegan leather";
    public string GetInfotainmentSystem() => "Android Auto with 17-inch touchscreen";

    public override string GetBrandName() => "Tesla";
    public override string GetEngineDescription() => (this as IElectricEngine).GetElectricEngineDescription();
    public override string GetTransmissionDescription() => (this as IAutomaticTransmission).GetAutomaticTransmissionDescription();
    public override string GetInteriorDescription() => (this as ICarInterior).GetCarInteriorDescription();
}

public class LadaCar : ACar, ICombustionEngine, IManualTransmission, ICarInterior
{
    public string GetEnginePower() => "87 hp";
    public string GetEngineConsumption() => "8.5 l/100km";
    public string GetEngineEcoClass() => "Euro 5";

    public string GetTransmissionType() => "manual";
    public int GetTransmissionGearsCount() => 5;
    public string GetTransmissionClutchType() => "dry, single-disc";

    public int GetSeatCount() => 5;
    public string GetUpholsteryMaterial() => "fabric";
    public string GetInfotainmentSystem() => "basic audio system";

    public override string GetBrandName() => "Lada";
    public override string GetEngineDescription() => (this as ICombustionEngine).GetCombustionEngineDescription();
    public override string GetTransmissionDescription() => (this as IManualTransmission).GetManualTransmissionDescription();
    public override string GetInteriorDescription() => (this as ICarInterior).GetCarInteriorDescription();
}

public class BmwCar : ACar, ICombustionEngine, IAutomaticTransmission, ICarInterior
{
    public string GetEnginePower() => "249 hp";
    public string GetEngineConsumption() => "7.2 l/100km";
    public string GetEngineEcoClass() => "Euro 6";

    public string GetTransmissionType() => "automatic Steptronic";
    public int GetTransmissionGearsCount() => 8;
    public string GetTransmissionConverterType() => "torque converter with lock-up";

    public int GetSeatCount() => 5;
    public string GetUpholsteryMaterial() => "leather";
    public string GetInfotainmentSystem() => "iDrive with 12.3-inch display";

    public override string GetBrandName() => "BMW";
    public override string GetEngineDescription() => (this as ICombustionEngine).GetCombustionEngineDescription();
    public override string GetTransmissionDescription() => (this as IAutomaticTransmission).GetAutomaticTransmissionDescription();
    public override string GetInteriorDescription() => (this as ICarInterior).GetCarInteriorDescription();
}

public class ToyotaCar : ACar, ICombustionEngine, IManualTransmission, ICarInterior
{
    public string GetEnginePower() => "150 hp";
    public string GetEngineConsumption() => "6.5 l/100km";
    public string GetEngineEcoClass() => "Euro 6";

    public string GetTransmissionType() => "manual";
    public int GetTransmissionGearsCount() => 6;
    public string GetTransmissionClutchType() => "dry, dual-disc";

    public int GetSeatCount() => 5;
    public string GetUpholsteryMaterial() => "premium fabric";
    public string GetInfotainmentSystem() => "Toyota Touch with 8-inch display";

    public override string GetBrandName() => "Toyota";
    public override string GetEngineDescription() => (this as ICombustionEngine).GetCombustionEngineDescription();
    public override string GetTransmissionDescription() => (this as IManualTransmission).GetManualTransmissionDescription();
    public override string GetInteriorDescription() => (this as ICarInterior).GetCarInteriorDescription();
}

public class UnknownCar : ACar
{
    public override string GetBrandName() => "Unknown";
    public override string GetEngineDescription() => "No engine information available";
    public override string GetTransmissionDescription() => "No transmission information available";
    public override string GetInteriorDescription() => "No interior information available";
    public override string GetDescription() => "Unknown car brand";
}

public enum CarType
{
    Tesla,
    Lada,
    Bmw,
    Toyota,
    Unknown,
}

public static class CarFactory
{
    public static ICar CreateCar(CarType type)
    {
        return type switch
        {
            CarType.Tesla => new TeslaCar(),
            CarType.Lada => new LadaCar(),
            CarType.Bmw => new BmwCar(),
            CarType.Toyota => new ToyotaCar(),
            _ => new UnknownCar(),
        };
    }
}

class Program
{
    const string DoneInput = "done";

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Program: Enter car brand or 'done' to stop:");

            Console.Write("User: ");
            string input = Console.ReadLine() ?? string.Empty;
            input = input.Trim().ToLower();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Program: Empty input, please enter car brand.\n");
                continue;
            }

            if (input == DoneInput)
            {
                break;
            }

            CarType type = GetCarType(input);
            ICar car = CarFactory.CreateCar(type);
            Console.WriteLine(car.GetDescription());

            Console.WriteLine();
        }
    }

    static CarType GetCarType(string type) => type switch
    {
        "tesla" => CarType.Tesla,
        "lada" => CarType.Lada,
        "bmw" => CarType.Bmw,
        "toyota" => CarType.Toyota,
        _ => CarType.Unknown,
    };
}
