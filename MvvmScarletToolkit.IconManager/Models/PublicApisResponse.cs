using System.Runtime.Serialization;

namespace MvvmScarletToolkit.IconManager
{
    [DataContract]
    public class PublicApisResponse
    {
        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public Entry[] Entries { get; set; }
    }
}
