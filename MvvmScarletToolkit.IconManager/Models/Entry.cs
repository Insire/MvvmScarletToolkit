using System.Runtime.Serialization;

namespace MvvmScarletToolkit.IconManager
{
    [DataContract]
    public class Entry
    {
        [DataMember]
        public string API { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Auth { get; set; }
        [DataMember]
        public bool HTTPS { get; set; }
        [DataMember]
        public string Cors { get; set; }
        [DataMember]
        public string Link { get; set; }
        [DataMember]
        public string Category { get; set; }
    }
}
