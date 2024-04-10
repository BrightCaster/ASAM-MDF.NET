namespace ASAM.MDF.Libary
{
    public interface INext<T> where T : INext<T>
    {
        T Next { get; }
    }
    public interface IPrevious<T> where T : IPrevious<T>
    {
        T Previous { get; set; }
    }
    public interface IParent<T>
    {
        T Parent { get; set; }
    }
}