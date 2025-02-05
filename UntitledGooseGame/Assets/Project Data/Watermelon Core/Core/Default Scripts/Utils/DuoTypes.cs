using UnityEngine;

[System.Serializable]
public class DuoInt
{
    public int firstValue;
    public int secondValue;

    public DuoInt(int firstValue, int secondValue)
    {
        this.firstValue = firstValue;
        this.secondValue = secondValue;
    }

    public DuoInt(int value)
    {
        this.firstValue = value;
        this.secondValue = value;
    }

    public int Random()
    {
        return UnityEngine.Random.Range(firstValue, secondValue + 1); // Because second parameter is exclusive. Withot + 1 method Random.Range(1,2) will always return 1
    }

    public static implicit operator Vector2Int(DuoInt value) => new Vector2Int(value.firstValue, value.secondValue);
    public static explicit operator DuoInt(Vector2Int vec) => new DuoInt(vec.x, vec.y);
    public static DuoInt operator *(DuoInt a, DuoInt b) => new DuoInt(a.firstValue * b.firstValue, a.secondValue * b.secondValue);
    public static DuoInt operator /(DuoInt a, DuoInt b)
    {
        if ((b.firstValue == 0) || (b.secondValue == 0))
        {
            throw new System.DivideByZeroException();
        }

        return new DuoInt(a.firstValue / b.firstValue, a.secondValue / b.secondValue);
    }

    public override string ToString()
    {
        return "(" + firstValue + ", " + secondValue + ")";
    }
}

[System.Serializable]
public class DuoFloat
{
    public float firstValue;
    public float secondValue;

    public DuoFloat(float firstValue, float secondValue)
    {
        this.firstValue = firstValue;
        this.secondValue = secondValue;
    }
    public DuoFloat(float value)
    {
        this.firstValue = value;
        this.secondValue = value;
    }

    public float Random()
    {
        return UnityEngine.Random.Range(firstValue, secondValue);
    }

    public static implicit operator Vector2 (DuoFloat value) => new Vector2(value.firstValue, value.secondValue);
    public static explicit operator DuoFloat(Vector2 vec) => new DuoFloat(vec.x, vec.y);
    public static DuoFloat operator *(DuoFloat a, DuoFloat b) => new DuoFloat(a.firstValue * b.firstValue, a.secondValue * b.secondValue);
    public static DuoFloat operator /(DuoFloat a, DuoFloat b)
    {
        if((b.firstValue == 0) || (b.secondValue == 0))
        {
            throw new System.DivideByZeroException();
        }

        return new DuoFloat(a.firstValue / b.firstValue, a.secondValue / b.secondValue);
    }

    public override string ToString()
    {
        return "(" + FormatValue(firstValue) + ", " + FormatValue(secondValue) + ")";
    }

    private string FormatValue(float value)
    {
        return value.ToString("0.0").Replace(',', '.');
    }
}

[System.Serializable]
public class DuoDouble
{
    public double firstValue;
    public double secondValue;
    private static System.Random random;

    public DuoDouble(double firstValue, double secondValue)
    {
        this.firstValue = firstValue;
        this.secondValue = secondValue;
    }

    public DuoDouble(double value)
    {
        this.firstValue = value;
        this.secondValue = value;
    }

    public double Random()
    {
        if(random == null)
        {
            random = new System.Random();
        }

        return random.NextDouble() * (this.secondValue - this.firstValue) + this.firstValue;
    }

    public static DuoDouble operator *(DuoDouble a, DuoDouble b) => new DuoDouble(a.firstValue * b.firstValue, a.secondValue * b.secondValue);
    public static DuoDouble operator /(DuoDouble a, DuoDouble b)
    {
        if ((b.firstValue == 0) || (b.secondValue == 0))
        {
            throw new System.DivideByZeroException();
        }

        return new DuoDouble(a.firstValue / b.firstValue, a.secondValue / b.secondValue);
    }

    public override string ToString()
    {
        return "(" + FormatValue(firstValue) + ", " + FormatValue(secondValue) + ")";
    }

    private string FormatValue(double value)
    {
        return value.ToString("0.0").Replace(',', '.');
    }
}

[System.Serializable]
public class DuoVector3
{
    public Vector3 firstValue;
    public Vector3 secondValue;

    public DuoVector3(Vector3 firstValue, Vector3 secondValue)
    {
        this.firstValue = firstValue;
        this.secondValue = secondValue;
    }

    public DuoVector3(Vector3 value)
    {
        this.firstValue = value;
        this.secondValue = value;
    }

    public static DuoVector3 operator *(DuoVector3 a, DuoVector3 b) => new DuoVector3(new Vector3(a.firstValue.x * b.firstValue.x, a.firstValue.y * b.firstValue.y, a.firstValue.z * b.firstValue.z), new Vector3(a.secondValue.x * b.secondValue.x, a.secondValue.y * b.secondValue.y, a.secondValue.z * b.secondValue.z));
    public static DuoVector3 operator /(DuoVector3 a, DuoVector3 b)
    {
        if ((b.firstValue.x == 0) || (b.firstValue.y == 0) || (b.firstValue.z == 0) || (b.secondValue.x == 0) || (b.secondValue.y == 0) || (b.secondValue.z == 0))
        {
            throw new System.DivideByZeroException();
        }

        return new DuoVector3(new Vector3(a.firstValue.x / b.firstValue.x, a.firstValue.y / b.firstValue.y, a.firstValue.z / b.firstValue.z), new Vector3(a.secondValue.x / b.secondValue.x, a.secondValue.y / b.secondValue.y, a.secondValue.z / b.secondValue.z));
    }

    public Vector3 Random()
    {
        return new Vector3(UnityEngine.Random.Range(firstValue.x, secondValue.x), UnityEngine.Random.Range(firstValue.y, secondValue.y), UnityEngine.Random.Range(firstValue.z, secondValue.z));
    }

    public override string ToString()
    {
        return "[" + FormatValue(firstValue) + ", " + FormatValue(secondValue) + "]";
    }

    private string FormatValue(float value)
    {
        return value.ToString("0.0").Replace(',', '.');
    }

    private string FormatValue(Vector3 value)
    {
        return "(" + FormatValue(value.x) + ", " + FormatValue(value.y) + ", " + FormatValue(value.z) + ")";
    }
}

