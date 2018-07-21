using System.Runtime.Serialization;

namespace MvvmScarletToolkit.IconManager
{
    [DataContract]
    public class CategoryResponse
    {
        [DataMember]
        public string[] Categoryies { get; set; }
    }
}
