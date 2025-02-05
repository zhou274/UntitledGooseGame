public interface ISaveController
{
    public string UniqueSaveName { get; }

    public void Load(ISaveObject saveObject);
    public ISaveObject Save();
}
