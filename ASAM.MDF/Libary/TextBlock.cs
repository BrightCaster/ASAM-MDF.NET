namespace ASAM.MDF.Libary
{
    using System;
    using System.Text;

    public class TextBlock : Block
    {
        private string m_Text;

        public TextBlock(Mdf mdf) : base(mdf)
        {
            var data = new byte[Size - 4];
            var read = Mdf.Data.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            m_Text = Mdf.IDBlock.Encoding.GetString(data, 0, data.Length);
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                SetStringValue(ref m_Text, value, ushort.MaxValue - 4);
            }
        }

        public static implicit operator string(TextBlock textBlock)
        {
            if (textBlock == null)
                return null;

            return textBlock.Text;
        }
        public static implicit operator TextBlock(string text)
        {
            if (text == null)
                return null;

            return text;
        }
    }
}
