namespace ASAM.MDF.Libary
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MdfVersionAttribute : Attribute
    {
        public MdfVersionAttribute(ushort version, object defaultValue)
        {
            Version = version;
            DefaultValue = defaultValue;
        }

        public ushort Version { get; private set; }
        public object DefaultValue { get; private set; }
    }
}
