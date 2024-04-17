using ASAM.MDF.Libary;
using System.Runtime.InteropServices;
using System.Threading.Channels;

var filename = ShowDialog();
try
{
    var bytes = File.ReadAllBytes(filename);
    //var lists = new List<byte>(bytes);
    var samebytes = new byte[0];
    var mdf = new Mdf(bytes);
    
    var list = new List<ChannelBlock>();


    for (int i = 0; i < mdf.DataGroups.Count; i++)
    {
        var group = mdf.DataGroups[i];
        for (int j = 0; j < group.ChannelGroups.Count; j++)
        {
            var channelGroup = group.ChannelGroups[j];
            for (int k = 0; k < channelGroup.Channels.Count; k++)
            {
                var channelBlock = channelGroup.Channels[k];
                list.Add(channelBlock);
            }
        }
    }
    list.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
    //list.RemoveAt(1);
    var indexs = new int[] { 2, 8, 12 };
    var listRemoved = list.Where((x, i) => indexs.Contains(i)).ToArray();
    var anotherChannels = list.Where(x => !listRemoved.Contains(x)).ToArray();


    samebytes = mdf.RemoveChannel(anotherChannels);
    //var newmdf = new Mdf(samebytes);
    var listValues = new List<double>();

    //for (int i = 0; i < mdf.DataGroups.Count; i++)
    //{
    //    var group = mdf.DataGroups[i];
    //    var records = group.Records;
    //    for (int j = 0; j < records.Length; j++)
    //    {
    //        var value = records[j].GetValue(list[1]);
    //        if (value == null)
    //            continue;
    //        listValues.Add(Convert.ToDouble(value));
    //    }
    //}

    var ex = Path.GetExtension(filename);
    var file = Path.GetFileNameWithoutExtension(filename) + "Test";
    var path = Path.GetDirectoryName(filename) + "\\" + file + ex;
    File.WriteAllBytes(path, samebytes);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine();
    throw;
}

static string ShowDialog()
{
    var ofn = new OpenFileName();
    ofn.lStructSize = Marshal.SizeOf(ofn);
    ofn.lpstrFile = new string(new char[256]);
    ofn.nMaxFile = ofn.lpstrFile.Length;
    ofn.lpstrFileTitle = new string(new char[64]);
    ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
    ofn.lpstrTitle = "Open File Dialog...";
    if (GetOpenFileName(ref ofn))
        return ofn.lpstrFile;
    return string.Empty;
}

[DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
static extern bool GetOpenFileName(ref OpenFileName ofn);

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OpenFileName
{
    public int lStructSize;
    public IntPtr hwndOwner;
    public IntPtr hInstance;
    public string lpstrFilter;
    public string lpstrCustomFilter;
    public int nMaxCustFilter;
    public int nFilterIndex;
    public string lpstrFile;
    public int nMaxFile;
    public string lpstrFileTitle;
    public int nMaxFileTitle;
    public string lpstrInitialDir;
    public string lpstrTitle;
    public int Flags;
    public short nFileOffset;
    public short nFileExtension;
    public string lpstrDefExt;
    public IntPtr lCustData;
    public IntPtr lpfnHook;
    public string lpTemplateName;
    public IntPtr pvReserved;
    public int dwReserved;
    public int flagsEx;
}